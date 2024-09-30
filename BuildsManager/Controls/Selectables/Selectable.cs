﻿using Blish_HUD.Content;
using Colors = Blish_HUD.ContentService.Colors;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Blish_HUD;
using Blish_HUD.Input;

namespace Kenedia.Modules.BuildsManager.Controls.Selectables
{
    public class Selectable<IBaseApiData> : Blish_HUD.Controls.Control
    {
        private Rectangle _textureBounds;
        private IBaseApiData? _data;

        protected AsyncTexture2D? Texture;

        public Selectable()
        {
            Size = new Point(64);
        }

        public IBaseApiData? Data { get => _data; set => Common.SetProperty(ref _data, value, ApplyData); }

        public SelectableType Type { get; private set; }

        public Rectangle TextureRegion { get; private set; }

        public Action<IBaseApiData?> OnClickAction { get; set; }

        public bool IsSelected { get; set; }

        public bool HighlightSelected { get; set; } = true;

        public bool HighlightHovered { get; set; } = true;

        protected virtual void ApplyData(object sender, Core.Models.ValueChangedEventArgs<IBaseApiData> e)
        {
            if (Data is null) return;

            switch (Data)
            {
                case Legend legend:
                    Type = SelectableType.Legend;
                    //TextureRegion = new(14, 14, 100, 100);
                    int lPadding = (int)(legend.Icon.Width * 0.109375);
                    TextureRegion = new(lPadding, lPadding, legend.Icon.Width - (lPadding * 2), legend.Icon.Height - (lPadding * 2));
                    Texture = legend.Icon;
                    ClipsBounds = true;

                    break;
                case Skill skill:
                    Type = SelectableType.Skill;
                    //TextureRegion = new(14, 14, 100, 100);
                    int sPadding = (int)(skill.Icon.Width * 0.109375);
                    TextureRegion = new(sPadding, sPadding, skill.Icon.Width - (sPadding * 2), skill.Icon.Height - (sPadding * 2));
                    Texture = skill.Icon;
                    ClipsBounds = true;

                    break;

                case Pet pet:
                    Type = SelectableType.Pet;
                    TextureRegion = new(16, 16, 200, 200);
                    Texture = pet.Icon;
                    ClipsBounds = false;
                    HighlightHovered = false;
                    HighlightSelected = false;

                    break;
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (Data is null) return;

            int pad = 48;
            _textureBounds = Data is not Pet ? LocalBounds : LocalBounds.Add(new(
                -pad,
                -pad,
                pad * 2,
                pad * 2
                ));
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (Data is null) return;

            int pad = 16;
            _textureBounds = Data is not Pet ? bounds : bounds.Add(new(
                -pad,
                -pad,
                pad * 2,
                pad * 2
                ));

            if (Texture is not null)
                spriteBatch.DrawOnCtrl(this, Texture, _textureBounds, TextureRegion, Color.White);

            if (HighlightSelected && IsSelected)
                spriteBatch.DrawFrame(this, bounds, Colors.ColonialWhite, 3);

            if (HighlightHovered && MouseOver)
                spriteBatch.DrawFrame(this, bounds, Colors.ColonialWhite, 2);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);
            if (Data is null) return;

            OnClickAction?.Invoke(Data);
        }
    }
}
