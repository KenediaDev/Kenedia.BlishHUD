using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Res;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Services;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Kenedia.Modules.Characters.Controls
{
    public enum NotificationType
    {
        Unkown,
        OCR,
        InvalidAPI,
        CharacterDeleted = 10,
    }

    public class BaseNotification : Control
    {
        private static int s_counter = 0;

        public BaseNotification()
        {
            s_counter++;
            Id = s_counter;
        }

        public NotificationType NotificationType { get; protected set; }

        public int Id { get; protected set; }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
        }
    }

    public class OCRSetupNotification : BaseNotification
    {
        private Rectangle _textRectangle;
        private DetailedTexture _settingsCog = new(155052, 157110);
        private DetailedTexture _dismiss = new(156012, 156011)
        {
            TextureRegion = new(4, 4, 24, 24)
        };

        public Action ClickAction { get; set; }

        public string Resolution { get; internal set; }

        public OCRSetupNotification()
        {
            NotificationType = NotificationType.OCR;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _dismiss.Bounds = new(0, 0, Height, Height);
            _settingsCog.Bounds = new(_dismiss.Bounds.Right + 2, 0, Height, Height);
            _textRectangle = new(_settingsCog.Bounds.Right + 6, 0, Height - _settingsCog.Bounds.Right - 6, Height);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            string txt = string.Empty;

            _dismiss.Draw(this, spriteBatch, RelativeMousePosition);
            _settingsCog.Draw(this, spriteBatch, RelativeMousePosition);
            spriteBatch.DrawStringOnCtrl(this, string.Format(strings.OCRNotification, Resolution), GameService.Content.DefaultFont14, _textRectangle, Color.White, false, HorizontalAlignment.Left, VerticalAlignment.Middle);

            BasicTooltipText = txt;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if (_settingsCog.Hovered)
            {
                ClickAction?.Invoke();
            }

            if (_dismiss.Hovered)
            {
                var p = Parent;

                Dispose();
                p?.Invalidate();
            }
        }
    }

    public class CharacterDeletedNotification : BaseNotification
    {
        private Rectangle _textRectangle;
        private DetailedTexture _delete = new(358366, 358367);

        private DetailedTexture _dismiss = new(156012, 156011)
        {
            TextureRegion = new(4, 4, 24, 24)
        };
        private Character_Model _markedCharacter;

        public CharacterDeletedNotification()
        {
            NotificationType = NotificationType.CharacterDeleted;
        }

        public Character_Model MarkedCharacter { get => _markedCharacter; set => Common.SetProperty(ref _markedCharacter, value, RecalculateLayout); }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _dismiss.Bounds = new(0, 0, Height, Height);
            _delete.Bounds = new(_dismiss.Bounds.Right + 2, 0, Height, Height);
            _textRectangle = new(_delete.Bounds.Right + 6, 0, Height - _delete.Bounds.Right - 6, Height);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            string txt = string.Empty;

            if (MarkedCharacter != null)
            {
                _delete.Draw(this, spriteBatch, RelativeMousePosition);
                if (_delete.Hovered)
                {
                    txt = string.Format(strings.DeletedCharacterNotification_DeleteTooltip, MarkedCharacter.Name);
                }

                _dismiss.Draw(this, spriteBatch, RelativeMousePosition);
                if (_dismiss.Hovered)
                {
                    txt = string.Format(strings.DeletedCharacterNotification_DismissTooltip, MarkedCharacter.Name);
                }

                spriteBatch.DrawStringOnCtrl(this, string.Format(strings.DeletedCharacterNotification, MarkedCharacter.Name, MarkedCharacter.Created.ToString("d")), GameService.Content.DefaultFont14, _textRectangle, Color.White, false, HorizontalAlignment.Left, VerticalAlignment.Middle);
            }

            BasicTooltipText = txt;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if (MarkedCharacter != null)
            {
                if (_delete.Hovered)
                {
                    MarkedCharacter.Delete();
                    Dispose();
                }

                if (_dismiss.Hovered)
                {
                    var p = Parent;
                    Dispose();
                    p?.Invalidate();
                }
            }
        }
    }

    public class NotificationPanel : FlowPanel
    {
        private readonly ObservableCollection<Character_Model> _characters;
        private readonly List<(Character_Model character, CharacterDeletedNotification control)> _markedCharacters = new();

        public NotificationPanel(ObservableCollection<Character_Model> characters) : base()
        {
            Height = 100;
            Width = 400;
            FlowDirection = ControlFlowDirection.SingleTopToBottom;

            CanScroll = true;

            _characters = characters;
            UpdateCharacters();
        }
        public Point MaxSize { get; set; } = Point.Zero;

        public void UpdateCharacters()
        {
            var addedCharacters = _markedCharacters.Select(e => e.character).ToList();

            foreach (Character_Model character in _characters)
            {
                if (character.MarkedAsDeleted && addedCharacters.Find(e => e == character) == null)
                {
                    _markedCharacters.Add(new(character, new()
                    {
                        Parent = this,
                        Height = 25,
                        MarkedCharacter = character,
                    }));
                }
            }

            RecalculateLayout();
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            SortChildren<BaseNotification>((a, b) =>
            {
                int type = a.NotificationType.CompareTo(b.NotificationType);
                int index = a.Id.CompareTo(b.Id);
                return type == 0 ? type + index : type;
            });

            foreach (var character in Children)
            {
                character.Width = ContentRegion.Width;
            }

            Height = MaxSize.Y > -1 ? Math.Min(MaxSize.Y, Children.Count > 0 ? Children.Max(e => e.Bottom) : 0) : Children.Count > 0 ? Children.Max(e => e.Bottom) : 0;

            if (Children == null || Children.Count == 0 || Children?.Where(e => e.Visible)?.Count() <= 0)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        protected override void OnChildRemoved(ChildChangedEventArgs e)
        {
            base.OnChildRemoved(e);

            Invalidate();
            Parent?.Invalidate();

            if (Children == null || Children.Count == 0 || Children?.Where(e => e.Visible)?.Count() <= 0)
            {
                Hide();
            }
        }

        protected override void OnChildAdded(ChildChangedEventArgs e)
        {
            base.OnChildAdded(e);
            Show();

            Invalidate();
            Parent?.Invalidate();
        }
    }
}
