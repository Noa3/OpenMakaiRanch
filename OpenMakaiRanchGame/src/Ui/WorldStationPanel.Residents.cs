using System.Linq;
using Godot;
using OpenMakaiRanch.Gameplay;
using OpenMakaiRanch.World;
using static OpenMakaiRanch.Locale.LocaleCatalog;

namespace OpenMakaiRanch.Ui;

public partial class WorldStationPanel
{
    private string _residentPage = "overview";
    private string _residentFeedback = "";

    private void RenderResident()
    {
        var character = _game.Roster.Find(_id);
        if (character is null) { Close(); return; }
        var name = _game.Roster.DefinitionFor(character).DisplayName;
        _title.Text = name;
        if (_id == "anon")
        {
            _content.AddChild(Text(T("world.resident.self", "This is your ranch-owner record, not another resident. Plan your evening at the ranch house.")));
            Destination(new WorldDestination("ranch", "ranch_house", "Ranch house"));
            return;
        }
        _content.AddChild(Text(T("world.resident.stats", "Energy {0} • Fatigue {1} • Morale {2} • Bond {3}", character.Energy, character.Fatigue, character.Morale, character.Bond)));
        _content.AddChild(Text(T("world.resident.budget", "Your daily stamina: {0}/{1}. Looking around and ordinary conversation are free.",
            _game.State.Player.Stamina, _game.State.Player.MaxStamina + _game.State.Player.DailyStaminaBonus)));
        if (_residentFeedback.Length > 0)
        {
            var feedback = Text(_residentFeedback); feedback.Name = "ResidentFeedback";
            _content.AddChild(feedback);
        }
        if (_residentPage != "overview")
            Action("ResidentSectionBack", T("world.resident.sections.back", "Back to the conversation"), () => ShowResidentPage("overview"));
        switch (_residentPage)
        {
            case "care":
                _content.AddChild(Text(T("world.resident.care.help", "Care is optional. Each of these activities can help this resident once a day. Unavailable choices explain why and cost nothing.")));
                AddResidentAction("ResidentEncourage", ResidentAction.Encourage, T("world.resident.encourage", "Listen and offer encouragement"));
                AddResidentAction("ResidentFeed", ResidentAction.Meal, T("world.resident.meal", "Offer one meal box"));
                AddResidentAction("ResidentRecovery", ResidentAction.Recovery, T("world.resident.recovery", "Take a recovery break"));
                Action("ResidentGifts", T("world.resident.gifts", "Choose a gift"), () => ShowResidentPage("gifts"));
                break;
            case "gifts":
                _content.AddChild(Text(T("world.resident.gifts.help", "One ordinary gift per resident per day. The chosen item is consumed only after a successful gift; special quest and adoption items are not gifts.")));
                foreach (var id in ResidentInteractionService.GiftIds.Where(id => _game.Data.Items.ContainsKey(id)))
                {
                    var owned = _game.State.Inventory.Items.TryGetValue(id, out var count) ? count : 0;
                    var label = T("world.resident.gift.stock", "{0} — in your bag: {1}", T("world.resident.gift." + id, _game.Data.Items[id].DisplayName), owned);
                    AddResidentAction("ResidentGift_" + id, ResidentAction.Gift, label, id);
                }
                break;
            case "practice":
                _content.AddChild(Text(T("world.resident.practice.skills", "Ranch {0}/10 • Craft {1}/10 • Combat {2}/10 • Magic aptitude {3}", character.RanchSkill, character.CraftSkill, character.CombatSkill, character.MagicPower)));
                _content.AddChild(Text(T("world.resident.practice.help", "One focused practice per resident each day, at most two across the ranch. Each uses 10 resident energy and adds fatigue. Mentoring is separate. Neither changes the work schedule or advances time.")));
                _content.AddChild(Text(T("world.resident.practice.remaining", "Practice sessions left today: {0}/2.", System.Math.Max(0, 2 - _game.State.Calendar.TrainedToday))));
                AddResidentAction("ResidentPractice_ranch", ResidentAction.RanchPractice, T("world.resident.practice.ranch", "Practice ranch work"));
                AddResidentAction("ResidentPractice_craft", ResidentAction.CraftPractice, T("world.resident.practice.craft", "Practice crafting"));
                AddResidentAction("ResidentPractice_combat", ResidentAction.CombatPractice, T("world.resident.practice.combat", "Practice combat basics"));
                AddResidentAction("ResidentPractice_magic", ResidentAction.MagicPractice, T("world.resident.practice.magic", "Study basic magic"));
                AddResidentAction("ResidentMentor", ResidentAction.Mentor, T("world.resident.mentor", "Share practical advice"));
                break;
            case "company":
                _content.AddChild(Text(T("world.resident.company.help", "Companionship is optional and does not replace work or ordinary friendship. Only available voluntary activities are offered here.")));
                if (_game.Dating.IsEligiblePartner(character)) RenderVoluntaryCompanionship(_id);
                else _content.AddChild(Text(T("world.resident.company.unavailable", "No outing is available with this resident. Talking, care and suitable practical lessons remain separate options.")));
                break;
            case "work":
                var jobId = _game.Schedule.GetAssignment(_id);
                var jobName = _game.Data.Jobs.TryGetValue(jobId, out var job) ? JobName(jobId, job.DisplayName) : jobId;
                _content.AddChild(Text(T("world.resident.work.plan", "Today's work: {0}. Output is calculated once at day end. Assign a new job at its actual station.", jobName)));
                var residentId = _id; var generation = _generation;
                Action("ResidentRest", T("world.resident.day_off", "Give the day off"), () =>
                    _game.TryAssignJob(residentId, "rest", generation) ? T("world.resident.rest_success", "Rest assigned. No production was paid early.")
                        : T("world.resident.already_resting", "Already resting."), jobId == "rest");
                Action("ResidentWorkplaces", T("world.resident.work.places", "Find a workplace"), () => { OpenGuide(); return ""; });
                break;
            default:
                AddResidentAction("ResidentTalk", ResidentAction.Chat, T("world.resident.chat", "Ask how things are — free"));
                Action("ResidentCarePage", T("world.resident.sections.care", "Care and encouragement"), () => ShowResidentPage("care"));
                Action("ResidentPracticePage", T("world.resident.sections.practice", "Learn and practice"), () => ShowResidentPage("practice"));
                Action("ResidentCompanyPage", T("world.resident.sections.company", "Spend time together"), () => ShowResidentPage("company"));
                Action("ResidentWorkPage", T("world.resident.sections.work", "Today's work plan"), () => ShowResidentPage("work"));
                break;
        }
    }

    private string ShowResidentPage(string page)
    {
        _residentPage = page; _residentFeedback = ""; _scroll.ScrollVertical = 0; Render();
        return "";
    }

    private void AddResidentAction(string name, ResidentAction action, string label, string? itemId = null)
    {
        var id = _id; var generation = _generation; var day = _day; var phase = _phase;
        var offer = _game.Residents.Inspect(id, action, itemId);
        var text = offer.StaminaCost == 0 ? label : T("world.resident.action.cost", "{0} — {1} stamina", label, offer.StaminaCost);
        Action(name, text, () =>
        {
            var result = _world.TryResidentAction(id, action, generation, day, phase, itemId);
            _residentFeedback = result.Message;
            // The full localized result stays in the scrollable conversation, not only a two-line footer.
            return "";
        }, !offer.Available);
        if (!offer.Available) _content.AddChild(Text(offer.Reason));
    }
}
