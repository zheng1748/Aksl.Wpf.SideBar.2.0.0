using Aksl.Dialogs.Services;
using Microsoft.Extensions.DependencyInjection;

using System.IO;
using System.Text.Json;

namespace Aksl.Infrastructure;

public static class JsonSerializerHelper
{
    public static async Task<string> SerializeStringAsync<TValue>(TValue value)
    {
        string json = default;

        await using MemoryStream stream = new();
        await JsonSerializer.SerializeAsync(stream, value);
        stream.Position = 0;
        using StreamReader reader = new(stream);
        json = await reader.ReadToEndAsync();

        return json;
    }
}

