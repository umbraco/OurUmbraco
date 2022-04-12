namespace OurUmbraco.Community.Nuget
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public class NugetRegistrationItem
    {
        /// <summary>
        ///  when there are multiple pages of packages then this contains the URL that will get you that page of items.
        /// </summary>
        [JsonProperty("@Id")]
        public string Id { get; set; }

        /// <summary>
        ///  when all items are returned in one list, this is full of items.
        /// </summary>
        [JsonProperty("items")]
        public IEnumerable<NugetRegistrationItemEntry> Items { get; set; }
    }
}
