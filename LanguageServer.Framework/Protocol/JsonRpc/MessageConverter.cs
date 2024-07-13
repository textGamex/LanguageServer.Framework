﻿using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.JsonRpc;

public class MessageConverter : JsonConverter<Message>
{
    public override Message Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var root = jsonDoc.RootElement;
        var paramsJson = root.GetProperty("params").GetRawText();
        if (root.TryGetProperty("method", out var methodElement) && methodElement.GetString() is { } method)
        {
            if (root.TryGetProperty("id", out var id))
            {
                if (id.ValueKind == JsonValueKind.Number)
                {
                    return new RequestMessage(id.GetInt32(), method, JsonDocument.Parse(paramsJson));
                }
                else if (id.ValueKind == JsonValueKind.String)
                {
                    return new RequestMessage(id.GetString()!, method, JsonDocument.Parse(paramsJson));
                }
            }
            else
            {
                return new NotificationMessage(method, JsonDocument.Parse(paramsJson));
            }
        }

        // Fallback to default deserialization for other methods
        // This part needs to be adjusted based on your specific needs and structure
        return JsonSerializer.Deserialize<Message>(root.GetRawText(), options)!;
    }

    public override void Write(Utf8JsonWriter writer, Message value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}