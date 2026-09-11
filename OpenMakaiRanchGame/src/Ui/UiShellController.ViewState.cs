using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace OpenMakaiRanch.Ui;

/// <summary>Preserves focus/scroll only for the current view; never retains gameplay state or retired controls.</summary>
public partial class UiShellController
{
    private bool _routingScreen;
    private ulong _viewRevision;
    private sealed record ContentFocus(string Key, int Occurrence, int Index);
    private sealed record ContentView(ulong Revision, ulong Generation, string Screen, int Scroll, ContentFocus? Focus);
    private ContentView? _pendingContentView;

    private bool HasPendingContentView => _pendingContentView is { } view
        && view.Revision == _viewRevision && view.Generation == _game.StateGeneration
        && view.Screen == _currentScreen;

    private int CaptureContentScroll() => HasPendingContentView
        ? _pendingContentView!.Scroll : _scroll.ScrollVertical;

    private static IEnumerable<Control> FocusableControls(Node root)
    {
        foreach (var child in root.GetChildren())
        {
            if (child is Control control && control.IsVisibleInTree()
                && control.FocusMode != FocusModeEnum.None && control is not BaseButton { Disabled: true })
                yield return control;
            foreach (var nested in FocusableControls(child)) yield return nested;
        }
    }

    private static string FocusKey(Control control) => control.GetType().Name + ":"
        + (control.Name.ToString().StartsWith('@')
            ? control is Button button ? button.Text : string.Empty : control.Name.ToString());

    private ContentFocus? CaptureContentFocus()
    {
        if (!IsVisibleInTree()) return null;
        // Several StateChanged notifications may rebuild the same view before Godot lays it out.
        // Keep the original snapshot rather than capturing a temporarily empty/clamped view.
        if (HasPendingContentView) return _pendingContentView!.Focus;
        var owner = GetViewport().GuiGetFocusOwner();
        if (owner is null || !_content.IsAncestorOf(owner)) return null;
        var controls = FocusableControls(_content).ToList();
        var index = controls.IndexOf(owner);
        if (index < 0) return null;
        var key = FocusKey(owner);
        return new ContentFocus(key, controls.Take(index).Count(control => FocusKey(control) == key), index);
    }

    private Control? FirstMenuFocus() => (_compactNavigationScroll.IsVisibleInTree()
            ? FocusableControls(_compactNavigation).FirstOrDefault()
            : FocusableControls(_navigation).FirstOrDefault())
        ?? FocusableControls(_content).FirstOrDefault();

    private void RestoreContentViewDeferred(ulong revision, int scroll, ContentFocus? focus)
    {
        _scroll.FollowFocus = true;
        _compactNavigationScroll.FollowFocus = true;
        // Options used to append this card in _Process, after restoration had already run.
        // Compose the complete view before waiting for its container layout.
        EnsureInputOptionsExtension();
        FinalizeSequentialManagementCards();
        var view = new ContentView(revision, _game.StateGeneration, _currentScreen, scroll, focus);
        _pendingContentView = view;
        var tree = GetTree();
        void AfterLayout()
        {
            // One-shot even when the shell was hidden, freed, rerouted, or replaced by a load.
            tree.ProcessFrame -= AfterLayout;
            if (!GodotObject.IsInstanceValid(this) || !IsInsideTree()
                || !ReferenceEquals(_pendingContentView, view)) return;
            _pendingContentView = null;
            if (!IsVisibleInTree() || _viewRevision != view.Revision
                || _game.StateGeneration != view.Generation || _currentScreen != view.Screen) return;

            if (view.Focus is { } savedFocus)
            {
                var controls = FocusableControls(_content).ToList();
                var target = controls.Where(control => FocusKey(control) == savedFocus.Key)
                    .Skip(savedFocus.Occurrence).FirstOrDefault()
                    ?? (controls.Count > 0 ? controls[Math.Min(savedFocus.Index, controls.Count - 1)] : null);
                target?.GrabFocus();
            }
            // Restore after focus: FollowFocus may otherwise replace the user's chosen position.
            // ScrollContainer clamps naturally if the new content is shorter.
            _scroll.ScrollVertical = view.Scroll;
        }
        // CallDeferred alone can run in the same message-queue flush as nested container sorts.
        // Godot requires a process-frame boundary before scrolling to newly added controls.
        tree.ProcessFrame += AfterLayout;
    }
}
