//using Datadog.Trace;
//using Datadog.Trace.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Samples.AspNetMvc4.Controllers
{
    public class HomeController : Controller
    {

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
/*
            // access the active scope through the global tracer (can return null)
            var scope = Tracer.Instance.ActiveScope;

            Process currentProcessInfo = System.Diagnostics.Process.GetCurrentProcess();
            var startTime = currentProcessInfo.StartTime;
            TimeSpan startTimeSpan = (startTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
            long startTimeNanoSeconds = startTimeSpan.Ticks * 100;


            
            // add a tag to the span
            scope.Span.SetTag("span.pid", currentProcessInfo.Id.ToString());
            scope.Span.SetTag("span.starttime", startTimeNanoSeconds.ToString());
            scope.Span.SetTag("span.hostname", Environment.MachineName);
*/
            base.OnActionExecuting(filterContext);
        }

        public static string FullyQualifiedApplicationPath(HttpRequestBase httpRequestBase)
        {
            string appPath = string.Empty;

            if (httpRequestBase != null)
            {
                //Formatting the fully qualified website url/name
                appPath = string.Format("{0}://{1}{2}{3}",
                            httpRequestBase.Url.Scheme,
                            httpRequestBase.Url.Host,
                            httpRequestBase.Url.Port == 80 ? string.Empty : ":" + httpRequestBase.Url.Port,
                            httpRequestBase.ApplicationPath);
            }

            if (!appPath.EndsWith("/"))
            {
                appPath += "/";
            }

            return appPath;
        }

        public ActionResult Index()
        {
            var prefixes = new[] { "COR_", "CORECLR_", "DD_", "DATADOG_", "foo" };

            var envVars = from envVar in Environment.GetEnvironmentVariables().Cast<DictionaryEntry>()
                          from prefix in prefixes
                          let key = (envVar.Key as string)?.ToUpperInvariant()
                          let value = envVar.Value as string
                          where key.StartsWith(prefix)
                          orderby key
                          select new KeyValuePair<string, string>(key, value);

            Process currentProcessInfo = System.Diagnostics.Process.GetCurrentProcess();
            var startTime = currentProcessInfo.StartTime;

            TimeSpan startTimeSpan = (startTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
            var startTimeMilliseconds = Convert.ToUInt64(Math.Truncate(startTimeSpan.TotalMilliseconds));

            ViewBag.ProcessID = currentProcessInfo.Id;
            ViewBag.startTime = startTimeMilliseconds;
            ViewBag.machineName = Environment.MachineName;

            return View(envVars.ToList());
        }

        public ActionResult Send()
        {
            // extract propagated http headers
            // var headers = httpContext.Request.Headers.Wrap();
            // propagatedContext = SpanContextPropagator.Instance.Extract(headers);

            string headers = String.Empty;
            foreach (var key in Request.Headers.AllKeys)
                headers += key + "=" + Request.Headers[key] + "<br/>" + Environment.NewLine;
            return Content(string.Format("[SEND] {0} Hi there from Send() on box {1}! (Answered {2})", headers, Environment.MachineName, DateTime.Now));
        }

        public ActionResult Receive()
        {
            string responseText = String.Empty;
            string clientUrl= ConfigurationManager.AppSettings["SENDER_URL"];
            if (String.IsNullOrEmpty(clientUrl)) {
                clientUrl = HomeController.FullyQualifiedApplicationPath(HttpContext.Request);
            }

            HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(string.Format("{0}/Home/Send",clientUrl));
            webReq.Method = "GET";
            HttpWebResponse webResponse = (HttpWebResponse)webReq.GetResponse();

            WebHeaderCollection header = webResponse.Headers;
            var encoding = ASCIIEncoding.ASCII;
            using (var reader = new System.IO.StreamReader(webResponse.GetResponseStream(), encoding))
            {
                 responseText = reader.ReadToEnd();
            }

            return Content(String.Format("[RECEIVE] from {0}/Home/Send" + "<br/>" + Environment.NewLine + "{1}", clientUrl, responseText));
        }

        public ActionResult ReceiveWrapped()
        {
            string responseText = String.Empty;
            string clientUrl = ConfigurationManager.AppSettings["SENDER_URL"];
            if (String.IsNullOrEmpty(clientUrl))
            {
                clientUrl = HomeController.FullyQualifiedApplicationPath(HttpContext.Request);
            }

            string externalServiceName = ConfigurationManager.AppSettings["SENDER_NAME"];
            if (String.IsNullOrEmpty(externalServiceName))
            {
                externalServiceName = HomeController.FullyQualifiedApplicationPath(HttpContext.Request);
            }

            var currentServiceName = ConfigurationManager.AppSettings["DD_SERVICE_NAME"];

/*
            using (var scope = Tracer.Instance.StartActive("external_service_call", serviceName: externalServiceName))
            {

                HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(string.Format("{0}/Home/Send", clientUrl));
                webReq.Method = "GET";
                HttpWebResponse webResponse = (HttpWebResponse)webReq.GetResponse();

                WebHeaderCollection header = webResponse.Headers;
                var encoding = ASCIIEncoding.ASCII;
                using (var reader = new System.IO.StreamReader(webResponse.GetResponseStream(), encoding))
                {
                    responseText = reader.ReadToEnd();
                }
            }
*/
            var finalResponse = String.Format("[RECEIVE] {0}/Home/Send" + "<br/>" + Environment.NewLine + "{1}", clientUrl, responseText);
            return Content(finalResponse);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult BadRequest()
        {
            throw new Exception("Oops, it broke.");
        }
    }
}
