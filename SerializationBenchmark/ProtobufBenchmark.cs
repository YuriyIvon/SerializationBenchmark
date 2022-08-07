using BenchmarkDotNet.Attributes;

namespace SerializationBenchmark
{
    [MemoryDiagnoser]
    public class ProtobufBenchmark
    {
        private readonly IEnumerable<SalesItem> _separateItemsCollection;
        private readonly SalesItemContainer _singleObject;
        private readonly byte[] _serializedSeparateItems;
        private readonly byte[] _serializedSingleObject;

        public ProtobufBenchmark()
        {
            var dataSource = new DataSource();
            _separateItemsCollection = dataSource.GetData();
            _singleObject = new SalesItemContainer
            {
                Items = _separateItemsCollection
            };

            _serializedSeparateItems = SerializeCollection(_separateItemsCollection);
            _serializedSingleObject = Serialize(_singleObject);

            Console.WriteLine($"Protobuf separate items serialized size: {_serializedSeparateItems.Length} bytes");
            Console.WriteLine($"Protobuf single object serialized size: {_serializedSingleObject.Length} bytes");
        }

        public bool Validate()
        {
            var deserializedCollection = DeserializeCollection<SalesItem>(_serializedSeparateItems);
            var deserializedSingleObject = Deserialize<SalesItemContainer>(_serializedSingleObject);

            return deserializedCollection.SequenceEqual(_separateItemsCollection)
                && deserializedSingleObject.Items.SequenceEqual(_singleObject.Items);
        }

        [Benchmark(Description = "Protobuf single object serialization")]
        public void BenchmarkSerialize()
        {
            _ = Serialize(_singleObject);
        }

        [Benchmark(Description = "Protobuf single object deserialization")]
        public void BenchmarkDeserialize()
        {
            _ = Deserialize<SalesItemContainer>(_serializedSingleObject);
        }

        [Benchmark(Description = "Protobuf separate items serialization")]
        public void BenchmarkSerializeCollection()
        {
            _ = SerializeCollection(_separateItemsCollection);
        }

        [Benchmark(Description = "Protobuf separate items deserialization")]
        public void BenchmarkDeserializeCollection()
        {
            _ = DeserializeCollection<SalesItem>(_serializedSeparateItems);
        }

        private static byte[] Serialize<T>(T obj)
        {
            using var ms = new MemoryStream();
            ProtoBuf.Serializer.Serialize(ms, obj);

            return ms.ToArray();
        }

        private static T Deserialize<T>(byte[] data)
        {
            using var ms = new MemoryStream(data);
            return ProtoBuf.Serializer.Deserialize<T>(ms);
        }

        private static byte[] SerializeCollection<T>(IEnumerable<T> items)
        {
            using var ms = new MemoryStream();
            foreach (var item in items)
            {
                ProtoBuf.Serializer.SerializeWithLengthPrefix(ms, item, ProtoBuf.PrefixStyle.Base128);
            }

            return ms.ToArray();
        }

        private static IEnumerable<T> DeserializeCollection<T>(byte[] data)
        {
            var items = new List<T>();

            using var ms = new MemoryStream(data);
            while(ms.Position < ms.Length)
            {
                items.Add(ProtoBuf.Serializer.DeserializeWithLengthPrefix<T>(ms, ProtoBuf.PrefixStyle.Base128));
            }

            return items;
        }
    }
}
