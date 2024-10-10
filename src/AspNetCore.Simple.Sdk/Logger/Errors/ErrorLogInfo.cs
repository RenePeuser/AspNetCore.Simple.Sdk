using System.Collections.Generic;

namespace AspNetCore.Simple.Sdk.Logger.Errors
{
    public class ErrorLogInfo
    {
        public ErrorLogInfo(string id,
                            string message,
                            string title,
                            IEnumerable<string> stackTrace,
                            IReadOnlyDictionary<string, object> requestInfos,
                            IReadOnlyDictionary<string, object> errorDetails,
                            string errorType)
        {
            Id = id;
            Title = title;
            Message = message;
            StackTrace = stackTrace;
            RequestInfos = requestInfos;
            ErrorDetails = errorDetails;
            ErrorType = errorType;
        }

        public string Id { get; }

        public string Title { get; }

        public string Message { get; }

        public IReadOnlyDictionary<string, object> ErrorDetails { get; }

        // Gets our unique identifier to find all our errors in cloud watch via search query
        public string ErrorType { get; }

        public IReadOnlyDictionary<string, object> RequestInfos { get; }

        public IEnumerable<string> StackTrace { get; }
    }
}
