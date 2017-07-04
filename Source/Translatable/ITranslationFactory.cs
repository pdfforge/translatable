using System;

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
}