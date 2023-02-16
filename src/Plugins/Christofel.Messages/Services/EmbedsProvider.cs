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
    /// <summary>
    /// Service for providing embeds from json files or json strings.
    /// </summary>
    public class EmbedsProvider : IDisposable, IAsyncDisposable
    {
        private readonly IDisposable? _embedsOptionsUpdateToken;
        private readonly JsonSerializerOptions _jsonOptions;
        private EmbedsOptions _embedsOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmbedsProvider"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="jsonOptions">The serializer options.</param>
        public EmbedsProvider(IOptionsMonitor<EmbedsOptions> options, IOptionsSnapshot<JsonSerializerOptions> jsonOptions)
        {
            _embedsOptionsUpdateToken = options.OnChange(c => _embedsOptions = c);
            _embedsOptions = options.CurrentValue;
            _jsonOptions = jsonOptions.Get("Discord");
        }

        /// <summary>
        /// Gets folder where the embeds are located.
        /// </summary>
        public string EmbedsFolder => _embedsOptions.Folder;

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            _embedsOptionsUpdateToken?.Dispose();
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _embedsOptionsUpdateToken?.Dispose();
        }

        /// <summary>
        /// Tries to parse Embed out of given string.
        /// </summary>
        /// <param name="embedString">The string to be parsed.</param>
        /// <returns>Parsed embed, if parsing was successful. Null otherwise.</returns>
        public Embed? GetEmbedFromString
            (string embedString) => (Embed?)JsonSerializer.Deserialize<IEmbed>(embedString, _jsonOptions);

        /// <summary>
        /// Tries to load embed from the given file name.
        /// </summary>
        /// <remarks>
        /// The file will be search for in <see cref="EmbedsFolder"/>.
        ///
        /// Json extension will be added automatically.
        /// </remarks>
        /// <param name="embedName">The name of the embed.</param>
        /// <returns>Parsed embed, if parsing was successful. Null otherwise.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the file name cannot be accepted.</exception>
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