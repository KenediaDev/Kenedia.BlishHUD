using Blish_HUD.Controls;
using Gw2Sharp.Models;
using Kenedia.Modules.BuildsManager.Controls_Old.BuildPage;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.Res;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using SkillSlot = Gw2Sharp.WebApi.V2.Models.SkillSlot;
using System.Text.RegularExpressions;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Blish_HUD.Input;
using System.Diagnostics;

namespace Kenedia.Modules.BuildsManager.Controls
{
    public class SkillsBar : Container
    {
        private readonly int _skillSize = 64;
        private readonly DetailedTexture _selectingFrame = new(157147);
        private readonly DetailedTexture _aquaticTexture = new(1988170);
        private readonly DetailedTexture _terrestrialTexture = new(1988171);

        private readonly SkillSelector _skillSelector;
        private SkillSlotControl _selectorAnchor;

        public Dictionary<SkillSlotType, SkillSlotControl> Skills { get; } = [];

        public TemplatePresenter TemplatePresenter { get; }

        public Data Data { get; }

        public SkillsBar(TemplatePresenter templatePresenter, Data data)
        {
            TemplatePresenter = templatePresenter;
            Data = data;
            Height = 80;
            Width = 500;

            _skillSelector = new()
            {
                Parent = Graphics.SpriteScreen,
                Visible = false,
            };

            var enviroments = new[] { SkillSlotType.Terrestrial, SkillSlotType.Aquatic };
            var states = new[] { SkillSlotType.Active, SkillSlotType.Inactive };
            var slots = new[] { SkillSlotType.Heal, SkillSlotType.Utility_1, SkillSlotType.Utility_2, SkillSlotType.Utility_3, SkillSlotType.Elite };

            foreach (var state in states)
            {
                foreach (var enviroment in enviroments)
                {
                    foreach (var slot in slots)
                    {
                        var skillSlot = slot | state | enviroment;
                        Skills[skillSlot] = new SkillSlotControl(skillSlot, templatePresenter, data, _skillSelector) { Parent = this, ShowSelector = true, };
                    }
                }
            }

            TemplatePresenter.ProfessionChanged += TemplatePresenter_ProfessionChanged;
            TemplatePresenter.TemplateChanged += TemplatePresenter_TemplateChanged;
            TemplatePresenter.LegendSlotChanged += TemplatePresenter_LegendSlotChanged;
        }

        private void TemplatePresenter_LegendSlotChanged(object sender, ValueChangedEventArgs<LegendSlotType> e)
        {
            SetSkillsVisibility();
        }

        private void TemplatePresenter_TemplateChanged(object sender, ValueChangedEventArgs<Template> e)
        {
            SetSkillsVisibility();
        }

        private void TemplatePresenter_ProfessionChanged(object sender, ValueChangedEventArgs<ProfessionType> e)
        {
            SetSkillsVisibility();
        }

        private void SetSkillsVisibility()
        {
            var state = TemplatePresenter.Template?.Profession is ProfessionType.Revenant && TemplatePresenter.LegendSlot is LegendSlotType.TerrestrialInactive or LegendSlotType.AquaticInactive ? SkillSlotType.Inactive : SkillSlotType.Active;

            foreach (var skill in Skills)
            {
                skill.Value.Visible = skill.Key.HasFlag(state);
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _terrestrialTexture.Bounds = new Rectangle(5, 2, 42, 42);
            _aquaticTexture.Bounds = new Rectangle(_terrestrialTexture.Bounds.Right + (_skillSize * 5) + 20, 2, 42, 42);

            var size = new Point(_skillSize, _skillSize + 15);

            foreach (var spair in Skills)
            {
                int left = (spair.Key.IsTerrestrial() ? _terrestrialTexture.Bounds.Right : _aquaticTexture.Bounds.Right) + 5;
                int xOffset = spair.Key.GetSlotPosition() * size.X;

                Skills[spair.Key].SetBounds(new(left + xOffset, 0, size.X, size.Y));
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            _terrestrialTexture.Draw(this, spriteBatch, RelativeMousePosition, Color.White);
            _aquaticTexture.Draw(this, spriteBatch, RelativeMousePosition, Color.White);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Skills.Values?.DisposeAll();
        }
    }
}
