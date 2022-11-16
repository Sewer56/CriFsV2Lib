using CriFsV2Lib.Definitions.Interfaces;

namespace CriFsV2Lib.Definitions;

/// <summary>
/// Interface for the batch file extractor utility.
/// </summary>
public interface IBatchFileExtractor<in T> : IDisposable where T : IBatchFileExtractorItem
{
    /// <summary>
    /// Number of items processed.
    /// </summary>
    int ItemsProcessed { get; }

    /// <summary>
    /// Queues an item for extracting.
    /// </summary>
    /// <param name="item">The item to queue.</param>
    void QueueItem(T item);

    /// <summary>
    /// Waits for completion of current items in a blocking fashion.
    /// </summary>
    /// <param name="timeBetweenCallbacks">Minimum time taken between individual callbacks.</param>
    /// <param name="callback">Action to be executed between each wait period.</param>
    void WaitForCompletion(int timeBetweenCallbacks = 64, Action? callback = null);

    /// <summary>
    /// Waits for completion of current items in a non-blocking fashion.
    /// </summary>
    /// <param name="timeBetweenCallbacks">Minimum time taken between individual callbacks.</param>
    /// <param name="callback">Action to be executed between each wait period.</param>
    Task WaitForCompletionAsync(int timeBetweenCallbacks = 64, Action? callback = null);
}