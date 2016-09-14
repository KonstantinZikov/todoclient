using System.Collections.Generic;
using System.Threading;
using System.Web.Http;
using ToDoClient.Infrastructure;
using ToDoClient.Infrastructure.Filters;
using ToDoClient.Infrastructure.Interfaces;
using ToDoClient.Models;

namespace ToDoClient.Controllers
{
    /// <summary>
    /// Processes sync requests.
    /// </summary>
    /// <returns>The list of todo-items.</returns>
    public class SyncController : ApiController
    {
        private readonly IUserService userService;
        private readonly IToDoRepository todoRepository;

        public SyncController(IUserService userService, IToDoRepository todoRepository)
        {
            this.userService = userService;
            this.todoRepository = todoRepository;
        }

        [SyncNotAvailable]
        public IList<ToDoItemViewModel> Get()
        {
            var userId = userService.GetOrCreateUser();
            return todoRepository.Sync(userId);
        }

        public IList<ToDoItemViewModel> Wait()
        {
            while (ToDoRepository.IsSyncronizing)
                Thread.Sleep(1000);
            var userId = userService.GetOrCreateUser();
            return todoRepository.GetItems(userId);
        }
    }
}
