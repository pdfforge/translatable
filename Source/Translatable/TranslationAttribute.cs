using System;

namespace Translatable
{
    [AttributeUsage(AttributeTargets.Field)]
    public class TranslationAttribute : Attribute
    {
        private readonly string _value;

        public TranslationAttribute(string value)
        {
            _value = value;
        }

        public static string GetValue(object value)
        {
            var fi = value.GetType().GetField(value.ToString());

            var attributes = fi.GetCustomAttributes(typeof(TranslationAttribute), false) as TranslationAttribute[];

            if (attributes != null && attributes.Length > 0)
                return attributes[0]._value;

            throw new ArgumentException(nameof(value));
        }
    }
}