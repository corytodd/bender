namespace Bender.Core.Layouts;

using System.Collections.Generic;
using System.Text;

/// <summary>
///     Maps a numeric value to a string representation
/// </summary>
public class Enumeration : ILayout
{
    /// <summary>
    ///     Get or Set name of this enumerate
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Get or Set the name lookup table
    /// </summary>
    public Dictionary<int, string> Values { get; set; }

    /// <inheritdoc />
    public IEnumerable<string> EnumerateLayout()
    {
        var content = ToTabbedString().Split('\n');
        foreach (var str in content)
        {
            yield return str;
        }
    }

    /// <summary>
    ///     Returns all properties as newline delimited string
    /// </summary>
    public string ToTabbedString()
    {
        var sb = new StringBuilder();

        sb.AppendFormat("Name: {0}\n", Name);

        sb.AppendLine("\tValues:");
        if (Values is not null)
        {
            foreach (var (key, value) in Values)
            {
                sb.Append($"\t\t{key}: {value}\n");
            }
        }

        return sb.ToString();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Name}";
    }
}