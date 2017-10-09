﻿using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace ShoeStore.ProductsApi
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Trace.TraceInformation("Application started");
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //https://stackoverflow.com/questions/29595034/how-to-set-machinekey-on-azure-website
            var mksType = typeof(MachineKeySection);
            var mksSection = ConfigurationManager.GetSection("system.web/machineKey") as MachineKeySection;
            var resetMethod = mksType.GetMethod("Reset", BindingFlags.NonPublic | BindingFlags.Instance);

            var newConfig = new MachineKeySection();
            newConfig.ApplicationName = mksSection.ApplicationName;
            newConfig.CompatibilityMode = mksSection.CompatibilityMode;
            newConfig.DataProtectorType = mksSection.DataProtectorType;
            newConfig.Validation = mksSection.Validation;

            newConfig.ValidationKey = ConfigurationManager.AppSettings["MK_ValidationKey"];
            newConfig.DecryptionKey = ConfigurationManager.AppSettings["MK_DecryptionKey"];
            newConfig.Decryption = "AES";
            newConfig.ValidationAlgorithm = "HMACSHA256";

            resetMethod.Invoke(mksSection, new object[] { newConfig });
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exc = Server.GetLastError();
            Trace.TraceError("Error occurred:" + exc.Message);

            var telemetry = new TelemetryClient();
            telemetry.TrackException(exc);
            throw exc;
        }

        protected void Application_EndRequest(Object sender, EventArgs e)
        {
            HttpApplication context = (HttpApplication)sender;
            context.Response.SuppressFormsAuthenticationRedirect = true;
        }
    }
}
