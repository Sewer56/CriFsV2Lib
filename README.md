CriFsV2Lib Reloaded
===========

A minimal library to extract contents from CRI Middleware's CPK archive format. (a.k.a. CRI Filemajik library)

Goals:  
- Clean codebase.  
- Minimalist.  (does minimal amount of work)
- Trimmable dependencies.  (min code used after Assembly Trimming)
- High performance.  

I wrote this library for my personal needs (i.e. for use with [CriFs.V2.Hook](https://github.com/Sewer56/CriFs.V2.Hook.ReloadedII) extensions).  
Does not support packing/repacking; if you need that functionality, fire a pull request 😇.  


Feature Support
===============

- Decompression (CRILayla)  
- Header Decryption  

Usage
=====

Coming Soon.


Performance
===========

Some parts of this library are heavily tuned for performance. For example the *decompression algorithm*.

```
BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19044.2130/21H2/November2021Update)
Intel Core i7-4790K CPU 4.00GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
.NET SDK=7.0.100
  [Host]     : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT AVX2

|   Method |     Mean |    Error |  StdDev | Ratio |    Gen0 |    Gen1 |    Gen2 | Allocated | Alloc Ratio |
|--------- |---------:|---------:|--------:|------:|--------:|--------:|--------:|----------:|------------:|
|   CriPak | 983.3 us | 11.02 us | 9.77 us |  1.00 | 50.7813 | 50.7813 | 50.7813 | 166.01 KB |        1.00 |
| CriFsLib | 355.4 us |  3.20 us | 2.83 us |  0.36 | 52.2461 | 52.2461 | 52.2461 | 165.71 KB |        1.00 |
```

(`CriPak` is the reference implementation from [CriPakTools](https://github.com/wmltogether/CriPakTools)).

Resources
=====

This code is based on the following previous projects.  

- Skyth (https://github.com/blueskythlikesclouds/MikuMikuLibrary/blob/dotnet/MikuMikuLibrary/Archives/CriMw/UtfTable.cs)  
- TGE (https://github.com/tge-was-taken/010-Editor-Templates/blob/master/releases/cri_archives/cri_archives_rel_1.bt)  
- CriPakTools by Falo, Nanashi3, esperknight, uyjulian & wmltogether (https://github.com/wmltogether/CriPakTools)

Building
=========
- Install .NET 7 SDK.
- Build with `dotnet build -c Release` 
or if you prefer an IDE, just open `CriFsV2Lib.sln`.