using System.Buffers;
using System.Runtime.InteropServices;
using System.Transactions;

namespace CriFsV2Lib.Utilities;

/// <summary>
/// Represents an individual borrowed array from the runtime.
/// </summary>
/// <summary>
/// Represents a temporary array rental from the runtime's ArrayPool.
/// </summary>
/// <typeparam name="T">Type of element to be rented from the runtime.</typeparam>
public struct ArrayRental<T> : IDisposable
{
    /// <summary>
    /// Empty rental.
    /// </summary>
    public static readonly ArrayRental<T> Empty = new ArrayRental<T>(0, false);

    private T[] _data;
    private int _count;
    private bool _canDispose;

    /// <summary>
    /// Rents an array of bytes from the arraypool.
    /// </summary>
    /// <param name="count">Needed amount of bytes.</param>
    public ArrayRental(int count) : this(count, true) { }

    /// <summary>
    /// Rents an array of bytes from the arraypool.
    /// </summary>
    /// <param name="count">Needed amount of bytes.</param>
    private ArrayRental(int count, bool canDispose)
    {
        _data = ArrayPool<T>.Shared.Rent(count);
        _count = count;
        _canDispose = canDispose;
    }

    /// <summary>
    /// Exposes the raw underlying array, which will likely
    /// be bigger than the number of elements.
    /// </summary>
    public T[] RawArray => _data;

    /// <summary>
    /// Returns the rented array as a span.
    /// </summary>
    public Span<T> Span => MemoryMarshal.CreateSpan(ref MemoryMarshal.GetArrayDataReference(_data), _count);

    /// <summary>
    /// Exposes the number of elements stored by this structure.
    /// </summary>
    public int Count => _count;

    /// <summary>
    /// Returns a reference to the first element.
    /// </summary>
    public ref T FirstElement => ref GetFirstElement();

    /// <summary>
    /// Returns the array to the pool.
    /// </summary>
    public void Dispose()
    {
        if (_canDispose)
            ArrayPool<T>.Shared.Return(_data, false);
    }

    /// <summary>
    /// Returns a reference to the first element.
    /// </summary>
    private ref T GetFirstElement() => ref MemoryMarshal.GetArrayDataReference(_data);
}