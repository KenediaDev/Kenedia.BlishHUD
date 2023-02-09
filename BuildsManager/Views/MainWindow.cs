using Blish_HUD.Content;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Views;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kenedia.Modules.BuildsManager.Views
{
    public class MainWindow : StandardWindow
    {
        private readonly Data _data;
        private readonly FlowPanel _contentPanel;

        public MainWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion, Data data) : base(background, windowRegion, contentRegion)
        {
            _data = data;

            _contentPanel = new FlowPanel()
            {
                Parent = this,
                WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize,
            };

            foreach (var item in _data.Weapons)
            {
                _ = new Image()
                {
                    Parent = _contentPanel,
                    Texture = item.Value.Icon,
                    SetLocalizedTooltip = () => item.Value.Name,
                    Size = new(64),
                };
            }

            foreach (var item in _data.Armors)
            {
                _ = new Image()
                {
                    Parent = _contentPanel,
                    Texture = item.Value.Icon,
                    SetLocalizedTooltip = () => item.Value.Name,
                    Size = new(64),
                };
            }

            foreach (var item in _data.Trinkets)
            {
                _ = new Image()
                {
                    Parent = _contentPanel,
                    Texture = item.Value.Icon,
                    SetLocalizedTooltip = () => item.Value.Name,
                    Size = new(64),
                };
            }

            foreach (var item in _data.Upgrades)
            {
                _ = new Image()
                {
                    Parent = _contentPanel,
                    Texture = item.Value.Icon,
                    SetLocalizedTooltip = () => item.Value.Name,
                    Size = new(64),
                };
            }
        }
    }
}
