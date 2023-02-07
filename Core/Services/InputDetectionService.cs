using Blish_HUD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kenedia.Modules.Core.Services
{
    public class InputDetectionService
    {
        double _lastInteraction;
        double _lastKeyInteraction;
        double _lastMouseInteraction;
        double _lastMouseMove;
        double _lastMouseClick;

        private Point _lastMousePosition;

        public InputDetectionService()
        {

        }

        public event EventHandler<double> Interacted;
        public event EventHandler<double> KeyInteracted;
        public event EventHandler<double> MouseInteracted;
        public event EventHandler<double> MouseMoved;
        public event EventHandler<double> MouseClicked;

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

        public void Run(GameTime gameTime)
        {
            double now = gameTime.TotalGameTime.TotalMilliseconds;

            LastKeyInteraction = GameService.Input.Keyboard.KeysDown.Count > 0 ? now : LastKeyInteraction;

            MouseState mouse = GameService.Input.Mouse.State;
            LastMouseClick = (mouse.LeftButton == ButtonState.Pressed || mouse.RightButton == ButtonState.Pressed) ? now : LastMouseClick;
            LastMouseMove = mouse.Position != _lastMousePosition ? now : LastMouseMove;
            LastMouseInteraction = Math.Max(LastMouseMove, LastMouseClick);
            LastInteraction = Math.Max(LastMouseInteraction, LastKeyInteraction);

            _lastMousePosition = mouse.Position;
        }
    }
}
