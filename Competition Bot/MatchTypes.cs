using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition_Bot
{
    public enum MatchTypes
    {
        OneVOne,
        OneVMany
    }

    static class RunTypeMethods
    {
        public static IConfig GetConfig(this MatchTypes type)
        {
            switch (type)
            {
                case MatchTypes.OneVOne:
                    return new OneVOneConfig();
                default:
                    throw new Exception("Unhandled Match Type");
            }
        }
    }
}
