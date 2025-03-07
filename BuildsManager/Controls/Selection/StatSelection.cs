﻿using Kenedia.Modules.BuildsManager.Extensions;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using AttributeType = Gw2Sharp.WebApi.V2.Models.AttributeType;

namespace Kenedia.Modules.BuildsManager.Controls.Selection
{
    public class StatSelection : BaseSelection
    {
        private readonly List<AttributeToggle> _statIcons = [];
        private readonly List<StatSelectable> _stats = [];
        private readonly bool _created;
        private IReadOnlyList<int> _statChoices;
        private double _attributeAdjustments;

        public StatSelection(TemplatePresenter templatePresenter, Data data)
        {
            TemplatePresenter = templatePresenter;
            Data = data;

            Search.PerformFiltering = FilterStats;
            Search.SetLocation(Search.Left, Search.Top + 30);

            SelectionContent.SetLocation(Search.Left, Search.Bottom + 5);

            FilterStats();

            _created = true;
            Data.Loaded += Data_Loaded;

            CreateStatSelectables();
        }

        private void Data_Loaded(object sender, EventArgs e)
        {
            CreateStatSelectables();
        }

        private void CreateStatSelectables()
        {
            if (_stats.Count > 0) return;

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

            AttributeToggle t;
            int i = 0;

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
            foreach (var stat in Data.Stats.Items)
            {
                _stats.Add(selectable = new()
                {
                    Parent = SelectionContent,
                    Width = SelectionContent.Width - 35,
                    Stat = stat.Value,
                    OnClickAction = () =>
                    {
                        if (TemplatePresenter?.Template is not null)
                        {
                            OnClickAction(stat.Value);
                        }
                    },
                });
            }
        }

        public TemplatePresenter TemplatePresenter { get; }

        public Data Data { get; }

        public IReadOnlyList<int> StatChoices { get => _statChoices; set => Common.SetProperty(ref _statChoices, value, OnStatChoicesChanged); }

        public double AttributeAdjustments { get => _attributeAdjustments; set => Common.SetProperty(ref _attributeAdjustments, value, OnAttributeAdjustmentsChanged); }

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
            foreach (var stat in _stats)
            {
                stat.Visible = false;
            }

            bool first = true;
            foreach (string s in txt.Split(' '))
            {
                string searchTxt = s.Trim().ToLower();
                bool anyName = string.IsNullOrEmpty(searchTxt);

                var validStats = StatChoices ?? new List<int>();
                bool anyStat = validStats.Count == 0;

                bool anyAttribute = !_statIcons.Any(e => e.Checked);
                var attributes = _statIcons.Where(e => e.Checked).Select(e => e.Attribute);

                foreach (var stat in _stats)
                {
                    var statAttributes = stat.Stat.Attributes.Select(e => e.Value.Id);

                    stat.Visible = (first || stat.Visible) &&
                        (anyAttribute || attributes.All(e => statAttributes.Contains(e))) &&
                        validStats.Contains(stat.Stat.Id) &&
                        (anyName || stat.Stat?.Name.ToLower().Trim().Contains(searchTxt) == true || stat.Stat?.DisplayAttributes.ToLower().Contains(searchTxt) == true);
                }

                first = false;
            }

            SelectionContent.SortChildren<StatSelectable>((a, b) => a.Stat.Name?.CompareTo(b.Stat?.Name) ?? 0);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            _stats?.DisposeAll();
            _stats?.Clear();
            _statIcons?.DisposeAll();
            _statIcons?.Clear();
        }
    }
}
