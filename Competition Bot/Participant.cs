﻿using Discord;
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

        public string Nickname
        {
            get
            {
                return GuildUser.Nickname.Split(new string[] { "💰" }, StringSplitOptions.None)[0].Trim();
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

        public async Task GiveCurrency(int value)
        {
            string fullName = Nickname + " 💰 " + (Currency + value);
            await GuildUser.ModifyAsync(u => u.Nickname = fullName);
        }
    }
}
