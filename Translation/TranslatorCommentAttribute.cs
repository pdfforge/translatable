using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Translation
{
    public class TranslatorCommentAttribute : Attribute
    {
        public TranslatorCommentAttribute(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static string GetValue(object value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            var attributes = fi.GetCustomAttributes(typeof(TranslatorCommentAttribute), false) as TranslatorCommentAttribute[];

            if (attributes != null && attributes.Length > 0)
                return attributes[0].Value;

            throw new ArgumentException(nameof(value));
        }
    }
}
