using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;
using ToDoClient.Infrastructure.Interfaces;
using ToDoClient.Models;

namespace ToDoClient.Infrastructure
{
    public class ToDoRepository: IToDoRepository
    {
        private readonly IToDoService todoService;
        private readonly Dictionary<int, ToDoItemViewModel> todoItems = new Dictionary<int, ToDoItemViewModel>();
        private readonly string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            "App_Data", ConfigurationManager.AppSettings["LocalStorageName"]);
        private readonly Dictionary<int, int> updateIndexes = new Dictionary<int, int>();
        private readonly Dictionary<int, int> createIndexes = new Dictionary<int, int>();
        private readonly Dictionary<int, int> deleteIndexes = new Dictionary<int, int>();
        private int nextId;
        private int updateId = 1;
		public static bool IsSyncronizing;

        private object commitLock = new object();
        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(RepositoryInfo));

        public ToDoRepository(IToDoService todoService)
        {
            var repositoryInfo = RestoreToDoItems();
            todoItems = repositoryInfo.ToDoItems;
            createIndexes = repositoryInfo.CreateIndexes;
            updateIndexes = repositoryInfo.UpdateIndexes;
            deleteIndexes = repositoryInfo.DeleteIndexes;
            nextId = repositoryInfo.NextId;
            this.todoService = todoService;
        }

        public IList<ToDoItemViewModel> GetItems(int userId)
        {
            var result = new List<ToDoItemViewModel>();
            foreach (var pair in todoItems)
            {
                if (!deleteIndexes.Keys.Contains(pair.Key) && pair.Value.UserId == userId)
                {
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

        public IList<ToDoItemViewModel> Sync(int userId)
        {
            IsSyncronizing = true;
            try
            {
                var serverData = todoService.GetItems(userId);
                var localData = todoItems.Values;
                var deleteList = new List<int>();
                var addList = new List<ToDoItemViewModel>();
                // search new data from service
                foreach (var todo in serverData)
                {
                    if (localData.FirstOrDefault((t) => t.ToDoId == todo.ToDoId)
                        == default(ToDoItemViewModel))
                    {
                        addList.Add(todo);
                    }
                }
                // search data 
                foreach (var pair in todoItems)
                {
                    if (serverData.FirstOrDefault((t) => t.ToDoId == pair.Value.ToDoId)
                        == default(ToDoItemViewModel))
                    {
                        deleteList.Add(pair.Key);
                    }
                }

                foreach (var key in deleteList)
                {
                    todoItems.Remove(key);
                }
                foreach (var todo in addList)
                {
                    todo.Name = todo.Name.Trim();
                    todoItems.Add(nextId++, todo);
                }
                Commit();
                return GetItems(userId);
            }
            finally
            {
                IsSyncronizing = false;
            }
        }

        public void UpdateItem(ToDoItemViewModel todo)
        {
            Task.Factory.StartNew(() =>
            {
                var currentUpdateId = 0;
                lock (commitLock)
                {
                    todoItems[todo.ToDoId].IsCompleted = todo.IsCompleted;
                    todoItems[todo.ToDoId].Name = todo.Name;
                    currentUpdateId = updateId++;
                    updateIndexes.Add(currentUpdateId, todo.ToDoId);
                }
                Commit();

                while (todoItems[todo.ToDoId].ToDoId == -1)
                    Thread.Sleep(1000);

                todoService.UpdateItem(todoItems[todo.ToDoId]);
                lock (commitLock) updateIndexes.Remove(currentUpdateId);
                Commit();
            });
        }

        public void DeleteItem(int id)
        {
            Task.Factory.StartNew(() =>
            {
                lock (commitLock) deleteIndexes.Add(id, id);
                Commit();

                while (todoItems[id].ToDoId == -1)
                    Thread.Sleep(1000);

                todoService.DeleteItem(todoItems[id].ToDoId);
                lock (commitLock)
                {
                    todoItems.Remove(id);
                    deleteIndexes.Remove(id);
                }
                Commit();
            });
        }

        public int CreateItem(ToDoItemViewModel todo)
        {
            var index = nextId++;
            lock (commitLock)
            {
                createIndexes.Add(index, index);
                todoItems.Add(index, todo);
            }
            Task.Factory.StartNew(() =>
            {
                var sendingTodo = new ToDoItemViewModel()
                {
                    ToDoId = -1,
                    IsCompleted = todo.IsCompleted,
                    UserId = todo.UserId,
                    Name = index + ":" + todo.Name
                };
                todoService.CreateItem(sendingTodo);
                var serverTodos = todoService.GetItems(todo.UserId);
                // It is more probably, that the value will be at the end.
                for (int i = serverTodos.Count - 1; i >= 0; i--)
                {
                    var parts = serverTodos[i].Name.Split(':');
                    if (parts.Length > 1)
                    {
                        int id;
                        if (int.TryParse(parts[0], out id))
                        {
                            if (id == index)
                            {
                                sendingTodo.ToDoId = serverTodos[i].ToDoId;
                                break;
                            }
                        }
                    }
                }
                lock (commitLock)
                {
                    createIndexes.Remove(index);
                    todo.ToDoId = sendingTodo.ToDoId;
                }
                todoService.UpdateItem(todo);               
                Commit();
            });

            Commit();
            return index;
        }
        /// <summary>
        /// Enables to save repository state to local storage
        /// </summary>
        private void Commit()
        {
            lock (commitLock)
            {
                using (var stream = File.Create(path))
                {
                    serializer.WriteObject(stream, new RepositoryInfo()
                    {
                        ToDoItems = todoItems,
                        CreateIndexes = createIndexes,
                        UpdateIndexes = updateIndexes,
                        DeleteIndexes = deleteIndexes
                    });
                }
            }
        }

        /// <summary>
        /// Restores repository state and synchronizes data with the cloud
        /// </summary>
        /// <returns></returns>
        private RepositoryInfo RestoreToDoItems()
        {
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
                foreach (var item in result.CreateIndexes)
                {
                    todoService.CreateItem(result.ToDoItems[item.Key]);
                }
                foreach (var item in result.UpdateIndexes)
                {
                    todoService.UpdateItem(result.ToDoItems[item.Value]);
                }
                foreach (var item in result.DeleteIndexes)
                {
                    todoService.DeleteItem(result.ToDoItems[item.Key].ToDoId);
                    result.ToDoItems.Remove(item.Key);
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