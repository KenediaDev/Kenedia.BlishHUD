using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using static Blish_HUD.ContentService;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage
{
    public class SkillIcon : DetailedTexture
    {
        private readonly AsyncTexture2D _noAquaticFlagTexture = AsyncTexture2D.FromAssetId(157145);
        private Rectangle _noAquaticFlagTextureRegion;
        private Skill _skill;

        public SkillIcon()
        {
            FallBackTexture = AsyncTexture2D.FromAssetId(157154);
            HoveredFrameTexture = AsyncTexture2D.FromAssetId(157143);

            TextureRegion = new(14, 14, 100, 100);
            HoveredFrameTextureRegion = new(8, 8, 112, 112);

            AutoCastTextureRegion = new Rectangle(6, 6, 52, 52);
            _noAquaticFlagTextureRegion = new(16, 16, 96, 96);
        }

        public Skill Skill { get => _skill; set => Common.SetProperty(ref _skill, value, ApplySkill); }

        public AsyncTexture2D HoveredFrameTexture { get; private set; }

        public AsyncTexture2D AutoCastTexture { get; set; }

        public Rectangle HoveredFrameTextureRegion { get; }

        public Rectangle AutoCastTextureRegion { get; }

        public SkillSlotType Slot { get; set; }

        public bool ShowSelector { get; set; } = false;

        public DetailedTexture Selector { get; } = new(157138, 157140);

        private void ApplySkill()
        {
            Texture = Skill?.Icon;
        }

        public void Draw(Control ctrl, SpriteBatch spriteBatch, bool terrestrial = true, Point? mousePos = null, Color? color = null, Color? bgColor = null, bool? forceHover = null, float? rotation = null, Vector2? origin = null)
        {
            base.Draw(ctrl, spriteBatch, mousePos, color, bgColor, forceHover, rotation, origin);

            color ??= Color.White;
            origin ??= Vector2.Zero;
            rotation ??= 0F;
            Color borderColor = Color.Black;

            // Top
            spriteBatch.DrawOnCtrl(ctrl, Textures.Pixel, new Rectangle(Bounds.Left, Bounds.Top, Bounds.Width, 1), Rectangle.Empty, borderColor * 0.6f);

            // Bottom
            spriteBatch.DrawOnCtrl(ctrl, Textures.Pixel, new Rectangle(Bounds.Left, Bounds.Bottom - 1, Bounds.Width, 1), Rectangle.Empty, borderColor * 0.6f);

            // Left
            spriteBatch.DrawOnCtrl(ctrl, Textures.Pixel, new Rectangle(Bounds.Left, Bounds.Top, 1, Bounds.Height), Rectangle.Empty, borderColor * 0.6f);

            // Right
            spriteBatch.DrawOnCtrl(ctrl, Textures.Pixel, new Rectangle(Bounds.Right - 1, Bounds.Top, 1, Bounds.Height), Rectangle.Empty, borderColor * 0.6f);

            color ??= Color.White;
            origin ??= Vector2.Zero;
            rotation ??= 0F;

            if (!terrestrial && Skill?.Flags.HasFlag(Gw2Sharp.SkillFlag.NoUnderwater) == true)
            {
                spriteBatch.DrawOnCtrl(
                    ctrl,
                    _noAquaticFlagTexture,
                    Bounds,
                    _noAquaticFlagTextureRegion,
                    (Color)color,
                    (float)rotation,
                    (Vector2)origin);
            }
            else if (Hovered)
            {
                spriteBatch.DrawOnCtrl(
                    ctrl,
                    HoveredFrameTexture,
                    Bounds,
                    HoveredFrameTextureRegion,
                    (Color)color,
                    (float)rotation,
                    (Vector2)origin);
            }

            if (ShowSelector)
            {
                Selector.Draw(ctrl, spriteBatch, mousePos);
            }
        }

        public override void Draw(Control ctrl, SpriteBatch spriteBatch, Point? mousePos = null, Color? color = null, Color? bgColor = null, bool? forceHover = null, float? rotation = null, Vector2? origin = null)
        {
            base.Draw(ctrl, spriteBatch, mousePos, color, bgColor, forceHover, rotation, origin);

            color ??= Color.White;
            origin ??= Vector2.Zero;
            rotation ??= 0F;
            Color borderColor = Color.Black;

            // Top
            spriteBatch.DrawOnCtrl(ctrl, Textures.Pixel, new Rectangle(Bounds.Left, Bounds.Top, Bounds.Width, 1), Rectangle.Empty, borderColor * 0.6f);

            // Bottom
            spriteBatch.DrawOnCtrl(ctrl, Textures.Pixel, new Rectangle(Bounds.Left, Bounds.Bottom - 1, Bounds.Width, 1), Rectangle.Empty, borderColor * 0.6f);

            // Left
            spriteBatch.DrawOnCtrl(ctrl, Textures.Pixel, new Rectangle(Bounds.Left, Bounds.Top, 1, Bounds.Height), Rectangle.Empty, borderColor * 0.6f);

            // Right
            spriteBatch.DrawOnCtrl(ctrl, Textures.Pixel, new Rectangle(Bounds.Right - 1, Bounds.Top, 1, Bounds.Height), Rectangle.Empty, borderColor * 0.6f);

            if (AutoCastTexture is not null)
            {
                spriteBatch.DrawOnCtrl(
                    ctrl,
                    AutoCastTexture,
                    Bounds.Add(-4, -4, 8, 8),
                    AutoCastTextureRegion,
                    (Color)color,
                    (float)rotation,
                    (Vector2)origin);
            }

            if (Hovered)
            {
                spriteBatch.DrawOnCtrl(
                    ctrl,
                    HoveredFrameTexture,
                    Bounds,
                    HoveredFrameTextureRegion,
                    (Color)color,
                    (float)rotation,
                    (Vector2)origin);
            }

            if (ShowSelector)
            {
                Selector.Draw(ctrl, spriteBatch, mousePos);
            }
        }
    }
}
