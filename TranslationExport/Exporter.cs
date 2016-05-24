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
                var comment = GetTranslatorcomment(property);
                if (!string.IsNullOrEmpty(comment))
                {
                    potBuilder.AppendLine("#. " + EscpaeString(comment));
                    poBuilder.AppendLine("#. " + EscpaeString(comment));
                }

                var escapedMessage = EscpaeString(property.GetValue(obj).ToString());

                potBuilder.AppendLine($"msgid \"{escapedMessage}\"");
                potBuilder.AppendLine("msgstr \"\"");
                potBuilder.AppendLine();

                poBuilder.AppendLine($"msgid \"{escapedMessage}\"");
                poBuilder.AppendLine($"msgstr \"{escapedMessage}\"");
                poBuilder.AppendLine();
            }

            var potFile = Path.Combine(outputDirectory, translatable.FullName + ".pot");
            File.WriteAllText(potFile, potBuilder.ToString());

            var poFile = Path.Combine(outputDirectory, translatable.FullName + ".po");
            File.WriteAllText(poFile, poBuilder.ToString());
        }

        private string EscpaeString(string str)
        {
            return str.Replace("\r", "\\r").Replace("\n", "\\n");
        }

        private string GetTranslatorcomment(PropertyInfo pi)
        {
            var attributes = pi.GetCustomAttributes(typeof(TranslatorCommentAttribute), false) as TranslatorCommentAttribute[];

            if (attributes != null && attributes.Length > 0)
                return attributes[0].Value;

            return null;
        }
    }
}
