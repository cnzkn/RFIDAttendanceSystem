namespace CloudAPI.Exceptions;

public class IdentityErrorException : ApplicationException
{
    public IdentityErrorException(string message) : base(message)
    {
        
    }

    public IdentityErrorException(IEnumerable<IdentityError> errors) : base($"Identity operation resulted in following errors.{Environment.NewLine}{string.Join(Environment.NewLine, errors)}")
    {
        
    }
    
    public IdentityErrorException(string message, IEnumerable<IdentityError> errors) : base($"{message}{Environment.NewLine}Error(s):{Environment.NewLine}{string.Join(Environment.NewLine, errors)}")
    {
    }
}
