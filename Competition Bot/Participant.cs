using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition_Bot
{
    public class Participant
    {
        //  Properties
        //  ==========

        public IGuildUser GuildUser { get; }

        public ulong Id
        {
            get
            {
                return GuildUser.Id;
            }
        }

        public int Currency
        {
            get
            {
                return int.Parse(GuildUser.Nickname.Split(' ').Last());
            }
        }

        public string Mention
        {
            get
            {
                return GuildUser.Mention;
            }
        }

        public string Username
        {
            get
            {
                return GuildUser.Username;
            }
        }

        //  Constructors
        //  ============

        public Participant(IGuildUser guildUser)
        {
            GuildUser = guildUser;
        }

        //  Methods
        //  =======

        public async Task SendMessage(string message)
        {
            await GuildUser.SendMessageAsync(message);
        }
    }
}
