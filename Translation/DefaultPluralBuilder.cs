using System;
using System.Collections.Generic;

namespace Translatable
{
    public class DefaultPluralBuilder : IPluralBuilder
    {
        public int NumberOfPlurals => 2;

        public string GetPlural(int number, IList<string> pluralForms)
        {
            if (pluralForms.Count != NumberOfPlurals)
                throw new InvalidOperationException($"The number of provided plural strings does not match the number of plurals required for this PluralBuilder.\r\nRequired: {NumberOfPlurals}\r\nProvided: {pluralForms.Count}");

            if (number == 1)
                return pluralForms[0];

            return pluralForms[1];
        }

        public string GetFormattedPlural(int number, IList<string> pluralForms)
        {
            var plural = GetPlural(number, pluralForms);
            return string.Format(plural, number);
        }
    }
}
