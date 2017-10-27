using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace EndConversationTest
{
    public static class ConversationTimeout
    {
        public static Timer _timer;
        public static string fromId;
        public static string fromName;
        public static string toId;
        public static string toName;
        public static string serviceUrl;
        public static string channelId;
        public static string conversationId;
        public static string id;

        public static void Initialize(Activity activity)
        {
            fromId = activity.From.Id;
            fromName = activity.From.Name;
            toId = activity.Recipient.Id;
            toName = activity.Recipient.Name;
            serviceUrl = activity.ServiceUrl;
            channelId = activity.ChannelId;
            conversationId = activity.Conversation.Id;
            id = activity.Id;

            _timer = new Timer(Timer_Elapsed);
            _timer.Change(Convert.ToInt64(TimeSpan.FromSeconds(10).TotalMilliseconds), Timeout.Infinite);
        }

        static void Timer_Elapsed(object state)
        {
            var userAccount = new ChannelAccount(toId, toName);
            var botAccount = new ChannelAccount(fromId, fromName);
            var connector = new ConnectorClient(new Uri(serviceUrl));

            var message = Activity.CreateMessageActivity() as Activity;
            message.ChannelId = channelId;
            message.Id = id;
            message.From = botAccount;
            message.Recipient = userAccount;
            message.Conversation = new ConversationAccount(id: conversationId);
            message.Locale = "en-Us";
            message.ServiceUrl = serviceUrl;

            AbortConversation(message).ConfigureAwait(false);
        }

        public static async Task AbortConversation(Activity message)
        {
            if (_timer != null)
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                _timer = null;
            }

            using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, message))
            {
                var token = new CancellationToken();
                var botData = scope.Resolve<IBotData>();
                await botData.LoadAsync(token);

                var stack = scope.Resolve<IDialogStack>();
                stack.Reset();

                // botData.UserData.Clear(); //<-- could clear userdata as well
                botData.ConversationData.Clear();
                botData.PrivateConversationData.Clear();
                await botData.FlushAsync(token);

                var botToUser = scope.Resolve<IBotToUser>();
                await botToUser.PostAsync(message.CreateReply("Timer fired.  Conversation aborted."));
            }
        }
    }
}