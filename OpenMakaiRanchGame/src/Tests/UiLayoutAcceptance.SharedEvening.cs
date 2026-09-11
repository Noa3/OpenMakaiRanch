using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Core.Resources;
using OpenMakaiRanch.Gameplay;
using OpenMakaiRanch.World;

namespace OpenMakaiRanch.Tests;

public partial class UiLayoutAcceptance
{
    private async Task CheckSharedEveningJourney(WorldGameController world)
    {
        var game = GameRoot.Instance;
        world.CloseManagement(); world.Transition?.CompleteImmediately();
        if (world.ActiveAreaId != "ranch") world.TravelTo("ranch");
        world.Transition?.CompleteImmediately(); await Frames(8);
        if (game.HasSaveSlot(99)) throw new InvalidOperationException("Refusing an occupied overnight fixture slot.");
        var definitions = new[] { "evening_fixture_owner", "evening_fixture_companion" };
        if (definitions.Any(game.Data.Characters.ContainsKey)) throw new InvalidOperationException("Fixture definitions already exist.");
        var savedReduced = game.State.Settings.ReducedMotion;
        var participants = new[] { game.Roster.Find("anon")!, game.Roster.Characters.First(c => c.Id != "anon") };
        var originals = participants.Select(c => JsonSerializer.Serialize(c)).ToArray();
        try
        {
            // Disposable opt-in fixture only: create separate test identities, never approve a
            // shipped definition or use this path in ordinary gameplay. Other numeric tests remain.
            for (var i = 0; i < participants.Length; i++)
            {
                var copy = new CharacterDefinition { Id = definitions[i], DisplayName = "Evening test participant " + (i + 1),
                    MaxHp = 100, MaxEnergy = 100, RanchSkill = 10, CombatSkill = 10, CraftSkill = 10 };
                copy.AdultEligibility = AdultEligibility.ConfirmedAdult; copy.ApparentAge = 30;
                game.Data.Characters.Add(copy.Id, copy);
                participants[i].DefinitionId = copy.Id; participants[i].AdultEligibility = AdultEligibility.ConfirmedAdult;
                participants[i].ApparentAge = 30; participants[i].Hp = 100; participants[i].Energy = 100;
                participants[i].Fatigue = 0; participants[i].Morale = 70; participants[i].Bond = 60;
                participants[i].Mature.IsCollapsed = false; participants[i].Mature.FallState = FallState.Normal;
            }
            var partnerId = participants[1].Id;
            participants[1].Mature.Favorability = 6000; participants[1].Mature.Aversion = 0;
            game.State.Dating.ActivePartnerId = partnerId;
            game.State.Dating.ActiveApproach = DateInviteApproach.Respectful;
            game.State.Dating.Partners[partnerId] = new DatingPartnerState { PositiveMoments = 4, DatesStarted = 4 };
            game.State.Calendar.Phase = DayPhase.Night; game.State.Calendar.NightAction = "rest";
            game.State.Settings.ReducedMotion = false;
            game.NotifyStateChanged(); await Frames(6);
            var house = world.ResolveStation("ranch_house")!;
            world.ActivePlayer!.GlobalPosition = house.GlobalPosition + new Vector3(0, 0.3f, 0);
            world.ActivePlayer.Velocity = Vector3.Zero; await Frames(6);
            Check(world.OpenStation(house), "evening: the nearby ranch house offers its nightly planning surface");
            await Resize(new Vector2I(640, 480)); await Frames(8);
            CheckLocalizedPanelGeometry(world.StationPanel!, "shared evening planning 640x480");
            var day = game.State.Calendar.Day; var stamina = game.State.Player.Stamina; var gold = game.Economy.Gold;
            await ClickStationButton(Descendants(world.StationPanel!).OfType<Button>().Single(b => b.Name == "PlanSharedEvening"));
            Check(game.GetSharedEveningStatus().Planned && game.Roster.Find(partnerId)!.Bond == 60
                && game.Economy.Gold == gold && game.State.Player.Stamina == stamina,
                "evening: the real invitation click plans without immediate relationship, gold or stamina rewards");
            await ClickStationButton(Descendants(world.StationPanel!).OfType<Button>().Single(b => b.Name == "CancelSharedEvening"));
            Check(!game.GetSharedEveningStatus().Planned && game.Roster.Find(partnerId)!.Bond == 60,
                "evening: the real cancellation click has no relationship penalty");
            await ClickStationButton(Descendants(world.StationPanel!).OfType<Button>().Single(b => b.Name == "PlanSharedEvening"));
            await Capture("shared-evening-planning-640x480");
            Check(game.SaveSlot(99) && game.LoadSlot(99) && game.GetSharedEveningStatus().Planned,
                "evening: a pending invitation survives the actual current-schema save/load boundary");
            world.Transition?.CompleteImmediately(); await Frames(8);
            house = world.ResolveStation("ranch_house")!;
            world.ActivePlayer!.GlobalPosition = house.GlobalPosition + new Vector3(0, 0.3f, 0);
            world.ActivePlayer.Velocity = Vector3.Zero; await Frames(6);
            Check(world.OpenStation(house), "evening: loading the invitation returns to the same available physical house");
            await Frames(8);
            var sleep = Descendants(world.StationPanel!).OfType<Button>().Single(b => b.Name == "HouseSleep");
            sleep.GrabFocus(); await Frames(6);
            Check(!sleep.Disabled && VisibleTarget(sleep), "evening: the real Sleep button remains visible below the optional plan");
            await Click(sleep);
            Check(game.State.Calendar.Day == day + 1 && game.HadSharedEvening(day)
                && world.Transition!.IsTransitioning && !world.Ranch!.InputGate.WorldInputEnabled,
                "evening: the actual Sleep click settles once, records the night and runs a non-explicit input-locked reveal");
            await Capture("shared-evening-reveal-640x480");
            var bond = game.Roster.Find(partnerId)!.Bond;
            var reportCount = game.State.Reports.Count;
            game.NotifyStateChanged(); game.NotifyStateChanged();
            await Frames(8);
            Check(game.Roster.Find(partnerId)!.Bond == bond && game.State.Reports.Count == reportCount
                && !game.GetSharedEveningStatus().Planned, "evening: redraws cannot replay the completed invitation or settlement");
            world.Transition!.CompleteImmediately(); await Frames(6);
            world.CloseManagement(); await Frames(6);
            Check(world.Ranch!.InputGate.WorldInputEnabled, "evening: finishing the reveal and closing the report restores world input");
            Check(game.SaveSlot(99) && game.LoadSlot(99) && game.HadSharedEvening(day)
                && !game.GetSharedEveningStatus().Planned, "evening: the completed receipt loads without resurrecting the invitation");
        }
        finally
        {
            game.Save.Delete(99);
            // Restore original state records and remove test-only definitions before teardown.
            for (var i = 0; i < participants.Length; i++)
            {
                var restored = JsonSerializer.Deserialize<CharacterState>(originals[i])!;
                var index = game.State.Roster.Characters.FindIndex(c => c.Id == restored.Id);
                if (index >= 0) game.State.Roster.Characters[index] = restored;
                game.Data.Characters.Remove(definitions[i]);
            }
            game.State.Dating.ActivePartnerId = string.Empty;
            game.State.Settings.ReducedMotion = savedReduced;
            world.Transition?.CompleteImmediately(); world.CloseManagement();
        }
    }
}
