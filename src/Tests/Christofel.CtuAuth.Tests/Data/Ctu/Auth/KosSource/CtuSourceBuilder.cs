//
//   CtuSourceBuilder.cs
//
//   Copyright (c) Christofel authors. All rights reserved.
//   Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Kos.Atom;
using Kos.Data;
using Usermap.Data;

namespace Christofel.CtuAuth.Tests.Data.Ctu.Auth.KosSource;

/// <summary>
/// Builder for creating CTU test data sources.
/// </summary>
public class CtuSourceBuilder : ICtuSourceBuilder
{
    private readonly Dictionary<string, Division> _faculties = new();
    private readonly Dictionary<string, Programme> _programmes = new();
    private readonly Dictionary<string, Person> _persons = new();
    private readonly Dictionary<string, bool> _personInKosApi = new();
    private readonly Dictionary<int, Student> _students = new();
    private readonly Dictionary<int, Teacher> _teachers = new();
    private readonly Dictionary<string, List<string>> _usermapRoles = new();
    private readonly Random _rng = new();
    private int _nextStudentId = 1;
    private int _nextTeacherId = 1;

    /// <summary>
    /// Adds a faculty with the specified code.
    /// </summary>
    /// <param name="code">The faculty code.</param>
    /// <returns>A faculty builder.</returns>
    public ICtuFacultyBuilder AddFaculty(string code)
    {
        return new CtuFacultyBuilder(this, code);
    }

    /// <summary>
    /// Adds a programme with the specified code.
    /// </summary>
    /// <param name="code">The programme code.</param>
    /// <returns>A programme builder.</returns>
    public ICtuProgrammeBuilder AddProgramme(string code)
    {
        return new CtuProgrammeBuilder(this, code);
    }

    /// <summary>
    /// Adds a person with the specified username.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <returns>A user builder.</returns>
    public ICtuUserBuilder AddPerson(string username)
    {
        return new CtuUserBuilder(this, username);
    }

    /// <summary>
    /// Adds a faculty internally.
    /// </summary>
    /// <param name="code">The faculty code.</param>
    /// <param name="faculty">The faculty to add.</param>
    private void AddFacultyInternal(string code, Division faculty)
    {
        _faculties[code] = faculty;
    }

    /// <summary>
    /// Adds a programme internally.
    /// </summary>
    /// <param name="code">The programme code.</param>
    /// <param name="programme">The programme to add.</param>
    private void AddProgrammeInternal(string code, Programme programme)
    {
        _programmes[code] = programme;
    }

    /// <summary>
    /// Adds a person internally.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="person">The person to add.</param>
    /// <param name="includeInKosApi">Whether to include in kosapi. If false, include only in usermap.</param>
    private void AddPersonInternal(string username, Person person, bool includeInKosApi)
    {
        _personInKosApi[username] = includeInKosApi;
        _persons[username] = person;
    }

    /// <summary>
    /// Adds a student internally.
    /// </summary>
    /// <param name="student">The student to add.</param>
    /// <returns>The student ID.</returns>
    private int AddStudentInternal(Student student)
    {
        var id = _nextStudentId++;
        _students[id] = student;
        return id;
    }

    /// <summary>
    /// Adds a teacher internally.
    /// </summary>
    /// <param name="teacher">The teacher to add.</param>
    /// <returns>The teacher ID.</returns>
    private int AddTeacherInternal(Teacher teacher)
    {
        var id = _nextTeacherId++;
        _teachers[id] = teacher;
        return id;
    }

    /// <summary>
    /// Gets the built faculties.
    /// </summary>
    public IReadOnlyDictionary<string, Division> Faculties => _faculties;

    /// <summary>
    /// Gets the built programmes.
    /// </summary>
    public IReadOnlyDictionary<string, Programme> Programmes => _programmes;

    /// <summary>
    /// Gets the built persons.
    /// </summary>
    public IReadOnlyDictionary<string, Person> Persons => _persons;

