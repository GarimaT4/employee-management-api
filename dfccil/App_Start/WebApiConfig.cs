using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Web.Http;

namespace dfccil
{
    public class CorsHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage();
            // Add CORS headers
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE");
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
            return base.SendAsync(request, cancellationToken);
        }
    }
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            // Configure Web API to use only bearer token authentication.
            //config.SuppressDefaultHostAuthentication();
            //config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            config.MessageHandlers.Add(new CorsHandler());

            var json = config.Formatters.JsonFormatter;
            json.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects;
            json.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;


           
            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/GetEmployees",
                defaults: new { id = RouteParameter.Optional }
            );
            config.Routes.MapHttpRoute(
               name: "AddEmployee",
               routeTemplate: "api/AddEmployee/{Empcode}/{Empname}/{fkDeptid}/{fkDesigId}/{basicsalary}/{ProfilePhotoUrl}",
               defaults: new { controller = "TestDB", action = "AddEmployee" }
           );
            config.Routes.MapHttpRoute(
                name: "UpdateEmpApi",
                routeTemplate: "api/UpdateEmployee/{pkEmpid}/{ECode}/{Ename}/{DeptId}/{DesigId}/{BasicSal}/{status}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }


}
