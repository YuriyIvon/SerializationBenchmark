using Avro.Reflect;
using ProtoBuf;

namespace SerializationBenchmark
{
    [ProtoContract]
    public record SalesItem
    {
        [ProtoMember(1)]
        [AvroField("Region")]
        public string Region { get; set; }

        [ProtoMember(2)]
        [AvroField("Country")]
        public string Country { get; set; }

        [ProtoMember(3)]
        [AvroField("ItemType")]
        public string ItemType { get; set; }

        [ProtoMember(4)]
        [AvroField("SalesChannel")]
        public string SalesChannel { get; set; }

        [ProtoMember(5)]
        [AvroField("OrderPriority")]
        public string OrderPriority { get; set; }

        [ProtoMember(6)]
        [AvroField("OrderDate", typeof(DateTimeToLongConverter))]
        public DateTime OrderDate { get; set; }

        [ProtoMember(7)]
        [AvroField("OrderID")]
        public int OrderId { get; set; }

        [ProtoMember(8)]
        [AvroField("ShipDate", typeof(DateTimeToLongConverter))]
        public DateTime ShipDate { get; set; }

        [ProtoMember(9)]
        [AvroField("UnitsSold")]
        public int UnitsSold { get; set; }

        [ProtoMember(10)]
        [AvroField("UnitPrice")]
        public double UnitPrice { get; set; }

        [ProtoMember(11)]
        [AvroField("UnitCost")]
        public double UnitCost { get; set; }

        [ProtoMember(12)]
        [AvroField("TotalRevenue")]
        public double TotalRevenue { get; set; }

        [ProtoMember(13)]
        [AvroField("TotalCost")]
        public double TotalCost { get; set; }

        [ProtoMember(14)]
        [AvroField("TotalProfit")]
        public double TotalProfit { get; set; }
    }
}
