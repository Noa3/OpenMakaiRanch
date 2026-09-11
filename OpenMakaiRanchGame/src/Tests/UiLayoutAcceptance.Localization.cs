using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;
using OpenMakaiRanch.App;
using OpenMakaiRanch.Locale;
using OpenMakaiRanch.Ui;
using OpenMakaiRanch.World;

namespace OpenMakaiRanch.Tests;

public partial class UiLayoutAcceptance
{
    private async Task CheckLocalizedTitle(MainMenuController menu)
    {
        var game = GameRoot.Instance;
        var state = game.State;
        var generation = game.StateGeneration;
        var day = state.Calendar.Day;
        var gold = game.Economy.Gold;
        var reduced = state.Settings.ReducedMotion;
        var originalLocale = LocaleCatalog.CurrentLocale;
        try
        {
            Check(LocaleCatalog.NormalizeLocale(" DE-de ") == "de"
                && LocaleCatalog.NormalizeLocale("ja_JP") == "ja"
                && LocaleCatalog.NormalizeLocale("not-a-language") == "en",
                "locale: supported regional language codes normalize; unknown codes use English");
            Check(LocaleCatalog.FormatForDisplay("Broken {9}", "Day {0}", CultureInfo.GetCultureInfo("de"), 3) == "Day 3"
                && LocaleCatalog.FormatForDisplay("Value {", "Value {0}", CultureInfo.GetCultureInfo("de"), 2) == "Value 2",
                "locale: malformed or out-of-range translation placeholders fall back without interrupting UI callbacks");
            var de = CultureInfo.GetCultureInfo("de");
            Check(LocaleCatalog.FormatForDisplay("Missing price", "Price {0}", de, 6) == "Price 6"
                && LocaleCatalog.FormatForDisplay("{0} {2}", "{0} {1}", de, "A", "B") == "A B",
                "locale: dropped or extra slots retain the English price and identity");
            Check(LocaleCatalog.FormatForDisplay("{1} / {0}", "{0} / {1}", de, "A", "B") == "B / A"
                && LocaleCatalog.FormatForDisplay("{0,9999}", "{0}", de, "A") == "A",
                "locale: reordering remains supported while excessive alignment falls back");
            var vista = menu.GetNode<SubViewport>("Root/Background/TitleVista/VistaViewport");
            Check(vista.OwnWorld3D && vista.GetNodeOrNull<Camera3D>("VistaWorld/VistaCamera") is not null,
                "title: the rendered diorama owns a separate world and camera instead of loading a player save");
            game.SetReducedMotion(true);
            await Frames(4);
            var camera = vista.GetNode<Camera3D>("VistaWorld/VistaCamera");
            var still = camera.Transform;
            await Frames(12);
            Check(camera.Transform.IsEqualApprox(still), "title: Reduced Motion stops the background camera");
            var culture = CultureInfo.CurrentCulture;
            var windowSize = GetWindow().Size;
            var windowMode = GetWindow().Mode;
            await ChooseLanguage(menu.GetNode<OptionButton>(menu.LangPickerPath), "de");
            Check(menu.GetNode<Button>(menu.NewGameButtonPath).Text == "Neues Spiel"
                && LocaleCatalog.T("world.direction.distance", "{0} {1:0.0} m", "→", 12.3) == "→ 12 m"
                && LocaleCatalog.FormatForDisplay("{0:0.0}", "{0:0.0}", CultureInfo.GetCultureInfo("de"), 6.5) == "6,5"
                && Equals(CultureInfo.CurrentCulture, culture),
                "locale: translated title and display-only number culture leave global parsing culture unchanged");
            Check(GetWindow().Size == windowSize && GetWindow().Mode == windowMode,
                "locale: selecting a language leaves the actual window size and display mode unchanged");
            Check(LocaleCatalog.T("world.test.missing", "English fallback") == "English fallback",
                "locale: missing translation keys keep readable English instead of exposing IDs");
            foreach (var size in new[] { new Vector2I(640, 480), new Vector2I(480, 800) })
            {
                await Resize(size);
                Check(Encloses(GetViewport().GetVisibleRect(), menu.GetNode<Control>(menu.PanelPath).GetGlobalRect())
                    && VisibleTarget(menu.GetNode<Button>(menu.NewGameButtonPath))
                    && VisibleTarget(menu.GetNode<OptionButton>(menu.LangPickerPath)),
                    $"locale: German title, main action and native-language picker fit {size.X}x{size.Y}");
                await Capture($"title-german-{size.X}x{size.Y}");
            }
            Check(ReferenceEquals(state, game.State) && game.StateGeneration == generation
                && game.State.Calendar.Day == day && game.Economy.Gold == gold,
                "title: diorama, resizing and actual language selection do not create a session, advance time or spend money");
        }
        finally
        {
            game.SetLocale(originalLocale);
            game.SetReducedMotion(reduced);
            await Resize(new Vector2I(1280, 720));
        }
    }

