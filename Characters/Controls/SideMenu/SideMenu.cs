using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Characters.Res;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Characters.Views;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Interfaces;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private readonly SideMenuToggles _toggles;
        private readonly SideMenuBehaviors _behaviors;
        private readonly CharacterSorting _characterSorting;

        public SideMenu(CharacterSorting characterSorting)
        {
            _characterSorting = characterSorting;

            WidthSizingMode = SizingMode.Standard;
            HeightSizingMode = SizingMode.Standard;
            BackgroundColor = Color.Black * 0.4f;
            BackgroundImage = AsyncTexture2D.FromAssetId(156003);
            ZIndex = 11;

            _headerPanel = new()
            {
                Parent = this,
                BackgroundColor = Color.Black * 0.95f,
                WidthSizingMode = SizingMode.Fill,
                Height = 25,
            };

            TabsButtonPanel.Location = new Point(0, _headerPanel.Bottom);
            CreateHeaderButtons();

            Width = 250;
            //Height = 415;
            HeightSizingMode = SizingMode.AutoSize;

            TextureRectangle = new Rectangle(30, 30, BackgroundImage.Width - 60, BackgroundImage.Height - 60);

            AddTab(_toggles = new SideMenuToggles()
            {
                Icon = AsyncTexture2D.FromAssetId(440021),
                Width = Width,
            });

            AddTab(_behaviors = new SideMenuBehaviors()
            {
                Icon = AsyncTexture2D.FromAssetId(156909),
            });
            _ = SwitchTab(_toggles);
        }

        private void CloseButton_Click(object sender, MouseEventArgs e)
        {
            Hide();
        }

        private void CreateHeaderButtons()
        {
            TextureManager tm = Characters.ModuleInstance.TextureManager;

            _ocrButton = new()
            {
                Parent = _headerPanel,
                Texture = tm.GetIcon(TextureManager.Icons.Camera),
                HoveredTexture = tm.GetIcon(TextureManager.Icons.Camera_Hovered),
                Size = new Point(20, 20),
                ClickAction = (m) => ModuleInstance.OCR?.ToggleContainer(),
                SetLocalizedTooltip = () => strings.EditOCR_Tooltip,
            };
            _buttons.Add(_ocrButton);

            _potraitButton = new()
            {
                Parent = _headerPanel,
                Texture = AsyncTexture2D.FromAssetId(358353),
                HoveredTexture = tm.GetIcon(TextureManager.Icons.Portrait_Hovered),
                Size = new Point(20, 20),
                ColorHovered = Color.White,
                SetLocalizedTooltip = () => strings.TogglePortraitCapture_Tooltip,
                ClickAction = (m) => ModuleInstance.PotraitCapture.ToggleVisibility(),
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
                        _characterSorting.Start(ModuleInstance.CharacterModels);
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
                ClickAction = (m) => ModuleInstance.GW2APIHandler.CheckAPI(),
                SetLocalizedTooltip = () => strings.RefreshAPI,
            };
            _buttons.Add(_refreshButton);

            _pinButton = new ImageToggleButton((b) => Settings.PinSideMenus.Value = b)
            {
                Parent = _headerPanel,
                Texture = Characters.ModuleInstance.TextureManager.GetIcon(TextureManager.Icons.Pin),
                HoveredTexture = Characters.ModuleInstance.TextureManager.GetIcon(TextureManager.Icons.Pin_Hovered),
                ActiveTexture = Characters.ModuleInstance.TextureManager.GetIcon(TextureManager.Icons.Pin_Hovered),
                ColorDefault = new Color(175, 175, 175),
                ColorActive = ContentService.Colors.ColonialWhite,
                Size = new(20, 20),
                Active = Settings.PinSideMenus.Value,
                SetLocalizedTooltip= () => strings.PinSideMenus_Description,
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

        private SettingsModel Settings => Characters.ModuleInstance.Settings;

        private Characters ModuleInstance => Characters.ModuleInstance;

        public void ResetToggles()
        {
            _toggles.ResetToggles();
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);
        }

        protected override bool SwitchTab(PanelTab tab = null)
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

            TextureRectangle = new Rectangle(30, 30, Math.Min(BackgroundImage.Width - 100, Width), Math.Min(BackgroundImage.Height - 100, Height));

            int gap = (Width - 6 - (_buttons.Count * 20)) / (_buttons.Count - 1);
            for (int i = 0; i < _buttons.Count; i++)
            {
                Control b = _buttons[i];
                b.Location = new(6 + (i * gap) + (i * b.Width), 3);
            }
        }
    }
}
