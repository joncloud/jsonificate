# Jsonificate

[![NuGet](https://img.shields.io/nuget/v/Jsonificate.svg)](https://www.nuget.org/packages/Jsonificate/)
![.NET](https://github.com/joncloud/jsonificate/workflows/.NET/badge.svg)

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

Use the extension method for `JsonSerializerOptions` in order to create a new cloner instance: `System.Text.Json.JsonSerializerOptionsExtensions.CreateCloner`. For more details, check out [DeepCloningObjects](./samples/ReadmeSamples/DeepCloningObjects.cs).

```csharp
using System;
using System.Text.Json;

var options = new JsonSerializerOptions();
// Setup options your way.

var cloner = options.CreateCloner();

var original = new Point { X = 10, Y = 53 };
var clone = cloner.Clone(original);

Console.WriteLine($"Original: {original}");
Console.WriteLine($"Clone: {clone}");
Console.WriteLine(object.ReferenceEquals(original, clone));
```

And the expected results:

```bash
Original: (10, 53)
Clone: (10, 53)
False
```

### Working with Object Pools

Augment an existing `JsonSerializerOptions` by adding an `ObjectPool<T>` to it with the extension method `System.Text.Json.JsonSerializerOptionsExtensions.AddPoolingConverter`. For more details, check out [WorkingWithObjectPools](./samples/ReadmeSamples/WorkingWithObjectPools.cs).

```csharp
using Microsoft.Extensions.ObjectPool;

// Create your own pools
ObjectPool<Point> pool = ...;

var options = new JsonSerializerOptions()
  .AddPoolingConverter(pool);

string json = "{\"X\":10,\"Y\":53}";

var p = JsonSerializer.Deserialize<Point>(json, options);

DoWork(p);

pool.Return(p);
```

#### Custom Converters and Object Pools

Using a top-level object that does not require a custom converter will keep thing simple, however when this cannot be achieved `Jsonificate.PoolingJsonConverter` can be overridden. For more details, check out [WorkingWithObjectPoolsCustomConverter](./samples/ReadmeSamples/WorkingWithObjectPoolsCustomConverter.cs).

```csharp

ObjectPool<Point> pool = ...;

var options = new JsonSerializerOptions();
options.Converters.Add(new PointPoolingJsonConverter(pool, options));

public class Point
{
  public int X { get; set; }
  public int Y { get; set; }
}

public class PointPoolingJsonConverter : PoolingJsonConverter<Point>
{
  public PointPoolingJsonConverter(ObjectPool<Point> pool, JsonSerializerOptions options)
    : base(pool, options)
  {
  }

  protected override void Populate(ref Utf8JsonReader reader, Type typeToConvert, Point value, JsonSerializerOptions options)
  {
    if (reader.TokenType != JsonTokenType.StartArray)
    {
      throw new JsonException();
    }

    value.X = reader.Read() ? reader.GetInt32() : throw new JsonException();
    value.Y = reader.Read() ? reader.GetInt32() : throw new JsonException();

    if (!reader.Read() || reader.TokenType != JsonTokenType.EndArray)
    {
      throw new JsonException();
    }
  }

  public override void Write(Utf8JsonWriter writer, Point value, JsonSerializerOptions options)
  {
    writer.WriteStartArray();
    writer.WriteNumberValue(value.X);
    writer.WriteNumberValue(value.Y);
    writer.WriteEndArray();
  }
}
```

### More Samples

* [Integrating with ASP.NET Core](./samples/AspNetCoreSamples)
* [Integrating with SignalR](./samples/SignalRSamples)

## Building the solution

The majority of the GitHub Actions steps are available through [actions](./actions), and can be invoked to replicate the behavior.
