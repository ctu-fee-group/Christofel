//
//   DummyError.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Results;

namespace Christofel.CtuAuth.Tests.Data.Ctu.Auth;

/// <summary>
/// A dummy error for testing purposes.
/// </summary>
/// <param name="Dummy">The dummy string to save and compare.</param>
public record DummyError(string Dummy)
    : ResultError("I am a dummy, {Dummy}");
