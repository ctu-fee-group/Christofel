//
//   DateTimeBaseProvider.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Christofel.Helpers.Date;

/// <summary>
/// Default DateTimeBaseProvidier, using DateTime.
/// </summary>
public class DateTimeBaseProvider : IDateTimeBaseProvider
{
    /// <inheritdoc/>
    public DateTime UtcNow => DateTime.UtcNow;

    /// <inheritdoc/>
    public DateTime SystemNow => DateTime.Now;
}
