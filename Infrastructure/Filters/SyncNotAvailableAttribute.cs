using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace ToDoClient.Infrastructure.Filters
{
    // Applies to actions, that not available when repository is syncronizing.
    public class SyncNotAvailableAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (ToDoRepository.IsSyncronizing)
            {
                actionContext.Response = actionContext.Request.CreateResponse("Sync");
            }
        }
    }
}