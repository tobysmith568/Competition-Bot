using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Newtonsoft.Json;

namespace Competition_Bot
{
    static class Tools
    {
        public static async void ReportError(ICommandContext context, Exception e)
        {
            await context.Channel.TriggerTypingAsync();
            await context.Channel.SendMessageAsync("Sorry, there was the following error:" + Environment.NewLine + e.Message);
        }
    }
}