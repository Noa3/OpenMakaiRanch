using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Core.Resources;
using OpenMakaiRanch.Gameplay;
using static OpenMakaiRanch.Locale.LocaleCatalog;

namespace OpenMakaiRanch.Ui;

/// <summary>
/// Screen-specific rendering methods for UiShellController.
/// </summary>
public partial class UiShellController
{
    private void RenderTitle()
    {
        var hero = CardContainer();
        _content.AddChild(hero);
        var heroInner = CardContent();
        hero.AddChild(heroInner);
        heroInner.AddChild(TitleLabel(T("screen.title", "Main Menu")));
        heroInner.AddChild(AddStyledLine(T("screen.title.subtitle", "SFW systems-first ranch management remake.")));
        heroInner.AddChild(AddStyledLine(T("screen.title.help", "Start immediately from a clean game or continue from slot 1.")));

        var cta = FlowRow(10);
        heroInner.AddChild(cta);

        var continueButton = PrimaryButton(_game.HasSaveSlot(1) ? T("screen.title.continue", "Continue Slot 1") : T("screen.title.no_save", "No Save In Slot 1"), T("tooltip.continue", "Load and continue from your last save in slot 1"));
        continueButton.Disabled = !_game.HasSaveSlot(1);
        continueButton.Visible = _game.HasSaveSlot(1);
        continueButton.Pressed += () => ExecuteUiAction(() => _game.LoadSlot(1), true, "ranch");
        cta.AddChild(continueButton);

        var newGame = SecondaryButton(T("screen.title.new_game", "New Game"), T("tooltip.new_game", "Start a fresh game from character creation"));
        newGame.Pressed += () => ExecuteUiAction(_game.NewGame, true, "character_creation");
        cta.AddChild(newGame);

        var reroll = SecondaryButton(T("screen.title.reroll_recruits", "Reroll Recruits Only"), T("tooltip.reroll_title", "Generate new random ranch hands without resetting game progress"));
        reroll.Pressed += () => ExecuteUiAction(_game.RerollGeneratedRecruits, true);
        cta.AddChild(reroll);

        var quick = CardContainer();
        _content.AddChild(quick);
        var quickInner = CardContent();
        quick.AddChild(quickInner);
        quickInner.AddChild(SubtitleLabel(T("screen.title.quick_flow", "Quick Flow")));
        quickInner.AddChild(AddStyledLine(T("screen.title.step_1", "1. Assign jobs in Schedule.")));
        quickInner.AddChild(AddStyledLine(T("screen.title.step_2", "2. Visit Town/Shop for upgrades and supplies.")));
        quickInner.AddChild(AddStyledLine(T("screen.title.step_3", "3. Build bonds and run Adventure missions.")));
        quickInner.AddChild(AddStyledLine(T("screen.title.step_4", "4. End day from the top bar and inspect reports in Overview.")));

        var recruits = CardContainer();
        _content.AddChild(recruits);
        var recruitsInner = CardContent();
        recruits.AddChild(recruitsInner);
        recruitsInner.AddChild(SubtitleLabel(T("screen.title.starting_recruits", "Starting Generated Recruits")));
        recruitsInner.AddChild(AddStyledLine(T("screen.title.recruit_info", "Every new ranch rolls extra recruits from the local talent pool.")));
        foreach (var recruit in _game.Roster.Characters.Where(character => character.IsGenerated))
        {
            var definition = _game.Roster.DefinitionFor(recruit);
            recruitsInner.AddChild(AddStyledLine($"{definition.DisplayName} - {definition.Trait} | {T("label.ranch", "Ranch")} {recruit.RanchSkill} {T("label.craft", "Craft")} {recruit.CraftSkill} {T("label.combat", "Combat")} {recruit.CombatSkill}"));
        }
    }

    private static readonly string[] LivingBuildingIds = { "office", "private_room", "barn", "guest_room", "dormitory" };
    private static readonly Dictionary<string, int> BuildingCapacities = new()
    {
        ["office"] = 1, ["private_room"] = 1, ["barn"] = 3, ["guest_room"] = 2, ["dormitory"] = 4
    };

    private void RenderRanch()
    {
        AddTitle(T("screen.ranch", "Ranch Overview"));

        if (_game.State.Calendar.Phase == DayPhase.Night) AddNightPlanningCard();
        AddRanchCommandDeck();

        var chars = _game.Roster.Characters.ToList();

        var buildingsCard = CardContainer();
        _content.AddChild(buildingsCard);
        var buildingsInner = CardContent();
        buildingsCard.AddChild(buildingsInner);
        buildingsInner.AddChild(SubtitleLabel(T("screen.ranch.locations", "Buildings & Characters")));

        foreach (var buildingId in LivingBuildingIds)
        {
            var occupantIdx = System.Array.IndexOf(LivingBuildingIds, buildingId);
            var isBuilt = buildingId switch
            {
                "office" => true,
                "private_room" => true,
                "barn" => true,
                "dormitory" => true,
                _ => FacilityLevel(buildingId) > 0
            };

            var cap = BuildingCapacities.TryGetValue(buildingId, out var capacity) ? capacity : 2;
            var occupants = new List<CharacterState>();
            if (occupantIdx >= 0 && occupantIdx < chars.Count)
            {
                var ch = chars[occupantIdx];
                if (ch is not null) occupants.Add(ch);
            }
            var used = occupants.Count;

            var buildingRow = new HBoxContainer();
            buildingRow.AddThemeConstantOverride("separation", 10);
            buildingRow.CustomMinimumSize = new Vector2(0, 100);
            buildingRow.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            buildingsInner.AddChild(buildingRow);

            var nameDef = _game.Data.Facilities.TryGetValue(buildingId, out var facDef)
                ? facDef.DisplayName
                : buildingId;

            var icon = isBuilt ? "🏠" : "🔒";
            var info = new VBoxContainer();
            info.CustomMinimumSize = new Vector2(180, 0);
            info.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
            info.AddThemeConstantOverride("separation", 4);
            buildingRow.AddChild(info);
            info.AddChild(SubtitleLabel($"{icon} {nameDef}"));
            info.AddChild(MutedLabel($"{T("screen.ranch.space", "Space")}: {used}/{cap}"));

            var charRow = new HBoxContainer();
            charRow.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            charRow.AddThemeConstantOverride("separation", 8);
            buildingRow.AddChild(charRow);

            if (!isBuilt || occupants.Count == 0)
            {
                charRow.AddChild(MutedLabel(isBuilt ? T("screen.ranch.vacant", "Vacant") : T("screen.ranch.locked", "Locked")));
                continue;
            }

            foreach (var ch in occupants)
            {
                var def = _game.Roster.DefinitionFor(ch);
                var charCard = CardContainer();
                charCard.CustomMinimumSize = new Vector2(0, 80);
                charCard.SizeFlagsHorizontal = SizeFlags.ExpandFill;
                charRow.AddChild(charCard);

                var charInner = CardContent();
                charCard.AddChild(charInner);

                var portrait = BuildCharacterVisual(ch, def);
                if (portrait is not null)
                {
                    var portraitRow = new HBoxContainer();
                    portraitRow.AddThemeConstantOverride("separation", 8);
                    charInner.AddChild(portraitRow);
                    portraitRow.AddChild(portrait);

                    var details = new VBoxContainer();
                    details.SizeFlagsHorizontal = SizeFlags.ExpandFill;
                    details.AddThemeConstantOverride("separation", 3);
                    portraitRow.AddChild(details);

                    details.AddChild(AddStyledLine(CharacterPickerName(ch)));
                    details.AddChild(MutedLabel($"HP {ch.Hp}/{def.MaxHp} | {T("label.energy", "E")} {ch.Energy} | {T("label.morale", "M")} {ch.Morale}"));

                    var jobId2 = _game.Schedule.GetAssignment(ch.Id);
                    var jobName2 = _game.Data.Jobs.TryGetValue(jobId2, out var j2) ? j2.DisplayName : "rest";
                    details.AddChild(MutedLabel($"{T("label.job", "Job")}: {jobName2}"));

                    var visitBtn = SmallButton(T("label.visit", "Visit"));
                    var capturedId = ch.Id;
                    visitBtn.Pressed += () => { _detailCharacterId = capturedId; ShowScreen("character_detail"); };
                    details.AddChild(visitBtn);
                }
            }
        }

        AddFacilityMap();

        var progress = CardContainer();
        _content.AddChild(progress);
        var progressInner = CardContent();
        progress.AddChild(progressInner);
        progressInner.AddChild(SubtitleLabel(T("screen.ranch.progress", "Endgame Progress")));
        progressInner.AddChild(MutedLabel(_game.WinCondition.ProgressSummary()));
        if (_game.WinCondition.IsGameComplete())
        {
            progressInner.AddChild(AddStyledLine(T("screen.ranch.complete", "The ranch is thriving! All objectives complete!")));
        }
    }

    private void AddRanchCommandDeck()
    {
        var summary = CardContainer();
        _content.AddChild(summary);
        var inner = CardContent();
        summary.AddChild(inner);
        inner.AddChild(SubtitleLabel(T("screen.ranch.command_deck", "Command Deck")));
        var calendar = _game.State.Calendar;
        inner.AddChild(AddStyledLine($"Year {calendar.Year} | {calendar.Season} Day {calendar.DayOfSeason:00} | {calendar.Weekday} | {calendar.Phase}"));
        inner.AddChild(MutedLabel($"Weather: {calendar.CurrentWeather}  |  Forecast: {calendar.TomorrowWeather}"));
        inner.AddChild(AddStyledLine($"{T("screen.ranch.gold", "Gold")}: {_game.Economy.Gold}  {T("screen.ranch.income", "Last income")}: {_game.State.Economy.LastIncome}  {T("label.net", "Net")}: {_game.State.Economy.LastIncome - _game.State.Economy.LastExpenses}"));
        inner.AddChild(MutedLabel($"{T("screen.ranch.health", "Ranch health")}: {_game.State.Ranch.CattleHealth}%  |  {T("screen.ranch.workload", "Workload")}: {_game.State.Ranch.Workload}%  |  {(_game.State.Ranch.BathtubClean ? T("screen.ranch.bath_clean", "Bath clean") : T("screen.ranch.bath_dirty", "Bath dirty"))}"));

        var actions = FlowRow(8);
        inner.AddChild(actions);

        var advance = DayAdvanceButton();
        AddFlowButton(actions, advance, 150);
        AddFlowButton(actions, DestinationButton(T("screen.schedule", "Daily Schedule"), "schedule", tooltip: T("tooltip.schedule", "Assign jobs before advancing time.")), 150);
        AddFlowButton(actions, DestinationButton(T("screen.town", "Town Hub"), "town", tooltip: T("tooltip.town", "Build facilities and visit town services.")), 132);
        AddFlowButton(actions, DestinationButton(T("screen.shop", "General Store"), "shop", tooltip: T("tooltip.shop", "Buy and sell supplies, equipment, and consumables.")), 132);
        AddFlowButton(actions, DestinationButton(T("screen.adventure", "Adventure Guild"), "adventure", tooltip: T("tooltip.adventure", "Select missions and prepare the party.")), 150);
        AddFlowButton(actions, DestinationButton(T("screen.saveload", "Save And Load"), "saveload", tooltip: T("tooltip.saveload", "Save or load the current game.")), 150);
    }

    private void AddFacilityMap()
    {
        AddFacilityTiles(T("screen.ranch.facility_map", "Buildings And Facilities"));
    }

    private void AddFacilityTiles(string title)
    {
        _content.AddChild(SubtitleLabel(title));
        var map = FlowRow(10);
        _content.AddChild(map);

        foreach (var facility in _game.Data.Facilities.Values.OrderBy(facility => facility.DisplayName))
        {
            var level = FacilityLevel(facility.Id);
            var cost = _game.Ranch.FacilityUpgradeCost(facility, level);
            var tile = CardContainer();
            tile.CustomMinimumSize = new Vector2(236, 150);
            tile.SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
            map.AddChild(tile);

            var inner = CardContent();
            tile.AddChild(inner);
            inner.AddChild(SubtitleLabel(facility.DisplayName));
            inner.AddChild(AddStyledLine(level > 0
                ? $"{T("label.level", "Level")} {level} | {T("screen.ranch.open", "Open")}" 
                : T("screen.ranch.locked_unbuilt", "Locked - not built")));

            if (facility.Capacity > 0)
            {
                var occupantCount = LivingBuildingIds.Contains(facility.Id) ? 1 : 0;
                inner.AddChild(MutedLabel($"{T("screen.ranch.space", "Space")}: {occupantCount}/{facility.Capacity}"));
            }

            if (facility.OutputBonus > 0)
                inner.AddChild(MutedLabel($"{T("screen.ranch.output", "Output")}: +{facility.OutputBonus}/{T("label.level", "level")} {facility.OutputResourceId}"));
            if (facility.UpkeepGold > 0)
                inner.AddChild(MutedLabel($"{T("screen.ranch.upkeep", "Upkeep")}: {facility.UpkeepGold}{T("unit.g", "g")}/{T("label.day", "day")}"));

            if (facility.BuildCost > 0)
            {
                var actionLabel = level > 0
                    ? $"{T("screen.town.upgrade", "Upgrade")} ({cost}{T("unit.g", "g")})"
                    : $"{T("screen.town.build", "Build")} ({cost}{T("unit.g", "g")})";
                var action = level > 0 ? SecondaryButton(actionLabel) : PrimaryButton(actionLabel);
                action.TooltipText = level > 0
                    ? T("tooltip.facility_upgrade", "Upgrade {0} to level {1}.", facility.DisplayName, level + 1)
                    : T("tooltip.facility_build", "Build {0} to unlock its output and services.", facility.DisplayName);
                action.Disabled = _game.Economy.Gold < cost;
                action.Pressed += () => ExecuteUiAction(() => _game.Ranch.UpgradeFacility(facility.Id, _game.Economy), false);
                inner.AddChild(action);

                if (action.Disabled)
                {
                    inner.AddChild(RequirementLabel($"{T("screen.town.need_gold", "Need")}: {cost - _game.Economy.Gold}{T("unit.g", "g")}"));
                }
            }
        }
    }

    private void AddLatestDailyReport()
    {
        if (_game.LastDailyReport is null)
        {
            return;
        }

        var report = CardContainer();
        _content.AddChild(report);
        var reportInner = CardContent();
        report.AddChild(reportInner);
        reportInner.AddChild(SubtitleLabel(T("screen.ranch.report", "Latest Daily Report")));

        var rpt = _game.LastDailyReport;
        reportInner.AddChild(AddStyledLine($"Day {rpt.Day} | Income: {rpt.Income}g | Expenses: {rpt.Expenses}g | Net: {rpt.NetGold}g"));

        if (rpt.MilkRevenue > 0)
            reportInner.AddChild(AddStyledLine($"Milk shipped: +{rpt.MilkRevenue}g"));

        if (rpt.SkillGains > 0)
            reportInner.AddChild(AddStyledLine($"Skill gains: {rpt.SkillGains} character(s) leveled up!"));
        foreach (var growth in rpt.CharacterGrowth)
        {
            reportInner.AddChild(MutedLabel($"  {growth.DisplayName}: {growth.SkillGained} skill +{growth.Amount}"));
        }

        foreach (var evt in rpt.Events)
        {
            var icon = evt.IsPositive ? "[+]" : "[-]";
            reportInner.AddChild(AddStyledLine($"{icon} {evt.Title}: {evt.Description}"));
        }

        foreach (var line in rpt.Lines)
        {
            reportInner.AddChild(MutedLabel(line));
        }
    }

    private int _reportHistoryIndex;
    private const int MaxReportHistory = 10;

    private void RenderDailyReport()
    {
        AddTitle(T("screen.report", "Daily Report"));

        var history = _game.State.Reports
            .OrderByDescending(report => report.Day)
            .ToList();
        if (history.Count == 0)
        {
            var empty = CardContainer();
            _content.AddChild(empty);
            empty.AddChild(AddStyledLine(T("screen.report.empty", "No daily reports yet. End your first day to see the daily summary.")));
            return;
        }

        if (_reportHistoryIndex >= history.Count)
        {
            _reportHistoryIndex = 0;
        }

        var rpt = history[_reportHistoryIndex];

        // Report header: day + summary toggle.
        var header = CardContainer();
        _content.AddChild(header);
        var headerInner = CardContent();
        header.AddChild(headerInner);
        headerInner.AddChild(SubtitleLabel(T("screen.report.title_pattern", "Day {0} Report", rpt.Day)));
        var milkSuffix = rpt.MilkRevenue > 0 ? $" | Shipments included: {rpt.MilkRevenue}g" : string.Empty;
        headerInner.AddChild(AddStyledLine($"Income: {rpt.Income}g | Expenses: {rpt.Expenses}g | Net: {rpt.NetGold}g{milkSuffix}"));
        if (rpt.SkillGains > 0)
        {
            headerInner.AddChild(AddStyledLine(T("screen.report.skill_gains", "{0} character(s) leveled up!", rpt.SkillGains)));
        }

        // Day navigation to browse history instead of scrolling a wall of text.
        var nav = FlowRow(8);
        headerInner.AddChild(nav);
        if (history.Count > 1)
        {
            var older = SecondaryButton(T("screen.report.older", "◀ Older Day"), "Browse the previous daily report");
            older.Disabled = _reportHistoryIndex >= history.Count - 1;
            older.Pressed += () =>
            {
                _reportHistoryIndex = Mathf.Min(history.Count - 1, _reportHistoryIndex + 1);
                _game.Feedback.PlayNavigate();
                ShowScreen("report");
            };
            nav.AddChild(older);

            var newer = SecondaryButton(T("screen.report.newer", "Newer Day ▶"), "Browse the next daily report");
            newer.Disabled = _reportHistoryIndex <= 0;
            newer.Pressed += () =>
            {
                _reportHistoryIndex = Mathf.Max(0, _reportHistoryIndex - 1);
                _game.Feedback.PlayNavigate();
                ShowScreen("report");
            };
            nav.AddChild(newer);
        }

        var back = SecondaryButton(T("label.back", "Back to Overview"), T("tooltip.report_back", "Return to the ranch overview"));
        back.Pressed += () => { _game.Feedback.PlayConfirm(); ShowScreen("ranch"); };
        nav.AddChild(back);

        // Structured, grouped log instead of one flat wall of text.
        RenderReportGroup(T("screen.report.section_events", "Events"), rpt.Events.Select(evt =>
        {
            var icon = evt.IsPositive ? "[+]" : "[-]";
            var sign = Math.Sign(evt.GoldDelta) >= 0 ? "+" : string.Empty;
            var goldSuffix = evt.GoldDelta == 0 ? string.Empty : " (" + sign + evt.GoldDelta + "g)";
            return $"{icon} {evt.Title}: {evt.Description}{goldSuffix}";
        }));

        var growth = rpt.CharacterGrowth
            .Select(growthEntry => $"{growthEntry.DisplayName}: {growthEntry.SkillGained} +{growthEntry.Amount}")
            .ToList();
        if (growth.Count > 0)
        {
            RenderReportGroup(T("screen.report.section_growth", "Skill Growth"), growth);
        }

        if (rpt.Lines.Count > 0)
        {
            RenderReportGroup(T("screen.report.section_log", "Ranch Log"), rpt.Lines);
        }
    }

    private void RenderReportGroup(string title, IEnumerable<string> entries)
    {
        var card = CardContainer();
        _content.AddChild(card);
        var inner = CardContent();
        card.AddChild(inner);
        inner.AddChild(SubtitleLabel(title));
        foreach (var entry in entries)
        {
            inner.AddChild(MutedLabel($"• {entry}"));
        }
    }

    private void RenderRoster()
    {
        AddTitle(T("screen.roster", "Characters"));
        foreach (var character in _game.Roster.Characters)
        {
            var definition = _game.Roster.DefinitionFor(character);
            var card = CardContainer();
            _content.AddChild(card);

            var row = new HBoxContainer { CustomMinimumSize = new Vector2(0, 130) };
            row.AddThemeConstantOverride("separation", 10);
            card.AddChild(row);

            row.AddChild(BuildCharacterVisual(character, definition));

            var details = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            details.AddThemeConstantOverride("separation", 6);
            row.AddChild(details);

            var nameRow = new HBoxContainer();
            nameRow.AddThemeConstantOverride("separation", 8);
            details.AddChild(nameRow);
            nameRow.AddChild(SubtitleLabel(CharacterPickerName(character)));
            var renameBtn = SmallButton(T("screen.roster.rename", "Rename"));
            renameBtn.Pressed += () =>
            {
                var dialog = new AcceptDialog
                {
                    Title = T("screen.roster.rename_title", "Rename Character"),
                    MinSize = new Vector2I(350, 120)
                };
                var nameInput = new LineEdit
                {
                    Text = CharacterPickerName(character),
                    PlaceholderText = T("screen.roster.name_placeholder", "Enter new name"),
                    CustomMinimumSize = new Vector2(300, 0)
                };
                dialog.AddChild(nameInput);
                GetTree().CurrentScene.AddChild(dialog);
                dialog.PopupCentered();
                dialog.Confirmed += () =>
                {
                    if (!string.IsNullOrWhiteSpace(nameInput.Text))
                        _game.SetRecruitName(character.Id, nameInput.Text.Trim());
                    if (IsInstanceValid(dialog)) dialog.QueueFree();
                };
                dialog.CloseRequested += () =>
                {
                    if (IsInstanceValid(dialog)) dialog.QueueFree();
                };
            };
            nameRow.AddChild(renameBtn);
            var infoLine = $"{definition.Race} | {definition.Personality} | {definition.JobClass}";
            if (!string.IsNullOrWhiteSpace(definition.Trait))
                infoLine += $" | {definition.Trait}";
            details.AddChild(MutedLabel(infoLine));
            details.AddChild(MutedLabel($"{T("label.body", "Body")}: {definition.BodyType}"));
            if (definition.Talents.Count > 0)
            {
                var talents = string.Join(", ", definition.Talents.Take(5));
                details.AddChild(MutedLabel($"Talents: {talents}{(definition.Talents.Count > 5 ? "..." : "")}"));
            }
            if (character.IsGenerated)
            {
                details.AddChild(MutedLabel(T("screen.roster.generated", "Generated recruit")));
            }

            var stats = new GridContainer { Columns = 3, SizeFlagsHorizontal = SizeFlags.ExpandFill };
            stats.AddThemeConstantOverride("h_separation", 8);
            stats.AddThemeConstantOverride("v_separation", 6);
            details.AddChild(stats);
            var effectiveRanch = character.RanchSkill + _game.Equipment.BonusRanchSkill(character.Id);
            var effectiveCraft = character.CraftSkill + _game.Equipment.BonusCraftSkill(character.Id);
            var effectiveCombat = character.CombatSkill + _game.Equipment.BonusCombatSkill(character.Id);
            stats.AddChild(StatChip($"{T("label.ranch", "Ranch")} {effectiveRanch}{(effectiveRanch > character.RanchSkill ? "*" : "")}"));
            stats.AddChild(StatChip($"{T("label.craft", "Craft")} {effectiveCraft}{(effectiveCraft > character.CraftSkill ? "*" : "")}"));
            stats.AddChild(StatChip($"{T("label.combat", "Combat")} {effectiveCombat}{(effectiveCombat > character.CombatSkill ? "*" : "")}"));
            stats.AddChild(StatChip($"{T("label.fatigue", "Fatigue")} {character.Fatigue}"));
            stats.AddChild(StatChip($"{T("label.morale", "Morale")} {character.Morale}"));
            stats.AddChild(StatChip($"{T("label.bond", "Bond")} {character.Bond}"));

            details.AddChild(StatBar(T("label.hp", "HP"), character.Hp, definition.MaxHp, new Color("55d6be")));
            details.AddChild(StatBar(T("label.energy", "Energy"), character.Energy, definition.MaxEnergy, new Color("5bbcff")));

            // Era-style action buttons
            var actionRow = FlowRow(6);
            details.AddChild(actionRow);

            var cId = character.Id;
            var milk = character.Milk;

            // Visit Slave → open the caring/visit screen
            var visitBtn = SecondaryButton("Visit Slave", "Visit and care for this character, or move on to training");
            var capturedVisitId = cId;
            visitBtn.Pressed += () =>
            {
                var charList = _game.Roster.Characters.ToList();
                var idx = charList.FindIndex(c => c.Id == capturedVisitId);
                if (idx >= 0) _visitCharIdx = idx;
                ShowScreen("visit");
            };
            actionRow.AddChild(visitBtn);

            // Customization → customize appearance
            var customBtn = SecondaryButton("Customization", "Customize appearance and traits");
            customBtn.Pressed += () => { _detailCharacterId = cId; ShowScreen("character_creation"); };
            actionRow.AddChild(customBtn);

            // Detailed Status → detail screen
            var statusBtn = SecondaryButton("Detailed Status", "View full stats and equipment");
            var capturedStatusId = cId;
            statusBtn.Pressed += () => { _detailCharacterId = capturedStatusId; ShowScreen("character_detail"); };
            actionRow.AddChild(statusBtn);

            // Automatic Scheduling → open schedule for this character
            var schedBtn = SecondaryButton("Auto Scheduling", "Assign daily jobs");
            schedBtn.Pressed += () => ShowScreen("schedule");
            actionRow.AddChild(schedBtn);

            // Lactation — show milk info + produce action
            if (milk is not null)
            {
                var milkLabel = MutedLabel($"Milk: {milk.CurrentAmount}/{milk.Capacity}ml  Quality:{milk.Quality}%");
                actionRow.AddChild(milkLabel);

                var milkBtn = SecondaryButton("Milk", "Produce milk now");
                var capturedMilkId = cId;
                milkBtn.Pressed += () => ExecuteUiAction(() =>
                {
                    _game.MilkEconomy.ProduceMilk(capturedMilkId);
                    _game.Feedback.PlayConfirm();
                    SetStatus("Produced milk!", false);
                    RefreshCurrentScreen();
                }, false);
                actionRow.AddChild(milkBtn);
            }

            // Equipment section
            var equipRow = FlowRow(4);
            details.AddChild(equipRow);
            equipRow.AddChild(MutedLabel("Equip:"));

            foreach (var slot in new[] { "weapon", "armor", "accessory", "head", "feet" })
            {
                var equipped = _game.Equipment.GetEquippedItem(character.Id, slot);
                var label = equipped?.DisplayName ?? slot;
                var btn = SmallButton(label);
                btn.Pressed += () =>
                {
                    var equipAction = () =>
                    {
                        if (equipped is not null)
                        {
                            _game.Equipment.Unequip(character.Id, slot);
                            return true;
                        }
                        // Find unequipped equipment items in inventory for this slot
                        var items = _game.State.Inventory.Items
                            .Where(kvp =>
                            {
                                if (!_game.Data.Items.TryGetValue(kvp.Key, out var def))
                                    return false;
                                return def.Category == ItemCategory.Equipment && def.Slot.ToString().ToLower() == slot && kvp.Value > 0;
                            })
                            .Select(kvp => _game.Data.Items[kvp.Key])
                            .ToList();
                        if (items.Count == 0) return false;
                        var first = items.First();
                        return _game.Equipment.Equip(character.Id, first.Id);
                    };
                    ExecuteUiAction(equipAction, false);
                };
                equipRow.AddChild(btn);
            }

            // Consumable items section
            var consumables = _game.Inventory.Items
                .Where(kvp => kvp.Value > 0 && _game.Data.Items.TryGetValue(kvp.Key, out var def) && def.Category == ItemCategory.Consumable)
                .ToList();
            if (consumables.Count > 0)
            {
                var itemRow = FlowRow(4);
                details.AddChild(itemRow);
                itemRow.AddChild(MutedLabel("Items:"));

                foreach (var kvp in consumables.Take(5))
                {
                    var capturedId = kvp.Key;
                    var btn = SmallButton($"{capturedId} ({kvp.Value})");
                    btn.Pressed += () => ExecuteUiAction(() =>
                    {
                        return _game.UseItemOnCharacter(capturedId, character.Id);
                    }, false);
                    itemRow.AddChild(btn);
                }
                if (consumables.Count > 5)
                {
                    itemRow.AddChild(MutedLabel($"+{consumables.Count - 5} more"));
                }
            }
        }
    }

