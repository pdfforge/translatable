using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
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

        public string GetTranslation(string section, string translationKey)
        {
            var untranslatedString = "UNTRANSLATED: " + translationKey;

            Catalog catalog;
            if (!_translationCatalogs.TryGetValue(section, out catalog))
                return untranslatedString;

            if (!catalog.Translations.ContainsKey(translationKey))
                return untranslatedString;

            var translation = catalog.GetString(translationKey);

            return translation;
        }

        public void Translate(ITranslatable o)
        {
            var type = o.GetType();
            var translationSection = type.FullName;

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (!property.CanRead || !property.CanWrite)
                    continue;

                if (!property.PropertyType.Equals(typeof(string)))
                    continue;

                var value = (string)property.GetValue(o);

                var translated = GetTranslation(translationSection, value);

                if (!string.IsNullOrEmpty(translated))
                    property.SetValue(o, translated);
            }
        }
    }
}
