using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ToDoClient.Models;

namespace ToDoClient.Infrastructure
{
    public class RepositoryInfo
    {
        public List<int> UpdateIndexes { get; set; }

        public List<int> CreateIndexes { get; set; }

        public List<int> DeleteIndexes { get; set; }

        public List<ToDoItemViewModel> ToDoItems { get; set; }
    }
}