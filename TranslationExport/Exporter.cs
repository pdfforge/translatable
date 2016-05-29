using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Translation;

namespace TranslationExport
{
    class Exporter
    {
        public void DoExport(string assemblyDirectory, string outputDirectory)
        {
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            assemblyDirectory = Path.GetFullPath(assemblyDirectory);

            var assemblies = Directory.EnumerateFiles(assemblyDirectory, "*.dll")
                .Union(Directory.EnumerateFiles(assemblyDirectory, "*.exe"));

            foreach (var assembly in assemblies)
            {
                Assembly.LoadFile(Path.GetFullPath(assembly));
            }
            

            var translatableType = typeof(ITranslatable);
            var translatables = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(t => translatableType.IsAssignableFrom(t) && !t.IsAbstract).ToList();

            foreach (var translatable in translatables)
            {
                Export(outputDirectory, translatable);
            }
        }

        private void Export(string outputDirectory, Type translatable)
        {
            StringBuilder potBuilder = new StringBuilder();
            StringBuilder poBuilder = new StringBuilder();

            var properties = translatable.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            var obj = Activator.CreateInstance(translatable);

            foreach (var property in properties)
            {
                if (property.PropertyType.IsAssignableFrom(typeof(IPluralBuilder)))
                    continue;

                var comment = GetTranslatorComment(property);
                if (!string.IsNullOrEmpty(comment))
                {
                    potBuilder.AppendLine("#. " + EscapeString(comment));
                    poBuilder.AppendLine("#. " + EscapeString(comment));
                }

                if (property.PropertyType == typeof(string))
                {
                    var escapedMessage = EscapeString(property.GetValue(obj).ToString());

                    potBuilder.AppendLine($"msgid \"{escapedMessage}\"");
                    potBuilder.AppendLine("msgstr \"\"");
                    potBuilder.AppendLine();

                    poBuilder.AppendLine($"msgid \"{escapedMessage}\"");
                    poBuilder.AppendLine($"msgstr \"{escapedMessage}\"");
                    poBuilder.AppendLine();

                    continue;
                }

                if (property.PropertyType == typeof(string[]))
                {
                    var value = (string[]) property.GetValue(obj);

                    if (value.Length != 2)
                        throw new InvalidDataException($"The plural string for {property.Name} must contain two strings: a singular and a plural form. It contained {value.Length} strings.");

                    var escapedSingular = EscapeString(value[0]);
                    var escapedPlural = EscapeString(value[1]);

                    potBuilder.AppendLine($"msgid \"{escapedSingular}\"");
                    potBuilder.AppendLine($"msgid_plural \"{escapedPlural}\"");
                    potBuilder.AppendLine("msgstr[0] \"\"");
                    potBuilder.AppendLine("msgstr[1] \"\"");
                    potBuilder.AppendLine();

                    poBuilder.AppendLine($"msgid \"{escapedSingular}\"");
                    poBuilder.AppendLine($"msgid_plural \"{escapedPlural}\"");
                    poBuilder.AppendLine($"msgstr[0] \"{value[0]}\"");
                    poBuilder.AppendLine($"msgstr[1] \"{value[1]}\"");
                    poBuilder.AppendLine();
                    continue;
                }

                throw new InvalidOperationException($"The type {property.PropertyType} is not supported in ITranslatables.");
            }

            var potFile = Path.Combine(outputDirectory, translatable.FullName + ".pot");
            File.WriteAllText(potFile, potBuilder.ToString());

            var poFile = Path.Combine(outputDirectory, translatable.FullName + ".po");
            File.WriteAllText(poFile, poBuilder.ToString());
        }

        private string EscapeString(string str)
        {
            return str.Replace("\r", "\\r").Replace("\n", "\\n");
        }

        private string GetTranslatorComment(PropertyInfo pi)
        {
            var attributes = pi.GetCustomAttributes(typeof(TranslatorCommentAttribute), false) as TranslatorCommentAttribute[];

            if (attributes != null && attributes.Length > 0)
                return attributes[0].Value;

            return null;
        }
    }
}
