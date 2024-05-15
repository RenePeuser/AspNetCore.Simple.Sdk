using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using Extensions.Pack;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.Simple.Sdk.ErrorHandling
{
    public class ProblemDetailsException : Exception
    {
        public ProblemDetailsException(string title,
                                       params (string key, object value)[] extensions) : this(HttpStatusCode.InternalServerError, title, string.Empty, extensions)
        {
        }

        public ProblemDetailsException(string title,
                                       string details,
                                       params (string key, object value)[] extensions) : this(HttpStatusCode.InternalServerError, title, details, extensions)
        {
        }

        public ProblemDetailsException(HttpStatusCode statusCode,
                                       string title,
                                       string details,
                                       params (string key, object value)[] extensions) : this(statusCode.ToInt(), title, details, extensions.ToImmutableDictionary(item => item.key, item => item.value))
        {
        }

        public ProblemDetailsException(int statusCode,
                                       string title,
                                       string details,
                                       params (string key, object value)[] extensions) : this(statusCode.ToInt(), title, details, extensions.ToImmutableDictionary(item => item.key, item => item.value))
        {
        }

        // i know this is evil with the conversion to immutable dictionary but a fast fix for now.

        public ProblemDetailsException(int statusCode,
                                       string title,
                                       string details,
                                       IImmutableDictionary<string, object> errorDetails) : base(title)
        {
            var problemDetails = new ProblemDetails() { Title = title.IsEmpty() ? null : title, Detail = details.IsEmpty() ? null : details, Status = statusCode };

            errorDetails.OrderBy(item => item.Key).ForEach(keyValue =>
            {
                var key = keyValue.Key.Split(" ").Select(value => value.FirstCharToUpper()).Flatten().FirstCharToLower();
                problemDetails.Extensions.Add(key, keyValue.Value);
            });

            ProblemDetails = problemDetails;
        }

        public ProblemDetails ProblemDetails { get; }
    }
}
