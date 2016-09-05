#r "./src/.buildTools/FAKE/tools/FakeLib.dll"

open Fake
open Fake.AssemblyInfoFile
open Fake.FileUtils
open Fake.FileSystemHelper
open Fake.NuGetHelper
open System.IO;
open System.Collections.Generic;
open Fake.Testing.NUnit3
open Fake.GitVersionHelper

let PRODUCT = "NewId"
let CLR_TOOLS_VERSION = "v4.0.30319"
let OUTPUT_PATH = ("bin" @@ "Release")

let buildOutputPath = "build_output"
let buildArtifactPath = "build_artifacts"
let nugetWorkingPath = "build_temp"
let packagesPath = FullName ("src " @@ "packages")
let keyFile = FullName "NewId.snk"
let gitProps = GitVersion (fun p -> 
                { p with 
                    ToolPath = ("src" @@ ".buildTools" @@ "GitVersion.CommandLine" @@ "tools" @@ "GitVersion.exe") 
                })

Target "Clean" (fun _ ->
  ensureDirectory buildOutputPath
  ensureDirectory buildArtifactPath
  ensureDirectory nugetWorkingPath

  CleanDir buildOutputPath
  CleanDir buildArtifactPath
  CleanDir nugetWorkingPath
)


Target "RestorePackages" (fun _ -> 
     ("src" @@ "NewId.sln")
     |> RestoreMSSolutionPackages (fun p ->
         { p with
             OutputPath = packagesPath
             Retries = 4 })
)

Target "GenerateSolutionVersion" (fun _ ->

    CreateCSharpAssemblyInfo ("src" @@ "SolutionVersion.cs")
        [Attribute.Description "NewId is an ordered 128-bit unique identifier generator using the Flake algorithm."
         Attribute.Product PRODUCT
         Attribute.Version gitProps.AssemblySemVer
         Attribute.FileVersion gitProps.AssemblySemVer
         Attribute.InformationalVersion gitProps.InformationalVersion
         Attribute.CLSCompliant true
         Attribute.ComVisible false]

)

Target "Build" (fun _ ->

  let buildMode = getBuildParamOrDefault "buildMode" "Release"
  let setParams defaults = { 
    defaults with
        Verbosity = Some(Quiet)
        Targets = ["Clean"; "Build"]
        Properties =
            [
                "Optimize", "True"
                "SignAssembly", "True"
                "AssemblyOriginatorKeyFile", keyFile
                "DebugSymbols", "True"
                "RestorePackages", "True"
                "Configuration", buildMode
                "TargetFrameworkVersion", "v4.5"
                "Platform", "Any CPU"
            ]
  }

  build setParams ("src" @@ "NewId.sln")
      |> DoNothing
)

Target "UnitTests" (fun _ ->
    
    !! ("src" @@ "**" @@ "bin" @@ "Release" @@ "*.Tests.dll")
        |> NUnit3 (fun p -> 
                {p with
                    Framework = NUnit3Runtime.Net40
                    DisposeRunners = true
                    ResultSpecs = [buildArtifactPath @@ "TestResult.xml"]
                    TeamCity = TeamCityVersion.IsSome
                })

)

Target "Zip" (fun _ ->

    let fileName = sprintf "NewId-%s.zip" gitProps.NuGetVersion

    !! ("src" @@ "NewId" @@ "bin" @@ "Release" @@ "*.*")
        |> Zip ("src" @@ "NewId" @@ "bin" @@ "Release") (buildArtifactPath @@ fileName)

)

Target "CreateNuGetPackage" (fun _ ->
    let localBuildOutputDir = ("src" @@ "NewId" @@ "bin" @@ "Release")

    cp_r localBuildOutputDir (nugetWorkingPath @@ "lib" @@ "net45")

    NuGetPack(fun p ->
        { p with
            Authors = ["Chris Patterson"]
            Project = "NewId"
            Description = "NewId is an ordered 128-bit unique identifier generator."
            OutputPath = buildArtifactPath
            Version = gitProps.NuGetVersion
            WorkingDir = nugetWorkingPath
            Copyright = "Copyright 2015 Chris Patterson, All rights reserved."
            Publish = false
        })
        "package.nuspec"
)

Target "Default" (fun _ ->
    trace "Build starting..."
)

"Clean"
  ==> "RestorePackages"
  ==> "GenerateSolutionVersion"
  ==> "Build"
  ==> "UnitTests"
  ==> "Zip"
  ==> "CreateNuGetPackage"
  ==> "Default"

RunTargetOrDefault "Default"