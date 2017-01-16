using System;
using System.IO;
using System.Reflection;

// ReSharper disable ArrangeThisQualifier

namespace Translatable
{
    public interface ITranslationFactory
    {
        /// <summary>
        ///     Creates a translated instance of the type T using reflection and the given ITranslationSource
        /// </summary>
        /// <typeparam name="T">The type that will be analyzed and translated</typeparam>
        /// <returns>A translated instance of T</returns>
        T CreateTranslation<T>() where T : ITranslatable, new();

        /// <summary>
        ///     Creates a translated instance of the type t using reflection and the given ITranslationSource
        /// </summary>
        /// <param name="t">The type to instantiate. It has to be derived from ITranslatable and must be </param>
        /// <returns></returns>
        ITranslatable CreateTranslation(Type t);

        /// <summary>
        ///     Creates a list of translations for a given enum type. This can be used to display translated enum values to the
        ///     user.
        /// </summary>
        /// <typeparam name="T">The type that will be analyzed and translated</typeparam>
        /// <returns>A list of translations of the enum T</returns>
        EnumTranslation<T>[] CreateEnumTranslation<T>() where T : struct, IConvertible;

        /// <summary>
        /// The event is raised when the translation has changed. Long-living classes should update their translations
        /// when the event is received.
        /// </summary>
        event EventHandler TranslationChanged;
    }

    /// <summary>
    ///     The TranslationFactory class uses reflection to translate instances of
    ///     ITranslatable into the target language.
    /// </summary>
    public class TranslationFactory : ITranslationFactory
    {
        private ITranslationSource _translationSource;

        /// <summary>
        ///     Create a new TranslationFactory with the given ITranslationSource
        /// </summary>
        /// <param name="translationSource">The source that will be used to look up all translations</param>
        public TranslationFactory(ITranslationSource translationSource = null)
        {
            TranslationSource = translationSource;
        }

        public ITranslationSource TranslationSource
        {
            get { return _translationSource; }
            set { _translationSource = value; TranslationChanged?.Invoke(this, EventArgs.Empty); }
        }

        /// <summary>
        ///     Creates a translated instance of the type T using reflection and the given ITranslationSource
        /// </summary>
        /// <typeparam name="T">The type that will be analyzed and translated</typeparam>
        /// <returns>A translated instance of T</returns>
        public T CreateTranslation<T>() where T : ITranslatable, new()
        {
            var instance = Activator.CreateInstance<T>();

            if (TranslationSource != null)
                Translate(instance, TranslationSource);

            return instance;
        }

        /// <summary>
        ///     Creates a translated instance of the type t using reflection and the given ITranslationSource
        /// </summary>
        /// <param name="t">The type to instantiate. It has to be derived from ITranslatable and must be </param>
        /// <returns></returns>
        public ITranslatable CreateTranslation(Type t)
        {
            var instance = (ITranslatable) Activator.CreateInstance(t);

            if (TranslationSource != null)
                Translate(instance, TranslationSource);

            return instance;
        }

        public EnumTranslation<T>[] CreateEnumTranslation<T>() where T : struct, IConvertible
        {
            return EnumTranslationFactory.CreateEnumTranslation<T>(TranslationSource);
        }

        private void Translate(ITranslatable o, ITranslationSource translationSource)
        {
            var type = o.GetType();

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            var pluralBuilder = translationSource.GetPluralBuilder();

            foreach (var property in properties)
            {
                if (!property.CanRead || !property.CanWrite)
                    continue;

                if (property.PropertyType == typeof(string))
                    SetStringProperty(o, property, translationSource);

                if (property.PropertyType == typeof(string[]))
                    SetStringArrayProperty(o, property, pluralBuilder, translationSource);

                if (property.PropertyType.IsArray &&
                    property.PropertyType.GetElementType().Name == typeof(EnumTranslation<>).Name)
                    SetEnumArrayProperty(o, property);

                if (property.PropertyType == typeof(IPluralBuilder))
                    SetPluralbuilderProperty(o, property, pluralBuilder);
            }
        }

        private void SetPluralbuilderProperty(ITranslatable translatable, PropertyInfo property,
            IPluralBuilder pluralBuilder)
        {
            property.SetValue(translatable, pluralBuilder, null);
        }

        private void SetStringProperty(ITranslatable o, PropertyInfo property, ITranslationSource translationSource)
        {
            var value = (string) property.GetValue(o, null);

            var translated = translationSource.GetTranslation(value);

            if (!string.IsNullOrEmpty(translated))
                property.SetValue(o, translated, null);
        }

        private void SetStringArrayProperty(ITranslatable o, PropertyInfo property, IPluralBuilder pluralBuilder,
            ITranslationSource translationSource)
        {
            var value = (string[]) property.GetValue(o, null);

            if (value.Length != 2)
                throw new InvalidDataException(
                    $"The plural string for key {property.Name} must contain two strings: a singular and a plural form. It contained {value.Length} strings.");

            var translations = translationSource.GetAllTranslations(value[0], pluralBuilder);

            if (translations.Length == pluralBuilder.NumberOfPlurals)
                property.SetValue(o, translations, null);
        }

        private void SetEnumArrayProperty(ITranslatable o, PropertyInfo property)
        {
            var propertyType = property.PropertyType.GetElementType();
            var genericDefinition = propertyType.GetGenericTypeDefinition();

            if (genericDefinition != typeof(EnumTranslation<>))
                throw new InvalidDataException($"The property {property.Name} is not of the type EnumTranslation<>!");

            // Type of the enum we would like to translate
            var enumType = propertyType.GetGenericArguments()[0];

            // Use Reflection to find the CreateEnumTranslation method from this class and bind the generic parameter
            var method = this.GetType().GetMethod(nameof(CreateEnumTranslation));
            var genericMethod = method.MakeGenericMethod(enumType);

            // Call CreateEnumTranslation to retrieve translated enum values
            var translations = genericMethod.Invoke(this, new object[] {});

            property.SetValue(o, translations, null);
        }

        public event EventHandler TranslationChanged;
    }
}
