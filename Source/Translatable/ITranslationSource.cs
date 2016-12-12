namespace Translatable
{
    /// <summary>
    /// ITranslationSource is the adapter to all supported translation systems and extension point
    /// for further development
    /// </summary>
    public interface ITranslationSource
    {
        IPluralBuilder GetPluralBuilder();
        string GetTranslation(string translationKey);
        string[] GetAllTranslations(string translationKey, IPluralBuilder pluralBuilder);
    }
}