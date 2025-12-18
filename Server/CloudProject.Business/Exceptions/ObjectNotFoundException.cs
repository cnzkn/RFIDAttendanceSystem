namespace CloudProject.Business.Exceptions;

public class ObjectNotFoundException : ApplicationException
{
    public ObjectNotFoundException(string message) : base(message) { }
}
