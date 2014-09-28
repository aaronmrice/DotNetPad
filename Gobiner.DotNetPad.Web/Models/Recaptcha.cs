using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Gobiner.DotNetPad.Web.Models.Recaptcha
{
    public enum Theme { Red, White, BlackGlass, Clean }

    [Serializable]
    public class InvalidKeyException : ApplicationException
    {
        public InvalidKeyException() { }
        public InvalidKeyException(string message) : base(message) { }
        public InvalidKeyException(string message, Exception inner) : base(message, inner) { }
    }

    public class ReCaptchaAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var userIP = filterContext.RequestContext.HttpContext.Request.UserHostAddress;

            var privateKey = ConfigurationManager.AppSettings["ReCaptcha.PrivateKey"];

            if (string.IsNullOrWhiteSpace(privateKey))
                throw new InvalidKeyException("ReCaptcha.PrivateKey missing from appSettings");

            var postData = string.Format("&privatekey={0}&remoteip={1}&challenge={2}&response={3}",
                                         privateKey,
                                         userIP,
                                         filterContext.RequestContext.HttpContext.Request.Form["recaptcha_challenge_field"],
                                         filterContext.RequestContext.HttpContext.Request.Form["recaptcha_response_field"]);

            var postDataAsBytes = Encoding.UTF8.GetBytes(postData);

            // Create web request
            var request = WebRequest.Create("http://www.google.com/recaptcha/api/verify");
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postDataAsBytes.Length;
            var dataStream = request.GetRequestStream();
            dataStream.Write(postDataAsBytes, 0, postDataAsBytes.Length);
            dataStream.Close();

            // Get the response.
            var response = request.GetResponse();

            using (dataStream = response.GetResponseStream())
            {
                using (var reader = new StreamReader(dataStream))
                {
                    var responseFromServer = reader.ReadToEnd();

                    if (!responseFromServer.StartsWith("true"))
                        ((Controller)filterContext.Controller).ModelState.AddModelError("ReCaptcha", "Captcha words typed incorrectly");
                }
            }
        }
    }
}