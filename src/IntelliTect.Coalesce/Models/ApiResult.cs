using System;

namespace IntelliTect.Coalesce.Models
{
    public class ApiResult
    {
        public bool WasSuccessful { get; set; }

        public string Message { get; set; }

        public ApiResult()
        {
        }

        public ApiResult(bool wasSuccessful) : this()
        {
            WasSuccessful = wasSuccessful;
        }

        public ApiResult(string problem) : this(false)
        {
            Message = problem;
        }

        public ApiResult(Exception ex) : this(false)
        {
            Message = ex.Message;
        }
    }
}