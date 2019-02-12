dotnet-warp
================


[![NuGet][main-nuget-badge]][main-nuget]
[![Build status](https://hrybak.visualstudio.com/dotnet-warp/_apis/build/status/dotnet-warp-CI)](https://hrybak.visualstudio.com/dotnet-warp/_build/latest?definitionId=12)

[main-nuget]: https://www.nuget.org/packages/dotnet-warp/
[main-nuget-badge]: https://img.shields.io/nuget/v/dotnet-warp.svg?style=flat-square&label=nuget

Global .NET Core tool, which allows to pack .NET Core projects to single executable. 

In fact it's just a wrapper around Warp (https://github.com/dgiagio/warp) and ILLink.Tasks.

Supported environments are same as Warp: win-x64, linux-x64, osx-x64

## Install


```bash
$ dotnet tool install --global dotnet-warp
```

## Usage

```bash
Usage: dotnet-warp [arguments] [options]

Arguments:
  ProjectFolder            Project path.

Options:
  -l|--link-level <LEVEL>  Optional. Sets link level. Available values: Normal, Aggressive.
  -nc|--no-crossgen        Optional. Disables Cross Gen during publish when linker is enabled. Sometimes required for linker to work. See issue: https://github.com/mono/linker/issues/314
  -v|--verbose             Optional. Enables verbose output.
```

## Examples

#### Packs project in current directory to single executable using Warp.
```bash
$ dotnet-warp 
```

#### Links project before packing using ILLink.Tasks
```bash
$ dotnet-warp -l aggressive
```

Aggressive option sets /p:RootAllApplicationAssemblies=false during publish. [More info](https://github.com/mono/linker/blob/fbe310a0c018ddcd701fe9ff91aa61ec6c026221/corebuild/README.md#options) 
