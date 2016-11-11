using System.Collections.Generic;
using System.Globalization;
using NGettext;
using NGettext.Loaders;

namespace Translation
{
    public class TranslationSource
    {
        private readonly Catalog catalog;

        public TranslationSource(string translationFolder, string moDomain, CultureInfo cultureInfo)
        {
            catalog = new Catalog(new MoLoader(moDomain, translationFolder), cultureInfo);
        }

        public IPluralBuilder GetPluralBuilder()
        {
            if (catalog == null)
                return new DefaultPluralBuilder();

            return new GettextPluralBuilder(catalog.PluralRule);
        }

        public string GetTranslation(string translationKey)
        {
            var untranslatedString = "UNTRANSLATED: " + translationKey;

            try
            {
                if (!catalog.Translations.ContainsKey(translationKey))
                    return untranslatedString;

                return catalog.GetString(translationKey);
            }
            catch (KeyNotFoundException)
            {
                return untranslatedString;
            }
        }

        public string[] GetAllTranslations(string translationKey, IPluralBuilder pluralBuilder)
        {
            if (!catalog.Translations.ContainsKey(translationKey))
                return new string[] {};

            var translations = catalog.GetTranslations(translationKey);

            if (translations.Length != pluralBuilder.NumberOfPlurals)
                return new string[] {};

            return translations;
        }
    }
}