    private void RenderCharacterDetail()
    {
        var character = _game.Roster.Characters.FirstOrDefault(c => c.Id == _detailCharacterId);
        if (character is null)
        {
            AddTitle(T("screen.character_detail.not_found", "Character Not Found"));
            var backBtn2 = SecondaryButton(T("label.back", "Back"));
            backBtn2.Pressed += () => ShowScreen("roster");
            _content.AddChild(backBtn2);
            return;
        }

        var definition = _game.Roster.DefinitionFor(character);
        _game.Clothing.SyncCharacterEquipment(character);

        var detailActions = FlowRow(8);
        _content.AddChild(detailActions);

        var backBtn = SecondaryButton(T("label.back", "← Back to Roster"));
        backBtn.Pressed += () => ShowScreen("roster");
        AddFlowButton(detailActions, backBtn, 148);

        if (character.Id != "anon")
        {
            var personalTime = PrimaryButton("Personal Time", "Open care, relationship and companionship/date options for this resident.");
            personalTime.Pressed += () =>
            {
                var index = _game.Roster.Characters.ToList().FindIndex(value => value.Id == character.Id);
                if (index >= 0)
                    _visitCharIdx = index;
                ShowScreen("visit");
            };
            AddFlowButton(detailActions, personalTime, 150);
        }

        AddTitle(CharacterPickerName(character));

        var card = CardContainer();
        _content.AddChild(card);

        var row = new HBoxContainer { CustomMinimumSize = new Vector2(0, 160) };
        row.AddThemeConstantOverride("separation", 12);
        card.AddChild(row);

        row.AddChild(BuildCharacterVisual(character, definition));

        var col = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        col.AddThemeConstantOverride("separation", 6);
        row.AddChild(col);

        col.AddChild(SubtitleLabel($"{definition.Race} | {definition.Personality} | {definition.JobClass}"));
        if (!string.IsNullOrWhiteSpace(definition.Description))
            col.AddChild(MutedLabel(definition.Description));
        col.AddChild(MutedLabel($"{T("label.body", "Body")}: {definition.BodyType}  |  {T("label.height", "Height")}: {definition.Height}  |  {T("label.level", "Level")}: {definition.Level}"));

        var magicPower = character.MagicPower > 0 ? character.MagicPower : definition.MagicPower;
        if (magicPower > 0)
            col.AddChild(MutedLabel($"{T("label.magic", "Magic Power")}: {magicPower}{(character.MagicPower > 0 && character.MagicPower != definition.MagicPower ? $" (base {definition.MagicPower})" : "")}"));

        // Stats grid
        var stats = new GridContainer { Columns = 4, SizeFlagsHorizontal = SizeFlags.ExpandFill };
        stats.AddThemeConstantOverride("h_separation", 10);
        stats.AddThemeConstantOverride("v_separation", 6);
        col.AddChild(stats);

        var effectiveRanch = character.RanchSkill + _game.Equipment.BonusRanchSkill(character.Id);
        var effectiveCraft = character.CraftSkill + _game.Equipment.BonusCraftSkill(character.Id);
        var effectiveCombat = character.CombatSkill + _game.Equipment.BonusCombatSkill(character.Id);

        stats.AddChild(StatChip($"{T("label.ranch", "Ranch")} {effectiveRanch}{(effectiveRanch > character.RanchSkill ? "*" : "")}"));
        stats.AddChild(StatChip($"{T("label.craft", "Craft")} {effectiveCraft}{(effectiveCraft > character.CraftSkill ? "*" : "")}"));
        stats.AddChild(StatChip($"{T("label.combat", "Combat")} {effectiveCombat}{(effectiveCombat > character.CombatSkill ? "*" : "")}"));
        stats.AddChild(StatChip($"{T("label.bond", "Bond")} {character.Bond}"));

        col.AddChild(StatBar(T("label.hp", "HP"), character.Hp, definition.MaxHp, new Color("55d6be")));
        col.AddChild(StatBar(T("label.energy", "Energy"), character.Energy, definition.MaxEnergy, new Color("5bbcff")));
        col.AddChild(StatBar(T("label.fatigue", "Fatigue"), 100 - character.Fatigue, 100, new Color("e07a5f")));
        col.AddChild(StatBar(T("label.morale", "Morale"), character.Morale, 100, new Color("e9c46a")));

        // Skill XP section
        var xpHeader = new HBoxContainer();
        xpHeader.AddThemeConstantOverride("separation", 8);
        _content.AddChild(xpHeader);
        xpHeader.AddChild(SubtitleLabel(T("screen.character_detail.skills", "Skill Development")));
        if (character.SkillXp.Count > 0)
        {
            var xpGrid = new GridContainer { Columns = 3, SizeFlagsHorizontal = SizeFlags.ExpandFill };
            xpGrid.AddThemeConstantOverride("h_separation", 12);
            xpGrid.AddThemeConstantOverride("v_separation", 4);
            _content.AddChild(xpGrid);
            foreach (var kvp in character.SkillXp.Take(12))
            {
                xpGrid.AddChild(MutedLabel($"{kvp.Key}"));
                xpGrid.AddChild(StatChip($"XP {kvp.Value}"));
                xpGrid.AddChild(new Label { Text = "" });
            }
        }
        else
        {
            xpHeader.AddChild(MutedLabel(T("screen.character_detail.no_xp", "(no growth data yet)")));
        }

        // Equipment section
        _content.AddChild(SubtitleLabel(T("screen.character_detail.equipment", "Equipment")));
        var equipGrid = new GridContainer { Columns = 2, SizeFlagsHorizontal = SizeFlags.ExpandFill };
        equipGrid.AddThemeConstantOverride("h_separation", 8);
        equipGrid.AddThemeConstantOverride("v_separation", 6);
        _content.AddChild(equipGrid);

        foreach (var slot in CharacterDetailEquipmentSlots)
        {
            var slotRow = new HBoxContainer();
            slotRow.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            slotRow.AddThemeConstantOverride("separation", 8);

            slotRow.AddChild(MutedLabel(slot.DisplayName));

            var equippedItemId = _game.Clothing.GetEquippedItemId(character, slot.Slot);
            var equipped = !string.IsNullOrWhiteSpace(equippedItemId) && _game.Data.Items.TryGetValue(equippedItemId, out var equippedDef)
                ? equippedDef
                : null;

            var buttonText = equipped?.DisplayName ?? T("screen.character_detail.empty_slot", "[Empty]");
            var button = SmallButton(buttonText);
            button.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            button.TooltipText = equipped is null
                ? T("tooltip.character_detail.equip", "Equip first available item for this slot")
                : T("tooltip.character_detail.unequip", "Unequip this item and return it to inventory");
            button.Pressed += () =>
            {
                var (success, message) = equipped is null
                    ? EquipFirstInventoryItemForSlot(character, slot.Slot)
                    : _game.UnequipCharacterItem(character.Id, slot.Slot);

                if (!success)
                {
                    _game.Feedback.PlayError();
                    SetStatus(message, true);
                    RefreshCurrentScreen();
                    return;
                }

                _game.Feedback.PlayConfirm();
                SetStatus(equipped is null
                    ? T("screen.character_detail.equipped", "Equipped {0}.", message)
                    : T("screen.character_detail.unequipped", "Unequipped {0}.", buttonText));
                RefreshCurrentScreen();
            };
            slotRow.AddChild(button);

            equipGrid.AddChild(slotRow);

            var bonusLine = EquippedBonusSummary(equipped);
            equipGrid.AddChild(string.IsNullOrWhiteSpace(bonusLine) ? new Label() : MutedLabel(bonusLine));
        }

        if (character.Equipment.ActiveClothingStyle != ClothingStyle.Default)
        {
            _content.AddChild(MutedLabel($"{T("screen.character_detail.style", "Style")}: {character.Equipment.ActiveClothingStyle}"));
        }

        // Talents section
        if (character.Talents.Count > 0)
        {
            _content.AddChild(SubtitleLabel(T("screen.character_detail.talents", "Talents")));
            var talentRow = new HBoxContainer();
            talentRow.AddThemeConstantOverride("separation", 6);
            talentRow.AddChild(new Label { Text = string.Join(", ", character.Talents) });
            _content.AddChild(talentRow);
        }
    }

    private static readonly (EquipmentSlot Slot, string DisplayName)[] CharacterDetailEquipmentSlots =
    {
        (EquipmentSlot.Weapon, "Clothes"),
        (EquipmentSlot.Armor, "Armor"),
        (EquipmentSlot.UnderwearTop, "Underwear Top"),
        (EquipmentSlot.UnderwearBottom, "Underwear Bottom"),
        (EquipmentSlot.Head, "Head"),
        (EquipmentSlot.Ears, "Eyes/Ears"),
        (EquipmentSlot.Arms, "Arms"),
        (EquipmentSlot.Legs, "Legs"),
        (EquipmentSlot.Feet, "Feet"),
        (EquipmentSlot.Necklace, "Necklace"),
        (EquipmentSlot.Coat, "Coat"),
        (EquipmentSlot.Accessory, "Accessory")
    };

    private (bool Success, string Message) EquipFirstInventoryItemForSlot(CharacterState character, EquipmentSlot slot)
    {
        var candidateId = _game.State.Inventory.Items
            .Where(kvp => kvp.Value > 0
                          && _game.Data.Items.TryGetValue(kvp.Key, out var item)
                          && item.Category == ItemCategory.Equipment
                          && item.Slot == slot)
            .Select(kvp => kvp.Key)
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(candidateId))
        {
            return (false, T("screen.character_detail.no_item_for_slot", "No inventory item available for this slot."));
        }

        var result = _game.EquipCharacterItem(character.Id, candidateId);
        if (!result.Success)
        {
            return (false, result.Error);
        }

