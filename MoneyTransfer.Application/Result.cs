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

        public static Result<T> Success(string message) => new Result<T>(true, message, new List<string>(), default!);
        public static Result<T> Success(T data) => new Result<T>(true, "Operation successful", new List<string>(), data);

        // Failure
        public static Result<T> Failure(string message) => new Result<T>(false, message, new List<string>(), default!);
    }
}
