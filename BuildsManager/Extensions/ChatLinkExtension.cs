using Gw2Sharp.ChatLinks;
using Gw2Sharp.WebApi.V2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.LinkLabel;

namespace Kenedia.Modules.BuildsManager.Extensions
{
    public static class ChatLinkExtension
    {
        public static string CreateChatLink(this Trait trait)
        {
            var link = new TraitChatLink() { TraitId = trait.Id };
            byte[] bytes = link.ToArray();
            link.Parse(bytes);

            return link.ToString();
        }
    }
}
