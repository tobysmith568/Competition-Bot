using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Competition_Bot
{
    public partial class ConfigFile
    {
        //  JSON Properties
        //  ===============

        [JsonProperty("Token")]
        private string _Token { get; set; }

        [JsonProperty("Game")]
        private string _Game { get; set; }

        [JsonProperty("ModRoleName")]
        private string _ModRoleName { get; set; }

        [JsonProperty("AllowsSingle")]
        private OneVOneConfig _AllowsSingle { get; set; }

        //  Static Properties
        //  =================

        private static ConfigFile singleton;

        public static string Token
        {
            get
            {
                return singleton._Token;
            }
        }

        public static string Game
        {
            get
            {
                return singleton._Game;
            }
        }

        public static string ModRoleName
        {
            get
            {
                return singleton._ModRoleName;
            }
        }

        public static OneVOneConfig AllowsSingle
        {
            get
            {
                return singleton._AllowsSingle;
            }
        }

        //  Constructors
        //  ============

        static ConfigFile()
        {
            singleton = new ConfigFile();
        }

        public static ConfigFile FromJson(string json)
        {
            singleton = JsonConvert.DeserializeObject<ConfigFile>(json, Converter.Settings);
            return singleton;
        }
    }
}