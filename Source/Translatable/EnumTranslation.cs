using System;

namespace Translatable
{
    public class EnumTranslation<T>
         where T : struct, IConvertible
    {
        public EnumTranslation(string translation, T value)
        {
            Translation = translation;
            Value = value;
        }

        public T Value { get; set; }
        public string Translation { get; set; }

        public static EnumTranslation<T>[] CreateDefaultEnumTranslation()
        {
            return EnumTranslationFactory.CreateEnumTranslation<T>(null);
        }
    }
}
