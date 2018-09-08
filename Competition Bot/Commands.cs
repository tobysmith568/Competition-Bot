using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Discord.Rest;

namespace Competition_Bot
{
    class Commands : ModuleBase
    {
        public class CommandSubCategory : ModuleBase
        {
            [Command("Challenge")]
            [Alias(new string[] { "Fight" })]
            [Summary("Challengers other users")]
            public async Task Challenge(IGuildUser oponent)
            {
                try
                {
                    if (ConfigFile.OneVOne == null)
                        return;
                    if (Context.Channel is IDMChannel)
                        return;
                    if (Context.Channel is IGroupChannel)
                        return;
                    await Context.Message.DeleteAsync();

                    //Get the challenger
                    Participant challenger = new Participant((IGuildUser)Context.User);

                    if (challenger.Currency < 10)
                    {
                        await challenger.SendMessage("Sorry, you don't have enough currency to place a bet!");
                        return;
                    }

                    if (Context.Guild.OwnerId == oponent.Id)
                    {
                        await challenger.SendMessage("Sorry, you can't challenge the server owner!");
                        return;
                    }

                    if (challenger.Id == oponent.Id)
                    {
                        await challenger.SendMessage("Sorry, you can't challenge yourself!");
                        return;
                    }

                    //Get the challenged
                    Participant challenged = new Participant(oponent);

                    if (challenged.Currency < 10)
                    {
                        await challenger.SendMessage($"Sorry, {challenged.Mention} doesn't have enough currency to bet!");
                        return;
                    }

                    OneVOneChallenge newChallenge = new OneVOneChallenge(challenger, challenged);
                    await newChallenge.CreateChallenge(Context.Guild);
                }
                catch (Exception ee)
                {
                    Tools.ReportError(Context, ee);
                }
            }
        }
    }
}