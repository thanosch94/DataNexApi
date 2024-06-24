using DataNex.Model.Dtos;
using Microsoft.AspNetCore.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace DataNexApi
{
    public class DataNexApiExceptionHandler:IExceptionHandler
    {
 
      
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var innerErrorMessage = string.Empty;

            if (exception.InnerException != null)
            {
                if (exception.InnerException.Message.Contains("The DELETE statement conflicted with the REFERENCE constraint"))
                {
                    innerErrorMessage = "Entity is in use and cannot be deleted.";
                }
                else if (exception.InnerException.Message.Contains("Cannot insert the value NULL into column"))
                {
                    string pattern = @"'((?:(?!dbo)[^'])*)'";

                    var columnName = Regex.Matches(exception.InnerException.Message, pattern);
                    if(columnName.Count==1)
                    {
                        innerErrorMessage = $"Value {columnName[0].Value} cannot be NULL";
                    }
                }
                else
                {
                    innerErrorMessage = exception.InnerException.Message;
                }
            }
            var errorResponse = new DataNexApiErrorResponseDto()
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = exception.Message,
                InnerExceptionMessage = innerErrorMessage,
                Title = "Internal Server Error"
            };
            httpContext.Response.StatusCode = errorResponse.StatusCode;
            await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);

            return true;
        }

 
    }
}
