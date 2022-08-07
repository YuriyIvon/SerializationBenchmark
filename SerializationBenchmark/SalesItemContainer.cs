using Avro.Reflect;
using ProtoBuf;

namespace SerializationBenchmark
{
    [ProtoContract]
    public record SalesItemContainer
    {
        [ProtoMember(1)]
        [AvroField("Items")]
        public IEnumerable<SalesItem> Items { get; set; }
    }
}
