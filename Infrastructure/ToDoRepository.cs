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
        private readonly Dictionary<int,ToDoItemViewModel> todoItems = new Dictionary<int, ToDoItemViewModel>();
        private readonly string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            "App_Data", ConfigurationManager.AppSettings["LocalStorageName"]);
        private readonly Dictionary<int, int> updateIndexes = new Dictionary<int, int>();
        private readonly Dictionary<int, int> createIndexes = new Dictionary<int, int>();
        private readonly Dictionary<int, int> deleteIndexes = new Dictionary<int, int>();
        private int nextId;

        public ToDoRepository()
        {
            var repositoryInfo = RestoreToDoItems();
            todoItems = repositoryInfo.ToDoItems;
            createIndexes = repositoryInfo.CreateIndexes;
            updateIndexes = repositoryInfo.UpdateIndexes;
            deleteIndexes = repositoryInfo.DeleteIndexes;
            nextId = repositoryInfo.NextId;
        }

        public IList<ToDoItemViewModel> GetItems(int userId)
        {
            var result = new List<ToDoItemViewModel>();
            foreach(var pair in todoItems)
            {
                if (!deleteIndexes.Keys.Contains(pair.Key) && pair.Value.UserId == userId){
                    result.Add(new ToDoItemViewModel()
                    {
                        ToDoId = pair.Key,
                        IsCompleted = pair.Value.IsCompleted,
                        Name = pair.Value.Name,
                        UserId = pair.Value.UserId
                    });
                }
            }
            return result;
        }

        public void UpdateItem(ToDoItemViewModel todo)
        {
            Task.Factory.StartNew(() =>
            {
                while (createIndexes.Count != 0)
                    Thread.Sleep(1000);
                   
                todoItems[todo.ToDoId].IsCompleted = todo.IsCompleted;
                todoItems[todo.ToDoId].Name = todo.Name;
                updateIndexes.Add(todo.ToDoId, todo.ToDoId);
                Commit();

                todoService.UpdateItem(todoItems[todo.ToDoId]);
                updateIndexes.Remove(todo.ToDoId);
                Commit();
            });
        }

        public void DeleteItem(int id)
        {
            Task.Factory.StartNew(() =>
            {
                deleteIndexes.Add(id,id);           
                Commit();

                while (createIndexes.Count != 0)
                    Thread.Sleep(1000);
                
                todoService.DeleteItem(todoItems[id].ToDoId);
                todoItems.Remove(id);

                deleteIndexes.Remove(id);
                Commit();
            });    
        }

        public int CreateItem(ToDoItemViewModel todo)
        {
            var index = nextId++;
            createIndexes.Add(index,index);
            todoItems.Add(index,todo);

            Task.Factory.StartNew(() => 
            {
                todoService.CreateItem(todo);
                var serverTodos = todoService.GetItems(todo.UserId);
                var last = serverTodos.Last();
                todoItems[index].ToDoId = last.ToDoId;
                createIndexes.Remove(index);
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
                if (new FileInfo(path).Length == 0)
                {
                    result = new RepositoryInfo()
                    {
                        CreateIndexes = new Dictionary<int, int>(),
                        UpdateIndexes = new Dictionary<int, int>(),
                        DeleteIndexes = new Dictionary<int, int>(),
                        ToDoItems = new Dictionary<int, ToDoItemViewModel>(),
                        NextId = 1
                    };
                }
                else
                {
                    result = serializer.ReadObject(stream) as RepositoryInfo ?? new RepositoryInfo();
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
    }
}