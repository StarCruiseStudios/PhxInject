# PHX.Inject.Generator

Compile time dependency injection for .NET.

Written as a Roslyn Source Generator, PHX.Inject will analyze dependency
specifications defined in your code and generate the source code that performs
the injection and linking at build time. This results in blazing fast injection
at runtime, and quick identification of dependency issues at compile time.

## Set up

PHX.Inject can be installed as a Nuget package using the .NET CLI.

```shell
dotnet add package Phx.Inject.Generator
```

## Getting Started

Documentation and set up instructions can be found in
the [PHX.Inject Repository](https://github.com/StarCruiseStudios/PhxInject)

---

Copyright (c) 2022 Star Cruise Studios LLC. All rights reserved.  
Licensed under the Apache License, Version 2.0.  
See http://www.apache.org/licenses/LICENSE-2.0 for full license information.