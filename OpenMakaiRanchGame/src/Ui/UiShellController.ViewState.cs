using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace OpenMakaiRanch.Ui;

/// <summary>Preserves focus/scroll only for the current view; never retains gameplay state.</summary>
public partial class UiShellController
{
    private bool _routingScreen;
    private ulong _viewRevision;
    private sealed record ContentFocus(string Key, int Occurrence, int Index);

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
        var generation = _game.StateGeneration;
        Callable.From(() =>
        {
            if (!GodotObject.IsInstanceValid(this) || !IsInsideTree() || !IsVisibleInTree()
                || _viewRevision != revision || _game.StateGeneration != generation) return;
            _scroll.ScrollVertical = scroll;
            if (focus is null) return;
            var controls = FocusableControls(_content).ToList();
            var target = controls.Where(control => FocusKey(control) == focus.Key).Skip(focus.Occurrence).FirstOrDefault()
                ?? (controls.Count > 0 ? controls[Math.Min(focus.Index, controls.Count - 1)] : null);
            target?.GrabFocus();
        }).CallDeferred();
    }
}
