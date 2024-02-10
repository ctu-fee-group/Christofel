//
//  DuplicateError.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Results;

namespace Christofel.CtuAuth.Errors;

public record DuplicateError() : ResultError("Detected duplicate user.");