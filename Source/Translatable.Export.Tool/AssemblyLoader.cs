using System.Reflection;
using Translatable.Export.Shared;

namespace Translatable.Export.Tool
{
    public class AssemblyLoader : IAssemblyLoader
    {
        private List<Assembly> _assemblies = new();
        public IEnumerable<Assembly> Assemblies => _assemblies;

        public void LoadAssemblies(IEnumerable<string> assemblyFiles)
        {
            foreach (var assemblyFile in assemblyFiles)
            {
                try
                {
                    var assemblyPath = Path.GetFullPath(assemblyFile);
                    var loadContext = new LoadContext(assemblyPath);
                    _assemblies.Add(loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(assemblyPath))));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not load assembly file '{assemblyFile}': {ex}");
                    throw;
                }
            }
        }

        public void Dispose()
        {
        }
    }
}