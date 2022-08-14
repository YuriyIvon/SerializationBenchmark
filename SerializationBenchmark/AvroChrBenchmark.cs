using BenchmarkDotNet.Attributes;
using Chr.Avro.Representation;
using Chr.Avro.Serialization;

namespace SerializationBenchmark
{
    [MemoryDiagnoser]
    public class AvroChrBenchmark
    {
        private readonly IEnumerable<SalesItem> _separateItemsCollection;
        private readonly SalesItemContainer _singleObject;
        private readonly byte[] _serializedSeparateItems;
        private readonly byte[] _serializedSingleObject;
        private readonly BinarySerializer<SalesItemContainer> _salesItemContainerSerializer;
        private readonly BinaryDeserializer<SalesItemContainer> _salesItemContainerDeserializer;
        private readonly BinarySerializer<SalesItem> _salesItemSerializer;
        private readonly BinaryDeserializer<SalesItem> _salesItemDeserializer;

        public AvroChrBenchmark()
        {
            var dataSource = new DataSource();
            _separateItemsCollection = dataSource.GetData();
            _singleObject = new SalesItemContainer
            {
                Items = _separateItemsCollection
            };

            var schemaReader = new JsonSchemaReader();
            var salesItemSchema = schemaReader.Read(File.ReadAllText("SalesItem.json"));
            var salesItemContainerSchema = schemaReader.Read(File.ReadAllText("SalesItemContainer.json"));
            _salesItemContainerSerializer = new BinarySerializerBuilder(BinarySerializerBuilder.CreateDefaultCaseBuilders())
                .BuildDelegate<SalesItemContainer>(salesItemContainerSchema);
            _salesItemContainerDeserializer = new BinaryDeserializerBuilder(BinaryDeserializerBuilder.CreateDefaultCaseBuilders())
                .BuildDelegate<SalesItemContainer>(salesItemContainerSchema);
            _salesItemSerializer = new BinarySerializerBuilder(BinarySerializerBuilder.CreateDefaultCaseBuilders())
                .BuildDelegate<SalesItem>(salesItemSchema);
            _salesItemDeserializer = new BinaryDeserializerBuilder(BinaryDeserializerBuilder.CreateDefaultCaseBuilders())
                .BuildDelegate<SalesItem>(salesItemSchema);

            _serializedSingleObject = Serialize(_singleObject);
            _serializedSeparateItems = SerializeCollection(_separateItemsCollection);

            Console.WriteLine($"Avro (Chr) separate items serialized size: {_serializedSeparateItems.Length} bytes");
            Console.WriteLine($"Avro (Chr) single object serialized size: {_serializedSingleObject.Length} bytes");
        }

        public bool Validate()
        {
            var deserializedCollection = DeserializeCollection<SalesItem>(_serializedSeparateItems);
            var deserializedSingleObject = Deserialize(_serializedSingleObject);

            return deserializedCollection.SequenceEqual(_separateItemsCollection)
                && deserializedSingleObject.Items.SequenceEqual(_singleObject.Items);
        }

        [Benchmark(Description = "Avro separate items serialization (Chr)")]
        public void BenchmarkCollectionSerialization()
        {
            _ = SerializeCollection(_separateItemsCollection);
        }

        [Benchmark(Description = "Avro single object serialization (Chr)")]
        public void BenchmarkSingleObjectSerialization()
        {
            _ = Serialize(_singleObject);
        }

        [Benchmark(Description = "Avro separate items deserialization (Chr)")]
        public void BenchmarkCollectionDeserialization()
        {
            _ = DeserializeCollection<SalesItem>(_serializedSeparateItems);
        }

        [Benchmark(Description = "Avro single object deserialization (Chr)")]
        public void BenchmarkSingleObjectDeserialization()
        {
            _ = Deserialize(_serializedSingleObject);
        }

        private byte[] SerializeCollection(IEnumerable<SalesItem> items)
        {
            using var memoryStream = new MemoryStream();
            var writer = new Chr.Avro.Serialization.BinaryWriter(memoryStream);

            foreach (var item in items)
            {
                _salesItemSerializer(item, writer);
            }

            return memoryStream.ToArray();
        }

        private IEnumerable<SalesItem> DeserializeCollection<T>(byte[] data)
        {
            var items = new List<SalesItem>();
            var reader = new Chr.Avro.Serialization.BinaryReader(data);

            while (reader.Index < data.Length)
            {
                items.Add(_salesItemDeserializer(ref reader));
            }

            return items;
        }

        private byte[] Serialize(SalesItemContainer obj)
        {
            using var memoryStream = new MemoryStream();
            var writer = new Chr.Avro.Serialization.BinaryWriter(memoryStream);
            _salesItemContainerSerializer(obj, writer);

            return memoryStream.ToArray();
        }

        private SalesItemContainer Deserialize(byte[] data)
        {
            var reader = new Chr.Avro.Serialization.BinaryReader(data);
            return _salesItemContainerDeserializer(ref reader);
        }
    }
}
