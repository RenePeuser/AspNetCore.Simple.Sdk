using System;
using System.Collections.Immutable;
using System.Linq;
using Extensions.Pack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.Simple.Sdk.ErrorHandling
{
    public class ValidationProblemDetailsException : Exception
    {
        public ValidationProblemDetailsException(params (string key, string[] value)[] errors) : this("One or more validation errors occurred.", errors)
        {
        }

        public ValidationProblemDetailsException(string title,
                                                 params (string key, string[] value)[] errors) : this(title, string.Empty, errors)
        {
        }

        public ValidationProblemDetailsException(string title,
                                                 string details,
                                                 params (string key, string[] value)[] errors) : this(title, details, errors.ToImmutableDictionary(item => item.key, item => item.value), ImmutableDictionary<string, string>.Empty)
        {
        }

        public ValidationProblemDetailsException(string title,
                                                 string details,
                                                 string type,
                                                 params (string key, string[] value)[] errors) : this(title, details, type, errors.ToImmutableDictionary(item => item.key, item => item.value), ImmutableDictionary<string, string>.Empty)
        {
        }

        public ValidationProblemDetailsException(string title,
                                                 string details,
                                                 IImmutableDictionary<string, string[]> errors,
                                                 IImmutableDictionary<string, string> extensions) : this(title, details, "https://www.rfc-editor.org/rfc/rfc7231#section-6.5.1", errors, extensions)
        {
        }

        public ValidationProblemDetailsException(string title,
                                                 string details,
                                                 string type,
                                                 IImmutableDictionary<string, string[]> errors,
                                                 IImmutableDictionary<string, string> extensions) : base(title)
        {
            var problemDetails = new ValidationProblemDetails()
            {
                Title = title.IsEmpty() ? null : "One or more validation errors occurred.",
                Detail = details.IsEmpty() ? null : details,
                Status = StatusCodes.Status400BadRequest,
                Type = type.IsEmpty() ? "https://www.rfc-editor.org/rfc/rfc7231#section-6.5.1" : type
            };

            extensions.OrderBy(item => item.Key).ForEach(keyValue =>
            {
                var key = keyValue.Key.Split(" ").Select(value => value.FirstCharToUpper()).Flatten().FirstCharToLower();
                problemDetails.Extensions.Add(key, keyValue.Value);
            });

            errors.OrderBy(item => item.Key).ForEach(keyValue =>
            {
                var key = keyValue.Key.Split(" ").Select(value => value.FirstCharToUpper()).Flatten().FirstCharToLower();
                problemDetails.Errors.Add(key, keyValue.Value);
            });

            ValidationProblemDetails = problemDetails;
        }

        public ValidationProblemDetails ValidationProblemDetails { get; }
    }
}
