using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Translatable.Export.Po;

namespace Translatable.Export
{
    public enum ResultCode
    {
        Success,
        NoTranslatablesFound,
        NoTranslationsFound
    }

    class Exporter
    {
        private IList<Assembly> _assemblies;

        public ResultCode DoExport(ExportOptions exportOptions)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;

            try
            {
                var potFile = Path.GetFullPath(exportOptions.OutputFile);
                var outputDirectory = Path.GetDirectoryName(potFile);

                if (!Directory.Exists(outputDirectory))
                    Directory.CreateDirectory(outputDirectory);

                _assemblies = LoadAssemblies(exportOptions.Assemblies).ToList();

                var translatableType = typeof(ITranslatable);
                var translatables = _assemblies
                    .SelectMany(s => s.GetTypes())
                    .Where(t => translatableType.IsAssignableFrom(t) && !t.IsAbstract).ToList();

                var enums = _assemblies
                    .SelectMany(s => s.GetTypes())
                    .Where(t => t.IsEnum && t.IsDefined(typeof(TranslatableAttribute), false))
                    .ToList();

                if (!translatables.Any() && !enums.Any())
                    return ResultCode.NoTranslationsFound;

                var catalog = new Catalog();

                foreach (var translatable in translatables)
                    ExportClass(translatable, catalog);

                foreach (var type in enums)
                {
                    ExportEnum(type, catalog);
                }

                if (!catalog.Entries.Any())
                    return ResultCode.NoTranslationsFound;

                var writer = new PotWriter();

                writer.WritePotFile(potFile, catalog);

            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomainOnAssemblyResolve;
            }

            return ResultCode.Success;
        }

        private Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var domain = (AppDomain)sender;

            foreach (var assembly in domain.GetAssemblies())
            {
                if (assembly.FullName == args.Name)
                {
                    return assembly;
                }
            }

            var first = _assemblies.First();

            var assemblyName = args.Name.Split(new[] {','}, StringSplitOptions.None)[0];
            var uri = new UriBuilder(first.CodeBase);
            var assemblyFolder = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));
            var assemblyPath = Path.Combine(assemblyFolder, assemblyName + ".dll");

            if (File.Exists(assemblyPath))
                return Assembly.LoadFile(assemblyPath);

            return null;
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

        private void ExportClass(Type translatable, Catalog catalog)
        {
            var properties = translatable.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            var obj = Activator.CreateInstance(translatable);

            foreach (var property in properties)
            {
                if (property.PropertyType.IsAssignableFrom(typeof(IPluralBuilder)))
                    continue;

                if (!property.CanRead)
                    throw new InvalidOperationException($"The property {property.Name} in type {translatable.FullName} is not readable!");

                if (!property.CanWrite)
                    throw new InvalidOperationException($"The property {property.Name} in type {translatable.FullName} is not writable!");

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

                throw new InvalidOperationException($"The type {property.PropertyType} in {translatable.FullName}.{property.Name} is not supported in ITranslatables.");
            }
        }

        private void ExportEnum(Type type, Catalog catalog)
        {
            if (!type.IsDefined(typeof(TranslatableAttribute), false))
                throw new InvalidOperationException("The type is no translatable enum! Add the Attribute Translatable to the enum declaration.");

            foreach (var value in Enum.GetValues(type))
            {
                try
                {
                    var msgid = TranslationAttribute.GetValue(value);
                    var escapedMessage = EscapeString(msgid);
                    var comment = GetTranslatorComment(value);

                    catalog.AddEntry(escapedMessage, comment, type.FullName);
                }
                catch (ArgumentException)
                {
                    throw new InvalidOperationException($"The value {value} in enum {type.Name} does not have the [Translation] attribute. This is required to make it translatable.");
                }
            }
        }

        private string EscapeString(string str)
        {
            return str
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r\n", "\n")
                .Replace("\n", "\\n");
        }

        private string GetTranslatorComment(PropertyInfo pi)
        {
            var attributes = pi.GetCustomAttributes(typeof(TranslatorCommentAttribute), false) as TranslatorCommentAttribute[];

            if (attributes != null && attributes.Length > 0)
                return attributes[0].Value;

            return "";
        }

        private string GetTranslatorComment(object value)
        {
            try
            {
                return TranslatorCommentAttribute.GetValue(value);
            }
            catch (ArgumentException)
            {
            }
            return "";
        }
    }
}
