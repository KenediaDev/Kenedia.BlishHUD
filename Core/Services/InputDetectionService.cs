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
        double _lastInteraction;
        double _lastKeyInteraction;
        double _lastMouseInteraction;
        double _lastMouseMove;
        double _lastMouseClick;
        double _lastClickOrKey;
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
            get => _lastInteraction;
            set
            {
                if (_lastInteraction != value)
                {
                    _lastInteraction = value;
                    Interacted?.Invoke(this, value);
                }
            }
        }

        public double LastKeyInteraction
        {
            get => _lastKeyInteraction;
            set
            {
                if (_lastKeyInteraction != value)
                {
                    _lastKeyInteraction = value;
                    KeyInteracted?.Invoke(this, value);
                }
            }
        }

        public double LastMouseInteraction
        {
            get => _lastMouseInteraction;
            set
            {
                if (_lastMouseInteraction != value)
                {
                    _lastMouseInteraction = value;
                    MouseInteracted?.Invoke(this, value);
                }
            }
        }

        public double LastMouseMove
        {
            get => _lastMouseMove;
            set
            {
                if (_lastMouseMove != value)
                {
                    _lastMouseMove = value;
                    MouseMoved?.Invoke(this, value);
                }
            }
        }

        public double LastMouseClick
        {
            get => _lastMouseClick;
            set
            {
                if (_lastMouseClick != value)
                {
                    _lastMouseClick = value;
                    MouseClicked?.Invoke(this, value);
                }
            }
        }

        public double LastClickOrKey 
        {
            get => _lastClickOrKey;
            set
            {
                if(_lastClickOrKey != value)
                {
                    _lastClickOrKey = value;
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
