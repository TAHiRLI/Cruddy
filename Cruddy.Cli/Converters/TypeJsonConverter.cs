using System.Text.Json.Serialization;
using System.Text.Json;

namespace Cruddy.Cli.Converters
{
    public class TypeJsonConverter : JsonConverter<Type>
    {
        public override Type? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var typeName = reader.GetString();
            if (string.IsNullOrEmpty(typeName))
                return null;

            return Type.GetType(typeName);
        }

        public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.FullName);
        }
    }
}