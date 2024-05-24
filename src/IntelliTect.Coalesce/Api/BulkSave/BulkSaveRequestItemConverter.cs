using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IntelliTect.Coalesce.Api
{
    internal class BulkSaveRequestItemConverter : JsonConverter<BulkSaveRequestItem>
    {
        // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/converters-how-to?pivots=dotnet-7-0#support-polymorphic-deserialization

        public override BulkSaveRequestItem Read(
            ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException();

            // Read ahead in the object to find the type, since the type name might not
            // occur before the other properties but we need the type name before we can do anything else.
            var typeReader = reader;
            string? typeName = null;
            while (typeReader.Read() && typeName is null)
            {
                if (typeReader.TokenType == JsonTokenType.EndObject)
                    throw new JsonException("Type missing.");
                else if (typeReader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException();

                var propertyName = typeReader.GetString();
                typeReader.Read();
                switch (propertyName?.ToLowerInvariant())
                {
                    case "type":
                        typeName = typeReader.GetString();
                        break;
                    default:
                        typeReader.Skip();
                        break;
                }
            }

            if (typeName is null) throw new JsonException($"Property 'type' is required on bulk save item.");

            var type = ReflectionRepository.Global.ClientTypesLookup[typeName];
            if (type is null || !type.IsDbMappedType)
            {
                throw new JsonException($"Unknown type '{typeName}'");
            }

            var entityTypeViewModel = type.BaseViewModel;

            var dtoClassViewModel = (type.IsCustomDto
                ? type
                : ReflectionRepository.Global.GeneratedParameterDtos[entityTypeViewModel])
                ?? throw new JsonException($"Cannot construct '{type.ParameterDtoTypeName}'");

            BulkSaveRequestItem ret = (BulkSaveRequestItem)Activator.CreateInstance(
                typeof(BulkSaveRequestItem<,>).MakeGenericType(
                    entityTypeViewModel.Type.TypeInfo,
                    dtoClassViewModel.Type.TypeInfo
                ))!;

            ret.Type = typeName;
            ret.ClassViewModel = entityTypeViewModel;
            ret.ParamDtoClassViewModel = dtoClassViewModel;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return ret;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString();
                    reader.Read();
                    switch (propertyName?.ToLowerInvariant())
                    {
                        case "data":
                            ret.Data = JsonSerializer.Deserialize(ref reader, dtoClassViewModel.Type.TypeInfo, options)
                                ?? throw new JsonException("Required property 'data' is missing");
                            break;
                        case "refs":
                            ret.Refs = JsonSerializer.Deserialize<Dictionary<string, int>>(ref reader, options);
                            break;
                        case "action":
                            ret.Action = reader.GetString() ?? throw new JsonException("Required property 'action' is missing");
                            break;
                        case "root":
                            ret.Root = reader.GetBoolean();
                            break;
                    }
                }
            }

            return ret;
        }

        public override void Write(
            Utf8JsonWriter writer,
            BulkSaveRequestItem dto,
            JsonSerializerOptions options)
            => throw new NotSupportedException();
    }
}
