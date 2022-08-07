using BenchmarkDotNet.Attributes;

namespace SerializationBenchmark
{
    [MemoryDiagnoser]
    public class BsonBenchmark
    {
        private readonly IEnumerable<SalesItem> _separateItemsCollection;
        private readonly SalesItemContainer _singleObject;
        private readonly byte[] _serializedSeparateItems;
        private readonly byte[] _serializedSingleObject;

        public BsonBenchmark()
        {
            var dataSource = new DataSource();
            _separateItemsCollection = dataSource.GetData();
            _singleObject = new SalesItemContainer
            {
                Items = _separateItemsCollection
            };

            _serializedSeparateItems = SerializeCollection(_separateItemsCollection);
            _serializedSingleObject = Serialize(_singleObject);

            Console.WriteLine($"BSON separate items serialized size: {_serializedSeparateItems.Length} bytes");
            Console.WriteLine($"BSON single object serialized size: {_serializedSingleObject.Length} bytes");
        }

        public bool Validate()
        {
            var deserializedCollection = DeserializeCollection<SalesItem>(_serializedSeparateItems);
            var deserializedSingleObject = Deserialize<SalesItemContainer>(_serializedSingleObject);

            return deserializedCollection.SequenceEqual(_separateItemsCollection)
                && deserializedSingleObject.Items.SequenceEqual(_singleObject.Items);
        }

        [Benchmark(Description = "BSON separate items serialization")]
        public void BenchmarkCollectionSerialization()
        {
            _ = SerializeCollection(_separateItemsCollection);
        }

        [Benchmark(Description = "BSON separate items deserialization")]
        public void BenchmarkCollectionDeserialization()
        {
            _ = DeserializeCollection<SalesItem>(_serializedSeparateItems);
        }

        [Benchmark(Description = "BSON single object serialization")]
        public void BenchmarkSingleObjectSerialization()
        {
            _ = Serialize(_singleObject);
        }

        [Benchmark(Description = "BSON single object deserialization")]
        public void BenchmarkSingleObjectDeserialization()
        {
            _ = Deserialize<SalesItemContainer>(_serializedSingleObject);
        }

        private static byte[] SerializeCollection<T>(IEnumerable<T> items)
        {
            var ms = new MemoryStream();
            var writer = new MongoDB.Bson.IO.BsonBinaryWriter(ms);
            foreach (var item in items)
            {
                MongoDB.Bson.Serialization.BsonSerializer.Serialize(writer, item);
            }

            return ms.ToArray();
        }

        private static byte[] Serialize<T>(T obj)
        {
            var ms = new MemoryStream();
            var writer = new MongoDB.Bson.IO.BsonBinaryWriter(ms);
            MongoDB.Bson.Serialization.BsonSerializer.Serialize(writer, obj);

            return ms.ToArray();
        }

        private static IEnumerable<T> DeserializeCollection<T>(byte[] data)
        {
            var items = new List<T>();

            using var ms = new MemoryStream(data);
            var reader = new MongoDB.Bson.IO.BsonBinaryReader(ms);
            while (ms.Position < ms.Length)
            {
                items.Add(MongoDB.Bson.Serialization.BsonSerializer.Deserialize<T>(reader));
            }

            return items;
        }

        private static T Deserialize<T>(byte[] data)
        {
            using var ms = new MemoryStream(data);
            var reader = new MongoDB.Bson.IO.BsonBinaryReader(ms);
            return MongoDB.Bson.Serialization.BsonSerializer.Deserialize<T>(reader);
        }
    }
}
