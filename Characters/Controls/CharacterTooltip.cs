using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Res;
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

namespace Kenedia.Modules.Characters.Controls
{
    public class CharacterTooltip : FramedContainer
    {
        private readonly AsyncTexture2D _iconFrame = AsyncTexture2D.FromAssetId(1414041);
        private readonly FlowPanel _contentPanel;
        private readonly Dummy _iconDummy;

        private readonly IconLabel _nameLabel;
        private readonly IconLabel _levelLabel;
        private readonly IconLabel _professionLabel;
        private readonly IconLabel _genderLabel;
        private readonly IconLabel _raceLabel;
        private readonly IconLabel _mapLabel;
        private readonly IconLabel _lastLoginLabel;
        private readonly TagFlowPanel _tagPanel;

        private readonly CraftingControl _craftingControl;
        private readonly List<Control> _dataControls;
        private readonly Func<Character_Model> _currentCharacter;
        private readonly TextureManager _textureManager;
        private readonly Data _data;
        private Rectangle _iconRectangle;
        private Rectangle _contentRectangle;

        private Point _textureOffset = new(25, 25);
        private Character_Model _character;
        private readonly List<Tag> _tags = new();
        private bool _updateCharacter;

        public CharacterTooltip(Func<Character_Model> currentCharacter, TextureManager textureManager, Data data, SettingsModel settings)
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

            _nameLabel = new IconLabel()
            {
                Parent = _contentPanel,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };

            _levelLabel = new IconLabel()
            {
                Parent = _contentPanel,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };
            _genderLabel = new IconLabel()
            {
                Parent = _contentPanel,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };
            _raceLabel = new IconLabel()
            {
                Parent = _contentPanel,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };
            _professionLabel = new IconLabel()
            {
                Parent = _contentPanel,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };
            _mapLabel = new IconLabel()
            {
                Parent = _contentPanel,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };

            _craftingControl = new CraftingControl(data, settings)
            {
                Parent = _contentPanel,
                Width = _contentPanel.Width,
                Height = 20,
                Character = Character,
            };

            _lastLoginLabel = new IconLabel()
            {
                Parent = _contentPanel,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };

            _tagPanel = new()
            {
                Parent = _contentPanel,
                Font = _lastLoginLabel.Font,
                FlowDirection = ControlFlowDirection.LeftToRight,
                ControlPadding = new Vector2(3, 2),
                HeightSizingMode = SizingMode.AutoSize,
                Visible = false,
            };

            _dataControls = new List<Control>()
            {
                _nameLabel,
                _levelLabel,
                _genderLabel,
                _raceLabel,
                _professionLabel,
                _mapLabel,
                _lastLoginLabel,
                _craftingControl,
                _tagPanel,
            };
        }

        public Color BackgroundTint { get; set; } = Color.Honeydew * 0.95f;

        public BitmapFont Font { get; set; } = GameService.Content.DefaultFont14;

        public BitmapFont NameFont { get; set; } = GameService.Content.DefaultFont18;

        public Character_Model Character
        {
            get => _character;
            set
            {
                if (_character != value)
                {
                    if (_character != null)
                    {
                        _character.Updated -= ApplyCharacter;
                    }

                    _character = value;

                    if (value != null)
                    {
                        _character.Updated += ApplyCharacter;
                        UpdateCharacterInfo();
                    }
                }
            }
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);
            Location = new Point(Input.Mouse.Position.X, Input.Mouse.Position.Y + 35);

