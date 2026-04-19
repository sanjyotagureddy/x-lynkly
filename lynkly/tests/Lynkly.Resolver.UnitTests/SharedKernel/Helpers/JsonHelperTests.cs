using Lynkly.Shared.Kernel.Core.Helpers;
using Lynkly.Shared.Kernel.Core.Helpers.Json;

using Newtonsoft.Json;

namespace Lynkly.Resolver.UnitTests.SharedKernel.Helpers;

public sealed class JsonHelperTests
{
    private sealed class Person
    {
        public string Name { get; init; } = string.Empty;
    }

    private sealed class Circular
    {
        public Circular Self => this;
    }

    [Fact]
    public void JsonHelper_SerializeAndDeserialize_Should_Work()
    {
        var json = JsonHelper.Serialize(new Person { Name = "lynkly" });
        var person = JsonHelper.Deserialize<Person>(json);

        Assert.Equal("lynkly", person.Name);
    }

    [Fact]
    public void JsonHelper_CreateSettings_Should_ApplyCustomizations()
    {
        var settings = JsonHelper.CreateSettings(options => options.NullValueHandling = NullValueHandling.Include);

        Assert.Equal(NullValueHandling.Include, settings.NullValueHandling);
    }

    [Fact]
    public void JsonHelper_Deserialize_Should_ValidateInput()
    {
        Assert.Throws<ArgumentException>(() => JsonHelper.Deserialize<Person>("  "));
        Assert.Throws<JsonSerializationException>(() => JsonHelper.Deserialize<Person>("null"));
    }

    [Fact]
    public void JsonHelper_TryDeserialize_Should_ReturnFalseForInvalidInput()
    {
        Assert.False(JsonHelper.TryDeserialize<Person>(null, out _));
        Assert.False(JsonHelper.TryDeserialize<Person>("{invalid", out _));
        Assert.True(JsonHelper.TryDeserialize<Person>("{\"Name\":\"lynkly\"}", out var person));
        Assert.Equal("lynkly", person!.Name);
    }

    [Fact]
    public void JsonHelper_SafeSerialize_Should_ReturnFallbackForJsonExceptions()
    {
        var circular = new Circular();
        var settings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Error };

        var result = JsonHelper.SafeSerialize(circular, "fallback", settings);

        Assert.Equal("fallback", result);
    }
}
