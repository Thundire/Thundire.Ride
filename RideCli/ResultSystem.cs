namespace RideCli;
static class ResultFactory
{
	public static Result<T> Success<T>(T data) => new SuccessResult<T>(data);
	public static Result<T> Failure<T>(int exitCode) => new FailureResult<T>() { ExitCode = exitCode };
}

record Result<T>(bool IsSuccess)
{
	public Result(bool isSuccess, T data) : this(isSuccess)
	{
		Data = data;
	}

	public bool IsFailed { get; } = !IsSuccess;
	public T? Data { get; init; }
	public int ExitCode { get; init; } = 0;
}

record SuccessResult<T>(T Data) : Result<T>(true, Data);
record FailureResult<T>() : Result<T>(false);
