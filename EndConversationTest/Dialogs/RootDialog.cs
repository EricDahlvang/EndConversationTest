using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace EndConversationTest.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            int length = (activity.Text ?? string.Empty).Length;
            await context.PostAsync($"You sent {activity.Text} which was {length} characters");

            if (length > 0)
            {
                string text = activity.Text.ToUpper().Replace(" ", "");
                if (text == "END")
                {
                    await context.PostAsync($"Ending Conversation from RootDialog");
                    context.EndConversation("Conversation Ended");
                }
                else if(text == "TEST")
                {
                    await context.PostAsync($"Forwarding future messages to TestDialog");
                    await context.Forward(new TestDialog(), ResumeAfterTest, null);
                }
            }
            else
            {
                context.Wait(MessageReceivedAsync);
            }
        }

        private Task ResumeAfterTest(IDialogContext context, IAwaitable<object> result)
        {
            context.PostAsync("Test Over");
            return Task.CompletedTask;
        }
    }
}