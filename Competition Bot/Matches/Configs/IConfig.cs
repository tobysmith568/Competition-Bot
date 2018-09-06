using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition_Bot
{
    public interface IConfig
    {
        //  Properties
        //  ==========

        string ChallengeChannelName { get; }

        string ChallengeAcceptReact { get; }

        string ChallengeCancelReact { get; }

        string ChallengeRaiseBetReact { get; }

        string ChallengeLowerBetReact { get; }
    }
}
