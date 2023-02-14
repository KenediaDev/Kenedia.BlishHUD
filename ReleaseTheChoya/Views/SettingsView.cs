using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Res;
using Kenedia.Modules.Core.Services;
using Microsoft.Xna.Framework;
using System;
using Panel = Kenedia.Modules.Core.Controls.Panel;

namespace Kenedia.Modules.ReleaseTheChoya.Views
{
    public class SettingsView : View
    {
        private StandardButton _openSettingsButton;
        private readonly Action _toggleWindow;
        private readonly TexturesService _texturesService;
        private RollingChoya _choya;

#nullable enable
        public SettingsView(Action? toggleWindow, TexturesService texturesService)
        {
            _toggleWindow = toggleWindow;
            _texturesService = texturesService;
        }
#nullable disable

        protected override void Build(Container buildPanel)
        {
            _openSettingsButton = new StandardButton()
            {
                Text = strings_common.OpenSettings,
                Width = 192,
                Parent = buildPanel,
            };

            var p = new Panel()
            {
                Parent = buildPanel,
                Location = new(0, 50),
                Height = buildPanel.Height - 50,
                WidthSizingMode = SizingMode.Fill,
            };

            _choya = new()
            {
                Size = new(buildPanel.Height - 75),
                Parent = p,
                ChoyaTexture = _texturesService.GetTexture(textures_common.RollingChoya, nameof(textures_common.RollingChoya)),
                CanMove= true,
                TravelDistance = new(5, 0),
                Steps = 360,
            };

            _openSettingsButton.Location = new Point(Math.Max((buildPanel.Width / 2) - (_openSettingsButton.Width / 2), 20), (50 - _openSettingsButton.Height) / 2);

            _openSettingsButton.Click += OpenSettingsButton_Click;
        }

        private void OpenSettingsButton_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            _toggleWindow?.Invoke();
        }

        protected override void Unload()
        {
            if (_openSettingsButton != null) _openSettingsButton.Click -= OpenSettingsButton_Click;

            _choya?.Dispose();
            _openSettingsButton?.Dispose();
        }
    }
}
