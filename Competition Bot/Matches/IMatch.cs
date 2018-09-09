using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition_Bot
{
    public interface IMatch : IComponent
    {
        Participant Challenger { get; set; }

        List<Participant> AllChallenged { get; set; }

        List<Participant> AllPlayers { get; }

        int Points { get; set; }

        Task EndMatch(INestedChannel fromChannel);
    }
}
