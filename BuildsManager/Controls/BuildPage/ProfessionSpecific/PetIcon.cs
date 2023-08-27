using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage.ProfessionSpecific
{
    public class PetControl : Control
    {
        private readonly DetailedTexture _highlight = new(156844) { TextureRegion = new(16, 16, 200, 200) };
        private readonly DetailedTexture _selector = new(157138, 157140);
        private readonly DetailedTexture _petTexture = new() { TextureRegion = new(16, 16, 200, 200), };
        private readonly DetailedTexture _emptySlotTexture = new(157154) { TextureRegion = new(14, 14, 100, 100) };
        private Pet _pet;

        public PetControl()
        {
            Tooltip = new PetTooltip();
        }

        public PetSlotType PetSlot { get; set; }

        public Pet Pet { get => _pet; set => Common.SetProperty(ref _pet, value, ApplyPet); }

        public Action<PetControl> LeftClickAction { get; set; }

        public Action<PetControl> RightClickAction { get; set; }

        private void ApplyPet(object sender, Core.Models.ValueChangedEventArgs<Pet> e)
        {
            _petTexture.Texture = Pet?.Icon;

            if (Tooltip is PetTooltip petTooltip)
            {
                petTooltip.Pet = Pet;
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            RecalculateLayout();
            _selector?.Draw(this, spriteBatch);
            _petTexture?.Draw(this, spriteBatch);

            if (Pet is null)
                _emptySlotTexture?.Draw(this, spriteBatch);

            if (MouseOver)
                _highlight?.Draw(this, spriteBatch);
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _petTexture.Bounds = new(0, 0, Width, Height);
            _highlight.Bounds = _petTexture.Bounds;

            var p = new Point(Width / 2, Height / 2);
            var s = new Point(64, 15);
            _selector.Bounds = new(p.X - (s.X / 2) + 4, p.Y - 36, s.X, s.Y);
            _emptySlotTexture.Bounds = new(p.X - (s.X / 2) + 4, p.Y - 36 + _selector.Bounds.Height, s.X, s.X);
        }

        protected override void OnRightMouseButtonPressed(MouseEventArgs e)
        {
            base.OnRightMouseButtonPressed(e);
            RightClickAction?.Invoke(this);
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
        {
            base.OnLeftMouseButtonPressed(e);
            LeftClickAction?.Invoke(this);
        }
    }

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
            Hovered = mousePos is not null && PawRegion.Contains((Point)mousePos);

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
