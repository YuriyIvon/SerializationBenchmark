using BenchmarkDotNet.Attributes;

namespace SerializationBenchmark
{
    [MemoryDiagnoser]
    public class JsonBenchmark
    {
        private readonly IEnumerable<SalesItem> _separateItemsCollection;
        private readonly SalesItemContainer _singleObject;
        private readonly IEnumerable<string> _serializedSeparateItems;
        private readonly string _serializedSingleObject;

        public JsonBenchmark()
        {
            var dataSource = new DataSource();
            _separateItemsCollection = dataSource.GetData();
            _singleObject = new SalesItemContainer { Items = _separateItemsCollection };

            _serializedSeparateItems = SerializeCollectionStandard(_separateItemsCollection);
            _serializedSingleObject = System.Text.Json.JsonSerializer.Serialize(_singleObject);

            Console.WriteLine($"JSON separate items serialized size: {_serializedSeparateItems.Sum(j => j.Length)} bytes");
            Console.WriteLine($"JSON single object serialized size: {_serializedSingleObject.Length} bytes");
        }

        public bool Validate()
        {
            var deserializedCollectionStandard = DeserializeCollectionStandard(_serializedSeparateItems);
            var deserializedContainerStandard = System.Text.Json.JsonSerializer.Deserialize<SalesItemContainer>(_serializedSingleObject);
            var deserializedCollectionNewtonsoft = DeserializeCollectionNewtonsoft(_serializedSeparateItems);
            var deserializedContainerNewtonsoft = Newtonsoft.Json.JsonConvert.DeserializeObject<SalesItemContainer>(_serializedSingleObject);

            return deserializedCollectionStandard.SequenceEqual(_separateItemsCollection) &&
                deserializedContainerStandard.Items.SequenceEqual(_separateItemsCollection) &&
                deserializedCollectionNewtonsoft.SequenceEqual(_separateItemsCollection) &&
                deserializedContainerNewtonsoft.Items.SequenceEqual(_separateItemsCollection);
        }

        [Benchmark(Description = "JSON separate items serialization (Standard library)")]
        public void BenchmarkSerializeCollectionStandard()
        {
            _ = SerializeCollectionStandard(_separateItemsCollection);
        }

        [Benchmark(Description = "JSON separate items serialization (Newtonsoft library)")]
        public void BenchmarkSerializeCollectionNewtonsoft()
        {
            _ = SerializeCollectionNewtonsoft(_separateItemsCollection);
        }

        [Benchmark(Description = "JSON separate items deserialization (Standard library)")]
        public void BenchmarkDeserializeCollectionStandard()
        {
            _ = DeserializeCollectionStandard(_serializedSeparateItems);
        }

        [Benchmark(Description = "JSON separate items deserialization (Newtonsoft library)")]
        public void BenchmarkDeserializeCollectionNewtonsoft()
        {
            _ = DeserializeCollectionNewtonsoft(_serializedSeparateItems);
        }

        [Benchmark(Description = "JSON single object serialization (Standard library)")]
        public void BenchmarkSerializeStandard()
        {
            _ = System.Text.Json.JsonSerializer.Serialize(_singleObject);
        }

        [Benchmark(Description = "JSON single object serialization (Newtonsoft library)")]
        public void BenchmarkSerializeNewtonsoft()
        {
            _ = Newtonsoft.Json.JsonConvert.SerializeObject(_singleObject);
        }

        [Benchmark(Description = "JSON single object deserialization (Standard library)")]
        public void BenchmarkDeserializeStandard()
        {
            _ = System.Text.Json.JsonSerializer.Deserialize<SalesItemContainer>(_serializedSingleObject);
        }

        [Benchmark(Description = "JSON single object deserialization (Newtonsoft library)")]
        public void BenchmarkDeserializeNewtonsoft()
        {
            _ = Newtonsoft.Json.JsonConvert.DeserializeObject<SalesItemContainer>(_serializedSingleObject);
        }

        private static IEnumerable<string> SerializeCollectionStandard<T>(IEnumerable<T> items) =>
            items.Select(i => System.Text.Json.JsonSerializer.Serialize(i)).ToArray();

        private static IEnumerable<string> SerializeCollectionNewtonsoft<T>(IEnumerable<T> items) =>
            items.Select(i => Newtonsoft.Json.JsonConvert.SerializeObject(i)).ToArray();

        private static IEnumerable<SalesItem> DeserializeCollectionStandard(IEnumerable<string> serialized) =>
            serialized.Select(i => System.Text.Json.JsonSerializer.Deserialize<SalesItem>(i)).ToArray();

        private static IEnumerable<SalesItem> DeserializeCollectionNewtonsoft(IEnumerable<string> serialized) =>
            serialized.Select(i => Newtonsoft.Json.JsonConvert.DeserializeObject<SalesItem>(i)).ToArray();
    }
}
