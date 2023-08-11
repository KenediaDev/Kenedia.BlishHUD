using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AttributeType = Gw2Sharp.WebApi.V2.Models.AttributeType;

namespace Kenedia.Modules.BuildsManager.Controls.Selection
{
    public class StatSelection : BaseSelection
    {
        private readonly List<AttributeToggle> _statIcons = new();
        private readonly List<StatSelectable> _stats = new();
        private readonly bool _created;
        private IReadOnlyList<int> _statChoices;
        private double _attributeAdjustments;

        public StatSelection(TemplatePresenter templatePresenter)
        {
            TemplatePresenter = templatePresenter;
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
                    OnClickAction = () =>
                    {
                        if (TemplatePresenter?.Template != null)
                        {
                            OnClickAction(stat.Value);
                        }
                    },
                });
            }

            Search.PerformFiltering = FilterStats;
            Search.SetLocation(Search.Left, Search.Top + 30);

            SelectionContent.SetLocation(Search.Left, Search.Bottom + 5);

            FilterStats();

            _created = true;
        }

        public TemplatePresenter TemplatePresenter { get; }

        public IReadOnlyList<int> StatChoices { get => _statChoices; set => Common.SetProperty(ref _statChoices , value, OnStatChoicesChanged); }

        public double AttributeAdjustments {  get => _attributeAdjustments; set => Common.SetProperty(ref _attributeAdjustments, value, OnAttributeAdjustmentsChanged); }

        private void OnAttributeAdjustmentsChanged(object sender, ValueChangedEventArgs<double> e)
        {
            foreach (var stat in _stats)
            {
                stat.AttributeAdjustment = AttributeAdjustments;
            }
        }

        private void OnStatChoicesChanged(object sender, ValueChangedEventArgs<IReadOnlyList<int>> e)
        {
            FilterStats(null);
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

            var validStats = StatChoices ?? new List<int>();
            bool anyStat = validStats.Count == 0;

            bool anyAttribute = !_statIcons.Any(e => e.Checked);
            var attributes = _statIcons.Where(e => e.Checked).Select(e => e.Attribute);

            foreach (var stat in _stats)
            {
                var statAttributes = stat.Stat.Attributes.Select(e => e.Value.Id);

                stat.Visible =
                    (anyAttribute || attributes.All(e => statAttributes.Contains(e))) &&
                    (validStats.Contains(stat.Stat.Id)) &&
                    (anyName || stat.Stat?.Name.ToLower().Trim().Contains(searchTxt) == true);
            }

            SelectionContent.SortChildren<StatSelectable>((a, b) => a.Stat.Name.CompareTo(b.Stat.Name));
        }
    }
}
