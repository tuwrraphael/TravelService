using System.Linq;
using System.Security.Claims;

namespace TravelService.Controllers
{
    public static class ControllerExtension
    {
        public static string GetId(this ClaimsPrincipal p)
        {
            return p.Claims.Where(v => v.Type == "sub").Single().Value;
        }
    }
}
