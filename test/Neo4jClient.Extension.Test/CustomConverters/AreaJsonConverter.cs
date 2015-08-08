using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnitsNet;

namespace Neo4jClient.Extension.Test.CustomConverters
{
    /// <summary>
    /// http://stackoverflow.com/questions/27063475/custom-jsonconverter-that-can-convert-decimal-minvalue-to-empty-string-and-back
    /// </summary>
    public class AreaJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(Area?) || objectType == typeof(Area));
        }

        public override object ReadJson(JsonReader reader, Type objectType,
                                        object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                if ((string)reader.Value == string.Empty)
                {
                    return null;
                }
            }
            else if (reader.TokenType == JsonToken.Float ||
                     reader.TokenType == JsonToken.Integer)
            {
                return Area.FromSquareMeters((double) reader.Value);
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, object value,
                                       JsonSerializer serializer)
        {
            var area = (Area?)value;
            if (!area.HasValue)
            {
                writer.WriteValue((float?) null);
            }
            else
            {
                writer.WriteValue(area.Value.SquareMeters);
            }
        }
    }
}
