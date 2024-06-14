using Translatable.Export.Shared;

namespace Translatable.Export
{
    class Program
    {
        private static void Main(string[] args)
        {
            CommandLineEntrypoint.Run<AssemblyLoader>(args);
        }
    }
}