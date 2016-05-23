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
        public void DoExport(string outputDirectory)
        {
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            //TODO Read from command line parameter
            var binFolder = Path.GetFullPath(".");

            var assemblies = Directory.EnumerateFiles(binFolder, "*.dll")
                .Union(Directory.EnumerateFiles(binFolder, "*.exe"));

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
            StringBuilder sb = new StringBuilder();

            var properties = translatable.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            var obj = Activator.CreateInstance(translatable);

            foreach (var property in properties)
            {
                var comment = GetTranslatorcomment(property);
                if (!string.IsNullOrEmpty(comment))
                    sb.AppendLine("#. " + EscpaeString(comment));

                var escapedMessage = EscpaeString(property.GetValue(obj).ToString());
                sb.AppendLine($"msgid \"{escapedMessage}\"");
                sb.AppendLine("msgstr \"\"");
                sb.AppendLine();
            }

            var file = Path.Combine(outputDirectory, translatable.FullName + ".pot");
            File.WriteAllText(file, sb.ToString());
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
