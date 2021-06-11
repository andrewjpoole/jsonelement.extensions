using System.Collections.Generic;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using NUnit.Framework;

namespace AJP
{
    [MemoryDiagnoser]
    public class Benchmarks 
    {
        [Benchmark]
        public void MutateV1()
        {
            var jElement = GetJsonElement();

            jElement = jElement.ParseAsJsonStringAndMutate((utf8JsonWriter1, namesOfPropertiesToRemove) =>
            {
                namesOfPropertiesToRemove.Add("EmailAddress");
                utf8JsonWriter1.WriteBoolean("IsAdmin", true);
            });
        }

        private static JsonElement GetJsonElement()
        {
            const string jsonString = "{ \"Name\": \"Andrew\", \"EmailAddress\": \"a@b.com\" }";
            var jElement = JsonDocument.Parse(jsonString).RootElement;
            return jElement;
        }

        [Benchmark]
        public void MutateV2_preserving_order()
        {
            var jElement = GetJsonElement();

            jElement = jElement.ParseAsJsonStringAndMutatePreservingOrder(props =>
            {
                props.Remove("EmailAddress");
                props.Insert("IsAdmin", true, 1);
            }, new JsonSerializerOptions());
        }
    }
}