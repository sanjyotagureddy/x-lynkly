# Lynkly.Shared.Kernel.Helpers

## Purpose
`Lynkly.Shared.Kernel.Helpers` is a framework-agnostic shared utility package for reusable technical helpers across services and domains.

## Folder Structure
- `Collections/CollectionExtensions.cs`
- `Collections/DictionaryHelper.cs`
- `Core/GeneralHelper.cs`
- `Core/TypeHelper.cs`
- `Conversion/ConversionHelper.cs`
- `Conversion/EnumExtensions.cs`
- `DateTime/DateTimeHelper.cs`
- `Encoding/EncodingHelper.cs`
- `IO/FileHelper.cs`
- `IO/StreamHelper.cs`
- `Json/JsonHelper.cs`
- `Math/MathHelper.cs`
- `Math/RandomHelper.cs`
- `Networking/HttpRequestHelper.cs`
- `Networking/RetryHelper.cs`
- `Reflection/ReflectionHelper.cs`
- `Security/SecurityHelper.cs`
- `Security/ValidationHelper.cs`
- `Text/StringExtensions.cs`
- `Threading/TaskHelper.cs`

## Usage Examples
```csharp
var utcNow = DateTimeHelper.EnsureUtc(DateTime.Now);
var slug = "Lynkly".Truncate(4); // Lynk
var json = JsonHelper.Serialize(new { Name = "Lynkly" });
var ok = JsonHelper.TryDeserialize(json, out Dictionary<string, string>? value);

var result = await RetryHelper.ExecuteAsync(
    async _ => await SomeCallAsync(),
    new RetryPolicyOptions { RetryCount = 3, DelayStrategy = RetryDelayStrategy.ExponentialBackoff });

var hash = SecurityHelper.ComputeSha256("payload");
```

## Dependency Rationale
`JsonHelper` uses `Newtonsoft.Json` to provide reusable `JsonSerializerSettings`-based behavior and broad compatibility with existing polymorphic and contract customization scenarios used across .NET services. This package keeps that dependency isolated to JSON helper functionality.
