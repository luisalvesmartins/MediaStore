using CoreObjects;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

namespace Functions
{
    public class UserProfile:UserCoreProfile
    {
        public bool isAuthenticated { get; set; }
        public static async Task<UserProfile> Load(HttpRequest req,string connectionString)
        {
            if (req.Headers["x-zumo-auth"].Count>0)
            {
                string pStrXZumoAuth = req.Headers["x-zumo-auth"].First();
                if (pStrXZumoAuth=="null" || pStrXZumoAuth =="")
                {
                    return new UserProfile() { allowedTags = "", email = "", isAuthenticated = false, isAdmin = false };
                }
                string authEndpoint=AuthInfoExtensions.GetEasyAuthEndpoint();
                //authEndpoint
                string email = await AuthFunctions.GetAuthProviderParam(authEndpoint, pStrXZumoAuth, "user_id");

                UserCoreProfile UCP= await UserCoreProfile.Load(connectionString, email);
                UserProfile UP = new UserProfile();
                UP.allowedEventTags = UCP.allowedEventTags;
                UP.allowedTags = UCP.allowedTags;
                UP.email = UCP.email;
                UP.events = UCP.events;
                UP.isAdmin = UCP.isAdmin;
                UP.isAuthenticated = true;

                return UP;
            }
            return new UserProfile() { allowedTags = "", email = "noone", isAuthenticated = false, isAdmin=false };
        }
    }
}
