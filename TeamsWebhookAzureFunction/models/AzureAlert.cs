using System.Collections.Generic;

namespace AzureAlerts.models
{
    internal class AzureAlert
    {
        public string RuleName { get; set; }
        public string Severity { get; set; }
        public string AlertDescription { get; set; }
        public string ItemName { get; set; }
        public string MonitorCondition { get; set; }
    }
    internal class Essentials
    {
        public string Description { get; set; }
        public string AlertRule { get; set; }
        public string Severity { get; set; }
        public string MonitorCondition { get; set; }
        public List<string> ConfigurationItems { get; set; }
    }

    internal class Data
    {
        public Essentials Essentials { get; set; }
    }
    internal class RootObject
    {
        public Data Data { get; set; }
    }
}