    /// <summary>
    /// Gets the built students.
    /// </summary>
    public IReadOnlyDictionary<int, Student> Students => _students;

    /// <summary>
    /// Gets the built teachers.
    /// </summary>
    public IReadOnlyDictionary<int, Teacher> Teachers => _teachers;

    /// <summary>
    /// Adds usermap roles for a username internally.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="roles">The usermap roles.</param>
    private void AddUsermapRolesInternal(string username, List<string> roles)
    {
        _usermapRoles[username] = new List<string>(roles);
    }

    /// <summary>
    /// Builds the KOS API source from the configured data.
    /// </summary>
    /// <returns>A KOS API source containing all the built data.</returns>
    public KosApiSource BuildKosApiSource()
    {
        var people = _persons.Where(x => _personInKosApi[x.Key]).ToDictionary();

        return new KosApiSource(
            Faculties: new Dictionary<string, Division>(_faculties),
            Programmes: new Dictionary<string, Programme>(_programmes),
            Persons: people,
            Students: new Dictionary<int, Student>(_students),
            Teachers: new Dictionary<int, Teacher>(_teachers)
        );
    }

    /// <summary>
    /// Builds the Usermap API source from the configured data.
    /// </summary>
    /// <returns>A dictionary of usernames to usermap persons.</returns>
    public Dictionary<string, UsermapPerson> BuildUsermapApiSource()
    {
        var result = new Dictionary<string, UsermapPerson>();

        foreach (var person in _persons.Values)
        {
            var usermapRoles = _usermapRoles.TryGetValue(person.Username, out var roles) ? roles : new List<string>();

            // Use original titles for UserMap API (always include, regardless of KOS API inclusion flags)
            var preTitlesStr = person.TitlesPre != null ? string.Join(" ", person.TitlesPre) : string.Empty;
            var postTitlesStr = person.TitlesPost != null ? string.Join(" ", person.TitlesPost) : string.Empty;

            var usermapPerson = new UsermapPerson(
                person.Username,
                (ulong)_rng.Next(100000, 999999),
                person.FirstName,
                person.LastName,
                $"{preTitlesStr} {person.FirstName} {person.LastName} {postTitlesStr}".Trim(),
                new List<string>(),
                string.Empty,
                new List<UsermapDepartment>(),
                new List<string>(),
                new List<string>(),
                new List<string>(usermapRoles)
            );

            result[person.Username] = usermapPerson;
        }

        return result;
    }

    /// <summary>
    /// Internal implementation of faculty builder.
    /// </summary>
    private class CtuFacultyBuilder : ICtuFacultyBuilder
    {
        private readonly CtuSourceBuilder _sourceBuilder;
        private readonly string _code;
        private string? _name;
        private string? _abbreviation;

        /// <summary>
        /// Initializes a new instance of the <see cref="CtuFacultyBuilder"/> class.
        /// </summary>
        /// <param name="sourceBuilder">The source builder.</param>
        /// <param name="code">The faculty code.</param>
        public CtuFacultyBuilder(CtuSourceBuilder sourceBuilder, string code)
        {
            _sourceBuilder = sourceBuilder;
            _code = code;
        }

        /// <summary>
        /// Sets the faculty name.
        /// </summary>
        /// <param name="name">The faculty name.</param>
        /// <returns>This builder.</returns>
        public ICtuFacultyBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        /// <summary>
        /// Sets the faculty abbreviation.
        /// </summary>
        /// <param name="abbreviation">The faculty abbreviation.</param>
        /// <returns>This builder.</returns>
        public ICtuFacultyBuilder WithAbbreviation(string abbreviation)
        {
            _abbreviation = abbreviation;
            return this;
        }

