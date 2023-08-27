using Blish_HUD;
using Blish_HUD.Settings;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SizingMode = Blish_HUD.Controls.SizingMode;
using ControlFlowDirection = Blish_HUD.Controls.ControlFlowDirection;
using Blish_HUD.Input;
using Microsoft.Xna.Framework.Input;
using Keyboard = Blish_HUD.Controls.Intern.Keyboard;
using Key = Blish_HUD.Controls.Extern.VirtualKeyShort;
using Kenedia.Modules.QoL.Res;
using Blish_HUD.Content;
using System.IO;
using Container = Blish_HUD.Controls.Container;
using SharpDX.Direct2D1.Effects;
using System.Diagnostics;

namespace Kenedia.Modules.QoL.SubModules.GameResets
{
    public class GameResets : SubModule
    {
        private readonly FlowPanel _container;
        private readonly IconLabel _serverTime;
        private readonly IconLabel _serverReset;
        private readonly IconLabel _weeklyReset;

        public GameResets(SettingCollection settings) : base(settings)
        {
            UI_Elements.Add(_container = new()
            {
                Parent = GameService.Graphics.SpriteScreen,
                Visible = Enabled,
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new Vector2(0, 2),
                Location = GameService.Graphics.SpriteScreen.LocalBounds.Center,
                Enabled = false,
                CaptureInput = false,
            });

            _serverTime = new IconLabel()
            {
                Parent = _container,
                Texture = new(517180) { Size = new(GameService.Content.DefaultFont14.LineHeight), TextureRegion = new(4, 4, 24, 24) },
                Text = "00:00:00",
                BasicTooltipText = "Server Time",
                AutoSize = true,
                Enabled = false,
                CaptureInput = false,
            };

            _serverReset = new IconLabel()
            {
                Parent = _container,
                Texture = new(943979) { Size = new(GameService.Content.DefaultFont14.LineHeight) },
                Text = "00:00:00",
                BasicTooltipText = "Server Reset",
                AutoSize = true,
                Enabled = false,
                CaptureInput = false,
            };

            _weeklyReset = new IconLabel()
            {
                Parent = _container,
                Texture = new(156692) { Size = new(GameService.Content.DefaultFont14.LineHeight) },
                Text = "0 days 00:00:00",
                BasicTooltipText = "Weekly Reset",
                AutoSize = true,
                Enabled = false,
                CaptureInput = false,
            };

            SetPositions();
        }

        public override SubModuleType SubModuleType => SubModuleType.GameResets;
        public override void Load()
        {
            base.Load();
            GameService.Gw2Mumble.UI.CompassSizeChanged += UI_CompassSizeChanged;
            GameService.Gw2Mumble.UI.IsCompassTopRightChanged += UI_IsCompassTopRightChanged;
            QoL.ModuleInstance.Services.ClientWindowService.ResolutionChanged += ClientWindowService_ResolutionChanged;
        }

        private void ClientWindowService_ResolutionChanged(object sender, ValueChangedEventArgs<Point> e)
        {
            SetPositions();
        }

        public override void Unload()
        {
            base.Unload();
            GameService.Gw2Mumble.UI.CompassSizeChanged -= UI_CompassSizeChanged;
            GameService.Gw2Mumble.UI.IsCompassTopRightChanged -= UI_IsCompassTopRightChanged;
            if (QoL.ModuleInstance is not null)
                QoL.ModuleInstance.Services.ClientWindowService.ResolutionChanged -= ClientWindowService_ResolutionChanged;
        }

        private void UI_IsCompassTopRightChanged(object sender, ValueEventArgs<bool> e)
        {
            SetPositions();
        }

        private void UI_CompassSizeChanged(object sender, ValueEventArgs<Gw2Sharp.Models.Size> e)
        {
            SetPositions();
        }

        protected override void Enable()
        {
            base.Enable();
            _container.Visible = true;
        }

        protected override void Disable()
        {
            base.Disable();
            _container.Visible = false;
        }

        public override void Update(GameTime gameTime)
        {
            if (!Enabled) return;
            _container.Visible = Enabled && GameService.GameIntegration.Gw2Instance.IsInGame && !GameService.Gw2Mumble.UI.IsMapOpen;
            SetTexts();
            SetPositions();
        }

        private void SetPositions()
        {
            var s = GameService.Gw2Mumble.UI.CompassSize;
            float scale = s.Height / 362F;
            scale = scale < 0.5 ? scale - 0.3F : scale;

            int y = GameService.Gw2Mumble.UI.IsCompassTopRight ?
                s.Height - _container.Height + (int)(24 * scale) :
                GameService.Graphics.SpriteScreen.Height - _container.Height - 60;

            _container.Location = new(
                GameService.Graphics.SpriteScreen.Width - s.Width - (int)(GameService.Gw2Mumble.UI.CompassSize.Width * 0.1),
                y);
        }

        private void SetTexts()
        {
            var now = DateTime.UtcNow;
            var nextDay = DateTime.UtcNow.AddDays(1);
            var nextWeek = DateTime.UtcNow;
            for (int i = 0; i < 8; i++)
            {
                nextWeek = DateTime.UtcNow.AddDays(i);
                if (nextWeek.DayOfWeek == DayOfWeek.Monday && (nextWeek.Day != now.Day || now.Hour < 7 || (now.Hour == 7 && now.Minute < 30))) break;
            }
            var t = new DateTime(nextDay.Year, nextDay.Month, nextDay.Day, 0, 0, 0);
            var w = new DateTime(nextWeek.Year, nextWeek.Month, nextWeek.Day, 7, 30, 0);

            _serverTime.Text = string.Format("{0}:{1:00}", now.Hour, now.Minute);

            var weeklyReset = w.Subtract(now);
            _weeklyReset.Text = string.Format("{1:0} {0} {2:00}:{3:00}:{4:00}", strings.Days, weeklyReset.Days, weeklyReset.Hours, weeklyReset.Minutes, weeklyReset.Seconds);

            var serverReset = t.Subtract(now);
            _serverReset.Text = string.Format("{0:00}:{1:00}:{2:00}", serverReset.Hours, serverReset.Minutes, serverReset.Seconds);
        }
    }
}
