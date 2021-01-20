using System;
using Microsoft.AspNetCore.Http;

namespace AspNetCore.Simple.Sdk.ErrorHandling
{
    public class UserResponseException : Exception
    {
        public UserResponseException(string title,
                                     string details,
                                     params string[] errors) : base(title)
        {
            UserErrorResponse = new UserErrorResponse(StatusCodes.Status400BadRequest, title, details, errors);
        }

        internal UserErrorResponse UserErrorResponse { get; }
    }
}
