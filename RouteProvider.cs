using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Misc.OneS
{
    class RouteProvider:IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Admin.Plugin.QuickBooks.Configure",
                 "Admin/MiscOneS/Configure",
                 new { controller = "MiscOneS", action = "Configure" },
                 new[] { "Nop.Plugin.Misc.One.Controllerss" }
            ).DataTokens.Add("Area", "Admin");

            routes.MapRoute("Nop.Plugin.Misc.OneS",
                "Nop.Plugin.Misc.OneS",
                new { controller = "MiscOneS", action = "ImportHandler" },
                new[] { "Nop.Plugin.Misc.One.Controllers" }
           ).DataTokens.Add("Area", "Admin"); ;

        }
        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
