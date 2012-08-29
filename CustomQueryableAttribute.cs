using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;

namespace TestWebApplication.Infrastructure
{
    public class CustomQueryableAttribute : QueryableAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {            
            var inlinecount = HttpUtility.ParseQueryString(actionExecutedContext.Request.RequestUri.Query).Get("$inlinecount");
            
            base.OnActionExecuted(actionExecutedContext);

            if (inlinecount == "allpages" && ResponseIsValid(actionExecutedContext.Response))
            {
                object responseObject;
                actionExecutedContext.Response.TryGetContentValue(out responseObject);

                if (responseObject is IQueryable)
                {
                    var robj = responseObject as IQueryable<object>;
                    long? newsize = robj.Count();
                    
                    if (newsize != null)
                    {
                        actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(HttpStatusCode.OK, new ODataMetadata<object>(robj, newsize));
                    }
                }
            }
        }

        public override void ValidateQuery(HttpRequestMessage request)
        {
            //everything is allowed
        }

        private bool ResponseIsValid(HttpResponseMessage response)
        {
            if (response == null || response.StatusCode != HttpStatusCode.OK || !(response.Content is ObjectContent)) return false;
            return true;
        }
    }
}