using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Gameplay;
using OpenMakaiRanch.World;
using static OpenMakaiRanch.Locale.LocaleCatalog;

namespace OpenMakaiRanch.Ui;

/// <summary>Local, read-only views of canonical development, food and progress. No new simulation.</summary>
public partial class WorldStationPanel
{
    private bool HasParentPage => _kind == "guide" && _id != "places"
        || _kind == "resident" && _residentPage != "overview"
        || _kind == "station" && _stationPage != "overview";

    public bool GoBack()
    {
        if (!Visible || !ContextMatches()) return false;
        if (_kind == "resident" && _residentPage != "overview")
        {
            ShowResidentPage(_residentPage switch
            {
                "development_history" or "protection" => "development",
                "gifts" => "care", _ => "overview"
            });
        }
        else if (_kind == "station" && _stationPage != "overview") ShowStationPage("overview");
        else if (_kind == "guide" && _id != "places") OpenGuide();
        else Close();
        return true;
    }

    private VBoxContainer InformationCard(string id, string heading)
    {
        var palette = _game.Theme;
        var card = new PanelContainer { Name = id, SizeFlagsHorizontal = SizeFlags.ExpandFill };
        card.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = palette.CardFill, BorderColor = palette.CardBorder,
            BorderWidthLeft = 1, BorderWidthRight = 1, BorderWidthTop = 1, BorderWidthBottom = 1,
            ContentMarginLeft = 10, ContentMarginRight = 10, ContentMarginTop = 8, ContentMarginBottom = 8,
            CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6, CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6
        });
        _content.AddChild(card);
        var body = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        body.AddThemeConstantOverride("separation", 6); card.AddChild(body);
        var title = Text(heading); title.AddThemeColorOverride("font_color", palette.HeaderText);
        body.AddChild(title);
        return body;
    }

    private void RenderResidentDevelopment(CharacterState character)
    {
        var snapshot = _game.GetCharacterDevelopment(character.Id);
        if (snapshot is null) return;
        if (_residentPage == "protection")
        {
            var body = InformationCard("ResidentResources", T("panel.resources.title", "Condition and resources"));
            var definition = _game.Roster.DefinitionFor(character);
            body.AddChild(Text(T("panel.resources.hp", "Health: {0}/{1} HP", character.Hp, definition.MaxHp)));
            body.AddChild(Text(T("panel.resources.energy", "Energy: {0}/{1}", character.Energy, definition.MaxEnergy)));
            body.AddChild(Text(T("panel.resources.spirit", "Spirit: {0}/{1} EP", snapshot.Protection.Spirit, snapshot.Protection.MaxSpirit)));
            body.AddChild(Text(T("panel.resources.mana", "Mana: {0}/{1} MP", snapshot.Protection.Mana, snapshot.Protection.MaxMana)));
            body.AddChild(Text(snapshot.Protection.Explanation));
            body.AddChild(Text(T("panel.resources.scope", "The ward display describes its resource reserve, not a list of permitted actions. EP and MP are different resources.")));
            if (snapshot.Protection.NeedsRecovery)
                body.AddChild(Text(T("panel.resources.recovery", "This resident needs recovery before taking part in activities.")));
            Action("DevelopmentCare", T("world.resident.sections.care", "Care and encouragement"), () => ShowResidentPage("care"));
            return;
        }
        if (_residentPage == "development_history")
        {
            _content.AddChild(Text(T("panel.history.title", "Recent development")));
            if (snapshot.RecentChanges.Count == 0)
                _content.AddChild(Text(T("panel.history.empty", "No recorded changes yet. A completed practical lesson or ordinary workday can establish the first comparison.")));
            foreach (var change in snapshot.RecentChanges.Reverse())
            {
                var line = Text(CharacterDevelopmentService.Describe(_game.Roster.DefinitionFor(character).DisplayName, change));
                _content.AddChild(line);
            }
            _content.AddChild(Text(T("panel.history.limit", "Up to {0} recent changes are retained. The baseline is the first observed state, not an invented history of this resident.", CharacterDevelopmentService.JournalCapacity)));
            return;
        }
        var values = InformationCard("DevelopmentOverview", T("panel.development.open", "Development and condition"));
        values.AddChild(Text(snapshot.FirstObservedDay is { } day
            ? T("panel.development.baseline", "Compared with the first observed state on day {0}.", day)
            : T("panel.development.unobserved", "A comparison starts after the first completed development action; inspecting this page grants no progress.")));
        foreach (var value in snapshot.Values.Take(4))
        {
            var name = DevelopmentFieldName(value.Field);
            values.AddChild(Text(value.Baseline is { } baseline
                ? T("panel.development.value", "{0}: {1} → {2}", name, baseline, value.Current)
                : T("panel.development.current", "{0}: {1}", name, value.Current)));
        }
        if (snapshot.Morph.HeightMillimetres > 0)
            values.AddChild(Text(T("panel.development.height", "Current height: {0:0.00} m", snapshot.Morph.HeightMillimetres / 1000.0)));
        Action("DevelopmentHistory", T("panel.history.title", "Recent development"), () => ShowResidentPage("development_history"));
        Action("DevelopmentResources", T("panel.resources.title", "Condition and resources"), () => ShowResidentPage("protection"));
        Action("DevelopmentPractice", T("world.resident.sections.practice", "Learn and practice"), () => ShowResidentPage("practice"));
        foreach (var focus in new[] { "combat", "magic" })
        {
            var track = _game.GetCharacterDevelopmentTrack(character.Id, focus);
            if (track is null) continue;
            var body = InformationCard("DevelopmentTrack_" + focus, focus == "combat"
                ? T("development.track.conditioning", "Conditioning") : T("development.track.attunement", "Magical development"));
            body.AddChild(new ProgressBar { Name = "DevelopmentProgress_" + focus,
                MinValue = 0, MaxValue = track.PointsPerStage * track.MaximumStages, Value = track.Points,
                ShowPercentage = false, CustomMinimumSize = new Vector2(0, 12), MouseFilter = MouseFilterEnum.Ignore });
            body.AddChild(Text(CharacterDevelopmentFeedback.Explain(track)));
        }
    }

    private static string DevelopmentFieldName(DevelopmentField field) => field switch
    {
        DevelopmentField.Ranch => T("character.development.ranch", "ranch skill"),
        DevelopmentField.Craft => T("character.development.craft", "craft skill"),
        DevelopmentField.Combat => T("character.development.combat", "combat skill"),
        _ => T("character.development.magic", "magical aptitude")
    };

    private void RenderProvisions()
    {
        _title.Text = T("panel.provisions.title", "Ranch provisions");
        var plan = _game.InspectNextLunch();
        var body = InformationCard("ProvisionsPlan", T("panel.provisions.next", "Next workday lunch"));
        body.AddChild(Text(T("panel.provisions.workers", "Scheduled working residents: {0}", plan.Workers)));
        body.AddChild(Text(T("panel.provisions.split", "From ranch meals: {0} • From meal boxes: {1} • Missing servings: {2}", plan.PantryMeals, plan.MealBoxes, plan.MissingMeals)));
        body.AddChild(Text(T("panel.provisions.stock", "In storage: {0} ranch meals • In your bag: {1} meal boxes",
            Math.Max(0, _game.State.Ranch.Stockpile.GetValueOrDefault("meals")), Math.Max(0, _game.State.Inventory.Items.GetValueOrDefault("meal_box")))));
        body.AddChild(Text(T("panel.provisions.order", "Existing ranch meals are used first. Food cooked during settlement is available for the next lunch, not retroactively. Reviewing this plan consumes nothing.")));
        var supplies = InformationCard("ProvisionsSupplies", T("panel.provisions.maintenance", "Facility supplies"));
        long demand = _game.Ranch.Facilities.Values.Sum(level => Math.Max(0L, level));
        supplies.AddChild(Text(T("panel.provisions.supplies", "Stored supplies: {0} • Current daily maintenance demand: {1}",
            Math.Max(0, _game.State.Ranch.Stockpile.GetValueOrDefault("supplies")), demand)));
        supplies.AddChild(Text(T("panel.provisions.forecast", "This is the plan for the current assignments and stocks. Later care, purchases or work-plan changes can alter it.")));
        Destination(new WorldDestination("ranch", "kitchen", "Kitchen"));
        Destination(new WorldDestination("ranch", "office", "Office"));
        var shop = _world.Town?.Services.FirstOrDefault(s => s.ScreenId == "shop");
        if (shop is not null) Destination(new WorldDestination("town", shop.ServiceId, shop.Label));
    }

    private void RenderAchievements()
    {
        _title.Text = T("panel.achievements.open", "Progress and optional challenges");
        var main = _game.WinCondition.InspectProgress();
        var overview = InformationCard("CampaignProgress", T("panel.campaign.title", "Main objectives"));
        overview.AddChild(Text(T("panel.campaign.progress", "Locations {0}/{1} • Bonds {2}/{3}\nFacilities {4}/{5} • Research {6}/{7}",
            main.Missions, main.MissionTarget, main.Bonds, main.BondTarget, main.Facilities, main.FacilityTarget, main.Research, main.ResearchTarget)));
        if (_game.State.VictoryDay is { } winDay)
            overview.AddChild(Text(T("panel.campaign.finished", "Completion recorded on day {0}. You can keep playing this ranch.", winDay)));
        Action("AchievementsProjects", T("project.journal.open", "Ranch projects — what could I do next?"), () => { Open("guide", "projects"); return ""; });
        _content.AddChild(Text(T("panel.achievements.help", "These optional records have no deadlines or failure penalty. They do not change the main objectives or award production bonuses.")));
        foreach (var achievement in _game.GetRanchAchievements())
        {
            var body = InformationCard("Achievement_" + achievement.Id, achievement.Title);
            body.AddChild(Text(achievement.Requirement));
            body.AddChild(Text(achievement.Complete
                ? T("panel.achievements.done", "First achieved on day {0}", achievement.CompletedDay)
                : T("panel.achievements.pending", "Not yet recorded — evaluated after a real workday")));
        }
    }
}
