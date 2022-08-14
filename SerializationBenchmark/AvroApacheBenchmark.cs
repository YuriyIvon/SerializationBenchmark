using Avro;
using Avro.Reflect;
using BenchmarkDotNet.Attributes;

namespace SerializationBenchmark
{
    [MemoryDiagnoser]
    public class AvroApacheBenchmark
    {
        private readonly Schema _salesItemSchema;
        private readonly Schema _salesItemContainerSchema;
        private readonly IEnumerable<SalesItem> _separateItemsCollection;
        private readonly SalesItemContainer _singleObject;
        private readonly byte[] _serializedSeparateItems;
        private readonly byte[] _serializedSingleObject;
        private readonly ReflectWriter<SalesItem> _salesItemWriter;
        private readonly ReflectReader<SalesItem> _salesItemReader;
        private readonly ReflectWriter<SalesItemContainer> _salesItemContainerWriter;
        private readonly ReflectReader<SalesItemContainer> _salesItemContainerReader;

        public AvroApacheBenchmark()
        {
            _salesItemSchema = Schema.Parse(File.ReadAllText("SalesItemApache.json"));
            _salesItemContainerSchema = Schema.Parse(File.ReadAllText("SalesItemContainerApache.json"));

            var dataSource = new DataSource();
            _separateItemsCollection = dataSource.GetData();
            _singleObject = new SalesItemContainer
            {
                Items = _separateItemsCollection
            };

            var cache = new ClassCache();
            cache.LoadClassCache(typeof(SalesItem), _salesItemSchema);
            cache.LoadClassCache(typeof(SalesItemContainer), _salesItemContainerSchema);
            _salesItemWriter = new ReflectWriter<SalesItem>(_salesItemSchema, cache);
            _salesItemReader = new ReflectReader<SalesItem>(_salesItemSchema, _salesItemSchema, cache);
            _salesItemContainerWriter = new ReflectWriter<SalesItemContainer>(_salesItemContainerSchema, cache);
            _salesItemContainerReader = new ReflectReader<SalesItemContainer>(_salesItemContainerSchema, _salesItemContainerSchema, cache);

            _serializedSeparateItems = SerializeCollection(_separateItemsCollection, _salesItemWriter);
            _serializedSingleObject = Serialize(_singleObject, _salesItemContainerWriter);

            Console.WriteLine($"Avro (Apache) separate items serialized size: {_serializedSeparateItems.Length} bytes");
            Console.WriteLine($"Avro (Apache) single object serialized size: {_serializedSingleObject.Length} bytes");
        }

        public bool Validate()
        {
            var deserializedCollection = DeserializeCollection(_serializedSeparateItems, _salesItemReader);
            var deserializedSingleObject = Deserialize(_serializedSingleObject, _salesItemContainerReader);

            return deserializedCollection.SequenceEqual(_separateItemsCollection)
                && deserializedSingleObject.Items.SequenceEqual(_singleObject.Items);
        }

        [Benchmark(Description = "Avro separate items serialization (Apache)")]
        public void BenchmarkCollectionSerialization()
        {
            _ = SerializeCollection(_separateItemsCollection, _salesItemWriter);
        }

        [Benchmark(Description = "Avro single object serialization (Apache)")]
        public void BenchmarkSingleObjectSerialization()
        {
            _ = Serialize(_singleObject, _salesItemContainerWriter);
        }

        [Benchmark(Description = "Avro separate items deserialization (Apache)")]
        public void BenchmarkCollectionDeserialization()
        {
            _ = DeserializeCollection(_serializedSeparateItems, _salesItemReader);
        }

        [Benchmark(Description = "Avro single object deserialization (Apache)")]
        public void BenchmarkSingleObjectDeserialization()
        {
            _ = Deserialize(_serializedSingleObject, _salesItemContainerReader);
        }

        private static byte[] Serialize<T>(T obj, ReflectWriter<T> writer)
        {
            using var ms = new MemoryStream();
            var encoder = new Avro.IO.BinaryEncoder(ms);
            writer.Write(obj, encoder);

            return ms.ToArray();
        }

        private static byte[] SerializeCollection<T>(IEnumerable<T> items, ReflectWriter<T> writer)
        {
            using var ms = new MemoryStream();
            var encoder = new Avro.IO.BinaryEncoder(ms);
            foreach (var item in items)
            {
                writer.Write(item, encoder);
            }

            return ms.ToArray();
        }

        private static T Deserialize<T>(byte[] data, ReflectReader<T> reader)
        {
            using var ms = new MemoryStream(data);
            var decoder = new Avro.IO.BinaryDecoder(ms);
            return reader.Read(decoder);
        }

        private static IEnumerable<T> DeserializeCollection<T>(byte[] data, ReflectReader<T> reader)
        {
            var items = new List<T>();

            using var ms = new MemoryStream(data);
            var decoder = new Avro.IO.BinaryDecoder(ms);
            while(ms.Position < ms.Length)
            {
                items.Add(reader.Read(decoder));
            }

            return items;
        }
    }
}
