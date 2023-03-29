using System;

namespace IntelliTect.Coalesce.Models
{
    public interface IApiResult
    {
        bool WasSuccessful { get; }
        string? Message { get; }
    }

    public class ApiResult : IApiResult
    {
        public bool WasSuccessful { get; set; } = true;

        public string? Message { get; set; }

        /// <summary>
        /// Controls the shape of the DTO mapping of the result object.
        /// </summary>
#if NETCOREAPP3_1_OR_GREATER
        [System.Text.Json.Serialization.JsonIgnore]
#endif
        [System.Runtime.Serialization.IgnoreDataMember] // for newtonsoft
        public IncludeTree? IncludeTree { get; set; }

        public ApiResult() { }

        public ApiResult(bool wasSuccessful, string? message = null) 
        {
            WasSuccessful = wasSuccessful;
            Message = message;
        }

        public ApiResult(string? errorMessage) : this(false, errorMessage) { }

        public ApiResult(ApiResult result) : this(result.WasSuccessful, result.Message)
        {
            IncludeTree = result.IncludeTree;
        }
    }
}