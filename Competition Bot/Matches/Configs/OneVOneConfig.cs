using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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
        [JsonProperty("ChallengeAcceptReact")]
        public string ChallengeAcceptReact { get; private set; }

        [JsonRequired]
        [JsonProperty("ChallengeCancelReact")]
        public string ChallengeCancelReact { get; private set; }

        [JsonRequired]
        [JsonProperty("ChallengeRaiseBetReact")]
        public string ChallengeRaiseBetReact { get; private set; }

        [JsonRequired]
        [JsonProperty("ChallengeLowerBetReact")]
        public string ChallengeLowerBetReact { get; private set; }
    }
}