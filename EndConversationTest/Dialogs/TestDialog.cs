using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace EndConversationTest.Dialogs
{
    [Serializable]
    public class TestDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            int length = (activity == null || activity.Text == null ? string.Empty : activity.Text).Length;

            if (length > 0)
            {
                var text = activity.Text.ToUpper().Replace(" ", "");
                await context.PostAsync($"InTest: You sent {text} which was {length} characters");

                if (text == "END")
                {
                    await context.PostAsync($"Ending Conversation from TestDialog");
                    context.EndConversation("Conversation Ended");
                    return;
                }
                else if(text == "DONE")
                {
                    context.Done("true");
                    return;
                }
            }

            context.Wait(MessageReceivedAsync);

        }
    }
}