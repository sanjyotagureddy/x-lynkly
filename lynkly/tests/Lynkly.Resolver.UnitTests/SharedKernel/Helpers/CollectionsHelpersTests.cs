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

    [Fact]
    public void CollectionExtensions_IsNullOrEmpty_Should_HandleReadOnlyCollection()
    {
        IEnumerable<int> empty = new ReadOnlyCollectionOnly<int>([]);
        IEnumerable<int> nonEmpty = new ReadOnlyCollectionOnly<int>([1, 2]);

        Assert.True(empty.IsNullOrEmpty());
        Assert.False(nonEmpty.IsNullOrEmpty());
    }

    [Fact]
    public void CollectionExtensions_IsNullOrEmpty_Should_HandlePlainEnumerable()
    {
        IEnumerable<int> empty = Enumerable.Empty<int>();
        IEnumerable<int> nonEmpty = Enumerable.Range(1, 2);

        Assert.True(empty.IsNullOrEmpty());
        Assert.False(nonEmpty.IsNullOrEmpty());
    }

    // Implements IReadOnlyCollection<T> but NOT ICollection<T>, to exercise that branch.
    private sealed class ReadOnlyCollectionOnly<T>(T[] items) : IReadOnlyCollection<T>
    {
        public int Count => items.Length;

        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)items).GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => items.GetEnumerator();
    }
}
