#r "packages/build/FAKE/tools/FakeLib.dll"

open Fake

let version = match buildServer with
                     | AppVeyor -> environVarOrFail "APPVEYOR_BUILD_VERSION"
                     | _ -> "0.1.0"

let buildSuffix = match buildServer with
                     | LocalBuild -> "-beta"
                     | _ -> ""

let fullVersion = version
let packageVersion = sprintf "%s%s" fullVersion buildSuffix

let buildDir = "build"
let artifactsDir = buildDir </> "artifacts"
let sourceDir = "Source"
let languageDir = sourceDir </> "TranslationTest/Languages"

open System
open System.IO

let execProcessOrFail cmd arguments =
    let result = ExecProcess (fun info ->
        info.FileName <- cmd
        info.Arguments <- arguments) (TimeSpan.FromMinutes 10.0)

    if result <> 0 then failwithf "the command '%s' failed with exit code %i" cmd result

// We are cleaning more directories than actually required. This ensures, that artifactsDir always exists in further targets
Description "Clean up"
Target "Clean" (fun _ ->
    let dirsToClean = [buildDir]
    
    let rec CleanOrRetry retries dirs =
        try CleanDirs dirs
        with | _ -> match retries with
                    | 0 -> reraise()
                    | _ -> tracefn "Clean failed... retries: %i" retries
                           System.Threading.Thread.Sleep 1000
                           CleanOrRetry (retries - 1) dirs

    CleanOrRetry 10 dirsToClean
)

Description "Update the assembly version number"
Target "SetAssemblyVersion" (fun _ ->
    !! (sourceDir </> "**/AssemblyInfo.cs")
    |> Seq.iter (fun f ->
      trace f 
      ReplaceAssemblyInfoVersions (fun p ->
      { p with
          OutputFileName = f
          AssemblyFileVersion = fullVersion
          AssemblyVersion = fullVersion
      }))
)

Description "Update the assembly copyright year"
Target "SetAssemblyInfoYear" (fun _ ->
    !! (sourceDir </> "**/AssemblyInfo.cs")
    |> Seq.iter (fun f ->
      trace f 
      ReplaceAssemblyInfoVersions (fun p ->
      { p with
          OutputFileName = f
          AssemblyCopyright = sprintf "Copyright pdfforge GmbH %i" (DateTime.Now.Year)
      }))
)

Description "Compile all C# projects"
Target "Compile" (fun _ ->
    !! (sourceDir </> "**/*.csproj")
      |> MSBuildRelease "" "Rebuild"
      |> Log "Build-Output: "
)

open Fake.Testing
Description "Run unit tests"
Target "Test" (fun _ ->
    let unitTestDir = buildDir </> "tests"

    !! (sourceDir </> "UnitTest/**/*.csproj")
      |> MSBuildRelease unitTestDir "Rebuild"
      |> Log "Build-Output: "

    !! (unitTestDir + @"\*Test.dll")
      |> xUnit (fun p -> {p with ToolPath = findToolInSubPath "xunit.console.exe" "packages/build" })
)

Description "Pack nuget packages"
Target "Pack" (fun _ ->
    Paket.Pack (fun p -> {p with OutputPath = artifactsDir; Version = packageVersion})
)

Description "Publish nuget packages"
Target "Publish" (fun _ ->
    Paket.Push (fun p -> {p with WorkingDir = artifactsDir})
)

Description "Export test translations"
Target "ExportPot" (fun _ ->
    let exportTool = findToolInSubPath "Translatable.Export.exe" (sourceDir </> "Translatable.Export/bin/Release/")
    let outputPath = languageDir </> "messages.pot"
    
    let assemblies = !! (sourceDir </> "TranslationTest/bin/Release/TranslationTest.exe")
                     |> Seq.toList
                     |> List.fold (fun str assembly -> sprintf "%s \"%s\"" str assembly) ""
    
    let argumentString = sprintf "--outputfile \"%s\" %s" outputPath assemblies

    execProcessOrFail exportTool argumentString
)

Description "Update po files with pots"
Target "UpdatePoFiles" (fun _ ->
    let msgmerge = findToolInSubPath "msgmerge.exe" "packages/build"
    
    !! (languageDir </> "*.pot")
    |> Seq.iter (fun potFile ->
        let poFilename = Path.ChangeExtension(Path.GetFileName(potFile), "po")
        !! (languageDir </> "**/" + poFilename)
        |> Seq.iter (fun poFile ->
            tracefn "Updating %s with %s" poFile potFile
            execProcessOrFail msgmerge (sprintf "--previous \"%s\" \"%s\" --output-file=\"%s\"" poFile potFile poFile)
        )
    )
)

Description "Compile mo files"
Target "CompileMoFiles" (fun _ ->
    let msgfmt = findToolInSubPath "msgfmt.exe" "packages/build"

    !! (languageDir </> "**/*.po")
    |> Seq.iter (fun poFile ->
        let moFile = Path.ChangeExtension(poFile, "mo")

        tracefn "Compiling %s" poFile

        let argString = sprintf "-o %s %s" moFile poFile
        execProcessOrFail msgfmt argString
    )
)

"Clean"
   ==> "SetAssemblyVersion"
   ==> "SetAssemblyInfoYear"
   ==> "Compile"
   ==> "Test"
   ==> "Pack"
   ==> "Publish"

"Compile"
   ==> "ExportPot"
   ==> "UpdatePoFiles"
   ==> "CompileMoFiles"

RunTargetOrDefault "Pack"
