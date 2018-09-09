using Discord;
using Discord.Addons.EmojiTools;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition_Bot
{
    public class OneVOneMatch : IMatch
    {
        //  Variables
        //  =========

        private const string embedDescription = "Indicate below if you won or lost!";

        //  Properties
        //  ==========

        public Participant Challenger { get; set; }

        public List<Participant> AllChallenged { get; set; }

        public List<Participant> AllPlayers
        {
            get
            {
                return new List<Participant>
                {
                    Challenger,
                    AllChallenged[0]
                };
            }
        }

        public int Points { get; set; }

        //  Constructors
        //  ============

        public OneVOneMatch(OneVOneChallenge challenge)
        {
            Challenger = challenge.Challenger;
            AllChallenged = challenge.AllChallenged;
            Points = challenge.Points;
        }

        private OneVOneMatch()
        {

        }

        public async static Task<OneVOneMatch> TryParse(IMessage message)
        {
            OneVOneMatch result = null;

            if (message.Embeds.Count != 1)
                return null;

            ulong? challengerId = null;
            ulong? challengedId = null;
            int? points = null;
            string description = message.Embeds.First().Description;

            foreach (EmbedField field in message.Embeds.First().Fields)
            {
                switch (field.Name)
                {
                    case "Challenger:":
                        TryParseNullableUlong(field.Value.Trim(new char[] { '<', '>', '@', '!' }), out challengerId);
                        break;
                    case "Challenged:":
                        TryParseNullableUlong(field.Value.Trim(new char[] { '<', '>', '@', '!' }), out challengedId);
                        break;
                    case "Amount:":
                        TryParseNullableInt(field.Value, out points);
                        break;
                    default:
                        break;
                }
            }

            if (challengerId == null || challengedId == null || points == null || description != embedDescription)
                return null;

            IGuild guild = ((IGuildChannel)message.Channel).Guild;
            result = new OneVOneMatch
            {
                Challenger = new Participant(await guild.GetUserAsync(challengerId.Value)),
                AllChallenged = new List<Participant> { new Participant(await guild.GetUserAsync(challengedId.Value)) },
                Points = points.Value
            };

            return result;
        }

        //  Methods
        //  =======

        public async Task CreateMatch(IMessageChannel channel, IGuild guild)
        {
            IUserMessage message = await channel.SendMessageAsync(embed: GetEmbed());
            await message.PinAsync();
            await message.AddReactionAsync(WonReact(guild));
            await message.AddReactionAsync(LostReact(guild));
        }

        private Embed GetEmbed()
        {
            return new EmbedBuilder
            {
                Title = $"{Challenger.Username} challenged {AllChallenged[0].Username}!",
                Description = embedDescription,
                Color = Color.Blue,
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "Challenger:",
                        Value = Challenger.Mention,
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Challenged:",
                        Value = AllChallenged[0].Mention,
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Amount:",
                        Value = Points,
                        IsInline = true
                    },
                }
            }.Build();
        }

        public async Task ReactionAdded(SocketReaction reaction, IUserMessage message, IGuild guild)
        {
            string wonReact = WonReact(guild).Name;
            string lostReact = LostReact(guild).Name;
            string actualReact = reaction.Emote.Name;

            if ((reaction.UserId != Challenger.Id && reaction.UserId != AllChallenged[0].Id)
             || (actualReact != wonReact && actualReact != lostReact))
            {
                await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
                return;
            }

            IEnumerable<ulong> wonReacts = (await message.GetReactionUsersAsync(WonReact(guild), 5).FlattenAsync()).Select(p => p.Id);
            IEnumerable<ulong> lostReacts = (await message.GetReactionUsersAsync(LostReact(guild), 5).FlattenAsync()).Select(p => p.Id);

            if ((wonReacts.Contains(Challenger.Id) && !wonReacts.Contains(AllChallenged[0].Id))//Challenger won
            && (!lostReacts.Contains(Challenger.Id) && lostReacts.Contains(AllChallenged[0].Id)))
            {
                await Winner(Challenger, AllChallenged[0]);
                await EndMatch((INestedChannel)message.Channel);
            }
            else if ((wonReacts.Contains(AllChallenged[0].Id) && !wonReacts.Contains(Challenger.Id))//Challenged won
            && (!lostReacts.Contains(AllChallenged[0].Id) && lostReacts.Contains(Challenger.Id)))
            {
                await Winner(AllChallenged[0], Challenger);
                await EndMatch((INestedChannel)message.Channel);
            }
            else if ((wonReacts.Contains(Challenger.Id) && wonReacts.Contains(AllChallenged[0].Id))//Disagreement
                || (lostReacts.Contains(Challenger.Id) && lostReacts.Contains(AllChallenged[0].Id)))
            {
                ulong categoryId = ((INestedChannel) message.Channel).CategoryId.Value;
                ICategoryChannel category = (await ((IGuildChannel)message.Channel).Guild.GetCategoriesAsync()).FirstOrDefault(c => c.Id == categoryId);

                foreach (IRole role in guild.Roles)
                {
                    if (role.Name == ConfigFile.ModRoleName && category.GetPermissionOverwrite(role) == null)
                    {
                        await category.AddPermissionOverwriteAsync(role, Permissions.canView);
                        await message.Channel.SendMessageAsync($"{role.Mention}, it looks like we might have a disagreement here!");
                        break;
                    }
                }
            }
        }

        private async Task Winner(Participant winner, Participant loser)
        {
            await winner.GiveCurrency(Points);
            await loser.GiveCurrency(0 - Points);
        }

        public async Task EndMatch(INestedChannel fromChannel)
        {
            ulong categoryId = (fromChannel).CategoryId.Value;

            foreach (ITextChannel channel in await fromChannel.Guild.GetTextChannelsAsync())
            {
                if (channel.CategoryId == categoryId)
                    await channel.DeleteAsync();
            }

            foreach (IVoiceChannel channel in await fromChannel.Guild.GetVoiceChannelsAsync())
            {
                if (channel.CategoryId == categoryId)
                    await channel.DeleteAsync();
            }

            await (await fromChannel.Guild.GetCategoriesAsync()).FirstOrDefault(c => c.Id == categoryId).DeleteAsync();
        }

        public IEmote WonReact(IGuild guild)
        {
            return GetEmote(ConfigFile.OneVOne.ChallengeAcceptReact, guild);
        }

        public IEmote LostReact(IGuild guild)
        {
            return GetEmote(ConfigFile.OneVOne.ChallengeCancelReact, guild);
        }

        private IEmote GetEmote(string emote, IGuild guild)
        {
            if (EmojiMap.Map.ContainsKey(emote))
                return new Emoji(EmojiMap.Map[emote]);
            return guild.Emotes.FirstOrDefault(e => e.Name == emote);
        }

        private static void TryParseNullableUlong(string input, out ulong? output)
        {
            if (ulong.TryParse(input, out ulong value))
                output = value;
            else
                output = null;
        }

        private static void TryParseNullableInt(string input, out int? output)
        {
            if (int.TryParse(input, out int value))
                output = value;
            else
                output = null;
        }
    }
}
