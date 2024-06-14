using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace Translatable.Export.Tool
{
    internal class LoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver _resolver;
        private readonly string _netCoreAppRuntimeDir;
        private readonly string _windowsDesktopRuntimeDir;

        public LoadContext(string assemblyPath)
        {
            _resolver = new(assemblyPath);
            _netCoreAppRuntimeDir = RuntimeEnvironment.GetRuntimeDirectory();
            // find the windowsDesktopApp runtime directory
            var versionWithoutPrefix = RuntimeEnvironment.GetSystemVersion().Replace("v", string.Empty);
            var windowsDesktopRuntimeDirectory = Path.Combine(
                    RuntimeEnvironment.GetRuntimeDirectory(),
                    "..",
                    "..",
                    "Microsoft.WindowsDesktop.App",
                    versionWithoutPrefix);
            if (!Path.Exists(windowsDesktopRuntimeDirectory))
                throw new Exception(
                    $"The windows desktop runtime could not be found. Version {versionWithoutPrefix} was expected.");
            _windowsDesktopRuntimeDir = windowsDesktopRuntimeDirectory;
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
                return LoadFromAssemblyPath(assemblyPath);

            assemblyPath = Path.Combine(_windowsDesktopRuntimeDir, $"{assemblyName.Name}.dll");
            if (File.Exists(assemblyPath))
                return LoadFromAssemblyPath(assemblyPath);

            assemblyPath = Path.Combine(_netCoreAppRuntimeDir, $"{assemblyName.Name}.dll");
            if (File.Exists(assemblyPath))
                return LoadFromAssemblyPath(assemblyPath);

            throw new Exception(
                $"Failed to find the assembly {assemblyName.Name} with the version {assemblyName.Version}");
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (libraryPath != null)
            {
                return LoadUnmanagedDllFromPath(libraryPath);
            }

            libraryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), unmanagedDllName);
            if (File.Exists(libraryPath))
                return LoadUnmanagedDllFromPath(libraryPath);
            throw new Exception($"Failed to find the unmanaged dll {unmanagedDllName}");
        }
    }
}