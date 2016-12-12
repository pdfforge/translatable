#r "packages/build/FAKE/tools/FakeLib.dll"

open Fake

let version = "0.1.0"
let buildNumber = match buildServer with
                     | TeamCity -> environVarOrFail "BUILD_NUMBER"
                     | GitLabCI -> environVarOrFail "CI_PIPELINE_ID"
                     | _ -> "0"

let buildSuffix = match buildServer with
                     | LocalBuild -> "-beta"
                     | _ -> ""

let fullVersion = sprintf "%s.%s" version buildNumber
let packageVersion = sprintf "%s%s" fullVersion buildSuffix

let buildDir = "build"
let artifactsDir = buildDir </> "artifacts"
let sourceDir = "Source"

open System

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
    !! (sourceDir @@ "**/AssemblyInfo.cs")
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
    !! (sourceDir @@ "**/AssemblyInfo.cs")
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
    !! (sourceDir @@ "**/*.csproj")
      |> MSBuildRelease "" "Build"
      |> Log "Build-Output: "
)

Description "Pack nuget packages"
Target "Pack" (fun _ ->
    Paket.Pack (fun p -> {p with OutputPath = artifactsDir; Version = packageVersion})
)

Description "Publish nuget packages"
Target "Publish" (fun _ ->
    Paket.Push (fun p -> {p with WorkingDir = artifactsDir})
)

"Clean"
   ==> "SetAssemblyVersion"
   ==> "SetAssemblyInfoYear"
   ==> "Compile"
   ==> "Pack"
   ==> "Publish"

RunTargetOrDefault "Pack"
