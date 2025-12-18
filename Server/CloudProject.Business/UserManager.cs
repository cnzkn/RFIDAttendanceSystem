using Microsoft.AspNetCore.Identity;

namespace CloudProject.Business;

public class UserManager
{
    private readonly ILogger<UserManager> _logger;
    private readonly UserManager<UserModel> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public UserManager(ILogger<UserManager> logger, UserManager<UserModel> userManager, RoleManager<IdentityRole<Guid>> roleManager)
    {
        _logger = logger;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    internal async Task<UserModel?> InternalGetByIdAsync(Guid userId, CancellationToken token = default)
    {
        return await _userManager.FindByIdAsync(userId.ToString());
    }
    
    public async Task<UserDto?> GetByIdAsync(Guid userId, CancellationToken token = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user?.ToDto();
    }
}
