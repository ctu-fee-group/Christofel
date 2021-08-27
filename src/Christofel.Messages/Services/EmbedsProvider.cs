using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Christofel.Messages.Options;
using Microsoft.Extensions.Options;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Christofel.Messages.Services
{
    public class EmbedsProvider : IDisposable, IAsyncDisposable
    {
        private EmbedsOptions _embedsOptions;
        private readonly IDisposable _embedsOptionsUpdateToken;
        private readonly JsonSerializerOptions _jsonOptions;

        public EmbedsProvider(IOptionsMonitor<EmbedsOptions> options, IOptions<JsonSerializerOptions> jsonOptions)
        {
            _embedsOptionsUpdateToken = options.OnChange(c => _embedsOptions = c);
            _embedsOptions = options.CurrentValue;
            _jsonOptions = jsonOptions.Value;
        }

        public string EmbedsFolder => _embedsOptions.Folder;

        public Embed? GetEmbedFromString(string embedString)
        {
            return (Embed?)JsonSerializer.Deserialize<IEmbed>(embedString, _jsonOptions);
        }

        public async Task<Embed?> GetEmbedFromFile(string embedName)
        {
            if (!Regex.IsMatch(embedName, @"^[a-zA-Z_\-\.]+$"))
            {
                throw new InvalidOperationException("File name cannot be accepted");
            }

            return GetEmbedFromString(await File.ReadAllTextAsync(Path.Join(EmbedsFolder, embedName + ".json")));
        }

        public void Dispose()
        {
            _embedsOptionsUpdateToken.Dispose();
        }

        public ValueTask DisposeAsync()
        {
            _embedsOptionsUpdateToken.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}