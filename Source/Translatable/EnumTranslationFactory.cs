using System;
using System.Collections.Generic;
using System.Linq;

namespace Translatable
{
    internal class EnumTranslationFactory
    {
        internal static EnumTranslation<T>[] CreateEnumTranslation<T>(ITranslationSource translationSource) where T : struct, IConvertible
        {
            var type = typeof(T);

            if (!type.IsEnum)
                throw new InvalidOperationException($"The type {type.Name} has to be an enum.");

            if (!type.IsDefined(typeof(TranslatableAttribute), false))
                throw new InvalidOperationException($"The type {type.Name} is no translatable enum! Add the Attribute Translatable to the enum declaration.");

            var values = new List<EnumTranslation<T>>();

            foreach (var value in Enum.GetValues(type).Cast<T>())
            {
                try
                {
                    var context = ContextAttribute.GetValue(value);
                    var msgid = TranslationAttribute.GetValue(value);
                    var translation = GetTranslation(msgid, context, translationSource);
                    values.Add(new EnumTranslation<T>(translation, value));
                }
                catch (ArgumentException)
                {
                    throw new InvalidOperationException($"The value {value} in enum {type.Name} does not have the [Translation] attribute. This is required to make it translatable.");
                }
            }

            return values.ToArray();
        }

        internal static EnumTranslation<T>[] UpdateEnumTranslation<T>(ITranslationSource translationSource, EnumTranslation<T>[] translations) where T : struct, IConvertible
        {
            var type = typeof(T);

            if (!type.IsEnum)
                throw new InvalidOperationException($"The type {type.Name} has to be an enum.");

            if (!type.IsDefined(typeof(TranslatableAttribute), false))
                throw new InvalidOperationException($"The type {type.Name} is no translatable enum! Add the Attribute Translatable to the enum declaration.");

            foreach (var value in translations)
            {
                try
                {
                    var context = ContextAttribute.GetValue(value.Value);
                    var msgid = TranslationAttribute.GetValue(value.Value);
                    var translation = GetTranslation(msgid, context, translationSource);
                    value.Translation = translation;
                }
                catch (ArgumentException)
                {
                    throw new InvalidOperationException($"The value {value} in enum {type.Name} does not have the [Translation] attribute. This is required to make it translatable.");
                }
            }

            return translations;
        }

        private static string GetTranslation(string msgId, string context, ITranslationSource translationSource = null)
        {
            if (translationSource == null)
                return msgId;
            return translationSource.GetTranslation(msgId, context);
        }
    }
}
