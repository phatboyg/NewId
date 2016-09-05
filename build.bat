@echo off
cls

"./src/.nuget/NuGet.exe" install FAKE						-o "./src/.buildTools" -ExcludeVersion "-Prerelease"
"./src/.nuget/NuGet.exe" install NUnit.Runners				-o "./src/.buildTools" -ExcludeVersion "-Prerelease"
"./src/.nuget/NuGet.exe" install GitVersion.CommandLine		-o "./src/.buildTools" -ExcludeVersion "-Prerelease"

"./src/.buildTools/FAKE/tools/FAKE.exe" build.fsx