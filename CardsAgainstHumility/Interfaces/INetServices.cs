using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardsAgainstHumility.Interfaces
{
    public interface INetServices
    {
        Task<string> JsonRequestAsync(Method method, string host, string route, object param, bool expectResponse = true);

        ISocketManager GetSocketManager();
    }
}
