using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Gobiner.DotNetPad.Web.Models.Recaptcha;

namespace Gobiner.DotNetPad.Web.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static MvcHtmlString GenerateCaptcha(this HtmlHelper helper, Theme theme = Theme.Clean, string callBack = null)
        {
            const string htmlInjectString = @"<div id=""recaptcha_div""></div>
<script type=""text/javascript"">
    Recaptcha.create(""{0}"", ""recaptcha_div"", {{ theme: ""{1}"" {2}}});
</script>";

            var publicKey = ConfigurationManager.AppSettings["ReCaptcha.PublicKey"];

            if (string.IsNullOrWhiteSpace(publicKey))
                throw new InvalidKeyException("ReCaptcha.PublicKey missing from appSettings");

            if (!string.IsNullOrWhiteSpace(callBack))
                callBack = string.Concat(", callback: ", callBack);

            var html = string.Format(htmlInjectString, publicKey, theme.ToString().ToLower(), callBack);
            return MvcHtmlString.Create(html);
        }
    }
}