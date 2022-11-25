using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace OurUmbraco.Community.Meetup
{
    public class CosmosService
    {
        public async Task<ItemResponse<Skybrud.Social.Meetup.Models.GraphQl.Events.MeetupEvent>> CreateMeetup(Skybrud.Social.Meetup.Models.GraphQl.Events.MeetupEvent meetup)
        {
            var key = ConfigurationManager.AppSettings["CosmosAuthKey"];
            using(var client = new CosmosClient("https://our-umbraco.documents.azure.com:443/", key)) 
            {
            
                var container = client.GetContainer("ourumbracocache", "ourcache");
                var create = await container.UpsertItemAsync(meetup);
                return create;
            }
        }
    }
}