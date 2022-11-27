using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace SuperBarber.Infrastructure
{
    public static class SetTempDataModelStateExtension
    {
        public static void SetTempData(Controller controller, ModelStateCustomException exception)
        {
            Dictionary<string, string> error = new Dictionary<string, string>
            {
                { exception.Key, exception.Message }
            };

            controller.TempData["ModelState"] = JsonConvert.SerializeObject(error);
        }

        public static void SetTempData(Controller controller, string key, string messege)
        {
            var exception = new ModelStateCustomException(key, messege);

            Dictionary<string, string> error = new Dictionary<string, string>
            {
                { exception.Key, exception.Message }
            };

            controller.TempData["ModelState"] = JsonConvert.SerializeObject(error);
        }
    }
}
