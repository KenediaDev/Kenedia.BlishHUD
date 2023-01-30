using Blish_HUD.Content;
using Blish_HUD.Controls;

namespace Kenedia.Modules.Core.Views
{
    public class BaseTab
    {
        public AsyncTexture2D Icon { get; set; }

        public string Name { get; set; }

        public int Priority { get; set; }

        public Container ContentContainer { get; set; } = null;
    }
}