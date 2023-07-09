using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kenedia.Modules.Core.Structs;
using MonoGame.Extended.BitmapFonts;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using System;
using Blish_HUD;

namespace Kenedia.Modules.BuildsManager.Controls.GearPage
{
    public class StatSummary : Blish_HUD.Controls.Control
    {
        private Template _template;
        private readonly bool _created;

        private readonly DetailedTexture _texturePanelHeader = new(1032325);
        private readonly DetailedTexture _textureRightCornerAccent = new(1002144);
        private readonly DetailedTexture _textureLeftCornerAccent = new(1002144);
        private readonly DetailedTexture _textureLeftSideAccent = new(605025);
        private readonly DetailedTexture _textureRightSideAccent = new(605025);

        private readonly AttributeTexture _power = new(66722) { TextureRegion = new(4, 4, 24, 24) };
        private readonly AttributeTexture _thoughness = new(156612) { TextureRegion = new(4, 4, 24, 24) };
        private readonly AttributeTexture _vitality = new(156613) { TextureRegion = new(4, 4, 24, 24) };
        private readonly AttributeTexture _precision = new(156609) { TextureRegion = new(4, 4, 24, 24) };
        private readonly AttributeTexture _ferocity = new(156602) { TextureRegion = new(4, 4, 24, 24) };
        private readonly AttributeTexture _condition = new(156600) { TextureRegion = new(4, 4, 24, 24) };
        private readonly AttributeTexture _expertise = new(156601) { TextureRegion = new(4, 4, 24, 24) };
        private readonly AttributeTexture _concentration = new(156599) { TextureRegion = new(4, 4, 24, 24) };

        private readonly AttributeTexture _agonyResistance = new(536049) { TextureRegion = new(4, 4, 24, 24) };
        private readonly AttributeTexture _profession = new(536050) { TextureRegion = new(4, 4, 24, 24) };
        private readonly AttributeTexture _armor = new(536048) { TextureRegion = new(4, 4, 24, 24) };
        private readonly AttributeTexture _health = new(536052) { TextureRegion = new(4, 4, 24, 24) };
        private readonly AttributeTexture _critChance = new(536051) { TextureRegion = new(4, 4, 24, 24) };
        private readonly AttributeTexture _critDamage = new(784327) { TextureRegion = new(4, 4, 24, 24) };
        private readonly AttributeTexture _healingPower = new(156606) { TextureRegion = new(4, 4, 24, 24) };
        private readonly AttributeTexture _conditionDuration = new(156601) { TextureRegion = new(4, 4, 24, 24) };
        private readonly AttributeTexture _boonDuration = new(156599) { TextureRegion = new(4, 4, 24, 24) };
        private readonly AttributeTexture _magicFind = new(536054) { TextureRegion = new(4, 4, 24, 24) };

        private readonly DetailedTexture _placeholder = new() { };

        public StatSummary()
        {
            _created = true;
            Size = new(380, 300);
            ClipsBounds = false;

            _placeholder.Texture = BuildsManager.ModuleInstance.ContentsManager.GetTexture(@"textures\choya_placeholder.png");
        }

