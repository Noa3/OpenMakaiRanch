using System;
using System.Collections.Generic;
using System.Linq;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Core.Resources;
using OpenMakaiRanch.Data;
using static OpenMakaiRanch.Locale.LocaleCatalog;

namespace OpenMakaiRanch.Gameplay;

public enum ResidentAction { Chat, Encourage, Meal, Gift, Recovery, Mentor, RanchPractice, CraftPractice, CombatPractice, MagicPractice }
public readonly record struct ResidentOffer(bool Available, int StaminaCost, string Reason);
public readonly record struct ResidentActionResult(bool Success, bool Changed, string Message);

/// <summary>
/// Non-explicit social/care/practice commands. Existing Visit, Bond and Training services own
/// their effects. This boundary owns eligibility and bounded daily receipts, not a second clock.
/// </summary>
public sealed class ResidentInteractionService
{
    // Fixed per-resident slots; overwrite the last day, never append an unbounded action history.
    public const int EncourageDay = 1_230_300, MealDay = 1_230_301, GiftDay = 1_230_302;
    public const int RecoveryDay = 1_230_303, MentorDay = 1_230_304, PracticeDay = 1_230_305;
    public static IReadOnlyList<string> GiftIds { get; } = Array.AsReadOnly(new[]
        { "gift_band", "gift_charm", "gift_flowers", "gift_hat", "gift_journal", "gift_ribbon", "gift_scarf", "keepsake" });
    private readonly SaveState _state;
    private readonly DataRegistry _data;
    private readonly FlagService _flags;
    private readonly VisitService _visit;
    private readonly BondService _bond;
    private readonly TrainingService _training;
    private readonly PlayerStaminaService _stamina;

    public ResidentInteractionService(SaveState state, DataRegistry data, FlagService flags,
        VisitService visit, BondService bond, TrainingService training, PlayerStaminaService stamina)
    { _state = state; _data = data; _flags = flags; _visit = visit; _bond = bond; _training = training; _stamina = stamina; }

    public static string? PracticeFocus(ResidentAction action) => action switch
    {
        ResidentAction.RanchPractice => "ranch", ResidentAction.CraftPractice => "craft",
        ResidentAction.CombatPractice => "combat", ResidentAction.MagicPractice => "magic", _ => null
    };

    public int Cost(ResidentAction action) => action switch
    {
        ResidentAction.Chat => 0,
        ResidentAction.Meal => _stamina.Cost(PlayerActivityKind.VisitFeed),
        ResidentAction.Gift => _stamina.Cost(PlayerActivityKind.VisitGift),
        ResidentAction.Mentor or ResidentAction.RanchPractice or ResidentAction.CraftPractice
            or ResidentAction.CombatPractice or ResidentAction.MagicPractice => _stamina.Cost(PlayerActivityKind.Mentorship),
        _ => _stamina.Cost(PlayerActivityKind.VisitCare)
    };

    public ResidentOffer Inspect(string? id, ResidentAction action, string? itemId = null)
    {
        var cost = Cost(action);
        ResidentOffer No(string reason) => new(false, cost, reason);
        if (!Enum.IsDefined(action)) return No(T("world.resident.reason.unknown", "This interaction is not available."));
        var character = Find(id);
        if (character is null || id == "anon") return No(T("world.resident.reason.target", "Choose another resident who is still on the ranch."));
        if (!_state.Story.FirstDayCompleted || _state.Calendar.Day < 2)
            return No(T("world.resident.reason.intro", "Finish the guided introduction first."));
        if (character.Hp <= 0 || character.Mature.IsCollapsed || character.Mature.FallState == FallState.Collapse)
            return No(T("world.resident.reason.unwell", "This resident needs recovery. Give them time off before asking for an activity."));
        if (action == ResidentAction.Chat) return new(true, 0, "");
        var flag = ReceiptFlag(action);
        if (_flags.GetCharIntFlag(character.Id, flag) >= _state.Calendar.Day)
            return No(T("world.resident.reason.today", "Already done together today. You can still talk, or return tomorrow."));
        var focus = PracticeFocus(action);
        if (focus is not null || action == ResidentAction.Mentor)
        {
            if (_state.Calendar.Phase == DayPhase.Night)
                return No(T("world.resident.reason.daytime", "Lessons take place during the day or evening. Choose a night plan at the ranch house."));
            if (character.Energy < 10 || character.Fatigue >= 80 || character.Morale < 25)
                return No(T("world.resident.reason.tired", "Not ready for a lesson: at least 10 energy, less than 80 fatigue and 25 morale are needed."));
            if (focus is not null && _state.Calendar.TrainedToday >= 2)
                return No(T("world.resident.reason.lessons", "The ranch's two practice sessions for today are used. More are available tomorrow."));
            if (focus is not null && !_training.CanTrain(character.Id, focus))
                return No(T("world.resident.reason.cap", "This skill cannot improve further through the current lesson."));
        }
        // Preview reads the canonical budget without invoking its normalizing mutator.
        if (_state.Player.Stamina < cost)
            return No(T("world.resident.reason.stamina", "You need {0} daily stamina. Talking and walking remain free.", cost));
        if (action == ResidentAction.Meal && _state.Inventory.Items.GetValueOrDefault("meal_box") < 1)
            return No(T("world.resident.reason.meal", "Bring one meal box from the General Store."));
        if (action == ResidentAction.Gift && (itemId is null || !GiftIds.Contains(itemId)
            || !_data.Items.TryGetValue(itemId, out var item) || item.Category != ItemCategory.Keepsake))
            return No(T("world.resident.reason.gift", "Choose an ordinary gift from the gift list."));
        if (action == ResidentAction.Gift && _state.Inventory.Items.GetValueOrDefault(itemId!) < 1)
            return No(T("world.resident.reason.stock", "That gift is not in your inventory. Nothing has been spent."));
        if (action == ResidentAction.Recovery && character.Energy >= EnergyLimit(character) && character.Fatigue <= 0)
            return No(T("world.resident.reason.rested", "Already rested. No recovery is needed."));
        return new(true, cost, "");
    }

