namespace CloudProject.Business;

public class ClassroomManager
{
    private readonly ILogger<ClassroomManager> _logger;
    private readonly IRepository<ClassroomModel> _classroomRepository;
    
    public ClassroomManager(ILogger<ClassroomManager> logger, IRepository<ClassroomModel> classroomRepository)
    {
        _logger = logger;
        _classroomRepository = classroomRepository;
    }

    public async Task<ClassroomDto[]> GetAllAsync(CancellationToken token)
    {
        var classrooms = await _classroomRepository.GetAllAsync(token);

        return classrooms.Select(x => x.ToDto())
            .ToArray();
    }
    
    public async Task<ClassroomDto?> GetByNameAsync(string name, CancellationToken token)
    {
        var classroom = await _classroomRepository.FirstOrDefaultAsync(x => x.Name == name, token);
        return classroom?.ToDto();
    }
    
    public async Task<Guid?> GetIdByNameAsync(string name, CancellationToken token)
    {
        var classroom = await _classroomRepository.FirstOrDefaultAsync(x => x.Name == name, token);
        return classroom?.Id;
    }
    
    internal async Task<ClassroomModel?> InternalGetClassroomByIdAsync(Guid classroomId, CancellationToken token = default)
    {
        return await _classroomRepository.GetByIdAsync(classroomId, token);
    }
}
