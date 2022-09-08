using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using Extensions.Pack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;

namespace AspNetCore.Simple.Sdk.ErrorHandling
{
    public class ProblemDetailsException : Exception
    {
        public ProblemDetailsException(string title,
                                       string details,
                                       params (string key, string value)[] extensions) : this(HttpStatusCode.InternalServerError, title, details, extensions)
        {
        }

        public ProblemDetailsException(HttpStatusCode statusCode,
                                       string title,
                                       string details,
                                       params (string key, string value)[] extensions) : this(statusCode.ToInt(), title, details, extensions.ToImmutableDictionary(item => item.key, item => item.value.ToString()))
        {
        }

        public ProblemDetailsException(int statusCode,
                                       string title,
                                       string details,
                                       params (string key, string value)[] extensions) : this(statusCode.ToInt(), title, details, extensions.ToImmutableDictionary(item => item.key, item => item.value.ToString()))
        {
        }

        // i know this is evil with the conversion to immutable dictionary but a fast fix for now.

        public ProblemDetailsException(int statusCode,
                                       string title,
                                       string details,
                                       IImmutableDictionary<string, string> errorDetails) : base(title)
        {
            var problemDetails = new ProblemDetails()
            {
                Title = title,
                Detail = details,
                Status = statusCode
            };

            errorDetails.ForEach(keyValue =>
            {
                var key = keyValue.Key.Split(" ").Select(value => value.FirstCharToUpper()).Flatten().FirstCharToLower();
                problemDetails.Extensions.Add(key, keyValue.Value);
            });

            ProblemDetails = problemDetails;
        }

        public ProblemDetails ProblemDetails { get; }
    }
}
