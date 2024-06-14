using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Translatable.Export.Shared;

namespace Translatable.Export
{
    public class AssemblyLoader : IAssemblyLoader
    {
        private readonly List<Assembly> _assemblies = new List<Assembly>();
        public IEnumerable<Assembly> Assemblies => _assemblies;

        public AssemblyLoader()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
        }

        public void LoadAssemblies(IEnumerable<string> assemblyFiles)
        {
            foreach (var assemblyFile in assemblyFiles)
            {
                try
                {
                    _assemblies.Add(Assembly.LoadFile(Path.GetFullPath(assemblyFile)));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not load assembly file '{assemblyFile}': {ex}");
                    throw;
                }
            }
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

            var assemblyName = args.Name.Split(new[] { ',' }, StringSplitOptions.None)[0];
            var uri = new UriBuilder(first.CodeBase);
            var assemblyFolder = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));
            var assemblyPath = Path.Combine(assemblyFolder, assemblyName + ".dll");

            if (File.Exists(assemblyPath))
                return Assembly.LoadFile(assemblyPath);

            return null;
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomainOnAssemblyResolve;
        }
    }
}