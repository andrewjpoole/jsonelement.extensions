using System;
using BenchmarkDotNet.Running;

namespace AJP.JsonElementExtensions.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<AJP.Benchmarks>();
        }
    }
}
