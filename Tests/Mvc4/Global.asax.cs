using SkyBuilding.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace Mvc4
{
    /// <inheritdoc />
    public class WebApiApplication : HttpApplication
    {
        /// <inheritdoc />
        protected void Application_Start()
        {
            ApiConfig.Register(GlobalConfiguration.Configuration);
        }
    }
}