    private async Task CheckLocalizationJourney(WorldGameController world)
    {
        var game = GameRoot.Instance;
        var originalLocale = LocaleCatalog.CurrentLocale;
        world.CloseManagement();
        world.Transition?.CompleteImmediately();
        await Frames(8);
        var state = game.State;
        var generation = game.StateGeneration;
        var day = state.Calendar.Day;
        var phase = state.Calendar.Phase;
        var gold = game.Economy.Gold;
        var stamina = state.Player.Stamina;
        var assignments = JsonSerializer.Serialize(state.Schedule.AssignedJobs);
        var inventory = JsonSerializer.Serialize(state.Inventory.Items);
        var stock = JsonSerializer.Serialize(state.Ranch.Stockpile);
        var area = world.ActiveAreaId;
        world.NavigationGuide!.Track(WorldDestinationCatalog.Office);
        var before = world.ActivePlayer!.GlobalPosition;
        try
        {
            Check(world.OpenDedicatedService("options"), "locale: physical-world utilities open the dedicated Options page");
            await Resize(new Vector2I(640, 480));
            await Frames(8);
            var picker = Descendants(world.Shell!).OfType<OptionButton>().Single(control => control.Name == "LanguagePicker");
            await ChooseLanguage(picker, "de");
            Check(world.Shell!.CurrentScreen == "options" && world.Shell.IsDedicatedService
                && Descendants(world.Shell).OfType<OptionButton>().Single(control => control.Name == "LanguagePicker").Selected == 1,
                "locale: the actual options picker updates the existing language setting and retains its dedicated route");
            await Capture("options-language-german-640x480");
            await ClickStationButton(Descendants(world.Shell).OfType<Button>().Single(button => button.Name == "ReturnToWorldButton"));
            await Frames(8);
            var after = world.ActivePlayer!.GlobalPosition;
            Check(!world.IsManagementVisible && world.ActiveAreaId == area
                && new Vector2(before.X - after.X, before.Z - after.Z).Length() < 0.1f
                && world.NavigationGuide.Target?.TargetId == "office" && world.NavigationGuide.Target.DisplayName == "Büro",
                "locale: leaving translated Options retains the same world position and selected destination ID");

            foreach (var size in new[] { new Vector2I(640, 480), new Vector2I(480, 800) })
            {
                await Resize(size);
                world.OpenWorldGuide();
                await Frames(8);
                var panel = world.StationPanel!;
                Check(panel.Visible && Descendants(panel).OfType<Label>().Single(label => label.Name == "StationTitle").Text == "Orte",
                    $"locale: the read-only places guide uses German at {size.X}x{size.Y}");
                CheckLocalizedPanelGeometry(panel, $"German places {size.X}x{size.Y}");
                await Capture($"places-german-{size.X}x{size.Y}");
                var target = Descendants(panel).OfType<Button>().Single(button => button.Name == "Place_dairy_barn");
                await ClickStationButton(target);
                Check(!world.IsStationPanelOpen && world.NavigationGuide.Target?.TargetId == "dairy_barn"
                    && world.NavigationGuide.Target.DisplayName == "Milchstall",
                    "locale: a translated destination button retains the stable station ID and only marks the route");
            }

            // Proximity is staged separately from the earlier physically walked doorway test.
            var station = world.ResolveStation("dairy_barn")!;
            world.ActivePlayer!.GlobalPosition = station.GlobalPosition + new Vector3(0, 0.3f, 0);
            world.ActivePlayer.Velocity = Vector3.Zero;
            await Frames(8);
            Check(world.OpenStation(station), "locale: the nearby barn opens its dedicated translated station");
            await Resize(new Vector2I(640, 480));
            await Frames(8);
            var work = world.StationPanel!;
            CheckLocalizedPanelGeometry(work, "German dairy 640x480");
            Check(Descendants(work).OfType<Label>().Single(label => label.Name == "StationTitle").Text == "Milchstall",
                "locale: the dairy station title is translated independently of its canonical ID");
            await ClickStationButton(Descendants(work).OfType<Button>().Single(button => button.Name == "StationTab_team"));
            var focused = Descendants(work).OfType<Button>().First(button => !button.Disabled
                && button.Name.ToString().StartsWith("Rest_", StringComparison.Ordinal));
            focused.GrabFocus(); await Frames(6);
            var focusName = focused.Name;
            game.NotifyStateChanged();
            await Frames(8);
            Check(GetViewport().GuiGetFocusOwner() is Button restored && restored.Name == focusName && VisibleTarget(restored),
                "locale: rebuilding a station preserves the logical work-button focus and keeps it reachable");
            await Capture("dairy-german-640x480");
            game.SetLocale("en");
            await Frames(8);
            Check(work.Visible && work.ContextId == "dairy_barn"
                && Descendants(work).OfType<Label>().Single(label => label.Name == "StationTitle").Text == "Dairy Barn"
                && GetViewport().GuiGetFocusOwner()?.Name == focusName,
                "locale: switching languages refreshes an open station without changing context or its focused action");
            Check(ReferenceEquals(state, game.State) && game.StateGeneration == generation
                && state.Calendar.Day == day && state.Calendar.Phase == phase && game.Economy.Gold == gold
                && state.Player.Stamina == stamina && JsonSerializer.Serialize(state.Schedule.AssignedJobs) == assignments
                && JsonSerializer.Serialize(state.Inventory.Items) == inventory && JsonSerializer.Serialize(state.Ranch.Stockpile) == stock,
                "locale: all language, guide and station inspections leave session, calendar, wallet, inventory, stock and jobs unchanged");
        }
        finally
        {
            world.CloseManagement();
            game.SetLocale(originalLocale);
            world.NavigationGuide!.ClearTarget();
            await Frames(6);
        }
    }