        public Template Template
        {
            get => _template; set
            {
                var temp = _template;
                if (Common.SetProperty(ref _template, value, ApplyTemplate))
                {
                    if (temp != null) temp.PropertyChanged -= TemplateChanged;
                    if (_template != null) _template.PropertyChanged += TemplateChanged;
                }
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            RecalculateLayout();

            BitmapFont font = Content.DefaultFont14;
            _texturePanelHeader.Draw(this, spriteBatch);
            //spriteBatch.DrawStringOnCtrl(this, "Attributes", Content.DefaultFont18, _texturePanelHeader.Bounds.Add(10, -2, 0, 0), Color.White);
            spriteBatch.DrawStringOnCtrl(this, "Praise the Choya!", Content.DefaultFont18, _texturePanelHeader.Bounds.Add(10, -2, 0, 0), Color.White, false, Blish_HUD.Controls.HorizontalAlignment.Center);
            _textureLeftCornerAccent.Draw(this, spriteBatch, SpriteEffects.FlipHorizontally, null, Color.Black);
            _textureLeftSideAccent.Draw(this, spriteBatch, SpriteEffects.FlipHorizontally, null, Color.Black);
            _textureRightCornerAccent.Draw(this, spriteBatch);
            _textureRightSideAccent.Draw(this, spriteBatch, null, Color.Black);
            _placeholder.Draw(this, spriteBatch, null, Color.White);

        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();
            if (_created)
            {
                int size = 64;
                int height = 28;
                int textWidth = (Width / 2) - height;
                BackgroundColor = Color.Black * 0.2F;

                int accentWidth = Math.Min(_size.X + 70, 256 + 70);

                _texturePanelHeader.Bounds = new(0, 0, Width, 36);
                _textureLeftCornerAccent.Bounds = new(-8, _texturePanelHeader.Bounds.Height - 10, accentWidth, _textureLeftCornerAccent.Texture.Height);
                _textureLeftSideAccent.Bounds = new(-8, _texturePanelHeader.Bounds.Height, 16, Height - _texturePanelHeader.Bounds.Height + 16);
                _textureRightCornerAccent.Bounds = new(Width - accentWidth + 8, Height - 10, accentWidth, _textureLeftCornerAccent.Texture.Height);
                _textureRightSideAccent.Bounds = new(-8 + Width, 0, 16, Height);

                _placeholder.Bounds = new(_textureLeftSideAccent.Bounds.X + 20, _textureLeftSideAccent.Bounds.Y + 20, _textureRightSideAccent.Bounds.Left - _textureLeftSideAccent.Bounds.Left - 40, _textureLeftSideAccent.Bounds.Height - 40);

                RectangleDimensions padding = new(10, 5 + _texturePanelHeader.Bounds.Height, 0, 8);

                Rectangle getBounds(int pos, int col)
                {
                    return new(padding.Left + (col * Width / 2), padding.Top + (pos * (height + padding.Bottom)), height, height);
                }

                Rectangle getTextRegion(AttributeTexture texture)
                {
                    return new(texture.Bounds.Right + 5, texture.Bounds.Top, textWidth, height);
                }

                _power.Bounds = getBounds(0, 0);
                _power.TextRegion = getTextRegion(_power);

                _thoughness.Bounds = getBounds(1, 0);
                _thoughness.TextRegion = getTextRegion(_thoughness);

                _vitality.Bounds = getBounds(2, 0);
                _vitality.TextRegion = getTextRegion(_vitality);

                _precision.Bounds = getBounds(3, 0);
                _precision.TextRegion = getTextRegion(_precision);

                _ferocity.Bounds = getBounds(4, 0);
                _ferocity.TextRegion = getTextRegion(_ferocity);

                _condition.Bounds = getBounds(5, 0);
                _condition.TextRegion = getTextRegion(_condition);

                _expertise.Bounds = getBounds(6, 0);
                _expertise.TextRegion = getTextRegion(_expertise);

                _concentration.Bounds = getBounds(7, 0);
                _concentration.TextRegion = getTextRegion(_concentration);

                _agonyResistance.Bounds = getBounds(8, 0);
                _agonyResistance.TextRegion = getTextRegion(_agonyResistance);

                _profession.Bounds = getBounds(0, 1);
                _profession.TextRegion = getTextRegion(_profession);

                _armor.Bounds = getBounds(1, 1);
                _armor.TextRegion = getTextRegion(_armor);

                _health.Bounds = getBounds(2, 1);
                _health.TextRegion = getTextRegion(_health);

                _critChance.Bounds = getBounds(3, 1);
                _critChance.TextRegion = getTextRegion(_critChance);

                _critDamage.Bounds = getBounds(4, 1);
                _critDamage.TextRegion = getTextRegion(_critDamage);

                _healingPower.Bounds = getBounds(5, 1);
                _healingPower.TextRegion = getTextRegion(_healingPower);

                _conditionDuration.Bounds = getBounds(6, 1);
                _conditionDuration.TextRegion = getTextRegion(_conditionDuration);

                _boonDuration.Bounds = getBounds(7, 1);
                _boonDuration.TextRegion = getTextRegion(_boonDuration);

                _magicFind.Bounds = getBounds(8, 1);
                _magicFind.TextRegion = getTextRegion(_magicFind);
            }
        }

        public void ApplyTemplate()
        {

        }

        private void TemplateChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ApplyTemplate();
        }
    }
}
