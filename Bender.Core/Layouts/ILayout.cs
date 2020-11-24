namespace Bender.Core.Layouts
{
    using System.Collections.Generic;

    /// <summary>
    /// A type that can print itself recursively to a string buffer
    /// </summary>
    public interface ILayout
    {
        /// <summary>
        /// Iterates over all public properties of this type to generate
        /// a formatted string representation. Each line in the result
        /// represents a natural newline break.
        /// </summary>
        /// <returns>Formatted content</returns>
        IEnumerable<string> EnumerateLayout();

        /// <summary>
        /// Returns contents as a formatted, tabbed, new-lined string
        /// suitable for printing on the console.
        /// </summary>
        /// <returns>string</returns>
        string ToTabbedString();
    }
}