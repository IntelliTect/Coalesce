﻿namespace IntelliTect.Coalesce.Models
{
    public class ApiResult
    {
        public bool WasSuccessful { get; set; } = true;

        public string? Message { get; set; }

        /// <summary>
        /// Controls the shape of the DTO mapping of the result object.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
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