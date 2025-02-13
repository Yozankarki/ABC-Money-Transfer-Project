namespace MoneyTransfer.Application
{
    public class Result<T>
    {
        public bool Succeeded { get; }
        public string Message { get; }
        public List<string> Errors { get; }
        public T Data { get; }

        private Result(bool succeeded, string message, List<string> errors, T data)
        {
            Succeeded = succeeded;
            Message = message;
            Errors = errors;
            Data = data;
        }

        // Success methods
        public static Result<T> Success(string message) => new Result<T>(true, message, new List<string>(), default!);
        public static Result<T> Success(string message, T data) => new Result<T>(true, message, new List<string>(), data);

        // Failure methods
        public static Result<T> Failure(string message) => new Result<T>(false, message, new List<string>(), default!);
        public static Result<T> Failure(List<string> errors) => new Result<T>(false, "Operation failed", errors, default!);
    }
}
