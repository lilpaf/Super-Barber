namespace SuperBarber.Infrastructure
{
    using System.Security.Claims;

    using static CustomRoles;

    public static class ClaimsPrincipalExtensions
    {
        public static string Id(this ClaimsPrincipal user)
            => user.FindFirstValue(ClaimTypes.NameIdentifier);

        public static bool IsAdmin(this ClaimsPrincipal user)
            => user.IsInRole(AdministratorRoleName);
        
        public static bool IsBarber(this ClaimsPrincipal user)
            => user.IsInRole(BarberRoleName);
    }
}
