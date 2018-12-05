using System;
using System.Collections.Generic;

namespace BlockApp.Models
{
    public class ErrorViewModel
    {
        public int HttpErrorCode { get; set; }
        
        public static readonly Dictionary<int, string> ErrorsTest = new Dictionary<int, string>
        {
            { 400, "Bad request" },
            { 401, "Not authorized" },
            { 403, "Access forbidden" },
            { 404, "Resource not found" },
            { 408, "The server timed out waiting for the request" },
            { 500, "Server error" }
        };
    }
}