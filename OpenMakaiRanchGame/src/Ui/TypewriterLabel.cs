using Godot;

namespace OpenMakaiRanch.Ui;

/// <summary>
/// Label that reveals text progressively (typewriter effect).
/// Reveals characters over time until the full text is shown; Finish() reveals at once.
/// </summary>
public partial class TypewriterLabel : Label
{
	private string _fullText = string.Empty;
	private int _displayed;
	private double _time;
	private double _tickInterval = 0.045;
	private int _charsPerTick = 2;
	private bool _complete;

	public bool IsComplete => _complete;

	/// <summary>Set the raw text (stored; not yet shown) so the effect can reveal it progressively.</summary>
	public void Begin(string text, double tickInterval = 0.045, int charsPerTick = 2)
	{
		_fullText = text;
		_tickInterval = Mathf.Max(0.01f, (float)tickInterval);
		_charsPerTick = Mathf.Max(1, charsPerTick);
		_displayed = 0;
		_time = 0;
		_complete = false;
		Text = string.Empty;
		VisibleCharacters = 0;
	}

	/// <summary>Instantly reveal the whole line.</summary>
	public void Finish()
	{
		_complete = true;
		Text = _fullText;
		VisibleCharacters = _fullText.Length;
	}

	public override void _Process(double delta)
	{
		if (_complete || _fullText.Length == 0)
		{
			return;
		}

		if (_displayed >= _fullText.Length)
		{
			Finish();
			return;
		}

		_time += delta;
		while (_time >= _tickInterval && _displayed < _fullText.Length)
		{
			_displayed = Mathf.Min(_fullText.Length, _displayed + _charsPerTick);
			_time -= _tickInterval;
		}

		Text = _fullText[.._displayed];
		VisibleCharacters = _displayed;
		if (_displayed >= _fullText.Length)
		{
			_complete = true;
			_time = 0;
		}
	}
}