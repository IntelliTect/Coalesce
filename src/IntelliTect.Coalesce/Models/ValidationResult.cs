using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Models
{
    public class ValidateResult<T> where T:class
    {
     public ValidateResult(bool wasSuccessful =true, string message = null, T returnObject = null)
        {
            WasSuccessful = wasSuccessful;
            Message = message;
            ReturnObject = returnObject;
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
        /// Set this object on WasSuccessful = false to send this object back to the client to set the values there.
        /// </summary>
        public T ReturnObject { get; set; }

        /// <summary>
        /// Merges another ValidateResult with this one.
        /// </summary>
        /// <param name="result"></param>
        public void Merge(ValidateResult<T> result)
        {
            WasSuccessful = WasSuccessful && result.WasSuccessful;
            if (result.ReturnObject != null) { ReturnObject = result.ReturnObject; }
            if (!string.IsNullOrWhiteSpace(result.Message) )
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


        //TODO: Build out field level support.
    }
}
