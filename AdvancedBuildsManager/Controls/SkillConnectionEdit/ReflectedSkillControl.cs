using System;
using System.Collections.Generic;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.DataModels;
using Blish_HUD;
using Kenedia.Modules.Core.Extensions;
using MonoGame.Extended.Collections;

namespace Kenedia.Modules.AdvancedBuildsManager.Controls.SkillConnectionEdit
{
    public class ReflectedSkillControl<T> : FlowPanel
        where T : BaseConnectionProperty, new()
    {
        private bool _canSave = true;
        private readonly Dictionary<string, (Label, SkillControl)> _controls = [];
        private readonly SkillSelector _selector;

        public ReflectedSkillControl(string title, SkillSelector selector)
        {
            HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize;
            FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleTopToBottom;
            ContentPadding = new(10, 5, 10, 5);
            Title = title;
            CanCollapse = true;

            _selector = selector;

            foreach (var pinfo in typeof(T).GetProperties())
            {
                if (pinfo.Name is not "Values" && pinfo.GetIndexParameters().Length == 0)
                {
                    var temp = UI.CreateLabeledControl<SkillControl>(this, pinfo.Name, 100, 400, 32);
                    temp.Item2.OnChangedAction = (prevId, newId) =>
                    {
                        Item ??= new();
                        pinfo.SetValue(Item, newId);

                        var prop = SkillConnection.GetType().GetProperty(typeof(T).Name);
                        if (prop is not null)
                        {
                            if (Item.HasValues())
                            {
                                if (prop.GetValue(SkillConnection) == null)
                                {
                                    prop.SetValue(SkillConnection, Item);
                                }
                            }
                            else if (prop.GetValue(SkillConnection) is not null)
                            {
                                prop.SetValue(SkillConnection, null);
                            }
                        }

                        Save();
                    };

                    temp.Item2.OnIconAction = (id) =>
                    {
                        _selector.Location = temp.Item2.AbsoluteBounds.Add(64, 32, 0, 0).Location;
                        _selector.Anchor = temp.Item2;
                        _selector.Show();
                    };

                    _controls[pinfo.Name] = temp;
                }
            }
        }

        public OldSkillConnection SkillConnection
        {
            get;
            set => Common.SetProperty(field, value, v => field = v, OnConnectionChanged);
        }

        public T Item { get; set => Common.SetProperty(field, value, v => field = v, ApplyItem); }

        private void ApplyItem()
        {
            _canSave = false;

            foreach (var pinfo in typeof(T).GetProperties())
            {
                if (pinfo.Name is not "Values" && pinfo.GetIndexParameters().Length == 0 && _controls.TryGetValue(pinfo.Name, out (Label, SkillControl) ctrl))
                {
                    BaseSkill skill = null;

                    if (Item is not null)
                    {
                        object value = pinfo.GetValue(Item);
                        skill = value is not null ? AdvancedBuildsManager.Data.BaseSkills.GetValueOrDefault((int)value) : null;
                    }

                    ctrl.Item2.Skill = skill;
                }
            }

            _canSave = true;
        }

        private void Save()
        {
            if (_canSave)
            {
                _ = AdvancedBuildsManager.Data.Save();
            }
        }

        private void OnConnectionChanged()
        {

        }
    }
}
