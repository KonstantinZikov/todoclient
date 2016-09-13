using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;
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
        }

        public  void UpdeteItem(ToDoItemViewModel todo)
        {
            Task.Factory.StartNew(() =>
            {
                while (createIndexes.Count != 0)
                    Thread.Sleep(1000);
                   
                todoItems[todo.ToDoId].IsCompleted = todo.IsCompleted;
                todoItems[todo.ToDoId].Name = todo.Name;
                updateIndexes.Add(todoItems[todo.ToDoId].ToDoId);
                Commit();

                todoService.UpdateItem(todoItems[todo.ToDoId]);
                updateIndexes.Clear();
                Commit();
            });
        }

        public  void DeleteItem(int id)
        {
            Task.Factory.StartNew(() =>
            {
                while (createIndexes.Count != 0)
                    Thread.Sleep(1000);
                deleteIndexes.Add(todoItems[id].ToDoId);
                todoItems.RemoveAt(id);
                Commit();

                todoService.DeleteItem(id);
                deleteIndexes.Clear();
                Commit();
            });    
        }

        public int CreateItem(ToDoItemViewModel todo)
        {
            var index = todoItems.Count;
            createIndexes.Add(todoItems.Count);
            todoItems.Add(todo);

            Task.Factory.StartNew(() => 
            {
                todoService.CreateItem(todo);
                var serverTodos = todoService.GetItems(todo.UserId);
                for (int i = 0; i < createIndexes.Count; i++)
                {
                    todoItems[createIndexes[i]].ToDoId = serverTodos[createIndexes[i]].ToDoId;
                }
                createIndexes.Clear();
                Commit();
            });

            Commit();
            return index;
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
                try
                {
                    result = serializer.ReadObject(stream) as RepositoryInfo ?? new RepositoryInfo();
                }
                catch (SerializationException ex)
                {
                    result = new RepositoryInfo()
                    {
                        CreateIndexes = new List<int>(),
                        UpdateIndexes = new List<int>(),
                        DeleteIndexes = new List<int>(),
                        ToDoItems = new List<ToDoItemViewModel>()
                    };
                }
            }
            Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < result.CreateIndexes.Count; i++)
                {
                    todoService.CreateItem(result.ToDoItems[result.CreateIndexes[i]]);
                }

                for (int i = 0; i < result.UpdateIndexes.Count; i++)
                {
                    todoService.UpdateItem(result.ToDoItems[result.UpdateIndexes[i]]);
                }

                for (int i = 0; i < result.DeleteIndexes.Count; i++)
                {
                    todoService.DeleteItem(result.ToDoItems[result.DeleteIndexes[i]].ToDoId);
                }
                createIndexes.Clear();
                updateIndexes.Clear();
                deleteIndexes.Clear();
                Commit();
            });

            return result;          
        }

        //private int FindItem(int itemId)
        //{
        //    var index = -1;
        //    for (int i = 0; i < todoItems.Count; i++)
        //    {
        //        if (todoItems[i].ToDoId == itemId)
        //        {
        //            index = i;
        //            break;
        //        }
        //    }
        //    return index;
        //}
    }
}