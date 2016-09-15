using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ToDoClient.Models;

namespace ToDoClient.Infrastructure
{
    public class RepositoryInfo
    {
        public Dictionary<int, int> UpdateIndexes { get; set; }

        public Dictionary<int, int> CreateIndexes { get; set; }

        public Dictionary<int, int> DeleteIndexes { get; set; }

        public Dictionary<int, ToDoItemViewModel> ToDoItems { get; set; }

        public int NextId { get; set; }

        public int UpdateId { get; set; }
    }
}