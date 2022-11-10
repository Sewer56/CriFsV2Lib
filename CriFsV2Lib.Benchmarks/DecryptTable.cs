using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using CriFsV2Lib.Encryption;
using CriFsV2Lib.Tests.Reference;
using FileEmulationFramework.Tests;

namespace CriFsV2Lib.Benchmarks;

[MemoryDiagnoser]
public unsafe class DecryptTable
{
    private byte[] _data;
    private byte* _dataPtr;
    private GCHandle _handle;
    
    // Note: Encryption here is a XOR, so it reverses itself, so test data can be anything.
    
    [GlobalSetup]
    public void Setup()
    {
        _data = new byte[1024 * 1024 * 2]; // 2MB
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
        return CriPakToolsDecryptTable.DecryptUTF(_data);
    }
    
    [Benchmark]
    public byte[] CriFsLib()
    {
        return TableDecryptor.DecryptUTF(_dataPtr, _data.Length);
    }
    
    [Benchmark]
    public void CriFsLib_InPlace()
    {
        TableDecryptor.DecryptUTFInPlace(_dataPtr, _data.Length);
    }
}