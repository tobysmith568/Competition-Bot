using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Competition_Bot
{
    public class OneVOneConfig : IConfig
    {
        //  JSON Properties
        //  ===============

        [JsonRequired]
        [JsonProperty("ChallengeChannelName")]
        public string ChallengeChannelName { get; }

        [JsonRequired]
        [JsonProperty("ChallengeAcceptReact")]
        public string ChallengeAcceptReact { get; }

        [JsonRequired]
        [JsonProperty("ChallengeCancelReact")]
        public string ChallengeCancelReact { get; }

        [JsonRequired]
        [JsonProperty("ChallengeRaiseBetReact")]
        public string ChallengeRaiseBetReact { get; }

        [JsonRequired]
        [JsonProperty("ChallengeLowerBetReact")]
        public string ChallengeLowerBetReact { get; }
    }
}