        /// <summary>
        /// Finishes building the faculty.
        /// </summary>
        /// <returns>The source builder.</returns>
        public ICtuSourceBuilder Finish()
        {
            var faculty = new Division(
                Abbreviation: _abbreviation,
                Code: _code,
                Name: _name ?? _code,
                Parent: null,
                Type: DivisionType.Faculty
            );

            _sourceBuilder.AddFacultyInternal(_code, faculty);
            return _sourceBuilder;
        }
    }

    /// <summary>
    /// Internal implementation of programme builder.
    /// </summary>
    private class CtuProgrammeBuilder : ICtuProgrammeBuilder
    {
        private readonly CtuSourceBuilder _sourceBuilder;
        private readonly string _code;
        private ProgrammeType? _type;
        private string? _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="CtuProgrammeBuilder"/> class.
        /// </summary>
        /// <param name="sourceBuilder">The source builder.</param>
        /// <param name="code">The programme code.</param>
        public CtuProgrammeBuilder(CtuSourceBuilder sourceBuilder, string code)
        {
            _sourceBuilder = sourceBuilder;
            _code = code;
        }

        /// <summary>
        /// Sets the programme type.
        /// </summary>
        /// <param name="type">The programme type.</param>
        /// <returns>This builder.</returns>
        public ICtuProgrammeBuilder WithType(ProgrammeType type)
        {
            _type = type;
            return this;
        }

        public ICtuProgrammeBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        /// <summary>
        /// Finishes building the programme.
        /// </summary>
        /// <returns>The source builder.</returns>
        public ICtuSourceBuilder Finish()
        {
            var programme = new Programme(
                AcademicTitle: null,
                Capacity: null,
                ClassesLang: null,
                Code: _code,
                Description: null,
                DiplomaName: null,
                Division: null,
                Guarantor: null,
                Name: _name,
                OpenForAdmission: null,
                StudyDuration: null,
                ProgrammeType: _type
            );

            _sourceBuilder.AddProgrammeInternal(_code, programme);
            return _sourceBuilder;
        }
    }

    /// <summary>
    /// Internal implementation of user builder.
    /// </summary>
    private class CtuUserBuilder : ICtuUserBuilder
    {
        private readonly CtuSourceBuilder _sourceBuilder;
        private readonly string _username;
        private readonly List<string> _usermapRoles = new();
        private readonly List<int> _studentRoleIds = new();
        private readonly List<int> _teacherRoleIds = new();
        private string[]? _preTitles;
        private string[]? _postTitles;
        private string? _name;
        private bool _includeInKosApi = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="CtuUserBuilder"/> class.
        /// </summary>
        /// <param name="sourceBuilder">The source builder.</param>
        /// <param name="username">The username.</param>
        public CtuUserBuilder(CtuSourceBuilder sourceBuilder, string username)
        {
            _sourceBuilder = sourceBuilder;
            _username = username;
        }

        /// <summary>
        /// Sets the user's pre-titles.
        /// </summary>
        /// <param name="titles">The pre-titles.</param>
        /// <returns>This builder.</returns>
        public ICtuUserBuilder WithPreTitles(string[] titles)
        {
            _preTitles = titles;
            return this;
        }

        /// <summary>
        /// Sets the user's post-titles.
        /// </summary>
        /// <param name="titles">The post-titles.</param>
        /// <returns>This builder.</returns>
        public ICtuUserBuilder WithPostTitles(string[] titles)
        {
            _postTitles = titles;
            return this;
        }

        /// <summary>
        /// The person shouldn't be returned by kosapi.
        /// </summary>
        /// <remarks>
        /// This is useful for testing Usermap fallback.
        /// </remarks>
        /// <returns>This builder.</returns>
        public ICtuUserBuilder DoNotIncludeInKosApi()
        {
            _includeInKosApi = false;
            return this;
        }

        /// <summary>
        /// Adds a usermap role.
        /// </summary>
        /// <param name="role">The usermap role.</param>
        /// <returns>This builder.</returns>
        public ICtuUserBuilder WithUsermapRole(string role)
        {
            _usermapRoles.Add(role);
            return this;
        }

