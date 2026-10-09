# Skyline.DataMiner.CICD.Packages

## About

### About Skyline.DataMiner.CICD.Packages packages

Skyline.DataMiner.CICD.Packages packages are NuGet packages available in the public [nuget store](https://www.nuget.org/) that contain assemblies that enhance the CICD experience.

The following packages are available:

- Skyline.DataMiner.CICD.DMApp.Automation
- Skyline.DataMiner.CICD.DMApp.Common
- Skyline.DataMiner.CICD.DMApp.Dashboard
- Skyline.DataMiner.CICD.DMApp.Visio
- Skyline.DataMiner.CICD.DMProtocol

Depending on the chosen NuGet, these libraries will provide the ability to easily convert from a DIS-provided Visual Studio Solution of your chosen type into either a *.dmapp* or *.dmprotocol* file. These files can then be installed on a DataMiner system.

### About DataMiner

DataMiner is a transformational platform that provides vendor-independent control and monitoring of devices and services. Out of the box and by design, it addresses key challenges such as security, complexity, multi-cloud, and much more. It has a pronounced open architecture and powerful capabilities enabling users to evolve easily and continuously.

The foundation of DataMiner is its powerful and versatile data acquisition and control layer. With DataMiner, there are no restrictions to what data users can access. Data sources may reside on premises, in the cloud, or in a hybrid setup.

A unique catalog of 7000+ connectors already exist. In addition, you can leverage DataMiner Development Packages to build you own connectors (also known as "protocols" or "drivers").

> **Note**
> See also: [About DataMiner](https://aka.dataminer.services/about-dataminer)

### About Skyline Communications

At Skyline Communications, we deal in world-class solutions that are deployed by leading companies around the globe. Check out [our proven track record](https://aka.dataminer.services/about-skyline) and see how we make our customers' lives easier by empowering them to take their operations to the next level.

### Getting Started

The code is loosely based on the *Builder* design pattern. You can create a builder object using one of the provided static Factory classes:

```csharp
var builder = await ProtocolPackageCreator.Factory.FromRepositoryAsync(logCollector, repositoryPath);
var builder = AppPackageCreatorForVisio.Factory.FromRepository(logCollector, repositoryPath, packageName, packageVersion);
var builder = AppPackageCreatorForDashboard.Factory.FromRepository(logCollector, repositoryPath, packageName, packageVersion);
var builder = AppPackageCreatorForAutomation.Factory.FromRepository(logCollector, repositoryPath, packageName, packageVersion);
```

In most cases you don't need to add or configure additional things to the builders, they will contain all necessary information.
To actually create the *.dmapp* or *.dmprotocol* on your system you can call:

```csharp
await builder.CreateAsync(destinationFolder, packageFileName);
```

There is also an option to create the package in memory and return the byte array.

```csharp
byte[] package = builder.CreateAsync():
```

And lastly there is also an option to return an IAppPackage object that represents the package, allowing validation of all assemblies, scripts, ... before creating the *.dmapp* file.

```csharp
var package = await creator.BuildPackageAsync();
package.CreatePackage(destinationFilePath);
```

Complete Example:

```csharp
ILogCollector logCollector = new LogCollector();
string repositoryPath = @"C:\GITHUB\SLC-AS-EmpowerDemo1Room0";
string packageName = "EmpowerDemo1Room0";
var packageVersion = DMAppVersion.FromProtocolVersion("1.0.0.1");
string destinationFolder = @"C:\MyPackages\"
string packageFileName = "EmpowerDemo1Room0.dmapp";

var builder = AppPackageCreatorForAutomation.Factory.FromRepository(logCollector, repositoryPath, packageName, packageVersion);
await builder.CreateAsync(destinationFolder, packageFileName);
```

### Advanced Usage

You can also use these libraries, combined with the Skyline.DataMiner.Core.AppPackageCreator NuGet to create advanced packages containing multiple scripts, connectors, visio's, dashboards or other files.

Start by creating a new AppPackageBuilder

```csharp
var appPackageBuilder = new AppPackage.AppPackageBuilder(PackageName, PackageVersion.ToString(), GlobalDefaults.MinimumSupportDataMinerVersionForDMApp);
```

You can now create one or more of the builders as shown in [Getting Started](### Getting Started)

Those builders can then be asked to add their contents to your appPackageBuilder like so:

```csharp
builder.AddItemsAsync(appPackageBuilder);
```

You can also add other things to the appPackageBuilder itself at this point. Like additional files or other artifacts.

Once you've added all items from the individual builders to the appPackageBuilder you can create an object representing your complete package. This allows for final validation if needed.

```csharp
var appPackage = appPackageBuilder.Build();
```

You can now create the *.dmapp* file.

```csharp
package.CreatePackage(destinationFilePath);
```

Complete Example:

```csharp
string packageName = "EmpowerDemoRoom0";
string destinationFilePath = @"C:\MyPackages\EmpowerDemoRoom0.dmapp";
var packageVersion = DMAppVersion.FromProtocolVersion("1.0.0.1");
ILogCollector logCollector = new LogCollector();

string repositoryPath1 = @"C:\GITHUB\SLC-AS-EmpowerDemo1Room0";
string childPackageName1 = "EmpowerDemo1Room0.dmapp";
string repositoryPath2 = @"C:\GITHUB\SLC-AS-EmpowerDemo2Room0";
string childPackageName2 = "EmpowerDemo2Room0.dmapp";

var appPackageBuilder = new AppPackage.AppPackageBuilder(packageName, packageVersion.ToString(), GlobalDefaults.MinimumSupportDataMinerVersionForDMApp);
var builder1 = AppPackageCreatorForAutomation.Factory.FromRepository(logCollector, repositoryPath1, childPackageName1, packageVersion);
var builder2 = AppPackageCreatorForAutomation.Factory.FromRepository(logCollector, repositoryPath2, childPackageName2, packageVersion);
builder1.AddItemsAsync(appPackageBuilder);
builder2.AddItemsAsync(appPackageBuilder);

var appPackage = appPackageBuilder.Build();
package.CreatePackage(destinationFilePath);
```

### Scripted connectors (Skyline.DataMiner.CICD.DMProtocol)

Besides the regular protocol package contents (`Protocol.xml`, help files, Visio drawings, alarm/trending/information
templates, ...), `Skyline.DataMiner.CICD.DMProtocol` also resolves and embeds **scripted connector** ("script")
projects declared in the protocol solution. A scripted connector is never authored or distributed on its own — it is
always a "script project" living inside a protocol solution. Discovery and validation of script projects (parsing
`manifest.json`) is handled by `Skyline.DataMiner.CICD.Assemblers.Protocol`; `DMProtocol` resolves each script's
Python dependencies via `pip` and embeds the resulting wheels into the built `.dmprotocol` package.

The dependency-resolution logic is a C# port of the reference implementation available at
[PythonScriptedConnectorWheelsResolver](https://github.com/SkylineCommunications/PythonScriptedConnectorWheelsResolver).

#### Source directory layout

A script project lives in a `ConnectorScript_{n}/` folder at the root of the protocol solution (a sibling of the
existing `Dlls/` and `QAction_{n}/` folders). The `{n}` suffix is just a sequential counter assigned when the
script project was created — it does **not** correspond to the script's `id`/`guid` declared in `protocol.xml`. The
matching folder is instead found by its `manifest.json`'s `project.id`, which must match the `guid` attribute of
a `<Script id="..." guid="...">` declared under `<Protocol><Edge><Scripts>` in `protocol.xml`:

```text
MyProtocolSolution/
├── protocol.xml
├── Dlls/
├── QAction_1/
└── ConnectorScript_1/
    ├── ConnectorScript_1.pyproj   (Visual Studio Python Tools project file, not packaged)
    ├── requirements.txt              (direct dependencies only, no transitive dependencies; not packaged)
    ├── manifest.json
    ├── README.md                     (optional, copied as-is into the package)
    ├── run/
    │   └── main.py                   (or whichever file 'runtime.python.entry_point' declares)
    └── Tests/                        (not packaged)
```

Packaging is **opt-in**: only `manifest.json`, an optional `README.md`, and the full contents of `run/` are copied
into the built package's `Scripts/{guid}/` folder, alongside the resolved `dependencies/`. Everything else in the
project folder (`requirements.txt`, the `.pyproj` file, `Tests/`, and any other development-only content) is left
out of the package.

`manifest.json` schema (see [Edge Node "Scripted Connector Packaging" documentation](https://aka.dataminer.services)
for the authoritative description):

| Field                              | Description                                                                 |
|-------------------------------------|-------------------------------------------------------------------------------|
| `schema_version`                    | Version of the manifest schema (e.g. `"1.0"`).                                |
| `project.id`                        | Globally unique identifier (GUID) of the scripted connector.                  |
| `project.name`                      | Display name.                                                                 |
| `project.version`                   | Semantic version.                                                             |
| `project.description`               | Optional short description.                                                  |
| `project.author`                    | Optional author/organization.                                                |
| `runtime.language`                  | Currently only `"python"` is supported.                                      |
| `runtime.supported_platforms`       | Subset of `"x86_64-windows-msvc"`, `"x86_64-linux-gnu"`.                      |
| `runtime.python.version`            | Python version constraint (e.g. `">=3.14;<3.15"`). Currently only Python 3.14 is supported by the Edge Node runtime. |
| `runtime.python.entry_point`        | Path to the entry point script, relative to the project folder (e.g. `"run/main.py"`). Must be a relative path without `.`/`..` segments, and must resolve to a file under `run/` since only `run/` is packaged. |

#### Dependency resolution and platform-specific wheel selection

`requirements.txt` must only list the connector's **direct** dependencies (not their transitive dependencies). This
maximizes the chance of finding a wheel set that is valid for both Windows and Linux.

For each declared dependency (and its transitive dependencies, resolved by `pip`), wheels are downloaded separately
for:

- **Windows** (`win_amd64` platform tag).
- **Linux** (`manylinux2014_x86_64`, then `manylinux_2_17_x86_64` through the latest known `manylinux_2_<x>_x86_64`
  tag — the tool queries the [GNU glibc mirror](https://ftpmirror.gnu.org/glibc) to determine the latest available
  glibc 2.X version, falling back to a hardcoded version if that lookup fails).

Once downloaded, wheels are post-processed and organized into the script project's `Scripts/{guid}/dependencies/`
folder within the built `.dmprotocol` package:

- Wheels that are identical on both platforms are moved to `dependencies/universal/`.
- If one platform only has a platform-specific wheel for a package, but the other platform has a universal
  (`py3-none-any`) wheel for the same package/version, the platform-specific wheel is dropped in favor of the
  (now shared) universal wheel — this reduces overall package size, since a universal wheel only needs to be shipped
  once.
- Remaining platform-specific wheels stay in `dependencies/x86_64-windows-msvc/` or `dependencies/x86_64-linux-gnu/`
  respectively.

This requires a working `pip` installation on the machine performing the resolution (`python -m pip`, `python3 -m
pip`, `pip3` or `pip`, tried in that order on `PATH`) — the same requirement as the original Python reference
implementation.

#### Validation and exceptions

- `RequirementsNotFoundException` — a declared script's `requirements.txt` does not exist.
- `ConflictingDependenciesException` — `pip` detected conflicting dependencies during a dry-run install; the pip
  standard output/error are available on the exception for diagnostics.
- `PipNotFoundException` — no working `pip` invocation could be found on the current system.
- `InvalidManifestException` — a declared script's `manifest.json` is missing, malformed, fails schema
  validation (missing required fields, an empty `runtime.supported_platforms` list, or an `entry_point` that is
  rooted/absolute or contains `.`/`..` path segments), its declared entry point is not located under `run/` (only
  `run/` is included when the package is built), or its declared entry point file does not exist, or no `run/`
  folder exists at all. A `ParserException` is raised instead (from `Assemblers.Protocol`) if no `ConnectorScript_*`
  folder at the solution root has a `manifest.json` whose `project.id` matches the `guid` declared for a
  `<Script id="..." guid="...">` in `protocol.xml`, or if more than one folder's manifest matches the same `guid`.

A warning (not an exception) is reported through the supplied `ILogCollector` if a platform declared in
`runtime.supported_platforms` could not be resolved at all (e.g. no compatible wheel exists for that platform for one
of the requirements).
