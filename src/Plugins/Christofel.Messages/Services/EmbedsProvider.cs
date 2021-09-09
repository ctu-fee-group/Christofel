//
//   EmbedsProvider.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Christofel.Messages.Options;
using Microsoft.Extensions.Options;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;

namespace Christofel.Messages.Services
{
    public class EmbedsProvider : IDisposable, IAsyncDisposable
    {
        private readonly IDisposable _embedsOptionsUpdateToken;
        private readonly JsonSerializerOptions _jsonOptions;
        private EmbedsOptions _embedsOptions;

        public EmbedsProvider(IOptionsMonitor<EmbedsOptions> options, IOptions<JsonSerializerOptions> jsonOptions)
        {
            _embedsOptionsUpdateToken = options.OnChange(c => _embedsOptions = c);
            _embedsOptions = options.CurrentValue;
            _jsonOptions = jsonOptions.Value;
        }

        public string EmbedsFolder => _embedsOptions.Folder;

        public ValueTask DisposeAsync()
        {
            _embedsOptionsUpdateToken.Dispose();
            return ValueTask.CompletedTask;
        }

        public void Dispose()
        {
            _embedsOptionsUpdateToken.Dispose();
        }

        public Embed? GetEmbedFromString
            (string embedString) => (Embed?) JsonSerializer.Deserialize<IEmbed>(embedString, _jsonOptions);

        public async Task<Embed?> GetEmbedFromFile(string embedName)
        {
            if (!Regex.IsMatch(embedName, @"^[a-zA-Z_\-\.]+$"))
            {
                throw new InvalidOperationException("File name cannot be accepted");
            }

            return GetEmbedFromString(await File.ReadAllTextAsync(Path.Join(EmbedsFolder, embedName + ".json")));
        }
    }
}