        /// <summary>
        /// Sets the user's name.
        /// </summary>
        /// <param name="name">The user's name.</param>
        /// <returns>This builder.</returns>
        public ICtuUserBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        /// <summary>
        /// Adds a student role to the user.
        /// </summary>
        /// <returns>A student role builder.</returns>
        public ICtuStudentRoleBuilder AddStudentRole()
        {
            return new CtuStudentRoleBuilder(_sourceBuilder, this, _username);
        }

        /// <summary>
        /// Adds a teacher role to the user.
        /// </summary>
        /// <returns>A teacher role builder.</returns>
        public ICtuTeacherRoleBuilder AddTeacherRole()
        {
            return new CtuTeacherRoleBuilder(_sourceBuilder, this, _username);
        }

        /// <summary>
        /// Adds a student role ID internally.
        /// </summary>
        /// <param name="studentRoleId">The student role ID.</param>
        private void AddStudentRoleId(int studentRoleId)
        {
            _studentRoleIds.Add(studentRoleId);
        }

        /// <summary>
        /// Adds a teacher role ID internally.
        /// </summary>
        /// <param name="teacherRoleId">The teacher role ID.</param>
        private void AddTeacherRoleId(int teacherRoleId)
        {
            _teacherRoleIds.Add(teacherRoleId);
        }

        /// <summary>
        /// Finishes building the user.
        /// </summary>
        /// <returns>The source builder.</returns>
        public ICtuSourceBuilder Finish()
        {
            var nameParts = _name?.Split(' ', 2) ?? new[] { _username, string.Empty };
            var firstName = nameParts.Length > 0 ? nameParts[0] : _username;
            var lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

            var person = new Person(
                FirstName: firstName,
                LastName: lastName,
                PersonalNumber: "000000000",
                Roles: new PersonRoles
                {
                    Students = _studentRoleIds.Select(x => new AtomLoadableEntity<Student> { Href = $"/students/{x}" }).ToList(),
                    Teacher = _teacherRoleIds.Select(x => new AtomLoadableEntity<Teacher> { Href = $"/teachers/{x}" }).FirstOrDefault()
                },
                TitlesPre: string.Join(" ", _preTitles ?? Array.Empty<string>()),
                TitlesPost: string.Join(" ", _postTitles ?? Array.Empty<string>()),
                Username: _username
            );

            _sourceBuilder.AddPersonInternal(_username, person, _includeInKosApi);
            _sourceBuilder.AddUsermapRolesInternal(_username, _usermapRoles);
            return _sourceBuilder;
        }

        /// <summary>
        /// Internal implementation of student role builder.
        /// </summary>
        private class CtuStudentRoleBuilder : ICtuStudentRoleBuilder
        {
            private readonly CtuSourceBuilder _sourceBuilder;
            private readonly CtuUserBuilder _userBuilder;
            private readonly string _username;
            private string? _facultyCode;
            private string? _programmeCode;
            private DateTime? _startDate;
            private DateTime? _endDate;
            private ushort? _grade;
            private StudyState _studyState = StudyState.Undefined;
            private StudyTermination _studyTermination = StudyTermination.Undefined;

            /// <summary>
            /// Initializes a new instance of the <see cref="CtuStudentRoleBuilder"/> class.
            /// </summary>
            /// <param name="sourceBuilder">The source builder.</param>
            /// <param name="userBuilder">The user builder.</param>
            /// <param name="username">The username.</param>
            public CtuStudentRoleBuilder(CtuSourceBuilder sourceBuilder, CtuUserBuilder userBuilder, string username)
            {
                _sourceBuilder = sourceBuilder;
                _userBuilder = userBuilder;
                _username = username;
            }

