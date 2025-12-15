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

namespace Kenedia.Modules.BuildsManager.Controls
{
    public class PetControl : Control
    {
        private readonly DetailedTexture _highlight = new(156844) { TextureRegion = new(16, 16, 200, 200) };
        private readonly DetailedTexture _selector = new(157138, 157140);
        private readonly DetailedTexture _petTexture = new(156794) { TextureRegion = new(16, 16, 200, 200), };
        private readonly DetailedTexture _emptySlotTexture = new(157154) { TextureRegion = new(14, 14, 100, 100) };

        public PetControl()
        {
            Tooltip = new PetTooltip();
        }

        public PetSlotType PetSlot { get; set; }

        public Pet? Pet { get; set => Common.SetProperty(field, value, v => field = v, ApplyPet); }

        public Action<PetControl> LeftClickAction { get; set; }

        public Action<PetControl> RightClickAction { get; set; }

        private void ApplyPet(object sender, Core.Models.ValueChangedEventArgs<Pet> e)
        {
            _petTexture.Texture = Pet?.Icon is null ? AsyncTexture2D.FromAssetId(156794) : Pet?.Icon;

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

            //if (Pet is null)
            //    _emptySlotTexture?.Draw(this, spriteBatch);

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
}
