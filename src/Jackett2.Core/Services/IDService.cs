using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jackett2.Core.Services
{
    public interface IIDService
    {
        string NewId();
    }

    public class IDService : IIDService
    {
        public string NewId()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}
