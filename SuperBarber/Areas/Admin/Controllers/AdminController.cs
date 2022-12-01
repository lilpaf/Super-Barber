using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static SuperBarber.Areas.Admin.AdminConstants;

namespace SuperBarber.Areas.Admin.Controllers
{
    [Area(AreaName)]
    [Authorize(Roles = AdministratorRoleName)]
    public abstract class AdminController : Controller
    {
    }
}
