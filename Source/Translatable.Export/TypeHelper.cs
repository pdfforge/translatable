using System;
using System.Linq;
using System.Reflection;

namespace Translatable.Export
{
    /// <summary>
    /// Helper class to provide methods to extract and check types and attributes via reflection.
    /// It uses names for comparisons to allow macthing the same type from several different strong-named assemblies.
    /// This is required to extract strings from assemblies compiled with a different version of Translatable than this export project.
    /// </summary>
    static class TypeHelper
    {
        public static bool IsTranslatable(Type t)
        {
            // Use name instead of type to be able to export assemblies compiled with other versions of Translatable
            return !t.IsAbstract
                && ImplementsInterface(t, typeof(ITranslatable));
        }

        public static bool HasTranslatableAttribute(Type t)
        {
            // Use name instead of type to be able to export assemblies compiled with other versions of Translatable
            return t.IsEnum
                && t.GetCustomAttributes(false).Any(a => a.GetType().FullName == typeof(TranslatableAttribute).FullName);
        }

        public static bool ImplementsInterface(Type typeToInspect, Type desiredInterface)
        {
            return typeToInspect.GetInterfaces().Any(i => i.FullName == desiredInterface.FullName);
        }

        public static string GetTranslationAttributeValue(object o)
        {
            var type = o.GetType();
            var memInfo = type.GetMember(o.ToString());

            if (memInfo.Length == 0)
                return "";

            var attribute =
                memInfo[0].GetCustomAttributes(false)
                    .FirstOrDefault(attr => IsType(attr.GetType(), typeof(TranslationAttribute)));

            if (attribute == null)
                return "";

            try
            {
                var propInfo = attribute.GetType().GetField("_value", BindingFlags.NonPublic | BindingFlags.Instance);
                var value = propInfo?.GetValue(attribute) ?? "";
                return (string) value;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static string GetTranslatorCommentAttributeValue(PropertyInfo propertyInfo)
        {
            var attribute =
                propertyInfo.GetCustomAttributes(false)
                    .FirstOrDefault(attr => IsType(attr.GetType(), typeof(TranslatorCommentAttribute)));

            if (attribute == null)
                return "";

            try
            {
                var propInfo = attribute.GetType().GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
                var value = propInfo.GetValue(attribute, new object[0]);
                return (string)value;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static string GetEnumTranslatorCommentAttributeValue(object o)
        {
            var type = o.GetType();
            var memInfo = type.GetMember(o.ToString());

            if (memInfo.Length == 0)
                return "";

            var attribute =
                memInfo[0].GetCustomAttributes(false)
                    .FirstOrDefault(attr => IsType(attr.GetType(), typeof(TranslatorCommentAttribute)));

            if (attribute == null)
                return "";

            try
            {
                var propInfo = attribute.GetType().GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
                var value = propInfo.GetValue(attribute, new object[0]);
                return (string)value;
            }
            catch (Exception)
            {
                return "";
            }
        }

        /// <summary>
        /// Compare two types to match their full names
        /// </summary>
        /// <param name="desiredType"></param>
        /// <param name="typeToInspect"></param>
        /// <returns></returns>
        public static bool IsType(Type typeToInspect, Type desiredType)
        {
            return desiredType.FullName == typeToInspect.FullName;
        }
    }
}
