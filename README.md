# jsonelement.extensions

## What is it?

A collection of extension methods for System.Text.Json.JsonElement, for adding/removing properties dynamically

## Why is it needed?

JsonElement is immutable, which is good, but lots of people have use case involving deserializing to a dynamic or JObject in order to 'tweak' an object thats returned from an API etc. There is currently no support for using dynamics with the serializers in System.Text.Json and wont be until after dotnet5. So I thought I'd try to fill the gap, at least temporarily. 

## How does it work?

So, these methods work by enumerating the existing properties of the JsonElement and then writing them into a new jsonstring in memory with additional properties added or existing properties removed along the way. The resulting string is parsed into a new JsonElement which is returned. The original JsonElement is obviously unchanged.

Please note this roundtrip process happens for every call, so if lots of changes are needed, please consider/test using ParseAsJsonStringAndMutate() so that all changes can be done together, with only one roudtrip process.

## How to use it?
```csharp
var jsonString = "{ \"Name\": \"Andrew\", \"EmailAddress\": \"a@b.com\" }";
var jElement = JsonDocument.Parse(jsonString).RootElement;

jElement = jElement.AddProperty("isAdmin", true);
jElement.ToString().Dump(); // yields {"isAdmin":true,"Name":"Andrew","EmailAddress":"a@b.com"}
```

## Limitations?

Doesn't currently support the ordering of properties, all new properties end up t the beginning

## License

Feel free to use for whatever - MIT license. If its usefull please let me know!

## Contributing

Contributions would be most welcome! the only thing I ask is that new features are covered by minimal unit tests, along the same lines as the existing ones. Please create a PR.
