using NGettext.Plural;
using System.Collections.Generic;
using Translatable;

namespace Translation
{
    public class GettextPluralBuilder : IPluralBuilder
    {
        private readonly IPluralRule _pluralRule;

        public GettextPluralBuilder(IPluralRule pluralRule)
        {
            _pluralRule = pluralRule;
        }

        public int NumberOfPlurals => _pluralRule.NumPlurals;

        public string GetPlural(int number, IList<string> pluralForms)
        {
            var pluralRule = _pluralRule.Evaluate(number);
            //Limit the plural rule if the default translation has less plural forms than the current language
            if (pluralRule >= pluralForms.Count)
                pluralRule = pluralForms.Count - 1;

            return pluralForms[pluralRule];
        }

        public string GetFormattedPlural(int number, IList<string> pluralForms)
        {
            return string.Format(GetPlural(number, pluralForms), number);
        }
    }
}