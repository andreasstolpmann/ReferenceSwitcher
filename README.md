# Reference Switcher

<!-- Replace this badge with your own-->
[![Build status](https://ci.appveyor.com/api/projects/status/hv6uyc059rqbc6fj?svg=true)](https://ci.appveyor.com/project/madskristensen/extensibilitytools)

<!-- Update the VS Gallery link after you upload the VSIX-->
Download this extension from the [VS Gallery](https://visualstudiogallery.msdn.microsoft.com/[GuidFromGallery])
or get the [CI build](http://vsixgallery.com/extension/7255b31e-0997-4b88-9498-93fb5606e249/).

---------------------------------------

Adds an item to the TOOLS menu to allow you to switch from PackageReferences to ProjectReferences. ProjectReferences are also added to the solution file.

When executing the command a OpenFileDialog will appear and ask for a json file containing the names of PackageReferences to be replaced and the paths of the corresponding csproj file.
```
{
  "ClassLibrary1": "C:\\Development\\ClassLibrary1\\ClassLibrary1\\ClassLibrary1.csproj",
}
```


See the [change log](CHANGELOG.md) for changes and road map.

## Contribute
Check out the [contribution guidelines](CONTRIBUTING.md)
if you want to contribute to this project.

For cloning and building this project yourself, make sure
to install the
[Extensibility Tools 2015](https://visualstudiogallery.msdn.microsoft.com/ab39a092-1343-46e2-b0f1-6a3f91155aa6)
extension for Visual Studio which enables some features
used by this project.

## License
[Apache 2.0](LICENSE)