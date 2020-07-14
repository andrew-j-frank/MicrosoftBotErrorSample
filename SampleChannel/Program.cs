using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Bot.Schema;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace SampleChannel
{
    class Program
    {
        private static string clientId = "BOT_MICROSOFT_APP_ID";
        private static string clientSecret = "BOT_MICROSOFT_APP_PASSWORD";
        static async Task Main(string[] args)
        {
            while (true)
            {
                var authClient = new HttpClient();
                var authRequest = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri($"https://login.microsoftonline.com/botframework.com/oauth2/v2.0/token"),
                    Content = new StringContent($"grant_type=client_credentials&client_id={clientId}&client_secret={clientSecret}&scope=https%3A%2F%2Fapi.botframework.com%2F.default", Encoding.UTF8, "application/x-www-form-urlencoded"),
                };

                var authResponse = await authClient.SendAsync(authRequest).ConfigureAwait(false);
                var authContent = await authResponse.Content.ReadAsStringAsync();
                Console.WriteLine(authContent);
                string accessToken = null;
                if (authResponse.IsSuccessStatusCode)
                {
                    accessToken = JObject.Parse(authContent).SelectToken("access_token").ToString();
                }

                if (accessToken != null)
                {
                    Console.WriteLine("Enter message for bot: ");
                    var client = new HttpClient();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var activity = new Activity();
                    activity.Type = ActivityTypes.Message;
                    activity.Text = Console.ReadLine();
                    activity.ChannelId = "sms";
                    // This is the service url that should be in the activity the bot gets
                    activity.ServiceUrl = "https://www.ourcustomapi.com";
                    activity.Id = "ACTIVITY_ID";
                    activity.Conversation = new ConversationAccount()
                    {
                        Id = "CONVERSATION_ID"
                    };
                    activity.Recipient = new ChannelAccount()
                    {
                        Id = "RECIPIENT_ID"
                    };
                    activity.From = new ChannelAccount()
                    {
                        Id = "FROM_ID"
                    };
                    var jsonBody = JsonConvert.SerializeObject(activity);
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri($"http://localhost:3978/api/messages"),
                        Content = new StringContent(jsonBody, Encoding.UTF8, "application/json"),
                    };

                    var response = await client.SendAsync(request).ConfigureAwait(false);
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(content);
                }
            }
        }
    }
}
