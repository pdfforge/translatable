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
        ///     Updates a translated instance of the type T using reflection and the given ITranslationSource.
        ///     If translation is null, a new instance will be created
        /// </summary>
        /// <typeparam name="T">The type that will be analyzed and translated</typeparam>
        /// <param name="translation">The translation object to update. If translation is null, a new instance will be created</param>
        /// <returns>A translated instance of T</returns>
        T UpdateOrCreateTranslation<T>(T translation) where T : ITranslatable, new();

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
        ///     Creates a list of translations for a given enum type. This can be used to display translated enum values to the
        ///     user. If translations is null or empty, a new instance will be created
        /// </summary>
        /// <typeparam name="T">The type that will be analyzed and translated</typeparam>
        /// <param name="translations">The translations array that will be updated</param>
        /// <returns>A list of translations of the enum T</returns>
        EnumTranslation<T>[] UpdateOrCreateEnumTranslation<T>(EnumTranslation<T>[] translations) where T : struct, IConvertible;

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
                Translate(TranslationSource, instance);

            return instance;
        }

        /// <summary>
        ///     Creates a translated instance of the type t using reflection and the given ITranslationSource
        /// </summary>
        /// <param name="t">The type to instantiate. It has to be derived from ITranslatable and must be </param>
        /// <returns></returns>
        public ITranslatable CreateTranslation(Type t)
        {
            var instance = (ITranslatable)Activator.CreateInstance(t);

            if (TranslationSource != null)
                Translate(TranslationSource, instance);

            return instance;
        }

        /// <summary>
        ///     Updates a translated instance of the type T using reflection and the given ITranslationSource.
        ///     If translation is null, a new instance will be created
        /// </summary>
        /// <typeparam name="T">The type that will be analyzed and translated</typeparam>
        /// <param name="translation">The translation object to update. If translation is null, a new instance will be created</param>
        /// <returns>A translated instance of T</returns>
        public T UpdateOrCreateTranslation<T>(T translation) where T : ITranslatable, new()
        {
            if (translation == null)
                return CreateTranslation<T>();

            var baseTranslation = (ITranslatable)Activator.CreateInstance<T>();

            if (TranslationSource != null)
                Translate(TranslationSource, translation, baseTranslation);

            return translation;
        }

        public EnumTranslation<T>[] CreateEnumTranslation<T>() where T : struct, IConvertible
        {
            return EnumTranslationFactory.CreateEnumTranslation<T>(TranslationSource);
        }

        public EnumTranslation<T>[] UpdateOrCreateEnumTranslation<T>(EnumTranslation<T>[] translation) where T : struct, IConvertible
        {
            if (translation == null || translation.Length == 0)
                return EnumTranslationFactory.CreateEnumTranslation<T>(TranslationSource);

            return EnumTranslationFactory.UpdateEnumTranslation(TranslationSource, translation);
        }

        private void Translate(ITranslationSource translationSource, ITranslatable targetTranslation, ITranslatable baseTranslation = null)
        {
            if (baseTranslation == null)
                baseTranslation = targetTranslation;

            var type = baseTranslation.GetType();

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            var pluralBuilder = translationSource.GetPluralBuilder();

            foreach (var property in properties)
            {
                if (!property.CanRead || !property.CanWrite)
                    continue;

                if (property.PropertyType == typeof(string))
                    SetStringProperty(property, translationSource, targetTranslation, baseTranslation);

                if (property.PropertyType == typeof(string[]))
                    SetStringArrayProperty(property, pluralBuilder, translationSource, targetTranslation, baseTranslation);

                if (property.PropertyType.IsArray &&
                    property.PropertyType.GetElementType().Name == typeof(EnumTranslation<>).Name)
                    SetEnumArrayProperty(property, targetTranslation);

                if (property.PropertyType == typeof(IPluralBuilder))
                    SetPluralbuilderProperty(property, pluralBuilder, targetTranslation);
            }
        }

        private void SetPluralbuilderProperty(PropertyInfo property, IPluralBuilder pluralBuilder, ITranslatable translatable)
        {
            property.SetValue(translatable, pluralBuilder, null);
        }

        private void SetStringProperty(PropertyInfo property, ITranslationSource translationSource, ITranslatable targetTranslation, ITranslatable baseTranslation)
        {
            var value = (string)property.GetValue(baseTranslation, null);
            var context = ContextAttribute.GetValue(property);

            var translated = translationSource.GetTranslation(value, context);

            if (!string.IsNullOrEmpty(translated))
                property.SetValue(targetTranslation, translated, null);
        }

        private void SetStringArrayProperty(PropertyInfo property, IPluralBuilder pluralBuilder,
            ITranslationSource translationSource, ITranslatable targetTranslation, ITranslatable baseTranslation)
        {
            var context = ContextAttribute.GetValue(property);
            var sourceStrings = (string[])property.GetValue(baseTranslation, null);

            if (sourceStrings.Length != 2)
                throw new InvalidDataException(
                    $"The plural string for key {property.Name} must contain two strings: a singular and a plural form. It contained {sourceStrings.Length} strings.");

            var translations = translationSource.GetAllTranslations(sourceStrings[0], context, pluralBuilder);

            if (translations.Length == pluralBuilder.NumberOfPlurals)
                property.SetValue(targetTranslation, translations, null);
            else
                property.SetValue(targetTranslation, sourceStrings, null);
        }

        private void SetEnumArrayProperty(PropertyInfo property, ITranslatable targetTranslation)
        {
            var currentTranslation = property.GetValue(targetTranslation, null);
            var propertyType = property.PropertyType.GetElementType();
            var genericDefinition = propertyType.GetGenericTypeDefinition();

            if (genericDefinition != typeof(EnumTranslation<>))
                throw new InvalidDataException($"The property {property.Name} is not of the type EnumTranslation<>!");

            // Type of the enum we would like to translate
            var enumType = propertyType.GetGenericArguments()[0];

            // Use Reflection to find the CreateEnumTranslation method from this class and bind the generic parameter
            var method = this.GetType().GetMethod(nameof(UpdateOrCreateEnumTranslation));
            var genericMethod = method.MakeGenericMethod(enumType);

            // Call CreateEnumTranslation to retrieve translated enum values
            var translations = genericMethod.Invoke(this, new object[] { currentTranslation });

            property.SetValue(targetTranslation, translations, null);
        }

        public event EventHandler TranslationChanged;
    }
}
