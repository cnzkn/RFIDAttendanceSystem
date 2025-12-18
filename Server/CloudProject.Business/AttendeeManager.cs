namespace CloudProject.Business;

public class AttendeeManager
{
    private readonly ILogger<AttendeeManager> _logger;
    private readonly IRepository<AttendeeModel> _attendeeRepository;
    
    public AttendeeManager(ILogger<AttendeeManager> logger, IRepository<AttendeeModel> attendeeRepository)
    {
        _logger = logger;
        _attendeeRepository = attendeeRepository;
    }
    
    internal async Task<AttendeeModel?> InternalGetByIdAsync(Guid attendeeId, CancellationToken token = default)
    {
        return await _attendeeRepository.GetByIdAsync(attendeeId, token);
    }
    
    internal async Task<AttendeeModel?> InternalGetByCardAsync(byte[] cardId, CancellationToken token = default)
    {
        return await _attendeeRepository.FirstOrDefaultAsync(x => cardId.SequenceEqual(x.CardUID), token);
    }
}