    public ResidentActionResult Execute(string? id, ResidentAction action, string? itemId = null)
    {
        var offer = Inspect(id, action, itemId);
        if (!offer.Available) return new(false, false, offer.Reason);
        var character = Find(id)!;
        if (action == ResidentAction.Chat) return new(true, false, Conversation(character));
        var focus = PracticeFocus(action);
        var beforeSkill = SkillValue(character, focus);
        var beforeEnergy = character.Energy;
        if (focus is not null)
        {
            if (!_training.Train(character.Id, focus))
                return new(false, false, T("world.resident.reason.changed", "The situation changed. Review this resident's options again."));
        }
        else
        {
            // Preconditions mirror the shared service contract. Never infer success by parsing a
            // localized display string, and never dispatch an opaque legacy training action ID.
            switch (action)
            {
                case ResidentAction.Encourage: _visit.CareTalk(character.Id); break;
                case ResidentAction.Meal: _visit.CareFeed(character.Id); break;
                case ResidentAction.Gift: _visit.CareGift(character.Id, itemId!); break;
                case ResidentAction.Recovery: _visit.CareRest(character.Id); break;
                case ResidentAction.Mentor: _bond.ConductMentorship(character.Id); break;
            }
        }
        _stamina.Spend(offer.StaminaCost);
        _flags.SetCharIntFlag(character.Id, ReceiptFlag(action), _state.Calendar.Day);
        var message = focus is not null
            ? T("world.resident.result.lesson", "Lesson completed: skill {0} → {1}, resident energy {2} → {3}. Their work assignment is unchanged.",
                beforeSkill, SkillValue(character, focus), beforeEnergy, character.Energy)
            : action switch
            {
                ResidentAction.Encourage => T("world.resident.result.encourage", "You take time to listen. Morale +7, bond +3 and favorability +150, up to their limits."),
                ResidentAction.Meal => T("world.resident.result.meal", "One meal box shared: fatigue -18, energy +10, morale +8 and bond +4, up to their limits."),
                ResidentAction.Gift => T("world.resident.result.gift", "One gift given: bond +8, morale +5 and favorability +400, up to their limits."),
                ResidentAction.Recovery => T("world.resident.result.recovery", "A recovery break: energy +25, fatigue -10 and morale +3, up to their limits. Daily work is not paid or reassigned."),
                _ => T("world.resident.result.mentor", "You share practical advice. Bond and morale improve; fatigue increases slightly. Today's work is unchanged.")
            };
        return new(true, true, message);
    }

    private CharacterState? Find(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        var found = _state.Roster.Characters.Where(character => character.Id == id).Take(2).ToArray();
        return found.Length == 1 ? found[0] : null;
    }

    private int EnergyLimit(CharacterState character) => Math.Max(1, character.MaxEnergyOverride
        ?? (_data.Characters.TryGetValue(character.DefinitionId, out var definition) ? definition.MaxEnergy : 150));

    private string Conversation(CharacterState character)
    {
        if (character.Fatigue >= 65 || character.Energy < 30)
            return T("world.resident.chat.tired", "“I could use a quiet break before taking on anything else.” A recovery break or a day off may help.");
        if (character.Morale < 40)
            return T("world.resident.chat.low", "“Today feels difficult. Thank you for checking in.” You can offer encouragement without changing their work plan.");
        var assignment = _state.Schedule.AssignedJobs.GetValueOrDefault(character.Id, "rest");
        if (assignment == "rest")
            return T("world.resident.chat.rest", "“I'm taking it easy today. We could chat or practice something later.” There is no need to fill every free moment.");
        var job = _data.Jobs.TryGetValue(assignment, out var definition) ? JobName(assignment, definition.DisplayName) : assignment;
        return T("world.resident.chat.work", "“My plan for today is {0}. How is your day going?” Work output is recorded at the end of the day.", job);
    }

    private static int SkillValue(CharacterState character, string? focus) => focus switch
    { "ranch" => character.RanchSkill, "craft" => character.CraftSkill, "combat" => character.CombatSkill, "magic" => character.MagicPower, _ => 0 };
    private static int ReceiptFlag(ResidentAction action) => action switch
    {
        ResidentAction.Encourage => EncourageDay, ResidentAction.Meal => MealDay, ResidentAction.Gift => GiftDay,
        ResidentAction.Recovery => RecoveryDay, ResidentAction.Mentor => MentorDay, _ => PracticeDay
    };
}
