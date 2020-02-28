# jsonelement.extensions

## What

A collection of extension methods for System.Text.Json.JsonElement, for adding/removing properties dynamically

## Why

JsonElement is immutable, which is good, but lots of people have use case involving deserializing to a dynamic or JObject in order to 'tweak' an object thats returned from an API etc. There is currently no support for using dynamics with the serializers in System.Text.Json and wont be until after dotnet5. So I thought I'd try to fill the gap, at least temporarily. 

## How

So, these methods work by enumerating the existing properties of the JsonElement and then writing them into a new jsonstring in memory with additional properties added or existing properties removed along the way. The resulting string is parsed into a new JsonElement which is returned. The original JsonElement is obviously unchanged.

Please note this roundtrip process happens for every call, so if lots of changes are needed, please consider/test using ParseAsJsonStringAndMutate() so that all changes can be done together, with only one roudtrip process.

## How to use
```csharp
var jsonString = "{ \"Name\": \"Andrew\", \"EmailAddress\": \"a@b.com\" }";
var jElement = JsonDocument.Parse(jsonString).RootElement;

jElement = jElement.AddProperty("isAdmin", true);
jElement.ToString().Dump(); // yields {"isAdmin":true,"Name":"Andrew","EmailAddress":"a@b.com"}
```

## Licence

MIT 
