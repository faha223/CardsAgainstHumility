using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CardsAgainstHumility.Interfaces;
using Newtonsoft.Json;
using System.Net.Http;

namespace CardsAgainstHumility.Android.NetServices
{
    class NetServices : INetServices
    {
        public ISocketManager GetSocketManager()
        {
            return new SocketManagement.SocketManager();
        }

        public async Task<string> JsonRequestAsync(Method method, string host, string route, object param, bool expectResponse = true)
        {
            string content = null;
            if (method == Method.POST && param != null)
            {
                content = JsonConvert.SerializeObject(param);
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri($"{host}");

                HttpResponseMessage response = null;

                switch (method)
                {
                    case Method.GET:
                        response = await client.GetAsync(route).ConfigureAwait(false);
                        break;
                    case Method.POST:
                        if (expectResponse)
                            response = await client.PostAsync(route, new StringContent(content, Encoding.UTF8, "application/json")).ConfigureAwait(false);
                        else
                            client.PostAsync(route, new StringContent(content, Encoding.UTF8, "application/json")).Wait();
                        break;
                    default:
                        return null;
                }

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                return null;
            }
        }
    }
}