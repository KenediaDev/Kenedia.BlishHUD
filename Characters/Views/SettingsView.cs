using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Characters.Res;
using Microsoft.Xna.Framework;
using System;

namespace Kenedia.Modules.Characters.Views
{
    public class SettingsView : View
    {
        private StandardButton _openSettingsButton;

        public SettingsView() 
        { 
        }

        protected override void Build(Container buildPanel)
        {
            _openSettingsButton = new StandardButton()
            {
                Text = string.Format(strings.OpenItem, strings.Settings),
                Width = 192,
                Parent = buildPanel,
            };

            _openSettingsButton.Location = new Point(Math.Max((buildPanel.Width / 2) - (_openSettingsButton.Width / 2), 20), Math.Max((buildPanel.Height / 2) - _openSettingsButton.Height, 20) - _openSettingsButton.Height - 10);

            _openSettingsButton.Click += OpenSettingsButton_Click;
        }

        private void OpenSettingsButton_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            Characters.ModuleInstance.SettingsWindow?.ToggleWindow();
        }

        protected override void Unload()
        {
            if (_openSettingsButton != null) _openSettingsButton.Click -= OpenSettingsButton_Click;
        }
    }
}