            if (Character != null && _lastLoginLabel.Visible && _currentCharacter?.Invoke() != Character)
            {
                TimeSpan ts = DateTimeOffset.UtcNow.Subtract(Character.LastLogin);
                _lastLoginLabel.Text = string.Format("{1} {0} {2:00}:{3:00}:{4:00}", strings.Days, Math.Floor(ts.TotalDays), ts.Hours, ts.Minutes, ts.Seconds);
            }
            UpdateLayout();
            if (_updateCharacter && Visible)
            {
                UpdateCharacterInfo();
            }
        }

        public void UpdateLayout()
        {
            UpdateLabelLayout();
            UpdateSize();

            _contentRectangle = new Rectangle(Point.Zero, _contentPanel.Size);
            _contentPanel.Location = _contentRectangle.Location;
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);
        }

        public void UpdateLabelLayout()
        {
            _nameLabel.Visible = true;
            _nameLabel.Font = NameFont;

            _levelLabel.Visible = true;
            _levelLabel.Font = Font;

            _professionLabel.Visible = true;
            _professionLabel.Font = Font;

            _genderLabel.Visible = true;
            _genderLabel.Font = Font;

            _raceLabel.Visible = true;
            _raceLabel.Font = Font;

            _mapLabel.Visible = true;
            _mapLabel.Font = Font;

            _lastLoginLabel.Visible = true;
            _lastLoginLabel.Font = Font;

            _craftingControl.Visible = true;
            _craftingControl.Font = Font;

            _tagPanel.Visible = Character.Tags.Count > 0;
            _tagPanel.Font = Font;

            _craftingControl.Height = Font.LineHeight + 2;
        }

        public void UpdateSize()
        {
            IEnumerable<Control> visibleControls = _dataControls.Where(e => e.Visible);
            int amount = visibleControls.Count();

            int height = visibleControls.Count() > 0 ? visibleControls.Aggregate(0, (result, ctrl) => result + ctrl.Height + (int)_contentPanel.ControlPadding.Y) : 0;
            int width = visibleControls.Count() > 0 ? visibleControls.Max(ctrl => ctrl != _tagPanel ?  ctrl.Width : 0) : 0;

            _contentPanel.Height = height;
            _contentPanel.Width = Width;

            _tagPanel.FitWidestTag(Width - 10);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            Location = new Point(Input.Mouse.Position.X, Input.Mouse.Position.Y + 35);
        }

        private void ApplyCharacter(object sender, EventArgs e)
        {
            _updateCharacter = true;
        }

        private void UpdateCharacterInfo()
        {
            _updateCharacter = false;

            _nameLabel.Text = Character.Name;
            _nameLabel.TextColor = new Color(168 + 15 + 25, 143 + 20 + 25, 102 + 15 + 25, 255);

            _levelLabel.Text = string.Format(strings.LevelAmount, Character.Level);
            _levelLabel.TextureRectangle = new Rectangle(2, 2, 28, 28);
            _levelLabel.Icon = AsyncTexture2D.FromAssetId(157085);

            _professionLabel.Icon = Character.SpecializationIcon;
            _professionLabel.Text = Character.SpecializationName;

            if (_professionLabel.Icon != null)
            {
                _professionLabel.TextureRectangle = _professionLabel.Icon.Width == 32 ? new Rectangle(2, 2, 28, 28) : new Rectangle(4, 4, 56, 56);
            }

            _genderLabel.Text = Character.Gender.ToString();
            _genderLabel.Icon = _textureManager.GetIcon(TextureManager.Icons.Gender);

            _raceLabel.Text = _data.Races[Character.Race].Name;
            _raceLabel.Icon = _data.Races[Character.Race].Icon;

            _mapLabel.Text = _data.GetMapById(Character.Map).Name;
            _mapLabel.TextureRectangle = new Rectangle(2, 2, 28, 28);
            _mapLabel.Icon = AsyncTexture2D.FromAssetId(358406); // 358406 //517180 //157122;

            _lastLoginLabel.Icon = AsyncTexture2D.FromAssetId(841721);
            _lastLoginLabel.Text = string.Format("{1} {0} {2:00}:{3:00}:{4:00}", strings.Days, 0, 0, 0, 0);
            _lastLoginLabel.TextureRectangle = Rectangle.Empty;

            var tagLlist = _tags.Select(e => e.Text);
            var characterTags = Character.Tags.ToList();

            var deleteTags = tagLlist.Except(characterTags);
            var addTags = characterTags.Except(tagLlist);

            bool tagChanged = deleteTags.Any() || addTags.Any();

            if (tagChanged)
            {
                var deleteList = new List<Tag>();
                foreach (string tag in deleteTags)
                {
                    var t = _tags.FirstOrDefault(e => e.Text == tag);
                    if (t != null) deleteList.Add(t);
                }

                foreach (var t in deleteList)
                {
                    t.Dispose();
                    _ = _tags.Remove(t);
                }

                foreach (string tag in addTags)
                {
                    _tags.Add(new Tag()
                    {
                        Parent = _tagPanel,
                        Text = tag,
                        Active = true,
                        ShowDelete = false,
                        CanInteract = false,
                    });
                }
            }

            _craftingControl.Character = Character;
            UpdateLabelLayout();
            UpdateSize();

            // UpdateLayout();
        }
    }
}
