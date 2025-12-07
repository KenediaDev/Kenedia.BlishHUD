using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Core.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Linq;
using Color = Microsoft.Xna.Framework.Color;
using FlowPanel = Kenedia.Modules.Core.Controls.FlowPanel;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Kenedia.Modules.Core.Utility;

namespace Kenedia.Modules.Characters.Controls
{
    public class CharacterTooltip : FramedContainer
    {
        private readonly AsyncTexture2D _iconFrame = AsyncTexture2D.FromAssetId(1414041);
        private readonly FlowPanel _contentPanel;
        private readonly Dummy _iconDummy;
        private readonly CharacterLabels _infoLabels;

        private readonly Func<Character_Model> _currentCharacter;
        private readonly TextureManager _textureManager;
        private readonly Data _data;
        private Point _textureOffset = new(25, 25);
        private readonly List<Tag> _tags = [];

        public CharacterTooltip(Func<Character_Model> currentCharacter, TextureManager textureManager, Data data, Settings settings)
        {
            _currentCharacter = currentCharacter;
            _textureManager = textureManager;
            _data = data;
            TextureRectangle = new Rectangle(60, 25, 250, 250);
            BackgroundImage = AsyncTexture2D.FromAssetId(156003);
            BorderColor = Color.Black;
            BorderWidth = new(2);
            BackgroundColor = Color.Black * 0.6f;

            HeightSizingMode = SizingMode.AutoSize;

            AutoSizePadding = new Point(5, 5);

            _contentPanel = new FlowPanel()
            {
                Parent = this,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new Vector2(5, 2),
                OuterControlPadding = new Vector2(5, 0),
                AutoSizePadding = new Point(5, 5),
            };
            _iconDummy = new Dummy()
            {
                Parent = this,
            };

            _infoLabels = new(_contentPanel)
            {
                Settings = settings,
                Data = data,
                TextureManager = textureManager,
                CurrentCharacter = _currentCharacter,
            };

            _infoLabels.UpdateDataControlsVisibility(true);
        }

        public Color BackgroundTint { get; set; } = Color.Honeydew * 0.95f;

        public BitmapFont Font { get; set; } = GameService.Content.DefaultFont14;

        public BitmapFont NameFont { get; set; } = GameService.Content.DefaultFont18;

        public Character_Model Character
        {
            get;
            set
            {
                _infoLabels.Character = value;

                var temp = field;
                if (Common.SetProperty(ref field, value))
                {
                    if (temp is not null) temp.Updated -= Character_Updated;
                    if (field is not null) field.Updated += Character_Updated;
                }
            }
        }

        private void Character_Updated(object sender, EventArgs e)
        {
            _infoLabels.UpdateDataControlsVisibility(true);
            _infoLabels.Update();
            UpdateSize();
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);
            Location = new Point(Input.Mouse.Position.X, Input.Mouse.Position.Y + 35);

        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);
        }

        public void UpdateSize()
        {
            _infoLabels.UpdateDataControlsVisibility(true);
            IEnumerable<Control> visibleControls = _infoLabels.DataControls.Where(e => e.Visible);
            int amount = visibleControls.Count();

            int height = amount > 0 ? visibleControls.Aggregate(0, (result, ctrl) => result + ctrl.Height + (int)_contentPanel.ControlPadding.Y) : 0;
            int width = amount > 0 ? visibleControls.Max(ctrl => ctrl != _infoLabels.TagPanel ? ctrl.Width : 0) : 0;

            _contentPanel.Height = height;
            _contentPanel.Width = Width;

            _infoLabels.TagPanel.FitWidestTag(Width - 10);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            Location = new Point(Input.Mouse.Position.X, Input.Mouse.Position.Y + 35);
            Character_Updated(this, null);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            if (Character is not null) Character.Updated -= Character_Updated;
        }
    }
}