# Central Package Management (CPM) Support

## Overview
This document describes the implementation of Central Package Management support in the Parsers.Common library.

## What is Central Package Management?
Central Package Management (CPM) is a NuGet feature that allows managing package versions centrally in a `Directory.Packages.props` file instead of specifying versions in individual project files. This is particularly useful for solutions with multiple projects that share common dependencies.

Learn more: https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management

## Implementation Details

### New Files
1. **DirectoryPackagesPropsParser.cs** - A new parser class that:
   - Searches for `Directory.Packages.props` files up the directory tree from a project directory
   - Parses `PackageVersion` items to extract package names and versions
   - Parses `GlobalPackageReference` items
   - Checks if CPM is enabled via the `ManagePackageVersionsCentrally` property
   - Returns a dictionary mapping package names to their versions

### Modified Files
1. **SdkStyleParser.cs** - Updated to:
   - Call `DirectoryPackagesPropsParser` to get central package versions
   - Pass central package versions to `LoadPackageReferenceItems`
   - Support `VersionOverride` attribute for packages
   - Merge package references from `.csproj` with versions from `Directory.Packages.props`

2. **LegacyStyleParser.cs** - Updated to:
   - Call `DirectoryPackagesPropsParser` to get central package versions
   - Pass central package versions to `LoadPackageReferenceItems`
   - Support `VersionOverride` attribute for packages
   - Maintain backward compatibility with packages.config and traditional package references

## Features Supported

### ✅ Supported Features
- **Directory.Packages.props discovery**: Automatically finds the appropriate `Directory.Packages.props` file by searching up the directory tree
- **PackageVersion items**: Reads package versions defined in `Directory.Packages.props`
- **GlobalPackageReference items**: Reads global package references that apply to all projects
- **VersionOverride**: Supports overriding centrally defined versions on a per-project basis
- **Backward compatibility**: Projects without CPM continue to work exactly as before
- **Legacy and SDK-style projects**: Both project types support CPM

### ⚠️ Limitations
- **Conditional versions**: MSBuild conditions on `PackageVersion` items are not evaluated
- **Transitive pinning**: Not explicitly handled (relies on normal NuGet resolution)
- **Multiple Directory.Packages.props**: Only the closest file is used (no automatic import of parent files)
- **Implicit GlobalPackageReferences**: Global package references are tracked but not automatically added to projects that don't reference them
- **Projects specifically disabling CPM**: Projects who disable CPM will still have versions resolved from `Directory.Packages.props`

## Usage Examples

### Example 1: Basic CPM Setup
**Directory.Packages.props:**
```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="Newtonsoft.Json" Version="13.0.3" />
    <PackageVersion Include="System.Text.Json" Version="8.0.0" />
  </ItemGroup>
</Project>
```

**MyProject.csproj:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Newtonsoft.Json" />
    <PackageReference Include="System.Text.Json" />
  </ItemGroup>
</Project>
```

The parser will return:
- PackageReference: Newtonsoft.Json, Version: 13.0.3
- PackageReference: System.Text.Json, Version: 8.0.0

### Example 2: Version Override
**Directory.Packages.props:**
```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="Newtonsoft.Json" Version="13.0.3" />
  </ItemGroup>
</Project>
```

**MyProject.csproj:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <PackageReference Include="Newtonsoft.Json" VersionOverride="12.0.3" />
  </ItemGroup>
</Project>
```

The parser will return:
- PackageReference: Newtonsoft.Json, Version: 12.0.3 (overridden)

### Example 3: Global Package References
**Directory.Packages.props:**
```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <GlobalPackageReference Include="Nerdbank.GitVersioning" Version="3.5.109" />
  </ItemGroup>
</Project>
```

Global package references are tracked and available but are not automatically added to projects.

## Testing Recommendations

1. **Test with CPM enabled**: Create a solution with `Directory.Packages.props` and verify packages are resolved correctly
2. **Test without CPM**: Verify existing projects without CPM continue to work
3. **Test version overrides**: Verify `VersionOverride` takes precedence over central versions
4. **Test directory hierarchy**: Place `Directory.Packages.props` at different levels and verify the closest one is used
5. **Test mixed scenarios**: Solutions with some projects using CPM and others not

## Migration Guide for Consumers

If you're using the Parsers.Common library to parse project files:

1. **No code changes required**: The API remains the same
2. **Automatic detection**: CPM is automatically detected and handled
3. **PackageReference version resolution**: 
   - If a project uses CPM, versions will be resolved from `Directory.Packages.props`
   - If not, versions will be read from the project file as before
   - `VersionOverride` always takes precedence

## Future Enhancements

Potential future improvements:
- Support for MSBuild condition evaluation in `PackageVersion` items
- Explicit handling of transitive pinning
- Support for importing parent `Directory.Packages.props` files
- Automatic inclusion of `GlobalPackageReference` items in all projects
