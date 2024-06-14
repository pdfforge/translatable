using Translatable.Export.Shared;

namespace Translatable.Export.Tool;

public class Program
{
    public static void Main(string[] args)
    {
        CommandLineEntrypoint.Run<AssemblyLoader>(args);
    }
}