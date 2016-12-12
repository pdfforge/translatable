namespace Translatable
{
    public interface ITranslationSource
    {
        IPluralBuilder GetPluralBuilder();
        string GetTranslation(string translationKey);
        string[] GetAllTranslations(string translationKey, IPluralBuilder pluralBuilder);
    }
}