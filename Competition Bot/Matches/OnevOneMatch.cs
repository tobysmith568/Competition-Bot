using Discord;
using Discord.Addons.EmojiTools;
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

        private const string embedDescription = "Indicate below who one!";

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
