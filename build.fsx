#r @"src/packages/FAKE/tools/FakeLib.dll"
open System.IO
open Fake
open Fake.AssemblyInfoFile
open Fake.Git.Information
open Fake.SemVerHelper

let buildOutputPath = FullName "./build_output"
let buildArtifactPath = FullName "./build_artifacts"
let packagesPath = FullName "./src/packages"
let keyFile = FullName "./NewId.snk"

let assemblyVersion = "3.0.0.0"
let baseVersion = "3.0.0"

let semVersion : SemVerInfo = parse baseVersion

let Version = semVersion.ToString()

let branch = (fun _ ->
  (environVarOrDefault "APPVEYOR_REPO_BRANCH" (getBranchName "."))
)

let FileVersion = (environVarOrDefault "APPVEYOR_BUILD_VERSION" (Version + "." + "0"))

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
  ensureDirectory buildOutputPath
  ensureDirectory buildArtifactPath

  CleanDir buildOutputPath
  CleanDir buildArtifactPath
)

Target "RestorePackages" (fun _ -> 
  DotNetCli.Restore (fun p -> { p with Project = "./src/" } )
)

Target "Build" (fun _ ->

  CreateCSharpAssemblyInfo @".\src\SolutionVersion.cs"
    [ Attribute.Title "NewId"
      Attribute.Description "NewId is an ordered 128-bit unique identifier generator using the Flake algorithm."
      Attribute.Product "NewId"
      Attribute.Version assemblyVersion
      Attribute.FileVersion FileVersion
      Attribute.InformationalVersion InfoVersion
    ]
  DotNetCli.Build (fun p-> { p with Project = @".\src\NewId"
                                    Configuration= "Release"
                                    Output = buildArtifactPath})
)

Target "Package" (fun _ ->
  DotNetCli.Pack (fun p-> { p with 
                                Project = @".\src\NewId"
                                Configuration= "Release"
                                OutputPath= buildArtifactPath })
)

Target "Default" (fun _ ->
  trace "Build starting..."
)

"Clean"
  ==> "RestorePackages"
  ==> "Build"
  ==> "Package"
  ==> "Default"

RunTargetOrDefault "Default"