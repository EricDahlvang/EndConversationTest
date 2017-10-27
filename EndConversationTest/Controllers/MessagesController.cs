using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs.Internals;
using System.Threading;
using Autofac;

using System;
using System.Collections.Generic;
using System.Linq;

using System.Web;
using System.Configuration;

namespace EndConversationTest
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                var text = activity.Text;
                if (!string.IsNullOrEmpty(text) 
                    && text.Equals("ABORT",StringComparison.InvariantCultureIgnoreCase) || text.Equals("START", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (activity.Text.ToUpper() == "ABORT")
                    {
                        await ConversationTimeout.AbortConversation(activity);
                    }
                    else if (activity.Text.ToUpper() == "START")
                    {
                        ConversationTimeout.Initialize(activity);

                        var reply = activity.CreateReply("Timer Started.  You have 10 seconds before abort...");
                        var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                        await connector.Conversations.SendToConversationAsync((Activity)reply);
                    }
                }
                else
                {
                    await Conversation.SendAsync(activity, () => new Dialogs.RootDialog());
                }
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }


        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}