using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Core.Controls;
using Microsoft.Xna.Framework;
using System;
using Label = Kenedia.Modules.Core.Controls.Label;

namespace Kenedia.Modules.Characters.Controls
{
    public class RunIndicator : FramedContainer
    {
        private readonly LoadingSpinner _loadingSpinner;
        private readonly Label _titleText;
        private readonly Label _statusText;
        private readonly Label _disclaimerText;

        private Point _screenPartionSize;

        public RunIndicator()
        {
            _screenPartionSize = new(Math.Min(640, GameService.Graphics.SpriteScreen.Size.X / 6), Math.Min(360, GameService.Graphics.SpriteScreen.Size.Y / 6));
            int x = (GameService.Graphics.SpriteScreen.Size.X - _screenPartionSize.X) / 2;
            int y = (GameService.Graphics.SpriteScreen.Size.Y - _screenPartionSize.Y) / 2;

            Parent = GameService.Graphics.SpriteScreen;
            Size = _screenPartionSize;
            Location = new Point(x, y);
            BorderColor = Color.Black;
            BackgroundImage = AsyncTexture2D.FromAssetId(156003);
            BackgroundImageColor = new Color(43, 43, 43) * 0.9f;
            TextureRectangle = new Rectangle(30, 30, 500, 500);
            Visible = false;

            _titleText = new()
            {
                Parent = this,
                Text = Characters.ModuleInstance.Name,
                AutoSizeHeight = true,
                HorizontalAlignment = HorizontalAlignment.Center,
                BackgroundColor = Color.Black * 0.5f,
                Font = GameService.Content.DefaultFont32,
                Width = Width,
            };

            int spinnerSize = Math.Min(_screenPartionSize.Y / 2, 96);
            _loadingSpinner = new()
            {
                Parent = this,
                Size = new(spinnerSize, spinnerSize),
                Location = new((_screenPartionSize.X - spinnerSize) / 2, ((_screenPartionSize.Y - spinnerSize) / 2) - 30)
            };

            _statusText = new()
            {
                Parent = this,
                Text = "Doing something very fancy right now ...",
                Height= 100,
                HorizontalAlignment = HorizontalAlignment.Center,
                Font = GameService.Content.DefaultFont18,
                Width = Width,
                Location = new(0, Height - 125)
            };

            _disclaimerText = new()
            {
                Parent = this,
                Text = "Any Key or Mouse press will cancel the current action!",
                Height= 50,
                HorizontalAlignment = HorizontalAlignment.Center,
                Font = GameService.Content.GetFont(ContentService.FontFace.Menomonia, (ContentService.FontSize)14, ContentService.FontStyle.Italic),
                Width = Width,
                Location = new(0, Height - 50)
            };
            CharacterSwapping.StatusChanged += CharacterSwapping_StatusChanged;
            CharacterSorting.StatusChanged += CharacterSorting_StatusChanged;

            CharacterSwapping.Started += ShowIndicator;
            CharacterSorting.Started += ShowIndicator;

            CharacterSwapping.Finished += HideIndicator;
            CharacterSorting.Finished += HideIndicator;
        }

        private void HideIndicator(object sender, EventArgs e)
        {
            Hide();
        }

        private void ShowIndicator(object sender, EventArgs e)
        {
            if(Characters.ModuleInstance.Settings.ShowStatusWindow.Value)
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
                _loadingSpinner.Location = new((_screenPartionSize.X - spinnerSize) / 2, ((_screenPartionSize.Y - spinnerSize) / 2) - 30);

                _disclaimerText.Width = Width;
                _disclaimerText.Location = new(0, Height - 50);

                Invalidate();

                Show();
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            CharacterSwapping.StatusChanged -= CharacterSwapping_StatusChanged;
            CharacterSorting.StatusChanged -= CharacterSorting_StatusChanged;

            CharacterSwapping.Started -= ShowIndicator;
            CharacterSorting.Started -= ShowIndicator;

            CharacterSwapping.Finished -= HideIndicator;
            CharacterSorting.Finished -= HideIndicator;
        }

        private void CharacterSorting_StatusChanged(object sender, EventArgs e)
        {
            _statusText.Text = CharacterSorting.Status;
        }

        private void CharacterSwapping_StatusChanged(object sender, EventArgs e)
        {
            _statusText.Text = CharacterSwapping.Status;
        }
    }
}