    private void CheckLocalizedPanelGeometry(WorldStationPanel panel, string context)
    {
        var card = panel.GetNode<Control>("StationCard");
        var scroll = Descendants(card).OfType<ScrollContainer>().Single(control => control.Name == "StationScroll");
        var content = Descendants(card).OfType<VBoxContainer>().Single(control => control.Name == "StationContent");
        var children = content.GetChildren().OfType<Control>().Where(control => control.Visible).ToArray();
        Check(Encloses(GetViewport().GetVisibleRect(), card.GetGlobalRect()) && content.Size.X <= scroll.Size.X + 1
            && children.Zip(children.Skip(1), (a, b) => a.GetGlobalRect().End.Y <= b.GlobalPosition.Y + 1).All(value => value)
            && VisibleTarget(Descendants(panel).OfType<Button>().Single(button => button.Name == "StationClose")),
            $"{context}: translated controls do not overlap, require no horizontal scroll, and keep Back visible");
    }

    private async Task ChooseLanguage(OptionButton picker, string locale)
    {
        picker.GrabFocus(); await Frames(6);
        Check(VisibleTarget(picker), "locale: the language picker is a visible physical hit target");
        // Use the engine's input entry point, including global button state and WindowId.
        // Viewport.PushInput alone skips the mouse mask used to suppress the opening release
        // in PopupMenu; emitting WindowInput alone skips Window's native virtual handler.
        var owner = picker.GetWindow();
        var point = owner.GetFinalTransform() * (picker.GetGlobalTransformWithCanvas() * (picker.Size / 2f));
        var ownerId = owner.GetWindowId();
        Input.ParseInputEvent(new InputEventMouseMotion { WindowId = ownerId,
            Position = point, GlobalPosition = point });
        Input.ParseInputEvent(new InputEventMouseButton { WindowId = ownerId,
            Position = point, GlobalPosition = point, ButtonIndex = MouseButton.Left, Pressed = true });
        Input.FlushBufferedEvents();
        await Frames(1);
        Input.ParseInputEvent(new InputEventMouseButton { WindowId = ownerId,
            Position = point, GlobalPosition = point, ButtonIndex = MouseButton.Left, Pressed = false });
        Input.FlushBufferedEvents();
        await Frames(4);
        Check(GodotObject.IsInstanceValid(picker),
            "locale: the opening click cannot choose a language or retire its own picker");
        if (!GodotObject.IsInstanceValid(picker))
            throw new InvalidOperationException("Opening the language popup retired its picker before a choice.");
        var popup = picker.GetPopup();
        Check(popup.Visible, "locale: clicking the language picker opens its actual popup");
        if (!popup.Visible) throw new InvalidOperationException("The language popup did not open.");
        var index = Array.IndexOf(LocaleCatalog.AvailableLocales, locale);
        if (index < 0) throw new InvalidOperationException("The requested test language is not offered.");
        var inputWindow = popup.IsEmbedded() ? ownerId : popup.GetWindowId();
        async Task PopupKey(Key key)
        {
            Input.ParseInputEvent(new InputEventKey { WindowId = inputWindow,
                Keycode = key, PhysicalKeycode = key, Pressed = true });
            Input.FlushBufferedEvents();
            await Frames(1);
            Input.ParseInputEvent(new InputEventKey { WindowId = inputWindow,
                Keycode = key, PhysicalKeycode = key, Pressed = false });
            Input.FlushBufferedEvents();
            await Frames(3);
        }
        try
        {
            // Home is not a PopupMenu command. Walk the actual focused item using Down,
            // with a bounded loop; never set Selected, ItemSelected or the language directly.
            for (var step = 0; step <= popup.ItemCount && popup.GetFocusedItem() != index; step++)
                await PopupKey(Key.Down);
            var focused = popup.GetFocusedItem();
            Check(focused == index,
                $"locale: engine-routed keys focus the requested popup item (target={index}, actual={focused}, embedded={popup.IsEmbedded()})");
            if (focused != index) throw new InvalidOperationException("The actual popup did not accept navigation keys.");
            await PopupKey(Key.Enter);
            await Frames(10);
            Check(LocaleCatalog.CurrentLocale == locale && GameRoot.Instance.State.Settings.Locale == locale,
                $"locale: the actual popup selection updates catalog and saved language to {locale}");
            if (LocaleCatalog.CurrentLocale != locale)
                throw new InvalidOperationException("The actual language choice did not update the catalog.");
        }
        finally
        {
            if (GodotObject.IsInstanceValid(popup) && popup.Visible) popup.Hide();
        }
    }
}
