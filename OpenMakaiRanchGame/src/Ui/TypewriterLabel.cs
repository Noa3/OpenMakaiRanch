using System;
using System.Linq;
using System.Text;
using Godot;

namespace OpenMakaiRanch.Ui;

/// <summary>
/// Reveals a stable, complete text layout using Godot's character visibility, not substring edits.
/// Initializing Text in an authored scene or object initializer is supported as well as Begin.
/// </summary>
public partial class TypewriterLabel : Label
{
    private bool _begun;
    private bool _complete;
    private int _displayed;
    private int _total;
    private double _time;
    private double _tickInterval = 0.045;
    private int _charsPerTick = 2;

    public bool IsComplete => _begun ? _complete : string.IsNullOrEmpty(Text);

    public override void _Ready()
    {
        if (!_begun) Begin(Text);
    }

    public void Begin(string text, double tickInterval = 0.045, int charsPerTick = 2)
    {
        Text = text ?? string.Empty;
        _total = Text.EnumerateRunes().Count();
        _tickInterval = double.IsFinite(tickInterval) ? Math.Max(0.01, tickInterval) : 0.045;
        _charsPerTick = Math.Max(1, charsPerTick);
        _displayed = 0;
        _time = 0;
        _begun = true;
        _complete = _total == 0;
        VisibleCharacters = _complete ? -1 : 0;
    }

    /// <summary>Finish is idempotent and never replaces or erases the displayed line.</summary>
    public void Finish()
    {
        if (!_begun) Begin(Text);
        _displayed = _total;
        _complete = true;
        _time = 0;
        VisibleCharacters = -1;
    }

    public override void _Process(double delta)
    {
        if (!_begun) Begin(Text);
        if (_complete || !double.IsFinite(delta) || delta <= 0) return;
        _time += Math.Min(delta, 1.0);
        var ticks = (int)Math.Min(_total, Math.Floor(_time / _tickInterval));
        if (ticks <= 0) return;
        _time -= ticks * _tickInterval;
        _displayed = (int)Math.Min(_total, (long)_displayed + (long)ticks * _charsPerTick);
        VisibleCharacters = _displayed;
        if (_displayed >= _total) Finish();
    }
}
