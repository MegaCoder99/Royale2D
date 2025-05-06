namespace Royale2D
{
    // REFACTOR no longer used due to chaos of exception handling inconsistency and complexity of if/else on every nested failure point, exceptions so much simpler, so remove this
    public class Result<T>
    {
        public string errorMessage { get; private set; }
        public bool succeeded => string.IsNullOrEmpty(errorMessage);
        public bool failed => !string.IsNullOrEmpty(errorMessage);
        public T data { get; private set; }

        // Constructor for success with data
        protected Result(T data)
        {
            this.data = data;
            errorMessage = "";
        }

        // Constructor for failure
        protected Result(string errorMessage)
        {
            this.errorMessage = errorMessage;
        }

        // Factory method for success with data
        public static Result<T> Success(T data) => new Result<T>(data);

        // Factory method for success without data
        public static Result<T> Success() => new Result<T>(default(T));

        // Factory methods for failure
        public static Result<T> Error(string errorMessage) => new Result<T>(errorMessage);
        public static Result<T> Error(Exception exception) => new Result<T>(exception.Message);

        public void ThrowOnFailure()
        {
            if (failed)
            {
                throw new Exception(errorMessage);
            }
        }

        // Returns a generic result, for outer scope function calls that don't care about the specific data shape obtained from more nested/inner scopes
        public Result ToGeneric()
        {
            return new Result(errorMessage);
        }
    }

    // This is a generic result for syntactic sugar, for places that don't care about a specific data shape, so just use bool as the placeholder
    public class Result : Result<bool>
    {
        public Result(bool success) : base(success) { }

        public Result(string errorMessage) : base(errorMessage) { }

        // Factory method for success without data
        public static new Result Success() => new Result(true);

        // Factory methods for failure
        public static new Result Error(string errorMessage) => new Result(errorMessage);
        public static new Result Error(Exception exception) => new Result(exception.Message);
    }

}
