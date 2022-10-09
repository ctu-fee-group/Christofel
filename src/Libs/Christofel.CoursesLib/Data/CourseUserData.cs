//
//  CourseUserData.cs
//
//  Copyright (c) Christofel authors. All rights reserved.
//  Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Christofel.CoursesLib.Database;

namespace Christofel.CoursesLib.Data;

public record CourseUserData(CourseAssignment Course, bool IsMember);