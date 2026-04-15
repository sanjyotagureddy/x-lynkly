using System.Text;
using Lynkly.Shared.Kernel.Helpers;

namespace Lynkly.Resolver.UnitTests.SharedKernel.Helpers;

public sealed class CoreHelpersTests
{
    private enum SampleEnum
    {
        Unknown = 0,
        Active = 1
    }

    [Fact]
    public void DateTimeHelper_EnsureUtc_Should_HandleKinds()
    {
        var unspecified = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Unspecified);
        var local = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Local);

        var utc = DateTimeHelper.EnsureUtc(unspecified);
        var localUtc = DateTimeHelper.EnsureUtc(local);

        Assert.Equal(DateTimeKind.Utc, utc.Kind);
        Assert.Equal(DateTimeKind.Utc, localUtc.Kind);
    }

    [Fact]
    public void DateTimeHelper_Should_ConvertUnixTime()
    {
        var now = DateTimeOffset.UtcNow;
        var unix = DateTimeHelper.ToUnixTimeSeconds(now);

        var restored = DateTimeHelper.FromUnixTimeSeconds(unix);

        Assert.Equal(now.ToUnixTimeSeconds(), restored.ToUnixTimeSeconds());
    }

    [Fact]
    public void DateTimeHelper_StartAndEndOfDayUtc_Should_Work()
    {
        var value = new DateTime(2026, 2, 2, 12, 30, 10, DateTimeKind.Utc);

        var start = DateTimeHelper.StartOfDayUtc(value);
        var end = DateTimeHelper.EndOfDayUtc(value);

        Assert.Equal(new DateTime(2026, 2, 2, 0, 0, 0, DateTimeKind.Utc), start);
        Assert.Equal(start.AddDays(1).AddTicks(-1), end);
    }

    [Fact]
    public void StringExtensions_Should_HandleValues()
    {
        string? empty = "   ";

        Assert.Null(empty.ToNullIfWhiteSpace());
        Assert.Equal("abc", "abcdef".Truncate(3));
        Assert.True("ABC".EqualsOrdinalIgnoreCase("abc"));
    }

    [Fact]
    public void StringExtensions_Truncate_Should_ValidateArguments()
    {
        Assert.Throws<ArgumentNullException>(() => StringExtensions.Truncate(null!, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => "abc".Truncate(-1));
    }

    [Fact]
    public void EncodingHelper_Should_EncodeDecodeUtf8()
    {
        var bytes = EncodingHelper.Utf8Encode("lynkly");

        Assert.Equal("lynkly", EncodingHelper.Utf8Decode(bytes));
        Assert.Throws<ArgumentNullException>(() => EncodingHelper.Utf8Encode(null!));
    }

    [Fact]
    public void EnumExtensions_Should_Work()
    {
        var value = SampleEnum.Active;

        Assert.Equal("Active", value.GetName());
        Assert.True(value.IsDefinedValue());
        Assert.Equal(SampleEnum.Active, EnumExtensions.ParseOrDefault("Active", SampleEnum.Unknown));
        Assert.Equal(SampleEnum.Unknown, EnumExtensions.ParseOrDefault("missing", SampleEnum.Unknown));
    }

    [Fact]
    public void TypeHelper_Should_Work()
    {
        Assert.True(TypeHelper.IsNullableType(typeof(string)));
        Assert.True(TypeHelper.IsNullableType(typeof(int?)));
        Assert.False(TypeHelper.IsNullableType(typeof(int)));
        Assert.Equal(0, TypeHelper.GetDefaultValue(typeof(int)));
        Assert.Null(TypeHelper.GetDefaultValue(typeof(string)));
        Assert.Throws<ArgumentNullException>(() => TypeHelper.IsNullableType(null!));
        Assert.Throws<ArgumentNullException>(() => TypeHelper.GetDefaultValue(null!));
    }

    [Fact]
    public void GeneralHelper_Should_Work()
    {
        var value = GeneralHelper.Coalesce<string?>(null, null, "value", "other");
        Assert.Equal("value", value);

        Assert.Null(GeneralHelper.Coalesce<string?>(null, null));
        Assert.Throws<ArgumentNullException>(() => GeneralHelper.Coalesce<string?>(null!));

        var left = 1;
        var right = 2;
        GeneralHelper.Swap(ref left, ref right);

        Assert.Equal(2, left);
        Assert.Equal(1, right);
    }

    [Fact]
    public void ConversionHelper_TryConvert_Should_HandleNullableValueTypes()
    {
        Assert.True(ConversionHelper.TryConvert<int?>("42", out var converted));
        Assert.Equal(42, converted);
    }
}
