using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Samples.AspNetMvc4.Controllers
{
    public class HomeController : Controller
    {

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
            var prefixes = new[] { "COR_", "CORECLR_", "DD_", "DATADOG_" };

            var envVars = from envVar in Environment.GetEnvironmentVariables().Cast<DictionaryEntry>()
                          from prefix in prefixes
                          let key = (envVar.Key as string)?.ToUpperInvariant()
                          let value = envVar.Value as string
                          where key.StartsWith(prefix)
                          orderby key
                          select new KeyValuePair<string, string>(key, value);

            return View(envVars.ToList());
        }

        public ActionResult Send()
        {
            return Content(string.Format("Hi there from sender! ({0})", DateTime.Now));
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

            return Content(String.Format("Hello from consumer - in response to - {0}", responseText));
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
