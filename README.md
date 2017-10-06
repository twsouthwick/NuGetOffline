# NuGet Offline Downloader

[![Build status](https://ci.appveyor.com/api/projects/status/di3gj4lebjbeagss/branch/master?svg=true)](https://ci.appveyor.com/project/twsouthwick/nugetoffline/branch/master)

The purpose of this tool is to help those in network-constrained environments to still be able to utilize NuGet packages in their projects. This uses the core NuGet libraries but provides a way to download a library and its dependencies independent of a `.csproj` or other msbuild project file.

## Disclaimer

**NuGet.exe is the preferred way of bringing NuGet packages into a project. This should only be used if you cannot use NuGet.exe due to network restrictions**

## Usage

The main executable of the tool is `NuGetOffline.exe` and may be executed without any arguments to see the usage example:

```
NuGetOffline v0.1.0.19

Must supply a framework

usage: NuGetOffline [--framework <arg>] [--name <arg>] [--version <arg>]
                    [--feed [arg]] [--zip] [--path <arg>] [--verbose]

    --framework <arg>    .NET Target Framework Moniker
    --name <arg>         Name of NuGet package to download
    --version <arg>      Version of NuGet package
    --feed [arg]         NuGet feed to use
    --zip                Zip results
    --path <arg>         Path to write output to
    --verbose            Turn verbosity on
```

# Example

For an example, assume you are looking to download [DocumentFormat.OpenXML](https://www.nuget.org/packages/DocumentFormat.OpenXml/):

```
.\NuGetOffline.exe --framework net40 --name DocumentFormat.OpenXml --version 2.7.2 --path packages --verbose
```

The resulting `packages` directory has the following layout:

```
│ App.config
│ NuGet.Imports.props
│ NuGet.Imports.targets
└───DocumentFormat.OpenXml
    └───2.7.2
        └───lib
            └───net40
                    DocumentFormat.OpenXml.dll
                    DocumentFormat.OpenXml.xml
```

The files at the top are how the libraries are included into the project. Feel free to incorporate them however fits naturally into a project, but for those unfamiliar with msbuild, do the followign:

1. Add a file `dir.props` at the top level of your solution
2. Copy the `packages` dir to this same directory. If you call the downloader with the argument `--file packages.zip --zip` you'll have a zip file that just needs to be expanded in that directory.
3. Modify the `csproj` or other msbuild project files that need the reference to include `Import` statements for `NuGet.Imports.props` and `NuGet.Imports.targets` as the following:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <Import Project="$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props" Condition="Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')" />
  <Import Project="$([MSBuild]::GetDirectoryNameOfFileAbove($(MSBuildThisFileDirectory), dir.props))\packages\NuGet.Imports.props" />

...

  <Import Project="$([MSBuild]::GetDirectoryNameOfFileAbove($(MSBuildThisFileDirectory), dir.props))\packages\NuGet.Imports.targets" />
  <Import Project="$(MSBuildToolsPath)\Microsoft.CSharp.targets" />
</Project>
```