            /// <summary>
            /// Sets the faculty for the student role.
            /// </summary>
            /// <param name="facultyCode">The faculty code.</param>
            /// <returns>This builder.</returns>
            public ICtuStudentRoleBuilder WithFaculty(string facultyCode)
            {
                _facultyCode = facultyCode;
                return this;
            }

            /// <summary>
            /// Sets the programme for the student role.
            /// </summary>
            /// <param name="programmeCode">The programme code.</param>
            /// <returns>This builder.</returns>
            public ICtuStudentRoleBuilder WithProgramme(string programmeCode)
            {
                _programmeCode = programmeCode;
                return this;
            }

            /// <summary>
            /// Sets the start date for the student role.
            /// </summary>
            /// <param name="year">The start year.</param>
            /// <param name="month">The start month.</param>
            /// <param name="day">The start day.</param>
            /// <returns>This builder.</returns>
            public ICtuStudentRoleBuilder WithStartDate(int year, int month, int day)
            {
                _startDate = new DateTime(year, month, day);
                return this;
            }

            /// <summary>
            /// Sets the end date for the student role.
            /// </summary>
            /// <param name="year">The end year.</param>
            /// <param name="month">The end month.</param>
            /// <param name="day">The end day.</param>
            /// <returns>This builder.</returns>
            public ICtuStudentRoleBuilder WithEndDate(int year, int month, int day)
            {
                _endDate = new DateTime(year, month, day);
                return this;
            }

            /// <summary>
            /// Sets the study grade for the student role.
            /// </summary>
            /// <param name="grade">The study grade.</param>
            /// <returns>This builder.</returns>
            public ICtuStudentRoleBuilder WithGrade(ushort grade)
            {
                _grade = grade;
                return this;
            }

            /// <summary>
            /// Sets the student as graduated.
            /// </summary>
            /// <returns>This builder.</returns>
            public ICtuStudentRoleBuilder Graduated()
            {
                _studyState = StudyState.Closed;
                _studyTermination = StudyTermination.Graduation;
                return this;
            }

            /// <summary>
            /// Sets the student as currently studying.
            /// </summary>
            /// <returns>This builder.</returns>
            public ICtuStudentRoleBuilder Studying()
            {
                _studyState = StudyState.Active;
                return this;
            }

            /// <summary>
            /// Sets the student as withdrew.
            /// </summary>
            /// <returns>This builder.</returns>
            public ICtuStudentRoleBuilder Withdrew()
            {
                _studyState = StudyState.Closed;
                _studyTermination = StudyTermination.Withdraw;
                return this;
            }

            /// <summary>
            /// Finishes building the student role.
            /// </summary>
            /// <returns>The user builder.</returns>
            public ICtuUserBuilder Finish()
            {
                var nameParts = _username.Split(' ', 2);
                var firstName = nameParts.Length > 0 ? nameParts[0] : _username;
                var lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

                AtomLoadableEntity<Division>? faculty = null;
                if (_facultyCode != null)
                {
                    faculty = new AtomLoadableEntity<Division> { Href = $"/divisions/{_facultyCode}", Title = _sourceBuilder.Faculties[_facultyCode].Name };
                }

                AtomLoadableEntity<Programme>? programme = null;
                if (_programmeCode != null)
                {
                    programme = new AtomLoadableEntity<Programme> { Href = $"/programmes/{_programmeCode}", Title = _sourceBuilder.Programmes[_programmeCode].Name };
                }

                var student = new Student(
                    Branch: null,
                    Department: null,
                    Email: null,
                    StartDate: _startDate,
                    Faculty: faculty,
                    FirstName: firstName,
                    Grade: _grade,
                    InterruptedUntil: null,
                    LastName: lastName,
                    PersonalNumber: "000000000",
                    Programme: programme,
                    EndDate: _endDate,
                    StudyForm: null,
                    StudyGroup: null,
                    StudyPlan: null,
                    StudyState: _studyState,
                    Supervisor: null,
                    SupervisorSpecialist: null,
                    StudyTerminationReason: _studyTermination,
                    TitlesPost: null,
                    TitlesPre: null,
                    Username: _username
                );

                var studentId = _sourceBuilder.AddStudentInternal(student);
                _userBuilder.AddStudentRoleId(studentId);
                return _userBuilder;
            }
        }

