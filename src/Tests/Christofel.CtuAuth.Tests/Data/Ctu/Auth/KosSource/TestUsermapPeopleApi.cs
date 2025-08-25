//
//   TestUsermapPeopleApi.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Usermap.Controllers;
using Usermap.Data;

namespace Christofel.CtuAuth.Tests.Data.Ctu.Auth.KosSource;

/// <summary>
/// Test implementation of IUsermapPeopleApi that uses a dictionary data source.
/// </summary>
public class TestUsermapPeopleApi : IUsermapPeopleApi
{
    private readonly Dictionary<string, UsermapPerson> _usermapSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestUsermapPeopleApi"/> class.
    /// </summary>
    /// <param name="usermapSource">The usermap source dictionary.</param>
    public TestUsermapPeopleApi(Dictionary<string, UsermapPerson> usermapSource)
    {
        _usermapSource = usermapSource;
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<UsermapPerson>> GetPeopleAsync(string? query, string? orderBy, uint limit, uint offset, CancellationToken token)
    {
        var results = _usermapSource.Values.AsEnumerable();

        if (!string.IsNullOrEmpty(query))
        {
            results = results.Where(p =>
                p.Username.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.FirstName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.LastName.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        results = results.Skip((int)offset).Take((int)limit);
        return Task.FromResult<IReadOnlyList<UsermapPerson>>(results.ToList());
    }

    /// <summary>
    /// Gets a specific person by username.
    /// </summary>
    /// <param name="username">The username to search for.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The usermap person if found, otherwise null.</returns>
    public Task<UsermapPerson?> GetPersonAsync(string username, CancellationToken ct = default)
    {
        _usermapSource.TryGetValue(username, out var person);
        return Task.FromResult(person);
    }

    /// <summary>
    /// Gets a person's photo.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The person's photo data.</returns>
    /// <exception cref="NotImplementedException">This method is not implemented in the test API.</exception>
    public Task<Image?> GetPersonPhotoAsync(string username, CancellationToken ct = default)
    {
        throw new NotImplementedException("GetPersonPhotoAsync is not implemented in the test API.");
    }

    /// <summary>
    /// Checks if a person has specific roles.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="roles">The roles to check for inclusion.</param>
    /// <param name="excludeRoles">The roles to check for exclusion.</param>
    /// <param name="departments">The departments to check.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>True if the person matches the role criteria.</returns>
    /// <exception cref="NotImplementedException">This method is not implemented in the test API.</exception>
    public Task<bool> CheckRolesAsync(string username, IEnumerable<string>? roles = null, IEnumerable<string>? excludeRoles = null, IEnumerable<string>? departments = null, CancellationToken ct = default)
    {
        throw new NotImplementedException("CheckRolesAsync is not implemented in the test API.");
    }
}
