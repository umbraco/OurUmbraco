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

            //NOTE: The method of Join Room - is a bit confusing here from GitterSharp lib
            //It's because you use two JoinRoom methods - this one to fetch the room based on its URI 'umbraco/playground'
            //Then another JoinRoom method is used where you use the Room GUID id from this request to join a user to a room
            return _gitterApi.JoinRoomAsync(roomName);
        }
    }
}
