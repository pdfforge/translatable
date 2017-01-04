namespace Translatable
{
    public class EnumTranslation<T>
    {
        public EnumTranslation(string translation, T value)
        {
            Translation = translation;
            Value = value;
        }

        public T Value { get; set; }
        public string Translation { get; set; }
    }
}
