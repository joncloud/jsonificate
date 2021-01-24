# Jsonificate

[![NuGet](https://img.shields.io/nuget/v/Jsonificate.svg)](https://www.nuget.org/packages/Jsonificate/)

## Description

Jsonificate is a set of extensions for System.Text.Json. Most notably:

* Deep Cloning objects
* Serializing objects with [ObjectPools](https://www.nuget.org/packages/Microsoft.Extensions.ObjectPool/)

## Licensing

Released under the MIT License.  See the [LICENSE](LICENSE.md) file for further details.

## Getting started

Setup your project by adding a package reference:

```bash
dotnet add package Jsonificate
```

### Deep Cloning Objects

Use the extension method for `JsonSerializerOptions` in order to create a new cloner instance: `System.Text.Json.JsonSerializerOptionsExtensions.CreateCloner`:

```csharp
using System;
using System.Text.Json;

var options = new JsonSerializerOptions();
// Setup options your way.

var cloner = options.CreateCloner();

var original = new { x = 10, y = 53 };
var clone = cloner.Clone(original);

Console.WriteLine($"Original: {original}");
Console.WriteLine($"Clone: {clone}");
Console.WriteLine(object.ReferenceEquals(original, clone));
```

And the expected results:

```bash
Original: { x = 10, y = 53 }
Clone: { x = 10, y = 53 }
False
```

### Working with Object Pools

Augment an existing `JsonSerializerOptions` by adding an `ObjectPool<T>` to it with the extension method `System.Text.Json.JsonSerializerOptionsExtensions.AddPoolingConverter`.

```csharp
// Create your own pools
ObjectPool<MyObject> pool = ...;

var options = new JsonSerializerOptions()
  .AddPoolingConverter(_pool);

string json = "...";

var o = JsonSerializer.Deserialize<MyObject>(json, options);

DoWork(o);

pool.Return(o);
```

#### Custom Converters and Object Pools

Currently the conversion routine does not allow for a `JsonConverter` to be used in order to serialize top-level objects and also leverage `ObjectPool<T>`. The work around for this would be to introduce a top-level placeholder object that contains a reference of the type that needs to be customized.

## Building the solution

The majority of the GitHub Actions steps are available through [./actions](actions), and can be invoked to replicate the behavior.
