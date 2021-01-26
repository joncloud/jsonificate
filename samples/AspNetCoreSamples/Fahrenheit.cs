namespace AspNetCoreSamples
{
    public class Fahrenheit
    {
        public double Temperature { get; set; }

        public double ToCelsius() =>
            (Temperature - 32.0) * 5.0 / 9.0;
    }
}
