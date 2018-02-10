using System;

namespace IntelliTect.Coalesce.Models
{
    public interface IApiResult
    {
        bool WasSuccessful { get; }
        string Message { get; }
    }

    public class ApiResult : IApiResult
    {
        public bool WasSuccessful { get; set; } = true;

        public string Message { get; set; }

        public ApiResult() { }

        public ApiResult(bool wasSuccessful, string message = null) 
        {
            WasSuccessful = wasSuccessful;
            Message = message;
        }

        public ApiResult(string message) : this(false, message) { }

        public ApiResult(ApiResult result) : this(result.WasSuccessful, result.Message) { }
    }
}