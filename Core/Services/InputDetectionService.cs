using Blish_HUD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kenedia.Modules.Core.Services
{
    public class InputDetectionService
    {
        private readonly List<Keys> _ignoredKeys =
        [
            Keys.None,
        ];
        private readonly List<Keys> _noKeys = [];

        private Point _lastMousePosition;

        public InputDetectionService()
        {

        }

        public event EventHandler<double> Interacted;
        public event EventHandler<double> KeyInteracted;
        public event EventHandler<double> MouseInteracted;
        public event EventHandler<double> MouseMoved;
        public event EventHandler<double> MouseClicked;
        public event EventHandler<double> ClickedOrKey;

        public bool Enabled { get; set; } = true;

        public double LastInteraction
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    Interacted?.Invoke(this, value);
                }
            }
        }

        public double LastKeyInteraction
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    KeyInteracted?.Invoke(this, value);
                }
            }
        }

        public double LastMouseInteraction
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    MouseInteracted?.Invoke(this, value);
                }
            }
        }

        public double LastMouseMove
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    MouseMoved?.Invoke(this, value);
                }
            }
        }

        public double LastMouseClick
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    MouseClicked?.Invoke(this, value);
                }
            }
        }

        public double LastClickOrKey 
        {
            get;
            set
            {
                if(field != value)
                {
                    field = value;
                    ClickedOrKey?.Invoke(this, value);
                }
            }
        }

        public void Run(GameTime gameTime)
        {
            if(!Enabled) return;

            double now = gameTime.TotalGameTime.TotalMilliseconds;

            var keys = GameService.Input.Keyboard.KeysDown.Count > 0 ? GameService.Input.Keyboard.KeysDown.Except(_ignoredKeys).Distinct() : _noKeys;
            LastKeyInteraction = keys.Count() > 0 ? now : LastKeyInteraction;

            MouseState mouse = GameService.Input.Mouse.State;
            LastMouseClick = (mouse.LeftButton == ButtonState.Pressed || mouse.RightButton == ButtonState.Pressed || mouse.MiddleButton == ButtonState.Pressed) ? now : LastMouseClick;
            LastMouseMove = mouse.Position != _lastMousePosition ? now : LastMouseMove;
            LastMouseInteraction = Math.Max(LastMouseMove, LastMouseClick);

            LastClickOrKey = Math.Max(LastKeyInteraction, LastMouseClick);
            LastInteraction = Math.Max(LastMouseInteraction, LastKeyInteraction);

            _lastMousePosition = mouse.Position;
        }
    }
}
