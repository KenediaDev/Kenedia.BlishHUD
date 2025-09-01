using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Controls.Intern;
using Blish_HUD.Input;
using Kenedia.Modules.Characters.Extensions;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Core.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using static System.Collections.Specialized.BitVector32;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Controls
{
    public class RadialMenu : Control
    {
        private readonly Data _data;
        private readonly AsyncTexture2D _dummy = AsyncTexture2D.FromAssetId(1128572);
        private readonly CharacterTooltip _tooltip;

        private readonly Settings _settings;
        private readonly ObservableCollection<Character_Model> _characters;
        private readonly Func<Character_Model> _currentCharacter;
        private List<Character_Model> _displayedCharacters;
        private int _iconSize;

        private readonly List<RadialMenuSection> _sections = new();
        private Character_Model? _selected;

        private Vector2 _center;
        private GraphicsDevice _graphicsDevice;
        private BasicEffect _effect;

        public RadialMenu(Settings settings, ObservableCollection<Character_Model> characters, Container parent, Func<Character_Model> currentCharacter, Data data, TextureManager textureManager)
        {
            _settings = settings;
            _characters = characters;
            _currentCharacter = currentCharacter;
            _data = data;
            Parent = parent;

            _tooltip = new(currentCharacter, textureManager, data, settings)
            {
                Parent = GameService.Graphics.SpriteScreen,
                ZIndex = int.MaxValue / 2 + 1,
                Size = new Point(300, 50),
                Visible = false,
            };

            BackgroundColor = Color.Transparent;

            foreach (Character_Model c in _characters)
            {
                c.Updated += Character_Updated;
            }

            Parent.Resized += Parent_Resized;
            Input.Keyboard.KeyPressed += Keyboard_KeyPressed;

            _graphicsDevice = GameService.Graphics.LendGraphicsDeviceContext().GraphicsDevice;
            _effect = new BasicEffect(_graphicsDevice)
            {
                VertexColorEnabled = true,
                Projection = Matrix.CreateOrthographicOffCenter
               (
                   0, _graphicsDevice.Viewport.Width,
                   _graphicsDevice.Viewport.Height, 0,
                   0, 1
               )
            };
        }

        public bool HasDisplayedCharacters()
        {
            if (_characters is not null) _displayedCharacters = _characters.Count > 0 ? _characters.Where(e => e.ShowOnRadial).ToList() : new();
            return _displayedCharacters.Count() > 0;
        }

        private void Keyboard_KeyPressed(object sender, KeyboardEventArgs e)
        {
            if (e.Key == Microsoft.Xna.Framework.Input.Keys.Escape)
            {
                Hide();
            }
        }

        private void Parent_Resized(object sender, ResizedEventArgs e)
        {
            RecalculateLayout();
        }

        private void Character_Updated(object sender, EventArgs e)
        {
            RecalculateLayout();
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (Parent is not null)
            {
                _displayedCharacters = _characters.Count > 0 ? _characters.Where(e => e.ShowOnRadial).ToList() : new();
                _displayedCharacters.Sort((a, b) => a.Name.CompareTo(b.Name));

                int size = (int)(Math.Min(Parent.Width, Parent.Height) * _settings.Radial_Scale.Value);
                Size = new(Parent.Width, Parent.Height);

                _sections.Clear();

                int amount = _displayedCharacters.Count;
                double adiv = Math.PI * 2 / amount;

                int radius = size / 2;

                _iconSize = Math.Min(105, ((int)(radius * (Math.Sqrt(2) / amount))) * 3);

                if (amount <= 2)
                {
                    //var points = new List<Vector2>();
                    //var c = _center.ToVector2();

                    //for (int i = 0; i < amount; i++)
                    //{
                    //    var p = new Vector2(
                    //        x: _center.X - (int)(radius * Math.Sin(2 * Math.PI * i / amount)),
                    //        y: _center.Y - (int)(radius * Math.Cos(2 * Math.PI * i / amount))
                    //        );

                    //    points.Add(p);
                    //}

                    //for (int i = 0; i < points.Count; i++)
                    //{
                    //    Vector2 a = points[i];
                    //    var p = a.ToPoint();
                    //    var s = _center.Distance2D(p);

                    //    _sections.Add(new()
                    //    {
                    //        Character = _displayedCharacters[i],
                    //        IconRectangle = new(p.X - (_iconSize / 2), p.Y - (_iconSize / 2), _iconSize, _iconSize)
                    //    });
                    //}
                }
                else
                {
                    var points = new List<Vector2>();
                    var c = _center;

                    for (int i = 0; i < amount; i++)
                    {
                        var p = new Vector2(
                            x: _center.X - (int)(radius * Math.Sin(2 * Math.PI * i / amount)),
                            y: _center.Y - (int)(radius * Math.Cos(2 * Math.PI * i / amount))
                            );

                        points.Add(p);
                    }

                    for (int i = 0; i < points.Count; i++)
                    {
                        Vector2 a = points[i];
                        Vector2 b = i == points.Count - 1 ? points[0] : points[i + 1];
                        var t = new Triangle(a, b, c);
                        var v = new Vector2((a.X + b.X + c.X) / 3, (a.Y + b.Y + c.Y) / 3);
                        var p = v.ToPoint();

                        var dis = Math.Min(a.ToPoint().Distance2D(v.ToPoint()),
                            Math.Min(b.ToPoint().Distance2D(v.ToPoint()),
                            c.ToPoint().Distance2D(v.ToPoint())));

                        _sections.Add(new()
                        {
                            Character = _displayedCharacters[i],
                            Triangle = t,
                            Lines = t.DrawingPoints(),
                            IconPos = v,
                            IconRectangle = new(p.X - (_iconSize / 2), p.Y - (_iconSize / 2), _iconSize, _iconSize)
                        });
                    }
                }
            }

            if (_graphicsDevice is null)
            {
                return;
            }

            var dpiScale = _graphicsDevice.PresentationParameters.BackBufferWidth
               / (float)GameService.Graphics.SpriteScreen.Size.X;

            var mouse = GameService.Input.Mouse.Position.ToVector2() * dpiScale;
            var absBounds = this.AbsoluteBounds;
            _center = new Vector2(mouse.X - absBounds.X, mouse.Y - absBounds.Y);
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            base.OnMouseMoved(e);

            //_selected = null;
            //_tooltip.Character = null;
        }

        public override void DoUpdate(GameTime gameTime)
        {
            base.DoUpdate(gameTime);

            if (!Input.Keyboard.KeysDown.Contains(_settings.RadialKey.Value.PrimaryKey))
            {
                Hide();
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            DrawRadialMenu2(spriteBatch, bounds);
        }

        private void DrawRadialMenu2(SpriteBatch spriteBatch, Rectangle bounds)
        {
            _tooltip.Hide();

            float dpiScale = _graphicsDevice.PresentationParameters.BackBufferWidth
               / (float)GameService.Graphics.SpriteScreen.Size.X;

            var mouse = GameService.Input.Mouse.Position.ToVector2() * dpiScale;
          

            float radius = Height * _settings.Radial_Scale.Value / 2;

            int count = _displayedCharacters.Count;
            if (count == 0) return;

            // Angle per slice
            float angleStep = MathHelper.TwoPi / count;

            for (int i = 0; i < count; i++)
            {
                var character = _displayedCharacters[i];

                // Calculate slice angles
                float startAngle = i * angleStep;
                float endAngle = startAngle + angleStep;
                float midAngle = (startAngle + endAngle) / 2f;

                // Convert mouse position to polar relative to _center
                Vector2 dir = mouse - _center;
                float distance = dir.Length();
                float angle = (float)Math.Atan2(dir.Y, dir.X);
                if (angle < 0) angle += MathHelper.TwoPi;

                // Check if mouse is inside slice
                bool contains_mouse = distance <= radius && angle >= startAngle && angle <= endAngle;

                // Colors
                var lineColor = _settings.Radial_UseProfessionColor.Value
                    ? character.Profession.GetData(_data.Professions).Color * 0.7f
                    : _settings.Radial_IdleColor.Value;
                var borderColor =( contains_mouse ? _settings.Radial_HoveredBorderColor.Value : _settings.Radial_IdleBorderColor.Value) * 0.5F;
                var backgroundColor = (contains_mouse ? _settings.Radial_HoveredColor.Value : _settings.Radial_IdleColor.Value) * 0.8F;

                // Draw the slice
                float innerRadius = radius * 0.5f; // adjust for donut thickness

                DrawSlice(_center, radius, startAngle, endAngle, backgroundColor);
                DrawBorder(_center, radius, startAngle, endAngle, _settings.Radial_IdleColor.Value * 1.5F);

                if (contains_mouse)
                {
                    DrawSliceBorder(_center, innerRadius, radius, startAngle, endAngle, borderColor);
                }

                var texture = _settings.Radial_UseProfessionIcons.Value ? character.ProfessionIcon : character.Icon;

                // Icon
                float iconRadius = (radius + innerRadius) / 2f;
                float scale = _iconSize / (float)texture.Width;

                Vector2 relativeCenter = _center / dpiScale - (texture.Bounds.Size.ToVector2() * scale / 2);
                Vector2 iconCenter = relativeCenter + (new Vector2((float)Math.Cos(midAngle), (float)Math.Sin(midAngle)) * iconRadius);

                spriteBatch.Draw(texture, iconCenter, texture.Bounds, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 1);

                if (contains_mouse)
                {
                    if (_settings.Radial_ShowAdvancedTooltip.Value)
                    {
                        _selected = character;
                        _tooltip.Character = character;
                        _tooltip.Show();
                    }
                    else
                    {
                        BasicTooltipText = character.Name;
                    }
                }
                else
                {

                }
            }
        }

        private void DrawBorder(Vector2 center, float radius, float startAngle, float endAngle, Color color, int segments = 30)
        {
            if (_graphicsDevice == null || _effect == null) return;

            // Clamp segments
            segments = Math.Max(2, segments);

            float angleStep = (endAngle - startAngle) / segments;
            VertexPositionColor[] vertices = new VertexPositionColor[segments + 1];

            for (int i = 0; i <= segments; i++)
            {
                float angle = startAngle + i * angleStep;
                Vector2 pos = center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                vertices[i] = new VertexPositionColor(new Vector3(pos, 0), color);
            }

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _graphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, vertices, 0, segments);
            }
        }

        private void DrawSliceBorder(Vector2 center, float innerRadius, float outerRadius, float startAngle, float endAngle, Color color, int segments = 30)
        {
            if (_graphicsDevice == null || _effect == null) return;

            segments = Math.Max(2, segments);
            float angleStep = (endAngle - startAngle) / segments;

            // Outer arc vertices
            var outerVertices = new VertexPositionColor[segments + 1];
            for (int i = 0; i <= segments; i++)
            {
                float angle = startAngle + i * angleStep;
                Vector2 pos = center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * outerRadius;
                outerVertices[i] = new VertexPositionColor(new Vector3(pos, 0), color);
            }

            // Inner arc vertices
            VertexPositionColor[] innerVertices = new VertexPositionColor[segments + 1];
            for (int i = 0; i <= segments; i++)
            {
                float angle = startAngle + i * angleStep;
                Vector2 pos = center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * innerRadius;
                innerVertices[i] = new VertexPositionColor(new Vector3(pos, 0), color);
            }

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                // Draw outer arc
                //_graphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, outerVertices, 0, segments);

                // Draw inner arc
                //_graphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, innerVertices, 0, segments);

                // Draw radial lines at start and end
                _graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, new[]
                {
            new VertexPositionColor(new Vector3(center, 0), color),
            new VertexPositionColor(new Vector3(center + new Vector2((float)Math.Cos(startAngle), (float)Math.Sin(startAngle)) * outerRadius, 0), color),

            new VertexPositionColor(new Vector3(center, 0), color),
            new VertexPositionColor(new Vector3(center + new Vector2((float)Math.Cos(endAngle), (float)Math.Sin(endAngle)) * outerRadius, 0), color),
        }, 0, 2);
            }
        }

        private void DrawSlice(Vector2 center, float radius, float startAngle, float endAngle, Color color, int segments = 30)
        {
            float angleStep = (endAngle - startAngle) / segments;

            // Each triangle = 3 vertices, we have `segments` triangles
            var vertices = new VertexPositionColor[segments * 3];

            for (int i = 0; i < segments; i++)
            {
                float angle1 = startAngle + (i * angleStep);
                float angle2 = startAngle + ((i + 1) * angleStep);

                Vector2 p1 = center + (new Vector2((float)Math.Cos(angle1), (float)Math.Sin(angle1)) * radius);
                Vector2 p2 = center + (new Vector2((float)Math.Cos(angle2), (float)Math.Sin(angle2)) * radius);

                // Build triangle: center → p1 → p2
                vertices[(i * 3) + 0] = new VertexPositionColor(new Vector3(center, 0), color);
                vertices[(i * 3) + 1] = new VertexPositionColor(new Vector3(p1, 0), color);
                vertices[(i * 3) + 2] = new VertexPositionColor(new Vector3(p2, 0), color);
            }

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, segments);
            }
        }

        protected override async void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if (_selected is not null)
            {
                Hide();

                if(await ExtendedInputService.WaitForNoKeyPressed())
                {
                    _selected.Swap();
                }
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            RecalculateLayout();
            _selected = null;
        }

        protected override void OnHidden(EventArgs e)
        {
            base.OnHidden(e);

            _tooltip.Character = null;
            _tooltip.Hide();
        }

        protected override void DisposeControl()
        {
            if (Parent is not null) Parent.Resized -= Parent_Resized;

            foreach (Character_Model c in _characters)
            {
                c.Updated -= Character_Updated;
            }
            _tooltip?.Dispose();
            base.DisposeControl();
        }
    }
}
