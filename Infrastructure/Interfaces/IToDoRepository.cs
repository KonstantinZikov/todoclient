using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoClient.Models;

namespace ToDoClient.Infrastructure.Interfaces
{
    public interface IToDoRepository
    {
        IList<ToDoItemViewModel> GetItems(int userId);
        void UpdateItem(ToDoItemViewModel todo);
        void DeleteItem(int id);
        int CreateItem(ToDoItemViewModel todo);
    }
}
