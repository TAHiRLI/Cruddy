using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Cruddy.Cli.Converters;

namespace Cruddy.Cli.Helpers
{
    public class JsonHelper
    {
        private static JsonSerializerOptions? _options;
        public static JsonSerializerOptions GetOptions()
        {
            if (_options == null)
            {
                _options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    Converters =
                    {
                        new TypeJsonConverter()
                    }
                };
            }

            return _options;
        }

        public static string Serialize<T>(T value)
        {
            return JsonSerializer.Serialize(value, GetOptions());
        }

        public static T? Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, GetOptions());
        }
    }
}