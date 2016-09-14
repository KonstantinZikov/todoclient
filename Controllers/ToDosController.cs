using System.Collections.Generic;
using System.Web.Http;
using ToDoClient.Infrastructure;
using ToDoClient.Infrastructure.Interfaces;
using ToDoClient.Models;
using ToDoClient.Services;

namespace ToDoClient.Controllers
{
    /// <summary>
    /// Processes todo requests.
    /// </summary>
    public class ToDosController : ApiController
    {
        private readonly IUserService userService;
        private readonly IToDoRepository todoRepository;

        public ToDosController(IUserService userService, IToDoRepository todoRepository)
        {
            this.userService = userService;
            this.todoRepository = todoRepository;
        }

        /// <summary>
        /// Returns all todo-items for the current user.
        /// </summary>
        /// <returns>The list of todo-items.</returns>
        public IList<ToDoItemViewModel> Get()
        {
            var userId = userService.GetOrCreateUser();
            return todoRepository.GetItems(userId);
        }

        /// <summary>
        /// Updates the existing todo-item.
        /// </summary>
        /// <param name="todo">The todo-item to update.</param>
        public void Put(ToDoItemViewModel todo)
        {
            todo.UserId = userService.GetOrCreateUser();
            todoRepository.UpdateItem(todo);
        }

        /// <summary>
        /// Deletes the specified todo-item.
        /// </summary>
        /// <param name="id">The todo item identifier.</param>
        public void Delete(int id)
        {
            todoRepository.DeleteItem(id);
        }

        /// <summary>
        /// Creates a new todo-item.
        /// </summary>
        /// <param name="todo">The todo-item to create.</param>
        public int Post(ToDoItemViewModel todo)
        {
            todo.UserId = userService.GetOrCreateUser();
            return todoRepository.CreateItem(todo);
        }
    }
}
