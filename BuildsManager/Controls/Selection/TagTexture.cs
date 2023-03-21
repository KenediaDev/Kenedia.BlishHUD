using Kenedia.Modules.Core.Models;
using Blish_HUD.Content;
using System;

namespace Kenedia.Modules.BuildsManager.Controls.Selection
{
    public class TagTexture : DetailedTexture
    {
        public TagTexture(AsyncTexture2D texture) : base(texture)
        {
        }

        public Enum TemplateTag { get; set; }
    }
}
