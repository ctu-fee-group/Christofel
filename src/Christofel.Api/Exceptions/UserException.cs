using System;
using System.Runtime.Serialization;
using Christofel.Api.GraphQL.Common;

namespace Christofel.Api.Exceptions
{
    /// <summary>
    /// Exception that is showed to the user
    /// </summary>
    public class UserException : Exception
    {
        public UserException(UserErrorCode code)
        {
            ErrorCode = code;
        }
        
        public UserErrorCode ErrorCode { get; }

        public UserException(UserErrorCode code, string? message) : base(message)
        {
        }

        public UserException(UserErrorCode code, string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}