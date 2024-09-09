using Kenedia.Modules.Core.Models;
using Blish_HUD.Content;
using System;
using Kenedia.Modules.BuildsManager.Models;

namespace Kenedia.Modules.BuildsManager.Controls.Selection
{
    public class TagTexture : DetailedTexture
    {
        public TagTexture(AsyncTexture2D texture) : base(texture)
        {
        }

        public TemplateTag Tag { get; internal set; }
    }
}
