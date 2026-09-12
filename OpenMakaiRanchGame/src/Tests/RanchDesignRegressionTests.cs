using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Godot;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Core.Resources;
using OpenMakaiRanch.Data;
using OpenMakaiRanch.Gameplay;
using OpenMakaiRanch.World;

namespace OpenMakaiRanch.Tests;

public static class RanchDesignRegressionTests
{
    public static void Run(SmokeTestResult result)
    {
        void Check(bool pass, string message)
        { result.Passed &= pass; result.Lines.Add($"SMOKE {(pass ? "OK" : "FAIL")} ranch design: {message}"); }
        var plots = RanchBuildingPlots.All;
        var errors = RanchBuildingPlots.Validate(plots);
        Check(errors.Count == 0, "reserved roofs/footprints/entrances fit without overlap: " + string.Join("; ", errors));
        Check(plots.Count == 8 && plots.Select(plot => plot.Id).Distinct().Count() == 8, "eight unique authored ranch plots");
        var townPlots = OrganicWorldLayout.TownPlots;
        Check(townPlots.Count == 7 && townPlots.Select(p => p.Id).Distinct().Count() == 7,
            "organic town layout preserves all seven service identities");
        Check(plots.Count(p => Mathf.Abs(Mathf.AngleDifference(p.Yaw, Mathf.Atan2(-p.Center.X, -p.Center.Y))) > 0.2f) >= 3,
            "authored farm buildings no longer all face one radial hub");
        var layoutJson = Godot.FileAccess.GetFileAsString(OrganicWorldLayout.SourcePath);
        var rejected = false;
        try { OrganicWorldLayout.ParsePlots(layoutJson.Replace("dairy_barn", "pasture"), "ranch"); }
        catch (InvalidOperationException) { rejected = true; }
        Check(rejected, "duplicate spatial IDs cannot silently replace a service");
        rejected = false;
        try { OrganicWorldLayout.ParsePlots(layoutJson.Replace("\"version\": 1", "\"version\": 99"), "town"); }
        catch (InvalidOperationException) { rejected = true; }
        Check(rejected, "unsupported spatial layout version fails explicitly");
        var bad = plots.ToArray();
        bad[1] = bad[1] with { Center = bad[0].Center };
        Check(RanchBuildingPlots.Validate(bad).Count > 0, "overlapping authoring is rejected");
        bad = plots.ToArray(); bad[0] = bad[0] with { Center = new Vector2(0, 13) };
        Check(RanchBuildingPlots.Validate(bad).Any(error => error.Contains("gate")), "town gate approach is reserved");
        bad = plots.ToArray(); bad[0] = bad[0] with { Center = new Vector2(float.NaN, 0) };
        Check(RanchBuildingPlots.Validate(bad).Count > 0, "nonfinite authoring is rejected");
        foreach (var plot in plots)
        {
            var building = new WalkInBuilding { BuildingId = plot.Id, Footprint = plot.Footprint };
            building.Build(); var count = building.CollisionBodyCount; var footprint = building.Footprint;
            foreach (var level in new[] { 0, 1, 2, 3, 30, int.MaxValue, 1 }) building.SetFacilityLevel(level);
            Check(building.Footprint == footprint && building.CollisionBodyCount == count && building.VisualGrade == 1,
                plot.Id + ": visual upgrades/downgrades preserve footprint, collision count and entrance dimensions");
            building.Free();
        }

        Check(RanchDressingClearance.Allows(new Rect2(-0.2f, 1.8f, 0.4f, 0.4f)), "small decorative footprint fits an unreserved plaza location");
        Check(!RanchDressingClearance.Allows(RanchBuildingPlots.GateApproach)
            && !RanchDressingClearance.Allows(new Rect2(0, 0, float.NaN, 1)), "dressing rejects the gate and malformed mesh dimensions");
        Check(plots.All(plot => !RanchDressingClearance.Allows(plot.ReservedBounds)
            && !RanchDressingClearance.Allows(plot.EntranceBounds)), "trees cannot occupy any reserved roof or entrance");
        var decoration = new Node3D { Position = new Vector3(0, 0, 2) };
        decoration.AddChild(new MeshInstance3D { Mesh = new BoxMesh { Size = Vector3.One * 0.2f } });
        Check(RanchDressingClearance.AllowsMeshes(decoration), "mesh clearance composes local transforms safely before tree entry");
        decoration.Position = new Vector3(plots[0].Center.X, 0, plots[0].Center.Y);
        Check(!RanchDressingClearance.AllowsMeshes(decoration), "moving a mesh into a plot rejects it before instantiation into the world");
        decoration.Free();

        var data = DataRegistry.CreateSeeded();
        var state = new SaveState(); var flags = new FlagService();
        var economy = new EconomyService(state);
        var ranch = new RanchService(state, data, new EquipmentService(state, data), new TalentService(state, data));
        var facility = data.Facilities.Values.First(f => f.BuildCost > 0);
        state.Ranch.Facilities[facility.Id] = int.MaxValue; state.Economy.Gold = int.MaxValue;
        Check(!ranch.UpgradeFacility(facility.Id, economy) && state.Economy.Gold == int.MaxValue,
            "overflowing facility level cannot wrap or charge gold");
        Check(ranch.FacilityUpgradeCost(facility, int.MaxValue) == int.MaxValue, "extreme upgrade price saturates instead of going negative");
        state.Ranch.Facilities.Clear();
        var before = JsonSerializer.Serialize(state);
        var flagCount = flags.GlobalFlagCount;
        for (var repeat = 0; repeat < 20; repeat++) RanchProjectService.Evaluate(state, flags);
        Check(JsonSerializer.Serialize(state) == before && flags.GlobalFlagCount == flagCount,
            "project inspection does not assign jobs, allocate history, advance time or pay gold");
        var projects = RanchProjectService.Evaluate(state, flags);
        Check(projects.Count == 4 && projects.Select(p => p.Id).Distinct().Count() == 4, "four optional projects use stable unique IDs");
        Check(projects.Single(p => p.Id == "breathing_room").StationId == "office", "missing repair supplies directs to production, not an impossible purchase");
        flags.SetGlobalFlag(RanchLeisureService.RestoredFlag, true);
        flags.SetGlobalIntFlag(CommunityRequestService.CompletedDeliveriesFlag, 3);
        state.Ranch.Facilities["kitchen"] = 2; state.Milestones.CompletedIds.Add("first_patrol");
        Check(RanchProjectService.Evaluate(state, flags).All(project => project.Complete), "existing accomplishments resolve all projects without new reward paths");

        // Standalone definitions belong only to this fixture. No shipped character receives approval.
        data = new DataRegistry(); state = new SaveState(); flags = new FlagService();
        state.Calendar.Day = 3; state.Calendar.Phase = DayPhase.Night; state.Calendar.NightAction = "rest";
        state.Story.FirstDayCompleted = true;
        foreach (var id in new[] { "anon", "fixture_partner" })
        {
            data.Characters[id] = new CharacterDefinition { Id = id, ApparentAge = 30, AdultEligibility = AdultEligibility.ConfirmedAdult };
            state.Roster.Characters.Add(new CharacterState { Id = id, DefinitionId = id, ApparentAge = 30,
                AdultEligibility = AdultEligibility.ConfirmedAdult, Hp = 100, Energy = 100, Bond = 60, Morale = 70 });
        }
        var partner = state.Roster.Characters[1]; partner.Mature.Favorability = 6000; partner.Mature.Aversion = 0;
        state.Dating.ActivePartnerId = partner.Id; state.Dating.ActiveApproach = DateInviteApproach.Respectful;
        state.Dating.Partners[partner.Id] = new DatingPartnerState { PositiveMoments = 4, DatesStarted = 4 };
        var dating = new DatingService(state, new PlayerStaminaService(state));
        var service = new SharedEveningService(state, data, flags, dating);
        before = JsonSerializer.Serialize(state);
        Check(service.Inspect().CanPlan && before == JsonSerializer.Serialize(state), "eligible voluntary overnight preview is read-only");
        Check(service.TryPlan(out _) && !service.TryPlan(out _) && state.Player.Stamina == 100,
            "planning is free and duplicate invitation is denied");
        service.Cancel();
        Check(!service.Inspect().Planned && partner.Bond == 60 && partner.Morale == 70, "cancelling does not punish either participant");
        foreach (var eligibility in new[] { AdultEligibility.Unknown, AdultEligibility.Ambiguous, AdultEligibility.Minor })
        {
            partner.AdultEligibility = eligibility;
            Check(!service.TryPlan(out _), "unreviewed/ambiguous/minor state denies overnight: " + eligibility);
        }
        partner.AdultEligibility = AdultEligibility.ConfirmedAdult;
        data.Characters[partner.Id].AdultEligibility = AdultEligibility.Unknown;
        Check(!service.TryPlan(out _), "runtime label alone cannot bypass definition review");
        data.Characters[partner.Id].AdultEligibility = AdultEligibility.ConfirmedAdult;
        foreach (var approach in new[] { DateInviteApproach.Pressured, DateInviteApproach.Forced })
        { state.Dating.ActiveApproach = approach; Check(!service.TryPlan(out _), "pressure is not romantic consent: " + approach); }
        state.Dating.ActiveApproach = DateInviteApproach.Respectful;
        partner.Fatigue = 90; Check(!service.TryPlan(out _), "exhausted companion can decline without cost"); partner.Fatigue = 0;
        state.Calendar.NightAction = "train"; Check(!service.TryPlan(out _), "night training cannot also receive shared-rest benefits");
        state.Calendar.NightAction = "rest"; service.TryPlan(out _);
        var snapshot = service.CaptureForSettlement();
        state.Calendar.Day++; state.Calendar.Phase = DayPhase.Morning;
        var report = new DailyReport { Day = 3 };
        Check(service.CompleteAfterSettlement(snapshot, report) && partner.Bond == 61 && partner.Morale == 72
            && state.Player.Stamina == 100 && report.Lines.Count == 1, "successful settlement records one modest bond acknowledgement, not another rest or bath reward");
        Check(!service.CompleteAfterSettlement(snapshot, report) && partner.Bond == 61 && report.Lines.Count == 1,
            "replayed completed night cannot repeat its effects");
        flags.SyncToStorage(state.Flags);
        var loaded = JsonSerializer.Deserialize<SaveState>(JsonSerializer.Serialize(state))!;
        var loadedFlags = new FlagService(); loadedFlags.SyncFromStorage(loaded.Flags);
        Check(loadedFlags.GetGlobalIntFlag(SharedEveningService.LastSettledDayFlag) == 3
            && loadedFlags.GetCharIntFlag(partner.Id, SharedEveningService.PlannedDayFlag) == 0,
            "current-schema JSON preserves receipt without a phantom invitation");
    }
}
