using System.Collections.Generic;
using ToDoClient.Models;

namespace ToDoClient.Infrastructure.Interfaces
{
    public interface IToDoService
    {
        IList<ToDoItemViewModel> GetItems(int userId);
        void CreateItem(ToDoItemViewModel item);
        void UpdateItem(ToDoItemViewModel item);
        void DeleteItem(int id);
    }
}
