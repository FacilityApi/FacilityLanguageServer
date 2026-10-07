# Facility Language Server

Language Server Protocol server for the [Facility API Framework](https://facilityapi.github.io/).

## Facility.LanguageServer

Language server for Facility Service Definition documents.

See [FacilityVSCode](https://github.com/FacilityApi/FacilityVSCode) and [FacilityVisualStudio](https://github.com/FacilityApi/FacilityVisualStudio) for extensions that use this server.

Install from NuGet as a .NET tool:

```sh
dotnet tool install --global Facility.LanguageServer
facility-language-server
```

Run directly from NuGet without installing first with .NET 10+:

```sh
dnx Facility.LanguageServer --yes
```

To build a tool package locally, run `./build.ps1 package`. That produces `release/Facility.LanguageServer.*.nupkg`, which can be installed with `dotnet tool install --add-source ./release --tool-path ./tools Facility.LanguageServer`.

To publish, use GitHub to [publish a new release](https://github.com/FacilityApi/FacilityLanguageServer/releases) using a new tag called `v1.2.3`, where `1.2.3` matches the version in [Directory.Build.props](https://github.com/FacilityApi/FacilityLanguageServer/blob/master/Directory.Build.props).
