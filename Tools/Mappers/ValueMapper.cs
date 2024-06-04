namespace NITHlibrary.Tools.Mappers
{
    public class ValueMapperLong
    {
        public long BaseMax { get; set; } = 0;
        public long DestMax { get; set; } = 0;

        public long Map(long value)
        {
            return (long)(value / (double)DestMax * BaseMax);
        }

        public static long Map(long value, long baseMax, long destMax)
        {
            return (long)(value / (double)destMax * baseMax);
        }

        public ValueMapperLong()
        {
        }

        public ValueMapperLong(long baseMax, long destMax)
        {
            DestMax = destMax;
            BaseMax = baseMax;
        }

    }

    public class ValueMapperDouble
    {
        public double BaseMax { get; set; } = 0;
        public double DestMax { get; set; } = 0;

        public double Map(double value)
        {
            return value / BaseMax * DestMax;
        }

        public static double Map(double value, double baseMax, double destMax)
        {
            return value / baseMax * destMax;
        }

        public ValueMapperDouble()
        {
        }

        public ValueMapperDouble(double baseMax, double destMax)
        {
            DestMax = destMax;
            BaseMax = baseMax;
        }
    }
}