        var itemName = _game.Data.Items.TryGetValue(candidateId, out var itemDef) ? itemDef.DisplayName : candidateId;
        return (true, itemName);
    }

    private static string EquippedBonusSummary(ItemDefinition? item)
    {
        if (item is null)
        {
            return string.Empty;
        }

        var bonuses = new List<string>();
        if (item.BonusRanchSkill != 0) bonuses.Add($"Ranch {(item.BonusRanchSkill > 0 ? "+" : string.Empty)}{item.BonusRanchSkill}");
        if (item.BonusCraftSkill != 0) bonuses.Add($"Craft {(item.BonusCraftSkill > 0 ? "+" : string.Empty)}{item.BonusCraftSkill}");
        if (item.BonusCombatSkill != 0) bonuses.Add($"Combat {(item.BonusCombatSkill > 0 ? "+" : string.Empty)}{item.BonusCombatSkill}");
        if (item.BonusMaxHp != 0) bonuses.Add($"HP {(item.BonusMaxHp > 0 ? "+" : string.Empty)}{item.BonusMaxHp}");
        if (item.BonusMaxEnergy != 0) bonuses.Add($"Energy {(item.BonusMaxEnergy > 0 ? "+" : string.Empty)}{item.BonusMaxEnergy}");
        if (item.BonusMorale != 0) bonuses.Add($"Morale {(item.BonusMorale > 0 ? "+" : string.Empty)}{item.BonusMorale}");
        return bonuses.Count == 0 ? string.Empty : string.Join(" | ", bonuses);
    }

    private void RenderSchedule()
    {
        var generation = _game.StateGeneration;
        AddTitle(T("screen.schedule", "Daily Schedule"));

        foreach (var character in _game.Roster.Characters)
        {
            var definition = _game.Roster.DefinitionFor(character);
            var card = CardContainer();
            _content.AddChild(card);

            var currentJob = _game.Schedule.GetAssignment(character.Id);
            var currentJobName = _game.Data.Jobs.TryGetValue(currentJob, out var jDef) ? jDef.DisplayName : "Rest";
            card.AddChild(SubtitleLabel($"{definition.DisplayName} — {currentJobName}"));
            card.AddChild(MutedLabel($"HP {character.Hp}/{definition.MaxHp} | {T("label.energy", "E")}: {character.Energy}/{definition.MaxEnergy} | {T("label.morale", "M")}: {character.Morale}"));

            var row = FlowRow(8);
            card.AddChild(row);

            foreach (var job in _game.Schedule.AssignableJobs)
            {
                var tooltipBits = new System.Collections.Generic.List<string>();
                if (job.GoldIncome > 0) tooltipBits.Add($"+{job.GoldIncome} gold");
                if (job.ResourceAmount > 0) tooltipBits.Add($"+{job.ResourceAmount} {job.ResourceId}");
                if (job.FatigueDelta > 0) tooltipBits.Add($"+{job.FatigueDelta} fatigue");
                if (job.FatigueDelta < 0) tooltipBits.Add($"{-job.FatigueDelta} fatigue recovery");
                if (job.MoraleDelta != 0) tooltipBits.Add($"{(job.MoraleDelta > 0 ? "+" : "")}{job.MoraleDelta} morale");
                if (job.BondDelta > 0) tooltipBits.Add($"+{job.BondDelta} bond");
                var tooltip = string.Join(", ", tooltipBits);
                var isCurrent = job.Id == currentJob;

                Button button;
                if (isCurrent)
                {
                    button = PrimaryButton(job.DisplayName, $"{tooltip} (current)");
                }
                else
                {
                    button = SecondaryButton(job.DisplayName, tooltip);
                }
                button.Disabled = isCurrent;
                button.Pressed += () => ExecuteUiAction(() => _game.TryAssignJob(character.Id, job.Id, generation), true);
                AddFlowButton(row, button, 150);
            }
        }
    }

    private void RenderTown()
    {
        var header = new HBoxContainer();
        header.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _content.AddChild(header);
        var titleLabel = TitleLabel(T("screen.town", "Town Hub"));
        titleLabel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        header.AddChild(titleLabel);
        var returnBtn = SecondaryButton(T("screen.town.return", "Return to Ranch"), T("tooltip.return_ranch", "Head back to your ranch"));
        returnBtn.SizeFlagsHorizontal = SizeFlags.ShrinkEnd;
        returnBtn.Pressed += () =>
        {
            if (!RequestWorldTravel("ranch"))
            {
                ShowScreen("ranch");
            }
        };
        header.AddChild(returnBtn);

        _content.AddChild(MutedLabel(T("screen.town.subtitle", "Okachi Town — Choose a building to visit.")));

        var buildings = FlowRow(12);
        _content.AddChild(buildings);

        AddTownBuilding(buildings, T("screen.shop", "General Store"), T("tooltip.shop", "Buy and sell supplies, equipment, and consumables"), "shop");
        AddTownBuilding(buildings, T("screen.adventure", "Adventure Guild"), T("tooltip.adventure", "Dispatch characters on missions and patrols"), "adventure");
        AddTownBuilding(buildings, T("screen.research", "Research Office"), T("tooltip.research", "Unlock new skills and technologies"), "research", "workshop");
        AddTownBuilding(buildings, T("screen.town.tavern", "Tavern"), T("tooltip.tavern", "Recruit adventurers, hear rumors, hire help"), "roster");
        AddTownBuilding(buildings, T("screen.town.bathhouse", "Bathhouse"), T("tooltip.bathhouse", "Raise morale and strengthen bonds between characters"), "bond");
        AddTownBuilding(buildings, T("screen.milestones", "Town Hall"), T("tooltip.milestones", "View records, achievements, and endgame progress"), "milestones");

        AddFacilityTiles(T("screen.town.facility_planning", "Facility Planning"));
    }

    private void AddTownBuilding(HFlowContainer row, string name, string desc, string screenId, string? requiredFacilityId = null)
    {
        var tile = CardContainer();
        tile.CustomMinimumSize = new Vector2(190, 130);
        tile.SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
        row.AddChild(tile);

        var inner = CardContent();
        tile.AddChild(inner);

        inner.AddChild(SubtitleLabel(name));
        inner.AddChild(MutedLabel(desc));

        var canEnter = true;
        var requirement = string.Empty;
        if (requiredFacilityId is not null)
        {
            canEnter = HasBuiltFacility(requiredFacilityId, out requirement);
        }

        var button = canEnter
            ? SmallButton(T("screen.town.enter", "Enter"))
            : SmallButton(T("screen.town.locked", "Locked"));
        button.Disabled = !canEnter;
        button.TooltipText = canEnter ? desc : requirement;
        button.Pressed += () =>
        {
            if (canEnter)
            {
                _game.Feedback.PlayNavigate();
                ShowScreen(screenId);
            }
        };
        inner.AddChild(button);
    }

    private void RenderShop()
    {
        AddTitle(T("screen.shop", "General Store"));

        var market = CardContainer();
        _content.AddChild(market);
        market.AddChild(SubtitleLabel(T("screen.shop.buy", "Buy Supplies")));
        foreach (var item in _game.Data.ShopItems())
        {
            var row = FlowRow(8);
            market.AddChild(row);

            row.AddChild(AddStyledLine($"{item.DisplayName} - {item.Price}{T("unit.g", "g")}: {item.Description}", true));
            var buy = PrimaryButton(T("common.buy", "Buy"), $"{T("tooltip.buy", "Purchase")} {item.DisplayName} ({item.Price}{T("unit.g", "g")})");
            buy.Disabled = _game.Economy.Gold < item.Price;
            buy.Pressed += () => ExecuteUiAction(() => _game.Shop.Buy(item.Id, 1), false);
            AddFlowButton(row, buy, 92);
        }

        var inventory = CardContainer();
        _content.AddChild(inventory);
        inventory.AddChild(SubtitleLabel(T("screen.shop.inventory", "Inventory")));
        foreach (var item in _game.Inventory.Items)
        {
            var row = FlowRow(8);
            inventory.AddChild(row);

            row.AddChild(AddStyledLine($"{item.Key}: {item.Value}", true));

            var sell = SecondaryButton(T("common.sell", "Sell"), T("tooltip.sell", "Sell one unit for half the purchase price"));
            sell.Disabled = item.Value <= 0;
            sell.Pressed += () => ExecuteUiAction(() => _game.Shop.Sell(item.Key, 1), false);
            AddFlowButton(row, sell, 92);

            var def = _game.Data.Item(item.Key);
            if (def.Category == ItemCategory.Consumable && _game.Roster.Characters.Any())
            {
                var use = SecondaryButton(T("screen.shop.use_tiredest", "Use On Tiredest"), T("tooltip.use_tiredest", "Use this item on the most fatigued character"));
                use.Disabled = item.Value <= 0;
                use.Pressed += () => ExecuteUiAction(() =>
                {
                    var target = _game.Roster.Characters.OrderByDescending(c => c.Fatigue).FirstOrDefault();
                    return target is not null && _game.UseItemOnCharacter(item.Key, target.Id);
                }, false);
                AddFlowButton(row, use, 150);
            }
        }
    }

    private void RenderAdventure()
    {
        AddTitle(T("screen.adventure", "Adventure Guild"));

        // === Discovery progress ===
        var discoveryCard = CardContainer();
        _content.AddChild(discoveryCard);
        discoveryCard.AddChild(SubtitleLabel(T("screen.adventure.discovery", "Exploration")));
        var discoveredCount = _game.Discovery.DiscoveredCount;
        var totalMissions = _game.Data.Missions.Count;
        discoveryCard.AddChild(AddStyledLine($"{T("screen.adventure.missions_discovered", "Missions Discovered")}: {discoveredCount}/{totalMissions}"));

        if (!_game.Discovery.AllDiscovered)
        {
            var scoutBtn = SecondaryButton(T("screen.adventure.scout", "Scout Area"), T("tooltip.scout", "Send scouts to discover new missions. Each scout costs 15g."));
            scoutBtn.Pressed += () =>
            {
                if (_game.Economy.Spend(15))
                {
                    _game.Discovery.DiscoverNext();
                    _game.Feedback.PlayConfirm();
                }
                ShowScreen(_currentScreen);
            };
            discoveryCard.AddChild(scoutBtn);
        }

        // === Party Selection ===
        var party = CardContainer();
        _content.AddChild(party);
        party.AddChild(SubtitleLabel(T("screen.adventure.party", "Party Selection")));
        foreach (var character in _game.Roster.Characters)
        {
            var definition = _game.Roster.DefinitionFor(character);
            var selected = _game.State.Adventure.SelectedPartyIds.Contains(character.Id);
            var row = new HBoxContainer();
            row.AddThemeConstantOverride("separation", 8);
            party.AddChild(row);

            var statsStr = $"{T("label.hp", "HP")}:{character.Hp} {T("roster.energy", "En")}:{character.Energy} {T("roster.combat_skill", "Cbt")}:{character.CombatSkill}";
            var tooltip = T("tooltip.party_member", $"{definition.DisplayName}: {statsStr}");

            var button = selected ? PrimaryButton($"{T("screen.adventure.in_party", "In Party")}: {definition.DisplayName}", tooltip) : SecondaryButton($"{T("screen.adventure.add_to_party", "Add")}: {definition.DisplayName}", tooltip);
            button.TooltipText = tooltip;
            var capturedId = character.Id;
            button.Pressed += () => ExecuteUiAction(() => _game.TogglePartyMember(capturedId), true);
            row.AddChild(button);

            var statsLabel = MutedLabel(statsStr);
            statsLabel.TooltipText = tooltip;
            statsLabel.CustomMinimumSize = new Vector2(0, 30);
            row.AddChild(statsLabel);
        }

        party.AddChild(AddStyledLine($"{T("screen.adventure.party_size", "Party size")}: {_game.State.Adventure.SelectedPartyIds.Count}/{_game.Roster.Characters.Count}"));

        // === Mercenary Hiring ===
        var mercCard = CardContainer();
        _content.AddChild(mercCard);
        mercCard.AddChild(SubtitleLabel(T("screen.adventure.mercenaries", "Mercenaries")));
        if (_game.Mercenary.AvailableMercenaries().Count == 0)
            _game.Mercenary.RefreshMercenaries();

        foreach (var merc in _game.Mercenary.AvailableMercenaries())
        {
            var mercRow = new HBoxContainer();
            mercRow.AddThemeConstantOverride("separation", 8);
            mercCard.AddChild(mercRow);

            var hireBtn = PrimaryButton($"{T("screen.adventure.hire", "Hire")} {merc.DisplayName} ({merc.Cost}{T("unit.g", "g")})", T("tooltip.hire_merc", $"{merc.DisplayName}: Combat {merc.CombatSkill}, HP+{merc.HpBonus}. Lasts 1 mission."));
            var mercId = merc.Id;
            hireBtn.Pressed += () =>
            {
                if (_game.Mercenary.Hire(mercId, out var hired) && hired is not null)
                {
                    _game.State.Adventure.ActiveMercenaryHpBonus += hired.HpBonus;
                    _game.Feedback.PlayConfirm();
                }
                ShowScreen(_currentScreen);
            };
            mercRow.AddChild(hireBtn);

            mercRow.AddChild(MutedLabel($"{merc.DisplayName}: {T("roster.combat_skill", "Combat")} {merc.CombatSkill}, HP+{merc.HpBonus}"));
        }

        if (_game.State.Adventure.ActiveMercenaryHpBonus > 0)
            mercCard.AddChild(AddStyledLine($"{T("screen.combat.merc_bonus", "Active merc bonus")}: +{_game.State.Adventure.ActiveMercenaryHpBonus} HP"));

        // === Capture Target Preferences ===
        var prefCard = CardContainer();
        _content.AddChild(prefCard);
        prefCard.AddChild(SubtitleLabel(T("screen.adventure.capture_target", "Capture Target")));

        var prefs = _game.State.Adventure.CapturePrefs;

        var raceRow = new HBoxContainer();
        raceRow.AddThemeConstantOverride("separation", 8);
        prefCard.AddChild(raceRow);
        raceRow.AddChild(MutedLabel(T("screen.adventure.race", "Race")));
        var racePicker = StyledPicker();
        racePicker.AddItem(T("screen.adventure.any", "Any"));
        foreach (var raceName in CharacterGenerationPools.Races)
        {
            racePicker.AddItem(raceName);
        }

        var raceIndex = 0;
        for (int i = 1; i < racePicker.ItemCount; i++)
        {
            if (racePicker.GetItemText(i) == prefs.Race)
            {
                raceIndex = i;
                break;
            }
        }
        racePicker.Select(raceIndex);
        racePicker.ItemSelected += index => prefs.Race = racePicker.GetItemText((int)index);
        raceRow.AddChild(racePicker);

        var bustRow = new HBoxContainer();
        bustRow.AddThemeConstantOverride("separation", 8);
        prefCard.AddChild(bustRow);
        bustRow.AddChild(MutedLabel(T("screen.adventure.bust", "Bust Size")));
        var bustPicker = StyledPicker();
        bustPicker.AddItem(T("screen.adventure.any", "Any"));
        for (var bust = 0; bust <= 15; bust++)
        {
            bustPicker.AddItem(bust.ToString());
        }
        var bustIndex = 0;
        for (var i = 1; i < bustPicker.ItemCount; i++)
        {
            if (int.TryParse(bustPicker.GetItemText(i), out var bustValue) && bustValue == (int.TryParse(prefs.BustSize, out var prefsBust) ? prefsBust : -1))
            {
                bustIndex = i;
                break;
            }
        }
        bustPicker.Select(bustIndex);
        bustPicker.ItemSelected += index =>
        {
            prefs.BustSize = index == 0 ? "Any" : bustPicker.GetItemText((int)index);
        };
        bustRow.AddChild(bustPicker);

        var jobRow = new HBoxContainer();
        jobRow.AddThemeConstantOverride("separation", 8);
        prefCard.AddChild(jobRow);
        jobRow.AddChild(MutedLabel(T("screen.adventure.job", "Job")));
        var jobPicker = StyledPicker();
        jobPicker.AddItem(T("screen.adventure.any", "Any"));
        foreach (var jobName in CharacterGenerationPools.AllJobs)
        {
            jobPicker.AddItem(jobName);
        }
        var jobIndex = 0;
        for (var i = 1; i < jobPicker.ItemCount; i++)
        {
            if (jobPicker.GetItemText(i) == prefs.Job)
            {
                jobIndex = i;
                break;
            }
        }
        jobPicker.Select(jobIndex);
        jobPicker.ItemSelected += index => prefs.Job = jobPicker.GetItemText((int)index);
        jobRow.AddChild(jobPicker);

        var manaRow = new HBoxContainer();
        manaRow.AddThemeConstantOverride("separation", 8);
        prefCard.AddChild(manaRow);
        manaRow.AddChild(MutedLabel(T("screen.adventure.mana", "Mana (1-5)")));
        var manaPicker = StyledPicker();
        manaPicker.AddItem(T("screen.adventure.any", "Any"));
        for (var mana = 1; mana <= 5; mana++)
        {
            manaPicker.AddItem(mana.ToString());
        }
        var manaIndex = 0;
        for (var i = 1; i < manaPicker.ItemCount; i++)
        {
            if (int.TryParse(manaPicker.GetItemText(i), out var manaLevel) && manaLevel == prefs.ManaAmount)
            {
                manaIndex = i;
                break;
            }
        }
        manaPicker.Select(manaIndex);
        manaPicker.ItemSelected += index =>
        {
            prefs.ManaAmount = index == 0 ? 0 : int.Parse(manaPicker.GetItemText((int)index));
        };
        manaRow.AddChild(manaPicker);

        prefCard.AddChild(MutedLabel(T("screen.adventure.capture_target_hint", "A captured target will be generated with your chosen race, bust, job, and mana. Leave at Any to roll freely.")));

        // === Last mission result ===
        if (!string.IsNullOrEmpty(_game.State.Adventure.LastSummary) && _game.State.Adventure.LastSummary != "No adventure has been attempted yet.")
        {
            var lastResult = CardContainer();
            _content.AddChild(lastResult);
            lastResult.AddChild(SubtitleLabel(T("screen.adventure.last_mission", "Last Mission Result")));
            lastResult.AddChild(AddStyledLine(_game.State.Adventure.LastSummary));
            if (_game.LastCombatReport is not null)
            {
                lastResult.AddChild(AddStyledLine($"{T("screen.combat.reward", "Reward")}: {_game.LastCombatReport.RewardGold}g{(string.IsNullOrWhiteSpace(_game.LastCombatReport.RewardItemId) ? "" : $" + {_game.LastCombatReport.RewardItemId}")}"));
                if (_game.LastCombatReport.CaptureAttempted)
                {
                    lastResult.AddChild(AddStyledLine(_game.LastCombatReport.CaptureSucceeded
                        ? T("screen.combat.capture_success", "Capture succeeded!")
                        : T("screen.combat.capture_failed", "Capture failed.")));
                }
            }
        }

        // === Missions ===
        var missions = CardContainer();
        _content.AddChild(missions);
        missions.AddChild(SubtitleLabel(T("screen.adventure.missions", "Missions")));

        var available = _game.Discovery.AvailableMissions();
        if (available.Count == 0)
        {
            missions.AddChild(MutedLabel(T("screen.adventure.no_missions", "No missions discovered yet. Scout the area first!")));
        }

        foreach (var mission in available)
        {
            var missionCard = CardContainer();
            missions.AddChild(missionCard);

            var header = AddStyledLine(mission.DisplayName, true);
            header.TooltipText = T("tooltip.mission_detail", $"{mission.DisplayName}: {mission.Tier} zone, Difficulty {mission.Difficulty}/30");
            missionCard.AddChild(header);

            var detailRow = new HBoxContainer();
            detailRow.AddThemeConstantOverride("separation", 12);
            missionCard.AddChild(detailRow);

            string tierStr = mission.Tier switch { MissionTier.Local => "\u2605", MissionTier.Regional => "\u2605\u2605", MissionTier.Dangerous => "\u2605\u2605\u2605", _ => "" };
            detailRow.AddChild(MutedLabel($"{tierStr} {T("screen.adventure.difficulty", "Diff")}: {mission.Difficulty}"));

            var rewardStr = $"{mission.RewardGold}{T("unit.g", "g")}";
            if (!string.IsNullOrEmpty(mission.RewardItemId) && _game.Data.Items.TryGetValue(mission.RewardItemId, out var itemDef))
                rewardStr += $" + {itemDef.DisplayName}";
            detailRow.AddChild(MutedLabel($"{T("screen.combat.reward", "Reward")}: {rewardStr}"));

            var actionRow = FlowRow(6);
            missionCard.AddChild(actionRow);

            var fightBtn = PrimaryButton(T("screen.combat.fight", "Fight"), T("tooltip.fight", "Engage in round-based combat with auto-battle support."));
            var capturedMissionId = mission.Id;
            fightBtn.Pressed += () =>
            {
                _game.StartNewCombat();
                _game.LastCombatReport = new CombatReport { MissionId = capturedMissionId };
                ShowScreen("combat");
            };
            AddFlowButton(actionRow, fightBtn, 110);

            var captureBtn = SecondaryButton(T("screen.adventure.capture", "Capture"), T("tooltip.capture_mission", "Battle with a capture attempt. Requires a hired mercenary and 1 Mana Shackle. Success may recruit a target."));
            captureBtn.Pressed += () =>
            {
                _game.StartNewCombat();
                _game.LastCombatReport = new CombatReport { MissionId = capturedMissionId, CaptureAttempted = true };
                ShowScreen("combat");
            };
            AddFlowButton(actionRow, captureBtn, 120);
        }

        missions.AddChild(MutedLabel($"{T("screen.adventure.capture_hint", "Capture Battle: requires a hired mercenary and 1 Mana Shackle (held: ")}{(_game.State.Inventory.Items.TryGetValue("mana_shackle", out var shackles) ? shackles : 0)}{T("screen.adventure.capture_hint2", ", mercenary bonus ")}: +{_game.State.Adventure.ActiveMercenaryHpBonus}). High party control improves success."));
    }

    private void RenderCombat()
    {
        AddTitle(T("screen.combat", "Combat And Mission Result"));

        var pauseCard = CardContainer();
        _content.AddChild(pauseCard);
        pauseCard.AddChild(SubtitleLabel(T("screen.combat.world_paused", "⏸ World Time Paused")));
        pauseCard.AddChild(MutedLabel(T("screen.combat.world_paused_hint",
            "Combat is turn-based. Ranch phase, weather progression and daily settlement cannot advance until you leave combat.")));

        if (_game.CurrentCombatPhase == CombatPhase.PreBattle)
        {
            RenderCombatPreBattle();
            return;
        }

        if (_game.CurrentCombatPhase == CombatPhase.PlayerTurn)
        {
            RenderCombatPlayerTurn();
            return;
        }

        if (_game.CurrentCombatPhase == CombatPhase.BattleResults)
        {
            RenderCombatResults();
            return;
        }

        RenderCombatOutro();
    }

    private void RenderCombatPreBattle()
    {
        var card = CardContainer();
        _content.AddChild(card);
        card.AddChild(SubtitleLabel(T("screen.combat.pre_battle", "Prepare for Battle")));
        card.AddChild(MutedLabel(T("screen.combat.rules",
            "Round order favors higher Speed. Attack is reduced by Defense; Defend halves incoming damage while active. Skills can provide utility/healing.")));

        var missionId = _game.LastCombatReport?.MissionId ?? _game.State.Adventure.LastMissionId;
        var mission = _game.Data.Missions.Values.FirstOrDefault(m => m.Id == missionId);
        if (mission is not null)
        {
            card.AddChild(AddStyledLine($"{T("screen.adventure.mission", "Mission")}: {mission.DisplayName}"));
            card.AddChild(AddStyledLine($"{T("screen.adventure.difficulty", "Difficulty")}: {mission.Difficulty}/30"));

            var enemies = _game.Combat.PickEnemies(mission);
            foreach (var enemy in enemies)
            {
                var enemyCard = CardContainer();
                _content.AddChild(enemyCard);
                enemyCard.AddChild(SubtitleLabel(enemy.DisplayName));
                enemyCard.AddChild(AddStyledLine($"{T("label.hp", "HP")}: {enemy.BaseHp} | {T("roster.energy", "ATK")}: {enemy.Attack} | {T("label.defense", "DEF")}: {enemy.Defense} | {T("label.speed", "SPD")}: {enemy.Speed}"));
                enemyCard.AddChild(AddStyledLine($"{T("screen.combat.reward", "Reward")}: {mission.RewardGold}{T("unit.g", "g")}{(string.IsNullOrEmpty(mission.RewardItemId) ? "" : $" + {mission.RewardItemId}")}"));
            }
        }

        var partyCard = CardContainer();
        _content.AddChild(partyCard);
        partyCard.AddChild(SubtitleLabel(T("screen.adventure.party", "Party")));
        var partyChars = _game.Roster.Characters
            .Where(c => _game.State.Adventure.SelectedPartyIds.Contains(c.Id) || _game.State.Adventure.SelectedPartyIds.Count == 0)
            .ToList();
        foreach (var c in partyChars)
        {
            int combatHp = Math.Max(50, c.Hp / 20);
            partyCard.AddChild(AddStyledLine($"{c.DisplayNameOverride}: {T("label.hp", "HP")} {combatHp} | {T("roster.energy", "Energy")} {c.Energy} | {T("roster.combat_skill", "Combat")} {c.CombatSkill}"));
        }

        // Mercenary HP bonus display
        if (_game.State.Adventure.ActiveMercenaryHpBonus > 0)
            partyCard.AddChild(AddStyledLine($"{T("screen.combat.merc_bonus", "Mercenary HP Bonus")}: +{_game.State.Adventure.ActiveMercenaryHpBonus}"));

        var actions = FlowRow(10);
        _content.AddChild(actions);

        var activeMissionId = mission?.Id ?? _game.State.Adventure.LastMissionId;
        var adventureCost = _game.AdventureStaminaCost(activeMissionId);
        var tacticalLabel = adventureCost > 0 ? $"Tactical Battle ({adventureCost} Stamina)" : "Tactical Battle (Tutorial — free)";
        var tacticalBtn = PrimaryButton(tacticalLabel, "Control the ranch owner each round. Companions and enemies act automatically; HP/SP/MP persist through the battle.");
        tacticalBtn.Disabled = !_game.CanStartInteractiveCombat(activeMissionId);
        tacticalBtn.Pressed += () =>
        {
            if (_game.BeginInteractiveCombat(activeMissionId))
                ShowScreen(_currentScreen);
        };
        AddFlowButton(actions, tacticalBtn, 178);

        var autoLabel = adventureCost > 0 ? $"Auto Battle ({adventureCost} Stamina)" : "Auto Battle (Tutorial — free)";
        var autoBtn = SecondaryButton(autoLabel, T("tooltip.auto_battle", "Resolve all combat rounds automatically with AI tactics. World time stays paused."));
        autoBtn.Disabled = !_game.CanStartAdventure(activeMissionId);
        autoBtn.Pressed += () =>
        {
            _game.RunRoundBasedMission(activeMissionId, true);
            ShowScreen(_currentScreen);
        };
        AddFlowButton(actions, autoBtn, 150);

        var captureBtn = SecondaryButton(adventureCost > 0 ? $"Capture Battle ({adventureCost} Stamina)" : "Capture Battle (Tutorial — free)", T("tooltip.capture_battle", "Fight with capture attempt. Success may recruit an enemy!"));
        captureBtn.Disabled = !_game.CanStartAdventure(activeMissionId);
        captureBtn.Pressed += () =>
        {
            _game.RunRoundBasedCapture(activeMissionId);
            ShowScreen(_currentScreen);
        };
        AddFlowButton(actions, captureBtn, 160);

        var backBtn = SecondaryButton(T("common.back", "Back"));
        backBtn.Pressed += () =>
        {
            _game.EndCombatSession();
            ShowScreen("adventure");
        };
        AddFlowButton(actions, backBtn, 96);

        if (!_game.CanStartInteractiveCombat(activeMissionId) && _game.CanStartAdventure(activeMissionId))
            _content.AddChild(RequirementLabel("Tactical Battle requires the ranch owner in the selected party."));
    }

    private void RenderCombatPlayerTurn()
    {
        var session = _game.ActiveCombatSession;
        if (session is null)
        {
            var missing = CardContainer();
            _content.AddChild(missing);
            missing.AddChild(AddStyledLine("No active tactical combat session."));
            var returnBtn = SecondaryButton(T("common.back", "Back"));
            returnBtn.Pressed += () =>
            {
                _game.EndCombatSession();
                ShowScreen("adventure");
            };
            _content.AddChild(returnBtn);
            return;
        }

        var player = session.PlayerState;
        if (player is null)
        {
            var waiting = CardContainer();
            _content.AddChild(waiting);
            waiting.AddChild(AddStyledLine("Resolving companion and enemy turns..."));
            return;
        }

        var turnCard = CardContainer();
        _content.AddChild(turnCard);
        turnCard.AddChild(SubtitleLabel($"Round {session.RoundNumber} — {player.DisplayName}'s Turn"));
        turnCard.AddChild(AddStyledLine(
            $"HP {player.CurrentHp}/{player.MaxHp}   SP {player.CurrentSp}/{player.MaxSp}   MP {player.CurrentMana}/{player.MaxMana}", true));
        turnCard.AddChild(MutedLabel(
            "Daily Stamina was committed when the battle began. Combat commands use combat HP/SP/MP, not additional daily Stamina."));

        var enemyCard = CardContainer();
        _content.AddChild(enemyCard);
        enemyCard.AddChild(SubtitleLabel("Enemies"));
        foreach (var enemy in session.EnemyState)
        {
            var status = enemy.IsAlive ? $"{enemy.CurrentHp}/{enemy.MaxHp} HP" : "Defeated";
            enemyCard.AddChild(AddStyledLine($"{enemy.DisplayName}: {status}"));
        }

        var partyCard = CardContainer();
        _content.AddChild(partyCard);
        partyCard.AddChild(SubtitleLabel("Party"));
        foreach (var ally in session.PartyState)
        {
            var status = ally.IsAlive
                ? $"{ally.CurrentHp}/{ally.MaxHp} HP   {ally.CurrentSp}/{ally.MaxSp} SP"
                : "Fallen";
            partyCard.AddChild(AddStyledLine($"{ally.DisplayName}: {status}"));
        }

        var attackCard = CardContainer();
        _content.AddChild(attackCard);
        attackCard.AddChild(SubtitleLabel("Attack"));
        var attackRow = FlowRow(8);
        attackCard.AddChild(attackRow);
        foreach (var enemy in session.EnemyState.Where(value => value.IsAlive))
        {
            var capturedEnemyId = enemy.Id;
            var attack = PrimaryButton($"Attack {enemy.DisplayName}", $"Basic attack against {enemy.DisplayName}. No SP or MP cost.");
            attack.Pressed += () =>
            {
                if (!_game.SubmitInteractiveCombatAction(CombatPlayerCommand.Attack, capturedEnemyId))
                    SetStatus(_game.ActiveCombatSession?.LastError ?? "Attack could not be resolved.", false);
                ShowScreen(_currentScreen);
            };
            AddFlowButton(attackRow, attack, 148);
        }

        var tacticsCard = CardContainer();
        _content.AddChild(tacticsCard);
        tacticsCard.AddChild(SubtitleLabel("Tactics"));
        var tacticsRow = FlowRow(8);
        tacticsCard.AddChild(tacticsRow);

        var defend = SecondaryButton("Defend", "Halve all incoming attack damage for the rest of this round.");
        defend.Pressed += () =>
        {
            if (!_game.SubmitInteractiveCombatAction(CombatPlayerCommand.Defend))
                SetStatus(_game.ActiveCombatSession?.LastError ?? "Defend could not be resolved.", false);
            ShowScreen(_currentScreen);
        };
        AddFlowButton(tacticsRow, defend, 120);

        var injuredAllies = session.PartyState.Where(value => value.IsAlive && value.CurrentHp < value.MaxHp).ToList();
        foreach (var ally in injuredAllies)
        {
            var capturedAllyId = ally.Id;
            var skill = SecondaryButton($"Skill Heal {ally.DisplayName} (-{CombatService.SkillSpCost} SP)",
                "Combat support skill. Uses this combatant's finite SP.");
            skill.Disabled = player.CurrentSp < CombatService.SkillSpCost;
            skill.Pressed += () =>
            {
                if (!_game.SubmitInteractiveCombatAction(CombatPlayerCommand.Skill, capturedAllyId))
                    SetStatus(_game.ActiveCombatSession?.LastError ?? "Skill could not be resolved.", false);
                ShowScreen(_currentScreen);
            };
            AddFlowButton(tacticsRow, skill, 190);

            var magic = SecondaryButton($"Magic Heal {ally.DisplayName} (-{CombatService.PlayerMagicHealCost} MP)",
                "Uses the ranch owner's persistent personal MP pool.");
            magic.Disabled = player.CurrentMana < CombatService.PlayerMagicHealCost;
            magic.Pressed += () =>
            {
                if (!_game.SubmitInteractiveCombatAction(CombatPlayerCommand.Magic, capturedAllyId))
                    SetStatus(_game.ActiveCombatSession?.LastError ?? "Magic could not be resolved.", false);
                ShowScreen(_currentScreen);
            };
            AddFlowButton(tacticsRow, magic, 194);
        }

        var autoFinish = SecondaryButton("Auto Finish", "Hand the remaining player turns to the same deterministic combat AI.");
        autoFinish.Pressed += () =>
        {
            _game.AutoFinishInteractiveCombat();
            ShowScreen(_currentScreen);
        };
        AddFlowButton(tacticsRow, autoFinish, 132);

        if (!string.IsNullOrWhiteSpace(session.LastError))
            _content.AddChild(RequirementLabel(session.LastError));

        var latestRound = session.Report.Rounds.LastOrDefault();
        if (latestRound is not null && latestRound.Actions.Count > 0)
        {
            var log = CardContainer();
            _content.AddChild(log);
            log.AddChild(SubtitleLabel($"Round {latestRound.RoundNumber} actions"));
            foreach (var action in latestRound.Actions.TakeLast(8))
                log.AddChild(MutedLabel(action.Description));
        }
    }

    private void RenderCombatResults()
    {
        var report = _game.LastCombatReport;
        if (report is null)
        {
            var card = CardContainer();
            _content.AddChild(card);
            card.AddChild(AddStyledLine(_game.State.Adventure.LastSummary));
            return;
        }

        // Outcome header
        var outcomeColor = report.Outcome switch
        {
            MissionOutcome.Success => "55d6be",
            MissionOutcome.PartialSuccess => "f0c060",
            _ => "ff6666"
        };
        var outcomeCard = CardContainer();
        _content.AddChild(outcomeCard);
        var outcomeLabel = AddStyledLine($"{T("screen.combat.outcome", "Outcome")}: {report.Outcome}", true);
        outcomeLabel.AddThemeColorOverride("font_color", Color.FromHtml(outcomeColor));
        outcomeCard.AddChild(outcomeLabel);
        outcomeCard.AddChild(MutedLabel(
            $"{T("screen.combat.rounds_resolved", "Rounds resolved")}: {report.Rounds.Count}  •  " +
            $"{T("screen.combat.world_clock", "World clock")}: {(_game.CombatWorldTimeLocked ? T("label.paused", "Paused") : T("label.active", "Active"))}"));
        var combatSummary = new TypewriterLabel
        {
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        combatSummary.Begin(string.IsNullOrWhiteSpace(report.Summary) ? T("screen.combat.no_summary", "The party returned from the mission.") : report.Summary);
        _prologueLines.Add(combatSummary);
        combatSummary.GuiInput += evt =>
        {
            if (evt.IsPressed())
            {
                FinishActiveTypewriting();
            }
        };
        outcomeCard.AddChild(combatSummary);

        // Rewards
        if (report.RewardGold > 0)
        {
            var rewardStr = $"{T("screen.combat.reward", "Reward")}: {report.RewardGold}{T("unit.g", "g")}";
            if (!string.IsNullOrEmpty(report.RewardItemId) && _game.Data.Items.TryGetValue(report.RewardItemId, out var itemDef))
                rewardStr += $" + {itemDef.DisplayName}";
            outcomeCard.AddChild(AddStyledLine(rewardStr));
        }

        if (report.CaptureAttempted)
        {
            if (report.CaptureSucceeded && !string.IsNullOrEmpty(report.CapturedCharacterId))
            {
                var captured = _game.Roster.Find(report.CapturedCharacterId);
                var name = captured is null ? report.CapturedCharacterId : CharacterPickerName(captured);
                outcomeCard.AddChild(AddStyledLine($"{T("screen.combat.capture_success", "Capture")}: {name} {T("screen.combat.recruited", "recruited!")}"));
            }
            else
            {
                outcomeCard.AddChild(AddStyledLine(T("screen.combat.capture_failed", "Capture failed.")));
            }
        }

        // Party status after battle
        if (report.PartyState.Count > 0)
        {
            var partyCard = CardContainer();
            _content.AddChild(partyCard);
            partyCard.AddChild(SubtitleLabel(T("screen.combat.party_status", "Party After Battle")));
            foreach (var member in report.PartyState)
            {
                string hpStr = member.IsAlive
                    ? $"{member.CurrentHp}/{member.MaxHp} {T("label.hp", "HP")}"
                    : T("screen.combat.fallen", "Fallen");
                partyCard.AddChild(AddStyledLine($"{member.DisplayName}: {hpStr}"));
            }
        }

        // Round-by-round log
        if (report.Rounds.Count > 0)
        {
            var logCard = CardContainer();
            _content.AddChild(logCard);
            logCard.AddChild(SubtitleLabel(T("screen.combat.round_log", "Battle Log")));
            foreach (var round in report.Rounds)
            {
                logCard.AddChild(AddStyledLine($"{T("screen.combat.round", "Round")} {round.RoundNumber}:", true));
                foreach (var action in round.Actions)
                {
                    var icon = action.ActionType == "Defend" ? "\uD83D\uDEE1" : (action.KilledTarget ? "\u2620" : "\u2694");
                    logCard.AddChild(MutedLabel($"  {icon} {action.Description}"));
                }
            }
        }

        // Enemy state
        if (report.EnemyState.Count > 0)
        {
            var enemyCard = CardContainer();
            _content.AddChild(enemyCard);
            enemyCard.AddChild(SubtitleLabel(T("screen.combat.enemies", "Enemies")));
            foreach (var enemy in report.EnemyState)
            {
                string status = enemy.IsAlive ? $"{enemy.CurrentHp}/{enemy.MaxHp} HP" : "Defeated";
                enemyCard.AddChild(AddStyledLine($"{enemy.DisplayName}: {status}"));
            }
        }

        var btn = PrimaryButton(T("common.back", "Back"));
        btn.Pressed += () =>
        {
            _game.EndCombatSession();
            ShowScreen("adventure");
        };
        _content.AddChild(btn);
    }

    private void RenderCombatOutro()
    {
        var summaryCard = CardContainer();
        _content.AddChild(summaryCard);
        var summaryInner = CardContent();
        summaryCard.AddChild(summaryInner);
        summaryInner.AddChild(SubtitleLabel(T("screen.combat.outro", "Mission Summary")));

        if (_game.LastCombatReport is not null)
        {
            var report = _game.LastCombatReport;
            summaryInner.AddChild(AddStyledLine($"Mission: {report.MissionId}", true));
            summaryInner.AddChild(MutedLabel($"Outcome: {report.Outcome}"));
            summaryInner.AddChild(MutedLabel($"Summary: {report.Summary}"));

            if (report.RewardGold > 0)
                summaryInner.AddChild(AddStyledLine($"Gold reward: +{report.RewardGold}{T("unit.g", "g")}", true));

            if (!string.IsNullOrEmpty(report.RewardItemId))
                summaryInner.AddChild(AddStyledLine($"Item received: {report.RewardItemId} x1", true));

            if (report.CaptureAttempted && report.CaptureSucceeded)
                summaryInner.AddChild(AddStyledLine($"Capture: {report.CapturedCharacterId}", true));

            if (report.TurnLog.Count > 0)
            {
                summaryInner.AddChild(MutedLabel("--- Battle Log ---"));
                foreach (var line in report.TurnLog)
                {
                    summaryInner.AddChild(MutedLabel(line));
                }
            }

            if (report.PartyState.Count > 0)
            {
                summaryInner.AddChild(MutedLabel("--- Party Status ---"));
                foreach (var snap in report.PartyState)
                {
                    summaryInner.AddChild(MutedLabel($"{snap.DisplayName}: HP {snap.CurrentHp}/{snap.MaxHp} | SP {snap.CurrentSp}/{snap.MaxSp} | Alive: {snap.IsAlive}"));
                }
            }
        }
        else
        {
            summaryInner.AddChild(MutedLabel(T("screen.combat.outro.no_report", "No combat report available.")));
        }

        var backBtn = SecondaryButton(T("common.back", "Back"), T("tooltip.back_adventure", "Return to the Adventure Guild"));
        backBtn.Pressed += () => ShowScreen("adventure");
        _content.AddChild(backBtn);
    }

    private void RenderMilestones()
    {
        AddTitle(T("screen.milestones", "Milestones"));
        var card = CardContainer();
        _content.AddChild(card);
        foreach (var milestone in _game.Data.Milestones.Values)
        {
            var done = _game.Milestones.Completed.Contains(milestone.Id) ? T("milestone.complete", "Complete") : T("milestone.open", "Open");
            card.AddChild(AddStyledLine($"{milestone.DisplayName}: {done} ({MilestoneTriggerText(milestone)})"));
        }
    }

    private void RenderResearch()
    {
        AddTitle(T("screen.research", "Research"));
        var card = CardContainer();
        _content.AddChild(card);
        foreach (var skill in _game.Data.Skills.Values)
        {
            var unlocked = _game.State.Research.UnlockedSkillIds.Contains(skill.Id);
            _game.State.Ranch.Stockpile.TryGetValue(skill.CostResourceId, out var availableCostResource);
            var canAfford = availableCostResource >= skill.CostAmount;
            var button = unlocked
                ? SecondaryButton($"{T("screen.research.unlocked", "Unlocked")}: {skill.DisplayName}", T("tooltip.skill_unlocked", "This skill is already unlocked"))
                : PrimaryButton($"{T("screen.research.unlock", "Unlock")} {skill.DisplayName} ({skill.CostAmount} {skill.CostResourceId})", T("tooltip.skill_unlock", $"Unlock {skill.DisplayName}: {skill.Description}"));
            button.Disabled = unlocked || !canAfford;
            button.Pressed += () => ExecuteUiAction(() => _game.Research.Unlock(skill.Id), false);
            card.AddChild(button);
            card.AddChild(MutedLabel(skill.Description));
        }

        card.AddChild(AddStyledLine(T("screen.research.passives", "Unlocked passives apply immediately: ranch planning boosts output and field medicine reduces mission fatigue.")));
    }

    private void RenderBond()
    {
        var generation = _game.StateGeneration;
        AddTitle(T("screen.bond", "Bond Events"));
        foreach (var character in _game.Roster.Characters)
        {
            var definition = _game.Roster.DefinitionFor(character);
            var characterCard = CardContainer();
            _content.AddChild(characterCard);

            var header = new HBoxContainer();
            header.AddThemeConstantOverride("separation", 10);
            characterCard.AddChild(header);

            var portrait = BuildCharacterVisual(character, definition);
            portrait.CustomMinimumSize = new Vector2(112, 112);
            header.AddChild(portrait);

            var info = new VBoxContainer();
            info.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            info.AddThemeConstantOverride("separation", 4);
            header.AddChild(info);
            info.AddChild(SubtitleLabel($"{definition.DisplayName} — Bond {character.Bond}"));

            var mentorshipCost = _game.PlayerStaminaCost(PlayerActivityKind.Mentorship);
            var mentorBtn = SecondaryButton($"Mentorship ({mentorshipCost} Stamina)", "Spend daily player stamina: +5 bond, +4 morale, +4 character fatigue.");
            mentorBtn.Disabled = !_game.CanSpendPlayerStamina(PlayerActivityKind.Mentorship);
            mentorBtn.Pressed += () => ExecuteUiAction(() => _game.TryConductMentorship(character.Id, generation), true);
            info.AddChild(mentorBtn);

            var events = _game.Bond.AvailableEvents(character.Id).ToList();
            if (events.Count == 0)
            {
                info.AddChild(MutedLabel("No events available yet. Raise bond to unlock more."));
            }

            foreach (var bondEvent in events)
            {
                var eventCard = CardContainer();
                _content.AddChild(eventCard);

                var eventHeader = AddStyledLine($"✦ {bondEvent.Title}", true);
                eventCard.AddChild(eventHeader);

                var reqLabel = MutedLabel($"Required bond: {bondEvent.RequiredBond}  |  Reward: +{bondEvent.BondReward} bond, +{bondEvent.MoraleReward} morale{(string.IsNullOrWhiteSpace(bondEvent.StockpileRewardId) ? "" : $", +{bondEvent.StockpileRewardAmount} {bondEvent.StockpileRewardId}")}");
                eventCard.AddChild(reqLabel);

                var narrativeBox = new PanelContainer();
                narrativeBox.AddThemeStyleboxOverride("panel", CardStyle(new Color("1a2a4a"), new Color("3a5a8a"), 1, 8));
                narrativeBox.CustomMinimumSize = new Vector2(0, 60);
                eventCard.AddChild(narrativeBox);

                var narrativeLabel = new TypewriterLabel
                {
                    AutowrapMode = TextServer.AutowrapMode.WordSmart,
                    SizeFlagsHorizontal = SizeFlags.ExpandFill,
                    SizeFlagsVertical = SizeFlags.ExpandFill
                };
                narrativeLabel.Begin(bondEvent.Description);
                _prologueLines.Add(narrativeLabel);
                narrativeLabel.GuiInput += evt =>
                {
                    if (evt.IsPressed())
                    {
                        FinishActiveTypewriting();
                    }
                };
                narrativeBox.AddChild(narrativeLabel);

                var bondEventCost = _game.PlayerStaminaCost(PlayerActivityKind.BondEvent);
                var completeBtn = PrimaryButton($"Complete Event ({bondEventCost} Stamina)", "Complete this bond event to earn rewards and progress the story."); 
                completeBtn.Disabled = !_game.CanSpendPlayerStamina(PlayerActivityKind.BondEvent);
                completeBtn.Pressed += () =>
                {
                    if (generation != _game.StateGeneration) return;
                    if (FinishActiveTypewriting())
                    {
                        return;
                    }

                    ExecuteUiAction(() => _game.TryCompleteBondEvent(bondEvent.Id, generation), true);
                };
                eventCard.AddChild(completeBtn);
            }

            // Show completed events count
            var completedCount = _game.Data.BondEvents.Values.Count(e => e.CharacterId == character.Id && _game.State.Bond.CompletedEventIds.Contains(e.Id));
            if (completedCount > 0)
            {
                var completedTitle = MutedLabel($"✓ {completedCount} event{(completedCount == 1 ? "" : "s")} completed");
                completedTitle.AddThemeColorOverride("font_color", new Color("66dd88"));
                characterCard.AddChild(completedTitle);
            }
        }
    }

    private void RenderPets()
    {
        AddTitle(T("screen.pets", "Pets"));
        var card = CardContainer();
        _content.AddChild(card);

        foreach (var pet in _game.Data.Pets.Values)
        {
            var adopted = _game.State.Pets.AdoptedPetIds.Contains(pet.Id);
            if (!adopted)
            {
                var adoptBtn = PrimaryButton($"{T("screen.pets.adopt", "Adopt")} {pet.DisplayName} ({pet.CareCost * 4}{T("unit.g", "g")}, {pet.CareCost}{T("unit.g", "g")}/{T("screen.pets.day", "day")})", T("tooltip.adopt_pet", $"Adopt a {pet.DisplayName}: initial cost {pet.CareCost * 4}g, daily care {pet.CareCost}g"));
                adoptBtn.Pressed += () => ExecuteUiAction(() => _game.Pets.Adopt(pet.Id), false);
                card.AddChild(adoptBtn);
            }
            else
            {
                var entry = _game.State.Pets.Entries.GetValueOrDefault(pet.Id) ?? new PetEntryState();
                var petCard = CardContainer();
                _content.AddChild(petCard);
                petCard.AddChild(SubtitleLabel($"{pet.DisplayName} — {_game.Pets.Status(pet.Id)}"));
                petCard.AddChild(AddStyledLine($"{T("screen.pets.stats", "Hunger")}: {entry.Hunger}% | {T("screen.pets.stats_mood", "Mood")}: {entry.Mood}% | {T("screen.pets.stats_bond", "Bond")}: {entry.Bond}% | {T("screen.pets.stats_training", "Training")}: {entry.Training}%"));

                var actions = FlowRow(6);
                petCard.AddChild(actions);

                var feedCost = _game.PlayerStaminaCost(PlayerActivityKind.PetFeed);
                var feedBtn = PrimaryButton($"{T("screen.pets.feed", "Feed")} (10{T("unit.g", "g")} + {feedCost} STA)", T("tooltip.feed_pet", "Feed the pet: Hunger+20, Mood+5, Bond+2"));
                feedBtn.Disabled = !_game.CanSpendPlayerStamina(PlayerActivityKind.PetFeed);
                feedBtn.Pressed += () => { var result = _game.TryFeedPet(pet.Id); SetStatus(result, result.StartsWith("Fed successfully", StringComparison.Ordinal)); ShowScreen(_currentScreen); };
                AddFlowButton(actions, feedBtn, 150);

                var playCost = _game.PlayerStaminaCost(PlayerActivityKind.PetPlay);
                var playBtn = SecondaryButton($"{T("screen.pets.play", "Play")} (5{T("unit.g", "g")} + {playCost} STA)", T("tooltip.play_pet", "Play with the pet: Mood+15, Bond+3, Hunger-5"));
                playBtn.Disabled = !_game.CanSpendPlayerStamina(PlayerActivityKind.PetPlay);
                playBtn.Pressed += () => { var result = _game.TryPlayWithPet(pet.Id); SetStatus(result, result.StartsWith("Played successfully", StringComparison.Ordinal)); ShowScreen(_currentScreen); };
                AddFlowButton(actions, playBtn, 150);

                var trainCost = _game.PlayerStaminaCost(PlayerActivityKind.PetTraining);
                var trainBtn = SecondaryButton($"{T("screen.pets.train", "Train")} (15{T("unit.g", "g")} + {trainCost} STA)", T("tooltip.train_pet", "Train the pet: Training+10, Bond+1, Hunger-10, Mood-5"));
                trainBtn.Disabled = !_game.CanSpendPlayerStamina(PlayerActivityKind.PetTraining);
                trainBtn.Pressed += () => { var result = _game.TryTrainPet(pet.Id); SetStatus(result, result.StartsWith("Trained successfully", StringComparison.Ordinal)); ShowScreen(_currentScreen); };
                AddFlowButton(actions, trainBtn, 160);

                // Progress bars
                AddMentalBar(petCard, T("screen.pets.stats", "Hunger"), entry.Hunger, 100, "ffaa44");
                AddMentalBar(petCard, T("screen.pets.stats_mood", "Mood"), entry.Mood, 100, "66dd88");
                AddMentalBar(petCard, T("screen.pets.stats_bond", "Bond"), entry.Bond, 100, "ff88cc");
                AddMentalBar(petCard, T("screen.pets.stats_training", "Training"), entry.Training, 100, "88aaff");
            }
        }

        if (!_game.State.Pets.AdoptedPetIds.Any())
        {
            card.AddChild(AddStyledLine(T("screen.pets.adopted_title", "No pets adopted yet. Choose one above!")));
        }
    }

    private void RenderSaveLoad()
    {
        AddTitle(T("screen.saveload", "Save and Load"));

        for (var slot = 1; slot <= 3; slot++)
        {
            var savedState = _game.Save.HasSave(slot) ? _game.Save.LoadMetadata(slot) : null;
            var card = CardContainer();
            _content.AddChild(card);
            var cardInner = CardContent();
            card.AddChild(cardInner);

            var row = new HBoxContainer();
            row.AddThemeConstantOverride("separation", 10);
            row.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            cardInner.AddChild(row);

            var infoCol = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            infoCol.AddThemeConstantOverride("separation", 4);
            row.AddChild(infoCol);

            if (savedState is not null && _game.Save.HasSave(slot))
            {
                infoCol.AddChild(SubtitleLabel($"{T("screen.saveload.slot", "Slot")} {slot}"));

                var dayGoldStr = $"{T("label.day", "Day")} {savedState.Day}  |  {T("screen.saveload.gold", "Gold")} {savedState.Gold}  |  {savedState.CharacterCount} {T("screen.saveload.characters", "characters")}";
                infoCol.AddChild(MutedLabel(dayGoldStr));

                if (savedState.SavedAt.HasValue)
                {
                    var local = savedState.SavedAt.Value.ToLocalTime();
                    var timeStr = local.ToString("yyyy-MM-dd HH:mm");
                    infoCol.AddChild(MutedLabel(timeStr));
                }

                if (savedState.VictoryDay.HasValue)
                {
                    var victoryLabel = MutedLabel(T("game.victory", "Victory!"));
                    victoryLabel.AddThemeColorOverride("font_color", new Color("66dd88"));
                    infoCol.AddChild(victoryLabel);
                }
            }
            else
            {
                infoCol.AddChild(SubtitleLabel($"{T("screen.saveload.slot", "Slot")} {slot}"));
                infoCol.AddChild(MutedLabel(T("screen.saveload.empty_slot", "Empty")));
            }

            var btnCol = FlowRow(6);
            row.AddChild(btnCol);

            var saveBtn = PrimaryButton(T("screen.saveload.save", "Save"));
            var capturedSlot = slot;
            saveBtn.Pressed += () => ExecuteUiAction(() => _game.SaveSlot(capturedSlot), true);
            AddFlowButton(btnCol, saveBtn, 92);

            if (savedState is not null)
            {
                var loadBtn = SecondaryButton(T("screen.saveload.load", "Load"));
                loadBtn.Pressed += () => ExecuteUiAction(() => _game.LoadSlot(capturedSlot), true);
                AddFlowButton(btnCol, loadBtn, 92);

                var deleteBtn = SecondaryButton(T("screen.saveload.delete", "Delete"));
                deleteBtn.Pressed += () =>
                {
                    _game.Save.Delete(capturedSlot);
                    _game.Feedback.PlayConfirm();
                    ShowScreen("saveload");
                };
                AddFlowButton(btnCol, deleteBtn, 92);
            }
        }

        var bottomCard = CardContainer();
        _content.AddChild(bottomCard);
        var bottomInner = CardContent();
        bottomCard.AddChild(bottomInner);
        var bottomRow = FlowRow(10);
        bottomInner.AddChild(bottomRow);

        var newGameBtn = SecondaryButton(T("screen.saveload.new_game", "New Game"));
        newGameBtn.Pressed += () => ExecuteUiAction(_game.NewGame, true, "character_creation");
        AddFlowButton(bottomRow, newGameBtn, 132);

        var titleBtn = SecondaryButton(T("screen.saveload.back", "Back To Main Menu"));
        titleBtn.Pressed += () =>
        {
            var error = GetTree().ChangeSceneToFile("res://scenes/MainMenu.tscn");
            if (error != Error.Ok)
            {
                GD.PushError($"Failed to return to MainMenu scene: {error}");
            }
        };
        AddFlowButton(bottomRow, titleBtn, 170);
    }

    private void RenderSettings()
    {
        AddTitle(T("screen.settings", "Settings"));
        var settings = _game.State.Settings;

        var platformCard = CardContainer();
        _content.AddChild(platformCard);
        var platform = CardContent();
        platformCard.AddChild(platform);
        platform.AddChild(SubtitleLabel(T("screen.settings.platform", "Platform & Recommended Setup")));
        platform.AddChild(MutedLabel($"{OS.GetName()} • {_game.RuntimeSettings.CurrentRenderingMethod} • {GetViewportRect().Size.X:0}×{GetViewportRect().Size.Y:0}"));
        platform.AddChild(MutedLabel(_game.RuntimeSettings.IsMobilePlatform
            ? T("screen.settings.mobile_note", "Mobile mode favors lower render cost, touch controls and readable UI. Native Android/iOS builds are preferred over browser play.")
            : T("screen.settings.desktop_note", "Desktop mode supports window/fullscreen, mouse/controller input and higher quality settings.")));
        var recommended = PrimaryButton(T("screen.settings.recommended", "Apply Recommended Settings"),
            T("tooltip.settings.recommended", "Choose a safe balanced preset for the current platform."));
        recommended.Pressed += () =>
        {
            _game.ApplyRecommendedSettings();
            ShowScreen("settings");
        };
        platform.AddChild(recommended);

        var graphicsCard = CardContainer();
        _content.AddChild(graphicsCard);
        var graphics = CardContent();
        graphicsCard.AddChild(graphics);
        graphics.AddChild(SubtitleLabel(T("screen.settings.graphics", "Graphics & Performance")));

        var qualityRow = FlowRow(8);
        graphics.AddChild(qualityRow);
        qualityRow.AddChild(AddStyledLine(T("screen.settings.quality", "Quality Preset"), true));
        var qualityPicker = StyledPicker(180);
        var qualities = new[] { "Low", "Medium", "High", "Ultra", "Custom" };
        var qualitySelected = 1;
        for (var q = 0; q < qualities.Length; q++)
        {
            qualityPicker.AddItem(qualities[q]);
            if (string.Equals(qualities[q], settings.GraphicsQuality, StringComparison.OrdinalIgnoreCase))
            {
                qualitySelected = q;
            }
        }
        qualityPicker.Selected = qualitySelected;
        qualityPicker.ItemSelected += idx =>
        {
            var value = qualities[(int)idx];
            if (value != "Custom")
            {
                ExecuteUiAction(() => _game.SetGraphicsQuality(value), false);
            }
        };
        qualityRow.AddChild(qualityPicker);

        var renderScaleRow = FlowRow(8);
        graphics.AddChild(renderScaleRow);
        renderScaleRow.AddChild(AddStyledLine($"{T("screen.settings.render_scale", "3D Render Scale")}: {settings.RenderScale * 100f:0}%", true));
        var renderScale = new HSlider
        {
            MinValue = 0.50,
            MaxValue = 1.00,
            Step = 0.05,
            Value = settings.RenderScale,
            CustomMinimumSize = new Vector2(240, 0)
        };
        renderScale.TooltipText = T("tooltip.settings.render_scale", "Lower this first if the 3D world runs slowly. UI remains at full resolution.");
        renderScale.ValueChanged += value => ExecuteUiAction(() => _game.SetRenderScale((float)value), false);
        renderScaleRow.AddChild(renderScale);

        var fpsRow = FlowRow(8);
        graphics.AddChild(fpsRow);
        fpsRow.AddChild(AddStyledLine(T("screen.settings.fps", "Frame Rate Limit"), true));
        var fpsPicker = StyledPicker(160);
        var fpsValues = new[] { 30, 45, 60, 90, 120, 144, 0 };
        var fpsSelected = 2;
        for (var i = 0; i < fpsValues.Length; i++)
        {
            fpsPicker.AddItem(fpsValues[i] == 0 ? T("label.unlimited", "Unlimited") : $"{fpsValues[i]} FPS");
            fpsPicker.SetItemMetadata(i, fpsValues[i]);
            if (fpsValues[i] == settings.FrameRateLimit) fpsSelected = i;
        }
        fpsPicker.Selected = fpsSelected;
        fpsPicker.ItemSelected += idx => ExecuteUiAction(
            () => _game.SetFrameRateLimit((int)fpsPicker.GetItemMetadata((int)idx).AsInt64()), false);
        fpsRow.AddChild(fpsPicker);

        var shadows = PrimaryButton($"{T("screen.settings.shadows", "Dynamic Shadows")}: {(settings.ShadowsEnabled ? T("label.on", "On") : T("label.off", "Off"))}");
        shadows.TooltipText = T("tooltip.settings.shadows", "Disable shadows for a substantial GPU saving on low-end or mobile hardware.");
        shadows.Pressed += () => _game.SetShadowsEnabled(!settings.ShadowsEnabled);
        graphics.AddChild(shadows);

        var atmosphere = PrimaryButton($"{T("screen.settings.atmosphere", "Atmosphere / Post Processing")}: {(settings.AtmosphereEffectsEnabled ? T("label.on", "On") : T("label.off", "Off"))}");
        atmosphere.TooltipText = T("tooltip.settings.atmosphere", "Controls situation-aware fog, glow and color adjustments. Low quality disables this automatically.");
        atmosphere.Pressed += () => _game.SetAtmosphereEffectsEnabled(!settings.AtmosphereEffectsEnabled);
        graphics.AddChild(atmosphere);

        var weatherFx = PrimaryButton($"{T("screen.settings.weather_fx", "Weather Visual Effects")}: {(settings.WeatherEffectsEnabled ? T("label.on", "On") : T("label.off", "Off"))}");
        weatherFx.TooltipText = T("tooltip.settings.weather_fx", "Keep weather gameplay while optionally reducing its visual atmosphere cost.");
        weatherFx.Pressed += () => _game.SetWeatherEffectsEnabled(!settings.WeatherEffectsEnabled);
        graphics.AddChild(weatherFx);

        var advancedLighting = PrimaryButton($"{T("screen.settings.advanced_lighting", "Forward+ Advanced Lighting")}: {(settings.AdvancedLightingEnabled ? T("label.on", "On") : T("label.off", "Off"))}");
        advancedLighting.TooltipText = _game.RuntimeSettings.IsForwardPlus
            ? T("tooltip.settings.advanced_lighting", "Forward+ only: SSAO/SSIL/SSR and volumetric atmosphere where the quality preset allows it.")
            : T("tooltip.settings.advanced_lighting_unavailable", "The current renderer does not support the full Forward+ advanced-lighting set.");
        advancedLighting.Disabled = !_game.RuntimeSettings.IsForwardPlus;
        advancedLighting.Pressed += () => _game.SetAdvancedLightingEnabled(!settings.AdvancedLightingEnabled);
        graphics.AddChild(advancedLighting);

        var particles = PrimaryButton($"{T("screen.settings.world_particles", "World Particles")}: {(settings.WorldParticlesEnabled ? T("label.on", "On") : T("label.off", "Off"))}");
        particles.TooltipText = T("tooltip.settings.world_particles", "Rain, snow, wind debris and seasonal leaf particles. Density follows the quality preset.");
        particles.Pressed += () => _game.SetWorldParticlesEnabled(!settings.WorldParticlesEnabled);
        graphics.AddChild(particles);

        var detailRow = FlowRow(8);
        graphics.AddChild(detailRow);
        detailRow.AddChild(AddStyledLine($"{T("screen.settings.world_detail", "World Detail")}: {settings.WorldDetailScale * 100f:0}%", true));
        var detailScale = new HSlider
        {
            MinValue = 0.35,
            MaxValue = 1.25,
            Step = 0.05,
            Value = settings.WorldDetailScale,
            CustomMinimumSize = new Vector2(240, 0)
        };
        detailScale.TooltipText = T("tooltip.settings.world_detail", "Scales decorative density independently from gameplay objects. Lower values keep paths, stations and interactions intact.");
        detailScale.ValueChanged += value => ExecuteUiAction(() => _game.SetWorldDetailScale((float)value), false);
        detailRow.AddChild(detailScale);

        var vsync = PrimaryButton($"{T("screen.settings.vsync", "VSync")}: {(settings.VSyncEnabled ? T("label.on", "On") : T("label.off", "Off"))}");
        vsync.Pressed += () => _game.SetVSyncEnabled(!settings.VSyncEnabled);
        graphics.AddChild(vsync);

        if (_game.RuntimeSettings.IsDesktopPlatform && !_game.RuntimeSettings.IsWebPlatform)
        {
            var windowRow = FlowRow(8);
            graphics.AddChild(windowRow);
            windowRow.AddChild(AddStyledLine(T("screen.settings.window_size", "Window Size / Aspect"), true));
            var windowPicker = StyledPicker(260);
            var windowSizes = new (string Label, int Width, int Height)[]
            {
                ("1280×720  (16:9)", 1280, 720),
                ("1600×900  (16:9)", 1600, 900),
                ("1920×1080  (16:9 Default)", 1920, 1080),
                ("1920×1200  (16:10)", 1920, 1200),
                ("2560×1440  (16:9)", 2560, 1440),
                ("2560×1080  (21:9)", 2560, 1080),
                ("3440×1440  (21:9)", 3440, 1440),
                ("3840×1080  (32:9)", 3840, 1080),
                ("5120×1440  (32:9)", 5120, 1440)
            };
            var selectedWindow = 2;
            for (var w = 0; w < windowSizes.Length; w++)
            {
                windowPicker.AddItem(windowSizes[w].Label);
                if (windowSizes[w].Width == settings.WindowWidth && windowSizes[w].Height == settings.WindowHeight)
                {
                    selectedWindow = w;
                }
            }
            windowPicker.Selected = selectedWindow;
            windowPicker.Disabled = settings.Fullscreen;
            windowPicker.TooltipText = T("tooltip.settings.window_size", "1920×1080 is the design resolution. Other aspect ratios expand the visible world; HUD remains in a centered safe region.");
            windowPicker.ItemSelected += idx =>
            {
                var size = windowSizes[(int)idx];
                ExecuteUiAction(() => _game.SetWindowSize(size.Width, size.Height), false);
            };
            windowRow.AddChild(windowPicker);

            var fullscreen = PrimaryButton($"{T("screen.settings.fullscreen", "Exclusive Fullscreen")}: {(settings.Fullscreen ? T("label.on", "On") : T("label.off", "Off"))}");
            fullscreen.TooltipText = T("tooltip.settings.fullscreen", "Fullscreen uses the monitor's native resolution/aspect ratio; the game does not force the monitor to another mode.");
            fullscreen.Pressed += () => _game.SetFullscreen(!settings.Fullscreen);
            graphics.AddChild(fullscreen);
        }

        var audioCard = CardContainer();
        _content.AddChild(audioCard);
        var audio = CardContent();
        audioCard.AddChild(audio);
        audio.AddChild(SubtitleLabel(T("screen.settings.audio", "Audio & Feedback")));

        var audioToggle = PrimaryButton($"{T("screen.settings.audio_feedback", "Audio")}: {(_game.Feedback.AudioEnabled ? T("label.on", "On") : T("label.off", "Off"))}");
        audioToggle.Pressed += () =>
        {
            _game.ToggleAudioFeedback();
            if (_game.Feedback.AudioEnabled) _game.Feedback.PlayConfirm();
        };
        audio.AddChild(audioToggle);

        AddVolumeSlider(audio, T("screen.settings.master_volume", "Master"), settings.MasterVolume, _game.SetMasterVolume);
        AddVolumeSlider(audio, T("screen.settings.music_volume", "Music"), settings.MusicVolume, _game.SetMusicVolume);
        AddVolumeSlider(audio, T("screen.settings.sfx_volume", "SFX"), settings.SfxVolume, _game.SetSfxVolume);
        AddVolumeSlider(audio, T("screen.settings.ui_volume", "UI"), settings.UiVolume, _game.SetUiVolume);

        var muteUnfocused = PrimaryButton($"{T("screen.settings.mute_unfocused", "Mute When Unfocused")}: {(settings.MuteWhenUnfocused ? T("label.on", "On") : T("label.off", "Off"))}");
        muteUnfocused.Pressed += () => _game.SetMuteWhenUnfocused(!settings.MuteWhenUnfocused);
        audio.AddChild(muteUnfocused);

        var hapticsToggle = PrimaryButton($"{T("screen.settings.handheld_vibration", "Handheld Vibration")}: {(_game.Feedback.HapticsEnabled ? T("label.on", "On") : T("label.off", "Off"))}");
        hapticsToggle.Disabled = !_game.Feedback.SupportsHaptics;
        hapticsToggle.Pressed += () =>
        {
            _game.ToggleHapticsFeedback();
            _game.Feedback.PulseHaptics(40, 0.45f);
        };
        audio.AddChild(hapticsToggle);

        var previewFeedback = SecondaryButton(T("screen.settings.preview_confirm", "Preview Confirm Feedback"));
        previewFeedback.Pressed += () => _game.Feedback.PlayConfirm();
        audio.AddChild(previewFeedback);

        var interfaceCard = CardContainer();
        _content.AddChild(interfaceCard);
        var ui = CardContent();
        interfaceCard.AddChild(ui);
        ui.AddChild(SubtitleLabel(T("screen.settings.interface", "Interface & Accessibility")));

        var themeRow = FlowRow(8);
        ui.AddChild(themeRow);
        themeRow.AddChild(AddStyledLine(T("screen.settings.color_theme", "Color Theme"), true));
        var themePicker = StyledPicker(220);
        var selectedThemeIndex = 0;
        var themeIndex = 0;
        foreach (var theme in UiThemeCatalog.All)
        {
            themePicker.AddItem(theme.DisplayName);
            themePicker.SetItemMetadata(themeIndex, theme.Id);
            if (string.Equals(theme.Id, settings.ThemeId, StringComparison.OrdinalIgnoreCase)) selectedThemeIndex = themeIndex;
            themeIndex++;
        }
        themePicker.Selected = selectedThemeIndex;
        themePicker.ItemSelected += selected =>
        {
            var selectedId = themePicker.GetItemMetadata((int)selected).AsString();
            ExecuteUiAction(() => _game.SetTheme(selectedId), false);
        };
        themeRow.AddChild(themePicker);

        var uiScaleRow = FlowRow(8);
        ui.AddChild(uiScaleRow);
        uiScaleRow.AddChild(AddStyledLine($"{T("screen.settings.ui_scale", "UI Scale")}: {settings.UiScale:0.00}x", true));
        var uiScale = new HSlider
        {
            MinValue = 0.80,
            MaxValue = 1.50,
            Step = 0.05,
            Value = settings.UiScale,
            CustomMinimumSize = new Vector2(240, 0)
        };
        uiScale.ValueChanged += value => ExecuteUiAction(() => _game.SetUiScale((float)value), false);
        uiScaleRow.AddChild(uiScale);

        var reducedMotion = PrimaryButton($"{T("screen.settings.reduced_motion", "Reduced Motion")}: {(settings.ReducedMotion ? T("label.on", "On") : T("label.off", "Off"))}");
        reducedMotion.Pressed += () => _game.SetReducedMotion(!settings.ReducedMotion);
        ui.AddChild(reducedMotion);

        var localeRow = FlowRow(8);
        ui.AddChild(localeRow);
        localeRow.AddChild(AddStyledLine(T("screen.settings.language", "Language"), true));
        var localePicker = StyledPicker(180);
        var selectedLocaleIndex = 0;
        for (var localeIdx = 0; localeIdx < AvailableLocales.Length; localeIdx++)
        {
            var lc = AvailableLocales[localeIdx];
            localePicker.AddItem(LocaleDisplayName(lc));
            localePicker.SetItemMetadata(localeIdx, lc);
            if (string.Equals(lc, settings.Locale, StringComparison.OrdinalIgnoreCase)) selectedLocaleIndex = localeIdx;
        }
        localePicker.Selected = selectedLocaleIndex;
        localePicker.ItemSelected += selected =>
        {
            var selectedLocale = localePicker.GetItemMetadata((int)selected).AsString();
            ExecuteUiAction(() => _game.SetLocale(selectedLocale), false);
        };
        localeRow.AddChild(localePicker);

        var controlsCard = CardContainer();
        _content.AddChild(controlsCard);
        var controls = CardContent();
        controlsCard.AddChild(controls);
        controls.AddChild(SubtitleLabel(T("screen.settings.controls", "Camera & Controls")));

        var sensitivityRow = FlowRow(8);
        controls.AddChild(sensitivityRow);
        sensitivityRow.AddChild(AddStyledLine($"{T("screen.settings.camera_sensitivity", "Camera Sensitivity")}: {settings.CameraSensitivity:0.00}x", true));
        var sensitivity = new HSlider
        {
            MinValue = 0.35,
            MaxValue = 2.50,
            Step = 0.05,
            Value = settings.CameraSensitivity,
            CustomMinimumSize = new Vector2(240, 0)
        };
        sensitivity.ValueChanged += value => ExecuteUiAction(() => _game.SetCameraSensitivity((float)value), false);
        sensitivityRow.AddChild(sensitivity);

        var fovRow = FlowRow(8);
        controls.AddChild(fovRow);
        fovRow.AddChild(AddStyledLine($"{T("screen.settings.camera_fov", "Camera FOV")}: {settings.CameraFov:0}°", true));
        var fov = new HSlider
        {
            MinValue = 55,
            MaxValue = 95,
            Step = 1,
            Value = settings.CameraFov,
            CustomMinimumSize = new Vector2(240, 0)
        };
        fov.ValueChanged += value => ExecuteUiAction(() => _game.SetCameraFov((float)value), false);
        fovRow.AddChild(fov);

        var invertY = PrimaryButton($"{T("screen.settings.invert_y", "Invert Camera Y")}: {(settings.InvertCameraY ? T("label.on", "On") : T("label.off", "Off"))}");
        invertY.Pressed += () => _game.SetInvertCameraY(!settings.InvertCameraY);
        controls.AddChild(invertY);

        var touch = PrimaryButton($"{T("screen.settings.touch_controls", "On-screen Touch Controls")}: {((_game.RuntimeSettings.ShouldShowTouchControls) ? T("label.on", "On") : T("label.off", "Off"))}");
        touch.TooltipText = _game.RuntimeSettings.IsMobilePlatform
            ? T("tooltip.settings.touch_mobile", "Touch controls are always available on mobile; this switch also allows testing them on desktop.")
            : T("tooltip.settings.touch_desktop", "Enable the mobile touch overlay for testing or touch-screen PCs.");
        touch.Pressed += () => _game.SetTouchControlsEnabled(!settings.TouchControlsEnabled);
        controls.AddChild(touch);

        var touchScaleRow = FlowRow(8);
        controls.AddChild(touchScaleRow);
        touchScaleRow.AddChild(AddStyledLine($"{T("screen.settings.touch_scale", "Touch Control Size")}: {settings.TouchControlScale:0.00}x", true));
        var touchScale = new HSlider
        {
            MinValue = 0.75,
            MaxValue = 1.50,
            Step = 0.05,
            Value = settings.TouchControlScale,
            CustomMinimumSize = new Vector2(240, 0)
        };
        touchScale.ValueChanged += value => ExecuteUiAction(() => _game.SetTouchControlScale((float)value), false);
        touchScaleRow.AddChild(touchScale);

        var gameplayCard = CardContainer();
        _content.AddChild(gameplayCard);
        var gameplay = CardContent();
        gameplayCard.AddChild(gameplay);
        gameplay.AddChild(SubtitleLabel(T("screen.settings.gameplay", "Gameplay Assistance & Saving")));

        var autosave = PrimaryButton($"{T("screen.settings.autosave", "Autosave")}: {(settings.AutosaveEnabled ? T("label.on", "On") : T("label.off", "Off"))}");
        autosave.TooltipText = T("tooltip.settings.autosave", "Autosave to slot 0 after each completed day and when a mobile app is suspended.");
        autosave.Pressed += () => _game.SetAutosaveEnabled(!settings.AutosaveEnabled);
        gameplay.AddChild(autosave);

        var tutorialToggle = PrimaryButton($"{T("screen.settings.tutorial_hints", "World Tutorial Hints")}: {(settings.TutorialHintsEnabled ? T("label.on", "On") : T("label.off", "Off"))}");
        tutorialToggle.Pressed += () => _game.SetTutorialHintsEnabled(!settings.TutorialHintsEnabled);
        gameplay.AddChild(tutorialToggle);

        var resetTutorial = SecondaryButton(T("screen.settings.restart_tutorial", "Restart Basic Ranch Tutorial"));
        resetTutorial.Pressed += () =>
        {
            _game.ResetTutorialProgress();
            _game.SetTutorialHintsEnabled(true);
            ShowScreen("settings");
        };
        gameplay.AddChild(resetTutorial);
        gameplay.AddChild(MutedLabel(T("screen.settings.world_help", "In the 3D world, press F1 at any time for controls and the basic gameplay loop.")));
    }

    private void AddVolumeSlider(VBoxContainer parent, string label, float value, Func<float, bool> setter)
    {
        var row = FlowRow(8);
        parent.AddChild(row);
        row.AddChild(AddStyledLine($"{label}: {value * 100f:0}%", true));
        var slider = new HSlider
        {
            MinValue = 0,
            MaxValue = 1,
            Step = 0.05,
            Value = value,
            CustomMinimumSize = new Vector2(240, 0)
        };
        slider.ValueChanged += changed => ExecuteUiAction(() => setter((float)changed), false);
        row.AddChild(slider);
    }

    // === Training state tracking ===
    private int _trainingCharIdx;
    private TrainingCategory _trainingCategory;
    private int _visitCharIdx;

    private void RenderTraining()
    {
        AddTitle(T("screen.training", "Training Room"));
        var chars = _game.Roster.Characters;
        if (!chars.Any())
        {
            var card = CardContainer();
            _content.AddChild(card);
            card.AddChild(AddStyledLine(T("screen.training.no_characters", "No characters on the ranch.")));
            return;
        }

        var slotsLeft = 2 - _game.State.Calendar.TrainedToday;
        _content.AddChild(MutedLabel(slotsLeft <= 0
            ? T("screen.training.no_slots", "Training limit reached for today. End the day to restore your 2 daily training slots.")
            : T("screen.training.slots_left", "Training slots left today: ") + slotsLeft));

        _trainingCharIdx = Math.Clamp(_trainingCharIdx, 0, chars.Count - 1);
        var character = chars[_trainingCharIdx];
        var mental = character.Mature;

        // === Resident / companion selector row ===
        var selectorRow = FlowRow(8);
        _content.AddChild(selectorRow);

        selectorRow.AddChild(MutedLabel($"{T("label.character", "Character")}:"));
        var charPicker = StyledPicker(240);
        charPicker.TooltipText = T("tooltip.training_char", "Select a character to train");
        for (var i = 0; i < chars.Count; i++)
        {
            charPicker.AddItem(CharacterPickerName(chars[i]));
            if (i == _trainingCharIdx) charPicker.Selected = i;
        }
        charPicker.ItemSelected += idx => { _trainingCharIdx = (int)idx; _game.NotifyStateChanged(); };
        selectorRow.AddChild(charPicker);

        // === Character stats card ===
        var statsCard = CardContainer();
        _content.AddChild(statsCard);
        statsCard.AddChild(AddStyledLine($"{CharacterPickerName(character)} - {T("label.energy", "Energy")} {character.Energy}  {T("label.fatigue", "Fatigue")} {character.Fatigue}  {T("label.bond", "Bond")} {character.Bond}", true));
        statsCard.AddChild(AddStyledLine($"{T("label.fall_state", "Fall State")}: {mental.FallState}  {T("label.resistance", "Resistance")} {mental.Resistance}  {T("label.lust", "Lust")} {mental.Lust}", true));
        statsCard.AddChild(AddStyledLine($"{T("label.affection", "Affection")} {mental.Favorability}  {T("label.obedience", "Obedience")} {mental.Obedience}  {T("label.submission", "Submission")} {mental.Submission}"));

        // === Category tabs ===
        var categories = (TrainingCategory[])Enum.GetValues(typeof(TrainingCategory));
        var catRow = FlowRow(4);
        catRow.CustomMinimumSize = new Vector2(0, 36);
        _content.AddChild(catRow);

        foreach (var cat in categories)
        {
            var isActive = cat == _trainingCategory;
            var btn = isActive ? PrimaryButton(T($"cat.{cat}", cat.ToString())) : SecondaryButton(T($"cat.{cat}", cat.ToString()));
            btn.Pressed += () => { _trainingCategory = cat; _game.NotifyStateChanged(); };
            catRow.AddChild(btn);
        }

        // === Actions list ===
        var actions = TrainingActionCatalog.ByCategory(_trainingCategory);
        if (!actions.Any())
        {
            var emptyCard = CardContainer();
            _content.AddChild(emptyCard);
            emptyCard.AddChild(MutedLabel(T("screen.training.no_actions", "No actions in this category.")));
            return;
        }

        foreach (var action in actions)
        {
            var card = CardContainer();
            _content.AddChild(card);

            var canAfford = character.Energy >= action.EnergyCost && character.Bond >= action.MinBond;
            var reason = !canAfford
                ? (character.Energy < action.EnergyCost
                    ? T("screen.training.low_energy", "Not enough energy.")
                    : T("screen.training.low_bond", $"Requires bond {action.MinBond}."))
                : "";
            if (reason.Length == 0 && action.RequiresConsent && !EnhancedTrainingService.HasConsent(character))
                reason = T("screen.training.no_consent", "Character does not consent yet.");
            if (reason.Length == 0 && !string.IsNullOrEmpty(action.ToolRequired))
            {
                var hasTool = _game.State.Inventory.Items.ContainsKey(action.ToolRequired)
                    || (EnhancedTrainingService.ResolveToolId(action.ToolRequired) != action.ToolRequired
                        && _game.State.Inventory.Items.ContainsKey(EnhancedTrainingService.ResolveToolId(action.ToolRequired)));
                if (!hasTool)
                    reason = T("screen.training.no_tool", $"Requires tool: {action.ToolRequired}.");
            }

            var sensations = string.Join(", ", action.SensationTypes.Select(s => s.ToString()));
            var sensStr = sensations.Length > 0 ? $" [{sensations}]" : "";
            var toolStr = string.IsNullOrEmpty(action.ToolRequired) ? "" : $" ({T("label.tool", "tool")}: {action.ToolRequired})";

            var header = AddStyledLine($"{action.DisplayName}{toolStr}", true);
            card.AddChild(header);

            var details = AddStyledLine($"{T("label.pleasure", "Pleasure")} {action.BasePleasure}  {T("label.pain", "Pain")} {action.BasePain}  {T("label.energy", "E")} {action.EnergyCost}  {T("label.fatigue", "F")} {action.FatigueCost}  {T("label.bond", "B")} {action.MinBond}{sensStr}");
            card.AddChild(details);

            if (action.MentalEffect != 0)
            {
                var mentalStr = action.MentalEffect > 0
                    ? $"+{action.MentalEffect} {T("label.morale", "Morale")}"
                    : $"{action.MentalEffect} {T("label.mental", "Mental")}";
                card.AddChild(AddStyledLine(mentalStr));
            }

            if (reason.Length > 0)
            {
                card.AddChild(MutedLabel(reason));
            }

            var perform = PrimaryButton(T("screen.training.perform", "Perform"), reason.Length > 0 ? reason : T("tooltip.perform_training", "Execute this training action on the selected character"));
            perform.Disabled = !canAfford;
            perform.Pressed += () =>
            {
                var report = _game.PerformTraining(character.Id, action.Id);
                if (report.Success)
                {
                    _game.Feedback.PlayConfirm();
                    ShowResultPopup(report);
                }
                else
                {
                    _game.Feedback.PlayError();
                }
                ShowScreen(_currentScreen);
            };
            card.AddChild(perform);
        }
    }

    private void ShowTrainingResult(string characterId, string skillName, int oldValue, int newValue, int fatigue, int morale)
    {
        var popup = new AcceptDialog
        {
            Title = T("screen.roster.train_result", "Training Complete"),
            MinSize = new Vector2I(350, 160)
        };
        var vbox = new VBoxContainer();
        vbox.AddChild(new Label { Text = $"{characterId}: {skillName} {oldValue} → {newValue}" });
        vbox.AddChild(new Label { Text = T("screen.roster.train_fatigue", "Fatigue +{0}").Replace("{0}", Math.Max(1, (int)(12 / _game.Talents.TrainingEfficiency(characterId))).ToString()) });
        vbox.AddChild(new Label { Text = T("screen.roster.train_morale", "Morale +1") });
        vbox.AddChild(new Label { Text = "" });
        vbox.AddChild(new Label { Text = $"{T("label.fatigue", "Fatigue")}: {fatigue}  |  {T("label.morale", "Morale")}: {morale}" });
        popup.AddChild(vbox);
        GetTree().CurrentScene.AddChild(popup);
        popup.PopupCentered();
        popup.Confirmed += () => { if (IsInstanceValid(popup)) popup.QueueFree(); };
        popup.CloseRequested += () => { if (IsInstanceValid(popup)) popup.QueueFree(); };
    }

    private void ShowResultPopup(TrainingReport report)
    {
        var popup = new AcceptDialog
        {
            Title = T("screen.training.result", "Training Result"),
            DialogText = report.Summary,
            MinSize = new Vector2I(400, 200)
        };
        var effects = report.Effects;
        if (effects is not null)
        {
            var vbox = new VBoxContainer();
            void AddEffectLine(string label, int value)
            {
                if (value != 0)
                    vbox.AddChild(new Label { Text = $"  {label}: {(value > 0 ? "+" : "")}{value}" });
            }
            AddEffectLine(T("label.resistance", "Resistance"), effects.ResistanceDelta);
            AddEffectLine(T("label.dignity", "Dignity"), effects.DignityDelta);
            AddEffectLine(T("label.aversion", "Aversion"), effects.AversionDelta);
            AddEffectLine(T("label.reason", "Reason"), effects.ReasonDelta);
            AddEffectLine(T("label.mental_strength", "Mental Strength"), effects.MentalStrengthDelta);
            AddEffectLine(T("label.favorability", "Favorability"), effects.FavorabilityDelta);
            AddEffectLine(T("label.obedience", "Obedience"), effects.ObedienceDelta);
            AddEffectLine(T("label.lust", "Lust"), effects.LustDelta);
            AddEffectLine(T("label.submission", "Submission"), effects.SubmissionDelta);
            AddEffectLine(T("label.pain", "Pain"), effects.PainDelta);
            AddEffectLine(T("label.fear", "Fear"), effects.FearDelta);
            AddEffectLine(T("label.disgust", "Disgust"), effects.DisgustDelta);
            AddEffectLine(T("label.despair", "Despair"), effects.DespairDelta);

            vbox.AddChild(new Label { Text = $"{T("label.fall_state", "Fall State")}: {report.NewFallState}" });
            vbox.AddChild(new Label { Text = "---" });
            vbox.AddChild(new Label { Text = T("screen.training.history", "Recent Training History") });
            var history = _game.State.Mature.TrainingHistory;
            var recent = history.Skip(Math.Max(0, history.Count - 5)).ToList();
            foreach (var record in recent)
            {
                vbox.AddChild(new Label { Text = $"  {record.Summary} ({T("label.day", "Day")} {record.Day})" });
            }

            popup.AddChild(vbox);
        }
        GetTree().CurrentScene.AddChild(popup);
        popup.PopupCentered();
        popup.Confirmed += () => { if (IsInstanceValid(popup)) popup.QueueFree(); };
        popup.CloseRequested += () => { if (IsInstanceValid(popup)) popup.QueueFree(); };
    }

    private void RenderVisit()
    {
        AddTitle(T("screen.visit", "Personal Time"));

        var freeTimeCard = CardContainer();
        _content.AddChild(freeTimeCard);
        var freeTimeInner = CardContent();
        freeTimeCard.AddChild(freeTimeInner);
        freeTimeInner.AddChild(SubtitleLabel(T("screen.visit.free_time", "Free Time")));
        freeTimeInner.AddChild(MutedLabel(T("screen.visit.free_time_original",
            "Original-era structure: free time can be spent resting in your room or spending time with an adopted pet. The remake also exposes resident conversations/care below through the existing Visit/Bond systems.")));

        var freeTimeActions = FlowRow(8);
        freeTimeInner.AddChild(freeTimeActions);

        var playerCharacter = _game.Roster.Characters.FirstOrDefault();
        if (playerCharacter is not null)
        {
            var restSelf = SecondaryButton(
                T("screen.visit.rest_self", "Rest In Your Room"),
                T("tooltip.visit.rest_self", "Use the existing rest recovery on the player character without advancing the world phase."));
            restSelf.Pressed += () =>
            {
                var line = _game.Visit.CareRest(playerCharacter.Id);
                SetStatus(line, false);
                _game.NotifyStateChanged();
            };
            AddFlowButton(freeTimeActions, restSelf, 170);
        }

        foreach (var petId in _game.State.Pets.AdoptedPetIds)
        {
            if (!_game.Data.Pets.TryGetValue(petId, out var petDef))
            {
                continue;
            }

            var capturedPetId = petId;
            var playPet = SecondaryButton(
                $"{T("screen.visit.spend_pet", "Spend Time With")} {petDef.DisplayName}",
                T("tooltip.visit.spend_pet", "Uses the existing pet Play action: improves mood/bond and costs its normal small fee."));
            playPet.Pressed += () =>
            {
                var line = _game.TryPlayWithPet(capturedPetId);
                SetStatus(line, line.StartsWith("Played successfully", StringComparison.Ordinal));
            };
            AddFlowButton(freeTimeActions, playPet, 220);
        }

        if (!_game.State.Pets.AdoptedPetIds.Any())
        {
            freeTimeInner.AddChild(MutedLabel(T("screen.visit.no_pet_free_time", "No adopted pet is available for free-time play yet.")));
        }

        var chars = _game.Roster.Characters;
        if (!chars.Any())
        {
            var card = CardContainer();
            _content.AddChild(card);
            card.AddChild(AddStyledLine(T("screen.visit.no_characters", "No characters on the ranch.")));
            return;
        }

        _visitCharIdx = Math.Clamp(_visitCharIdx, 0, chars.Count - 1);
        var character = chars[_visitCharIdx];
        var mental = character.Mature;

        // === Character selector row ===
        var selectorRow = FlowRow(8);
        _content.AddChild(selectorRow);
        selectorRow.AddChild(MutedLabel($"{T("label.character", "Character")}:"));
        var charPicker = StyledPicker(240);
        charPicker.TooltipText = T("tooltip.visit_char", "Select a resident or companion to spend personal time with");
        for (var i = 0; i < chars.Count; i++)
        {
            charPicker.AddItem(CharacterPickerName(chars[i]));
            if (i == _visitCharIdx) charPicker.Selected = i;
        }
        charPicker.ItemSelected += idx => { _visitCharIdx = (int)idx; _game.NotifyStateChanged(); };
        selectorRow.AddChild(charPicker);

        // === Character stats card ===
        var statsCard = CardContainer();
        _content.AddChild(statsCard);
        statsCard.AddChild(AddStyledLine($"{CharacterPickerName(character)} - {T("label.energy", "Energy")} {character.Energy}  {T("label.fatigue", "Fatigue")} {character.Fatigue}  {T("label.bond", "Bond")} {character.Bond}", true));
        statsCard.AddChild(AddStyledLine($"{T("label.morale", "Morale")} {character.Morale}  {T("label.hp", "HP")} {character.Hp}  {T("screen.visit.fall", "Fall State")}: {mental.FallState}", true));
        statsCard.AddChild(AddStyledLine($"{T("label.favorability", "Favorability")} {mental.Favorability}  {T("label.lust", "Lust")} {mental.Lust}  {T("label.submission", "Submission")} {mental.Submission}"));
        statsCard.AddChild(AddStyledLine($"Resistance {mental.Resistance}  Dignity {mental.Dignity}  Aversion {mental.Aversion}  Antipathy {mental.Antipathy}"));

        // === Companionship / dating ===
        var dateCard = CardContainer();
        _content.AddChild(dateCard);
        dateCard.AddChild(SubtitleLabel("Companionship & Dating"));

        if (character.Id == "anon")
        {
            dateCard.AddChild(MutedLabel("Select an eligible adult ranch resident to invite as a companion."));
        }
        else if (!_game.Dating.IsEligiblePartner(character))
        {
            dateCard.AddChild(MutedLabel("This resident is not currently eligible for an adult companionship/date outing."));
        }
        else
        {
            dateCard.AddChild(AddStyledLine(_game.Dating.RelationshipSummary(character.Id), true));
            dateCard.AddChild(MutedLabel(
                "Relationship outcomes use the same Bond, Morale, Favorability, Aversion, Dignity, Fear and other mental values as the ranch systems. Pressure can produce negative consequences rather than faster romance."));

            var activePartnerId = _game.Dating.ActivePartnerId;
            if (string.IsNullOrWhiteSpace(activePartnerId))
            {
                var inviteRow = FlowRow(8);
                dateCard.AddChild(inviteRow);

                void AddInvite(DateInviteApproach approach, string label, string tooltip)
                {
                    var button = approach == DateInviteApproach.Respectful
                        ? PrimaryButton(label, tooltip)
                        : SecondaryButton(label, tooltip);
                    button.Pressed += () =>
                    {
                        var result = _game.StartDate(character.Id, approach);
                        SetStatus(result.Message, result.Success);
                        RefreshCurrentScreen();
                    };
                    AddFlowButton(inviteRow, button, approach == DateInviteApproach.Respectful ? 168 : 184);
                }

                AddInvite(DateInviteApproach.Respectful, "Invite Respectfully",
                    "They may decline if trust and mood are too low. A willing outing is the safest route to a positive relationship.");
                AddInvite(DateInviteApproach.Pressured, "Press the Invitation",
                    "They come reluctantly. This can lower mood/trust and raise negative mental values, especially if the activity does not fit their current mood.");
                AddInvite(DateInviteApproach.Forced, "Force Them to Come",
                    "Forces the outing but seriously harms Bond/Morale and raises Aversion, Antipathy and Fear. It can erode Dignity, but is not a shortcut to romance.");
            }
            else if (!string.Equals(activePartnerId, character.Id, StringComparison.Ordinal))
            {
                var active = _game.Roster.Find(activePartnerId);
                var activeName = active is null ? activePartnerId : CharacterPickerName(active);
                dateCard.AddChild(MutedLabel($"{activeName} is already accompanying you. End that outing before inviting someone else."));
            }
            else
            {
                dateCard.AddChild(AddStyledLine("Currently accompanying you", true));

                var thought = _game.Dating.ThoughtsFor(character.Id).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(thought))
                    dateCard.AddChild(MutedLabel($"Current thought: “{thought}”"));

                var activityRow = FlowRow(8);
                dateCard.AddChild(activityRow);

                void AddActivity(DateActivityKind kind, string label)
                {
                    var cost = _game.Dating.ActivityCost(kind);
                    var available = _game.Dating.CanPerformActivity(kind, out var reason);
                    var button = kind == DateActivityKind.TownOuting
                        ? PrimaryButton($"{label} ({cost} STA)", available ? "Spend meaningful time together." : reason)
                        : SecondaryButton($"{label} ({cost} STA)", available ? "Spend meaningful time together." : reason);
                    button.Disabled = !available;
                    button.Pressed += () =>
                    {
                        var result = _game.PerformDateActivity(kind);
                        SetStatus(result.Message, result.Success);
                        RefreshCurrentScreen();
                    };
                    AddFlowButton(activityRow, button, 170);
                }

                AddActivity(DateActivityKind.RanchWalk, "Ranch Walk");
                AddActivity(DateActivityKind.WorkTogether, "Work Together");
                AddActivity(DateActivityKind.SharedMeal, "Shared Meal");
                AddActivity(DateActivityKind.TownOuting, "Town Outing");
                AddActivity(DateActivityKind.QuietRest, "Quiet Rest");

                dateCard.AddChild(MutedLabel(
                    "Only one meaningful companion activity can be completed per day phase. Walking around together itself is free. Personality, fatigue, mood and invitation approach influence the result."));

                var endDate = SecondaryButton("End Outing", "The companion returns to their normal ranch routine.");
                endDate.Pressed += () =>
                {
                    var result = _game.EndDate();
                    SetStatus(result.Message, result.Success);
                    RefreshCurrentScreen();
                };
                dateCard.AddChild(endDate);
            }
        }

        // === Care actions ===
        var careRow = FlowRow(6);
        _content.AddChild(careRow);

        var feedBtn = PrimaryButton($"Feed ({_game.PlayerStaminaCost(PlayerActivityKind.VisitFeed)} STA)", T("tooltip.visit_feed", "Feed a meal_box: Fatigue-18, Energy+10, Morale+8, Bond+4"));
        feedBtn.Disabled = !_game.CanSpendPlayerStamina(PlayerActivityKind.VisitFeed);
        feedBtn.Pressed += () =>
        {
            var line = _game.TryVisitCare(character.Id, "feed");
            SetStatus(line, false);
            RefreshCurrentScreen();
        };
        careRow.AddChild(feedBtn);

        var batheBtn = SecondaryButton($"Bathe ({_game.PlayerStaminaCost(PlayerActivityKind.VisitCare)} STA)", T("tooltip.visit_bathe", "Wash and groom her: Fatigue-12, Morale+6, Bond+2"));
        batheBtn.Disabled = !_game.CanSpendPlayerStamina(PlayerActivityKind.VisitCare);
        batheBtn.Pressed += () =>
        {
            var line = _game.TryVisitCare(character.Id, "bathe");
            SetStatus(line, false);
            RefreshCurrentScreen();
        };
        careRow.AddChild(batheBtn);

        var talkBtn = SecondaryButton($"Talk ({_game.PlayerStaminaCost(PlayerActivityKind.VisitCare)} STA)", T("tooltip.visit_talk", "Talk and comfort: Morale+7, Bond+3, Favorability+150"));
        talkBtn.Disabled = !_game.CanSpendPlayerStamina(PlayerActivityKind.VisitCare);
        talkBtn.Pressed += () =>
        {
            var line = _game.TryVisitCare(character.Id, "talk");
            SetStatus(line, false);
            RefreshCurrentScreen();
        };
        careRow.AddChild(talkBtn);

        var groomBtn = SecondaryButton($"Groom ({_game.PlayerStaminaCost(PlayerActivityKind.VisitCare)} STA)", T("tooltip.visit_groom", "Brush and groom: Morale+5, Bond+3"));
        groomBtn.Disabled = !_game.CanSpendPlayerStamina(PlayerActivityKind.VisitCare);
        groomBtn.Pressed += () =>
        {
            var line = _game.TryVisitCare(character.Id, "groom");
            SetStatus(line, false);
            RefreshCurrentScreen();
        };
        careRow.AddChild(groomBtn);

        var restBtn = SecondaryButton($"Rest ({_game.PlayerStaminaCost(PlayerActivityKind.VisitCare)} STA)", T("tooltip.visit_rest", "Let her rest: Energy+25, Fatigue-10, Morale+3"));
        restBtn.Disabled = !_game.CanSpendPlayerStamina(PlayerActivityKind.VisitCare);
        restBtn.Pressed += () =>
        {
            var line = _game.TryVisitCare(character.Id, "rest");
            SetStatus(line, false);
            RefreshCurrentScreen();
        };
        careRow.AddChild(restBtn);

        // === Gift section (keepsake items) ===
        var keepsakes = _game.State.Inventory.Items
            .Where(kvp => kvp.Value > 0 && _game.Data.Items.TryGetValue(kvp.Key, out var def) && def.Category == ItemCategory.Keepsake)
            .ToList();
        if (keepsakes.Count > 0)
        {
            var giftCard = CardContainer();
            _content.AddChild(giftCard);
            giftCard.AddChild(SubtitleLabel(T("screen.visit.gift", "Give Gift")));
            foreach (var (itemId, count) in keepsakes)
            {
                var def = _game.Data.Item(itemId);
                var row = FlowRow(8);
                giftCard.AddChild(row);
                row.AddChild(AddStyledLine($"{def.DisplayName} x{count}", true));
                var give = SecondaryButton($"{T("screen.visit.give", "Give")} ({_game.PlayerStaminaCost(PlayerActivityKind.VisitGift)} STA)", $"Give {def.DisplayName}: Bond+8, Morale+5, Favorability+400");
                give.Disabled = !_game.CanSpendPlayerStamina(PlayerActivityKind.VisitGift);
                give.Pressed += () =>
                {
                    var line = _game.TryVisitCare(character.Id, "gift", itemId);
                    SetStatus(line, false);
                    RefreshCurrentScreen();
                };
                AddFlowButton(row, give, 92);
            }
        }
        else
        {
            var giftCard = CardContainer();
            _content.AddChild(giftCard);
            giftCard.AddChild(MutedLabel(T("screen.visit.no_gifts", "Buy a keepsake gift at the General Store to gift it.")));
        }

        // === Link to training ===
        var trainLink = CardContainer();
        _content.AddChild(trainLink);
        trainLink.AddChild(MutedLabel(T("screen.visit.training_hint", "For disciplined training, open the Training Room.")));
        var toTraining = PrimaryButton(T("screen.visit.open_training", "Open Training Room"), T("tooltip.visit_training", "Perform training actions on this character"));
        toTraining.Pressed += () =>
        {
            var charList = _game.Roster.Characters.ToList();
            var idx = charList.FindIndex(c => c.Id == character.Id);
            if (idx >= 0) _trainingCharIdx = idx;
            ShowScreen("training");
        };
        trainLink.AddChild(toTraining);
    }

    // === Milk state tracking ===
    private int _milkCharIdx;

    private void RenderMilkEconomy()
    {
        AddTitle(T("screen.milk", "Milk Processing"));
        var chars = _game.Roster.Characters;
        if (!chars.Any())
        {
            var card = CardContainer();
            _content.AddChild(card);
            card.AddChild(AddStyledLine(T("screen.milk.no_characters", "No characters on the ranch.")));
            return;
        }

        _milkCharIdx = Math.Clamp(_milkCharIdx, 0, chars.Count - 1);
        var character = chars[_milkCharIdx];
        var milk = character.Milk;

        // === Character selector row ===
        var selectorRow = FlowRow(8);
        _content.AddChild(selectorRow);
        selectorRow.AddChild(MutedLabel($"{T("label.character", "Character")}:"));
        var charPicker = StyledPicker(240);
        charPicker.TooltipText = T("tooltip.milk_char", "Select a character to manage milk production");
        for (var i = 0; i < chars.Count; i++)
        {
            charPicker.AddItem(CharacterPickerName(chars[i]));
            if (i == _milkCharIdx) charPicker.Selected = i;
        }
        charPicker.ItemSelected += idx => { _milkCharIdx = (int)idx; _game.NotifyStateChanged(); };
        selectorRow.AddChild(charPicker);

        // === Stats card ===
        var statsCard = CardContainer();
        _content.AddChild(statsCard);
        statsCard.AddChild(SubtitleLabel($"{CharacterPickerName(character)} - {T("screen.milk.volume", "Milk Volume")}"));

        var volumeStr = milk.CurrentAmount < milk.Capacity
            ? $"{milk.CurrentAmount} / {milk.Capacity} {T("unit.ml", "ml")}"
            : $"{milk.CurrentAmount} / {milk.Capacity} {T("unit.ml", "ml")} ({T("screen.milk.full", "full")})";
        statsCard.AddChild(AddStyledLine(volumeStr));
        statsCard.AddChild(AddStyledLine($"{T("screen.milk.production", "Production")}: {milk.Production + milk.BaseOutput} {T("unit.ml", "ml")}/{T("label.day", "day")} (base {milk.BaseOutput} + bonus {milk.Production})"));

        // Quality display
        var qColor = milk.Quality switch { >= 80 => "55d6be", >= 50 => "f0c060", _ => "d0a0a0" };
        statsCard.AddChild(AddStyledLine($"{T("screen.milk.quality", "Quality")}: {milk.Quality}% — {ConcentrationLabel(milk.Concentration)} ({T("screen.milk.price_hint", "price per unit")}: {3 + milk.Quality / 50 + ConcentrationBonus(milk.Concentration)}{T("unit.g", "g")})"));

        if (milk.HasMilkConstitution || milk.HasMagicMilkConstitution)
        {
            var traits = new System.Collections.Generic.List<string>();
            if (milk.HasMilkConstitution) traits.Add(T("screen.milk.milk_constitution", "Milk Constitution"));
            if (milk.HasMagicMilkConstitution) traits.Add(T("screen.milk.magic_milk", "Magic Milk Constit."));
            statsCard.AddChild(AddStyledLine($"{T("label.traits", "Traits")}: {string.Join(", ", traits)}"));
        }
        else if (!character.Talents.Contains("extreme_milk_pressure"))
        {
            statsCard.AddChild(AddStyledLine(T("screen.milk.no_constitution", "No milk constitution yet. Use a Lactation Drug on this character to start producing milk.")));
        }

        statsCard.AddChild(AddStyledLine($"{T("screen.milk.equipment", "Equipment")}: {(milk.EquippedMilkerId > 0 ? $"{T("screen.milk.milker", "Milker")} #{milk.EquippedMilkerId}" : T("screen.milk.no_milker", "None"))}"));

        // Lifetime stats
        statsCard.AddChild(AddStyledLine($"{T("screen.milk.total_shipped", "Lifetime")}: {milk.TotalShipped} {T("unit.units", "units")} shipped, {milk.TotalRevenue}{T("unit.g", "g")} {T("screen.milk.revenue", "revenue")}"));

        // Global stats
        statsCard.AddChild(AddStyledLine($"{T("screen.milk.ranch_total", "Ranch Total")}: {_game.State.Mature.TotalMilkProduced} {T("unit.units", "units")} produced, {_game.State.Mature.TotalMilkRevenue}{T("unit.g", "g")} {T("screen.milk.revenue", "revenue")}"));

        // === Action buttons ===
        var actions = FlowRow(10);
        _content.AddChild(actions);

        var shipBtn = PrimaryButton($"{T("screen.milk.ship", "Ship")} ({milk.CurrentAmount} {T("unit.units", "units")})", T("tooltip.ship_milk", "Sell all stored milk from this character. Price depends on quality and concentration."));
        shipBtn.Pressed += () =>
        {
            var revenue = _game.ShipMilk(character.Id);
            if (revenue > 0) _game.Feedback.PlayConfirm();
            ShowScreen(_currentScreen);
        };
        actions.AddChild(shipBtn);

        // Ship all characters button
        var hasAnyMilk = chars.Any(c => c.Milk.CurrentAmount > 0);
        var shipAll = SecondaryButton(T("screen.milk.ship_all_characters", "Ship All Characters"), T("tooltip.ship_all", "Sell all stored milk from every character at once"));
        shipAll.Disabled = !hasAnyMilk;
        shipAll.Pressed += () =>
        {
            foreach (var c in chars) _game.ShipMilk(c.Id);
            _game.Feedback.PlayConfirm();
            ShowScreen(_currentScreen);
        };
        _content.AddChild(shipAll);

        if (milk.CurrentAmount == 0)
            _content.AddChild(MutedLabel(T("screen.milk.no_milk", "No milk stored. Production is calculated once during daily settlement; advance/end the day to produce the next batch.")));
    }

    private static string ConcentrationLabel(string concentration) => concentration switch
    {
        "standard" => "Standard",
        "rich" => "Rich",
        "superior" => "Superior",
        "premium" => "Premium",
        "supreme" => "Supreme",
        _ => concentration
    };

    private static int ConcentrationBonus(string concentration) => concentration switch
    {
        "rich" => 2,
        "superior" => 4,
        "premium" => 6,
        "supreme" => 10,
        _ => 0
    };

    private int _mentalCharIdx;

    private void RenderMentalState()
    {
        AddTitle(T("screen.mental", "Mental State Overview"));
        var chars = _game.Roster.Characters;
        if (!chars.Any())
        {
            var card = CardContainer();
            _content.AddChild(card);
            card.AddChild(AddStyledLine(T("screen.mental.no_characters", "No characters on the ranch.")));
            return;
        }

        _mentalCharIdx = Math.Clamp(_mentalCharIdx, 0, chars.Count - 1);
        var character = chars[_mentalCharIdx];
        var m = character.Mature;

        // === Character selector ===
        var selectorRow = FlowRow(8);
        _content.AddChild(selectorRow);
        selectorRow.AddChild(MutedLabel($"{T("label.character", "Character")}:"));
        var charPicker = StyledPicker(240);
        charPicker.TooltipText = T("tooltip.mental_char", "Select a character to inspect mental state");
        for (var i = 0; i < chars.Count; i++)
        {
            charPicker.AddItem(CharacterPickerName(chars[i]));
            if (i == _mentalCharIdx) charPicker.Selected = i;
        }
        charPicker.ItemSelected += idx => { _mentalCharIdx = (int)idx; _game.NotifyStateChanged(); };
        selectorRow.AddChild(charPicker);

        // === Fall State card ===
        var fallCard = CardContainer();
        _content.AddChild(fallCard);
        var fallLabel = AddStyledLine($"{T("screen.mental.fall_state", "Fall State")}: {FallStateDisplayName(m.FallState)}", true);
        fallLabel.TooltipText = T("tooltip.fall_state", "Current mental fall state, determined by thresholds of key mental stats");
        fallCard.AddChild(fallLabel);
        fallCard.AddChild(AddStyledLine($"  {T("screen.mental.collapsed", "Collapsed")}: {(m.IsCollapsed ? T("common.yes", "Yes") : T("common.no", "No"))} | {T("screen.mental.brainwashed", "Brainwashed")}: {(m.IsBrainwashed ? T("common.yes", "Yes") : T("common.no", "No"))}"));

        // fall state thresholds legend
        var legend = CardContainer();
        legend.AddThemeConstantOverride("separation", 2);
        legend.AddChild(MutedLabel(T("screen.mental.thresholds", "Thresholds (when all affections < 1000, positive > 8000):")));
        legend.AddChild(MutedLabel($"{FallStateDisplayName(FallState.Collapse)}: {T("screen.mental.collapse_cond", "Pain+Fear+Despair >= 15000")}"));
        legend.AddChild(MutedLabel($"{FallStateDisplayName(FallState.MilkCow)}: {T("screen.mental.milkcow_cond", "MilkCow >= 5000")}"));
        legend.AddChild(MutedLabel($"{FallStateDisplayName(FallState.Slave)}: {T("screen.mental.slave_cond", "Submission >= 8000")}"));
        legend.AddChild(MutedLabel($"{FallStateDisplayName(FallState.Devotion)}: {T("screen.mental.devotion_cond", "Obedience >= 8000")}"));
        legend.AddChild(MutedLabel($"{FallStateDisplayName(FallState.Love)}: {T("screen.mental.love_cond", "Favorability >= 8000")}"));
        fallCard.AddChild(legend);

        // === Mental Parameters ===
        var mentalCard = CardContainer();
        _content.AddChild(mentalCard);
        mentalCard.AddChild(SubtitleLabel(T("screen.mental.params", "Mental Parameters")));
        AddMentalBar(mentalCard, T("label.resistance", "Resistance"), m.Resistance, 10000, "ff6666", T("tooltip.resistance", "Resistance to mental influence. Decreases with training."));
        AddMentalBar(mentalCard, T("label.dignity", "Dignity"), m.Dignity, 10000, "ff9966", T("tooltip.dignity", "Self-worth. Lowered by degrading acts."));
        AddMentalBar(mentalCard, T("label.aversion", "Aversion"), m.Aversion, 10000, "cc66ff", T("tooltip.aversion", "Dislike of sexual acts. Increases with aggressive training."));
        AddMentalBar(mentalCard, T("label.reason", "Reason"), m.Reason, 10000, "66aaff", T("tooltip.reason", "Logical thinking. Reduces as fall state progresses."));
        AddMentalBar(mentalCard, T("label.mental_strength", "Mental Strength"), m.MentalStrength, 10000, "66ddaa", T("tooltip.mental_strength", "Overall mental fortitude. Painful training reduces it."));

        // === Affection ===
        var affectionCard = CardContainer();
        _content.AddChild(affectionCard);
        affectionCard.AddChild(SubtitleLabel(T("screen.mental.affection", "Affection / Dependence")));
        AddMentalBar(affectionCard, T("label.favorability", "Favorability"), m.Favorability, 20000, "ff88cc", T("tooltip.favorability", "Liking toward the owner. Increases with pleasure-based training."));
        AddMentalBar(affectionCard, T("label.obedience", "Obedience"), m.Obedience, 20000, "88ccff", T("tooltip.obedience", "Willingness to follow orders. Balanced pleasure/pain increases it."));
        AddMentalBar(affectionCard, T("label.lust", "Lust"), m.Lust, 20000, "ff66aa", T("tooltip.lust", "Sexual desire. Increases with pleasure-focused actions."));
        AddMentalBar(affectionCard, T("label.submission", "Submission"), m.Submission, 20000, "aa88ff", T("tooltip.submission", "Acceptance of domination. Pain-based training raises it."));
        AddMentalBar(affectionCard, T("label.milkcow", "Milk Cow"), m.MilkCow, 20000, "ffcc88", T("tooltip.milkcow", "Dairy instinct. Pain-based training on breasts/nipples raises it."));

        // === Pain / Negative ===
        var painCard = CardContainer();
        _content.AddChild(painCard);
        painCard.AddChild(SubtitleLabel(T("screen.mental.negative", "Pain / Negative Emotions")));
        AddMentalBar(painCard, T("label.pain", "Pain"), m.Pain, 10000, "ff4444", T("tooltip.pain_state", "Physical pain accumulated. Contributes to Collapse fall state."));
        AddMentalBar(painCard, T("label.fear", "Fear"), m.Fear, 10000, "aa44ff", T("tooltip.fear", "Fear response. Grows with fear-inducing sensations."));
        AddMentalBar(painCard, T("label.disgust", "Disgust"), m.Disgust, 10000, "66aa44", T("tooltip.disgust", "Revulsion. Builds from degrading or disgusting acts."));
        AddMentalBar(painCard, T("label.antipathy", "Antipathy"), m.Antipathy, 10000, "886644", T("tooltip.antipathy", "Hostility toward the owner. Counteracts favorability."));
        AddMentalBar(painCard, T("label.despair", "Despair"), m.Despair, 10000, "444488", T("tooltip.despair", "Hopelessness. High despair accelerates Collapse."));

        // === Pleasure ===
        var pleasureCard = CardContainer();
        _content.AddChild(pleasureCard);
        pleasureCard.AddChild(SubtitleLabel(T("screen.mental.pleasure", "Pleasure / Sensitivity")));
        AddMentalBar(pleasureCard, T("label.pleasure_v", "Pleasure (Vaginal)"), m.PleasureV, 10000, "ff88ff");
        AddMentalBar(pleasureCard, T("label.pleasure_a", "Pleasure (Anal)"), m.PleasureA, 10000, "88aaff");
        AddMentalBar(pleasureCard, T("label.pleasure_b", "Pleasure (Breast)"), m.PleasureB, 10000, "ffaacc");
        AddMentalBar(pleasureCard, T("label.pleasure_c", "Pleasure (Clitoral)"), m.PleasureC, 10000, "ff66dd");
        AddMentalBar(pleasureCard, T("label.pleasure_n", "Pleasure (Nipple)"), m.PleasureN, 10000, "cc88aa");
        AddMentalBar(pleasureCard, T("label.lubrication_v", "Lubrication V"), m.LubricationV, 10000, "88ddff");

        // === Addictions ===
        var a = character.Addictions;
        var addictionCard = CardContainer();
        _content.AddChild(addictionCard);
        addictionCard.AddChild(SubtitleLabel(T("screen.mental.addictions", "Addictions")));
        AddMentalBar(addictionCard, T("label.addiction_v", "Vaginal Ejaculation"), a.VaginalEjaculation, 100, "ff6688", T("tooltip.addiction_v", "Addiction to vaginal ejaculation. Raised by VInsertion actions."));
        AddMentalBar(addictionCard, T("label.addiction_a", "Anal Ejaculation"), a.AnalEjaculation, 100, "88aaff", T("tooltip.addiction_a", "Addiction to anal ejaculation. Raised by AInsertion actions."));
        AddMentalBar(addictionCard, T("label.addiction_b", "Breast Ejaculation"), a.BreastEjaculation, 100, "ffaacc", T("tooltip.addiction_b", "Addiction to breast ejaculation. Raised by PenisAction."));
        AddMentalBar(addictionCard, T("label.addiction_semen", "Semen Drinking"), a.SemenDrinking, 100, "cc88ff", T("tooltip.addiction_semen", "Addiction to consuming semen. Raised by Mouth actions."));
        AddMentalBar(addictionCard, T("label.addiction_masochism", "Masochism"), a.Masochism, 100, "ff4466", T("tooltip.addiction_masochism", "Deriving pleasure from pain. Raised by Pain actions."));
        AddMentalBar(addictionCard, T("label.addiction_milking", "Milking"), a.Milking, 100, "ffcc66", T("tooltip.addiction_milking", "Addiction to being milked. Raised by Tool actions."));
        AddMentalBar(addictionCard, T("label.addiction_tentacle", "Tentacle"), a.Tentacle, 100, "66ff88", T("tooltip.addiction_tentacle", "Addiction to tentacle stimulation. Raised by Tentacle actions."));
        AddMentalBar(addictionCard, T("label.addiction_service", "Service Spirit"), a.ServiceSpirit, 100, "88ddff", T("tooltip.addiction_service", "Drive to serve. Raised by Service actions."));
        if (a.SemenAddiction > 0 || a.Gangbang > 0 || a.Sadism > 0 || a.Lesbian > 0)
        {
            AddMentalBar(addictionCard, T("label.addiction_semen_add", "Semen Addiction"), a.SemenAddiction, 100, "aa66ff");
            AddMentalBar(addictionCard, T("label.addiction_gangbang", "Gangbang"), a.Gangbang, 100, "ff6688");
            AddMentalBar(addictionCard, T("label.addiction_sadism", "Sadism"), a.Sadism, 100, "ff4466");
            AddMentalBar(addictionCard, T("label.addiction_lesbian", "Lesbian"), a.Lesbian, 100, "ff88cc");
        }
    }

    private void AddMentalBar(Container parent, string label, int current, int max, string colorHex = "88ccff", string tooltip = "")
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 6);
        row.CustomMinimumSize = new Vector2(0, 24);
        row.TooltipText = tooltip;

        var pct = Math.Clamp(current * 100f / max, 0, 100);
        var labelNode = new Label
        {
            Text = $"{label}: {current}/{max}",
            CustomMinimumSize = new Vector2(260, 0),
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            VerticalAlignment = Godot.VerticalAlignment.Center
        };
        labelNode.AddThemeFontSizeOverride("font_size", 12);
        row.AddChild(labelNode);

        var bar = new ProgressBar
        {
            Value = pct,
            ShowPercentage = false,
            CustomMinimumSize = new Vector2(120, 16),
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        if (!string.IsNullOrEmpty(colorHex))
        {
            var c = Color.FromHtml(colorHex);
            bar.AddThemeStyleboxOverride("fill", new StyleBoxFlat { BgColor = c });
        }
        row.AddChild(bar);

        var pctLabel = new Label
        {
            Text = $"{pct:F0}%",
            CustomMinimumSize = new Vector2(36, 0),
            VerticalAlignment = Godot.VerticalAlignment.Center,
            HorizontalAlignment = Godot.HorizontalAlignment.Right
        };
        pctLabel.AddThemeFontSizeOverride("font_size", 11);
        row.AddChild(pctLabel);

        parent.AddChild(row);
    }

    private static string FallStateDisplayName(FallState state) => state switch
    {
        FallState.Normal => "Normal",
        FallState.Love => "In Love",
        FallState.Devotion => "Devoted",
        FallState.Collapse => "Collapsed",
        FallState.MilkCow => "Milk Cow",
        FallState.Slave => "Slave",
        _ => state.ToString()
    };

    private string CharacterPickerName(CharacterState character)
    {
        var definitionName = _game.Roster.DefinitionFor(character).DisplayName;
        if (!string.IsNullOrWhiteSpace(definitionName))
        {
            return definitionName;
        }

        if (!string.IsNullOrWhiteSpace(character.DisplayNameOverride))
        {
            return character.DisplayNameOverride!;
        }

        return character.Id;
    }

    private Control BuildPicker(string[] options, string current, Action<string> onSelect)
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 8);
        row.CustomMinimumSize = new Vector2(0, 32);
        var picker = StyledPicker(180);
        var selIdx = 0;
        for (var i = 0; i < options.Length; i++)
        {
            picker.AddItem(options[i]);
            picker.SetItemMetadata(i, options[i]);
            if (string.Equals(options[i], current, StringComparison.OrdinalIgnoreCase))
                selIdx = i;
        }
        picker.Selected = selIdx;
        picker.ItemSelected += selected => onSelect(picker.GetItemMetadata((int)selected).AsString());
        row.AddChild(picker);
        return row;
    }

    private void RenderCharacterCreation()
    {
        AddTitle(T("screen.character_creation", "Character Creation"));
        var player = _game.State.Player;
        var scene = GD.Load<PackedScene>("res://scenes/CharacterCreationScreen.tscn");
        var root = scene.Instantiate<VBoxContainer>();
        _content.AddChild(root);

        var cardStyle = CardStyle(Palette.CardFill, Palette.CardBorder, 1, 8);
        root.GetNode<PanelContainer>("CreationBody/PreviewCard").AddThemeStyleboxOverride("panel", cardStyle);
        foreach (var name in new[] { "BasicCard", "BodyCard", "AppearanceCard", "AccessoriesCard", "PetMountCard" })
            root.GetNode<PanelContainer>($"CreationBody/SettingsColumn/{name}").AddThemeStyleboxOverride("panel", cardStyle);

        var previewTitle = root.GetNode<Label>("CreationBody/PreviewCard/PreviewInner/PreviewTitle");
        previewTitle.Text = T("screen.character_creation.preview", "3D Character Preview");
        previewTitle.AddThemeColorOverride("font_color", Palette.SectionText);

        // --- Basic Information ---
        {
            var title = root.GetNode<Label>("CreationBody/SettingsColumn/BasicCard/BasicInner/BasicTitle");
            title.AddThemeColorOverride("font_color", Palette.SectionText);
            ConfigureReadableLabel(title);
            title.Text = T("screen.character_creation.basic", "Basic Information");

            var grid = root.GetNode<GridContainer>("CreationBody/SettingsColumn/BasicCard/BasicInner/BasicGrid");
            StyleGridLabels(grid);

            var nameInput = root.GetNode<LineEdit>("CreationBody/SettingsColumn/BasicCard/BasicInner/BasicGrid/NameInput");
            nameInput.PlaceholderText = T("screen.character_creation.name_hint", "Enter your name");
            nameInput.Text = player.Name;
            nameInput.TextChanged += _ => _game.SetPlayerName(nameInput.Text);

            PopulatePicker(root.GetNode<OptionButton>("CreationBody/SettingsColumn/BasicCard/BasicInner/BasicGrid/SpeciesPicker"), CharacterGenerationPools.Races, player.Race, val => _game.SetPlayerRace(val));
            PopulatePicker(root.GetNode<OptionButton>("CreationBody/SettingsColumn/BasicCard/BasicInner/BasicGrid/GenderPicker"), new[] { "Male", "Female" }, player.Gender, val =>
            {
                _game.SetPlayerGender(val);
                var female = string.Equals(val, "Female", StringComparison.OrdinalIgnoreCase);
                root.GetNode<Label>("CreationBody/SettingsColumn/BodyCard/BodyInner/BodyGrid/ChestLabel").Visible = female;
                root.GetNode<OptionButton>("CreationBody/SettingsColumn/BodyCard/BodyInner/BodyGrid/ChestPicker").Visible = female;
            });

            var ranchInput = root.GetNode<LineEdit>("CreationBody/SettingsColumn/BasicCard/BasicInner/BasicGrid/RanchInput");
            ranchInput.Text = player.RanchName;
            ranchInput.PlaceholderText = T("screen.character_creation.ranch_hint", "Enter your ranch name");
            ranchInput.TextChanged += _ => _game.SetRanchName(ranchInput.Text);
        }

        // --- Body ---
        {
            var title = root.GetNode<Label>("CreationBody/SettingsColumn/BodyCard/BodyInner/BodyTitle");
            title.AddThemeColorOverride("font_color", Palette.SectionText);
            ConfigureReadableLabel(title);
            title.Text = T("screen.character_creation.body", "Body");

            var grid = root.GetNode<GridContainer>("CreationBody/SettingsColumn/BodyCard/BodyInner/BodyGrid");
            StyleGridLabels(grid);

            var heightLabels = CharacterGenerationPools.HeightRanges.Select(h => h.Label).ToArray();
            var currentHeightLabel = CharacterGenerationPools.HeightRanges.FirstOrDefault(h => h.Min <= player.Height && player.Height <= h.Max).Label ?? "Imposing";
            PopulatePicker(root.GetNode<OptionButton>("CreationBody/SettingsColumn/BodyCard/BodyInner/BodyGrid/HeightPicker"), heightLabels, currentHeightLabel, val =>
            {
                var range = CharacterGenerationPools.HeightRanges.FirstOrDefault(h => h.Label == val);
                _game.ModifyPlayer(p => { p.Height = (range.Min + range.Max) / 2; });
            });

            var ageLabels = CharacterGenerationPools.PlayerApparentAges.Select(a => a.Label).ToArray();
            var currentAgeLabel = CharacterGenerationPools.PlayerApparentAges.FirstOrDefault(a => a.Age == player.ApparentAge).Label ?? "Adult";
            PopulatePicker(root.GetNode<OptionButton>("CreationBody/SettingsColumn/BodyCard/BodyInner/BodyGrid/AgePicker"), ageLabels, currentAgeLabel, val =>
            {
                var entry = CharacterGenerationPools.PlayerApparentAges.FirstOrDefault(a => a.Label == val);
                _game.ModifyPlayer(p => p.ApparentAge = entry.Age);
            });

            PopulatePicker(root.GetNode<OptionButton>("CreationBody/SettingsColumn/BodyCard/BodyInner/BodyGrid/BuildPicker"), CharacterGenerationPools.BodyShapes, player.BodyShape, val => _game.ModifyPlayer(p => p.BodyShape = val));

            var chestLabel = root.GetNode<Label>("CreationBody/SettingsColumn/BodyCard/BodyInner/BodyGrid/ChestLabel");
            var chestPicker = root.GetNode<OptionButton>("CreationBody/SettingsColumn/BodyCard/BodyInner/BodyGrid/ChestPicker");
            PopulatePicker(chestPicker, CharacterGenerationPools.BreastSizeLabels, player.BustSize, val => _game.ModifyPlayer(p => p.BustSize = val));
            var showChest = string.Equals(player.Gender, "Female", StringComparison.OrdinalIgnoreCase);
            chestLabel.Visible = showChest;
            chestPicker.Visible = showChest;

            var skinPicker = root.GetNode<OptionButton>("CreationBody/SettingsColumn/BodyCard/BodyInner/BodyGrid/SkinRow/SkinPicker");
            PopulatePicker(skinPicker, CharacterGenerationPools.SkinColors, player.SkinColor, val => _game.ModifyPlayer(p => p.SkinColor = val));

            PopulatePicker(root.GetNode<OptionButton>("CreationBody/SettingsColumn/BodyCard/BodyInner/BodyGrid/TailPicker"), CharacterGenerationPools.TailTypes, player.TailType, val => _game.ModifyPlayer(p => p.TailType = val));
        }

        // --- Appearance ---
        {
            var title = root.GetNode<Label>("CreationBody/SettingsColumn/AppearanceCard/AppearanceInner/AppearanceTitle");
            title.AddThemeColorOverride("font_color", Palette.SectionText);
            ConfigureReadableLabel(title);
            title.Text = T("screen.character_creation.appearance", "Appearance");

            var grid = root.GetNode<GridContainer>("CreationBody/SettingsColumn/AppearanceCard/AppearanceInner/AppearanceGrid");
            StyleGridLabels(grid);

            var hairPicker = root.GetNode<OptionButton>("CreationBody/SettingsColumn/AppearanceCard/AppearanceInner/AppearanceGrid/HairColorRow/HairColorPicker");
            PopulatePicker(hairPicker, CharacterGenerationPools.HairColors, player.HairColor, val => _game.ModifyPlayer(p => p.HairColor = val));

            PopulatePicker(root.GetNode<OptionButton>("CreationBody/SettingsColumn/AppearanceCard/AppearanceInner/AppearanceGrid/HairLengthPicker"), CharacterGenerationPools.HairFeatures, player.HairFeature, val => _game.ModifyPlayer(p => p.HairFeature = val));
            PopulatePicker(root.GetNode<OptionButton>("CreationBody/SettingsColumn/AppearanceCard/AppearanceInner/AppearanceGrid/HairstylePicker"), CharacterGenerationPools.HairStyles, player.HairStyle, val => _game.ModifyPlayer(p => p.HairStyle = val));

            var eyePicker = root.GetNode<OptionButton>("CreationBody/SettingsColumn/AppearanceCard/AppearanceInner/AppearanceGrid/EyeColorRow/EyeColorPicker");
            PopulatePicker(eyePicker, CharacterGenerationPools.EyeColors, player.EyeColor, val => _game.ModifyPlayer(p => p.EyeColor = val));

            PopulatePicker(root.GetNode<OptionButton>("CreationBody/SettingsColumn/AppearanceCard/AppearanceInner/AppearanceGrid/EyeStylePicker"), CharacterGenerationPools.EyeShapes, player.EyeShape, val => _game.ModifyPlayer(p => p.EyeShape = val));
        }

        // --- Accessories ---
        {
            var title = root.GetNode<Label>("CreationBody/SettingsColumn/AccessoriesCard/AccessoriesInner/AccessoriesTitle");
            title.AddThemeColorOverride("font_color", Palette.SectionText);
            ConfigureReadableLabel(title);
            title.Text = T("screen.character_creation.accessories", "Accessories");

            var grid = root.GetNode<GridContainer>("CreationBody/SettingsColumn/AccessoriesCard/AccessoriesInner/AccessoriesGrid");
            StyleGridLabels(grid);

            var hornsCb = root.GetNode<CheckBox>("CreationBody/SettingsColumn/AccessoriesCard/AccessoriesInner/AccessoriesGrid/AccessoriesRow/HornsCheck");
            hornsCb.ButtonPressed = player.HasHorns;
            hornsCb.Toggled += on => _game.ModifyPlayer(p => p.HasHorns = on);

            var glassesCb = root.GetNode<CheckBox>("CreationBody/SettingsColumn/AccessoriesCard/AccessoriesInner/AccessoriesGrid/AccessoriesRow/GlassesCheck");
            glassesCb.ButtonPressed = player.HasGlasses;
            glassesCb.Toggled += on => _game.ModifyPlayer(p => p.HasGlasses = on);

            PopulatePicker(root.GetNode<OptionButton>("CreationBody/SettingsColumn/AccessoriesCard/AccessoriesInner/AccessoriesGrid/BodyFurPicker"), CharacterGenerationPools.BodyFurOptions, player.BodyFur, val => _game.ModifyPlayer(p => p.BodyFur = val));
        }

        // --- Pet & Mount ---
        {
            var title = root.GetNode<Label>("CreationBody/SettingsColumn/PetMountCard/PetMountInner/PetMountTitle");
            title.AddThemeColorOverride("font_color", Palette.SectionText);
            ConfigureReadableLabel(title);
            title.Text = T("screen.character_creation.pet_mount", "Pet & Mount");

            var grid = root.GetNode<GridContainer>("CreationBody/SettingsColumn/PetMountCard/PetMountInner/PetMountGrid");
            StyleGridLabels(grid);

            var petIds = _game.Data.Pets.Keys.ToList();
            var petNames = petIds.Select(id => _game.Data.Pets[id].DisplayName).ToArray();
            var currentPetIdx = Math.Max(0, petIds.FindIndex(id => string.Equals(id, player.StartingPetId, StringComparison.OrdinalIgnoreCase)));
            var petPicker = root.GetNode<OptionButton>("CreationBody/SettingsColumn/PetMountCard/PetMountInner/PetMountGrid/PetPicker");
            PopulatePicker(petPicker, petNames, currentPetIdx >= 0 ? petNames[currentPetIdx] : petNames[0], val =>
            {
                var idx = Array.IndexOf(petNames, val);
                if (idx >= 0) _game.ModifyPlayer(p => p.StartingPetId = petIds[idx]);
            });

            var mountIds = petIds.Where(id => _game.Data.Pets[id].IsMountable).ToList();
            var mountNames = new[] { T("screen.character_creation.no_mount", "None") }.Concat(mountIds.Select(id => _game.Data.Pets[id].DisplayName)).ToArray();
            var mountValues = new[] { "none" }.Concat(mountIds).ToArray();
            var mountPicker = root.GetNode<OptionButton>("CreationBody/SettingsColumn/PetMountCard/PetMountInner/PetMountGrid/MountPicker");
            var mountCurr = mountValues.Contains(player.StartingMountId) ? player.StartingMountId : "none";
            var mountCurrName = mountNames[Array.IndexOf(mountValues, mountCurr)];
            PopulatePicker(mountPicker, mountNames, mountCurrName, val =>
            {
                var idx = Array.IndexOf(mountNames, val);
                if (idx >= 0) _game.ModifyPlayer(p => p.StartingMountId = mountValues[idx]);
            });
        }

        // --- Action Buttons ---
        {
            var start = root.GetNode<Button>("ActionRow/StartButton");
            start.Text = T("screen.character_creation.start", "Start Game");
            start.TooltipText = T("screen.character_creation.start_tip", "Begin your story and head to the ranch");
            ApplyPrimaryButtonStyle(start);
            start.Pressed += () => { _game.Feedback.PlayConfirm(); ShowScreen("prologue"); };

            var back = root.GetNode<Button>("ActionRow/BackButton");
            back.Text = T("screen.character_creation.back", "Back To Title");
            back.TooltipText = T("screen.character_creation.back_tip", "Return to the main menu");
            ApplySecondaryButtonStyle(back);
            back.Pressed += () =>
            {
                _game.Feedback.PlayConfirm();
                GetTree().ChangeSceneToFile("res://scenes/MainMenu.tscn");
            };
        }
    }

    private void StyleGridLabels(GridContainer grid)
    {
        foreach (var child in grid.GetChildren())
        {
            if (child is Label label)
            {
                label.AddThemeColorOverride("font_color", Palette.BodyText);
                label.VerticalAlignment = VerticalAlignment.Center;
            }
        }
    }

    private void PopulatePicker(OptionButton picker, string[] options, string current, Action<string> onSelect)
    {
        picker.Clear();
        var selIdx = 0;
        for (var i = 0; i < options.Length; i++)
        {
            picker.AddItem(options[i]);
            picker.SetItemMetadata(i, options[i]);
            if (string.Equals(options[i], current, StringComparison.OrdinalIgnoreCase))
                selIdx = i;
        }
        picker.Selected = selIdx;
        picker.ItemSelected += selected => onSelect(picker.GetItemMetadata((int)selected).AsString());
    }

    // === Prologue state tracking ===
    private int _prologuePage;
    private readonly System.Collections.Generic.List<TypewriterLabel> _prologueLines = new();

    /// <summary>True when an interactive typewriter skipped the remaining text (first press shows full line, second press advances).</summary>
    private bool FinishActiveTypewriting()
    {
        var hasPending = _prologueLines.Any(line => !line.IsComplete);
        foreach (var line in _prologueLines)
        {
            line.Finish();
        }

        return hasPending;
    }

    private void RenderPrologue()
    {
        _prologuePage = 0;
        ShowProloguePage();
    }

    private void ShowProloguePage()
    {
        ClearContent();
        _prologueLines.Clear();
        AddTitle(T("screen.prologue", "Opening"));
        var player = _game.State.Player;

        var pages = new[]
        {
            // Page 0
            new[] {
                T("prologue.world", "This is a world where the demons of Makai and the ningens on the surface are in constant conflict."),
                T("prologue.world2", "...That being said, many creatures from Makai don't actually wish to fight."),
                T("prologue.player", $"You are one of those demons who avoids combat. You chose not to join the demon lord's army. In the town of Okachi, located in the Makai Plains, as the owner of a small countryside ranch, you lead a leisurely life taking care of Makai's dairy cows.")
            },
            // Page 1
            new[] {
                T("prologue.slay1", $"Though it's a small ranch, {player.Name} isn't running {player.RanchName} alone.", player.Name, player.RanchName),
                T("prologue.slay2", "A few months ago, a ningen girl named Slay somehow wandered into Makai."),
                T("prologue.slay3", "Without weapons or magic, she posed no threat. While the townspeople were wondering what to do with her, your ranch had just finished a part-time contract, and being short on help, you thought it might be fate and decided to let her live and work there.")
            },
            // Page 2
            new[] {
                T("prologue.eugene1", "\"I need a favor. I'll bring a ningen girl here, so could you try milking her for me?\" — Eugene"),
                T("prologue.eugene2", "One day, Eugene, a researcher from the Demon King's Army Research Institute and a friend of yours, said something strange."),
                T("prologue.eugene3", "He explained that after many failed experiments with the magic recovery potion, the main office demanded they 'just mix in some breast milk from the ningen female that got caught.'"),
                T("prologue.eugene4", "After a lot of trial and error, a <Mana-Infused Breast Milk Stimulant> was created, specifically for ningens."),
                T("prologue.eugene5", "\"We'll handle all the preparations on our side, so you'll benefit from this arrangement.\"")
            },
            // Page 3
            new[] {
                T("prologue.aftermath", "A few days later..."),
                T("prologue.mano1", "\"We're all set! Oh, and it seems like Mano is coming over, too.\" — Eugene"),
                T("prologue.mano2", "\"Oh my, so from tonight onward, the owner will be doing naughty things with this girl.\" — Mano"),
                T("prologue.virgin", "\"Apparently, there's something called a 【Virginity Barrier】, and because of that blessing, you can't use her front hole.\" — Eugene"),
                T("prologue.virgin2", "Due to restrictions, the ningen girl Maria has a virginity barrier that prevents vaginal use. Other methods are available for training and milk extraction.")
            },
            // Page 4
            new[] {
                T("prologue.milk1", "\"Wait a second, but for ningens, breast milk can't be produced unless they're pregnant, right?\" — Mano"),
                T("prologue.milk2", "\"That's not an issue. With the <Mana-Infused Breast Milk Stimulant>, they can produce breast milk even without pregnancy.\" — Eugene"),
                T("prologue.objectives1", "◆ Slave Training: Use <Mana-Infused Breast Milk Stimulant> to extract and ship 【Mana-Infused Milk】"),
                T("prologue.objectives2", "◆ Use various <Milking Machines> or schedule <Milking> to extract milk for shipping"),
                T("prologue.objectives3", "◆ Extracted milk is picked up by the Demon Realm Agricultural Cooperative staff"),
                T("prologue.objectives4", "◆ Feel free to handle the slave however you like")
            },
            // Page 5
            new[] {
                T("prologue.final1", "\"Well, I'm counting on you! There might be more requests once the samples are collected, so please take care.\" — Eugene"),
                T("prologue.final2", "\"Well then, I'll be off for now〜. Looks like there's something to look forward to, huh?\" — Mano"),
                T("prologue.final3", "And so, your new life as a rancher — with a side of slave training and milk production — begins in earnest."),
                T("prologue.arrival", "The next morning, the ranch wakes with you. There is work to assign, supplies to check, and a road into Okachi Town when you are ready.")
            }
        };

        var body = new VBoxContainer();
        body.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        body.SizeFlagsVertical = SizeFlags.ExpandFill;
        _content.AddChild(body);

        foreach (var line in pages[_prologuePage])
        {
            var label = new TypewriterLabel
            {
                Text = line,
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
            label.AddThemeColorOverride("font_color", Palette.BodyText);
            _prologueLines.Add(label);
            body.AddChild(label);
        }

        var actions = new HBoxContainer();
        actions.AddThemeConstantOverride("separation", 12);
        _content.AddChild(actions);

        var back = SecondaryButton(T("prologue.back", "Back"));
        back.Disabled = _prologuePage == 0;
        back.Pressed += () =>
        {
            if (FinishPrologueTyping())
            {
                return;
            }

            _prologuePage--;
            _game.Feedback.PlayConfirm();
            ShowProloguePage();
        };
        actions.AddChild(back);

        if (_prologuePage < pages.Length - 1)
        {
            var next = PrimaryButton(T("prologue.continue", "Continue"));
            next.Pressed += () =>
            {
                if (FinishPrologueTyping())
                {
                    return;
                }

                _prologuePage++;
                _game.Feedback.PlayConfirm();
                ShowProloguePage();
            };
            actions.AddChild(next);
        }
        else
        {
            var begin = PrimaryButton(T("prologue.begin", "Enter the Ranch"));
            begin.Pressed += () => { if (FinishPrologueTyping()) return; _game.Feedback.PlayConfirm(); ShowScreen("ranch"); };
            actions.AddChild(begin);
        }

        var skip = SecondaryButton(T("prologue.skip", "Skip"));
        skip.Pressed += () => { _game.Feedback.PlayConfirm(); ShowScreen("ranch"); };
        actions.AddChild(skip);
    }

    /// <summary>
    /// Completes any typewriter text that is still animating and reports whether one was skipped.
    /// Returns true (blocking navigation) when the player pressed to skip typing; a second press proceeds.
    /// </summary>
    private bool FinishPrologueTyping()
    {
        return FinishActiveTypewriting();
    }

    private static string MilestoneTriggerText(Core.Resources.MilestoneDefinition milestone)
    {
        return milestone.TriggerKind switch
        {
            Core.Resources.MilestoneTriggerKind.DayReached => $"{T("milestone.trigger.day", "Reach day")} {milestone.TriggerAmount}",
            Core.Resources.MilestoneTriggerKind.GoldReached => $"{T("milestone.trigger.gold", "Reach")} {milestone.TriggerAmount} {T("label.gold", "gold")}",
            Core.Resources.MilestoneTriggerKind.MissionCompleted => $"{T("milestone.trigger.mission", "Complete mission")} {milestone.TriggerId}",
            Core.Resources.MilestoneTriggerKind.BondReached => $"{T("milestone.trigger.bond", "Raise any bond to")} {milestone.TriggerAmount}",
            Core.Resources.MilestoneTriggerKind.ResearchUnlocked => milestone.TriggerId == "any" ? T("milestone.trigger.research_any", "Unlock any research") : $"{T("milestone.trigger.research", "Unlock research")} {milestone.TriggerId}",
            _ => T("milestone.trigger.unknown", "Unknown")
        };
    }

    private void RenderVictory()
    {
        AddTitle(T("screen.victory", "Victory!"));
        var card = CardContainer();
        _content.AddChild(card);

        card.AddChild(AddStyledLine("Ranch completed all objectives!"));
        card.AddChild(SubtitleLabel(T("victory.summary", "Summary")));
        int winDay = _game.State.VictoryDay ?? _game.State.Calendar.Day;
        card.AddChild(MutedLabel($"Day {winDay}: All missions discovered, all facilities maxed, all research unlocked, and bonds established."));

        if (_game.State.NgPlusActive)
        {
            card.AddChild(SubtitleLabel(T("victory.ngplus", "New Game+ Mode")));
            card.AddChild(MutedLabel("You are playing in New Game+ with bonus starting gold."));
        }

        var stats = CardContainer();
        _content.AddChild(stats);
        stats.AddChild(SubtitleLabel(T("victory.stats", "Ranch Statistics")));
        stats.AddChild(MutedLabel($"Gold: {_game.State.Economy.Gold}"));
        stats.AddChild(MutedLabel($"Characters: {_game.Roster.Characters.Count}"));
        stats.AddChild(MutedLabel($"Day Achieved: {winDay}"));
        stats.AddChild(MutedLabel($"Facilities: {_game.Ranch.Facilities.Count(f => f.Value >= 5)} / {_game.Data.Facilities.Count} maxed"));
        stats.AddChild(MutedLabel($"Missions: {_game.State.Adventure.DiscoveredMissionIds.Count} / {_game.Data.Missions.Count} discovered"));
        stats.AddChild(MutedLabel($"Research: {_game.State.Research.UnlockedSkillIds.Count} / {_game.Data.Skills.Count} unlocked"));

        var actions = new HBoxContainer();
        actions.AddThemeConstantOverride("separation", 12);
        _content.AddChild(actions);

        var continueBtn = PrimaryButton(T("victory.continue", "Continue Ranching"), T("victory.continue_hint", "Keep playing after victory. You can start New Game+ later from the main menu."));
        continueBtn.Pressed += () => { _game.Feedback.PlayConfirm(); ShowScreen("ranch"); };
        actions.AddChild(continueBtn);

        if (!_game.State.NgPlusActive)
        {
            var ngPlus = SecondaryButton(T("victory.new_game_plus", "New Game+"), T("victory.new_game_plus_hint", "Start a new game with bonus gold and carryover items"));
            ngPlus.Pressed += () =>
            {
                _game.StartNewGamePlus();
                _game.Feedback.PlayConfirm();
                ShowScreen("ranch");
            };
            actions.AddChild(ngPlus);
        }

        var title = SecondaryButton(T("victory.title_screen", "Title Screen"));
        title.Pressed += () => { _game.Feedback.PlayConfirm(); ShowScreen("title"); };
        actions.AddChild(title);
    }

    // === Milk Cow (Milk Processing) ===

    private void RenderMilkCow()
    {
        AddTitle(T("screen.milk_cow", "Milk Cow"));

        var chars = _game.Roster.Characters.ToList();
        if (!chars.Any())
        {
            _content.AddChild(MutedLabel("No characters available."));
            return;
        }

        var charPicker = StyledPicker(240);
        var selectorRow = FlowRow(8);
        _content.AddChild(selectorRow);
        selectorRow.AddChild(MutedLabel("Character"));
        for (var i = 0; i < chars.Count; i++)
        {
            charPicker.AddItem(CharacterPickerName(chars[i]));
            if (i == 0) charPicker.Selected = 0;
        }
        selectorRow.AddChild(charPicker);

        var character = chars[charPicker.Selected];
        if (character == null) character = chars[0];

        // MilkState: Capacity, Production, CurrentAmount, Quality, HasMilkConstitution, Concentration
        var milk = character.Milk;
        _content.AddChild(AddStyledLine($"Selected: {CharacterPickerName(character)}", true));
        _content.AddChild(AddStyledLine($"Milk Capacity: {milk.Capacity}", true));
        _content.AddChild(AddStyledLine($"Milk Production: {milk.Production}"));
        _content.AddChild(AddStyledLine($"Current Stock: {milk.CurrentAmount}"));
        _content.AddChild(AddStyledLine($"Milk Quality: {milk.Quality}%"));
        _content.AddChild(MutedLabel($"Constitution: {(milk.HasMilkConstitution ? "Yes" : "No")} {(milk.HasMagicMilkConstitution ? "+Magic" : "")}"));
        _content.AddChild(MutedLabel($"Concentration: {milk.Concentration}"));

        if (milk.CurrentAmount > 0)
        {
            var collectBtn = PrimaryButton("Collect Milk");
            collectBtn.Pressed += () =>
            {
                var amount = milk.CurrentAmount;
                _game.State.Inventory.Items["milk"] = _game.State.Inventory.Items.GetValueOrDefault("milk", 0) + amount;
                milk.CurrentAmount = 0;
                milk.TotalShipped += amount;
                _game.State.Economy.Gold += amount / 2;
                _game.NotifyStateChanged();
                _game.Feedback.PlayConfirm();
            };
            _content.AddChild(collectBtn);
        }

        // MilkCow corruption influence (from MentalState.MilkCow)
        var mental = character.Mature;
        if (mental != null)
        {
            var milkCowLevel = mental.MilkCow;
            if (milkCowLevel > 3000)
            {
                _content.AddChild(MutedLabel($"Milk Cow influence: {milkCowLevel}/20000"));
                _content.AddChild(MutedLabel($"Production boost: +{(milkCowLevel / 20000 * 50):F0}%"));
            }

            if (milkCowLevel > 8000)
            {
                _content.AddChild(AddStyledLine($"Corruption progress: {milkCowLevel / 200}%%", true));
            }
        }
    }

    // === Corruption Status (Fall State) ===

    private void RenderFallState()
    {
        AddTitle(T("screen.fall_state", "Corruption Status"));

        var chars = _game.Roster.Characters.ToList();
        if (!chars.Any())
        {
            _content.AddChild(MutedLabel("No characters available."));
            return;
        }

        foreach (var character in chars)
        {
            var definition = _game.Roster.DefinitionFor(character);
            var card = CardContainer();
            _content.AddChild(card);
            card.AddChild(SubtitleLabel(definition.DisplayName));

            var mental = character.Mature;
            if (mental == null)
            {
                card.AddChild(MutedLabel("No mental state data."));
                continue;
            }

            // Corruption level derived from Resistance + Dignity + Reason + MentalStrength
            var totalMental = mental.Resistance + mental.Dignity + mental.Reason + mental.MentalStrength;
            var maxMental = 40000;
            var corruptionPercent = Math.Max(0, (maxMental - totalMental) / maxMental * 100);

            card.AddChild(AddStyledLine($"Corruption Level: {corruptionPercent}% {mental.FallState}", true));
            card.AddChild(MutedLabel($"Resistance: {mental.Resistance}/10000"));
            card.AddChild(MutedLabel($"Dignity: {mental.Dignity}/10000"));
            card.AddChild(MutedLabel($"Submission: {mental.Submission}/20000"));
            card.AddChild(MutedLabel($"Lust: {mental.Lust}/20000"));
            card.AddChild(MutedLabel($"Milk Cow: {mental.MilkCow}/20000"));

            card.AddChild(MutedLabel($"Is Collapsed: {mental.IsCollapsed}"));
            card.AddChild(MutedLabel($"Is Brainwashed: {mental.IsBrainwashed}"));

            if (mental.Marks.Any())
            {
                card.AddChild(AddStyledLine($"Marks: {string.Join(", ", mental.Marks)}", true));
            }

            if (mental.FallState != Core.Models.FallState.Normal)
            {
                var recoveryBtn = SecondaryButton("Recovery Training");
                recoveryBtn.Pressed += () =>
                {
                    var effects = new Gameplay.MentalStateEffects
                    {
                        ResistanceDelta = 500,
                        DignityDelta = 500,
                        ReasonDelta = 300,
                        MentalStrengthDelta = 400,
                        ObedienceDelta = -200,
                        LustDelta = -100
                    };
                    _game.MentalState.ApplyEffects(character, effects);
                    _game.NotifyStateChanged();
                    _game.Feedback.PlayConfirm();
                    ShowScreen("mental");
                };
                card.AddChild(recoveryBtn);
            }
        }
    }


    // ==================== Clothing Screens ====================

    private void RenderClothingList()
    {
        ClearContent();
        UpdateTopBar();
        AddTitle(T("screen.clothing.title", "Clothing"));

        if (_detailCharacterId.Length == 0)
        {
            _content.AddChild(MutedLabel(T("screen.clothing.no_character", "No character selected.")));
            return;
        }

        var character = _game.State.Roster.Characters.FirstOrDefault(c => c.Id == _detailCharacterId);
        if (character is null)
        {
            _content.AddChild(MutedLabel(T("screen.clothing.character_not_found", "Character not found.")));
            return;
        }

        var equippedCard = CardContainer();
        _content.AddChild(equippedCard);
        var equippedInner = CardContent();
        equippedCard.AddChild(equippedInner);
        equippedInner.AddChild(SubtitleLabel(T("screen.clothing.equipped", "Currently Equipped")));

        if (character.EquippedItems is null || character.EquippedItems.Count == 0)
        {
            equippedInner.AddChild(MutedLabel(T("screen.clothing.equipped_empty", "Nothing equipped.")));
        }
        else
        {
            foreach (var slotAndItem in character.EquippedItems)
            {
                var itemDef = _game.Data.Items.TryGetValue(slotAndItem.Value, out var item) ? item.DisplayName : slotAndItem.Value;
                equippedInner.AddChild(AddStyledLine(slotAndItem.Key + ": " + itemDef));
            }
        }

        var availableCard = CardContainer();
        _content.AddChild(availableCard);
        var availableInner = CardContent();
        availableCard.AddChild(availableInner);
        availableInner.AddChild(SubtitleLabel(T("screen.clothing.available", "Available Clothing")));

        var clothingItems = _game.Data.Items.Values
            .Where(item => item.Category == OpenMakaiRanch.Core.Resources.ItemCategory.Equipment)
            .ToList();

        if (clothingItems.Count == 0)
        {
            availableInner.AddChild(MutedLabel(T("screen.clothing.no_clothing", "No clothing items available.")));
        }
        else
        {
            foreach (var item in clothingItems)
            {
                var isEquipped = character.EquippedItems is not null && character.EquippedItems.Any(e => e.Value == item.Id);

                if (!isEquipped)
                {
                    var equipBtn = PrimaryButton(T("screen.clothing.equip", "Equip"), "Equip " + item.DisplayName);
                    equipBtn.Disabled = _game.Economy.Gold < item.Price;
                    var capturedItem = item;
                    var capturedChar = character;
                    equipBtn.Pressed += () =>
                    {
                        ExecuteUiAction(() =>
                        {
                            var successResult = _game.Clothing.EquipItem(capturedChar, capturedItem.Id);
                            if (successResult.Success)
                            {
                                _game.NotifyStateChanged();
                                _game.Feedback.PlayConfirm();
                                ShowScreen("clothing_change");
                            }
                            else
                            {
                                _game.Feedback.PlayError();
                                SetStatus(successResult.Error, true);
                            }
                        }, true, "clothing_change");
                    };
                    availableInner.AddChild(equipBtn);
                    if (equipBtn.Disabled)
                    {
                        availableInner.AddChild(RequirementLabel("Need " + (capturedItem.Price - _game.Economy.Gold) + "g"));
                    }
                }
                else
                {
                    var unequipBtn = SecondaryButton(T("screen.clothing.unequip", "Unequip"), "Remove " + item.DisplayName);
                    var capturedSlotKey = item.Slot.ToString();
                    var capturedChar2 = character;
                    unequipBtn.Pressed += () =>
                    {
                        ExecuteUiAction(() =>
                        {
                            if (Enum.TryParse<OpenMakaiRanch.Core.Resources.EquipmentSlot>(capturedSlotKey, true, out var slot))
                            {
                                var successResult = _game.Clothing.UnequipItem(capturedChar2, slot);
                                if (successResult.Success)
                                {
                                    _game.NotifyStateChanged();
                                    _game.Feedback.PlayConfirm();
                                    ShowScreen("clothing_change");
                                }
                                else
                                {
                                    _game.Feedback.PlayError();
                                    SetStatus(successResult.Error, true);
                                }
                            }
                        }, true, "clothing_change");
                    };
                    availableInner.AddChild(unequipBtn);
                }
            }
        }

        var backBtn = SecondaryButton(T("label.back", "Back"), T("tooltip.clothing_back", "Return to ranch overview"));
        backBtn.Pressed += () => { _game.Feedback.PlayConfirm(); ShowScreen("ranch"); };
        _content.AddChild(backBtn);
    }

    private void RenderClothingChange()
    {
        ClearContent();
        UpdateTopBar();
        AddTitle(T("screen.clothing.change_title", "Change Clothing"));

        if (_detailCharacterId.Length == 0)
        {
            _content.AddChild(MutedLabel(T("screen.clothing.no_character", "No character selected.")));
            return;
        }

        var character = _game.State.Roster.Characters.FirstOrDefault(c => c.Id == _detailCharacterId);
        if (character is null)
        {
            _content.AddChild(MutedLabel(T("screen.clothing.character_not_found", "Character not found.")));
            return;
        }

        var definition = _game.Roster.DefinitionFor(character);

        var outfitCard = CardContainer();
        _content.AddChild(outfitCard);
        var outfitInner = CardContent();
        outfitCard.AddChild(outfitInner);
        outfitInner.AddChild(SubtitleLabel(T("screen.clothing.outfit", "Current Outfit")));

        if (character.EquippedItems is null || character.EquippedItems.Count == 0)
        {
            outfitInner.AddChild(MutedLabel(T("screen.clothing.outfit_empty", "Nothing equipped.")));
        }
        else
        {
            var bonuses = _game.Clothing.GetTotalBonuses(character);
            foreach (var slotAndItem in character.EquippedItems)
            {
                var itemDef = _game.Data.Items.TryGetValue(slotAndItem.Value, out var item) ? item.DisplayName : slotAndItem.Value;
                outfitInner.AddChild(AddStyledLine(slotAndItem.Key + ": " + itemDef + " | Ranch +" + bonuses.Item1 + " Craft +" + bonuses.Item2 + " Combat +" + bonuses.Item3));
            }
        }

        var visualCard = CardContainer();
        _content.AddChild(visualCard);
        var portrait = BuildCharacterVisual(character, definition);
        if (portrait is not null) visualCard.AddChild(portrait);

        var changeCard = CardContainer();
        _content.AddChild(changeCard);
        var changeInner = CardContent();
        changeCard.AddChild(changeInner);
        changeInner.AddChild(SubtitleLabel(T("screen.clothing.available_items", "Available Items")));

        var availableItems = _game.Data.Items.Values
            .Where(item => item.Category == OpenMakaiRanch.Core.Resources.ItemCategory.Equipment)
            .ToList();

        if (availableItems.Count == 0)
        {
            changeInner.AddChild(MutedLabel(T("screen.clothing.no_items", "No items available.")));
        }
        else
        {
            foreach (var item in availableItems)
            {
                var isEquipped = character.EquippedItems is not null && character.EquippedItems.Any(e => e.Value == item.Id);
                var itemCard = CardContainer();
                itemCard.SizeFlagsHorizontal = SizeFlags.ExpandFill;
                _content.AddChild(itemCard);
                var itemInner = CardContent();
                itemCard.AddChild(itemInner);
                itemInner.AddChild(SubtitleLabel(item.DisplayName + (isEquipped ? " (Equipped)" : "")));
                itemInner.AddChild(MutedLabel(item.Description));

                if (!isEquipped)
                {
                    var equipBtn = PrimaryButton(T("screen.clothing.equip", "Equip"), "Equip " + item.DisplayName);
                    equipBtn.Disabled = _game.Economy.Gold < item.Price;
                    var capturedItem2 = item;
                    var capturedChar3 = character;
                    equipBtn.Pressed += () =>
                    {
                        ExecuteUiAction(() =>
                        {
                            var successResult = _game.Clothing.EquipItem(capturedChar3, capturedItem2.Id);
                            if (successResult.Success)
                            {
                                _game.NotifyStateChanged();
                                _game.Feedback.PlayConfirm();
                                ShowScreen("clothing_change");
                            }
                            else
                            {
                                _game.Feedback.PlayError();
                                SetStatus(successResult.Error, true);
                            }
                        }, true, "clothing_change");
                    };
                    itemInner.AddChild(equipBtn);
                    if (equipBtn.Disabled)
                    {
                        itemInner.AddChild(RequirementLabel("Need " + (capturedItem2.Price - _game.Economy.Gold) + "g"));
                    }
                }
            }
        }

        var backBtn = SecondaryButton(T("label.back", "Back"), T("tooltip.clothing_back", "Return to clothing list"));
        backBtn.Pressed += () => { _game.Feedback.PlayConfirm(); ShowScreen("clothing_list"); };
        _content.AddChild(backBtn);
    }

    private void RenderClothingStrip()
    {
        ClearContent();
        UpdateTopBar();
        AddTitle(T("screen.clothing.strip_title", "Remove Clothing"));

        if (_detailCharacterId.Length == 0)
        {
            _content.AddChild(MutedLabel(T("screen.clothing.no_character", "No character selected.")));
            return;
        }

        var character = _game.State.Roster.Characters.FirstOrDefault(c => c.Id == _detailCharacterId);
        if (character is null)
        {
            _content.AddChild(MutedLabel(T("screen.clothing.character_not_found", "Character not found.")));
            return;
        }

        var definition = _game.Roster.DefinitionFor(character);

        var outfitCard = CardContainer();
        _content.AddChild(outfitCard);
        var outfitInner = CardContent();
        outfitCard.AddChild(outfitInner);
        outfitInner.AddChild(SubtitleLabel(T("screen.clothing.current_outfit", "Current Outfit")));

        if (character.EquippedItems is null || character.EquippedItems.Count == 0)
        {
            outfitInner.AddChild(MutedLabel(T("screen.clothing.no_equipment", "No equipment equipped.")));
            outfitInner.AddChild(AddStyledLine(T("screen.clothing.already_stripped", "Character has nothing to remove.")));
        }
        else
        {
            foreach (var slotAndItem in character.EquippedItems)
            {
                var itemDef = _game.Data.Items.TryGetValue(slotAndItem.Value, out var item) ? item.DisplayName : slotAndItem.Value;
                var itemCard = CardContainer();
                itemCard.SizeFlagsHorizontal = SizeFlags.ExpandFill;
                _content.AddChild(itemCard);
                var itemInner = CardContent();
                itemCard.AddChild(itemInner);
                itemInner.AddChild(SubtitleLabel(slotAndItem.Key + ": " + itemDef));

                var removeBtn = PrimaryButton(T("screen.clothing.remove", "Remove"), "Remove " + itemDef);
                var capturedItem3 = item;
                var capturedSlotKey2 = slotAndItem.Key;
                var capturedChar4 = character;
                removeBtn.Pressed += () =>
                {
                    ExecuteUiAction(() =>
                    {
                        if (Enum.TryParse<OpenMakaiRanch.Core.Resources.EquipmentSlot>(capturedSlotKey2, true, out var slot2))
                        {
                            var successResult = _game.Clothing.UnequipItem(capturedChar4, slot2);
                            if (successResult.Success)
                            {
                                _game.NotifyStateChanged();
                                _game.Feedback.PlayConfirm();
                                ShowScreen("clothing_change");
                            }
                            else
                            {
                                _game.Feedback.PlayError();
                                SetStatus(successResult.Error, true);
                            }
                        }
                    }, true, "clothing_change");
                };
                itemInner.AddChild(removeBtn);
            }
        }

        var visualCard = CardContainer();
        _content.AddChild(visualCard);
        var portrait2 = BuildCharacterVisual(character, definition);
        if (portrait2 is not null) visualCard.AddChild(portrait2);

        var backBtn = SecondaryButton(T("label.back", "Back"), T("tooltip.clothing_back", "Return to clothing list"));
        backBtn.Pressed += () => { _game.Feedback.PlayConfirm(); ShowScreen("clothing_list"); };
        _content.AddChild(backBtn);
    }

    private void RenderRoomAssign()
    {
        ClearContent();
        UpdateTopBar();
        AddTitle(T("screen.room.title", "Room Assignment"));

        var buildingsCard = CardContainer();
        _content.AddChild(buildingsCard);
        var buildingsInner = CardContent();
        buildingsCard.AddChild(buildingsInner);
        buildingsInner.AddChild(SubtitleLabel(T("screen.room.buildings", "Available Buildings")));

        foreach (var buildingId in LivingBuildingIds)
        {
            var cap = BuildingCapacities.TryGetValue(buildingId, out var capacity) ? capacity : 2;
            var isBuilt = buildingId switch
            {
                "office" => true,
                "private_room" => true,
                "barn" => true,
                "dormitory" => true,
                _ => _game.State.Ranch.Facilities.TryGetValue(buildingId, out var level) && level > 0
            };
            var nameDef = _game.Data.Facilities.TryGetValue(buildingId, out var facDef) ? facDef.DisplayName : buildingId;

            var buildingRow = CardContainer();
            buildingRow.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _content.AddChild(buildingRow);
            var buildingInner = CardContent();
            buildingRow.AddChild(buildingInner);
            buildingInner.AddChild(SubtitleLabel(nameDef));
            buildingInner.AddChild(MutedLabel("Capacity: " + cap + " | " + (isBuilt ? T("screen.room.open", "Open") : T("screen.room.locked", "Locked"))));

            if (!isBuilt && facDef is not null)
            {
                var cost = _game.Ranch.FacilityUpgradeCost(facDef, 0);
                var buildBtn = PrimaryButton("Build (" + cost + "g)");
                buildBtn.Disabled = _game.Economy.Gold < cost;
                var capturedBldId = buildingId;
                var capturedBldFac = facDef;
                buildBtn.Pressed += () =>
                {
                    ExecuteUiAction(() =>
                    {
                        if (_game.Ranch.UpgradeFacility(capturedBldId, _game.Economy))
                        {
                            _game.NotifyStateChanged();
                            _game.Feedback.PlayConfirm();
                            ShowScreen("room_assign");
                        }
                        else
                        {
                            _game.Feedback.PlayError();
                            SetStatus(T("screen.room.build_failed", "Building failed."), true);
                        }
                    }, true, "room_assign");
                };
                buildingInner.AddChild(buildBtn);
                if (buildBtn.Disabled)
                {
                    buildingInner.AddChild(RequirementLabel("Need " + (cost - _game.Economy.Gold) + "g"));
                }
            }
        }

        var assignmentCard = CardContainer();
        _content.AddChild(assignmentCard);
        var assignmentInner = CardContent();
        assignmentCard.AddChild(assignmentInner);
        assignmentInner.AddChild(SubtitleLabel(T("screen.room.assignments", "Current Assignments")));

        foreach (var character in _game.Roster.Characters)
        {
            var jobId = _game.Schedule.GetAssignment(character.Id);
            var jobName = _game.Data.Jobs.TryGetValue(jobId, out var jobDef) ? jobDef.DisplayName : jobId;
            var def = _game.Roster.DefinitionFor(character);
            var assignmentRow = CardContainer();
            assignmentRow.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _content.AddChild(assignmentRow);
            var assignmentRowInner = CardContent();
            assignmentRow.AddChild(assignmentRowInner);
            assignmentRowInner.AddChild(SubtitleLabel(def.DisplayName));
            assignmentRowInner.AddChild(MutedLabel("Current Job: " + jobName));

            var assignBtn = SecondaryButton(T("screen.room.assign", "Assign"), T("tooltip.room_assign", "Assign this character to a different job"));
            var capturedCharId = character.Id;
            assignBtn.Pressed += () => { _detailCharacterId = capturedCharId; ShowScreen("schedule"); };
            assignmentRowInner.AddChild(assignBtn);
        }

        var backBtn = SecondaryButton(T("label.back", "Back"), T("tooltip.room_back", "Return to ranch overview"));
        backBtn.Pressed += () => { _game.Feedback.PlayConfirm(); ShowScreen("ranch"); };
        _content.AddChild(backBtn);
    }

    private void RenderOptions()
    {
        ClearContent();
        UpdateTopBar();
        AddTitle(T("screen.options.title", "Options"));

        var settings = _game.State.Settings;

        var uiCard = CardContainer();
        _content.AddChild(uiCard);
        var uiInner = CardContent();
        uiCard.AddChild(uiInner);
        uiInner.AddChild(SubtitleLabel(T("screen.options.ui", "UI Settings")));

        uiInner.AddChild(AddStyledLine("UI Scale: " + (settings.UiScale * 100).ToString("F0") + "%"));
        var scaleUpBtn = PrimaryButton(T("screen.options.scale_up", "Scale Up"));
        // RuntimeSettings owns viewport scaling; scaling this panel as well applies it twice.
        scaleUpBtn.Pressed += () => ExecuteUiAction(
            () => _game.SetUiScale(_game.State.Settings.UiScale + 0.1f), true);
        uiInner.AddChild(scaleUpBtn);

        var scaleDownBtn = SecondaryButton(T("screen.options.scale_down", "Scale Down"));
        scaleDownBtn.Pressed += () => ExecuteUiAction(
            () => _game.SetUiScale(_game.State.Settings.UiScale - 0.1f), true);
        uiInner.AddChild(scaleDownBtn);

        var dataCard = CardContainer();
        _content.AddChild(dataCard);
        var dataInner = CardContent();
        dataCard.AddChild(dataInner);
        dataInner.AddChild(SubtitleLabel(T("screen.options.data", "Data & Save")));

        var exportBtn = PrimaryButton(T("screen.options.export", "Export Save Data"), T("tooltip.export", "Export the current save as JSON"));
        exportBtn.Pressed += () =>
        {
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(_game.State, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                var exportDir = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? System.IO.Directory.GetCurrentDirectory(), "exports");
                System.IO.Directory.CreateDirectory(exportDir);
                var exportPath = System.IO.Path.Combine(exportDir, "save_day" + _game.State.Calendar.Day + ".json");
                System.IO.File.WriteAllText(exportPath, json);
                _game.NotifyStateChanged();
                _game.Feedback.PlayConfirm();
                SetStatus("Save exported to: " + exportPath, true);
            }
            catch (System.Exception ex)
            {
                _game.Feedback.PlayError();
                SetStatus("Export failed: " + ex.Message, true);
            }
        };
        dataInner.AddChild(exportBtn);

        var backBtn = SecondaryButton(T("label.back", "Back"), T("tooltip.options_back", "Return to ranch overview"));
        backBtn.Pressed += () => { _game.Feedback.PlayConfirm(); ShowScreen("ranch"); };
        _content.AddChild(backBtn);
    }

    private void RenderAbility()
    {
        ClearContent();
        UpdateTopBar();
        AddTitle(T("screen.ability.title", "Abilities & Talents"));

        if (_detailCharacterId.Length == 0)
        {
            _content.AddChild(MutedLabel(T("screen.ability.no_character", "No character selected.")));
            return;
        }

        var character = _game.State.Roster.Characters.FirstOrDefault(c => c.Id == _detailCharacterId);
        if (character is null)
        {
            _content.AddChild(MutedLabel(T("screen.ability.character_not_found", "Character not found.")));
            return;
        }

        var definition = _game.Roster.DefinitionFor(character);

        var talentsCard = CardContainer();
        _content.AddChild(talentsCard);
        var talentsInner = CardContent();
        talentsCard.AddChild(talentsInner);
        talentsInner.AddChild(SubtitleLabel(T("screen.ability.talents", "Talents")));

        if (definition.Talents is null || definition.Talents.Count == 0)
        {
            talentsInner.AddChild(MutedLabel(T("screen.ability.no_talents", "No talents defined.")));
        }
        else
        {
            foreach (var talentId in definition.Talents)
            {
                var talentDef = _game.Data.Talents.TryGetValue(talentId, out var talent) ? talent : null;
                var talentName = talentDef != null ? talentDef.DisplayName : talentId;
                var talentDesc = talentDef != null ? talentDef.Description : "";
                var talentRow = CardContainer();
                talentRow.SizeFlagsHorizontal = SizeFlags.ExpandFill;
                _content.AddChild(talentRow);
                var talentRowInner = CardContent();
                talentRow.AddChild(talentRowInner);
                talentRowInner.AddChild(SubtitleLabel(talentName));
                talentRowInner.AddChild(MutedLabel(talentDesc));
            }
        }

        var skillsCard = CardContainer();
        _content.AddChild(skillsCard);
        var skillsInner = CardContent();
        skillsCard.AddChild(skillsInner);
        skillsInner.AddChild(SubtitleLabel(T("screen.ability.skills", "Skills")));

        var effectiveRanch = character.RanchSkill + _game.Equipment.BonusRanchSkill(_detailCharacterId);
        var effectiveCraft = character.CraftSkill + _game.Equipment.BonusCraftSkill(_detailCharacterId);
        var effectiveCombat = character.CombatSkill + _game.Equipment.BonusCombatSkill(_detailCharacterId);

        skillsInner.AddChild(AddStyledLine("Ranch: " + character.RanchSkill + " (" + effectiveRanch + " effective)"));
        skillsInner.AddChild(AddStyledLine("Craft: " + character.CraftSkill + " (" + effectiveCraft + " effective)"));
        skillsInner.AddChild(AddStyledLine("Combat: " + character.CombatSkill + " (" + effectiveCombat + " effective)"));

        var backBtn = SecondaryButton(T("label.back", "Back"), T("tooltip.ability_back", "Return to ranch overview"));
        backBtn.Pressed += () => { _game.Feedback.PlayConfirm(); ShowScreen("ranch"); };
        _content.AddChild(backBtn);
    }

    private void RenderPharmacyList()
    {
        ClearContent();
        UpdateTopBar();
        AddTitle(T("screen.pharmacy.title", "Pharmacy"));

        var itemsCard = CardContainer();
        _content.AddChild(itemsCard);
        var itemsInner = CardContent();
        itemsCard.AddChild(itemsInner);
        itemsInner.AddChild(SubtitleLabel(T("screen.pharmacy.items", "Available Items")));

        var consumables = _game.Data.Items.Values
            .Where(item => item.Category == OpenMakaiRanch.Core.Resources.ItemCategory.Consumable || item.Category == OpenMakaiRanch.Core.Resources.ItemCategory.Material)
            .ToList();

        if (consumables.Count == 0)
        {
            itemsInner.AddChild(MutedLabel(T("screen.pharmacy.no_items", "No pharmacy items available.")));
        }
        else
        {
            foreach (var item in consumables)
            {
                var itemCard = CardContainer();
                itemCard.SizeFlagsHorizontal = SizeFlags.ExpandFill;
                _content.AddChild(itemCard);
                var itemInner = CardContent();
                itemCard.AddChild(itemInner);
                itemInner.AddChild(SubtitleLabel(item.DisplayName));
                itemInner.AddChild(MutedLabel(item.Description));
                itemInner.AddChild(MutedLabel("Price: " + item.Price + "g"));
            }
        }

        var inventoryCard = CardContainer();
        _content.AddChild(inventoryCard);
        var inventoryInner = CardContent();
        inventoryCard.AddChild(inventoryInner);
        inventoryInner.AddChild(SubtitleLabel(T("screen.pharmacy.inventory", "Current Inventory")));

        var inventory = _game.Inventory.Items;
        if (inventory is null || inventory.Count == 0)
        {
            inventoryInner.AddChild(MutedLabel(T("screen.pharmacy.empty_inventory", "Inventory is empty.")));
        }
        else
        {
            foreach (var kvp in inventory)
            {
                var itemDef = _game.Data.Items.TryGetValue(kvp.Key, out var item) ? item.DisplayName : kvp.Key;
                inventoryInner.AddChild(AddStyledLine(itemDef + ": x" + kvp.Value));
            }
        }

        var backBtn = SecondaryButton(T("label.back", "Back"), T("tooltip.pharmacy_back", "Return to ranch overview"));
        backBtn.Pressed += () => { _game.Feedback.PlayConfirm(); ShowScreen("ranch"); };
        _content.AddChild(backBtn);
    }

    private void RenderPharmacyCraft()
    {
        ClearContent();
        UpdateTopBar();
        AddTitle(T("screen.pharmacy.craft_title", "Craft Items"));

        var materials = _game.Data.Items.Values
            .Where(item => item.Category == OpenMakaiRanch.Core.Resources.ItemCategory.Material)
            .ToList();

        var craftCard = CardContainer();
        _content.AddChild(craftCard);
        var craftInner = CardContent();
        craftCard.AddChild(craftInner);
        craftInner.AddChild(SubtitleLabel(T("screen.pharmacy.craftable", "Craftable Items")));

        if (materials.Count == 0)
        {
            craftInner.AddChild(MutedLabel(T("screen.pharmacy.no_materials", "No craftable materials available.")));
        }
        else
        {
            foreach (var material in materials)
            {
                var craftRow = CardContainer();
                craftRow.SizeFlagsHorizontal = SizeFlags.ExpandFill;
                _content.AddChild(craftRow);
                var craftRowInner = CardContent();
                craftRow.AddChild(craftRowInner);
                craftRowInner.AddChild(SubtitleLabel(material.DisplayName));
                craftRowInner.AddChild(MutedLabel(material.Description));

                var craftBtn = PrimaryButton(T("screen.pharmacy.craft", "Craft"), "Craft " + material.DisplayName);
                craftBtn.Disabled = _game.Economy.Gold < material.Price;
                var capturedMat = material;
                craftBtn.Pressed += () =>
                {
                    ExecuteUiAction(() =>
                    {
                        if (_game.Economy.Spend(capturedMat.Price))
                        {
                            _game.Inventory.AddItem(capturedMat.Id, 1);
                            _game.NotifyStateChanged();
                            _game.Feedback.PlayConfirm();
                            ShowScreen("pharmacy_craft");
                        }
                        else
                        {
                            _game.Feedback.PlayError();
                            SetStatus(T("screen.pharmacy.craft_failed", "Crafting failed — not enough gold."), true);
                        }
                    }, true, "pharmacy_craft");
                };
                craftRowInner.AddChild(craftBtn);
                if (craftBtn.Disabled)
                {
                    craftRowInner.AddChild(RequirementLabel("Need " + (capturedMat.Price - _game.Economy.Gold) + "g"));
                }
            }
        }

        var backBtn = SecondaryButton(T("label.back", "Back"), T("tooltip.pharmacy_back", "Return to pharmacy list"));
        backBtn.Pressed += () => { _game.Feedback.PlayConfirm(); ShowScreen("pharmacy_list"); };
        _content.AddChild(backBtn);
    }

    private void RenderMagicBasic()
    {
        ClearContent();
        UpdateTopBar();
        AddTitle(T("screen.magic.title", "Magic"));
        AddManaResourceCard("magic_basic");

        var spells = _game.Data.Spells.Values.ToList();

        if (spells.Count == 0)
        {
            _content.AddChild(MutedLabel(T("screen.magic.no_spells", "No spells available.")));
            return;
        }

        var spellsCard = CardContainer();
        _content.AddChild(spellsCard);
        var spellsInner = CardContent();
        spellsCard.AddChild(spellsInner);
        spellsInner.AddChild(SubtitleLabel(T("screen.magic.spells", "Available Spells")));

        foreach (var spell in spells)
        {
            var spellCard = CardContainer();
            spellCard.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _content.AddChild(spellCard);
            var spellInner = CardContent();
            spellCard.AddChild(spellInner);
            spellInner.AddChild(SubtitleLabel(spell.DisplayName));
            spellInner.AddChild(MutedLabel(spell.Description));
            spellInner.AddChild(MutedLabel("Type: " + spell.Type));
            spellInner.AddChild(MutedLabel("Cost: " + spell.ManaCost + " personal MP"));

            var castBtn = PrimaryButton(T("screen.magic.cast", "Cast"), "Cast " + spell.DisplayName);
            castBtn.Disabled = !_game.Magic.CanSpendPlayerMana(spell.ManaCost);
            var capturedSpell = spell;
            castBtn.Pressed += () =>
            {
                ExecuteUiAction(() =>
                {
                    if (_game.CastPlayerSpell(capturedSpell.Id, capturedSpell.ManaCost))
                    {
                        _game.Feedback.PlayConfirm();
                        SetStatus("Cast: " + capturedSpell.DisplayName + " - " + capturedSpell.EffectDescription, true);
                        ShowScreen("magic_basic");
                    }
                    else
                    {
                        _game.Feedback.PlayError();
                        SetStatus("Need " + Math.Max(0, capturedSpell.ManaCost - _game.Magic.CurrentMana) + " personal MP", true);
                    }
                }, true, "magic_basic");
            };
            spellInner.AddChild(castBtn);
            if (castBtn.Disabled)
            {
                spellInner.AddChild(RequirementLabel("Need " + Math.Max(0, capturedSpell.ManaCost - _game.Magic.CurrentMana) + " personal MP"));
            }
        }

        var backBtn = SecondaryButton(T("label.back", "Back"), T("tooltip.magic_back", "Return to ranch overview"));
        backBtn.Pressed += () => { _game.Feedback.PlayConfirm(); ShowScreen("ranch"); };
        _content.AddChild(backBtn);
    }

    private void RenderMagicForbidden()
    {
        ClearContent();
        UpdateTopBar();
        AddTitle(T("screen.magic.forbidden_title", "Magic — Restricted Spells"));
        AddManaResourceCard("magic_forbidden");

        var spells = _game.Data.Spells.Values.ToList();

        if (spells.Count == 0)
        {
            _content.AddChild(MutedLabel(T("screen.magic.no_spells", "No spells available.")));
            return;
        }

        var spellsCard = CardContainer();
        _content.AddChild(spellsCard);
        var spellsInner = CardContent();
        spellsCard.AddChild(spellsInner);
        spellsInner.AddChild(SubtitleLabel(T("screen.magic.spells", "Available Spells")));

        foreach (var spell in spells)
        {
            var spellCard = CardContainer();
            spellCard.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _content.AddChild(spellCard);
            var spellInner = CardContent();
            spellCard.AddChild(spellInner);
            spellInner.AddChild(SubtitleLabel(spell.DisplayName));
            spellInner.AddChild(MutedLabel(spell.Description));
            spellInner.AddChild(MutedLabel("Type: " + spell.Type));
            spellInner.AddChild(MutedLabel("Cost: " + spell.ManaCost + " personal MP"));

            var castBtn = PrimaryButton(T("screen.magic.cast", "Cast"), "Cast " + spell.DisplayName);
            castBtn.Disabled = !_game.Magic.CanSpendPlayerMana(spell.ManaCost);
            var capturedSpell2 = spell;
            castBtn.Pressed += () =>
            {
                ExecuteUiAction(() =>
                {
                    if (_game.CastPlayerSpell(capturedSpell2.Id, capturedSpell2.ManaCost))
                    {
                        _game.Feedback.PlayConfirm();
                        SetStatus("Cast: " + capturedSpell2.DisplayName + " - " + capturedSpell2.EffectDescription, true);
                        ShowScreen("magic_forbidden");
                    }
                    else
                    {
                        _game.Feedback.PlayError();
                        SetStatus("Need " + Math.Max(0, capturedSpell2.ManaCost - _game.Magic.CurrentMana) + " personal MP", true);
                    }
                }, true, "magic_forbidden");
            };
            spellInner.AddChild(castBtn);
            if (castBtn.Disabled)
            {
                spellInner.AddChild(RequirementLabel("Need " + Math.Max(0, capturedSpell2.ManaCost - _game.Magic.CurrentMana) + " personal MP"));
            }
        }

        var backBtn = SecondaryButton(T("label.back", "Back"), T("tooltip.magic_back", "Return to ranch overview"));
        backBtn.Pressed += () => { _game.Feedback.PlayConfirm(); ShowScreen("ranch"); };
        _content.AddChild(backBtn);
    }

    private void RenderMagicTentacle()
    {
        ClearContent();
        UpdateTopBar();
        AddTitle(T("screen.magic.tentacle_title", "Magic — Special Spells"));
        AddManaResourceCard("magic_tentacle");

        var spells = _game.Data.Spells.Values.ToList();

        if (spells.Count == 0)
        {
            _content.AddChild(MutedLabel(T("screen.magic.no_spells", "No spells available.")));
            return;
        }

        var spellsCard = CardContainer();
        _content.AddChild(spellsCard);
        var spellsInner = CardContent();
        spellsCard.AddChild(spellsInner);
        spellsInner.AddChild(SubtitleLabel(T("screen.magic.spells", "Available Spells")));

        foreach (var spell in spells)
        {
            var spellCard = CardContainer();
            spellCard.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _content.AddChild(spellCard);
            var spellInner = CardContent();
            spellCard.AddChild(spellInner);
            spellInner.AddChild(SubtitleLabel(spell.DisplayName));
            spellInner.AddChild(MutedLabel(spell.Description));
            spellInner.AddChild(MutedLabel("Type: " + spell.Type));
            spellInner.AddChild(MutedLabel("Cost: " + spell.ManaCost + " personal MP"));

            var castBtn = PrimaryButton(T("screen.magic.cast", "Cast"), "Cast " + spell.DisplayName);
            castBtn.Disabled = !_game.Magic.CanSpendPlayerMana(spell.ManaCost);
            var capturedSpell3 = spell;
            castBtn.Pressed += () =>
            {
                ExecuteUiAction(() =>
                {
                    if (_game.CastPlayerSpell(capturedSpell3.Id, capturedSpell3.ManaCost))
                    {
                        _game.Feedback.PlayConfirm();
                        SetStatus("Cast: " + capturedSpell3.DisplayName + " - " + capturedSpell3.EffectDescription, true);
                        ShowScreen("magic_tentacle");
                    }
                    else
                    {
                        _game.Feedback.PlayError();
                        SetStatus("Need " + Math.Max(0, capturedSpell3.ManaCost - _game.Magic.CurrentMana) + " personal MP", true);
                    }
                }, true, "magic_tentacle");
            };
            spellInner.AddChild(castBtn);
            if (castBtn.Disabled)
            {
                spellInner.AddChild(RequirementLabel("Need " + Math.Max(0, capturedSpell3.ManaCost - _game.Magic.CurrentMana) + " personal MP"));
            }
        }

        var backBtn = SecondaryButton(T("label.back", "Back"), T("tooltip.magic_back", "Return to ranch overview"));
        backBtn.Pressed += () => { _game.Feedback.PlayConfirm(); ShowScreen("ranch"); };
        _content.AddChild(backBtn);
    }

    private void AddManaResourceCard(string returnScreen)
    {
        var magic = _game.Magic;
        var card = CardContainer();
        _content.AddChild(card);
        var inner = CardContent();
        card.AddChild(inner);
        inner.AddChild(SubtitleLabel("Mana Resources"));
        inner.AddChild(MutedLabel($"Personal MP: {magic.CurrentMana:N0}/{magic.MaxMana:N0}  •  Rest recovery: {_game.State.Player.ManaRecoveryPercent}%"));

        if (magic.StorageCapacity > 0)
        {
            inner.AddChild(MutedLabel($"Stored Mana: {magic.StoredMana:N0}/{magic.StorageCapacity:N0} MP"));
        }
        else
        {
            inner.AddChild(MutedLabel("Stored Mana: no reservoir installed"));
        }

        var refill = SecondaryButton("Replenish Personal MP", "Use the Magic Supply Device to move Stored Mana into personal MP without increasing Max MP.");
        refill.Disabled = !magic.HasManaSupplyDevice || magic.StoredMana <= 0 || magic.CurrentMana >= magic.MaxMana;
        refill.Pressed += () =>
        {
            ExecuteUiAction(() =>
            {
                var transferred = _game.RechargePlayerManaFromStorage();
                if (transferred > 0)
                {
                    _game.Feedback.PlayConfirm();
                    SetStatus($"Replenished {transferred:N0} personal MP from Stored Mana.", true);
                }
                else
                {
                    _game.Feedback.PlayError();
                    SetStatus(magic.HasManaSupplyDevice
                        ? "No Stored Mana can be transferred right now."
                        : "A Magic Supply Device is required to replenish personal MP from the reservoir.", true);
                }
                ShowScreen(returnScreen);
            }, true, returnScreen);
        };
        inner.AddChild(refill);

        if (!magic.HasManaSupplyDevice)
        {
            inner.AddChild(RequirementLabel("Magic Supply Device required for reservoir → personal MP transfer."));
        }
    }


}
