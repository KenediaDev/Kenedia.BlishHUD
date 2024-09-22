using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Blish_HUD.ContentService;
using Colors = Microsoft.Xna.Framework.Color;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Blish_HUD.Input;
using Kenedia.Modules.BuildsManager.Res;
using System.Text.RegularExpressions;
using Kenedia.Modules.Core.DataModels;
using Gw2Sharp.Models;
using System.Collections.Generic;
using System.Linq;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.BuildsManager.Controls.Selectables;

namespace Kenedia.Modules.BuildsManager.Controls
{
    public class SkillSlotControl : Control
    {
        private Skill _skill;

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

        public SkillSlotControl(SkillSlotType skillSlot, TemplatePresenter templatePresenter, Data data, SkillSelector skillSelector)
        {
            SkillSlot = skillSlot;
            TemplatePresenter = templatePresenter;
            Data = data;
            SkillSelector = skillSelector;

            Tooltip = SkillTooltip = new SkillTooltip();
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

        public SkillTooltip SkillTooltip { get; }

        public SkillSlotType SkillSlot { get; }

        public TemplatePresenter TemplatePresenter { get; }
        public Data Data { get; }
        public SkillSelector SkillSelector { get; }
        public Vector2 Origin { get; private set; } = Vector2.Zero;

        public float Rotation { get; private set; } = 0F;

        public Color? BackgroundDrawColor { get; private set; }

        public Color Color { get; private set; }

        public Color? HoverDrawColor { get; private set; }

        public Color? DrawColor { get; private set; }

        public bool ShowSelector { get; set; } = false;

        public bool IsSelectorHovered => ShowSelector && SelectorBounds.Contains(RelativeMousePosition);

        public Skill? Skill { get => _skill; set => Common.SetProperty(ref _skill, value, OnSkillChanged); }

        private void OnSkillChanged(object sender, Core.Models.ValueChangedEventArgs<Skill> e)
        {
            SkillTooltip.Skill = e.NewValue;
        }

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

            if (ShowSelector)
            {
                Selector.Draw(this, spriteBatch, RelativeMousePosition);
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int selectorHeight = 15;
            SkillBounds = new(new(0, selectorHeight - 2), new(Width, Height - selectorHeight));
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Tooltip?.Dispose();
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);
            SetSelector();
        }

        protected override void OnRightMouseButtonPressed(MouseEventArgs e)
        {
            base.OnRightMouseButtonPressed(e);
            SetSelector();
        }

        private void SetSelector()
        {
            SkillSelector.Anchor = this;
            SkillSelector.AnchorOffset = new(-2, 10);
            SkillSelector.ZIndex = ZIndex + 100;
            SkillSelector.SelectedItem = Skill;

            var slot = SkillSlot;

            slot &= ~(SkillSlotType.Aquatic | SkillSlotType.Inactive | SkillSlotType.Terrestrial | SkillSlotType.Active);
            SkillSelector.Label = strings.ResourceManager.GetString($"{Regex.Replace($"{slot.ToString().Trim()}", @"[_0-9]", "")}Skills");
            SkillSelector.Enviroment = SkillSlot.HasFlag(SkillSlotType.Aquatic) ? Enviroment.Aquatic : Enviroment.Terrestrial;
            SkillSelector.OnClickAction = (skill) =>
            {
                TemplatePresenter?.Template.SetSkill(SkillSlot, skill);
                SkillSelector.Hide();
            };

            GetSelectableSkills(SkillSlot);
            SkillSelector.Show();
        }

        private void GetSelectableSkills(SkillSlotType skillSlot)
        {
            if (TemplatePresenter?.Template?.Profession is null or 0)
                return;

            var slot = skillSlot.HasFlag(SkillSlotType.Utility_1) ? Gw2Sharp.WebApi.V2.Models.SkillSlot.Utility :
            skillSlot.HasFlag(SkillSlotType.Utility_2) ? Gw2Sharp.WebApi.V2.Models.SkillSlot.Utility :
            skillSlot.HasFlag(SkillSlotType.Utility_3) ? Gw2Sharp.WebApi.V2.Models.SkillSlot.Utility :
            skillSlot.HasFlag(SkillSlotType.Heal) ? Gw2Sharp.WebApi.V2.Models.SkillSlot.Heal :
            Gw2Sharp.WebApi.V2.Models.SkillSlot.Elite;

            if (TemplatePresenter?.Template?.Profession != ProfessionType.Revenant)
            {
                var skills = Data.Professions[TemplatePresenter.Template.Profession].Skills;
                var filteredSkills = skills.Where(e => e.Value.PaletteId > 0 && e.Value.Slot is not null && e.Value.Slot == slot && (e.Value.Specialization == 0 || TemplatePresenter.Template.HasSpecialization(e.Value.Specialization, out _))).ToList();

                var racialSkills = TemplatePresenter.Template.Race != Core.DataModels.Races.None ? Data.Races[TemplatePresenter.Template.Race]?.Skills.Where(e => e.Value.PaletteId > 0 && e.Value.Slot is not null && e.Value.Slot == slot).ToList() : new();
                if (racialSkills is not null) filteredSkills.AddRange(racialSkills);

                SkillSelector.SetItems(filteredSkills.OrderBy(e => e.Value.Categories).Select(e => e.Value));
            }
            else
            {
                List<Skill> filteredSkills = new();
                LegendSlotType legendSlot = skillSlot.GetEnviromentState() switch
                {
                    SkillSlotType.Active | SkillSlotType.Aquatic => LegendSlotType.AquaticActive,
                    SkillSlotType.Inactive | SkillSlotType.Aquatic => LegendSlotType.AquaticInactive,
                    SkillSlotType.Active | SkillSlotType.Terrestrial => LegendSlotType.TerrestrialActive,
                    SkillSlotType.Inactive | SkillSlotType.Terrestrial => LegendSlotType.TerrestrialInactive,
                    _ => LegendSlotType.TerrestrialActive,
                };

                var skills = TemplatePresenter.Template?.Legends[legendSlot];

                if (skills is not null)
                {
                    switch (slot)
                    {
                        case Gw2Sharp.WebApi.V2.Models.SkillSlot.Heal:
                            filteredSkills.Add(skills.Heal);
                            break;
                        case Gw2Sharp.WebApi.V2.Models.SkillSlot.Elite:
                            filteredSkills.Add(skills.Elite);
                            break;
                        case Gw2Sharp.WebApi.V2.Models.SkillSlot.Utility:
                            filteredSkills.AddRange(skills.Utilities.Select(e => e.Value));
                            break;
                    }
                }

                SkillSelector.SetItems(filteredSkills);
            }
        }

    }
}
