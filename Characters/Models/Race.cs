using Blish_HUD.Content;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Services;

namespace Kenedia.Modules.Characters.Models
{
    public class Race
    {
        public Races Id { get; set; }

        public LocalizedString Names { get; set; } = [];

        public string Name
        {
            get => Names.Text;
            set => Names.Text = value;
        }

        public AsyncTexture2D Icon
        {
            get
            {
                field ??= TexturesService.GetTextureFromRef(@"textures\races\" + Id.ToString().ToLower() + ".png");
                return field;
            }
        }
    }
}
