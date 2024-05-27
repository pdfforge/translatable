using GlobExpressions;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using System;
using System.Linq;

namespace build_tools
{
    internal static class PaketToolResolver
    {
        public static string ResolveNUnit3ConsoleRunner(AbsolutePath rootDirectory)
        {
            var nunitConsoleRunner = Glob.Files(rootDirectory / "packages", "**/nunit3-console.exe").FirstOrDefault();
            if (nunitConsoleRunner == null)
            {
                throw new Exception(
                    "The nunit3-console.exe was not found in the packages directory. Please make sure NUnit.ConsoleRunner 3.x is installed.");
            }

            return rootDirectory / "packages" / nunitConsoleRunner;
        }

        public static Tool ResolvePaketTool(AbsolutePath rootDirectory, string toolName)
        {
            var toolExecutable = Glob.Files(rootDirectory / "packages", $"**/{toolName}").FirstOrDefault();
            if (toolExecutable == null)
            {
                throw new Exception(
                    $"The tool {toolName} was not found in the packages directory. Please make sure it is installed with paket.");
            }

            return ToolResolver.GetTool(rootDirectory / "packages" / toolExecutable);
        }
    }
}