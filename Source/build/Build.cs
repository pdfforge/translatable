using build_tools;
using Nuke.Common;
using Nuke.Common.CI.AppVeyor;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.NUnit;
using Nuke.Common.Tools.Paket;
using Nuke.Common.Utilities.Collections;
using Serilog;
using System;
using System.IO;
using System.Linq;

internal class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main() => Execute<Build>(x => x.Pack);

    private string BaseVersion => "0.3";
    private Version FullVersion => VersionHelper.GetTagVersion(BaseVersion);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    private readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution(GenerateProjects = true)]
    private Solution Solution { get; set; }

    private AbsolutePath SourceDir => RootDirectory / "Source";
    private AbsolutePath BuildDir => RootDirectory / "build";
    private AbsolutePath CompiledDir => BuildDir / "compiled";
    private AbsolutePath ArtifactsDir => BuildDir / "artifacts";
    private AbsolutePath LanguageDir => Solution.Translatable_SampleProject.Directory / "Languages";

    private string Version => IsServerBuild ? $"{BaseVersion}.{AppVeyor.Instance.BuildNumber}" : $"{BaseVersion}.0";
    private string BuildSuffix => IsLocalBuild ? "-beta" : string.Empty;
    private string PackageVersion => $"{Version}{BuildSuffix}";

    private Target Clean => t => t
        .Executes(() =>
        {
            BuildDir.DeleteDirectory();
        });

    private Target Restore => t => t
        .Executes(() =>
        {
            DotNetTasks.DotNetToolRestore();
        });

    private Target UpdateAssemblyInfo => t => t
        .DependsOn(Restore)
        .Executes(() =>
        {
            SourceDir
                .GlobFiles("**/AssemblyInfo.cs")
                .ForEach(f =>
                {
                    AssemblyInfoHelper.UpdateAssemblyInfo(f, FullVersion.ToString(4), "Avanquest pdfforge GmbH");
                });
        });

    private Target Compile => t => t
        .DependsOn(UpdateAssemblyInfo)
        .Executes(() =>
        {
            Solution.AllProjects
                .Where(project => project.Name != "_build")
                .ForEach(project =>
            {
                project.GetTargetFrameworks().ForEach(targetFramework =>
                {
                    DotNetTasks.DotNetBuild(build => build
                        .SetProjectFile(project)
                        .SetNoRestore(false)
                        .SetFramework(targetFramework)
                        .SetConfiguration(Configuration)
                        .SetOutputDirectory(CompiledDir / project.Name / targetFramework)
                    );
                });
            });
        });

    private Target RunUnitTests => t => t
        .DependsOn(Compile)
        .Executes(() =>
        {
            var unitTestDlls = CompiledDir.GlobFiles("**/*UnitTest.dll")
                .Select(absolutePath => absolutePath.ToString());

            NUnitTasks.NUnit3(nunit => nunit
                .SetProcessToolPath(PaketToolResolver.ResolveNUnit3ConsoleRunner(RootDirectory))
                .SetInputFiles(unitTestDlls));
        });

    private Target Pack => t => t
        .DependsOn(RunUnitTests)
        .Requires(() => Configuration == Configuration.Release)
        .Executes(() =>
        {// If you have a paket.template with the type project, paket tries to read stuff from the project file and the bin folder.
            // But we don't like our build script to fight with visual studio or any other too. For that reason we build to RepositoryRoot/BuildDir/...
            // It is not possible to tell paket to use a different folder to look for the binaries.
            // This task is a workaround for that. We copy the csproj and the paket template to the output folder and do the paket publish from there.

            var paketTemplates = SourceDir.GlobFiles("**/paket.template");

            foreach (var paketTemplate in paketTemplates)
            {
                // find the project file
                var csprojFile = paketTemplate.Parent.GlobFiles("*.csproj").FirstOrDefault();
                Assert.True(csprojFile.FileExists(), "Failed to find the csproj file to build the project. Please make sure it is located next to your paket.template.");

                // reorganize the output folder

                var workingDir = CompiledDir / csprojFile.NameWithoutExtension;
                var binDir = workingDir / "bin" / Configuration;
                binDir.CreateDirectory();
                workingDir.GlobDirectories("net*").ForEach(frameworkOutputDir => frameworkOutputDir.MoveToDirectory(binDir));

                File.Copy(csprojFile, workingDir / csprojFile.Name);
                File.Copy(paketTemplate, workingDir / paketTemplate.Name);
                // if the project has dependencies copy the paket.references file as well
                var paketReferencesFile = paketTemplate.Parent.GlobFiles("paket.references").FirstOrDefault();
                if (paketReferencesFile is not null && paketReferencesFile.Exists())
                {
                    File.Copy(paketReferencesFile, workingDir / "paket.references");
                }

                // Do the paket pack
                PaketTasks.PaketPack(pack => pack
                    .SetProcessWorkingDirectory(RootDirectory)
                    .SetPackageVersion(PackageVersion)
                    .SetTemplateFile(workingDir / paketTemplate.Name)
                    .SetOutputDirectory(ArtifactsDir)
                    .SetProcessArgumentConfigurator(arguments =>
                    {
                        var newArguments = new Arguments();
                        newArguments.Add("{value}", ["paket"]);
                        return newArguments.Concatenate(arguments);
                    })
                    .SetProcessToolPath("dotnet")
                );
            }

            // It is not possible to pack dotnet tools with paket. For that reason we use the default nuget task.
            DotNetTasks.DotNetPack(pack => pack
                .SetProject(Solution.Translatable_Export_Tool)
                .SetVersion(Version)
                .SetNoBuild(true)
                .SetProperty("OutputPath", CompiledDir / Solution.Translatable_Export_Tool.Name / Solution.Translatable_Export_Tool.GetTargetFrameworks()!.First())
                .SetOutputDirectory(ArtifactsDir));
        });

    private Target Publish => t => t
        .DependsOn(Pack)
        .Executes(() =>
        {
        });

    private Target ExportSampleProjectPot => t => t
        .DependsOn(Compile)
        .Executes(() =>
        {
            // Run the export with Translatable.Export for .NET Framework
            var inputDllFramework = CompiledDir / Solution.Translatable_SampleProject.Name / "net472" /
                Solution.Translatable_SampleProject.Name + ".exe";
            var outputFileFramework = LanguageDir / "net472_messages.pot";
            DotNetTasks.DotNetRun(run => run
                .SetProjectFile(Solution.Translatable_Export)
                .SetNoRestore(true)
                .SetConfiguration(Configuration)
                .SetApplicationArguments($"--outputfile {outputFileFramework} {inputDllFramework}")
            );

            // Run the export with Translatable.Export.Tool for .NET
            var targetFrameworkNet = Solution.Translatable_Export_Tool.GetTargetFrameworks()!.First();
            var inputDllNet = CompiledDir / Solution.Translatable_SampleProject.Name / $"{targetFrameworkNet}-windows" /
                Solution.Translatable_SampleProject.Name + ".dll";
            var outputFileNet = LanguageDir / $"{targetFrameworkNet}_messages.pot";
            DotNetTasks.DotNetRun(run => run
                .SetProjectFile(Solution.Translatable_Export_Tool)
                .SetNoRestore(true)
                .SetConfiguration(Configuration)
                .SetApplicationArguments($"--outputfile {outputFileNet} {inputDllNet}")
            );
        });

    private Target UpdatePoFiles => t => t
        .DependsOn(ExportSampleProjectPot)
        .Executes(() =>
        {
            var msgmerge = PaketToolResolver.ResolvePaketTool(RootDirectory, "msgmerge.exe");

            LanguageDir.GlobFiles("**/*.pot").ForEach(potFile =>
            {
                var poFileName = potFile.WithExtension("po").Name.Split('_')[1]; // this removes the framework prefix from the pot file
                LanguageDir.GlobFiles($"**/{poFileName}").ForEach(poFile =>
                {
                    msgmerge($"--previous {poFile} {potFile} --output-file={poFile}", RootDirectory, logger: (type, s) =>
                    {
                        if (s.StartsWith("..") &&
                            s.EndsWith(" done.")) // For some reason the msgmerge tool logs this as an error. Redirect it to information.
                        {
                            Log.Information(s);
                        }
                        else if (type == OutputType.Std)
                        {
                            Log.Information(s);
                        }
                        else if (s.Contains("warning: "))
                        {
                            Log.Warning(s);
                        }
                        else
                        {
                            Log.Error(s);
                        }
                    });
                });
            });
        });

    private Target CompileMoFiles => t => t
        .DependsOn(UpdatePoFiles)
        .Executes(() =>
        {
            var msgfmt = PaketToolResolver.ResolvePaketTool(RootDirectory, "msgfmt.exe");
            LanguageDir.GlobFiles("**/*.po").ForEach(poFile =>
            {
                var moFile = poFile.WithExtension("mo");
                msgfmt($"-o {moFile} {poFile}", RootDirectory);
            });
        });
}