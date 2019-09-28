using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GreenShade.WebApi.WS.Controllers
{
    [Route("api/[controller]")]
    public class ImageDataController : Controller
    {
        public class PutRequest
        {
            public string Image { get; set; }
        }

        [HttpPut("{id}")]
        public async void Put(string id, [FromBody]PutRequest imageData)
        {
            var data = Convert.FromBase64String(imageData.Image);

            var socketconnectionList = ImageReceiverWebSocketMiddleware.Connections.Where(x => x.DeviceId.Equals(id)).ToArray();

            foreach (var socketconnection in socketconnectionList)
            {
                var socket = socketconnection.SocketConnection;
                if (socket.State == System.Net.WebSockets.WebSocketState.Open)
                {
                    var type = WebSocketMessageType.Text;
                    var buffer = new ArraySegment<Byte>(Encoding.ASCII.GetBytes(imageData.Image));
                    try
                    {
                        await socket.SendAsync(buffer, type, true, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        
                    }
                }
                else
                {
                    ImageReceiverWebSocketMiddleware.Connections.Remove(socketconnection);
                }
            }
        }
    }
}