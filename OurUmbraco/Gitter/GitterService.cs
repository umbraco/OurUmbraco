using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using GitterSharp.Model;
using GitterSharp.Model.Requests;
using GitterSharp.Services;

namespace OurUmbraco.Gitter
{
    public class GitterService
    {
        private GitterApiService _gitterApi;
        private string _roomId;

        public GitterService()
        {
            _gitterApi = new GitterApiService(ConfigurationManager.AppSettings["GitterApiToken"]);
            _roomId = ConfigurationManager.AppSettings["GitterRoomId"];
        }

        public Task<IEnumerable<Message>> GetMessages(int numberOfMessages = 2)
        {
            return _gitterApi.GetRoomMessagesAsync(_roomId, new MessageRequest {Limit = numberOfMessages});
        }
    }
}
