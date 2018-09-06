using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Competition_Bot.Matches.Challenges
{
    public class OneVOneChallenge : IChallenge
    {
        //  Properties
        //  ==========

        public IConfig Config => new OneVOneConfig();

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

            foreach (EmbedField field in message.Embeds.First().Fields)
            {
                if (field.Name == "Challenger:")
                    TryParseNullableUlong(field.Value.Trim(new char[] { '<', '>', '@', '!' }), out challengerId);
                if (field.Name == "Challenged:")
                    TryParseNullableUlong(field.Value.Trim(new char[] { '<', '>', '@', '!' }), out challengedId);
                if (field.Name == "Amount:")
                    TryParseNullableInt(field.Value, out points);
            }

            if (challengerId == null || challengedId == null || points == null)
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
            ITextChannel challengeChannel = (await guild.GetTextChannelsAsync()).First(c => c.Name == Config.ChallengeChannelName);
            challengeChannel = challengeChannel ?? await guild.GetTextChannelAsync(guild.DefaultChannelId);

            Embed embed = new EmbedBuilder
            {
                Title = $"{Challenger.Username} challenges {AllChallenged[0].Username}!",
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

            await challengeChannel.SendMessageAsync(embed: embed);
        }

        public void AcceptReact()
        {

        }

        public void CancelReact()
        {

        }

        public void RaiseBetReact()
        {

        }

        public void LowerBetReact()
        {

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
