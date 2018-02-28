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

        public GitterService()
        {
            _gitterApi = new GitterApiService(ConfigurationManager.AppSettings["GitterApiToken"]);
        }

        public Task<IEnumerable<Message>> GetMessages(string roomId, int numberOfMessages = 10)
        {
            //TODO: Clean & sanity check the room ID of a whitelist of rooms from a gitter.json config file or something
            //Otherwise open for abuse?
            return _gitterApi.GetRoomMessagesAsync(roomId, new MessageRequest {Limit = numberOfMessages});
        }

        
        public Task<Room> GetRoomInfo(string roomName)
        {
            //TODO: Perhaps clean & sanity check with our whitelist of rooms in appsetting or json file?!
            //Otherwise open for abuse?
            return _gitterApi.GetRoomAsync(roomName);
        }
    }
}
