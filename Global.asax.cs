using Datadog.Trace;
using Datadog.Trace.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Samples.AspNetMvc4
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            var settings = TracerSettings.FromDefaultSources();

            settings.Integrations["AdoNet"].Enabled = false;
            settings.Integrations["AspNet"].Enabled = true;
            settings.Integrations["AspNetMvc"].Enabled = true;
            settings.Integrations["AspNetWebApi2"].Enabled = true;
            settings.Integrations["Wcf"].Enabled = true;
            settings.Integrations["HttpMessageHandler"].Enabled = true;
            settings.Integrations["WebRequest"].Enabled = true;
            // create a new Tracer using these settings
            var tracer = new Tracer(settings);

            // set the global tracer
            Tracer.Instance = tracer;
        }
    }
}
