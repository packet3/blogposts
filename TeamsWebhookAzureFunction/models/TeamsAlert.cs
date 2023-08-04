using Newtonsoft.Json;
using System.Collections.Generic;

namespace AzureAlerts.models
{

    public class Body
    {
        public string type { get; set; }
        public string weight { get; set; }
        public string style { get; set; }
        public string color { get; set; }
        public string text { get; set; }
        public List<Fact> facts { get; set; }
    }

    public class Fact
    {
        public string title { get; set; }
        public string value { get; set; }
    }

    public class Entity
    {
        public string type { get; set; }
        public string text { get; set; }
        public Mentioned mentioned { get; set; }
    }

    public class Mentioned
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class Msteams
    {
        public List<Entity> entities { get; set; }
        public string width { get; set; }
    }

    public class Content
    {
        public string type { get; set; }
        public List<Body> body { get; set; }

        [JsonProperty(PropertyName = "$schema")]
        public string schema { get; set; }
        public string version { get; set; }
        public Msteams msteams { get; set; }
    }

    public class Attachment
    {
        public string contentType { get; set; }
        public Content content { get; set; }
    }

    public class TeamsAlert
    {
        public string type { get; set; }
        public List<Attachment> attachments { get; set; }
    }
}