using Godot;
using OpenMakaiRanch.App;

namespace OpenMakaiRanch.World;

/// <summary>
/// Small first-visit onboarding/help surface for Okachi Town.
/// </summary>
public partial class TownTutorialController : Control
{
    private TownWorldController? _town;
    private PanelContainer? _hintCard;
    private Label? _hintLabel;
    private Button? _dismissButton;
    private Button? _helpButton;
    private PanelContainer? _helpPanel;
    private Button? _closeHelpButton;
    private bool _serviceUsed;

    public bool HelpVisible => _helpPanel?.Visible == true;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        _town = GetNodeOrNull<TownWorldController>("../..");
        _hintCard = GetNodeOrNull<PanelContainer>("HintCard");
        _hintLabel = GetNodeOrNull<Label>("HintCard/Inner/HintLabel");
        _dismissButton = GetNodeOrNull<Button>("HintCard/Inner/DismissButton");
        _helpButton = GetNodeOrNull<Button>("HelpButton");
        _helpPanel = GetNodeOrNull<PanelContainer>("HelpPanel");
        _closeHelpButton = GetNodeOrNull<Button>("HelpPanel/Inner/CloseButton");

        if (_helpPanel is not null) _helpPanel.Visible = false;

        if (_town is not null)
        {
            _town.ServiceScreenRequested += OnServiceRequested;
            _town.TravelRequested += OnTravelRequested;
        }

        if (_dismissButton is not null) _dismissButton.Pressed += DismissHint;
        if (_helpButton is not null) _helpButton.Pressed += ToggleHelp;
        if (_closeHelpButton is not null) _closeHelpButton.Pressed += CloseHelp;

        RefreshHint();
    }

    public override void _ExitTree()
    {
        if (_town is not null && GodotObject.IsInstanceValid(_town))
        {
            _town.ServiceScreenRequested -= OnServiceRequested;
            _town.TravelRequested -= OnTravelRequested;
        }
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("open_help"))
        {
            ToggleHelp();
        }
    }

    public void ToggleHelp()
    {
        if (_helpPanel is null || _town is null)
        {
            return;
        }

        if (_helpPanel.Visible)
        {
            CloseHelp();
            return;
        }

        _helpPanel.Visible = true;
        _town.InputGate.SetUiOwnsInput(true);
    }

    public void CloseHelp()
    {
        if (_helpPanel is null || _town is null)
        {
            return;
        }

        _helpPanel.Visible = false;
        _town.InputGate.SetUiOwnsInput(false);
    }

    private void DismissHint()
    {
        GameRoot.Instance?.MarkTutorialSeen("town_basic_hint");
        RefreshHint();
    }

    private void OnServiceRequested(string screenId)
    {
        _serviceUsed = true;
        GameRoot.Instance?.MarkTutorialSeen("town_service_used");
        RefreshHint();
    }

    private void OnTravelRequested(string destinationId)
    {
        if (destinationId == "ranch")
        {
            GameRoot.Instance?.MarkTutorialSeen("town_return_learned");
        }
    }

    private void RefreshHint()
    {
        if (_hintCard is null || _hintLabel is null || GameRoot.Instance is not { } game)
        {
            return;
        }

        if (!game.State.Settings.TutorialHintsEnabled)
        {
            _hintCard.Visible = false;
            return;
        }

        if (!game.HasSeenTutorial("town_basic_hint"))
        {
            _hintCard.Visible = true;
            _hintLabel.Text = "Okachi Town: walk to a building and press F to use its existing service. The south gate or Return to Ranch button takes you home.";
            return;
        }

        if (!game.HasSeenTutorial("town_service_used") && !_serviceUsed)
        {
            _hintCard.Visible = true;
            _hintLabel.Text = "Try a town service: General Store, Adventure Guild, Research Office, Tavern, Bathhouse, Town Hall, or Planning.";
            return;
        }

        _hintCard.Visible = false;
    }
}
