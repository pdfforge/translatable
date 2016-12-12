using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Translatable.Export.Po;

namespace Translatable.Export
{
    class Exporter
    {
        public void DoExport(ExportOptions exportOptions)
        {
            var outputDirectory = Path.GetFullPath(exportOptions.OutputPath);

            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            var assemblies = LoadAssemblies(exportOptions.Assemblies);

            var translatableType = typeof(ITranslatable);
            var translatables = assemblies
                .SelectMany(s => s.GetTypes())
                .Where(t => translatableType.IsAssignableFrom(t) && !t.IsAbstract).ToList();

            var catalog = new Catalog();

            foreach (var translatable in translatables)
                Export(translatable, catalog);

            var writer = new PotWriter();

            var potFile = Path.Combine(outputDirectory, "messages.pot");
            writer.WritePotFile(potFile, catalog);
        }

        private IEnumerable<Assembly> LoadAssemblies(IEnumerable<string> assemblyFiles)
        {
            var assemblies = new List<Assembly>();
            foreach (var assemblyFile in assemblyFiles)
            {
                try
                {
                    assemblies.Add(Assembly.LoadFile(Path.GetFullPath(assemblyFile)));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not load assembly file '{assemblyFile}': {ex}");
                    throw;
                }
            }

            return assemblies;
        }

        private void Export(Type translatable, Catalog catalog)
        {
            var properties = translatable.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            var obj = Activator.CreateInstance(translatable);

            foreach (var property in properties)
            {
                if (property.PropertyType.IsAssignableFrom(typeof(IPluralBuilder)))
                    continue;

                var comment = GetTranslatorComment(property);

                if (property.PropertyType == typeof(string))
                {
                    var escapedMessage = EscapeString(property.GetValue(obj, null).ToString());

                    catalog.AddEntry(escapedMessage, comment, translatable.FullName);

                    continue;
                }

                if (property.PropertyType == typeof(string[]))
                {
                    var value = (string[]) property.GetValue(obj, null);

                    if (value.Length != 2)
                        throw new InvalidDataException($"The plural string for {property.Name} must contain two strings: a singular and a plural form. It contained {value.Length} strings.");

                    var escapedSingular = EscapeString(value[0]);
                    var escapedPlural = EscapeString(value[1]);

                    catalog.AddPluralEntry(escapedSingular, escapedPlural, comment, translatable.FullName);

                    continue;
                }

                throw new InvalidOperationException($"The type {property.PropertyType} is not supported in ITranslatables.");
            }
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

            return "";
        }
    }
}
