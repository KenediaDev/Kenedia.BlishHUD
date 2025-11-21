using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Structs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Controls
{
    public class RadialMenu : Core.Controls.RadialMenu
    {
        private readonly Data _data;
        private readonly AsyncTexture2D _dummy = AsyncTexture2D.FromAssetId(1128572);
        private readonly CharacterTooltip _tooltip;

        private readonly Settings _settings;
        private readonly ObservableCollection<Character_Model> _characters;
        private readonly Func<Character_Model> _currentCharacter;
        private List<Character_Model> _displayedCharacters;
        private int _iconSize;

        private readonly List<RadialMenuSection> _sections = [];
        private Character_Model? _selected;

        private Vector2 _center;
        private GraphicsDevice _graphicsDevice;
        private BasicEffect _effect;

        public RadialMenu(Settings settings, ObservableCollection<Character_Model> characters, Container parent, Func<Character_Model> currentCharacter, Data data, TextureManager textureManager) : base()
        {
            _settings = settings;

            _settings.Radial_SliceHighlight.PropertyChanged += Radial_Colors_PropertyChanged;
            _settings.Radial_SliceBackground.PropertyChanged += Radial_Colors_PropertyChanged;

            _characters = characters;
            _currentCharacter = currentCharacter;
            _data = data;
            Parent = parent;

            _tooltip = new(currentCharacter, textureManager, data, settings)
            {
                Parent = GameService.Graphics.SpriteScreen,
                ZIndex = int.MaxValue / 2 + 1,
                Size = new Point(300, 50),
                Visible = false,
            };

            BackgroundColor = Color.Transparent * 0.2F;

            foreach (Character_Model c in _characters)
            {
                c.Updated += Character_Updated;
            }

            Parent.Resized += Parent_Resized;
            Input.Keyboard.KeyPressed += Keyboard_KeyPressed;

            SliceBackground = _settings.Radial_SliceBackground.Value;
            SliceHighlight = _settings.Radial_SliceHighlight.Value;
        }

        private void Radial_Colors_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SliceBackground = _settings.Radial_SliceBackground.Value;
            SliceHighlight = _settings.Radial_SliceHighlight.Value;
        }

        public void SetDisplayedCharacters()
        {
            _displayedCharacters = _characters?.Where(e => e.ShowOnRadial).ToList() ?? [];
            Slices = _displayedCharacters.Count;
        }

        public bool HasDisplayedCharacters()
        {
            return _displayedCharacters.Count > 0;
        }

        private void Keyboard_KeyPressed(object sender, KeyboardEventArgs e)
        {
            if (e.Key == Microsoft.Xna.Framework.Input.Keys.Escape)
            {
                Hide();
            }
        }

        private void Parent_Resized(object sender, ResizedEventArgs e)
        {
            RecalculateLayout();
        }

        private void Character_Updated(object sender, EventArgs e)
        {
            RecalculateLayout();
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (_graphicsDevice is null)
            {
                return;
            }
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            base.OnMouseMoved(e);

            //_selected = null;
            //_tooltip.Character = null;
        }

        public override void DoUpdate(GameTime gameTime)
        {
            base.DoUpdate(gameTime);

            if (!Input.Keyboard.KeysDown.Contains(_settings.RadialKey.Value.PrimaryKey))
            {
                Hide();
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            _tooltip?.Hide();
            base.Paint(spriteBatch, bounds);
        }

        protected override ColorGradient GetSliceColors(int index, bool contains_mouse)
        {
            if (_settings.Radial_UseProfessionColor.Value && _displayedCharacters.Count > index)
            {
                var character = _displayedCharacters[index];
                return new ColorGradient(character.Profession.GetProfessionColor() * (contains_mouse ? 0.8F : 0.5F)) ;
            }

            return base.GetSliceColors(index, contains_mouse);
        }

        protected override void DrawSliceContent(SpriteBatch spriteBatch, bool contains_mouse, Vector2 center, float midAngle, float iconRadius, int sliceIndex)
        {
            if (_displayedCharacters.Count <= sliceIndex) return;

            var character = _displayedCharacters[sliceIndex];
            var texture = _settings.Radial_UseProfessionIcons.Value
                ? character.SpecializationIcon ?? character.ProfessionIcon ?? _dummy
                : character.Icon ?? character.ProfessionIcon;

            if (_settings.Radial_UseProfessionIconsColor.Value)
            {
                texture = character.SpecializationIcon ?? texture;
            }

            // Slice geometry
            int totalSlices = _displayedCharacters.Count;
            float sliceAngle = MathHelper.TwoPi / totalSlices;

            // Available chord width at iconRadius
            float sliceWidthAtCenter = 2f * iconRadius * (float)Math.Sin(sliceAngle / 2f);

            // Icon size based on that width
            float iconSize = sliceWidthAtCenter * 0.75f;
            float scale = iconSize / (float)texture.Width;

            // Place icon at the given midAngle and radius
            Vector2 iconOffset = new Vector2(
                (float)Math.Cos(midAngle),
                (float)Math.Sin(midAngle)
            ) * iconRadius;

            Vector2 relativeCenter = center / DpiScale - (texture.Bounds.Size.ToVector2() * scale / 2);

            var color = _settings.Radial_UseProfessionIconsColor.Value ? character.Profession.GetProfessionColor() : Color.White;
            spriteBatch.Draw(texture, relativeCenter, texture.Bounds, color, 0, Vector2.Zero, scale, SpriteEffects.None, 1);

            // Tooltip handling
            if (contains_mouse)
            {
                if (_settings.Radial_ShowAdvancedTooltip.Value)
                {
                    BasicTooltipText = string.Empty;
                    _selected = character;
                    _tooltip.Character = character;
                    _tooltip.Show();
                }
                else
                {
                    BasicTooltipText = character.Name;
                }
            }
        }

        protected override async void OnSliceClick(int i)
        {
            if (_selected is not null)
            {
                Hide();

                if (_displayedCharacters.Count > i)
                {
                    var character = _displayedCharacters[i];

                    if (await ExtendedInputService.WaitForNoKeyPressed())
                    {
                        character.Swap();
                    }
                }
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            RecalculateLayout();
        }

        protected override void OnHidden(EventArgs e)
        {
            base.OnHidden(e);
            _tooltip.Character = null;
            _tooltip.Hide();
        }

        protected override void DisposeControl()
        {
            if (Parent is not null) Parent.Resized -= Parent_Resized;

            foreach (Character_Model c in _characters)
            {
                c.Updated -= Character_Updated;
            }

            _tooltip?.Dispose();
            base.DisposeControl();
        }
    }
}
