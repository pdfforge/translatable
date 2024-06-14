using Nuke.Common.IO;
using System;

namespace build_tools
{
    public static class AssemblyInfoHelper
    {
        public static void UpdateAssemblyInfo(AbsolutePath file, string version, string company)
        {
            var lines = file.ReadAllLines();

            if (version.Split('.').Length == 3)
                version += ".0";
            var parsedVersion = new Version(version);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("AssemblyCompanyAttribute"))
                    lines[i] = $"[assembly: AssemblyCompanyAttribute(\"{company}\")]";

                if (lines[i].Contains("AssemblyCompany"))
                    lines[i] = $"[assembly: AssemblyCompany(\"{company}\")]";

                if (lines[i].Contains("AssemblyCopyrightAttribute"))
                    lines[i] = $"[assembly: AssemblyCopyrightAttribute(\"Copyright {DateTime.Now.Year} {company}\")]";

                if (lines[i].Contains("AssemblyCopyright"))
                    lines[i] = $"[assembly: AssemblyCopyright(\"Copyright {DateTime.Now.Year} {company}\")]";

                if (lines[i].Contains("AssemblyFileVersionAttribute"))
                    lines[i] = $"[assembly: AssemblyFileVersionAttribute(\"{parsedVersion.ToString(4)}\")]";

                if (lines[i].Contains("AssemblyFileVersion"))
                    lines[i] = $"[assembly: AssemblyFileVersion(\"{parsedVersion.ToString(4)}\")]";

                if (lines[i].Contains("AssemblyVersionAttribute"))
                    lines[i] = $"[assembly: AssemblyVersionAttribute(\"{parsedVersion.ToString(4)}\")]";

                if (lines[i].Contains("AssemblyVersion"))
                    lines[i] = $"[assembly: AssemblyVersion(\"{parsedVersion.ToString(4)}\")]";
            }

            file.WriteAllLines(lines);
        }
    }
}