# Skyline.DataMiner.CICD.Tools.Packager

## About

This .NET tool allows you to create application (.dmapp) and protocol (.dmprotocol) packages.

For more information about application packages, refer to [Application packages](https://aka.dataminer.services/application-packages).

### About DataMiner

DataMiner is a transformational platform that provides vendor-independent control and monitoring of devices and services. Out of the box and by design, it addresses key challenges such as security, complexity, multi-cloud, and much more. It has a pronounced open architecture and powerful capabilities enabling users to evolve easily and continuously.

The foundation of DataMiner is its powerful and versatile data acquisition and control layer. With DataMiner, there are no restrictions to what data users can access. Data sources may reside on premises, in the cloud, or in a hybrid setup.

A unique catalog of 7000+ connectors already exist. In addition, you can leverage DataMiner Development Packages to build you own connectors (also known as "protocols" or "drivers").

> **Note**
> See also: [About DataMiner](https://aka.dataminer.services/about-dataminer).

### About Skyline Communications

At Skyline Communications, we deal in world-class solutions that are deployed by leading companies around the globe. Check out [our proven track record](https://aka.dataminer.services/about-skyline) and see how we make our customers' lives easier by empowering them to take their operations to the next level.

## Getting Started

In commandline:
dotnet tool install -g Skyline.DataMiner.CICD.Tools.Packager

Then run the command for dmapp packages

```md
dataminer-package-create dmapp --help
```

or dmprotocol packages

```md
dataminer-package-create dmprotocol --help
```

## Details

### General options

```console
Description:
  This .NET tool allows you to create DataMiner application (.dmapp) and protocol (.dmprotocol) packages.

Usage:
  Skyline.DataMiner.CICD.Tools.Packager [command] [options]

Options:
  -o, --output <OUTPUT_DIRECTORY> (REQUIRED)  Directory where the package will be stored.
  -n, --name <OUTPUT_NAME>                    Name of the package.
  --version                                   Show version information
  -?, -h, --help                              Show help and usage information

Commands:
  dmapp <directory>       Creates a DataMiner application (.dmapp) package based on the type.
  dmprotocol <directory>  Creates a protocol (.dmprotocol) package based on a protocol solution.

```

### dmapp options

```console
Description:
  Creates a DataMiner application (.dmapp) package based on the type.

Usage:
  Skyline.DataMiner.CICD.Tools.Packager dmapp [<directory>] [options]

Arguments:
  <directory>  Directory containing the package items

Options:
  -t, --type <automation|dashboard|protocolvisio|visio>   Type of dmapp package.
  (REQUIRED)
  -bn, --build-number <BUILD_NUMBER>                      The build number.
  -v, --version <VERSION>                                 The version number for the artifact. This takes precedence
                                                          over 'build-number'. Supported formats: 'A.B.C', 'A.B.C.D',
                                                          'A.B.C-suffix' and 'A.B.C.D-suffix'.
  -pn, --protocolName <PROTOCOL_NAME>                     The protocol name. Only applicable for the 'protocolvisio'
                                                          type.
  -o, --output <OUTPUT_DIRECTORY> (REQUIRED)              Directory where the package will be stored.kv
  -n, --name <OUTPUT_NAME>                                Name of the package.
  -?, -h, --help                                          Show help and usage information
```

### dmprotocol options

```console
Description:
  Creates a protocol package (.dmprotocol) based on a protocol solution.

Usage:
  Skyline.DataMiner.CICD.Tools.Packager dmprotocol [<directory>] [options]

Arguments:
  <directory>  Directory containing the package items

Options:
  -vo, --versionOverride <VERSION_OVERRIDE>   Override the version in the protocol.
  -o, --output <OUTPUT_DIRECTORY> (REQUIRED)  Directory where the package will be stored.
  -n, --name <OUTPUT_NAME>                    Name of the package.
  -?, -h, --help                              Show help and usage information
```
