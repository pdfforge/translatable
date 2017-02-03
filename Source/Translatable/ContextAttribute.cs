using System;
using System.Linq;
using System.Reflection;

namespace Translatable
{
    /// <summary>
    /// Allows to add a translation context that allows to diffirentiate between identical msgids for different purposes
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ContextAttribute : Attribute
    {
        public ContextAttribute(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static string GetValue(object value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            var attributes = fi?.GetCustomAttributes(typeof(ContextAttribute), false) as ContextAttribute[];

            if ((attributes != null) && (attributes.Length > 0))
                return attributes[0].Value;

            return "";
        }

        public static string GetValue(PropertyInfo propertyInfo)
        {
            var attributes = propertyInfo.GetCustomAttributes(typeof(ContextAttribute), false) as ContextAttribute[];

            if ((attributes != null) && (attributes.Length > 0))
                return attributes[0].Value;

            return "";
        }
    }
}
