using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace Competition_Bot.Matches.Challenges
{
    interface IChallenge
    {
        //  Properties
        //  ==========

        Participant Challenger { get; set; }

        List<Participant> AllChallenged { get; set; }

        List<Participant> AllPlayers { get; }

        int Points { get; set; }

        //  Methods
        //  =======

        Task CreateChallenge(IGuild guild);

        IEmote AcceptReact(IGuild guild);

        IEmote CancelReact(IGuild guild);

        IEmote RaiseBetReact(IGuild guild);

        IEmote LowerBetReact(IGuild guild);
    }
}
