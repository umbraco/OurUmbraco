using Newtonsoft.Json;
using System;

namespace OurUmbraco.Documentation.Models
{
    public class AzureDevopsPayload
    {
        [JsonProperty("subscriptionId")]
        public Guid SubscriptionId { get; set; }

        [JsonProperty("notificationId")]
        public long NotificationId { get; set; }

        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("eventType")]
        public string EventType { get; set; }

        [JsonProperty("publisherId")]
        public string PublisherId { get; set; }

        [JsonProperty("message")]
        public object Message { get; set; }

        [JsonProperty("detailedMessage")]
        public object DetailedMessage { get; set; }

        [JsonProperty("resource")]
        public AzureDevopsResource Resource { get; set; }

        [JsonProperty("resourceVersion")]
        public string ResourceVersion { get; set; }

        [JsonProperty("resourceContainers")]
        public AzureDevopsResourceContainers ResourceContainers { get; set; }

        [JsonProperty("createdDate")]
        public DateTimeOffset CreatedDate { get; set; }
    }

    public partial class AzureDevopsResource
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }
    }

    public partial class AzureDevopsResourceContainers
    {
        [JsonProperty("collection")]
        public AzureDevopsAccount Collection { get; set; }

        [JsonProperty("account")]
        public AzureDevopsAccount Account { get; set; }

        [JsonProperty("project")]
        public AzureDevopsAccount Project { get; set; }
    }

    public partial class AzureDevopsAccount
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("baseUrl")]
        public Uri BaseUrl { get; set; }
    }
}
