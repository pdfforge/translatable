using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace Translation
{
    public class TranslationFactory
    {
        private readonly string _translationDir;
        private TranslationSource _translationSource;

        public TranslationFactory(string translationDir)
        {
            _translationDir = translationDir;
            SetLanguage(new CultureInfo("en-US"));
        }

        public void SetLanguage(CultureInfo culture)
        {
            _translationSource = new TranslationSource(_translationDir, "messages", culture);
        }

        public T CreateTranslation<T>() where T: ITranslatable
        {
            var instance = Activator.CreateInstance<T>();

            Translate(instance, _translationSource);

            return instance;
        }

        private void Translate(ITranslatable o, TranslationSource translationSource)
        {
            var type = o.GetType();

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            IPluralBuilder pluralBuilder = translationSource.GetPluralBuilder();

            foreach (var property in properties)
            {
                if (!property.CanRead || !property.CanWrite)
                    continue;

                if (property.PropertyType == typeof(string))
                {
                    SetStringProperty(o, property, translationSource);
                    continue;
                }

                if (property.PropertyType == typeof(string[]))
                {
                    SetStringArrayProperty(o, property, pluralBuilder, translationSource);
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

        private void SetStringProperty(ITranslatable o, PropertyInfo property, TranslationSource translationSource)
        {
            var value = (string)property.GetValue(o);

            var translated = translationSource.GetTranslation(value);

            if (!string.IsNullOrEmpty(translated))
                property.SetValue(o, translated);
        }

        private void SetStringArrayProperty(ITranslatable o, PropertyInfo property, IPluralBuilder pluralBuilder, TranslationSource translationSource)
        {
            var value = (string[])property.GetValue(o);

            if (value.Length != 2)
                throw new InvalidDataException($"The plural string for key {property.Name} must contain two strings: a singular and a plural form. It contained {value.Length} strings.");

            var translations = translationSource.GetAllTranslations(value[0], pluralBuilder);

            if (translations.Length == pluralBuilder.NumberOfPlurals)
                property.SetValue(o, translations);
        }
    }
}
