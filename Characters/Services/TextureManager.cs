using Kenedia.Modules.Core.Services;
using Microsoft.Xna.Framework.Graphics;

namespace Kenedia.Modules.Characters.Services
{
    public class TextureManager
    {
        private readonly TexturesService _texturesService;

        public TextureManager(TexturesService texturesService)
        {
            _texturesService = texturesService;
        }

        public enum ControlTextures
        {
            Separator,
            Plus_Button,
            Plus_Button_Hovered,
            Minus_Button,
            Minus_Button_Hovered,
            ZoomIn_Button,
            ZoomIn_Button_Hovered,
            ZoomOut_Button,
            ZoomOut_Button_Hovered,
            Drag_Button,
            Drag_Button_Hovered,
            Potrait_Button,
            Potrait_Button_Hovered,
            Delete_Button,
            Delete_Button_Hovered,
            Random_Button,
            Random_Button_Hovered,
            Eye_Button,
            Eye_Button_Hovered,
            Telescope_Button,
            Telescope_Button_Hovered,
            Choya,
            Info_Button,
            Info_Button_Hovered,
        }

        public enum Icons
        {
            Bug,
            ModuleIcon,
            ModuleIcon_Hovered,
            ModuleIcon_HoveredWhite,
            Folder,
            Folder_Hovered,
            Camera,
            Dice,
            Dice_Hovered,
            Pin,
            Pin_Hovered,
            Camera_Hovered,
            Portrait_Hovered,
            Gender,
            Gender_Hovered,
            Female,
            Female_Hovered,
            Male,
            Male_Hovered,
        }

        public enum Backgrounds
        {
            MainWindow,
        }

        public Texture2D GetBackground(Backgrounds background)
        {
            return _texturesService.GetTexture(@"textures\backgrounds\" + (int)background + ".png", $"Background {background}");
        }

        public Texture2D GetIcon(Icons icon)
        {
            return _texturesService.GetTexture(@"textures\icons\" + (int)icon + ".png", $"Icon {icon}");
        }

        public Texture2D GetControlTexture(ControlTextures control)
        {
            return _texturesService.GetTexture(@"textures\controls\" + (int)control + ".png", $"Control {control}");
        }
    }
}
