using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace SuperBarber.Infrastructure
{
    public class RestoreModelStateFromTempDataAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            Controller? controller = filterContext.Controller as Controller;

            if (controller != null)
            {
                if (controller.TempData.ContainsKey("ModelState"))
                {
                    var error = JsonConvert.DeserializeObject<Dictionary<string, string>>((string)controller.TempData["ModelState"]);
                    if (error.Count > 0) 
                    {
                        controller.ViewData.ModelState.AddModelError(error.First().Key, error.First().Value);
                    }
                }
            }
        }
    }
}
