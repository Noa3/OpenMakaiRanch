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
        hero.AddChild(TitleLabel(T("screen.title", "Main Menu")));
        hero.AddChild(AddStyledLine(T("screen.title.subtitle", "SFW systems-first ranch management remake.")));
        hero.AddChild(AddStyledLine(T("screen.title.help", "Start immediately from a clean game or continue from slot 1.")));

        var cta = new HBoxContainer();
        cta.AddThemeConstantOverride("separation", 10);
        hero.AddChild(cta);

        var continueButton = PrimaryButton(_game.HasSaveSlot(1) ? T("screen.title.continue", "Continue Slot 1") : T("screen.title.no_save", "No Save In Slot 1"), T("tooltip.continue", "Load and continue from your last save in slot 1"));
        continueButton.Disabled = !_game.HasSaveSlot(1);
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
        quick.AddChild(SubtitleLabel(T("screen.title.quick_flow", "Quick Flow")));
        quick.AddChild(AddStyledLine(T("screen.title.step_1", "1. Assign jobs in Schedule.")));
        quick.AddChild(AddStyledLine(T("screen.title.step_2", "2. Visit Town/Shop for upgrades and supplies.")));
        quick.AddChild(AddStyledLine(T("screen.title.step_3", "3. Build bonds and run Adventure missions.")));
        quick.AddChild(AddStyledLine(T("screen.title.step_4", "4. End day from the top bar and inspect reports in Overview.")));

        var recruits = CardContainer();
        _content.AddChild(recruits);
        recruits.AddChild(SubtitleLabel(T("screen.title.starting_recruits", "Starting Generated Recruits")));
        recruits.AddChild(AddStyledLine(T("screen.title.recruit_info", "Every new ranch rolls extra recruits from the local talent pool.")));
        foreach (var recruit in _game.Roster.Characters.Where(character => character.IsGenerated))
        {
            var definition = _game.Roster.DefinitionFor(recruit);
            recruits.AddChild(AddStyledLine($"{definition.DisplayName} - {definition.Trait} | {T("label.ranch", "Ranch")} {recruit.RanchSkill} {T("label.craft", "Craft")} {recruit.CraftSkill} {T("label.combat", "Combat")} {recruit.CombatSkill}"));
        }
    }

    private void RenderRanch()
    {
        AddTitle(T("screen.ranch", "Ranch Overview"));

        var summary = CardContainer();
        _content.AddChild(summary);
        summary.AddChild(SubtitleLabel(T("screen.ranch.economy", "Economy Snapshot")));
        summary.AddChild(AddStyledLine($"{T("label.day", "Day")} {_game.State.Calendar.Day} {T("label.income", "income")}: {_game.State.Economy.LastIncome}"));
        summary.AddChild(AddStyledLine($"{T("label.expenses", "Expenses")}: {_game.State.Economy.LastExpenses}  {T("label.net", "Net")}: {_game.State.Economy.LastIncome - _game.State.Economy.LastExpenses}"));

        var stock = CardContainer();
        _content.AddChild(stock);
        stock.AddChild(SubtitleLabel(T("screen.ranch.stockpile", "Stockpile")));
        foreach (var entry in _game.Ranch.Stockpile.OrderBy(entry => entry.Key))
        {
            stock.AddChild(AddStyledLine($"{entry.Key}: {entry.Value}"));
        }

        var facilities = CardContainer();
        _content.AddChild(facilities);
        facilities.AddChild(SubtitleLabel(T("screen.ranch.facilities", "Facilities")));
        foreach (var facility in _game.Ranch.Facilities.OrderBy(entry => entry.Key))
        {
            var name = _game.Data.Facilities.TryGetValue(facility.Key, out var definition) ? definition.DisplayName : facility.Key;
            facilities.AddChild(AddStyledLine($"{name}: {T("label.level", "level")} {facility.Value}"));
        }

        if (_game.LastDailyReport is not null)
        {
            var report = CardContainer();
            _content.AddChild(report);
            report.AddChild(SubtitleLabel(T("screen.ranch.report", "Latest Daily Report")));

            // Income/Expenses summary
            var rpt = _game.LastDailyReport;
            report.AddChild(AddStyledLine($"Day {rpt.Day} | Income: {rpt.Income}g | Expenses: {rpt.Expenses}g | Net: {rpt.NetGold}g"));

            // Milk revenue
            if (rpt.MilkRevenue > 0)
                report.AddChild(AddStyledLine($"Milk shipped: +{rpt.MilkRevenue}g"));

            // Skill gains
            if (rpt.SkillGains > 0)
                report.AddChild(AddStyledLine($"Skill gains: {rpt.SkillGains} character(s) leveled up!"));
            foreach (var growth in rpt.CharacterGrowth)
            {
                report.AddChild(MutedLabel($"  {growth.DisplayName}: {growth.SkillGained} skill +{growth.Amount}"));
            }

            // Random events
            foreach (var evt in rpt.Events)
            {
                var icon = evt.IsPositive ? "[+]" : "[-]";
                report.AddChild(AddStyledLine($"{icon} {evt.Title}: {evt.Description}"));
            }

            // Report lines
            foreach (var line in rpt.Lines)
            {
                report.AddChild(MutedLabel(line));
            }
        }

        // Win condition progress
        var progress = CardContainer();
        _content.AddChild(progress);
        progress.AddChild(SubtitleLabel(T("screen.ranch.progress", "Endgame Progress")));
        progress.AddChild(MutedLabel(_game.WinCondition.ProgressSummary()));
        if (_game.WinCondition.IsGameComplete())
        {
            progress.AddChild(AddStyledLine("The ranch is thriving! All objectives complete!"));
        }
    }

    private void RenderRoster()
    {
        AddTitle(T("screen.roster", "Characters"));
        foreach (var character in _game.Roster.Characters)
        {
            var definition = _game.Roster.DefinitionFor(character);
            var card = new PanelContainer();
            card.AddThemeStyleboxOverride("panel", CardStyle(new Color("132036"), new Color("355070"), 1, 10));
            _content.AddChild(card);

            var row = new HBoxContainer { CustomMinimumSize = new Vector2(0, 130) };
            row.AddThemeConstantOverride("separation", 10);
            card.AddChild(row);

            row.AddChild(BuildCharacterVisual(character, definition));

            var details = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
            details.AddThemeConstantOverride("separation", 6);
            row.AddChild(details);

            details.AddChild(SubtitleLabel(definition.DisplayName));
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

            var trainingRow = new HBoxContainer();
            trainingRow.AddThemeConstantOverride("separation", 6);
            details.AddChild(trainingRow);

            var trainRanch = SecondaryButton(T("screen.roster.train_ranch", "Train Ranch"), T("tooltip.train_ranch", "Spend 10 energy: +1 Ranch skill, +12 fatigue, +1 morale"));
            trainRanch.Disabled = character.Energy < 10;
            trainRanch.Pressed += () => ExecuteUiAction(() => _game.TrainCharacter(character.Id, "ranch"), false);
            trainingRow.AddChild(trainRanch);

            var trainCraft = SecondaryButton(T("screen.roster.train_craft", "Train Craft"), T("tooltip.train_craft", "Spend 10 energy: +1 Craft skill, +12 fatigue, +1 morale"));
            trainCraft.Disabled = character.Energy < 10;
            trainCraft.Pressed += () => ExecuteUiAction(() => _game.TrainCharacter(character.Id, "craft"), false);
            trainingRow.AddChild(trainCraft);

            var trainCombat = SecondaryButton(T("screen.roster.train_combat", "Train Combat"), T("tooltip.train_combat", "Spend 10 energy: +1 Combat skill, +12 fatigue, +1 morale"));
            trainCombat.Disabled = character.Energy < 10;
            trainCombat.Pressed += () => ExecuteUiAction(() => _game.TrainCharacter(character.Id, "combat"), false);
            trainingRow.AddChild(trainCombat);

            // Equipment section
            var equipRow = new HBoxContainer();
            equipRow.AddThemeConstantOverride("separation", 4);
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
                var itemRow = new HBoxContainer();
                itemRow.AddThemeConstantOverride("separation", 4);
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

    private void RenderSchedule()
    {
        AddTitle(T("screen.schedule", "Daily Schedule"));
        foreach (var character in _game.Roster.Characters)
        {
            var definition = _game.Roster.DefinitionFor(character);
            var card = CardContainer();
            _content.AddChild(card);

            card.AddChild(SubtitleLabel($"{definition.DisplayName}: {_game.Schedule.GetAssignment(character.Id)}"));
            var row = new HBoxContainer();
            row.AddThemeConstantOverride("separation", 8);
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
                var button = SecondaryButton(job.DisplayName, tooltip);
                button.Pressed += () => ExecuteUiAction(() => _game.Schedule.AssignJob(character.Id, job.Id), false);
                row.AddChild(button);
            }
        }
    }

    private void RenderTown()
    {
        AddTitle(T("screen.town", "Town Hub"));

        var actions = CardContainer();
        _content.AddChild(actions);
        actions.AddChild(SubtitleLabel(T("screen.town.activities", "Town Activities")));

        var storeBtn = PrimaryButton(T("screen.town.general_store", "General Store"), T("tooltip.shop", "Buy and sell supplies, equipment, and consumables"));
        storeBtn.Pressed += () => { _game.Feedback.PlayConfirm(); ShowScreen("shop"); };
        actions.AddChild(storeBtn);

        var researchBtn = PrimaryButton(T("screen.town.research_office", "Research Office"), T("tooltip.research", "Unlock new skills and technologies for the ranch"));
        researchBtn.Pressed += () => { _game.Feedback.PlayConfirm(); ShowScreen("research"); };
        actions.AddChild(researchBtn);

        var adventureBtn = PrimaryButton(T("screen.town.adventure_guild", "Adventure Guild"), T("tooltip.adventure", "Dispatch characters on missions and patrols"));
        adventureBtn.Pressed += () => { _game.Feedback.PlayConfirm(); ShowScreen("adventure"); };
        actions.AddChild(adventureBtn);

        var recordsBtn = SecondaryButton(T("screen.town.hall_records", "Town Hall Records"));
        recordsBtn.Pressed += () =>
        {
            _game.Feedback.PlayConfirm();
            var stats = _game.WinCondition.ProgressSummary();
            SetStatus($"Town Records — {stats}");
        };
        actions.AddChild(recordsBtn);

        actions.AddChild(MutedLabel(T("screen.town.visit_hint", "Visit locations around town to access services and recruit new workers.")));

        var planning = CardContainer();
        _content.AddChild(planning);
        planning.AddChild(SubtitleLabel(T("screen.town.facility_planning", "Facility Planning")));
        foreach (var facility in _game.Data.Facilities.Values)
        {
            _game.Ranch.Facilities.TryGetValue(facility.Id, out var currentLevel);
            var cost = _game.Ranch.FacilityUpgradeCost(facility, currentLevel);

            var row = new HBoxContainer();
            row.AddThemeConstantOverride("separation", 8);
            planning.AddChild(row);

            row.AddChild(AddStyledLine($"{facility.DisplayName} {T("label.level", "level")} {currentLevel} -> {currentLevel + 1} ({cost}{T("unit.g", "g")})", true));
            var upgrade = PrimaryButton(T("screen.town.upgrade", "Upgrade"), T("tooltip.facility_upgrade", $"Upgrade to level {currentLevel + 1}: increases output by {facility.OutputBonus} per level, upkeep {facility.UpkeepGold}g"));
            upgrade.Disabled = _game.Economy.Gold < cost;
            upgrade.Pressed += () => ExecuteUiAction(() => _game.Ranch.UpgradeFacility(facility.Id, _game.Economy), false);
            row.AddChild(upgrade);
        }

        var recruitment = CardContainer();
        _content.AddChild(recruitment);
        recruitment.AddChild(SubtitleLabel(T("screen.town.recruitment", "Recruitment Board")));
        var generatedCount = _game.Roster.Characters.Count(character => character.IsGenerated);
        recruitment.AddChild(AddStyledLine($"{T("screen.town.generated_recruits", "Generated recruits in ranch")}: {generatedCount}"));
        var offer = _game.Recruitment.CurrentOffer;
        if (offer is null)
        {
            recruitment.AddChild(MutedLabel(T("screen.town.no_offer", "No recruitment offer is currently available.")));
            return;
        }

        var offerDefinition = _game.Roster.DefinitionFor(offer);

        var preview = new HBoxContainer();
        preview.AddThemeConstantOverride("separation", 10);
        recruitment.AddChild(preview);
        preview.AddChild(BuildCharacterVisual(offer, offerDefinition));

        var previewDetails = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        previewDetails.AddThemeConstantOverride("separation", 4);
        preview.AddChild(previewDetails);

        previewDetails.AddChild(SubtitleLabel(offerDefinition.DisplayName));
        previewDetails.AddChild(MutedLabel(offerDefinition.Trait));
        var chips = new HBoxContainer();
        chips.AddThemeConstantOverride("separation", 6);
        chips.AddChild(StatChip($"{T("label.ranch", "Ranch")} {offer.RanchSkill}"));
        chips.AddChild(StatChip($"{T("label.craft", "Craft")} {offer.CraftSkill}"));
        chips.AddChild(StatChip($"{T("label.combat", "Combat")} {offer.CombatSkill}"));
        previewDetails.AddChild(chips);
        previewDetails.AddChild(StatBar(T("label.hp", "HP"), offer.Hp, offerDefinition.MaxHp, new Color("55d6be")));
        previewDetails.AddChild(StatBar(T("label.energy", "Energy"), offer.Energy, offerDefinition.MaxEnergy, new Color("5bbcff")));

        var recruitmentActions = new HBoxContainer();
        recruitmentActions.AddThemeConstantOverride("separation", 8);
        recruitment.AddChild(recruitmentActions);

        var hireRecruit = PrimaryButton($"{T("screen.town.hire_offer", "Hire Offer")} ({RecruitmentService.DefaultRecruitCost}{T("unit.g", "g")})", T("tooltip.hire_recruit", "Add this recruit to the ranch roster permanently"));
        hireRecruit.Disabled = _game.Economy.Gold < RecruitmentService.DefaultRecruitCost;
        hireRecruit.Pressed += () => ExecuteUiAction(() => _game.Recruitment.HireOffer(), false);
        recruitmentActions.AddChild(hireRecruit);

        var rerollRecruit = SecondaryButton($"{T("screen.town.reroll_offer", "Reroll Offer")} ({RecruitmentService.RerollOfferCost}{T("unit.g", "g")})", T("tooltip.reroll_recruit", "Discard current offer and generate a new candidate (costs gold)"));
        rerollRecruit.Disabled = _game.Economy.Gold < RecruitmentService.RerollOfferCost;
        rerollRecruit.Pressed += () => ExecuteUiAction(() => _game.Recruitment.RerollOffer(), false);
        recruitmentActions.AddChild(rerollRecruit);

        recruitment.AddChild(MutedLabel(T("screen.town.hire_hint", "Hiring adds the displayed recruit and assigns rest by default. Rerolling previews a new candidate.")));
    }

    private void RenderShop()
    {
        AddTitle(T("screen.shop", "General Store"));

        var market = CardContainer();
        _content.AddChild(market);
        market.AddChild(SubtitleLabel(T("screen.shop.buy", "Buy Supplies")));
        foreach (var item in _game.Data.ShopItems())
        {
            var row = new HBoxContainer();
            row.AddThemeConstantOverride("separation", 8);
            market.AddChild(row);

            row.AddChild(AddStyledLine($"{item.DisplayName} - {item.Price}{T("unit.g", "g")}: {item.Description}", true));
            var buy = PrimaryButton(T("common.buy", "Buy"), $"{T("tooltip.buy", "Purchase")} {item.DisplayName} ({item.Price}{T("unit.g", "g")})");
            buy.Disabled = _game.Economy.Gold < item.Price;
            buy.Pressed += () => ExecuteUiAction(() => _game.Shop.Buy(item.Id, 1), false);
            row.AddChild(buy);
        }

        var inventory = CardContainer();
        _content.AddChild(inventory);
        inventory.AddChild(SubtitleLabel(T("screen.shop.inventory", "Inventory")));
        foreach (var item in _game.Inventory.Items)
        {
            var row = new HBoxContainer();
            row.AddThemeConstantOverride("separation", 8);
            inventory.AddChild(row);

            row.AddChild(AddStyledLine($"{item.Key}: {item.Value}", true));

            var sell = SecondaryButton(T("common.sell", "Sell"), T("tooltip.sell", "Sell one unit for half the purchase price"));
            sell.Disabled = item.Value <= 0;
            sell.Pressed += () => ExecuteUiAction(() => _game.Shop.Sell(item.Key, 1), false);
            row.AddChild(sell);

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
                row.AddChild(use);
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

            var actionRow = new HBoxContainer();
            actionRow.AddThemeConstantOverride("separation", 6);
            missionCard.AddChild(actionRow);

            var fightBtn = PrimaryButton(T("screen.combat.fight", "Fight"), T("tooltip.fight", "Engage in round-based combat with auto-battle support."));
            var capturedMissionId = mission.Id;
            fightBtn.Pressed += () =>
            {
                _game.StartNewCombat();
                _game.LastCombatReport = new CombatReport { MissionId = capturedMissionId };
                ShowScreen("combat");
            };
            actionRow.AddChild(fightBtn);

            var captureBtn = SecondaryButton(T("screen.adventure.capture", "Capture"), T("tooltip.capture_mission", "Battle with a capture attempt. Success may recruit a target."));
            captureBtn.Pressed += () =>
            {
                _game.StartNewCombat();
                _game.LastCombatReport = new CombatReport { MissionId = capturedMissionId, CaptureAttempted = true };
                ShowScreen("combat");
            };
            actionRow.AddChild(captureBtn);
        }

        missions.AddChild(MutedLabel(T("screen.adventure.capture_hint", "Capture Battle: standard combat plus a post-battle capture check. High party control helps.")));
    }

    private void RenderCombat()
    {
        AddTitle(T("screen.combat", "Combat And Mission Result"));

        if (_game.CurrentCombatPhase == CombatPhase.PreBattle)
        {
            RenderCombatPreBattle();
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

        var actions = new HBoxContainer();
        actions.AddThemeConstantOverride("separation", 10);
        _content.AddChild(actions);

        var autoBtn = PrimaryButton(T("screen.combat.auto_battle", "Auto Battle"), T("tooltip.auto_battle", "Resolve all combat rounds automatically with AI tactics"));
        autoBtn.Pressed += () =>
        {
            _game.RunRoundBasedMission(mission?.Id ?? _game.State.Adventure.LastMissionId, true);
            ShowScreen(_currentScreen);
        };
        actions.AddChild(autoBtn);

        var captureBtn = SecondaryButton(T("screen.combat.capture_battle", "Capture Battle"), T("tooltip.capture_battle", "Fight with capture attempt. Success may recruit an enemy!"));
        captureBtn.Pressed += () =>
        {
            _game.RunRoundBasedCapture(mission?.Id ?? _game.State.Adventure.LastMissionId);
            ShowScreen(_currentScreen);
        };
        actions.AddChild(captureBtn);

        var backBtn = SecondaryButton(T("common.back", "Back"));
        backBtn.Pressed += () => { _game.StartNewCombat(); ShowScreen("adventure"); };
        actions.AddChild(backBtn);
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
        outcomeCard.AddChild(AddStyledLine(report.Summary));

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
                var name = captured?.DisplayNameOverride ?? report.CapturedCharacterId;
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
        btn.Pressed += () => { _game.StartNewCombat(); ShowScreen("adventure"); };
        _content.AddChild(btn);
    }

    private void RenderCombatOutro()
    {
        var btn = PrimaryButton(T("common.back", "Back"));
        btn.Pressed += () => ShowScreen("adventure");
        _content.AddChild(btn);
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

            var mentorBtn = SecondaryButton("Mentorship (+4 bond, -4 fatigue)", "Spend 4 fatigue: +5 bond, +4 morale");
            mentorBtn.Pressed += () => ExecuteUiAction(() => _game.Bond.ConductMentorship(character.Id), false);
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

                var narrativeLabel = new Label
                {
                    Text = bondEvent.Description,
                    AutowrapMode = TextServer.AutowrapMode.WordSmart,
                    SizeFlagsHorizontal = SizeFlags.ExpandFill,
                    SizeFlagsVertical = SizeFlags.ExpandFill
                };
                narrativeBox.AddChild(narrativeLabel);

                var completeBtn = PrimaryButton("Complete Event", "Complete this bond event to earn rewards and progress the story");
                completeBtn.Pressed += () => ExecuteUiAction(() => _game.Bond.CompleteEvent(bondEvent.Id), false);
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

                var actions = new HBoxContainer();
                actions.AddThemeConstantOverride("separation", 6);
                petCard.AddChild(actions);

                var feedBtn = PrimaryButton($"{T("screen.pets.feed", "Feed")} (10{T("unit.g", "g")})", T("tooltip.feed_pet", "Feed the pet: Hunger+20, Mood+5, Bond+2"));
                feedBtn.Pressed += () => { var result = _game.Pets.Feed(pet.Id); _game.Feedback.PlayConfirm(); ShowScreen(_currentScreen); };
                actions.AddChild(feedBtn);

                var playBtn = SecondaryButton($"{T("screen.pets.play", "Play")} (5{T("unit.g", "g")})", T("tooltip.play_pet", "Play with the pet: Mood+15, Bond+3, Hunger-5"));
                playBtn.Pressed += () => { var result = _game.Pets.Play(pet.Id); _game.Feedback.PlayConfirm(); ShowScreen(_currentScreen); };
                actions.AddChild(playBtn);

                var trainBtn = SecondaryButton($"{T("screen.pets.train", "Train")} (15{T("unit.g", "g")})", T("tooltip.train_pet", "Train the pet: Training+10, Bond+1, Hunger-10, Mood-5"));
                trainBtn.Pressed += () => { var result = _game.Pets.Train(pet.Id); _game.Feedback.PlayConfirm(); ShowScreen(_currentScreen); };
                actions.AddChild(trainBtn);

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
        AddTitle(T("screen.saveload", "Save And Load"));
        var card = CardContainer();
        _content.AddChild(card);

        var save = PrimaryButton(T("screen.saveload.save", "Save Slot 1"), T("tooltip.save", "Save your current progress to slot 1"));
        save.Pressed += () => ExecuteUiAction(() => _game.SaveSlot(1), true);
        card.AddChild(save);

        var load = SecondaryButton(T("screen.saveload.load", "Load Slot 1"), T("tooltip.load", "Load saved progress from slot 1"));
        load.Pressed += () => ExecuteUiAction(() => _game.LoadSlot(1), true);
        card.AddChild(load);

        var newGame = SecondaryButton(T("screen.saveload.new_game", "New Game"));
        newGame.Pressed += () => ExecuteUiAction(_game.NewGame, true, "ranch");
        card.AddChild(newGame);

        var title = SecondaryButton(T("screen.saveload.back", "Back To Main Menu"));
        title.Pressed += () =>
        {
            var error = GetTree().ChangeSceneToFile("res://scenes/MainMenu.tscn");
            if (error != Error.Ok)
            {
                GD.PushError($"Failed to return to MainMenu scene: {error}");
            }
        };
        card.AddChild(title);
    }

    private void RenderSettings()
    {
        AddTitle(T("screen.settings", "Settings"));
        var card = CardContainer();
        _content.AddChild(card);
        card.AddChild(AddStyledLine(T("screen.settings.menu_flow", "Menu flow has been simplified: grouped navigation, cards, and clear action priorities.")));
        card.AddChild(AddStyledLine(T("screen.settings.feedback_info", "Mobile and handheld feedback can use short UI tones and optional vibration.")));

        var audioToggle = PrimaryButton($"{T("screen.settings.audio_feedback", "Audio Feedback")}: {(_game.Feedback.AudioEnabled ? T("label.on", "On") : T("label.off", "Off"))}");
        audioToggle.Pressed += () =>
        {
            _game.ToggleAudioFeedback();
            if (_game.Feedback.AudioEnabled)
            {
                _game.Feedback.PlayConfirm();
            }
        };
        card.AddChild(audioToggle);

        var hapticsToggle = PrimaryButton($"{T("screen.settings.handheld_vibration", "Handheld Vibration")}: {(_game.Feedback.HapticsEnabled ? T("label.on", "On") : T("label.off", "Off"))}");
        hapticsToggle.Pressed += () =>
        {
            _game.ToggleHapticsFeedback();
            _game.Feedback.PulseHaptics(40, 0.45f);
        };
        card.AddChild(hapticsToggle);

        var previewFeedback = SecondaryButton(T("screen.settings.preview_confirm", "Preview Confirm Feedback"));
        previewFeedback.Pressed += () => _game.Feedback.PlayConfirm();
        card.AddChild(previewFeedback);

        var previewError = SecondaryButton(T("screen.settings.preview_error", "Preview Error Feedback"));
        previewError.Pressed += () => _game.Feedback.PlayError();
        card.AddChild(previewError);

        var themeRow = new HBoxContainer();
        themeRow.AddThemeConstantOverride("separation", 8);
        card.AddChild(themeRow);
        themeRow.AddChild(AddStyledLine(T("screen.settings.color_theme", "Color Theme"), true));

        var themePicker = new OptionButton { Name = "ThemeOption", CustomMinimumSize = new Vector2(220, 34) };
        var currentThemeId = _game.State.Settings.ThemeId;
        var selectedThemeIndex = 0;
        var index = 0;
        foreach (var theme in UiThemeCatalog.All)
        {
            themePicker.AddItem(theme.DisplayName);
            themePicker.SetItemMetadata(index, theme.Id);
            if (string.Equals(theme.Id, currentThemeId, System.StringComparison.OrdinalIgnoreCase))
            {
                selectedThemeIndex = index;
            }

            index += 1;
        }

        themePicker.Selected = selectedThemeIndex;
        themePicker.ItemSelected += selected =>
        {
            var selectedId = themePicker.GetItemMetadata((int)selected).AsString();
            ExecuteUiAction(() => _game.SetTheme(selectedId), false);
        };
        themeRow.AddChild(themePicker);

        var uiScaleRow = new HBoxContainer();
        uiScaleRow.AddThemeConstantOverride("separation", 8);
        card.AddChild(uiScaleRow);
        uiScaleRow.AddChild(AddStyledLine($"{T("screen.settings.ui_scale", "UI Scale")}: {_game.State.Settings.UiScale:0.00}x", true));
        var uiScale = new HSlider
        {
            Name = "UiScaleSlider",
            MinValue = 0.85f,
            MaxValue = 1.35f,
            Step = 0.05f,
            Value = _game.State.Settings.UiScale,
            CustomMinimumSize = new Vector2(240, 0)
        };
        uiScale.ValueChanged += value => ExecuteUiAction(() => _game.SetUiScale((float)value), false);
        uiScaleRow.AddChild(uiScale);

        var localeRow = new HBoxContainer();
        localeRow.AddThemeConstantOverride("separation", 8);
        card.AddChild(localeRow);
        localeRow.AddChild(AddStyledLine(T("screen.settings.language", "Language"), true));
        var localePicker = new OptionButton { Name = "LocaleOption", CustomMinimumSize = new Vector2(180, 34) };
        var selectedLocaleIndex = 0;
        for (var localeIdx = 0; localeIdx < AvailableLocales.Length; localeIdx++)
        {
            var lc = AvailableLocales[localeIdx];
            localePicker.AddItem(LocaleDisplayName(lc));
            localePicker.SetItemMetadata(localeIdx, lc);
            if (string.Equals(lc, _game.State.Settings.Locale, System.StringComparison.OrdinalIgnoreCase))
            {
                selectedLocaleIndex = localeIdx;
            }
        }
        localePicker.Selected = selectedLocaleIndex;
        localePicker.ItemSelected += selected =>
        {
            var selectedLocale = localePicker.GetItemMetadata((int)selected).AsString();
            ExecuteUiAction(() => _game.SetLocale(selectedLocale), false);
        };
        localeRow.AddChild(localePicker);

        card.AddChild(MutedLabel($"{T("screen.settings.haptics_supported", "Haptics supported on this device")}: {(_game.Feedback.SupportsHaptics ? T("label.yes", "Yes") : T("label.no", "No"))}"));
        card.AddChild(MutedLabel(T("screen.settings.android_haptics", "Android exports need the VIBRATE permission enabled for handheld vibration.")));
    }

    // === Training state tracking ===
    private int _trainingCharIdx;
    private TrainingCategory _trainingCategory;

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

        _trainingCharIdx = Math.Clamp(_trainingCharIdx, 0, chars.Count - 1);
        var character = chars[_trainingCharIdx];
        var mental = character.Mature;

        // === Character selector row ===
        var selectorRow = new HBoxContainer();
        selectorRow.AddThemeConstantOverride("separation", 8);
        _content.AddChild(selectorRow);

        selectorRow.AddChild(MutedLabel($"{T("label.character", "Character")}:"));
        var charPicker = new OptionButton { CustomMinimumSize = new Vector2(200, 30), TooltipText = T("tooltip.training_char", "Select a character to train") };
        for (var i = 0; i < chars.Count; i++)
        {
            charPicker.AddItem(chars[i].DisplayNameOverride);
            if (i == _trainingCharIdx) charPicker.Selected = i;
        }
        charPicker.ItemSelected += idx => { _trainingCharIdx = (int)idx; _game.NotifyStateChanged(); };
        selectorRow.AddChild(charPicker);

        // === Character stats card ===
        var statsCard = CardContainer();
        _content.AddChild(statsCard);
        statsCard.AddChild(AddStyledLine($"{character.DisplayNameOverride} — {T("label.energy", "Energy")} {character.Energy}  {T("label.fatigue", "Fatigue")} {character.Fatigue}  {T("label.bond", "Bond")} {character.Bond}", true));
        statsCard.AddChild(AddStyledLine($"{T("label.fall_state", "Fall State")}: {mental.FallState}  {T("label.resistance", "Resistance")} {mental.Resistance}  {T("label.lust", "Lust")} {mental.Lust}", true));
        statsCard.AddChild(AddStyledLine($"{T("label.affection", "Affection")} {mental.Favorability}  {T("label.obedience", "Obedience")} {mental.Obedience}  {T("label.submission", "Submission")} {mental.Submission}"));

        // === Category tabs ===
        var categories = (TrainingCategory[])Enum.GetValues(typeof(TrainingCategory));
        var catRow = new HBoxContainer();
        catRow.AddThemeConstantOverride("separation", 4);
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
        var selectorRow = new HBoxContainer();
        selectorRow.AddThemeConstantOverride("separation", 8);
        _content.AddChild(selectorRow);
        selectorRow.AddChild(MutedLabel($"{T("label.character", "Character")}:"));
        var charPicker = new OptionButton { CustomMinimumSize = new Vector2(200, 30), TooltipText = T("tooltip.milk_char", "Select a character to manage milk production") };
        for (var i = 0; i < chars.Count; i++)
        {
            charPicker.AddItem(chars[i].DisplayNameOverride);
            if (i == _milkCharIdx) charPicker.Selected = i;
        }
        charPicker.ItemSelected += idx => { _milkCharIdx = (int)idx; _game.NotifyStateChanged(); };
        selectorRow.AddChild(charPicker);

        // === Stats card ===
        var statsCard = CardContainer();
        _content.AddChild(statsCard);
        statsCard.AddChild(SubtitleLabel($"{character.DisplayNameOverride} — {T("screen.milk.volume", "Milk Volume")}"));

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

        statsCard.AddChild(AddStyledLine($"{T("screen.milk.equipment", "Equipment")}: {(milk.EquippedMilkerId > 0 ? $"{T("screen.milk.milker", "Milker")} #{milk.EquippedMilkerId}" : T("screen.milk.no_milker", "None"))}"));

        // Lifetime stats
        statsCard.AddChild(AddStyledLine($"{T("screen.milk.total_shipped", "Lifetime")}: {milk.TotalShipped} {T("unit.units", "units")} shipped, {milk.TotalRevenue}{T("unit.g", "g")} {T("screen.milk.revenue", "revenue")}"));

        // Global stats
        statsCard.AddChild(AddStyledLine($"{T("screen.milk.ranch_total", "Ranch Total")}: {_game.State.Mature.TotalMilkProduced} {T("unit.units", "units")} produced, {_game.State.Mature.TotalMilkRevenue}{T("unit.g", "g")} {T("screen.milk.revenue", "revenue")}"));

        // === Action buttons ===
        var actions = new HBoxContainer();
        actions.AddThemeConstantOverride("separation", 10);
        _content.AddChild(actions);

        var produceBtn = PrimaryButton(T("screen.milk.produce", "Produce Milk Now"), T("tooltip.produce_milk", "Generate milk based on production rate, quality, and constitution traits"));
        produceBtn.TooltipText = T("tooltip.produce_milk", "Generate milk based on production rate, quality, and constitution traits");
        produceBtn.Pressed += () =>
        {
            _game.MilkEconomy.ProduceMilk(character.Id);
            _game.Feedback.PlayConfirm();
            ShowScreen(_currentScreen);
        };
        actions.AddChild(produceBtn);

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
            _content.AddChild(MutedLabel(T("screen.milk.no_milk", "No milk stored. Use Produce to generate milk, or advance a day for automatic production.")));
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
        var selectorRow = new HBoxContainer();
        selectorRow.AddThemeConstantOverride("separation", 8);
        _content.AddChild(selectorRow);
        selectorRow.AddChild(MutedLabel($"{T("label.character", "Character")}:"));
        var charPicker = new OptionButton { CustomMinimumSize = new Vector2(200, 30), TooltipText = T("tooltip.mental_char", "Select a character to inspect mental state") };
        for (var i = 0; i < chars.Count; i++)
        {
            charPicker.AddItem(chars[i].DisplayNameOverride);
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

    private Control BuildPicker(string[] options, string current, Action<string> onSelect)
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 8);
        row.CustomMinimumSize = new Vector2(0, 32);
        var picker = new OptionButton { CustomMinimumSize = new Vector2(180, 30) };
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

        // === Section 1: Identity ===
        var idCard = CardContainer();
        _content.AddChild(idCard);
        idCard.AddChild(SubtitleLabel(T("screen.character_creation.identity", "Identity")));

        var grid = new GridContainer { Columns = 2 };
        grid.AddThemeConstantOverride("h_separation", 10);
        grid.AddThemeConstantOverride("v_separation", 6);
        idCard.AddChild(grid);

        grid.AddChild(MutedLabel(T("screen.character_creation.name", "Name:")));
        var nameInput = new LineEdit { Text = player.Name, PlaceholderText = T("screen.character_creation.name_hint", "Enter your name"), CustomMinimumSize = new Vector2(220, 30) };
        nameInput.TextChanged += _ => _game.SetPlayerName(nameInput.Text);
        grid.AddChild(nameInput);

        grid.AddChild(MutedLabel(T("label.race", "Race:")));
        grid.AddChild(BuildPicker(CharacterGenerationPools.Races, player.Race, val => _game.SetPlayerRace(val)));

        grid.AddChild(MutedLabel(T("label.gender", "Gender:")));
        grid.AddChild(BuildPicker(new[] { "Male", "Female" }, player.Gender, val => _game.SetPlayerGender(val)));

        // === Section 2: Body ===
        var bodyCard = CardContainer();
        _content.AddChild(bodyCard);
        bodyCard.AddChild(SubtitleLabel(T("screen.character_creation.body", "Body")));

        var bodyGrid = new GridContainer { Columns = 3 };
        bodyGrid.AddThemeConstantOverride("h_separation", 10);
        bodyGrid.AddThemeConstantOverride("v_separation", 6);
        bodyCard.AddChild(bodyGrid);

        var heightLabels = CharacterGenerationPools.HeightRanges.Select(h => h.Label).ToArray();
        var currentHeightLabel = CharacterGenerationPools.HeightRanges.FirstOrDefault(h => h.Min <= player.Height && player.Height <= h.Max).Label ?? "Imposing";
        AddLabeledOption(bodyGrid, T("label.height", "Height:"), heightLabels, currentHeightLabel, val =>
        {
            var range = CharacterGenerationPools.HeightRanges.FirstOrDefault(h => h.Label == val);
            _game.ModifyPlayer(p => { p.Height = (range.Min + range.Max) / 2; });
        });

        var ageLabels = CharacterGenerationPools.ApparentAges.Select(a => a.Label).ToArray();
        var currentAgeLabel = CharacterGenerationPools.ApparentAges.FirstOrDefault(a => a.Age == player.ApparentAge).Label ?? "Adult";
        AddLabeledOption(bodyGrid, T("label.age", "Age:"), ageLabels, currentAgeLabel, val =>
        {
            var entry = CharacterGenerationPools.ApparentAges.FirstOrDefault(a => a.Label == val);
            _game.ModifyPlayer(p => p.ApparentAge = entry.Age);
        });

        AddLabeledOption(bodyGrid, T("label.body", "Body:"), CharacterGenerationPools.BodyShapes, player.BodyShape, val => _game.ModifyPlayer(p => p.BodyShape = val));
        AddLabeledOption(bodyGrid, T("label.skin", "Skin:"), CharacterGenerationPools.SkinColors, player.SkinColor, val => _game.ModifyPlayer(p => p.SkinColor = val));

        bodyGrid.AddChild(new Control()); // spacer
        var featsRow = new HBoxContainer();
        featsRow.AddThemeConstantOverride("separation", 16);
        bodyGrid.AddChild(featsRow);

        var hornsCb = new CheckBox { Text = T("screen.character_creation.horns", "Horns"), ButtonPressed = player.HasHorns };
        hornsCb.Toggled += on => _game.ModifyPlayer(p => p.HasHorns = on);
        featsRow.AddChild(hornsCb);

        var glassesCb = new CheckBox { Text = T("screen.character_creation.glasses", "Glasses"), ButtonPressed = player.HasGlasses };
        glassesCb.Toggled += on => _game.ModifyPlayer(p => p.HasGlasses = on);
        featsRow.AddChild(glassesCb);

        // If female, show bust options
        if (string.Equals(player.Gender, "Female", StringComparison.OrdinalIgnoreCase))
        {
            var bustGrid = new GridContainer { Columns = 2 };
            bustGrid.AddThemeConstantOverride("h_separation", 10);
            bustGrid.AddThemeConstantOverride("v_separation", 6);
            bodyCard.AddChild(bustGrid);
            AddLabeledOption(bustGrid, T("label.bust", "Bust:"), CharacterGenerationPools.BreastSizeLabels, player.BustSize, val => _game.ModifyPlayer(p => p.BustSize = val));
        }

        // === Section 3: Hair & Eyes ===
        var hairCard = CardContainer();
        _content.AddChild(hairCard);
        hairCard.AddChild(SubtitleLabel(T("screen.character_creation.hair_eyes", "Hair & Eyes")));

        var hairGrid = new GridContainer { Columns = 2 };
        hairGrid.AddThemeConstantOverride("h_separation", 10);
        hairGrid.AddThemeConstantOverride("v_separation", 6);
        hairCard.AddChild(hairGrid);

        AddLabeledOption(hairGrid, T("label.hair", "Hair:"), CharacterGenerationPools.HairColors, player.HairColor, val => _game.ModifyPlayer(p => p.HairColor = val));
        AddLabeledOption(hairGrid, T("label.hair_style", "Style:"), CharacterGenerationPools.HairStyles, player.HairStyle, val => _game.ModifyPlayer(p => p.HairStyle = val));
        AddLabeledOption(hairGrid, T("label.hair_feature", "Feature:"), CharacterGenerationPools.HairFeatures, player.HairFeature, val => _game.ModifyPlayer(p => p.HairFeature = val));
        AddLabeledOption(hairGrid, T("label.eye", "Eyes:"), CharacterGenerationPools.EyeColors, player.EyeColor, val => _game.ModifyPlayer(p => p.EyeColor = val));
        AddLabeledOption(hairGrid, T("label.eye_shape", "Eye Shape:"), CharacterGenerationPools.EyeShapes, player.EyeShape, val => _game.ModifyPlayer(p => p.EyeShape = val));

        // === Ranch Name ===
        var ranchCard = CardContainer();
        _content.AddChild(ranchCard);
        ranchCard.AddChild(SubtitleLabel(T("screen.character_creation.ranch", "Ranch Name")));
        var ranchInput = new LineEdit { Text = player.RanchName, PlaceholderText = T("screen.character_creation.ranch_hint", "Enter your ranch name"), CustomMinimumSize = new Vector2(400, 30) };
        ranchInput.TextChanged += _ => _game.SetRanchName(ranchInput.Text);
        ranchCard.AddChild(ranchInput);

        // === Starting Characters Preview ===
        var charCard = CardContainer();
        _content.AddChild(charCard);
        charCard.AddChild(SubtitleLabel(T("screen.character_creation.staff", "Ranch Staff")));
        foreach (var character in _game.Roster.Characters)
        {
            var def = _game.Roster.DefinitionFor(character);
            charCard.AddChild(AddStyledLine($"{def.DisplayName} — {character.Race} | {T("label.ranch", "Ranch")} {character.RanchSkill} {T("label.craft", "Craft")} {character.CraftSkill} {T("label.combat", "Combat")} {character.CombatSkill}"));
        }

        var rerollBtn = SecondaryButton(T("screen.character_creation.reroll_recruits", "Reroll Generated Hands"));
        rerollBtn.Pressed += () => ExecuteUiAction(() => { _game.RerollGeneratedRecruits(); return true; }, false);
        charCard.AddChild(rerollBtn);

        // === Action Buttons ===
        var actions = new HBoxContainer();
        actions.AddThemeConstantOverride("separation", 12);
        _content.AddChild(actions);
        var start = PrimaryButton(T("screen.character_creation.start", "Start Game"));
        start.Pressed += () => { _game.Feedback.PlayConfirm(); ShowScreen("prologue"); };
        actions.AddChild(start);
        var back = SecondaryButton(T("screen.character_creation.back", "Back To Title"));
        back.Pressed += () => ExecuteUiAction(_game.NewGame, true, "title");
        actions.AddChild(back);
    }

    private static void AddLabeledOption(GridContainer grid, string label, string[] options, string current, Action<string> onSelect)
    {
        grid.AddChild(new Label { Text = label, VerticalAlignment = VerticalAlignment.Center });
        var picker = new OptionButton { CustomMinimumSize = new Vector2(180, 30) };
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
        grid.AddChild(picker);
    }

    // === Prologue state tracking ===
    private int _prologuePage;

    private void RenderPrologue()
    {
        _prologuePage = 0;
        ShowProloguePage();
    }

    private void ShowProloguePage()
    {
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
                T("prologue.final3", "And so, your new life as a rancher — with a side of slave training and milk production — begins in earnest.")
            }
        };

        var card = CardContainer();
        _content.AddChild(card);

        foreach (var line in pages[_prologuePage])
        {
            card.AddChild(AddStyledLine(line));
        }

        var actions = new HBoxContainer();
        actions.AddThemeConstantOverride("separation", 12);
        _content.AddChild(actions);

        if (_prologuePage < pages.Length - 1)
        {
            var next = PrimaryButton(T("prologue.continue", "Continue"));
            next.Pressed += () =>
            {
                _prologuePage++;
                _game.NotifyStateChanged();
            };
            actions.AddChild(next);
        }
        else
        {
            var begin = PrimaryButton(T("prologue.begin", "Begin Game"));
            begin.Pressed += () => { _game.Feedback.PlayConfirm(); ShowScreen("ranch"); };
            actions.AddChild(begin);
        }

        var skip = SecondaryButton(T("prologue.skip", "Skip"));
        skip.Pressed += () => { _game.Feedback.PlayConfirm(); ShowScreen("ranch"); };
        actions.AddChild(skip);
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

        var ngPlus = PrimaryButton(T("victory.new_game_plus", "New Game+"));
        ngPlus.Pressed += () =>
        {
            _game.StartNewGamePlus();
            _game.Feedback.PlayConfirm();
            ShowScreen("ranch");
        };
        actions.AddChild(ngPlus);

        var title = SecondaryButton(T("victory.title_screen", "Title Screen"));
        title.Pressed += () => { _game.Feedback.PlayConfirm(); ShowScreen("title"); };
        actions.AddChild(title);
    }
}
