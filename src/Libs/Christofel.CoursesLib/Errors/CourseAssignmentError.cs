//
//  CourseAssignmentError.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Remora.Results;

namespace Christofel.CoursesLib.Errors;

public record CourseAssignmentError(string Message) : ResultError(Message);