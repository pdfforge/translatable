using System;
using System.Collections.Generic;
using System.Reflection;

namespace Translatable.Export.Shared
{
    public interface IAssemblyLoader : IDisposable
    {
        void LoadAssemblies(IEnumerable<string> assemblyFiles);

        IEnumerable<Assembly> Assemblies { get; }
    }
}