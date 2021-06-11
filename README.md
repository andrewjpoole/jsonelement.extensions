# JsonElement.Extensions


![Azure DevOps builds](https://img.shields.io/azure-devops/build/andrewjpoole/cdadc2b6-5273-431d-b963-d334c86e308b/7)
![Nuget](https://img.shields.io/nuget/v/AJP.JsonElementExtensions?label=nuget%20version)
![Nuget](https://img.shields.io/nuget/dt/AJP.JsonElementExtensions?label=nuget%20downloads)


## What is it?

A collection of extension methods for System.Text.Json.JsonElement, for adding/removing properties dynamically

## Why is it needed?

JsonElement is immutable, which is good, but lots of people have use case involving deserializing to a dynamic or JObject in order to 'tweak' an object thats returned from an API etc. There is currently no support for using dynamics with the serializers in System.Text.Json and wont be until after dotnet5. So I thought I'd try to fill the gap, at least temporarily. 

## How does it work?

So, these methods work by enumerating the existing properties of the JsonElement and then writing them into a new json string in memory with additional properties added or existing properties removed along the way. The resulting string is parsed into a new JsonElement which is returned. The original JsonElement is obviously unchanged. Hopefully the methods are fairly self explanatory and are well documented in the tripple slash comments.

## How to use it?
```csharp
var jsonString = "{ \"Name\": \"Andrew\", \"EmailAddress\": \"a@b.com\" }";
var jElement = JsonDocument.Parse(jsonString).RootElement;

jElement = jElement.AddProperty("isAdmin", true); // Remember to re-assign the return value, as it is a new JsonElement, i.e. the current one doesn't change
jElement.ToString().Dump(); // yields {"isAdmin":true,"Name":"Andrew","EmailAddress":"a@b.com"}
```

## Bulk updates

Please note this roundtrip process happens for every call, so if lots of changes are needed, please consider/test using `ParseAsJsonStringAndMutate()` which enables multiple changes to be made, with only one roudtrip process. This method provides an `Action<Utf8JsonWriter, List<string>>` the `Utf8JsonWriter` can be used to write additional properties (which will end up at the start of the list of properties) and the `List<string>` is a list of existing property names to skip when writing out the properties.

## Preserving property order

The recently added `Insert()`, `InsertNull()` and `Update()` methods all use `ParseAsJsonStringAndMutatePreservingOrder()` under the hood.
`ParseAsJsonStringAndMutatePreservingOrder()` first enermurates the JsonElement and then provides an `Action<PropertyList>` allowing the caller to mutate the list while preserving the order of the list, the mutated list is then enumerated to write the mutated properties into the new json string. Hence these methods are slightly more expensive in terms of speed and allocation than the non-order preserving counterparts (`AddProperty()`, `AddNullProperty()`, `RemoveProperty()` and `ParseAsJsonStringAndMutate()`).

```
|                    Method |     Mean |     Error |    StdDev |   Median |  Gen 0 |  Gen 1 | Gen 2 | Allocated |
|-------------------------- |---------:|----------:|----------:|---------:|-------:|-------:|------:|----------:|
|                  MutateV1 | 1.601 us | 0.0316 us | 0.0511 us | 1.593 us | 0.2060 |      - |     - |      1 KB |
| MutateV2_preserving_order | 3.013 us | 0.2588 us | 0.7549 us | 2.683 us | 0.6256 | 0.0038 |     - |      4 KB |
```

## License

Feel free to use for whatever - MIT license. If its usefull please let me know!

## Contributing

Contributions would be most welcome! the only thing I ask is that new features are covered by minimal unit tests, along the same lines as the existing ones. Please create a PR.
