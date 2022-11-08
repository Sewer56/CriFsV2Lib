// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using CriFsV2Lib.Benchmarks;

BenchmarkRunner.Run<Decompression>();
Console.WriteLine("Hello, World!");