namespace AspNetCoreSamples
{
    public class Celsius
    {
        public double Temperature { get; set; }

        public double ToFahrenheit() =>
            (Temperature * 9.0 / 5.0) + 32.0;
    }
}
