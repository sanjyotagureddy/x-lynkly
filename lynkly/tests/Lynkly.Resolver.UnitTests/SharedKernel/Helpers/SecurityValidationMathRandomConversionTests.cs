using Lynkly.Shared.Kernel.Core.Helpers;
using Lynkly.Shared.Kernel.Core.Helpers.Conversion;
using Lynkly.Shared.Kernel.Core.Helpers.Math;
using Lynkly.Shared.Kernel.Core.Helpers.Security;

namespace Lynkly.Resolver.UnitTests.SharedKernel.Helpers;

public sealed class SecurityValidationMathRandomConversionTests
{
    private enum Mode
    {
        Unknown = 0,
        Active = 1
    }

    [Fact]
    public void SecurityHelper_Should_Work()
    {
        var hash = SecurityHelper.ComputeSha256("abc");
        var base64 = SecurityHelper.ToBase64("lynkly");

        Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", hash);
        Assert.Equal("lynkly", SecurityHelper.FromBase64(base64));
        Assert.True(SecurityHelper.FixedTimeEquals("a", "a"));
        Assert.False(SecurityHelper.FixedTimeEquals("a", "b"));
        Assert.False(SecurityHelper.FixedTimeEquals("a", "aa"));
        Assert.True(SecurityHelper.FixedTimeEquals(null, null));
        Assert.False(SecurityHelper.FixedTimeEquals(null, "a"));

        Assert.Throws<ArgumentNullException>(() => SecurityHelper.ComputeSha256(null!));
        Assert.Throws<ArgumentNullException>(() => SecurityHelper.ToBase64(null!));
        Assert.Throws<ArgumentNullException>(() => SecurityHelper.FromBase64(null!));
    }

    [Fact]
    public void ValidationHelper_Should_Work()
    {
        Assert.Equal("x", ValidationHelper.AgainstNull("x", "p"));
        Assert.Equal("x", ValidationHelper.AgainstNullOrWhiteSpace("x", "p"));
        Assert.Equal(2, ValidationHelper.AgainstOutOfRange(2, 1, 3, "p"));

        Assert.Throws<ArgumentNullException>(() => ValidationHelper.AgainstNull<string>(null, "p"));
        Assert.Throws<ArgumentException>(() => ValidationHelper.AgainstNullOrWhiteSpace(" ", "p"));
        Assert.Throws<ArgumentOutOfRangeException>(() => ValidationHelper.AgainstOutOfRange(4, 1, 3, "p"));
    }

    [Fact]
    public void MathHelper_Should_Work()
    {
        Assert.Equal(5m, MathHelper.Clamp(5m, 1m, 10m));
        Assert.Equal(1m, MathHelper.Clamp(0m, 1m, 10m));
        Assert.Equal(10m, MathHelper.Clamp(11m, 1m, 10m));
        Assert.Equal(25m, MathHelper.Percentage(1m, 4m));
        Assert.Equal(1.23m, MathHelper.Round(1.234m, 2));

        Assert.Throws<ArgumentException>(() => MathHelper.Clamp(1m, 2m, 1m));
        Assert.Throws<DivideByZeroException>(() => MathHelper.Percentage(1m, 0m));
    }

    [Fact]
    public void RandomHelper_Should_Work()
    {
        var value = RandomHelper.NextInt(1, 5);
        Assert.InRange(value, 1, 4);

        var bytes = RandomHelper.NextBytes(8);
        var secureBytes = RandomHelper.NextSecureBytes(8);
        var secureValue = RandomHelper.NextSecureInt(1, 5);

        Assert.Equal(8, bytes.Length);
        Assert.Equal(8, secureBytes.Length);
        Assert.InRange(secureValue, 1, 4);

        Assert.Throws<ArgumentOutOfRangeException>(() => RandomHelper.NextBytes(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => RandomHelper.NextSecureBytes(-1));
    }

    [Fact]
    public void ConversionHelper_Should_Work()
    {
        Assert.True(ConversionHelper.TryConvert<int>("42", out var intResult));
        Assert.Equal(42, intResult);

        Assert.True(ConversionHelper.TryConvert<Mode>("Active", out var enumResult));
        Assert.Equal(Mode.Active, enumResult);

        Assert.True(ConversionHelper.TryConvert<Mode>(1, out enumResult));
        Assert.Equal(Mode.Active, enumResult);

        Assert.True(ConversionHelper.TryConvert<Mode?>("Active", out var nullableEnumResult));
        Assert.Equal(Mode.Active, nullableEnumResult);

        Assert.False(ConversionHelper.TryConvert<int>(null, out _));
        Assert.False(ConversionHelper.TryConvert<int>("bad", out _));
        Assert.False(ConversionHelper.TryConvert<int>(new object(), out _));
        Assert.False(ConversionHelper.TryConvert<int>(long.MaxValue, out _));

        Assert.Equal(5, ConversionHelper.ConvertOrDefault("bad", 5));
        Assert.Equal(10, ConversionHelper.ConvertOrDefault("10", 5));
    }
}
