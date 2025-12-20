namespace CloudProject.API.Utility;

public class ControllerEx : Controller
{
    protected UserManager<UserModel> UserManager => HttpContext.RequestServices.GetRequiredService<UserManager<UserModel>>();

    protected Guid? GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return null;
        
        return Guid.Parse(userId);
    }
    
    protected async Task<UserModel?> GetCurrentUserAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return null;

        var user = await UserManager.FindByIdAsync(userId);
        if (user == null) return null;
        
        return user;
    }
}
