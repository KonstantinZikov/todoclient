using System.Collections.Generic;
using System.Threading;
using System.Web.Http;
using ToDoClient.Infrastructure;
using ToDoClient.Infrastructure.Filters;
using ToDoClient.Models;
using ToDoClient.Services;

namespace ToDoClient.Controllers
{
    /// <summary>
    /// Processes sync requests.
    /// </summary>
    /// <returns>The list of todo-items.</returns>
    public class SyncController : ApiController
    {
        public readonly UserService userService = new UserService();

        [SyncNotAvailable]
        public IList<ToDoItemViewModel> Get()
        {
            var userId = userService.GetOrCreateUser();
            return ToDosController.todoRepository.Sync(userId);
        }

        public IList<ToDoItemViewModel> Wait()
        {
            while (ToDoRepository.IsSyncronizing)
                Thread.Sleep(1000);
            var userId = userService.GetOrCreateUser();
            return ToDosController.todoRepository.GetItems(userId);
        }
    }
}
