using System.Linq;
using Godot;
using OpenMakaiRanch.Gameplay;

namespace OpenMakaiRanch.Ui;

public partial class UiShellController
{
    private void FinalizeSequentialManagementCards()
    {
        // These legacy renderers use PanelContainer as a vertical list. Godot instead
        // gives every direct Control child the same rectangle. Normalize once, during
        // composition, before focus restoration/container layout. Do not touch authored
        // overlays, character previews or other screens that may intentionally overlap.
        AddDedicatedNavigation();
        if (_currentScreen == "adventure") AddAdventureReadinessCard();
        if (_currentScreen is "adventure" or "combat" or "shop" or "schedule" or "research" or "milestones")
            StackSequentialCards(_content);
    }

    private void AddAdventureReadinessCard()
    {
        if (_content.GetNodeOrNull<Control>("AdventureReadinessCard") is not null) return;
        var card = CardContainer();
        card.Name = "AdventureReadinessCard";
        _content.AddChild(card);
        _content.MoveChild(card, 1);
        var content = CardContent();
        card.AddChild(content);
        var player = _game.State.Player;
        var cost = _game.PlayerStaminaCost(PlayerActivityKind.Adventure);
        content.AddChild(SubtitleLabel($"Daily Stamina: {player.Stamina}/{player.MaxStamina + player.DailyStaminaBonus}"));
        var automatic = _game.State.Adventure.SelectedPartyIds.Count == 0;
        var partyCount = _game.Roster.Characters.Count(character => automatic
            || _game.State.Adventure.SelectedPartyIds.Contains(character.Id));
        content.AddChild(MutedLabel(automatic
            ? $"Party: all {partyCount} residents automatically. Mission entry: {cost} Stamina."
            : $"Party: {partyCount} selected residents. Mission entry: {cost} Stamina."));
        content.AddChild(MutedLabel("Choose Fight below to prepare a mission. Preparation is free."));
        card.TooltipText = "An ordinary mission commits daily Stamina once at entry. Tactical Battle requires the ranch owner in the party; its turns use combat HP, SP and MP. Auto Battle resolves the encounter automatically. Back from results resumes world time.";
        if (player.Stamina < cost)
            content.AddChild(RequirementLabel("Not enough daily Stamina for an ordinary mission. Rest or finish the day before starting another expedition."));
    }

    internal static void StackSequentialCards(Node root)
    {
        // Work bottom-up on a stable snapshot so nested mission/result cards are handled
        // without reparenting the same control twice. Existing single-content cards stay
        // untouched, preserving their nodes, signals, focus keys and explicit layout.
        foreach (var child in root.GetChildren())
            StackSequentialCards(child);
        if (root is not PanelContainer panel) return;
        var children = panel.GetChildren().OfType<Control>().ToArray();
        if (children.Length <= 1) return;
        var stack = new VBoxContainer
        {
            Name = "SequentialCardContent",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        stack.AddThemeConstantOverride("separation", 6);
        panel.AddChild(stack);
        foreach (var child in children)
            child.Reparent(stack, false);
    }
}
