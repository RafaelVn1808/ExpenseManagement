namespace ExpenseApi.ErrorResponse
{
    public class BusinessException : Exception
    {
        public BusinessException(string message) : base(message)
        {
        }
    }
}
