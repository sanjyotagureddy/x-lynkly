using Lynkly.Shared.Kernel.Core.Helpers;
using Lynkly.Shared.Kernel.Core.Helpers.Collections;

using CollectionExtensions = Lynkly.Shared.Kernel.Core.Helpers.Collections.CollectionExtensions;

namespace Lynkly.Resolver.UnitTests.SharedKernel.Helpers;

public sealed class CollectionsHelpersTests
{
    [Fact]
    public void DictionaryHelper_GetOrAdd_Should_AddAndReturn()
    {
        var dictionary = new Dictionary<string, int>();

        var created = DictionaryHelper.GetOrAdd(dictionary, "a", () => 10);
        var existing = DictionaryHelper.GetOrAdd(dictionary, "a", () => 20);

        Assert.Equal(10, created);
        Assert.Equal(10, existing);
        Assert.Equal(10, DictionaryHelper.GetValueOrDefault(dictionary, "a"));
        Assert.Equal(-1, DictionaryHelper.GetValueOrDefault(dictionary, "missing", -1));

        Assert.Throws<ArgumentNullException>(() => DictionaryHelper.GetOrAdd<string, int>(null!, "a", () => 0));
        Assert.Throws<ArgumentNullException>(() => DictionaryHelper.GetOrAdd(dictionary, "a", null!));
        Assert.Throws<ArgumentNullException>(() => DictionaryHelper.GetValueOrDefault<string, int>(null!, "a"));
    }

    [Fact]
    public void CollectionExtensions_Should_Work()
    {
        IEnumerable<int>? nullSequence = null;

        Assert.True(nullSequence.IsNullOrEmpty());
        Assert.True(Array.Empty<int>().IsNullOrEmpty());
        Assert.False(new[] { 1 }.IsNullOrEmpty());

        var filtered = new string?[] { "a", null, "b" }.WhereNotNull().ToArray();
        Assert.Equal(["a", "b"], filtered);

        var list = new List<int>();
        new[] { 1, 2, 3 }.ForEach(list.Add);
        Assert.Equal([1, 2, 3], list);

        Assert.Throws<ArgumentNullException>(() => CollectionExtensions.WhereNotNull<string>(null!));
        Assert.Throws<ArgumentNullException>(() => CollectionExtensions.ForEach<int>(null!, _ => { }));
        Assert.Throws<ArgumentNullException>(() => new[] { 1 }.ForEach(null!));
    }
}
