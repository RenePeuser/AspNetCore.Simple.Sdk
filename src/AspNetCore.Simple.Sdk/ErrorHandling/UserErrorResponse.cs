using System.Collections.Generic;

namespace AspNetCore.Simple.Sdk.ErrorHandling
{
    public record UserErrorResponse(int StatusCode,
                                    string Title,
                                    string Details,
                                    IEnumerable<string> Errors);
}