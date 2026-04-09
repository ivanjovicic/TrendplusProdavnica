#nullable enable
using System;
using System.Collections.Generic;

namespace TrendplusProdavnica.Application.Admin.Common
{
    public class AdminValidationException : Exception
    {
        public AdminValidationException(string message)
            : base(message)
        {
            Errors = new Dictionary<string, string[]>
            {
                ["general"] = new[] { message }
            };
        }

        public AdminValidationException(string message, IDictionary<string, string[]> errors)
            : base(message)
        {
            Errors = new Dictionary<string, string[]>(errors);
        }

        public IReadOnlyDictionary<string, string[]> Errors { get; }
    }

    public class AdminNotFoundException : Exception
    {
        public AdminNotFoundException(string message)
            : base(message)
        {
        }
    }

    public class AdminConflictException : Exception
    {
        public AdminConflictException(string message)
            : base(message)
        {
        }
    }
}
