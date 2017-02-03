using System.Globalization;
using System.IO;
using System.Linq;
using NGettext;
using Translation;

namespace Translatable.NGettext
{
    public class GettextTranslationSource : ITranslationSource
    {
        private readonly Catalog _catalog;

        public GettextTranslationSource(string translationFolder, string moDomain, CultureInfo cultureInfo)
        {
            _catalog = new Catalog(moDomain, translationFolder, cultureInfo);
        }

        public GettextTranslationSource(Stream moStream, CultureInfo cultureInfo)
        {
            _catalog = new Catalog(moStream, cultureInfo);
        }

        public GettextTranslationSource(Catalog catalog)
        {
            _catalog = catalog;
        }

        public IPluralBuilder GetPluralBuilder()
        {
            if (_catalog == null)
                return new DefaultPluralBuilder();

            return new GettextPluralBuilder(_catalog.PluralRule);
        }

        public string GetTranslation(string translationKey, string context = "")
        {
            if (string.IsNullOrWhiteSpace(context))
                return _catalog.GetString(translationKey);

            return _catalog.GetParticularString(context, translationKey);
        }

        public string[] GetAllTranslations(string translationKey, string context, IPluralBuilder pluralBuilder)
        {
            if (!_catalog.Translations.ContainsKey(translationKey))
                return new string[] {};

            if (!string.IsNullOrWhiteSpace(context))
                translationKey = context + Catalog.CONTEXT_GLUE + translationKey;

            var translations = _catalog.GetTranslations(translationKey);

            if (translations.Length != pluralBuilder.NumberOfPlurals)
                return new string[] {};

            return translations;
        }
    }
}
