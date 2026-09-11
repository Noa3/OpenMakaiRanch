using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace OpenMakaiRanch.Locale;

/// <summary>Bounded UI format validation; translated text never becomes a command or a data ID.</summary>
internal static class LocaleTemplate
{
    internal static bool TryArguments(string text, out HashSet<int> arguments)
    {
        arguments = new HashSet<int>();
        if (text.Length > 8192) return false;
        try { CompositeFormat.Parse(text); }
        catch (FormatException) { return false; }
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] != '{') continue;
            if (i + 1 < text.Length && text[i + 1] == '{') { i++; continue; }
            var start = ++i;
            while (i < text.Length && char.IsAsciiDigit(text[i])) i++;
            if (!int.TryParse(text.AsSpan(start, i - start), NumberStyles.None, CultureInfo.InvariantCulture, out var index)
                || index is < 0 or > 31) return false;
            arguments.Add(index);
            while (i < text.Length && text[i] == ' ') i++;
            if (i < text.Length && text[i] == ',')
            {
                start = ++i;
                while (i < text.Length && text[i] != ':' && text[i] != '}') i++;
                if (!int.TryParse(text.AsSpan(start, i - start), NumberStyles.Integer, CultureInfo.InvariantCulture, out var width)
                    || width is < -256 or > 256) return false;
            }
            while (i < text.Length && text[i] != '}') i++;
        }
        return true;
    }

    internal static bool Matches(string translated, string english, int argumentCount)
    {
        if (!TryArguments(english, out var expected) || !TryArguments(translated, out var actual)
            || !expected.SetEquals(actual)) return false;
        foreach (var index in actual) if (index >= argumentCount) return false;
        return true;
    }

    // Acceptance-only expansion: literal text grows, while placeholders and inserted player
    // names/values remain untouched. This does not add a shipped locale or change save data.
    internal static string Expand(string template)
    {
        var result = new StringBuilder("［");
        var token = false;
        foreach (var ch in template)
        {
            if (ch == '{') token = true;
            result.Append(ch);
            if (!token && "aeiouAEIOU".Contains(ch)) result.Append(ch).Append(ch);
            if (ch == '}') token = false;
        }
        return result.Append('］').ToString();
    }
}
