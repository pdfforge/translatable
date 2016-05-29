using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NGettext.Plural;

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
            return pluralForms[_pluralRule.Evaluate(number)];
        }

        public string GetFormattedPlural(int number, IList<string> pluralForms)
        {
            return string.Format(GetPlural(number, pluralForms), number);
        }
    }
}
