using Godot;

namespace OpenMakaiRanch.World;

public partial class FirstDayFlowController
{
    private Vector2 _dialogueViewport;

    private void StyleOpeningDialogue(VBoxContainer content)
    {
        if (_dialoguePanel is null || _bodyLabel is null || _speakerLabel is null) return;
        _dialoguePanel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = new Color("111d28f5"), BorderColor = new Color("bfab79"),
            BorderWidthTop = 2, BorderWidthBottom = 1, BorderWidthLeft = 1, BorderWidthRight = 1,
            CornerRadiusTopLeft = 14, CornerRadiusTopRight = 14,
            CornerRadiusBottomLeft = 14, CornerRadiusBottomRight = 14,
            ContentMarginLeft = 22, ContentMarginRight = 22, ContentMarginTop = 16, ContentMarginBottom = 16
        });
        _speakerLabel.AddThemeColorOverride("font_color", new Color("ecd49d"));
        _bodyLabel.AddThemeColorOverride("font_color", new Color("edf1f5"));
        _bodyLabel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _bodyLabel.SizeFlagsVertical = SizeFlags.ShrinkBegin;
        var index = _bodyLabel.GetIndex();
        content.RemoveChild(_bodyLabel);
        var scroll = new ScrollContainer
        {
            Name = "NarrativeScroll", SizeFlagsVertical = SizeFlags.ExpandFill,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
            FollowFocus = true
        };
        content.AddChild(scroll);
        content.MoveChild(scroll, index);
        scroll.AddChild(_bodyLabel);
        if (_objectivePanel is not null)
            _objectivePanel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
            {
                BgColor = new Color("111d28df"), CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8,
                CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8,
                ContentMarginLeft = 14, ContentMarginRight = 14, ContentMarginTop = 10, ContentMarginBottom = 10
            });
        UpdateDialogueLayout();
    }

    private void UpdateDialogueLayout()
    {
        if (_dialoguePanel is null) return;
        var viewport = GetViewport().GetVisibleRect().Size;
        if (viewport == _dialogueViewport) return;
        _dialogueViewport = viewport;
        var width = Mathf.Max(240, Mathf.Min(1000, viewport.X - 32));
        var height = Mathf.Min(330, viewport.Y * 0.62f);
        _dialoguePanel.OffsetLeft = -width / 2;
        _dialoguePanel.OffsetRight = width / 2;
        _dialoguePanel.OffsetTop = -height - 18;
        _dialoguePanel.OffsetBottom = -18;
        if (_objectivePanel is not null)
        {
            var objectiveWidth = Mathf.Min(680, viewport.X - 32);
            _objectivePanel.OffsetLeft = -objectiveWidth / 2;
            _objectivePanel.OffsetRight = objectiveWidth / 2;
            _objectivePanel.OffsetTop = 72;
            _objectivePanel.OffsetBottom = 134;
        }
        if (_skipButton is not null)
        {
            _skipButton.OffsetLeft = -228;
            _skipButton.OffsetTop = 18;
            _skipButton.OffsetRight = -16;
            _skipButton.OffsetBottom = 58;
        }
    }
}
