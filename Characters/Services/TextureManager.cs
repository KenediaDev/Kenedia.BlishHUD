using Kenedia.Modules.Core.Services;
using Microsoft.Xna.Framework.Graphics;

namespace Kenedia.Modules.Characters.Services
{
    public class TextureManager
    {
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
            CharacterRoutine,
            CharacterRoutine_Hovered,
            CharacterRoutine_Active,
        }

        public enum Backgrounds
        {
            MainWindow,
        }

        public enum Emblems
        {
            CharacterRoutine,
            CharacterRoutine_2,
            CharacterRoutine_3,
            CharacterRoutine_4,
            CharacterRoutine_5,
            CharacterRoutine_6,
            CharacterRoutine_7,
            CharacterRoutine_8,
            CharacterRoutine_9,
            CharacterRoutine_10,
        }

        public Texture2D GetBackground(Backgrounds background)
        {
            return TexturesService.GetTextureFromRef(@"textures\backgrounds\" + (int)background + ".png", $"Background {background}");
        }

        public Texture2D GetIcon(Icons icon)
        {
            return TexturesService.GetTextureFromRef(@"textures\icons\" + (int)icon + ".png", $"Icon {icon}");
        }

        public Texture2D GetControlTexture(ControlTextures control)
        {
            return TexturesService.GetTextureFromRef(@"textures\controls\" + (int)control + ".png", $"Control {control}");
        }

        public Texture2D GetEmblem(Emblems emblem)
        {
            return TexturesService.GetTextureFromRef(@"textures\emblems\" + (int)emblem + ".png", $"Emblem {emblem}");
        }
    }
}
