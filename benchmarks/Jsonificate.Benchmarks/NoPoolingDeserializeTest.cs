using System.Text.Json;

namespace Jsonificate.Benchmarks
{
    class NoPoolingDeserializeTest : IDeserializeTest
    {
        readonly string _simpleJson;
        readonly string _complexJson;
        readonly JsonSerializerOptions _options;

        public NoPoolingDeserializeTest(string simpleJson, string complexJson)
        {
            _simpleJson = simpleJson;
            _complexJson = complexJson;
            _options = new JsonSerializerOptions();
        }

        public int Complex()
        {
            var result = JsonSerializer.Deserialize<Complex>(_complexJson, _options);
            var num = 0;
            unchecked
            {
                num += result.One.Int32;
                num += result.Two.Int32;
                num += result.Three.Int32;
            }
            return num;
        }

        public int Simple()
        {
            var result = JsonSerializer.Deserialize<Simple>(_simpleJson, _options);
            var num = result.Int32;
            return num;
        }
    }
}
