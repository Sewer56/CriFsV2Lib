using CriFsV2Lib.Compression;
using CriFsV2Lib.Encryption.Game;
using CriFsV2Lib.Structs;
using System.Collections.Concurrent;
using static CriFsV2Lib.CpkHelper;

namespace CriFsV2Lib.Utilities;

/// <summary>
/// Utility for efficient batch extracting of items.
/// </summary>
public class BatchFileExtractor<T> : IDisposable where T : IBatchFileExtractorItem
{
    private const int SleepTime = 16;
    private static FileStreamOptions _readOptions = new FileStreamOptions()
    {
        Access = FileAccess.Read,
        BufferSize = 0,
        Mode = FileMode.Open,
        Options = FileOptions.SequentialScan
    };

    private ConcurrentQueue<T> _extractItems = new ConcurrentQueue<T>();
    private int _numQueuedItems = 0;
    private ConcurrentQueue<FilePipelineItem> _decompressItems = new ConcurrentQueue<FilePipelineItem>();
    private ConcurrentQueue<FilePipelineItem> _writeItems = new ConcurrentQueue<FilePipelineItem>();

    private CancellationTokenSource _shutdownToken;
    private Thread _fileLoadThread;
    private Thread _fileWriteThread;
    private FileStream _sourceCpkStream;
    private InPlaceDecryptionFunction? _decrypt;

    public BatchFileExtractor(string sourceCpkPath, InPlaceDecryptionFunction? decrypt = null)
    {
        _shutdownToken = new CancellationTokenSource();
        _fileLoadThread = new Thread(FileLoadThread);
        _fileWriteThread = new Thread(FileWriteThread);
        _sourceCpkStream = new FileStream(sourceCpkPath, _readOptions);
        _fileLoadThread.Start();
        _fileWriteThread.Start();
        this._decrypt = decrypt;
    }

    ~BatchFileExtractor() => Dispose();

    /// <summary>
    /// Shuts down the batch extractor.
    /// </summary>
    public void Dispose()
    {
        _shutdownToken.Cancel();
        _sourceCpkStream.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Queues an item for extracting.
    /// </summary>
    /// <param name="item">The item to queue.</param>
    public void QueueItem(T item)
    {
        Interlocked.Add(ref _numQueuedItems, 1);
        _extractItems.Enqueue(item);
    }

    /// <summary>
    /// Waits for completion of current items in a blocking fashion.
    /// </summary>
    public void WaitForCompletion()
    {
        while (_numQueuedItems > 0)
            Thread.Sleep(SleepTime);
    }

    private void FileLoadThread(object? obj)
    {
        while (true)
        {
            // Get Item, Exit if Requested
            T? item; // Don't move above while true, else the capture for Task.Run might not copy
            while (!_extractItems.TryDequeue(out item))
            {
                Thread.Sleep(SleepTime);
                if (_shutdownToken.IsCancellationRequested)
                    return;
            }

            var data = CpkHelper.ExtractFileNoDecompression(item.File, _sourceCpkStream, out bool needsDecompression, _decrypt);
            if (!needsDecompression)
                _writeItems.Enqueue(new FilePipelineItem(item.FullPath, data));
            else
                Task.Run(() => { Decompress(data, item.FullPath); });

            // 👆 Decompress on ThreadPool
        }
    }

    private unsafe void Decompress(ArrayRental<byte> data, string fullPath)
    {
        try
        {
            fixed (byte* dataPtr = data.RawArray)
                _writeItems.Enqueue(new FilePipelineItem(fullPath, CriLayla.DecompressToArrayPool(dataPtr)));
        }
        finally
        {
            data.Dispose();
        }
    }

    private void FileWriteThread(object? obj)
    {
        FilePipelineItem item;
        while (true)
        {
            // Get Item, Exit if Requested
            while (!_writeItems.TryDequeue(out item))
            {
                Thread.Sleep(SleepTime);
                if (_shutdownToken.IsCancellationRequested)
                    return;
            }

            try
            {
                // Create Directory
                Directory.CreateDirectory(Path.GetDirectoryName(item.fullPath)!);

                // Write to disk.
                using var outputStream = new FileStream(item.fullPath, new FileStreamOptions()
                {
                    Access = FileAccess.Write,
                    BufferSize = 0,
                    Mode = FileMode.Create,
                    PreallocationSize = item.data.Span.Length
                });

                outputStream.Write(item.data.Span);
            }
            finally
            {
                item.data.Dispose();
                Interlocked.Add(ref _numQueuedItems, -1);
            }
        }
    }

    // Don't need records, it wastes IL space.
    private struct FilePipelineItem
    {
        public string fullPath; 
        public ArrayRental<byte> data;

        public FilePipelineItem(string fullPath, ArrayRental<byte> data)
        {
            this.fullPath = fullPath;
            this.data = data;
        }
    }
}

/// <summary>
/// Base interface used by items that can be queued to batch extractor.
/// </summary>
public interface IBatchFileExtractorItem
{
    /// <summary>
    /// Full path to where the file should be extracted to.
    /// </summary>
    public string FullPath { get; set; }

    /// <summary>
    /// Details of the file to be extracted.
    /// </summary>
    public CpkFile File { get; }
}
