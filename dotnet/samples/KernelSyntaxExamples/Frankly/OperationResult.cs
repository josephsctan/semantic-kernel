using System;
using System.Reflection;

namespace Frankly
{

    /// <summary>
    /// A generically useful operation result with an error message
    /// </summary>
    public class OperationResult<T>
    {

        /// <summary>
        /// when using OperationResult, this provides a way to create 
        /// a result that starts as unsccessful 
        /// </summary>
        /// <returns></returns>
        public static OperationResult<T> NotOK(string? message = null)
        {
            message ??= "Initialized To Unsuccessful";
            return new OperationResult<T>() { OK = false, ErrorMessage = message };
        }

        /// <summary>
        /// create a result thst starts as successful
        /// </summary>
        /// <returns></returns>
        public static OperationResult<T> StartOK()
        {
            return new OperationResult<T>() { OK = true, ErrorMessage = null };
        }

        /// <summary>
        /// When this is called, the error is nulled, and the return value is sent
        /// </summary>
        public OperationResult<T> SetOK(T result, string? successMessage = null)
        {
            OK = true;
            Result = result;
            if (!string.IsNullOrEmpty(successMessage))
            {
                ErrorMessage = successMessage;
            }
            return this;
        }

        /// <summary>
        /// When this is called, the error message is set 
        /// </summary>
        /// <param name="callerToLogErrorFor">
        /// if provided (e.g. MethodBase.GetCurrentMethod()) - LOG ERROR  [CommonBase.Logger.Error]
        /// </param>
        public OperationResult<T> SetFail(string errorMessage, T? result = default(T), MethodBase? callerToLogErrorFor = null)
        {
            OK = false;
            ErrorMessage = string.IsNullOrEmpty(errorMessage) ? "Failed" : errorMessage;
            Result = result;

            // 2023-05-11: if a caller is provided, we should log the error
            if (callerToLogErrorFor != null)
            {
                Console.WriteLine(errorMessage);
            }
            return this;

        }

        /// <summary>
        /// set progress without changing the OK value
        /// </summary>
        /// <param name="message">Progress message to be appended to Error Message</param>
        /// <returns></returns>
        public OperationResult<T> AppendProgress(string message, bool log = false)
        {
            if (!string.IsNullOrEmpty(message))
            {
                ErrorMessage ??= "";
                ErrorMessage += "\r\n" + message;
                if (log)
                {
                    Console.WriteLine(message);
                }
            }
            return this;
        }


        /// <summary>
        /// Is Operation OK?
        /// </summary>
        public bool OK { get; private set; } = false;

        /// <summary>
        /// Error / Progress message
        /// </summary>
        /// <remarks>
        /// If it is null, that means there was no error
        /// </remarks>
        public string? ErrorMessage { get; private set; } = String.Empty;

        /// <summary>
        /// short cut that appends the "E:"  error prefix to the message 
        /// </summary>
        public string EErrorMessage => $"E:{ErrorMessage}";

        /// <summary>
        /// the operation result  (can be any thing)
        /// </summary>
        public T? Result { get; set; }
    }
}
