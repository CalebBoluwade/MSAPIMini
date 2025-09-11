namespace MS.API.Mini.Data
{
    public class APIResponse<T>
    {
        public string Message { get; set; }
        public bool Success { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public T Data { get; set; }

        [JsonPropertyName("Errors"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string> Errors { get; set; }

        private APIResponse(bool success, string message, T data, List<string>? errors)
        {
            Success = success;
            Message = message;
            Data = data;
            Errors = errors!;
        }

        public static APIResponse<T> SuccessResult(T data, string message = ResponseMessage.Success)
        {
            return new APIResponse<T>(true, message, data, null);
        }

        public static APIResponse<T?> ErrorResult(string message, List<string>? errors = null)
        {
            return new APIResponse<T?>(false, message, default, errors ?? []);
        }
    }

    public class PagedResult<T>
    {
        public List<T> Data { get; set; } = [];
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;
    }

    public struct ResponseMessage
    {
        public const string Success = "Successful";
        public const string ExistingAccount = "Email is already registered.";
        public const string UserError = "Invalid Username and / or Password";
        public const string ExpiredLicense = "License Expired";
        public const string UnrecognizedDomain = "Email domain is not associated with any organization.";
        public const string Error = "Unexpected Error Occured";
        public const string FailedValidation = "Validation Failed";
    }
}