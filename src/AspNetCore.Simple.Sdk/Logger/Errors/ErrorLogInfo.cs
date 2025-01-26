using System.Collections.Generic;

namespace AspNetCore.Simple.Sdk.Logger.Errors
{
    public class ErrorLogInfo(string id,
                              string message,
                              string title,
                              IEnumerable<string> stackTrace,
                              IReadOnlyDictionary<string, object> requestInfos,
                              IReadOnlyDictionary<string, object> errorDetails,
                              string errorType)
    {
        public string Id { get; } = id;

        public string Title { get; } = title;

        public string Message { get; } = message;

        public IReadOnlyDictionary<string, object> ErrorDetails { get; } = errorDetails;

        // Gets our unique identifier to find all our errors in cloud watch via search query
        public string ErrorType { get; } = errorType;

        public IReadOnlyDictionary<string, object> RequestInfos { get; } = requestInfos;

        public IEnumerable<string> StackTrace { get; } = stackTrace;
    }
}
