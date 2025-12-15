namespace OdontoSys.Api.Application;

public class ApplicationResult<T>
{
    public T? Data { get; private set; }
    public string? Message { get; private set; }
    public bool IsSuccess { get; private set; }
    public int StatusCode { get; private set; }
    
    private ApplicationResult(T? data, bool isSuccess, int statusCode, string? message = null)
    {
        Data = data;
        IsSuccess = isSuccess;
        StatusCode = statusCode;
        Message = message;
    }
    
    public static ApplicationResult<T> Success(T data, int statusCode = 200, string? message = null)
    {
        return new ApplicationResult<T>(data, true, statusCode, message);
    }
    
    public static ApplicationResult<T> Failure(string message, int statusCode)
    {
        return new ApplicationResult<T>(default, false, statusCode, message);
    }  
}