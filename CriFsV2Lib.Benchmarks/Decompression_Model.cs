using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using CriFsV2Lib.Tests.Reference;
using FileEmulationFramework.Tests;

namespace CriFsV2Lib.Benchmarks;

[MemoryDiagnoser]
public unsafe class Decompression_Model
{
    private byte[] _data;
    private byte* _dataPtr;
    private GCHandle _handle;
    
    [GlobalSetup]
    public void Setup()
    {
        _data = File.ReadAllBytes(Assets.SampleCompressedModelFile);
        _handle = GCHandle.Alloc(_data, GCHandleType.Pinned);
        _dataPtr = (byte*)_handle.AddrOfPinnedObject();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _handle.Free();
        _data = null;
    }

    [Benchmark(Baseline = true)]
    public byte[] CriPak()
    {
        return CriPakToolsCriLayla.DecompressLegacyCRI(_data, _data.Length);
    }
    
    [Benchmark]
    public byte[] CriFsLib()
    {
        return Compression.CriLayla.Decompress(_dataPtr);
    }
}