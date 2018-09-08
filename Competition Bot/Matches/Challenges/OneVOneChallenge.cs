using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.EmojiTools;
using Discord.WebSocket;

namespace Competition_Bot
{
    public class OneVOneChallenge : IChallenge
    {
        //  Variables
        //  =========

        private const string embedDescription = "Setup your challenge!";

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

        public int Points { get; set; } = 10;

        //  Constructors
        //  ============

        public OneVOneChallenge(Participant challenger, Participant challenged)
        {
            Challenger = challenger;
            AllChallenged = new List<Participant> { challenged };
        }

        private OneVOneChallenge()
        {

        }

        public async static Task<OneVOneChallenge> TryParse(IMessage message)
        {
            OneVOneChallenge result = null;

            if (message.Embeds.Count != 1)
                return null;

            ulong? challengerId = null;
            ulong? challengedId = null;
            int? points = null;
            string description = message.Embeds.First().Description;

            foreach (EmbedField field in message.Embeds.First().Fields)
            {
                if (field.Name == "Challenger:")
                    TryParseNullableUlong(field.Value.Trim(new char[] { '<', '>', '@', '!' }), out challengerId);
                if (field.Name == "Challenged:")
                    TryParseNullableUlong(field.Value.Trim(new char[] { '<', '>', '@', '!' }), out challengedId);
                if (field.Name == "Amount:")
                    TryParseNullableInt(field.Value, out points);
            }

            if (challengerId == null || challengedId == null || points == null || description != embedDescription)
                return null;

            IGuild guild = ((IGuildChannel)message.Channel).Guild;
            result = new OneVOneChallenge
            {
                Challenger = new Participant(await guild.GetUserAsync(challengerId.Value)),
                AllChallenged = new List<Participant> { new Participant(await guild.GetUserAsync(challengedId.Value)) },
                Points = points.Value
            };

            return result;
        }

        //  Methods
        //  =======

        public async Task CreateChallenge(IGuild guild)
        {
            ICategoryChannel category = await guild.CreateCategoryAsync($"{Challenger.Nickname} vs. {AllChallenged[0].Nickname}");
            await category.AddPermissionOverwriteAsync(guild.EveryoneRole, Permissions.cantViewNoReactions);
            await category.AddPermissionOverwriteAsync(Challenger.GuildUser, Permissions.canView);
            await category.AddPermissionOverwriteAsync(AllChallenged[0].GuildUser, Permissions.canView);
            ITextChannel textChannel = await guild.CreateTextChannelAsync("messaging", m => m.CategoryId = category.Id);
            await guild.CreateVoiceChannelAsync("talking", t => t.CategoryId = category.Id);

            IUserMessage message = await textChannel.SendMessageAsync($"{Challenger.Mention} {AllChallenged[0].Mention}");
            await message.PinAsync();

            Embed embed = GetEmbed();

            await message.ModifyAsync(m => { m.Content = ""; m.Embed = embed; });
            await message.AddReactionAsync(AcceptReact(guild));
            await message.AddReactionAsync(CancelReact(guild));
            await message.AddReactionAsync(RaiseBetReact(guild));
            await message.AddReactionAsync(LowerBetReact(guild));
        }

        private Embed GetEmbed()
        {
            return new EmbedBuilder
            {
                Title = $"{Challenger.Username} challenges {AllChallenged[0].Username}!",
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
            if (reaction.UserId != Challenger.Id && reaction.UserId != AllChallenged[0].Id)
                await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);

            if (reaction.Emote.Name == AcceptReact(guild).Name)
            {
                IEnumerable<ulong> reactedIds = (await message.GetReactionUsersAsync(reaction.Emote, 5)
                    .FlattenAsync())
                    .Select(u => u.Id);

                if (reactedIds.Contains(Challenger.Id) && reactedIds.Contains(AllChallenged[0].Id))
                {
                    await message.DeleteAsync();

                    OneVOneMatch match = new OneVOneMatch(this);
                    await match.CreateMatch(message.Channel, guild);
                }
            }
            else if (reaction.Emote.Name == CancelReact(guild).Name)
            {
                IEnumerable<ulong> reactedIds = (await message.GetReactionUsersAsync(reaction.Emote, 5)
                    .FlattenAsync())
                    .Select(u => u.Id);

                if (reactedIds.Contains(Challenger.Id) && reactedIds.Contains(AllChallenged[0].Id))
                    await CloseChallenge(reaction, message, guild);
            }
            else if (reaction.Emote.Name == RaiseBetReact(guild).Name)
            {
                await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);

                if (Points + 10 > Challenger.Currency)
                    return;
                if (Points + 10 > AllChallenged[0].Currency)
                    return;

                Points += 10;

                await message.ModifyAsync(m => m.Embed = GetEmbed());
            }
            else if (reaction.Emote.Name == LowerBetReact(guild).Name)
            {
                await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);

                if (Points - 10 <= 0)
                    return;

                Points -= 10;

                await message.ModifyAsync(m => m.Embed = GetEmbed());
            }
            else
                await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
        }

        public IEmote AcceptReact(IGuild guild)
        {
            return GetEmote(ConfigFile.OneVOne.ChallengeAcceptReact, guild);
        }

        public IEmote CancelReact(IGuild guild)
        {
            return GetEmote(ConfigFile.OneVOne.ChallengeCancelReact, guild);
        }

        public IEmote RaiseBetReact(IGuild guild)
        {
            return GetEmote(ConfigFile.OneVOne.ChallengeRaiseBetReact, guild);
        }

        public IEmote LowerBetReact(IGuild guild)
        {
            return GetEmote(ConfigFile.OneVOne.ChallengeLowerBetReact, guild);
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

        private IEmote GetEmote(string emote, IGuild guild)
        {
            if (EmojiMap.Map.ContainsKey(emote))
                return new Emoji(EmojiMap.Map[emote]);
            return guild.Emotes.FirstOrDefault(e => e.Name == emote);
        }

        public async Task CloseChallenge(SocketReaction reaction, IUserMessage message, IGuild guild)
        {
            ulong categoryId = ((INestedChannel)reaction.Channel).CategoryId.Value;

            foreach (ITextChannel channel in await guild.GetTextChannelsAsync())
            {
                if (channel.CategoryId == categoryId)
                    await channel.DeleteAsync();
            }

            foreach (IVoiceChannel channel in await guild.GetVoiceChannelsAsync())
            {
                if (channel.CategoryId == categoryId)
                    await channel.DeleteAsync();
            }

            await (await guild.GetCategoriesAsync()).FirstOrDefault(c => c.Id == categoryId).DeleteAsync();
        }
    }
}
