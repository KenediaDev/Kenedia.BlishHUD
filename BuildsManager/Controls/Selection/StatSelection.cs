using Gw2Sharp.WebApi;
using Kenedia.Modules.BuildsManager.Controls.GearPage;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Utility;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using AttributeType = Gw2Sharp.WebApi.V2.Models.AttributeType;

namespace Kenedia.Modules.BuildsManager.Controls.Selection
{
    public class SkillSelection : BaseSelection
    {
        private Template _template;
        private readonly List<SkillSelectable> _skills = new();

        public Template Template
        {
            get => _template; set
            {
                var temp = _template;
                if (Common.SetProperty(ref _template, value, ApplyTemplate))
                {
                    foreach (var skill in _skills)
                    {
                        skill.Template = value;
                    }

                    if (temp != null) temp.PropertyChanged -= TemplateChanged;
                    if (_template != null) _template.PropertyChanged += TemplateChanged;
                }
            }
        }

        public SkillSelection()
        {
            var ids = new List<int>();

            foreach (var profession in BuildsManager.Data.Professions)
            {
                foreach (var skill in profession.Value.Skills)
                {
                    if (!ids.Contains(skill.Value.Id))
                    {
                        _skills.Add(new()
                        {
                            Parent = SelectionContent,
                            Width = SelectionContent.Width - 35,
                            Skill = skill.Value,
                            GetRotationElement = () => Anchor,
                        });

                        ids.Add(skill.Value.Id);
                    }
                }
            }

            Search.PerformFiltering = FilterSkills;
        }

        private void FilterSkills(string txt)
        {
            txt ??= Search.Text;
            string searchTxt = txt.Trim().ToLower();
            bool anyName = searchTxt.IsNullOrEmpty();

            foreach (var skill in _skills)
            {
                skill.Visible = anyName || skill.Skill?.Name.ToLower().Trim().Contains(searchTxt) == true;
            }

            SelectionContent.SortChildren<SkillSelectable>((a, b) => a.Skill.Name.CompareTo(b.Skill.Name));
        }

        public RotationElement Anchor { get; set; }

        private void TemplateChanged(object sender, PropertyChangedEventArgs e)
        {
            ApplyTemplate();
        }

        private void ApplyTemplate()
        {

        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            _skills.Clear();
        }

        protected override void OnSelectionContent_Resized(object sender, Blish_HUD.Controls.ResizedEventArgs e)
        {
            base.OnSelectionContent_Resized(sender, e);

            Search?.SetSize(SelectionContent.Width - 5);

            foreach (var skill in _skills)
            {
                skill.Width = SelectionContent.Width - 35;
            }
        }
    }

    public class StatSelection : BaseSelection
    {
        private readonly List<AttributeToggle> _statIcons = new();
        private readonly List<StatSelectable> _stats = new();
        private readonly bool _created;

        private Template _template;
        private BaseTemplateEntry _templateSlot;

        public StatSelection()
        {
            AttributeToggle t;
            int i = 0;
            int size = 25;
            Point start = new(0, 0);

            var stats = new List<AttributeType>()
            {
                AttributeType.Power,
                AttributeType.Toughness,
                AttributeType.Vitality,
                AttributeType.Healing,
                AttributeType.Precision,
                AttributeType.CritDamage,
                AttributeType.ConditionDamage,
                AttributeType.ConditionDuration,
                AttributeType.BoonDuration,
            };

            foreach (AttributeType stat in stats)
            {
                if (stat is AttributeType.Unknown or AttributeType.None or AttributeType.AgonyResistance) continue;

                int j = 0;

                _statIcons.Add(t = new()
                {
                    Parent = this,
                    Location = new(start.X + (i * (size + 16)), start.Y + (j * size)),
                    Size = new(size, size),
                    Attribute = stat,
                    OnCheckChanged = (isChecked) => FilterStats(),
                    Checked = false,
                    BasicTooltipText = $"{stat.GetDisplayName()}",
                });

                i++;
            }

            StatSelectable selectable;
            foreach (var stat in BuildsManager.Data.Stats)
            {
                _stats.Add(selectable = new()
                {
                    Parent = SelectionContent,
                    Width = SelectionContent.Width - 35,
                    Stat = stat.Value,
                });
            }

            Search.PerformFiltering = FilterStats;
            Search.SetLocation(Search.Left, Search.Top + 30);

            SelectionContent.SetLocation(Search.Left, Search.Bottom + 5);

            FilterStats();

            _created = true;
        }

