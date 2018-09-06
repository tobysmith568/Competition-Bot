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
        IConfig Config { get; }

        Participant Challenger { get; set; }

        List<Participant> AllChallenged { get; set; }

        List<Participant> AllPlayers { get; }

        int Points { get; set; }

        Task CreateChallenge(IGuild guild);

        void AcceptReact();

        void CancelReact();

        void RaiseBetReact();

        void LowerBetReact();
    }
}
