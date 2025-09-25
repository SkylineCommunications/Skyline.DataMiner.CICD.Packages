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
- Skyline.DataMiner.CICD.Assemblers.Common
- Skyline.DataMiner.CICD.Assemblers.Automation
- Skyline.DataMiner.CICD.Assemblers.Protocol
- Skyline.DataMiner.CICD.Parsers.Common
- Skyline.DataMiner.CICD.Parsers.Automation
- Skyline.DataMiner.CICD.Parsers.Protocol

Depending on the chosen NuGet, these libraries will provide the ability to easily convert from a DIS-provided Visual Studio Solution of your chosen type into either a *.dmapp* or *.dmprotocol* file. These files can then be installed on a DataMiner system.

## Repository Structure

This repository contains multiple related packages that have been merged from separate repositories to streamline development and maintenance:

### Main Packages
- **DMApp.*** - Application package creation functionality
- **DMProtocol** - Protocol package creation functionality  
- **Tools.Packager** - Command-line tool for package creation

### Integrated Components
- **Assemblers.*** - Previously from `Skyline.DataMiner.CICD.Assemblers` repository
- **Parsers.*** - Previously from `Skyline.DataMiner.CICD.Parsers` repository

The integration allows for better dependency management and more efficient development workflows.

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
