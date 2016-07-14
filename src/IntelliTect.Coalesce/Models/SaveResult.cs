using System;
using System.Collections.Generic;

namespace IntelliTect.Coalesce.Models
{
    public class SaveResult
    {
        public bool WasSuccessful { get; set; }
        public string Message { get; set; }
        public List<ValidationIssue> ValidationIssues { get; set; }

        public SaveResult()
        {
            ValidationIssues = new List<ValidationIssue>();
        }

        public SaveResult(bool wasSuccessful) : this()
        {
            WasSuccessful = wasSuccessful;
        }
        public SaveResult(Exception ex) : this(false)
        {
            ValidationIssues.Add(new ValidationIssue("", "Exception: " + AddIssue(ex)));
        }

        public SaveResult(string problem) : this(false)
        {
            ValidationIssues.Add(new ValidationIssue("", problem));
        }


        protected string AddIssue(Exception ex)
        {
            var issues = ex.Message;
            if (ex.InnerException != null)
            {
                issues = Environment.NewLine + AddIssue(ex.InnerException);
            }
            return issues;
        }

    }

    public class SaveResult<T>: SaveResult
    {
        public T Object { get; set; }

        public SaveResult(): base()
        {
        }

        public SaveResult(string problem) : base(problem)
        {
        }


        public SaveResult(bool wasSuccessful, T obj) : base(wasSuccessful)
        {
            Object = obj;
        }

        public SaveResult(Exception ex) : base(false)
        {
            ValidationIssues.Add(new ValidationIssue("", "Exception: " + AddIssue(ex)));
        }


    }

}