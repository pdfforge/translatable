using System;
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

        public void Translate(ITranslatable o)
        {
            var type = o.GetType();
            var translationSection = type.FullName;

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            IPluralBuilder pluralBuilder = new DefaultPluralBuilder();

            foreach (var property in properties)
            {
                if (!property.CanRead || !property.CanWrite)
                    continue;

                if (property.PropertyType == typeof(string))
                {
                    SetStringProperty(o, property, translationSection);
                    continue;
                }

                if (property.PropertyType == typeof(string[]))
                {
                    SetStringArrayProperty(o, property, translationSection, pluralBuilder);
                    continue;
                }

                if (property.PropertyType == typeof(IPluralBuilder))
                {
                    SetPluralbuilderProperty(o, property, pluralBuilder);
                    continue;
                }

                throw new InvalidOperationException($"The type {property.PropertyType} is not supported in ITranslatables.");
            }
        }

        private void SetPluralbuilderProperty(ITranslatable translatable, PropertyInfo property, IPluralBuilder pluralBuilder)
        {
            property.SetValue(translatable, pluralBuilder);
        }

        private void SetStringProperty(ITranslatable o, PropertyInfo property, string translationSection)
        {
            var value = (string) property.GetValue(o);

            var translated = GetTranslation(translationSection, value);

            if (!string.IsNullOrEmpty(translated))
                property.SetValue(o, translated);
        }

        private void SetStringArrayProperty(ITranslatable o, PropertyInfo property, string translationSection, IPluralBuilder pluralBuilder)
        {
            var value = (string[])property.GetValue(o);

            if (value.Length != 2)
                throw new InvalidDataException($"The plural string for section {translationSection} and key {property.Name} must contain two strings: a singular and a plural form. It contained {value.Length} strings.");

            var translations = GetAllTranslations(translationSection, value[0], pluralBuilder);

            if (translations.Length == pluralBuilder.NumberOfPlurals)
                property.SetValue(o, translations);
        }
    }
}
