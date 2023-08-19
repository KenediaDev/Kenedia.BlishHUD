using Blish_HUD.Content;
using Colors = Blish_HUD.ContentService.Colors;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Blish_HUD;
using Blish_HUD.Input;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage
{
    public class Selectable<IBaseApiData> : Blish_HUD.Controls.Control
    {
        private AsyncTexture2D _texture;
        private IBaseApiData _data;

        public IBaseApiData Data { get => _data; set => Common.SetProperty(ref _data, value, ApplyData); }

        public Selectable()
        {
            Size = new Point(64);
        }

        public SelectableType Type { get; private set; }

        public Rectangle TextureRegion { get; private set; }

        public Action<IBaseApiData> OnClickAction { get; set; }

        public bool IsSelected { get; set; }

        private void ApplyData(object sender, Core.Models.ValueChangedEventArgs<IBaseApiData> e)
        {
            if (Data is null) return;

            switch (Data)
            {
                case Skill skill:
                    Type = SelectableType.Skill;
                    //TextureRegion = new(14, 14, 100, 100);
                    int sPadding = (int)(skill.Icon.Width * 0.109375);
                    TextureRegion = new(sPadding, sPadding, skill.Icon.Width - (sPadding * 2), skill.Icon.Height - (sPadding * 2));
                    _texture = skill.Icon;
                    Tooltip = new SkillTooltip() { Skill = skill };
                    break;

                case Pet pet:
                    Type = SelectableType.Pet;
                    TextureRegion = new(16, 16, 200, 200);
                    _texture = pet.Icon;
                    break;
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (Data is null) return;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (Data is null) return;

            spriteBatch.DrawOnCtrl(this, _texture, bounds, TextureRegion, Color.White);

            if (IsSelected)
                spriteBatch.DrawFrame(this, bounds, Colors.ColonialWhite, 3);

            if (MouseOver)
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
