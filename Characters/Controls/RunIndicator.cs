using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Settings;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Core.Controls;
using Microsoft.Xna.Framework;
using System;
using Label = Kenedia.Modules.Core.Controls.Label;
using LoadingSpinner = Kenedia.Modules.Core.Controls.LoadingSpinner;

namespace Kenedia.Modules.Characters.Controls
{
    public class RunIndicator : FramedContainer
    {
        private readonly LoadingSpinner _loadingSpinner;
        private readonly ChoyaSpinner _choyaSpinner;

        private readonly Label _titleText;
        private readonly Label _statusText;
        private readonly Label _disclaimerText;

        private Point _screenPartionSize;

        private readonly CharacterSorting _characterSorting;
        private readonly CharacterSwapping _characterSwapping;
        private readonly SettingEntry<bool> _isEnabled;
        private readonly TextureManager _textureManager;
        private readonly SettingEntry<bool> _showChoya;

        public RunIndicator(CharacterSorting characterSorting, CharacterSwapping characterSwapping, SettingEntry<bool> isEnabled, TextureManager textureManager, SettingEntry<bool> showChoya)
        {
            _characterSorting = characterSorting;
            _characterSwapping = characterSwapping;
            _isEnabled = isEnabled;
            _textureManager = textureManager;
            _showChoya = showChoya;
            _screenPartionSize = new(Math.Min(640, GameService.Graphics.SpriteScreen.Size.X / 5), Math.Min(360, GameService.Graphics.SpriteScreen.Size.Y / 5));

            int x = (GameService.Graphics.SpriteScreen.Size.X - _screenPartionSize.X) / 2;
            int y = (GameService.Graphics.SpriteScreen.Size.Y - _screenPartionSize.Y) / 2;

            Parent = GameService.Graphics.SpriteScreen;
            Size = _screenPartionSize;
            Location = new Point(x, y);
            //BorderColor = Color.Black;
            //BorderWidth = new(2);
            //BackgroundImageColor = new Color(43, 43, 43) * 0.9f;

            //Darkish
            BackgroundImage = AsyncTexture2D.FromAssetId(1863949);
            TextureRectangle = new Rectangle(30, 0, BackgroundImage.Width - 30, BackgroundImage.Height);

            //BackgroundImage = AsyncTexture2D.FromAssetId(536041);
            //TextureRectangle = new Rectangle(0, 0, BackgroundImage.Width -200, BackgroundImage.Height-650);
            Visible = false;
            BackgroundImageColor = Color.White * 0.9f;

            _titleText = new()
            {
                Parent = this,
                Location = new Point(0, 10),
                Text = Characters.ModuleName,
                AutoSizeHeight = true,
                HorizontalAlignment = HorizontalAlignment.Center,
                Font = GameService.Content.GetFont(ContentService.FontFace.Menomonia, (ContentService.FontSize)36, ContentService.FontStyle.Regular),
                TextColor = Color.White,
                Width = Width,
            };

            int spinnerSize = Math.Max(_screenPartionSize.Y / 2, 96);
            _loadingSpinner = new()
            {
                Parent = this,
                Size = new(spinnerSize, spinnerSize),
                Location = new((_screenPartionSize.X - spinnerSize) / 2, ((_screenPartionSize.Y - spinnerSize) / 2) - 20),
                Visible = !_showChoya.Value,
            };

            _choyaSpinner = new(_textureManager)
            {
                Parent = this,
                Size = new(_screenPartionSize.X - 20, spinnerSize),
                Location = new(10, ((_screenPartionSize.Y - spinnerSize) / 2) - 20),
                Visible = _showChoya.Value,
            };

            _statusText = new()
            {
                Parent = this,
                Text = "Doing something very fancy right now ...",
                Height = 100,
                HorizontalAlignment = HorizontalAlignment.Center,
                Font = GameService.Content.DefaultFont18,
                Width = Width,
                Location = new(0, Height - 125)
            };

            _disclaimerText = new()
            {
                Parent = this,
                Text = "Any Key or Mouse press will cancel the current action!",
                Height = 50,
                HorizontalAlignment = HorizontalAlignment.Center,
                Font = GameService.Content.GetFont(ContentService.FontFace.Menomonia, (ContentService.FontSize)14, ContentService.FontStyle.Regular),
                Width = Width,
                Location = new(0, Height - 50)
            };
            _characterSwapping.StatusChanged += CharacterSwapping_StatusChanged;
            _characterSorting.StatusChanged += CharacterSorting_StatusChanged;

            _characterSwapping.Started += ShowIndicator;
            _characterSorting.Started += ShowIndicator;

            _characterSwapping.Finished += HideIndicator;
            _characterSorting.Finished += HideIndicator;

            _showChoya.SettingChanged += ShowChoya_SettingChanged;
        }

        private void ShowChoya_SettingChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            _loadingSpinner.Visible = !e.NewValue;
            _choyaSpinner.Visible = e.NewValue;
        }

        private void HideIndicator(object sender, EventArgs e)
        {
            Hide();
        }

        private void ShowIndicator(object sender, EventArgs e)
        {
            if (_isEnabled.Value)
            {
                _screenPartionSize = new(Math.Min(640, GameService.Graphics.SpriteScreen.Size.X / 6), Math.Min(360, GameService.Graphics.SpriteScreen.Size.Y / 6));
                int x = (GameService.Graphics.SpriteScreen.Size.X - _screenPartionSize.X) / 2;
                int y = (GameService.Graphics.SpriteScreen.Size.Y - _screenPartionSize.Y) / 2;
                Location = new Point(x, y);
                Size = _screenPartionSize;

                _titleText.Width = Width;
                _statusText.Width = Width;

                _statusText.Location = new(0, Height - 125);
                int spinnerSize = Math.Min(_screenPartionSize.Y / 2, 96);

                _loadingSpinner.Size = new(spinnerSize, spinnerSize);
                _loadingSpinner.Location = new((_screenPartionSize.X - spinnerSize) / 2, ((_screenPartionSize.Y - spinnerSize) / 2) - 20);

                _choyaSpinner.Size = new(_screenPartionSize.X - 20, spinnerSize);
                _choyaSpinner.Location = new(10, ((_screenPartionSize.Y - spinnerSize) / 2) - 20);

                _disclaimerText.Width = Width;
                _disclaimerText.Location = new(0, Height - 50);

                Invalidate();

                Show();
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            _showChoya.SettingChanged -= ShowChoya_SettingChanged;

            _characterSwapping.StatusChanged -= CharacterSwapping_StatusChanged;
            _characterSorting.StatusChanged -= CharacterSorting_StatusChanged;

            _characterSwapping.Started -= ShowIndicator;
            _characterSorting.Started -= ShowIndicator;

            _characterSwapping.Finished -= HideIndicator;
            _characterSorting.Finished -= HideIndicator;
        }

        private void CharacterSorting_StatusChanged(object sender, EventArgs e)
        {
            _statusText.Text = _characterSorting.Status;
        }

        private void CharacterSwapping_StatusChanged(object sender, EventArgs e)
        {
            _statusText.Text = _characterSwapping.Status;
        }
    }
}
