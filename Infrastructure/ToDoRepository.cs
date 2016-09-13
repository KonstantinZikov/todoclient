using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Web;
using ToDoClient.Models;
using ToDoClient.Services;

namespace ToDoClient.Infrastructure
{
    public class ToDoRepository
    {
        private readonly ToDoService todoService = new ToDoService();
        private readonly List<ToDoItemViewModel> todoItems = new List<ToDoItemViewModel>();
        private readonly string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            "App_Data", ConfigurationManager.AppSettings["LocalStorageName"]);
        private readonly List<int> updateIndexes = new List<int>();
        private readonly List<int> createIndexes = new List<int>();
        private readonly List<int> deleteIndexes = new List<int>();

        public ToDoRepository()
        {
            var repositoryInfo = RestoreToDoItems();
            todoItems = repositoryInfo.ToDoItems;
            createIndexes = repositoryInfo.CreateIndexes;
            updateIndexes = repositoryInfo.UpdateIndexes;
            deleteIndexes = repositoryInfo.DeleteIndexes;
        }

        public IList<ToDoItemViewModel> GetItems(int userId)
        {
            return todoItems.Where(x => x.UserId == userId).ToList();
            //return todoService.GetItems(userId);
        }

        public  void UpdeteItem(ToDoItemViewModel todo)
        {
            var index = FindItem(todo.ToDoId);
            if(index != -1)
            {
                todoItems[index].IsCompleted = todo.IsCompleted;
                todoItems[index].Name = todo.Name;
            }
            //todoService.UpdateItem(todo);
        }

        public  void DeleteItem(int id)
        {
            var index = FindItem(id);
            if (index != -1)
            {
                todoItems.RemoveAt(index);
            }
            //todoService.DeleteItem(id);
        }

        public void CreateItem(ToDoItemViewModel todo)
        {
            todo.ToDoId = todoItems.Count+1;
            todoItems.Add(todo);
            //todoService.CreateItem(todo);
        }

        private void Commit()
        {
            var serializer = new DataContractJsonSerializer(typeof(RepositoryInfo));
            
            using (var stream = File.Create(path))
            {
                serializer.WriteObject(stream, new RepositoryInfo() {
                    ToDoItems = todoItems,
                    CreateIndexes = createIndexes,
                    UpdateIndexes = updateIndexes,
                    DeleteIndexes = deleteIndexes
                });
            }
        }

        private RepositoryInfo RestoreToDoItems()
        {
            var serializer = new DataContractJsonSerializer(typeof(RepositoryInfo));
            var result = default(RepositoryInfo);

            using (var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read))
            {
                result = (RepositoryInfo) serializer.ReadObject(stream);
            }

            return result;          
        }

        private int FindItem(int itemId)
        {
            var index = -1;
            for (int i = 0; i < todoItems.Count; i++)
            {
                if (todoItems[i].ToDoId == itemId)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }
    }
}