dotnet-pack
================


[![NuGet][main-nuget-badge]][main-nuget]

[main-nuget]: https://www.nuget.org/packages/dotnet-pack/
[main-nuget-badge]: https://img.shields.io/nuget/v/dotnet-pack.svg?style=flat-square&label=nuget

Global .NET Core tool, which allows to pack .NET Core projects to single executable. 

In fact it's just a wrapper around Warp (https://github.com/dgiagio/warp) and ILLink.Tasks.

Supported environments are same as Warp: win-x64, linux-x64, osx-x64

## Install


```bash
$ dotnet tool install --global dotnet-depends
```

## Usage

```bash
Usage: dotnet-pack [arguments] [options]

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
$ dotnet-pack 
```

#### Links project before packing using ILLink.Tasks
```bash
$ dotnet-pack -l aggressive
```

Aggressive option sets /p:RootAllApplicationAssemblies=false during publish. [More info](https://github.com/mono/linker/blob/fbe310a0c018ddcd701fe9ff91aa61ec6c026221/corebuild/README.md#options) 
