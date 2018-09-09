using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition_Bot
{
    public interface IComponent
    {
        Task ReactionAdded(SocketReaction reaction, IUserMessage message, IGuild guild);
    }
}
