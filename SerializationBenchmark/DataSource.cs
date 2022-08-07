using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace SerializationBenchmark
{
    public class DataSource
    {
        public IEnumerable<SalesItem> GetData()
        {
            var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture);
            using var streamReader = new StreamReader(@"1000 Sales Records.csv");
            using var csvReader = new CsvReader(streamReader, csvConfiguration);

            csvReader.Read();
            csvReader.ReadHeader();

            var items = new List<SalesItem>();

            for (int i = 0; csvReader.Read() && i < 1000; i++)
            {
                items.Add(new SalesItem
                {
                    Country = csvReader.GetField<string>("Country"),
                    ItemType = csvReader.GetField<string>("ItemType"),
                    OrderDate = csvReader.GetField<DateTime>("OrderDate").ToUniversalTime(),
                    OrderId = csvReader.GetField<int>("OrderID"),
                    OrderPriority = csvReader.GetField<string>("OrderPriority"),
                    Region = csvReader.GetField<string>("Region"),
                    SalesChannel = csvReader.GetField<string>("SalesChannel"),
                    ShipDate = csvReader.GetField<DateTime>("ShipDate").ToUniversalTime(),
                    TotalCost = csvReader.GetField<double>("TotalCost"),
                    TotalProfit = csvReader.GetField<double>("TotalProfit"),
                    TotalRevenue = csvReader.GetField<double>("TotalRevenue"),
                    UnitCost = csvReader.GetField<double>("UnitCost"),
                    UnitPrice = csvReader.GetField<double>("UnitPrice"),
                    UnitsSold = csvReader.GetField<int>("UnitsSold")
                });
            }

            return items;
        }
    }
}
