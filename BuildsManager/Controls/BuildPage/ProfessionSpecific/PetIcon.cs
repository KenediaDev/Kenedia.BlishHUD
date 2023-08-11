using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage.ProfessionSpecific
{
    public class PetIcon : DetailedTexture
    {
        private readonly AsyncTexture2D _paw = AsyncTexture2D.FromAssetId(156797);
        private readonly AsyncTexture2D _pawPressed = AsyncTexture2D.FromAssetId(156796);

        private Pet _pet;

        public PetIcon()
        {
            FallBackTexture = AsyncTexture2D.FromAssetId(156797);
        }

        public PetSlotType PetSlot { get; set; }

        public Rectangle PawRegion { get; set; }

        public Pet Pet { get => _pet; set => Common.SetProperty(ref _pet, value, ApplyPet); }

        private void ApplyPet()
        {
            Texture = Pet?.Icon;
            TextureRegion = new(16, 16, 200, 200);
        }

        public override void Draw(Control ctrl, SpriteBatch spriteBatch, Point? mousePos = null, Color? color = null, Color? bgColor = null, bool? forceHover = null, float? rotation = null, Vector2? origin = null)
        {
            base.Draw(ctrl, spriteBatch, mousePos, color, bgColor, forceHover, rotation, origin);

            color ??= Color.White;
            origin ??= Vector2.Zero;
            rotation ??= 0F;
            Hovered = mousePos != null && PawRegion.Contains((Point)mousePos);

            if (Hovered)
            {
                spriteBatch.DrawOnCtrl(
                    ctrl,
                    _paw,
                    PawRegion,
                    _paw.Bounds,
                    (Color)color,
                    (float)rotation,
                    (Vector2)origin);
            }
        }
    }
}
