using System.Linq;
using Godot;
using OpenMakaiRanch.Core.Models;
using OpenMakaiRanch.Gameplay;

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
        hero.AddChild(TitleLabel("Main Menu"));
        hero.AddChild(AddStyledLine("SFW systems-first ranch management remake."));
        hero.AddChild(AddStyledLine("Start immediately from a clean game or continue from slot 1."));

        var cta = new HBoxContainer();
        cta.AddThemeConstantOverride("separation", 10);
        hero.AddChild(cta);

        var continueButton = PrimaryButton(_game.HasSaveSlot(1) ? "Continue Slot 1" : "No Save In Slot 1");
        continueButton.Disabled = !_game.HasSaveSlot(1);
        continueButton.Pressed += () => ExecuteUiAction(() => _game.LoadSlot(1), true, "ranch");
        cta.AddChild(continueButton);

        var newGame = SecondaryButton("New Game");
        newGame.Pressed += () => ExecuteUiAction(_game.NewGame, true, "ranch");
        cta.AddChild(newGame);

        var reroll = SecondaryButton("Reroll Recruits Only");
        reroll.Pressed += () => ExecuteUiAction(_game.RerollGeneratedRecruits, true);
        cta.AddChild(reroll);

        var quick = CardContainer();
        _content.AddChild(quick);
        quick.AddChild(SubtitleLabel("Quick Flow"));
        quick.AddChild(AddStyledLine("1. Assign jobs in Schedule."));
        quick.AddChild(AddStyledLine("2. Visit Town/Shop for upgrades and supplies."));
        quick.AddChild(AddStyledLine("3. Build bonds and run Adventure missions."));
        quick.AddChild(AddStyledLine("4. End day from the top bar and inspect reports in Overview."));

        var recruits = CardContainer();
        _content.AddChild(recruits);
        recruits.AddChild(SubtitleLabel("Starting Generated Recruits"));
        recruits.AddChild(AddStyledLine("Every new ranch rolls extra recruits from the local talent pool."));
        foreach (var recruit in _game.Roster.Characters.Where(character => character.IsGenerated))
        {
            var definition = _game.Roster.DefinitionFor(recruit);
            recruits.AddChild(AddStyledLine($"{definition.DisplayName} - {definition.Trait} | Ranch {recruit.RanchSkill} Craft {recruit.CraftSkill} Combat {recruit.CombatSkill}"));
        }
    }

    private void RenderRanch()
    {
        AddTitle("Ranch Overview");

        var summary = CardContainer();
        _content.AddChild(summary);
        summary.AddChild(SubtitleLabel("Economy Snapshot"));
        summary.AddChild(AddStyledLine($"Day {_game.State.Calendar.Day} income: {_game.State.Economy.LastIncome}"));
        summary.AddChild(AddStyledLine($"Expenses: {_game.State.Economy.LastExpenses}  Net: {_game.State.Economy.LastIncome - _game.State.Economy.LastExpenses}"));

        var stock = CardContainer();
        _content.AddChild(stock);
        stock.AddChild(SubtitleLabel("Stockpile"));
        foreach (var entry in _game.Ranch.Stockpile.OrderBy(entry => entry.Key))
        {
            stock.AddChild(AddStyledLine($"{entry.Key}: {entry.Value}"));
        }

        var facilities = CardContainer();
        _content.AddChild(facilities);
        facilities.AddChild(SubtitleLabel("Facilities"));
        foreach (var facility in _game.Ranch.Facilities.OrderBy(entry => entry.Key))
        {
            var name = _game.Data.Facilities.TryGetValue(facility.Key, out var definition) ? definition.DisplayName : facility.Key;
            facilities.AddChild(AddStyledLine($"{name}: level {facility.Value}"));
        }

        if (_game.LastDailyReport is not null)
        {
            var report = CardContainer();
            _content.AddChild(report);
            report.AddChild(SubtitleLabel("Latest Daily Report"));
            foreach (var line in _game.LastDailyReport.Lines)
            {
                report.AddChild(AddStyledLine(line));
            }
        }
    }

    private void RenderRoster()
    {
        AddTitle("Characters");
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
            details.AddChild(MutedLabel(definition.Trait));
            details.AddChild(MutedLabel($"Body: {definition.BodyType}"));
            if (character.IsGenerated)
            {
                details.AddChild(MutedLabel("Generated recruit"));
            }

            var stats = new GridContainer { Columns = 3, SizeFlagsHorizontal = SizeFlags.ExpandFill };
            stats.AddThemeConstantOverride("h_separation", 8);
            stats.AddThemeConstantOverride("v_separation", 6);
            details.AddChild(stats);
            stats.AddChild(StatChip($"Ranch {character.RanchSkill}"));
            stats.AddChild(StatChip($"Craft {character.CraftSkill}"));
            stats.AddChild(StatChip($"Combat {character.CombatSkill}"));
            stats.AddChild(StatChip($"Fatigue {character.Fatigue}"));
            stats.AddChild(StatChip($"Morale {character.Morale}"));
            stats.AddChild(StatChip($"Bond {character.Bond}"));

            details.AddChild(StatBar("HP", character.Hp, definition.MaxHp, new Color("55d6be")));
            details.AddChild(StatBar("Energy", character.Energy, definition.MaxEnergy, new Color("5bbcff")));

            var trainingRow = new HBoxContainer();
            trainingRow.AddThemeConstantOverride("separation", 6);
            details.AddChild(trainingRow);

            var trainRanch = SecondaryButton("Train Ranch");
            trainRanch.Disabled = character.Energy < 10;
            trainRanch.Pressed += () => ExecuteUiAction(() => _game.TrainCharacter(character.Id, "ranch"), false);
            trainingRow.AddChild(trainRanch);

            var trainCraft = SecondaryButton("Train Craft");
            trainCraft.Disabled = character.Energy < 10;
            trainCraft.Pressed += () => ExecuteUiAction(() => _game.TrainCharacter(character.Id, "craft"), false);
            trainingRow.AddChild(trainCraft);

            var trainCombat = SecondaryButton("Train Combat");
            trainCombat.Disabled = character.Energy < 10;
            trainCombat.Pressed += () => ExecuteUiAction(() => _game.TrainCharacter(character.Id, "combat"), false);
            trainingRow.AddChild(trainCombat);
        }
    }

    private void RenderSchedule()
    {
        AddTitle("Daily Schedule");
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
                var button = SecondaryButton(job.DisplayName);
                button.Pressed += () => ExecuteUiAction(() => _game.Schedule.AssignJob(character.Id, job.Id), false);
                row.AddChild(button);
            }
        }
    }

    private void RenderTown()
    {
        AddTitle("Town Hub");

        var actions = CardContainer();
        _content.AddChild(actions);
        actions.AddChild(SubtitleLabel("Current Town Activities"));
        foreach (var action in _game.Town.Actions)
        {
            actions.AddChild(AddStyledLine(action));
        }

        var planning = CardContainer();
        _content.AddChild(planning);
        planning.AddChild(SubtitleLabel("Facility Planning"));
        foreach (var facility in _game.Data.Facilities.Values)
        {
            _game.Ranch.Facilities.TryGetValue(facility.Id, out var currentLevel);
            var cost = _game.Ranch.FacilityUpgradeCost(facility, currentLevel);

            var row = new HBoxContainer();
            row.AddThemeConstantOverride("separation", 8);
            planning.AddChild(row);

            row.AddChild(AddStyledLine($"{facility.DisplayName} level {currentLevel} -> {currentLevel + 1} ({cost}g)", true));
            var upgrade = PrimaryButton("Upgrade");
            upgrade.Disabled = _game.Economy.Gold < cost;
            upgrade.Pressed += () => ExecuteUiAction(() => _game.Ranch.UpgradeFacility(facility.Id, _game.Economy), false);
            row.AddChild(upgrade);
        }

        var recruitment = CardContainer();
        _content.AddChild(recruitment);
        recruitment.AddChild(SubtitleLabel("Recruitment Board"));
        var generatedCount = _game.Roster.Characters.Count(character => character.IsGenerated);
        recruitment.AddChild(AddStyledLine($"Generated recruits in ranch: {generatedCount}"));
        var offer = _game.Recruitment.CurrentOffer;
        if (offer is null)
        {
            recruitment.AddChild(MutedLabel("No recruitment offer is currently available."));
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
        chips.AddChild(StatChip($"Ranch {offer.RanchSkill}"));
        chips.AddChild(StatChip($"Craft {offer.CraftSkill}"));
        chips.AddChild(StatChip($"Combat {offer.CombatSkill}"));
        previewDetails.AddChild(chips);
        previewDetails.AddChild(StatBar("HP", offer.Hp, offerDefinition.MaxHp, new Color("55d6be")));
        previewDetails.AddChild(StatBar("Energy", offer.Energy, offerDefinition.MaxEnergy, new Color("5bbcff")));

        var recruitmentActions = new HBoxContainer();
        recruitmentActions.AddThemeConstantOverride("separation", 8);
        recruitment.AddChild(recruitmentActions);

        var hireRecruit = PrimaryButton($"Hire Offer ({RecruitmentService.DefaultRecruitCost}g)");
        hireRecruit.Disabled = _game.Economy.Gold < RecruitmentService.DefaultRecruitCost;
        hireRecruit.Pressed += () => ExecuteUiAction(() => _game.Recruitment.HireOffer(), false);
        recruitmentActions.AddChild(hireRecruit);

        var rerollRecruit = SecondaryButton($"Reroll Offer ({RecruitmentService.RerollOfferCost}g)");
        rerollRecruit.Disabled = _game.Economy.Gold < RecruitmentService.RerollOfferCost;
        rerollRecruit.Pressed += () => ExecuteUiAction(() => _game.Recruitment.RerollOffer(), false);
        recruitmentActions.AddChild(rerollRecruit);

        recruitment.AddChild(MutedLabel("Hiring adds the displayed recruit and assigns rest by default. Rerolling previews a new candidate."));
    }

    private void RenderShop()
    {
        AddTitle("General Store");

        var market = CardContainer();
        _content.AddChild(market);
        market.AddChild(SubtitleLabel("Buy Supplies"));
        foreach (var item in _game.Data.ShopItems())
        {
            var row = new HBoxContainer();
            row.AddThemeConstantOverride("separation", 8);
            market.AddChild(row);

            row.AddChild(AddStyledLine($"{item.DisplayName} - {item.Price}g: {item.Description}", true));
            var buy = PrimaryButton("Buy");
            buy.Disabled = _game.Economy.Gold < item.Price;
            buy.Pressed += () => ExecuteUiAction(() => _game.Shop.Buy(item.Id, 1), false);
            row.AddChild(buy);
        }

        var inventory = CardContainer();
        _content.AddChild(inventory);
        inventory.AddChild(SubtitleLabel("Inventory"));
        foreach (var item in _game.Inventory.Items)
        {
            var row = new HBoxContainer();
            row.AddThemeConstantOverride("separation", 8);
            inventory.AddChild(row);

            row.AddChild(AddStyledLine($"{item.Key}: {item.Value}", true));

            var sell = SecondaryButton("Sell");
            sell.Disabled = item.Value <= 0;
            sell.Pressed += () => ExecuteUiAction(() => _game.Shop.Sell(item.Key, 1), false);
            row.AddChild(sell);

            if (item.Key == "meal_box")
            {
                var use = SecondaryButton("Use On Tiredest");
                use.Disabled = !_game.Roster.Characters.Any();
                use.Pressed += () => ExecuteUiAction(() =>
                {
                    var target = _game.Roster.Characters.OrderByDescending(character => character.Fatigue).FirstOrDefault();
                    return target is not null && _game.UseItemOnCharacter(item.Key, target.Id);
                }, false);
                row.AddChild(use);
            }
        }
    }

    private void RenderAdventure()
    {
        AddTitle("Adventure Guild");

        var party = CardContainer();
        _content.AddChild(party);
        party.AddChild(SubtitleLabel("Party Selection"));
        foreach (var character in _game.Roster.Characters)
        {
            var definition = _game.Roster.DefinitionFor(character);
            var selected = _game.State.Adventure.SelectedPartyIds.Contains(character.Id);
            var button = selected ? PrimaryButton($"Selected: {definition.DisplayName}") : SecondaryButton($"Add: {definition.DisplayName}");
            button.Pressed += () => ExecuteUiAction(() => _game.TogglePartyMember(character.Id), true);
            party.AddChild(button);
        }

        party.AddChild(AddStyledLine($"Current party size: {_game.State.Adventure.SelectedPartyIds.Count}"));

        var missions = CardContainer();
        _content.AddChild(missions);
        missions.AddChild(SubtitleLabel("Missions"));
        foreach (var mission in _game.Data.Missions.Values)
        {
            var row = new HBoxContainer();
            row.AddThemeConstantOverride("separation", 8);
            missions.AddChild(row);

            var normalBattle = PrimaryButton($"Run {mission.DisplayName} (difficulty {mission.Difficulty})");
            normalBattle.Pressed += () => ExecuteUiAction(() => _game.RunMission(mission.Id, false), true, "combat");
            row.AddChild(normalBattle);

            var captureBattle = SecondaryButton("Capture Battle");
            captureBattle.Pressed += () => ExecuteUiAction(() => _game.RunMission(mission.Id, true), true, "combat");
            row.AddChild(captureBattle);
        }

        missions.AddChild(MutedLabel("Capture Battle uses a turn-style control check and can recruit a random generated target on success."));
    }

    private void RenderCombat()
    {
        AddTitle("Combat And Mission Result");
        var card = CardContainer();
        _content.AddChild(card);

        card.AddChild(AddStyledLine(_game.LastCombatReport?.Summary ?? _game.State.Adventure.LastSummary));
        if (_game.LastCombatReport is not null)
        {
            var rewardItemText = string.IsNullOrWhiteSpace(_game.LastCombatReport.RewardItemId)
                ? string.Empty
                : $", item {_game.LastCombatReport.RewardItemId}";
            card.AddChild(AddStyledLine($"Reward: {_game.LastCombatReport.RewardGold} gold{rewardItemText}"));

            if (_game.LastCombatReport.CaptureAttempted)
            {
                var capturedCharacter = _game.Roster.Find(_game.LastCombatReport.CapturedCharacterId);
                var capturedName = capturedCharacter is null
                    ? _game.LastCombatReport.CapturedCharacterId
                    : _game.Roster.DefinitionFor(capturedCharacter).DisplayName;
                card.AddChild(AddStyledLine(_game.LastCombatReport.CaptureSucceeded
                    ? $"Capture succeeded: {capturedName}"
                    : "Capture failed."));
            }

            if (_game.LastCombatReport.TurnLog.Count > 0)
            {
                card.AddChild(SubtitleLabel("Turn Log"));
                foreach (var line in _game.LastCombatReport.TurnLog)
                {
                    card.AddChild(MutedLabel(line));
                }
            }
        }
    }

    private void RenderMilestones()
    {
        AddTitle("Milestones");
        var card = CardContainer();
        _content.AddChild(card);
        foreach (var milestone in _game.Data.Milestones.Values)
        {
            var done = _game.Milestones.Completed.Contains(milestone.Id) ? "Complete" : "Open";
            card.AddChild(AddStyledLine($"{milestone.DisplayName}: {done} ({MilestoneTriggerText(milestone)})"));
        }
    }

    private void RenderResearch()
    {
        AddTitle("Research");
        var card = CardContainer();
        _content.AddChild(card);
        foreach (var skill in _game.Data.Skills.Values)
        {
            var unlocked = _game.State.Research.UnlockedSkillIds.Contains(skill.Id);
            _game.State.Ranch.Stockpile.TryGetValue(skill.CostResourceId, out var availableCostResource);
            var canAfford = availableCostResource >= skill.CostAmount;
            var button = unlocked
                ? SecondaryButton($"Unlocked: {skill.DisplayName}")
                : PrimaryButton($"Unlock {skill.DisplayName} ({skill.CostAmount} {skill.CostResourceId})");
            button.Disabled = unlocked || !canAfford;
            button.Pressed += () => ExecuteUiAction(() => _game.Research.Unlock(skill.Id), false);
            card.AddChild(button);
            card.AddChild(MutedLabel(skill.Description));
        }

        card.AddChild(AddStyledLine("Unlocked passives apply immediately: ranch planning boosts output and field medicine reduces mission fatigue."));
    }

    private void RenderBond()
    {
        AddTitle("Bond And Mentorship");
        foreach (var character in _game.Roster.Characters)
        {
            var definition = _game.Roster.DefinitionFor(character);
            var card = CardContainer();
            _content.AddChild(card);

            card.AddChild(SubtitleLabel($"{definition.DisplayName} (bond {character.Bond})"));
            var mentor = PrimaryButton("Conduct Mentorship");
            mentor.Pressed += () => ExecuteUiAction(() => _game.Bond.ConductMentorship(character.Id), false);
            card.AddChild(mentor);

            foreach (var bondEvent in _game.Bond.AvailableEvents(character.Id))
            {
                var eventButton = SecondaryButton($"Event: {bondEvent.Title}");
                eventButton.Pressed += () => ExecuteUiAction(() => _game.Bond.CompleteEvent(bondEvent.Id), false);
                card.AddChild(eventButton);
                card.AddChild(MutedLabel(bondEvent.Description));
            }

            if (_game.State.Settings.ContentMode == ContentMode.MatureSkeleton)
            {
                card.AddChild(MutedLabel(_game.MatureContentHooks.BondScenePlaceholder(character.Id)));
            }
        }
    }

    private void RenderPets()
    {
        AddTitle("Pets");
        var card = CardContainer();
        _content.AddChild(card);

        foreach (var pet in _game.Data.Pets.Values)
        {
            var adopted = _game.State.Pets.AdoptedPetIds.Contains(pet.Id);
            var button = adopted
                ? SecondaryButton($"Adopted: {pet.DisplayName}")
                : PrimaryButton($"Adopt {pet.DisplayName} ({pet.CareCost * 4}g, {pet.CareCost}g/day)");
            button.Disabled = adopted;
            button.Pressed += () => ExecuteUiAction(() => _game.Pets.Adopt(pet.Id), false);
            card.AddChild(button);
        }

        card.AddChild(SubtitleLabel("Adopted"));
        foreach (var petId in _game.State.Pets.AdoptedPetIds)
        {
            var displayName = _game.Data.Pets.TryGetValue(petId, out var pet) ? pet.DisplayName : petId;
            card.AddChild(AddStyledLine(displayName));
        }
    }

    private void RenderSaveLoad()
    {
        AddTitle("Save And Load");
        var card = CardContainer();
        _content.AddChild(card);

        var save = PrimaryButton("Save Slot 1");
        save.Pressed += () => ExecuteUiAction(() => _game.SaveSlot(1), true);
        card.AddChild(save);

        var load = SecondaryButton("Load Slot 1");
        load.Pressed += () => ExecuteUiAction(() => _game.LoadSlot(1), true);
        card.AddChild(load);

        var newGame = SecondaryButton("New Game");
        newGame.Pressed += () => ExecuteUiAction(_game.NewGame, true, "ranch");
        card.AddChild(newGame);

        var title = SecondaryButton("Back To Main Menu");
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
        AddTitle("Settings");
        var card = CardContainer();
        _content.AddChild(card);
        card.AddChild(AddStyledLine("Menu flow has been simplified: grouped navigation, cards, and clear action priorities."));
        card.AddChild(AddStyledLine("Mobile and handheld feedback can use short UI tones and optional vibration."));

        var audioToggle = PrimaryButton($"Audio Feedback: {(_game.Feedback.AudioEnabled ? "On" : "Off")}");
        audioToggle.Pressed += () =>
        {
            _game.ToggleAudioFeedback();
            if (_game.Feedback.AudioEnabled)
            {
                _game.Feedback.PlayConfirm();
            }
        };
        card.AddChild(audioToggle);

        var hapticsToggle = PrimaryButton($"Handheld Vibration: {(_game.Feedback.HapticsEnabled ? "On" : "Off")}");
        hapticsToggle.Pressed += () =>
        {
            _game.ToggleHapticsFeedback();
            _game.Feedback.PulseHaptics(40, 0.45f);
        };
        card.AddChild(hapticsToggle);

        var previewFeedback = SecondaryButton("Preview Confirm Feedback");
        previewFeedback.Pressed += () => _game.Feedback.PlayConfirm();
        card.AddChild(previewFeedback);

        var previewError = SecondaryButton("Preview Error Feedback");
        previewError.Pressed += () => _game.Feedback.PlayError();
        card.AddChild(previewError);

        var themeRow = new HBoxContainer();
        themeRow.AddThemeConstantOverride("separation", 8);
        card.AddChild(themeRow);
        themeRow.AddChild(AddStyledLine("Color Theme", true));

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
        uiScaleRow.AddChild(AddStyledLine($"UI Scale: {_game.State.Settings.UiScale:0.00}x", true));
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
        localeRow.AddChild(AddStyledLine("Language", true));
        var localePicker = new OptionButton { Name = "LocaleOption", CustomMinimumSize = new Vector2(180, 34) };
        localePicker.AddItem("English");
        localePicker.SetItemMetadata(0, "en");
        localePicker.AddItem("Japanese (skeleton)");
        localePicker.SetItemMetadata(1, "ja");
        localePicker.Selected = string.Equals(_game.State.Settings.Locale, "ja", System.StringComparison.OrdinalIgnoreCase) ? 1 : 0;
        localePicker.ItemSelected += selected =>
        {
            var selectedLocale = localePicker.GetItemMetadata((int)selected).AsString();
            ExecuteUiAction(() => _game.SetLocale(selectedLocale), false);
        };
        localeRow.AddChild(localePicker);

        var contentModeButton = PrimaryButton($"Content Mode: {_game.State.Settings.ContentMode}");
        contentModeButton.Name = "ContentModeButton";
        contentModeButton.Pressed += () =>
        {
            if (_game.State.Settings.ContentMode == ContentMode.Sfw)
            {
                ExecuteUiAction(() => _game.TrySetContentMode(ContentMode.MatureSkeleton, true), false);
                return;
            }

            ExecuteUiAction(() => _game.TrySetContentMode(ContentMode.Sfw, true), false);
        };
        card.AddChild(contentModeButton);
        card.AddChild(MutedLabel("MatureSkeleton mode only enables placeholder hooks and TODO stubs; explicit content is not included."));

        card.AddChild(MutedLabel($"Haptics supported on this device: {(_game.Feedback.SupportsHaptics ? "Yes" : "No")}"));
        card.AddChild(MutedLabel("Android exports need the VIBRATE permission enabled for handheld vibration."));
    }

    private static string MilestoneTriggerText(Core.Resources.MilestoneDefinition milestone)
    {
        return milestone.TriggerKind switch
        {
            Core.Resources.MilestoneTriggerKind.DayReached => $"Reach day {milestone.TriggerAmount}",
            Core.Resources.MilestoneTriggerKind.GoldReached => $"Reach {milestone.TriggerAmount} gold",
            Core.Resources.MilestoneTriggerKind.MissionCompleted => $"Complete mission {milestone.TriggerId}",
            Core.Resources.MilestoneTriggerKind.BondReached => $"Raise any bond to {milestone.TriggerAmount}",
            Core.Resources.MilestoneTriggerKind.ResearchUnlocked => milestone.TriggerId == "any" ? "Unlock any research" : $"Unlock research {milestone.TriggerId}",
            _ => "Unknown"
        };
    }
}