        private void SelectStat(Stat selectedStat)
        {
            bool applyall = false;

            switch (applyall)
            {
                case false:
                    (TemplateSlot as GearTemplateEntry).Stat = selectedStat;
                    break;

                case true:
                    List<GearTemplateEntry> slots = new();

                    slots.AddRange(Template?.GearTemplate?.Armors.Values.Cast<GearTemplateEntry>());
                    slots.AddRange(Template?.GearTemplate?.Weapons.Values.Cast<GearTemplateEntry>());
                    slots.AddRange(Template?.GearTemplate?.Juwellery.Values.Cast<GearTemplateEntry>());

                    foreach (var slot in slots)
                    {
                        if (slot.Item is not null)
                        {
                            slot.Stat = selectedStat;
                        }
                    }
                    break;
            }
        }

        public Template Template
        {
            get => _template; set
            {
                var temp = _template;
                if (Common.SetProperty(ref _template, value, ApplyTemplate))
                {
                    foreach (var stat in _stats)
                    {
                        stat.Template = value;
                    }

                    if (temp != null) temp.PropertyChanged -= TemplateChanged;
                    if (_template != null) _template.PropertyChanged += TemplateChanged;
                }
            }
        }

        public BaseTemplateEntry TemplateSlot
        {
            get => _templateSlot;
            set => Common.SetProperty(ref _templateSlot, value, () =>
            {
                FilterStats(null);

                double exoticTrinkets = 0;

                foreach (var stat in _stats)
                {
                    stat.ActiveTemplateSlot = value;
                    stat.AttributeAdjustment = _templateSlot?.Item != null ? ((_templateSlot?.Item as EquipmentItem).AttributeAdjustment + exoticTrinkets) : 0;
                }
            });
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();
            if (!_created) return;

        }

        protected override void OnSelectionContent_Resized(object sender, Blish_HUD.Controls.ResizedEventArgs e)
        {
            base.OnSelectionContent_Resized(sender, e);

            Search?.SetSize(SelectionContent.Width - 5);

            foreach (var stat in _stats)
            {
                stat.Width = SelectionContent.Width - 35;
            }
        }

        private void FilterStats(string? txt = null)
        {
            txt ??= Search.Text;
            string searchTxt = txt.Trim().ToLower();
            bool anyName = searchTxt.IsNullOrEmpty();

            var validStats = TemplateSlot?.Item != null ? (TemplateSlot?.Item as EquipmentItem).StatChoices : new List<int>();
            bool anyStat = validStats.Count == 0;

            bool anyAttribute = !_statIcons.Any(e => e.Checked);
            var attributes = _statIcons.Where(e => e.Checked).Select(e => e.Attribute);

            foreach (var stat in _stats)
            {
                var statAttributes = stat.Stat.Attributes.Select(e => e.Value.Id);

                stat.Visible =
                    (anyAttribute || attributes.All(e => statAttributes.Contains(e))) &&
                    validStats.Contains(stat.Stat.Id) &&
                    (anyName || stat.Stat?.Name.ToLower().Trim().Contains(searchTxt) == true);
            }

            SelectionContent.SortChildren<StatSelectable>((a, b) => a.Stat.Name.CompareTo(b.Stat.Name));
        }

        private void TemplateChanged(object sender, PropertyChangedEventArgs e)
        {
            ApplyTemplate();
        }

        private void ApplyTemplate()
        {
            FilterStats();
        }
    }
}
