using System;
using System.IO;
using System.Reflection;

namespace Translatable
{
    public interface ITranslationFactory
    {
        /// <summary>
        /// Creates a translated instance of the type T using reflection and the given ITranslationSource
        /// </summary>
        /// <typeparam name="T">The type that will be analyzed and translated</typeparam>
        /// <returns>A translated instance of T</returns>
        T CreateTranslation<T>() where T: ITranslatable;
    }

    /// <summary>
    /// The TranslationFactory class uses reflection to translate instances of
    /// ITranslatable into the target language.
    /// </summary>
    public class TranslationFactory : ITranslationFactory
    {
        public ITranslationSource TranslationSource { get; set; }

        /// <summary>
        /// Create a new TranslationFactory with the given ITranslationSource
        /// </summary>
        /// <param name="translationSource">The source that will be used to look up all translations</param>
        public TranslationFactory(ITranslationSource translationSource = null)
        {
            TranslationSource = translationSource;
        }

        /// <summary>
        /// Creates a translated instance of the type T using reflection and the given ITranslationSource
        /// </summary>
        /// <typeparam name="T">The type that will be analyzed and translated</typeparam>
        /// <returns>A translated instance of T</returns>
        public T CreateTranslation<T>() where T: ITranslatable
        {
            var instance = Activator.CreateInstance<T>();

            if (TranslationSource != null)
                Translate(instance, TranslationSource);

            return instance;
        }

        private void Translate(ITranslatable o, ITranslationSource translationSource)
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
            property.SetValue(translatable, pluralBuilder, null);
        }

        private void SetStringProperty(ITranslatable o, PropertyInfo property, ITranslationSource translationSource)
        {
            var value = (string)property.GetValue(o, null);

            var translated = translationSource.GetTranslation(value);

            if (!string.IsNullOrEmpty(translated))
                property.SetValue(o, translated, null);
        }

        private void SetStringArrayProperty(ITranslatable o, PropertyInfo property, IPluralBuilder pluralBuilder, ITranslationSource translationSource)
        {
            var value = (string[])property.GetValue(o, null);

            if (value.Length != 2)
                throw new InvalidDataException($"The plural string for key {property.Name} must contain two strings: a singular and a plural form. It contained {value.Length} strings.");

            var translations = translationSource.GetAllTranslations(value[0], pluralBuilder);

            if (translations.Length == pluralBuilder.NumberOfPlurals)
                property.SetValue(o, translations, null);
        }
    }
}
