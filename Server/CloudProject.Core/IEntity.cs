namespace CloudProject.Core;

public interface IEntity
{
    /// <summary>
    /// Unique identifier of this model.
    /// </summary>
    Guid Id { get; set; }
}
