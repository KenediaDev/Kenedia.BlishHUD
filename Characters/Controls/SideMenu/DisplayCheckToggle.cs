using Blish_HUD.Content;
using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Core.Controls;
using Microsoft.Xna.Framework;
using System;
using static Kenedia.Modules.Characters.Services.TextureManager;
using FlowPanel = Kenedia.Modules.Core.Controls.FlowPanel;
using Label = Kenedia.Modules.Core.Controls.Label;

namespace Kenedia.Modules.Characters.Controls.SideMenu
{
    public class DisplayCheckToggle : FlowPanel
    {
        private readonly AsyncTexture2D _eye;
        private readonly AsyncTexture2D _eyeHovered;
        private readonly AsyncTexture2D _telescope;
        private readonly AsyncTexture2D _telescopeHovered;

        private readonly ImageToggle _showButton;
        private readonly ImageToggle _checkButton;
        private readonly Label _textLabel;

        private readonly string _key;
        private readonly Settings _settings;

        private bool _checkChecked;
        private bool _showChecked;

        public event EventHandler<Tuple<bool, bool>> Changed;
        public event EventHandler<bool> ShowChanged;
        public event EventHandler<bool> CheckChanged;

        public DisplayCheckToggle(TextureManager textureManager, bool displayButton_Checked = true, bool checkbox_Checked = true)
        {
            _eye = textureManager.GetControlTexture(ControlTextures.Eye_Button);
            _eyeHovered = textureManager.GetControlTexture(ControlTextures.Eye_Button_Hovered);
            _telescope = textureManager.GetControlTexture(ControlTextures.Telescope_Button);
            _telescopeHovered = textureManager.GetControlTexture(ControlTextures.Telescope_Button_Hovered);

            WidthSizingMode = SizingMode.Fill;
            HeightSizingMode = SizingMode.AutoSize;

            FlowDirection = ControlFlowDirection.SingleLeftToRight;
            ControlPadding = new(5, 0);

            _showButton = new()
            {
                Parent = this,
                ShowX = true,
                Texture = _eye,
                HoveredTexture = _eyeHovered,
                Size = new Point(20, 20),
                Checked = displayButton_Checked,
            };
            ShowChecked = _showButton.Checked;
            _showButton.CheckedChanged += SettingChanged;

            _checkButton = new()
            {
                Parent = this,
                ShowX = true,
                Texture = _telescope,
                HoveredTexture = _telescopeHovered,
                Size = new Point(20, 20),
                Checked = checkbox_Checked,
            };
            CheckChecked = _checkButton.Checked;
            _checkButton.CheckedChanged += SettingChanged;

            _textLabel = new()
            {
                Parent = this,
                Height = 20,
                VerticalAlignment = VerticalAlignment.Middle,
                AutoSizeWidth = true,
            };
        }

        public DisplayCheckToggle(TextureManager textureManager, Settings settings, string key, bool show = true, bool check = true) : this(textureManager, true, true)
        {
            _settings = settings;
            _key = key;

            if (!settings.DisplayToggles.Value.ContainsKey(_key))
            {
                settings.DisplayToggles.Value.Add(_key, new(show, check));
            }

            _showButton.Checked = settings.DisplayToggles.Value[_key].Show;
            ShowChecked = _showButton.Checked;

            _checkButton.Checked = settings.DisplayToggles.Value[_key].Check;
            CheckChecked = _checkButton.Checked;
        }

        public string Text
        {
            get => _textLabel.Text;
            set => _textLabel.Text = value;
        }

        public string CheckTooltip
        {
            get => _checkButton.BasicTooltipText;
            set => _checkButton.BasicTooltipText = value;
        }

        public string DisplayTooltip
        {
            get => _showButton.BasicTooltipText;
            set => _showButton.BasicTooltipText = value;
        }

        public bool CheckChecked { get => _checkChecked; set { _checkChecked = value; _checkButton.Checked = value; } }
        public bool ShowChecked { get => _showChecked; set { _showChecked = value; _showButton.Checked = value; } }

        private void SettingChanged(object sender, CheckChangedEvent e)
        {
            if (_settings != null)
            {
                _settings.DisplayToggles.Value = new(_settings.DisplayToggles.Value)
                {
                    [_key] = new(_showButton.Checked, _checkButton.Checked)
                };
            }

            if (_checkChecked != _checkButton.Checked)
            {
                _checkChecked = _checkButton.Checked;
                CheckChanged?.Invoke(this, _checkButton.Checked);
            }

            if (_showChecked != _showButton.Checked)
            {
                _showChecked = _showButton.Checked;
                ShowChanged?.Invoke(this, _showButton.Checked);
            }

            Changed?.Invoke(this, new(_showButton.Checked, _checkButton.Checked));
        }
    }
}