        /// <summary>
        /// Internal implementation of teacher role builder.
        /// </summary>
        private class CtuTeacherRoleBuilder : ICtuTeacherRoleBuilder
        {
            private readonly CtuSourceBuilder _sourceBuilder;
            private readonly CtuUserBuilder _userBuilder;
            private readonly string _username;
            private string? _divisionCode;
            private bool? _external;
            private string? _email;
            private string? _phone;

            /// <summary>
            /// Initializes a new instance of the <see cref="CtuTeacherRoleBuilder"/> class.
            /// </summary>
            /// <param name="sourceBuilder">The source builder.</param>
            /// <param name="userBuilder">The user builder.</param>
            /// <param name="username">The username.</param>
            public CtuTeacherRoleBuilder(CtuSourceBuilder sourceBuilder, CtuUserBuilder userBuilder, string username)
            {
                _sourceBuilder = sourceBuilder;
                _userBuilder = userBuilder;
                _username = username;
            }

            /// <summary>
            /// Sets the division for the teacher role.
            /// </summary>
            /// <param name="divisionCode">The division code.</param>
            /// <returns>This builder.</returns>
            public ICtuTeacherRoleBuilder WithDivision(string divisionCode)
            {
                _divisionCode = divisionCode;
                return this;
            }

            /// <summary>
            /// Sets whether the teacher is external.
            /// </summary>
            /// <param name="external">True if external, false otherwise.</param>
            /// <returns>This builder.</returns>
            public ICtuTeacherRoleBuilder WithExternal(bool external)
            {
                _external = external;
                return this;
            }

            /// <summary>
            /// Sets the email for the teacher.
            /// </summary>
            /// <param name="email">The email address.</param>
            /// <returns>This builder.</returns>
            public ICtuTeacherRoleBuilder WithEmail(string email)
            {
                _email = email;
                return this;
            }

            /// <summary>
            /// Sets the phone for the teacher.
            /// </summary>
            /// <param name="phone">The phone number.</param>
            /// <returns>This builder.</returns>
            public ICtuTeacherRoleBuilder WithPhone(string phone)
            {
                _phone = phone;
                return this;
            }

            /// <summary>
            /// Sets a random start date for the teacher role.
            /// </summary>
            /// <returns>This builder.</returns>
            public ICtuTeacherRoleBuilder WithRandomStartDate()
            {
                // For testing purposes, we'll just use a fixed "random" date
                return this;
            }

            /// <summary>
            /// Finishes building the teacher role.
            /// </summary>
            /// <returns>The user builder.</returns>
            public ICtuUserBuilder Finish()
            {
                var nameParts = _username.Split(' ', 2);
                var firstName = nameParts.Length > 0 ? nameParts[0] : _username;
                var lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

                AtomLoadableEntity<Division>? division = null;
                if (_divisionCode != null)
                {
                    division = new AtomLoadableEntity<Division> { Href = $"/divisions/{_divisionCode}", Title = _divisionCode };
                }

                var teacher = new Teacher(
                    Division: division,
                    Email: _email,
                    Extern: _external,
                    FirstName: firstName,
                    LastName: lastName,
                    PersonalNumber: "000000000",
                    Phone: _phone,
                    StageName: null,
                    SupervisionPhDStudents: null,
                    TitlesPost: null,
                    TitlesPre: null,
                    Username: _username
                );

                var teacherId = _sourceBuilder.AddTeacherInternal(teacher);
                _userBuilder.AddTeacherRoleId(teacherId);
                return _userBuilder;
            }
        }
    }
}
