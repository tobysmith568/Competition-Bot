using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Competition_Bot
{
    public interface IChallenge : IComponent
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

        Task CloseChallenge(SocketReaction reaction, IUserMessage message, IGuild guild);
    }
}
