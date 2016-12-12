using System.Collections.Generic;

namespace Translatable
{
    public interface IPluralBuilder
    {
        /// <summary>
        /// Number of plural forms for this PluralBuilder.
        /// The common default is 2 (singular and plural)
        /// </summary>
        int NumberOfPlurals { get; }

        /// <summary>
        /// Analyzes the number and selects the appropriate plural form from the list of plural form strings
        /// </summary>
        /// <param name="number">The number to analyze</param>
        /// <param name="pluralForms">All available plural forms (including the singular form) for this translation</param>
        /// <returns></returns>
        string GetPlural(int number, IList<string> pluralForms);

        /// <summary>
        /// Analyzes the number and selects the appropriate plural form from the list of plural form strings and then applies string.Format.
        /// </summary>
        /// <param name="number">The number to analyze</param>
        /// <param name="pluralForms">All available plural forms (including the singular form) for this translation</param>
        /// <returns></returns>
        string GetFormattedPlural(int number, IList<string> pluralForms);
    }
}
