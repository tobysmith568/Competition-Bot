using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Competition_Bot
{
    public class Permissions
    {
        public static OverwritePermissions canView = new OverwritePermissions(viewChannel: PermValue.Allow);
        public static OverwritePermissions cantView = new OverwritePermissions(viewChannel: PermValue.Deny);
        public static OverwritePermissions noReactions = new OverwritePermissions(addReactions: PermValue.Deny);
        public static OverwritePermissions cantViewNoReactions = new OverwritePermissions(viewChannel: PermValue.Deny, addReactions: PermValue.Deny);
    }
}
