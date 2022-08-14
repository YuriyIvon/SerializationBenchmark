using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Perfolizer.Horology;
using SerializationBenchmark;
using System.Globalization;
using System.Reflection;

var jsonBenchmark = new JsonBenchmark();
if (!jsonBenchmark.Validate())
{
    throw new Exception("Serialization correctness check failed for JSON");
}

var bsonBenchmark = new BsonBenchmark();
if (!bsonBenchmark.Validate())
{
    throw new Exception("Serialization correctness check failed for BSON");
}

var avroBenchmark = new AvroApacheBenchmark();
if (!avroBenchmark.Validate())
{
    throw new Exception("Serialization correctness check failed for Apache Avro");
}

var avroChrBenchmark = new AvroChrBenchmark();
if (!avroChrBenchmark.Validate())
{
    throw new Exception("Serialization correctness check failed for Chr Avro");
}

var protobufBenchmark = new ProtobufBenchmark();
if (!protobufBenchmark.Validate())
{
    throw new Exception("Serialization correctness check failed for Protobuf");
}

var config = ManualConfig
    .Create(DefaultConfig.Instance)
    .WithOptions(ConfigOptions.JoinSummary)
    .WithSummaryStyle(new SummaryStyle(
        CultureInfo.CurrentCulture,
        true,
        SizeUnit.KB,
        TimeUnit.Microsecond,
        printUnitsInContent: false));

BenchmarkRunner.Run(Assembly.GetExecutingAssembly(), config);
