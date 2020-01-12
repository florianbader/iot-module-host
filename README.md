# IoT Edge Module Generic Host

![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/iot-module-host)
[![Build status](https://dev.azure.com/ait-fb/Public/_apis/build/status/IoT/iot-module-host.NuGet)](https://dev.azure.com/ait-fb/Public/_build/latest?definitionId=52?branchName=master)

IoT Module Host is a generic host implementation for IoT Edge modules. It makes it easier to start a new module implementation by taking away the boilerplate code which is required.

The solution consists of the following features:

* Startup class for
  * dependency injection
  * configuration of module client
  * desired property update
  * connection status change
* Define message handlers in classes
* Define method handlers in classes

## Getting Started

### Prerequisites
[.NET Core 3.0+ SDK](https://www.microsoft.com/net/download/core) must be installed.

### Installing
Install the dotnet-script CLI tool: ``dotnet tool restore``  
Build the solution ``dotnet build``

## Sample
See the [sample solution](samples/starter) to see how it works.

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct, and the process for submitting pull requests to us.

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/FlorianBader/iot-module-host/tags).

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details
