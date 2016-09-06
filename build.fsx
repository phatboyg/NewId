#r @"src/packages/FAKE/tools/FakeLib.dll"
open System.IO
open Fake
open Fake.AssemblyInfoFile
open Fake.Git.Information
open Fake.SemVerHelper

let buildOutputPath = "./build_output"
let buildArtifactPath = "./build_artifacts"
let nugetWorkingPath = FullName "./build_temp"
let keyFile = FullName "./NewId.snk"
let buildMode = getBuildParamOrDefault "buildMode" "Release"

let assemblyVersion = "3.0.0.0"
let baseVersion = "3.0.0"

let semVersion : SemVerInfo = parse baseVersion

let Version = semVersion.ToString()

let branch = (fun _ ->
  (environVarOrDefault "APPVEYOR_REPO_BRANCH" (getBranchName "."))
)

let FileVersion = (environVarOrDefault "APPVEYOR_BUILD_VERSION" (Version + "." + "0"))
let commitHash = getCurrentHash()

let informationalVersion = (fun _ ->
  let branchName = (branch ".")
  let label = if branchName="master" then "" else " (" + branchName + "/" + (getCurrentSHA1 ".").[0..7] + ")"
  (FileVersion + label)
)

let nugetVersion = (fun _ ->
  let branchName = (branch ".")
  let label = if branchName="master" then "" else "-" + (branchName)
  (Version + label)
)

let InfoVersion = informationalVersion()
let NuGetVersion = nugetVersion()


printfn "Using version: %s" Version

Target "Clean" (fun _ ->

    !! buildOutputPath
      ++ buildArtifactPath
      ++ nugetWorkingPath
      ++ "src/*/bin"
      ++ "src/*/obj"
        |> CleanDirs

    ensureDirectory buildOutputPath
    ensureDirectory buildArtifactPath
    ensureDirectory nugetWorkingPath
)

Target "Build" (fun _ ->

  CreateCSharpAssemblyInfo @".\src\SolutionVersion.cs"
    [ Attribute.Title "NewId"
      Attribute.Description "NewId is an ordered 128-bit unique identifier generator using the Flake algorithm."
      Attribute.Product "NewId"
      Attribute.Version assemblyVersion
      Attribute.FileVersion FileVersion
      Attribute.InformationalVersion InfoVersion
      Attribute.Metadata("githash", commitHash)
    ]

  let restore f = DotNetCli.Restore (fun p ->
                { p with
                    AdditionalArgs = [f] })

  let projectJsonFiles = !! "src/*/project.json"

  projectJsonFiles
       |> Seq.iter restore

  projectJsonFiles
        |> DotNetCli.Build
            (fun p ->
                { p with
                    Configuration = buildMode })
)

Target "UnitTests" (fun _ ->

    !! "src/NewId.Tests/project.json"
        |>  DotNetCli.Test
            (fun p ->
                    { p with
                        Configuration = buildMode
                        AdditionalArgs = ["--result " + buildArtifactPath + "/nunit-test-results.xml"] })
)

Target "Package" (fun _ ->

    let libDir = nugetWorkingPath + "/lib"
    CopyRecursive "./src/NewId/bin/Release" libDir true |> ignore

    !! (libDir + "/**/*.deps.json")
      |> Seq.iter DeleteFile

    let setParams defaults = {
      defaults with 
        Authors = ["Chris Patterson"]
        Description = "NewId is an ordered 128-bit unique identifier generator using the Flake algorithm."
        OutputPath = buildArtifactPath
        Project = "NewId"
        Summary = "NewId is an ordered 128-bit unique identifier generator using the Flake algorithm."
        SymbolPackage = NugetSymbolPackage.Nuspec
        Version = Version
        WorkingDir =  nugetWorkingPath
    } 

    NuGet setParams (FullName "./template.nuspec")
)

Target "Default" (fun _ ->
  trace "Build starting..."
)

"Clean"
  ==> "Build"
  ==> "UnitTests"
  ==> "Package"
  ==> "Default"

RunTargetOrDefault "Default"