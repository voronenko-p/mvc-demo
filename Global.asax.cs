using Datadog.Trace;
using Datadog.Trace.Configuration;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
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

            log4net.Config.XmlConfigurator.Configure();

            var settings = TracerSettings.FromDefaultSources();

            settings.Integrations["AdoNet"].Enabled = false;
            settings.Integrations["AspNet"].Enabled = true;
            settings.Integrations["AspNetMvc"].Enabled = true;
            settings.Integrations["AspNetWebApi2"].Enabled = true;
            settings.Integrations["Wcf"].Enabled = true;
            settings.Integrations["HttpMessageHandler"].Enabled = true;
            settings.Integrations["WebRequest"].Enabled = true;
                        
            Process currentProcessInfo = System.Diagnostics.Process.GetCurrentProcess();
            var startTime = currentProcessInfo.StartTime;

            TimeSpan startTimeSpan = (startTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
            var startTimeMilliseconds = Convert.ToUInt64(Math.Truncate(startTimeSpan.TotalMilliseconds));

            ILog log = log4net.LogManager.GetLogger(typeof(Tracer));
            log.Info($"Starting... {currentProcessInfo.Id.ToString()}");

            var enrichMode = ConfigurationManager.AppSettings["STS_ENRICH_MODE"];
            if (string.IsNullOrEmpty(enrichMode) || enrichMode != "OFF")
            {
                log.Info("Enriching with pid starttime hostname");
                settings.GlobalTags.Add("span.pid", currentProcessInfo.Id.ToString());
                settings.GlobalTags.Add("span.starttime", startTimeMilliseconds.ToString());
                if (!settings.GlobalTags.ContainsKey("span.hostname"))
                {
                    settings.GlobalTags.Add("span.hostname", Environment.MachineName);
                }
            } else
            {
                log.Info("SKIPPED enrichment on init.");
            }

            // create a new Tracer using these settings
            var tracer = new Tracer(settings);

            // set the global tracer
            Tracer.Instance = tracer;
        }
    }
}
