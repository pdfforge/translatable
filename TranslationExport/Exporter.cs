using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Translation;
using TranslationTest;

namespace TranslationExport
{
    class Exporter
    {
        // TODO Hack to actually load assembly
        private MainWindowTranslation x = new MainWindowTranslation();

        public void DoExport(string outputDirectory)
        {
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

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
                var comment = GetTranslatorcomment(property, obj);
                if (!string.IsNullOrEmpty(comment))
                    sb.AppendLine("//" + comment);

                sb.AppendLine($"{property.GetValue(obj)}");
            }

            var file = Path.Combine(outputDirectory, translatable.FullName + ".pot");
            File.WriteAllText(file, sb.ToString());
        }

        private string GetTranslatorcomment(PropertyInfo pi, object o)
        {
            var attributes = pi.GetCustomAttributes(typeof(TranslatorCommentAttribute), false) as TranslatorCommentAttribute[];

            if (attributes != null && attributes.Length > 0)
                return attributes[0].Value;

            return null;
        }
    }
}
