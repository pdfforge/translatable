using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NGettext;
using NGettext.Loaders;

namespace Translation
{
    public class TranslationSource
    {
        private readonly Dictionary<string, Catalog> _translationCatalogs = new Dictionary<string, Catalog>();

        public TranslationSource(string translationFolder, CultureInfo cultureInfo)
        {
            var folder = Path.Combine(translationFolder, cultureInfo.Name);
            foreach (var moFile in Directory.EnumerateFiles(folder, "*.mo", SearchOption.AllDirectories))
            {
                using (var moStream = File.OpenRead(moFile))
                {
                    var loader = new MoLoader(moStream);
                    var moDomain = Path.GetFileNameWithoutExtension(moFile);
                    _translationCatalogs[moDomain] = new Catalog(loader, cultureInfo);
                }
            }
        }

        public IPluralBuilder GetPluralBuilder()
        {
            if (_translationCatalogs.Count == 0)
                return new DefaultPluralBuilder();

            return new GettextPluralBuilder(_translationCatalogs.First().Value.PluralRule);
        }

        public string GetTranslation(string section, string translationKey)
        {
            var untranslatedString = "UNTRANSLATED: " + translationKey;

            Catalog catalog;

            if (!_translationCatalogs.TryGetValue(section, out catalog))
                return untranslatedString;

            if (!catalog.Translations.ContainsKey(translationKey))
                return untranslatedString;

            return catalog.GetString(translationKey);
        }

        public string[] GetAllTranslations(string section, string translationKey, IPluralBuilder pluralBuilder)
        {
            Catalog catalog;
            if (!_translationCatalogs.TryGetValue(section, out catalog))
                return new string[] {};

            if (!catalog.Translations.ContainsKey(translationKey))
                return new string[] { };

            var translations = catalog.GetTranslations(translationKey);

            if (translations.Length != pluralBuilder.NumberOfPlurals)
                return new string[] {};

            return translations;
        }
    }
}
