using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Models
{
    public class ValidateResult
    {
        public ValidateResult(bool wasSuccessful = true, string message = null)
        {
            WasSuccessful = wasSuccessful;
            Message = message;
        }

        /// <summary>
        /// Set to true if the validation was successful.
        /// </summary>
        public bool WasSuccessful { get; set; } = true;

        /// <summary>
        /// General error message to hand back to the client.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Merges another ValidateResult with this one.
        /// </summary>
        /// <param name="result"></param>
        public void Merge(ValidateResult result)
        {
            WasSuccessful = WasSuccessful && result.WasSuccessful;
            if (!string.IsNullOrWhiteSpace(result.Message))
            {
                if (!string.IsNullOrWhiteSpace(Message))
                {
                    Message = Message + "  " + result.Message;
                }
                else
                {
                    Message = result.Message;
                }
            }
        }


        public static implicit operator ValidateResult(bool success)
        {
            return new ValidateResult(wasSuccessful: success);
        }

        public static implicit operator ValidateResult(string message)
        {
            return new ValidateResult(wasSuccessful: false, message: message);
        }

        //TODO: Build out field level support.
    }
    public class ValidateResult<T> : ValidateResult where T : class
    {
        public ValidateResult(bool wasSuccessful = true, string message = null, T returnObject = null)
               : base(wasSuccessful, message)
        {
            ReturnObject = returnObject;
        }

        /// <summary>
        /// Set this object on WasSuccessful = false to send this object back to the client to set the values there.
        /// </summary>
        public T ReturnObject { get; set; }

        /// <summary>
        /// Merges another ValidateResult with this one.
        /// </summary>
        /// <param name="result"></param>
        public void Merge(ValidateResult<T> result)
        {
            base.Merge(result);
            if (result.ReturnObject != null) { ReturnObject = result.ReturnObject; }
        }


        public static implicit operator ValidateResult<T>(bool success)
        {
            return new ValidateResult<T>(wasSuccessful: success);
        }

        public static implicit operator ValidateResult<T>(string message)
        {
            return new ValidateResult<T>(wasSuccessful: false, message: message);
        }
    }
}
