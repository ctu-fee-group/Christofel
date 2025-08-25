//
//   TestKosAtomApi.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kos.Abstractions;
using Kos.Atom;
using Kos.Data;

namespace Christofel.CtuAuth.Tests.Data.Ctu.Auth.KosSource;

/// <summary>
/// Test implementation of IKosAtomApi that uses KosApiSource data.
/// </summary>
public class TestKosAtomApi : IKosAtomApi
{
    private readonly KosApiSource _source;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestKosAtomApi"/> class.
    /// </summary>
    /// <param name="source">The KOS API source data.</param>
    public TestKosAtomApi(KosApiSource source)
    {
        _source = source;
    }

    /// <summary>
    /// Load the given loadable entry from the api.
    /// </summary>
    /// <param name="endpoint">The endpoint to be called.</param>
    /// <param name="configureRequest">Action for configuring the request.</param>
    /// <param name="token">The cancellation token for the operation.</param>
    /// <typeparam name="TContent">The type of the entity that will be loaded.</typeparam>
    /// <returns>The loaded entity. If not found, null.</returns>
    public Task<AtomEntry<TContent>?> LoadEntryAsync<TContent>
    (
        string endpoint,
        Action<AtomEntryQueryBuilder>? configureRequest = null,
        CancellationToken token = default
    )
        where TContent : class, new()
    {
        // Parse the endpoint: /{entity}/{identifier}
        var parts = endpoint.Split('/', options: StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            throw new ArgumentException($"Invalid endpoint format. Expected /{{entity}}/{{identifier}}, got: {endpoint}");
        }

        var entityType = parts[0];
        var identifier = parts[1];

        // Get the entity based on URL entity type
        object? entity = entityType.ToLowerInvariant() switch
        {
            "divisions" => _source.Faculties.TryGetValue(identifier, out var faculty) ? faculty : null,
            "programmes" => _source.Programmes.TryGetValue(identifier, out var programme) ? programme : null,
            "people" => _source.Persons.TryGetValue(identifier, out var person) ? person : null,
            "students" when int.TryParse(identifier, out var studentId) =>
                _source.Students.TryGetValue(studentId, out var student) ? student : null,
            "teachers" when int.TryParse(identifier, out var teacherId) =>
                _source.Teachers.TryGetValue(teacherId, out var teacher) ? teacher : null,
            _ => throw new ArgumentException($"Unknown entity type: {entityType}")
        };

        if (entity == null)
        {
            return Task.FromResult<AtomEntry<TContent>?>(null);
        }

        // Validate that the loaded entity matches the requested type
        if (entity is not TContent content)
        {
            throw new InvalidOperationException($"Entity type mismatch. URL requested {entityType} but TContent is {typeof(TContent).Name}. Entity is {entity.GetType().Name}");
        }

        var atomEntry = new AtomEntry<TContent>
        {
            Id = $"kos:{entityType}:{identifier}",
            Title = $"{entityType} {identifier}",
            Author = new AtomAuthor { Name = "Test KOS API" },
            Updated = DateTime.UtcNow,
            Content = content
        };

        return Task.FromResult<AtomEntry<TContent>?>(atomEntry);
    }

    /// <summary>
    /// Load the given loadable entry from the api.
    /// </summary>
    /// <param name="kosLoadable">The entity to be loaded.</param>
    /// <param name="configureRequest">Action for configuring the request.</param>
    /// <param name="token">The cancellation token for the operation.</param>
    /// <typeparam name="TContent">The type of the entity that will be loaded.</typeparam>
    /// <returns>The loaded entity. If not found, null.</returns>
    public Task<AtomEntry<TContent>?> LoadEntryAsync<TContent>
    (
        AtomLoadableEntity<TContent>? kosLoadable,
        Action<AtomEntryQueryBuilder>? configureRequest = null,
        CancellationToken token = default
    )
        where TContent : class, new()
    {
        if (kosLoadable?.Href == null)
        {
            return Task.FromResult<AtomEntry<TContent>?>(null);
        }

        return LoadEntryAsync<TContent>(kosLoadable.Href, configureRequest, token);
    }

    /// <summary>
    /// Load the feed on the given endpoint.
    /// </summary>
    /// <param name="endpoint">The endpoint to retrieve.</param>
    /// <param name="configureRequest">Action for configuring the request.</param>
    /// <param name="token">The cancellation token for the operation.</param>
    /// <typeparam name="T">The type of the entity that will be located inside of the feed.</typeparam>
    /// <returns>The loaded entity. If not found, null.</returns>
    public Task<AtomFeed<T>?> LoadFeedAsync<T>
    (
        string endpoint,
        Action<AtomFeedQueryBuilder>? configureRequest = null,
        CancellationToken token = default
    )
        where T : class, new()
    {
        // Parse endpoint to get entity type (e.g., "/divisions", "/programmes", etc.)
        var entityType = endpoint[1..].ToLowerInvariant();

        // Get all entities based on URL entity type and validate against TContent
        var entities = entityType switch
        {
            "divisions" => _source.Faculties.Select(x => (x.Key.ToString(), x.Value as T)).ToList(),
            "programmes" => _source.Programmes.Select(x => (x.Key.ToString(), x.Value as T)).ToList(),
            "people" => _source.Persons.Select(x => (x.Key.ToString(), x.Value as T)).ToList(),
            "students" => _source.Students.Select(x => (x.Key.ToString(), x.Value as T)).ToList(),
            "teachers" => _source.Teachers.Select(x => (x.Key.ToString(), x.Value as T)).ToList(),
            _ => throw new ArgumentException($"Unknown feed endpoint: {endpoint}")
        };

        // Create AtomEntry for each entity with proper IDs
        var entries = entities.Select(x =>
        {
            var (key, entity) = x;
            if (entity is null)
            {
                throw new ArgumentException($"The requested type {typeof(T)} is not type of the entities obtained!");
            }

            // Generate proper ID based on entity type and identifier
            return new AtomEntry<T>
            {
                Id = $"kos:{entityType}:{key}",
                Title = $"{entityType} {key}",
                Author = new AtomAuthor { Name = "Test KOS API" },
                Updated = DateTime.UtcNow,
                Content = entity
            };
        }).ToList();

        var feed = new AtomFeed<T>
        {
            Id = $"kos:feed:{entityType}",
            Title = $"{entityType} Feed",
            Updated = DateTime.UtcNow,
            Entries = entries,
            Next = new AtomLink { Href = endpoint }
        };

        return Task.FromResult<AtomFeed<T>?>(feed);
    }
}
