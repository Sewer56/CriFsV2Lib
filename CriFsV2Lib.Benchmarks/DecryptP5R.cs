using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using CriFsV2Lib.Encryption.Game;
using CriFsV2Lib.Tests.Reference;
using Microsoft.VisualStudio.TestPlatform.Utilities;

namespace CriFsV2Lib.Benchmarks;

public unsafe class DecryptP5R
{
    private const int Length = 0x1000;
    private byte* _dataPtr;
    private GCHandle _handle;

    [GlobalSetup]
    public void Setup()
    {
        var data = new byte[Length]; // 2MB
        for (int x = 0; x < data.Length; x++) 
            data[x] = (byte)x;

        _handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        _dataPtr = (byte*)_handle.AddrOfPinnedObject();
    }

    [GlobalCleanup]
    public void Cleanup() => _handle.Free();
    
    [Benchmark]
    public void Lipsum() => LipsumP5REncrypt.EncryptVecUnsafe(_dataPtr, Length);
    
    [Benchmark]
    public void AtlusPC() => AtlusEncrypt.EncryptAtlusP5R(_dataPtr, Length);
    
    [Benchmark]
    public void CriFsLibAvx() => P5RCrypto.DecryptInPlaceAvx(_dataPtr + P5RCrypto.EncryptedDataOffset);
    
    [Benchmark]
    public void CriFsLibSse() => P5RCrypto.DecryptInPlaceSse(_dataPtr + P5RCrypto.EncryptedDataOffset);
    
    [Benchmark]
    public void CriFsLibLong() => P5RCrypto.DecryptInPlaceLong(_dataPtr + P5RCrypto.EncryptedDataOffset);
}