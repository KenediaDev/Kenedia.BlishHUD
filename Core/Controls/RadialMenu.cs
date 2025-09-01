using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kenedia.Modules.Core.Controls
{
    internal class RadialMenu : Control
    {
        private GraphicsDevice _graphicsDevice;
        private BasicEffect _effect;

        public RadialMenu()
        {
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

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            DrawRadialMenu(spriteBatch, bounds.Center.ToVector2(), _graphicsDevice, _effect);
        }

        private void DrawRadialMenu(SpriteBatch spriteBatch, Vector2 center, GraphicsDevice graphicsDevice, BasicEffect effect)
        {
            float dpiScale = graphicsDevice.PresentationParameters.BackBufferWidth
               / (float)GameService.Graphics.SpriteScreen.Size.X;

            var mouse = GameService.Input.Mouse.Position.ToVector2() * dpiScale;
            float radius = Height / 2; //Set your slice radius here

            int count = 8; //Set your number of slices here
            if (count == 0) return;

            // Angle per slice
            float angleStep = MathHelper.TwoPi / count;

            for (int i = 0; i < count; i++)
            {
                // Calculate slice angles
                float startAngle = i * angleStep;
                float endAngle = startAngle + angleStep;
                float midAngle = (startAngle + endAngle) / 2f;

                // Convert mouse position to polar relative to center
                Vector2 dir = mouse - center;
                float distance = dir.Length();
                float angle = (float)Math.Atan2(dir.Y, dir.X);
                if (angle < 0) angle += MathHelper.TwoPi;

                // Check if mouse is inside slice
                bool contains_mouse = distance <= radius && angle >= startAngle && angle <= endAngle;

                // Colors
                var borderColor = Color.White;
                var outerBorderColor = Color.Gray;
                var backgroundColor = (contains_mouse ? Color.Green : Color.Black) * 0.8F;

                // Draw the slice
                float innerRadius = radius * 0.5f; // adjust for donut thickness

                DrawRadialSlice(graphicsDevice, effect, center, radius, startAngle, endAngle, backgroundColor);
                DrawOuterBorder(graphicsDevice, effect, center, radius, startAngle, endAngle, outerBorderColor);

                if (contains_mouse)
                {
                    DrawSliceBorders(graphicsDevice, effect, center, innerRadius, radius, startAngle, endAngle, borderColor);
                }

                var texture = AsyncTexture2D.FromAssetId(1128572); // Set your icon texture here

                // Icon
                float iconSize = 64; // Set your desired icon size here
                float iconRadius = (radius + innerRadius) / 2f;
                float scale = iconSize / (float)texture.Width;

                Vector2 relativeCenter = center / dpiScale - (texture.Bounds.Size.ToVector2() * scale / 2);
                Vector2 iconCenter = relativeCenter + (new Vector2((float)Math.Cos(midAngle), (float)Math.Sin(midAngle)) * iconRadius);

                spriteBatch.Draw(texture, iconCenter, texture.Bounds, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 1);

                if (contains_mouse)
                {
                    //Set tooltips or whatnot
                }
            }
        }

        private void DrawOuterBorder(GraphicsDevice graphicsDevice, BasicEffect effect, Vector2 center, float radius, float startAngle, float endAngle, Color color, int segments = 30)
        {
            if (graphicsDevice == null || effect == null) return;

            // Clamp segments
            segments = Math.Max(2, segments);

            float angleStep = (endAngle - startAngle) / segments;
            var vertices = new VertexPositionColor[segments + 1];

            for (int i = 0; i <= segments; i++)
            {
                float angle = startAngle + i * angleStep;
                Vector2 pos = center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                vertices[i] = new VertexPositionColor(new Vector3(pos, 0), color);
            }

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, vertices, 0, segments);
            }
        }

        private void DrawSliceBorders(GraphicsDevice graphicsDevice, BasicEffect effect, Vector2 center, float innerRadius, float outerRadius, float startAngle, float endAngle, Color color, int segments = 30)
        {
            if (graphicsDevice == null || effect == null) return;

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
            var innerVertices = new VertexPositionColor[segments + 1];
            for (int i = 0; i <= segments; i++)
            {
                float angle = startAngle + i * angleStep;
                Vector2 pos = center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * innerRadius;
                innerVertices[i] = new VertexPositionColor(new Vector3(pos, 0), color);
            }

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                // Draw radial lines at start and end
                graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, new[]
                {
                    new VertexPositionColor(new Vector3(center, 0), color),
                    new VertexPositionColor(new Vector3(center + new Vector2((float)Math.Cos(startAngle), (float)Math.Sin(startAngle)) * outerRadius, 0), color),

                    new VertexPositionColor(new Vector3(center, 0), color),
                    new VertexPositionColor(new Vector3(center + new Vector2((float)Math.Cos(endAngle), (float)Math.Sin(endAngle)) * outerRadius, 0), color),
                }, 0, 2);
            }
        }

        private void DrawRadialSlice(GraphicsDevice graphicsDevice, BasicEffect effect, Vector2 center, float radius, float startAngle, float endAngle, Color color, int segments = 30)
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

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, segments);
            }
        }
    }
}
