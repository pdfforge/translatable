using System.Collections.Generic;
using System.Reflection;

namespace Translation
{
    public class TranslationSource
    {
        public IDictionary<string, string> Translations { get; private set; } = new Dictionary<string, string>();

        public string GetTranslation(string section, string translationKey)
        {
            if (Translations.ContainsKey(translationKey))
                return Translations[translationKey];

            return "UNTRANSLATED: " + translationKey;
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
