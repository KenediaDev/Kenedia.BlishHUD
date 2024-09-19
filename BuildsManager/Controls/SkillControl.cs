using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Gw2Sharp.Models;
using Gw2Sharp;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.TextureAtlases;
using System.Diagnostics;
using System.Linq;
using static Blish_HUD.ContentService;
using Colors = Microsoft.Xna.Framework.Color;
using Kenedia.Modules.Core.Models;

namespace Kenedia.Modules.BuildsManager.Controls
{
    public class SkillControl : Control
    {
        public DetailedTexture Selector { get; } = new(157138, 157140);

        public AsyncTexture2D Texture => Skill?.Icon;

        public AsyncTexture2D HoveredFrameTexture { get; } = AsyncTexture2D.FromAssetId(157143);

        public AsyncTexture2D HoveredTexture { get; } = AsyncTexture2D.FromAssetId(157143);

        public AsyncTexture2D FallBackTexture { get; } = AsyncTexture2D.FromAssetId(157154);

        public AsyncTexture2D NoAquaticFlagTexture { get; } = AsyncTexture2D.FromAssetId(157145);

        public Rectangle TextureRegion { get; } = new(14, 14, 100, 100);

        public Rectangle NoAquaticFlagTextureRegion { get; } = new(16, 16, 96, 96);

        public Rectangle FallbackRegion { get; }

        public Rectangle FallbackBounds { get; private set; }

        public Rectangle SkillBounds { get; private set; }

        public Rectangle SelectorBounds { get; private set; }

        public Rectangle HoveredFrameTextureRegion { get; } = new(8, 8, 112, 112);

        public Rectangle AutoCastTextureRegion { get; } = new(6, 6, 52, 52);

        public SkillControl(SkillSlotType skillSlot, TemplatePresenter templatePresenter)
        {
            SkillSlot = skillSlot;
            TemplatePresenter = templatePresenter;

            Tooltip = new SkillTooltip(skillSlot, templatePresenter);
            Size = new(64);

            TemplatePresenter.TemplateChanged += TemplatePresenter_TemplateChanged;
            TemplatePresenter.SkillChanged += TemplatePresenter_SkillChanged;
        }

        private void TemplatePresenter_TemplateChanged(object sender, Core.Models.ValueChangedEventArgs<Template> e)
        {
            Skill = TemplatePresenter?.Template?[SkillSlot];
        }

        private void TemplatePresenter_SkillChanged(object sender, SkillChangedEventArgs e)
        {
            Skill = TemplatePresenter?.Template?[SkillSlot];
        }

        public SkillSlotType SkillSlot { get; }

        public TemplatePresenter TemplatePresenter { get; }

        public Vector2 Origin { get; private set; } = Vector2.Zero;

        public float Rotation { get; private set; } = 0F;

        public Color? BackgroundDrawColor { get; private set; }

        public Color Color { get; private set; }

        public Color? HoverDrawColor { get; private set; }

        public Color? DrawColor { get; private set; }

        public bool ShowSelector { get; set; } = false;

        public bool IsSelectorHovered => ShowSelector && SelectorBounds.Contains(RelativeMousePosition);

        public Skill? Skill { get; set; }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            bool terrestrial = SkillSlot.IsTerrestrial();

            if (BackgroundDrawColor is Colors color)
            {
                spriteBatch.DrawOnCtrl(
                this,
                Textures.Pixel,
                SkillBounds,
                Rectangle.Empty,
                color,
                Rotation,
                Origin);
            }

            if (ShowSelector)
            {
                Selector.Draw(this, spriteBatch, RelativeMousePosition);
            }

            bool hovered = MouseOver && SkillBounds.Contains(RelativeMousePosition);

            if (FallBackTexture is not null || Texture is not null)
            {
                Color = (hovered && HoverDrawColor is not null ? HoverDrawColor : DrawColor) ?? Colors.White;
                Color = Colors.White;

                if (HoveredTexture is not null && hovered)
                {
                    spriteBatch.DrawOnCtrl(
                        this,
                        HoveredTexture,
                        SkillBounds,
                        TextureRegion,
                        Color,
                        Rotation,
                        Origin);
                }

                if (Texture is not null)
                {
                    spriteBatch.DrawOnCtrl(
                        this,
                        Texture,
                        SkillBounds,
                        TextureRegion,
                        Color,
                        Rotation,
                        Origin);
                }
                else
                {
                    spriteBatch.DrawOnCtrl(
                        this,
                        FallBackTexture,
                        FallbackBounds == Rectangle.Empty ? SkillBounds : FallbackBounds,
                        SkillBounds,
                        Color,
                        Rotation,
                        Origin);
                }
            }

            Color borderColor = Colors.Black;

            // Top
            spriteBatch.DrawOnCtrl(this, Textures.Pixel, new Rectangle(SkillBounds.Left, SkillBounds.Top, SkillBounds.Width, 1), Rectangle.Empty, borderColor * 0.6f);

            // Bottom
            spriteBatch.DrawOnCtrl(this, Textures.Pixel, new Rectangle(SkillBounds.Left, SkillBounds.Bottom - 1, SkillBounds.Width, 1), Rectangle.Empty, borderColor * 0.6f);

            // Left
            spriteBatch.DrawOnCtrl(this, Textures.Pixel, new Rectangle(SkillBounds.Left, SkillBounds.Top, 1, SkillBounds.Height), Rectangle.Empty, borderColor * 0.6f);

            // Right
            spriteBatch.DrawOnCtrl(this, Textures.Pixel, new Rectangle(SkillBounds.Right - 1, SkillBounds.Top, 1, SkillBounds.Height), Rectangle.Empty, borderColor * 0.6f);

            if (!terrestrial && Skill?.Flags.HasFlag(Gw2Sharp.SkillFlag.NoUnderwater) == true)
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    NoAquaticFlagTexture,
                    SkillBounds,
                    NoAquaticFlagTextureRegion,
                    (Color)Color,
                    (float)Rotation,
                    (Vector2)Origin);
            }
            else if (hovered && HoveredFrameTexture is not null)
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    HoveredFrameTexture,
                    SkillBounds,
                    HoveredFrameTextureRegion,
                    (Color)Color,
                    (float)Rotation,
                    (Vector2)Origin);
            }

            //if (ShowSelector)
            //{
            //    Selector.Draw(ctrl, spriteBatch, mousePos);
            //}
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int selectorHeight = 15;
            SkillBounds = new(new(0, selectorHeight - 2), new(Width, Height - selectorHeight));
        }

        private void SetSkill(Skill skill)
        {

        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Tooltip?.Dispose();
        }
    }
}
