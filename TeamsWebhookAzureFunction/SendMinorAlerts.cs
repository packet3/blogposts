using Azure;
using Azure.Data.Tables;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using AzureAlerts.enums;
using AzureAlerts.models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AzureAlerts
{
    public class TeamContact : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public string ADUsername { get; set; }
        public string TeamMember { get; set; }
    }
    public static class SendMinorAlerts
    {


        [FunctionName("SendMinorAlertsToTeamsWebhook")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
           ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // Define Needed Variables
            SecretClient client = null;
            var storageConnectionStringsecret = "";
            var teamsWebHookSecret = "";


            var isDevelopment = string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "development", StringComparison.InvariantCultureIgnoreCase);
            if (isDevelopment)
            {
                client = new SecretClient(vaultUri: new Uri("https://kclkeyjoetest.vault.azure.net"), credential: new VisualStudioCredential());

                var storageConnectionKey = client.GetSecretAsync("AzureAlertsStorageAccount").Result;
                var teamsWebHook = client.GetSecretAsync("TeamsWebhook").Result;

                storageConnectionStringsecret = storageConnectionKey.Value.Value.ToString();
                teamsWebHookSecret = teamsWebHook.Value.Value.ToString();

            }
            else
            {
                var storageConnectionKey = System.Environment.GetEnvironmentVariable("AzureAlertStorageAccount");
                var teamsWebHook = System.Environment.GetEnvironmentVariable("TeamsWebhook");

                storageConnectionStringsecret = storageConnectionKey.ToString();
                teamsWebHookSecret = teamsWebHook.ToString();
            }

            var tableServiceClient = new TableServiceClient(storageConnectionStringsecret);

            var tableClient = tableServiceClient.GetTableClient("TeamContacts");
            AsyncPageable<TeamContact> teamContacts = tableClient.QueryAsync<TeamContact>();

            var entities = new List<Entity>();

            var mentionNames = "Hi ";

            await foreach (TeamContact entity in teamContacts)
            {
                entities.Add(new Entity
                {
                    type = "mention",
                    text = $"<at>{entity.TeamMember}</at>",
                    mentioned = new Mentioned
                    {
                        id = entity.ADUsername,
                        name = entity.TeamMember
                    }
                });

                mentionNames += $"<at>{entity.TeamMember}</at>, ";

            }




            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            RootObject root = JsonConvert.DeserializeObject<RootObject>(requestBody);

            AzureAlert azureAlert = new AzureAlert();

            // Safety checking if null using null-conditional operator and null-coalescing operator
            azureAlert.AlertDescription = root.Data?.Essentials?.Description ?? "Description not available";
            azureAlert.ItemName = root.Data?.Essentials?.ConfigurationItems?.FirstOrDefault() ?? "Rule name not available";
            azureAlert.RuleName = root.Data?.Essentials?.AlertRule ?? "Item name not available";
            azureAlert.Severity = root.Data?.Essentials?.Severity ?? "Severity not available";
            azureAlert.MonitorCondition = root.Data?.Essentials?.MonitorCondition ?? "Alert Condition Not Avaliable";

            TeamsAlert teamsAlert = new TeamsAlert()
            {
                type = "message",
                attachments = new List<Attachment>
            {
                new Attachment
                {
                    contentType = "application/vnd.microsoft.card.adaptive",
                    content = new Content
                    {
                        type = "AdaptiveCard",
                        body = new List<Body>
                        {
                            new Body
                            {
                                type = "TextBlock",
                                weight = "Bolder",
                                style = "heading",
                                color = "Warning",
                                text = "Minor Azure Alert has been triggered."
                            },
                            new Body
                            {
                                type = "TextBlock",
                                text = mentionNames
                            },
                            new Body
                            {
                                type = "TextBlock",
                                text = "The Following alert has been triggered: "
                            },
                            new Body
                            {
                                type = "FactSet",
                                facts = new List<Fact>
                                {
                                    new Fact { title = "Alert Name:", value = azureAlert.RuleName },
                                    new Fact { title = "Alert Desc:", value = azureAlert.AlertDescription },
                                    new Fact { title = "Alert Severity:", value = azureAlert.Severity },
                                    new Fact { title = "Azure Resource", value = azureAlert.ItemName }
                                }
                            }
                        },
                        schema = "http://adaptivecards.io/schemas/adaptive-card.json",
                        version = "1.0",
                        msteams = new Msteams
                        {
                            width = "Full",
                            entities = entities

                        }
                    }
                }
            }
            };



            ////Only send alert if it's been fired
            if (azureAlert.MonitorCondition == MonitorCondition.Fired.ToString())
            {
                // Serialize to JSON
                string jsonPayload = JsonConvert.SerializeObject(teamsAlert, Formatting.Indented);


                using (HttpClient httpClient = new HttpClient())
                {

                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(teamsWebHookSecret, content);

                    return new OkObjectResult("Message sent to Teams channel!");
                }
            }

            return new OkObjectResult("Function Triggered, Webhook not fired");




        }
    }
}

