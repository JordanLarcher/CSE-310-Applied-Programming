using System.Threading.Tasks;

namespace TaskScheduler.Application.Services.Interfaces
{
    public interface IGoogleAuthService
    {
        Task<GoogleUserInfo> ValidateGoogleTokenAsync(string idToken);
    }

    public class GoogleUserInfo
    {
        public string GoogleId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Picture { get; set; }
        public bool EmailVerified { get; set; }
    }
}
