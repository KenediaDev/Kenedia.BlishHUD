using System;
using System.Collections.Generic;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.DataModels;
using Blish_HUD;
using Kenedia.Modules.Core.Extensions;
using MonoGame.Extended.Collections;

namespace Kenedia.Modules.BuildsManager.Controls.SkillConnectionEdit
{
    public class ReflectedSkillControl<T> : FlowPanel
        where T : BaseConnectionProperty, new()
    {
        private T _item;
        private bool _canSave = true;
        private readonly Dictionary<string, (Label, SkillControl)> _controls = new();
        private readonly SkillSelector _selector;

        private SkillConnection _skillConnection;

        public ReflectedSkillControl(string title, SkillSelector selector)
        {
            HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize;
            FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleTopToBottom;
            ContentPadding = new(10, 40, 10, 5);
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
                        if (prop != null)
                        {
                            if (Item.HasValues())
                            {
                                if (prop.GetValue(SkillConnection) == null)
                                {
                                    prop.SetValue(SkillConnection, Item);
                                }
                            }
                            else if (prop.GetValue(SkillConnection) != null)
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

        public SkillConnection SkillConnection
        {
            get => _skillConnection;
            set => Common.SetProperty(ref _skillConnection, value, OnConnectionChanged);
        }

        public T Item { get => _item; set => Common.SetProperty(ref _item, value, ApplyItem); }

        private void ApplyItem()
        {
            _canSave = false;

            foreach (var pinfo in typeof(T).GetProperties())
            {
                if (pinfo.Name is not "Values" && pinfo.GetIndexParameters().Length == 0 && _controls.TryGetValue(pinfo.Name, out (Label, SkillControl) ctrl))
                {
                    BaseSkill skill = null;

                    if (Item != null)
                    {
                        object value = pinfo.GetValue(Item);
                        skill = value != null ? BuildsManager.Data.BaseSkills.GetValueOrDefault((int)value) : null;
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
                _ = BuildsManager.Data.Save();
            }
        }

        private void OnConnectionChanged()
        {

        }
    }
}
