using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ToDoClient.Infrastructure.Interfaces
{
    public interface IUserService
    {
        int CreateUser(string userName);
        int GetOrCreateUser();
    }
}