﻿using Blish_HUD.Content;
using Blish_HUD.Input;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Res;
using Kenedia.Modules.Core.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.OverflowTradingAssist.Controls
{
    public class ItemPanel : FlowPanel
    {
        private DetailedTexture _buttonImage = new();
        private DetailedTexture _texture = new(255295, 255297);

        public ItemPanel()
        {
            ContentPadding = new(5);
            ControlPadding = new(2);

            _buttonImage = new()
            {
                Texture = TexturesService.GetTextureFromRef(textures_common.ImageButtonBackground, nameof(textures_common.ImageButtonBackground)),
                HoveredTexture = TexturesService.GetTextureFromRef(textures_common.ImageButtonBackground_Hovered, nameof(textures_common.ImageButtonBackground_Hovered)),
            };

            FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleTopToBottom;
        }

        public AsyncTexture2D Texture { get => _texture.Texture; set => _texture.Texture = value; }

        public AsyncTexture2D HoveredTexture { get => _texture.HoveredTexture; set => _texture.HoveredTexture = value; }

        public Action ClickAction { get; set; }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            _buttonImage.Draw(this, spriteBatch, RelativeMousePosition);
            _texture.Draw(this, spriteBatch, RelativeMousePosition);
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _buttonImage.Bounds = new(LayoutHeaderBounds.Right - LayoutHeaderBounds.Height + 5, 5, LayoutHeaderBounds.Height - 10, LayoutHeaderBounds.Height - 10);
            _texture.Bounds = new(LayoutHeaderBounds.Right - LayoutHeaderBounds.Height + 5, 5, LayoutHeaderBounds.Height - 10, LayoutHeaderBounds.Height - 10);
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            BasicTooltipText = _buttonImage.Hovered || _texture.Hovered ? "Add Item" : null;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if (_buttonImage.Hovered || _texture.Hovered)
            {
                ClickAction?.Invoke();
            }
        }
    }
}
