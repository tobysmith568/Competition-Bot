using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition_Bot
{
    public interface IMatch
    {
        Participant Challenger { get; set; }

        Participant[] AllChallenged { get; set; }

        Participant[] AllPlayers { get; set; }

        int Points { get; set; }
    }
}
