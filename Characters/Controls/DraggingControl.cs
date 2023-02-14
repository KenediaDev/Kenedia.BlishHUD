using Blish_HUD;
using Blish_HUD.Controls;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using System;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;

namespace Kenedia.Modules.Characters.Controls
{
    public class DraggingControl : Container
    {
        private CharacterCard _characterControl;

        private CharacterCard _internalCharacterCard;
        private bool _layoutRefreshed;
        private double _lastlayoutRefreshed;

        public DraggingControl()
        {
            Parent = GameService.Graphics.SpriteScreen;
            Visible = false;
            ZIndex = int.MaxValue - 1;

            WidthSizingMode = SizingMode.AutoSize;
            HeightSizingMode = SizingMode.AutoSize;
        }

        public CharacterCard CharacterControl
        {
            get => _characterControl;
            set
            {
                if (_characterControl != value)
                {
                    _characterControl = value;
                    _internalCharacterCard?.Dispose();

                    if (value != null)
                    {
                        _internalCharacterCard = new(value)
                        {
                            Parent = this,
                            IsDraggingTarget = true,
                            Enabled = false,
                            Visible = true,
                            BackgroundColor = Color.Black * 0.8f,
                        };

                        _internalCharacterCard.UniformWithAttached(true);
                    }
                    else
                    {
                        _internalCharacterCard?.Dispose();
                        _internalCharacterCard = null;
                    }
                }
            }
        }

        public bool IsActive => _internalCharacterCard != null && Visible;

        public void StartDragging(CharacterCard characterCard)
        {
            CharacterControl = characterCard;
            _lastlayoutRefreshed = Common.Now() + 5;
            _layoutRefreshed = false;
            Show();
        }

        public void EndDragging()
        {
            if (_internalCharacterCard != null)
            {
                _ = RemoveChild(_internalCharacterCard);
                _internalCharacterCard?.Dispose();
                _internalCharacterCard = null;
            }

            _lastlayoutRefreshed = 0;
            _characterControl = null;

            Hide();
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if (_internalCharacterCard != null)
            {
                if (!_layoutRefreshed && Visible && gameTime.TotalGameTime.TotalMilliseconds - _lastlayoutRefreshed >= 0)
                {
                    _lastlayoutRefreshed = gameTime.TotalGameTime.TotalMilliseconds;

                    _layoutRefreshed = true;
                    _internalCharacterCard.UniformWithAttached(true);
                }

                Blish_HUD.Input.MouseHandler m = Input.Mouse;
                Location = new Point(m.Position.X - 15, m.Position.Y - 15);
            }
        }

        protected override void OnHidden(EventArgs e)
        {
            base.OnHidden(e);
        }
    }
}
