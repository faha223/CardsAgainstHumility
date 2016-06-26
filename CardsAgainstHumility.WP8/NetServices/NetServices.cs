using CardsAgainstHumility.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace CardsAgainstHumility.WP8.NetServices
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
                var baseAddress = new Uri($"{host}");
                var requestUri = new Uri(baseAddress, route);

                HttpResponseMessage response = null;

                switch (method)
                {
                    case Method.GET:
                        response = await client.GetAsync(requestUri);
                        break;
                    case Method.POST:
                        if (expectResponse)
                            response = await client.PostAsync(requestUri, new HttpStringContent(content, UnicodeEncoding.Utf8, "application/json"));
                        else
                            client.PostAsync(requestUri, new HttpStringContent(content, UnicodeEncoding.Utf8, "application/json")).AsTask().Wait();
                        break;
                    default:
                        return null;
                }

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                return null;
            }
        }
    }
}
