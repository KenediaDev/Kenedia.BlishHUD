using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.Characters.Res;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Interfaces;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Panel = Kenedia.Modules.Core.Controls.Panel;
using TabbedPanel = Kenedia.Modules.Core.Controls.TabbedPanel;

namespace Kenedia.Modules.Characters.Controls.SideMenu
{
    public class SideMenu : TabbedPanel, ILocalizable
    {
        private readonly Panel _headerPanel;
        private readonly List<Control> _buttons = new();
        private ImageButton _closeButton;
        private ImageToggleButton _pinButton;
        private ImageButton _ocrButton;
        private ImageButton _potraitButton;
        private ImageButton _fixButton;
        private ImageButton _refreshButton;

        private readonly SettingsModel _settings;
        private readonly TextureManager _textureManager;
        private readonly CharacterSorting _characterSorting;
        private readonly Action _toggleOCR;
        private readonly Action _togglePotrait;
        private readonly Action _refreshAPI;

        public SideMenu(Action toggleOCR, Action togglePotrait, Action refreshAPI, TextureManager textureManager, SettingsModel settings, CharacterSorting characterSorting)
        {
            _textureManager = textureManager;
            _toggleOCR = toggleOCR;
            _togglePotrait = togglePotrait;
            _refreshAPI = refreshAPI;
            _settings = settings;
            _characterSorting = characterSorting;

            Parent = GameService.Graphics.SpriteScreen;
            BorderWidth = new(2);
            BorderColor = Color.Black;

            BackgroundColor = Color.Black * 0.4f;
            BackgroundImage = AsyncTexture2D.FromAssetId(156003);
            ZIndex = 11;

            _headerPanel = new()
            {
                Parent = this,
                BackgroundColor = Color.Black * 0.95f,
                Height = 25,
            };

            TabsButtonPanel.Location = new Point(0, _headerPanel.Bottom);
            CreateHeaderButtons();

            Width = 250;
            //Height = 415;
            HeightSizingMode = SizingMode.AutoSize;

            if (BackgroundImage != null) TextureRectangle = new Rectangle(30, 30, BackgroundImage.Width - 60, BackgroundImage.Height - 60);
        }

        public SideMenuToggles TogglesTab { get; set; }

        private void CloseButton_Click(object sender, MouseEventArgs e)
        {
            Hide();
        }

        private void CreateHeaderButtons()
        {
            _ocrButton = new()
            {
                Parent = _headerPanel,
                Texture = _textureManager.GetIcon(TextureManager.Icons.Camera),
                HoveredTexture = _textureManager.GetIcon(TextureManager.Icons.Camera_Hovered),
                Size = new Point(20, 20),
                ClickAction = (m) => _toggleOCR?.Invoke(),
                SetLocalizedTooltip = () => strings.EditOCR_Tooltip,
            };
            _buttons.Add(_ocrButton);

            _potraitButton = new()
            {
                Parent = _headerPanel,
                Texture = AsyncTexture2D.FromAssetId(358353),
                HoveredTexture = _textureManager.GetIcon(TextureManager.Icons.Portrait_Hovered),
                Size = new Point(20, 20),
                ColorHovered = Color.White,
                SetLocalizedTooltip = () => strings.TogglePortraitCapture_Tooltip,
                ClickAction = (m) => _togglePotrait?.Invoke(),
            };
            _buttons.Add(_potraitButton);

            _fixButton = new()
            {
                Parent = _headerPanel,
                Texture = AsyncTexture2D.FromAssetId(156760),
                HoveredTexture = AsyncTexture2D.FromAssetId(156759),
                Size = new Point(20, 20),
                SetLocalizedTooltip = () => strings.FixCharacters_Tooltip,
                ClickAction = (m) =>
                {
                    if (!GameService.GameIntegration.Gw2Instance.IsInGame)
                    {
                        _characterSorting.Start();
                    }
                }
            };
            _buttons.Add(_fixButton);

            _refreshButton = new()
            {
                Parent = _headerPanel,
                Texture = AsyncTexture2D.FromAssetId(156749),
                HoveredTexture = AsyncTexture2D.FromAssetId(156750),
                Size = new Point(20, 20),
                ClickAction = (m) => _refreshAPI?.Invoke(),
                SetLocalizedTooltip = () => strings.RefreshAPI,
            };
            _buttons.Add(_refreshButton);

            _pinButton = new ImageToggleButton((b) => _settings.PinSideMenus.Value = b)
            {
                Parent = _headerPanel,
                Texture = _textureManager.GetIcon(TextureManager.Icons.Pin),
                HoveredTexture = _textureManager.GetIcon(TextureManager.Icons.Pin_Hovered),
                ActiveTexture = _textureManager.GetIcon(TextureManager.Icons.Pin_Hovered),
                ColorDefault = new Color(175, 175, 175),
                ColorActive = ContentService.Colors.ColonialWhite,
                Size = new(20, 20),
                Active = _settings.PinSideMenus.Value,
                SetLocalizedTooltip = () => strings.PinSideMenus_Tooltip,
            };
            _buttons.Add(_pinButton);

            _closeButton = new()
            {
                Parent = _headerPanel,
                Texture = AsyncTexture2D.FromAssetId(156012),
                HoveredTexture = AsyncTexture2D.FromAssetId(156011),
                Size = new Point(20, 20),
                TextureRectangle = new Rectangle(7, 7, 20, 20),
                ClickAction = (m) => Hide(),
                SetLocalizedTooltip = () => strings.Close,
            };
            _buttons.Add(_closeButton);
        }

        public void ResetToggles()
        {
            TogglesTab?.ResetToggles();
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (_headerPanel != null) _headerPanel.Width = ContentRegion.Width;
        }

        public override bool SwitchTab(PanelTab tab = null)
        {
            bool result = base.SwitchTab(tab);

            foreach (PanelTab t in Tabs)
            {
                if (t != tab) t.Height = 5;
            }

            return result;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);

            if (BackgroundImage != null) TextureRectangle = new Rectangle(30, 30, Math.Min(BackgroundImage.Width - 100, Width), Math.Min(BackgroundImage.Height - 100, Height));

            int gap = (_headerPanel.Width - 7 - (_buttons.Count * 20)) / (_buttons.Count - 1);
            for (int i = 0; i < _buttons.Count; i++)
            {
                Control b = _buttons[i];
                b.Location = new(6 + (i * gap) + (i * b.Width), 3);
            }
        }
    }
}
