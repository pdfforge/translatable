using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        T CreateTranslation<T>() where T: ITranslatable, new();

        /// <summary>
        /// Creates a translated instance of the type t using reflection and the given ITranslationSource
        /// </summary>
        /// <param name="t">The type to instantiate. It has to be derived from ITranslatable and must be </param>
        /// <returns></returns>
        ITranslatable CreateTranslation(Type t);

        /// <summary>
        /// Creates a list of translations for a given enum type. This can be used to display translated enum values to the user.
        /// </summary>
        /// <typeparam name="T">The type that will be analyzed and translated</typeparam>
        /// <returns>A list of translations of the enum T</returns>
        IList<EnumTranslation<T>> CreateEnumTranslation<T>() where T : struct, IConvertible;
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
        public T CreateTranslation<T>() where T: ITranslatable, new()
        {
            var instance = Activator.CreateInstance<T>();

            if (TranslationSource != null)
                Translate(instance, TranslationSource);

            return instance;
        }

        /// <summary>
        /// Creates a translated instance of the type t using reflection and the given ITranslationSource
        /// </summary>
        /// <param name="t">The type to instantiate. It has to be derived from ITranslatable and must be </param>
        /// <returns></returns>
        public ITranslatable CreateTranslation(Type t)
        {
            var instance = (ITranslatable)Activator.CreateInstance(t);

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
                    SetStringProperty(o, property, translationSource);

                if (property.PropertyType == typeof(string[]))
                    SetStringArrayProperty(o, property, pluralBuilder, translationSource);

                if (property.PropertyType == typeof(IPluralBuilder))
                    SetPluralbuilderProperty(o, property, pluralBuilder);
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

        public IList<EnumTranslation<T>> CreateEnumTranslation<T>() where T : struct, IConvertible
        {
            var type = typeof(T);

            if (!type.IsEnum)
                throw new InvalidOperationException($"The type {type.Name} has to be an enum.");

            if (!type.IsDefined(typeof(TranslatableAttribute), false))
                throw new InvalidOperationException($"The type {type.Name} is no translatable enum! Add the Attribute Translatable to the enum declaration.");

            var values = new List<EnumTranslation<T>>();

            foreach (var value in Enum.GetValues(type).Cast<T>())
            {
                try
                {
                    var msgid = TranslationAttribute.GetValue(value);
                    var translation = GetTranslation(msgid);
                    values.Add(new EnumTranslation<T>(translation, value));
                }
                catch (ArgumentException)
                {
                    throw new InvalidOperationException($"The value {value} in enum {type.Name} does not have the [Translation] attribute. This is required to make it translatable.");
                }
            }

            return values;
        }

        private string GetTranslation(string msgId)
        {
            if (TranslationSource == null)
                return msgId;
            return TranslationSource.GetTranslation(msgId);
            ;
        }
    }
}
