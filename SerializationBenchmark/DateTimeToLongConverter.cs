using Avro;
using Avro.Reflect;

namespace SerializationBenchmark
{
    public class DateTimeToLongConverter : IAvroFieldConverter
    {
        public object ToAvroType(object o, Schema s)
        {
            var dt = (DateTime)o;
            return dt.ToFileTimeUtc();
        }

        public object FromAvroType(object o, Schema s)
        {
            var dt = DateTime.FromFileTimeUtc((long)o);
            return dt;
        }

        public Type GetAvroType()
        {
            return typeof(long);
        }
        public Type GetPropertyType()
        {
            return typeof(DateTime);
        }
    }
}
