using Blish_HUD;
using Blish_HUD.Content;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kenedia.Modules.BuildsManager.Controls.SkillConnectionEdit
{
    public class ReflectedTraitControl : FlowPanel
    {
        private readonly ImageButton _addButton;
        private Traited _item;
        private bool _canSave = true;
        private readonly Dictionary<int, (Label, TraitControl, SkillControl)> _controls = new();
        private readonly TraitSelector _traitSelector;
        private readonly SkillSelector _skillSelector;

        private OldSkillConnection _skillConnection;

        public ReflectedTraitControl(string title, TraitSelector traitselector, SkillSelector skillSelector)
        {
            _traitSelector = traitselector;
            _skillSelector = skillSelector;

            HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize;
            FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleTopToBottom;
            ContentPadding = new(10, 5, 10, 5);
            Title = title;
            CanCollapse = true;

            _addButton = new ImageButton()
            {
                Parent = this,
                Texture = AsyncTexture2D.FromAssetId(155902),
                HoveredTexture = AsyncTexture2D.FromAssetId(155904),
                Size = new(32),
                ClickAction = (m) =>
                {
                    if (SkillConnection != null) CreateTraitControls(new(0, 0));
                }
            };
        }

        public OldSkillConnection SkillConnection
        {
            get => _skillConnection;
            set => Common.SetProperty(ref _skillConnection, value, OnConnectionChanged);
        }

        public Traited Item { get => _item; set => Common.SetProperty(ref _item, value, ApplyItem); }

        private void ApplyItem()
        {
            _canSave = false;

            _canSave = true;
        }

        private void Save()
        {
            if (_canSave)
            {
                if (Item?.HasValues() == true)
                {
                    SkillConnection.Traited ??= Item;
                }
                else if (SkillConnection.Traited != null)
                {
                    SkillConnection.Traited = null;
                }

                _ = BuildsManager.Data.Save();
            }
        }

        private void CreateTraitControls(KeyValuePair<int, int> traitPair)
        {
            int skillid = traitPair.Key;
            int traitid = traitPair.Value;

            Item ??= new();

            if (Item.ContainsKey(traitid)) return;

            var temp = CreateLabeledControl(this, $"Trait", 75, 250, 32);
            var trait = GetTrait(traitid);
            _ = BuildsManager.Data.BaseSkills.TryGetValue(skillid, out BaseSkill skill);

            if (trait != null) temp.Item2.Trait = trait;
            if (skill != null) temp.Item3.Skill = skill;

            temp.Item3.OnChangedAction = (prevId, newId) =>
            {
                prevId ??= 0;

                if (newId != null)
                {
                    if (prevId != null) _ = Item.Remove((int)prevId);

                    Item[(int)newId] = temp.Item2.Trait != null ? (int)temp.Item2.Trait?.Id : 0;

                    Save();
                }
            };

            temp.Item2.OnTraitChangedAction = (trait_id) =>
            {
                if (trait_id != null)
                {
                    int id = temp.Item3.Skill != null ? temp.Item3.Skill.Id : 0;
                    Item[id] = (int)trait_id;

                    Save();
                }
            };

            temp.Item2.OnIconAction = (id) =>
            {
                _traitSelector.Location = temp.Item2.AbsoluteBounds.Add(64, 32, 0, 0).Location;
                _traitSelector.Anchor = temp.Item2;
                _traitSelector.Show();
            };

            temp.Item3.OnIconAction = (id) =>
            {
                _skillSelector.Location = temp.Item3.AbsoluteBounds.Add(64, 32, 0, 0).Location;
                _skillSelector.Anchor = temp.Item3;
                _skillSelector.Show();
            };

            temp.Item3.OnDeleteAction = (id) =>
            {
                id ??= 0;

                if (id != null)
                {
                    _ = Item?.Remove((int)id);
                    temp.Item1.Dispose();
                }

                if (Item?.HasValues() == true)
                {
                    SkillConnection.Traited ??= Item;
                }
                else if (SkillConnection.Traited != null)
                {
                    SkillConnection.Traited = null;
                }

                Save();
            };

            _controls[skillid] = temp;
        }

        private void OnConnectionChanged()
        {
            for (int i = Children.Count - 1; i >= 0; i--)
            {
                Blish_HUD.Controls.Control item = Children[i];
                if (item != null && item != _addButton)
                {
                    item?.Dispose();
                }
            }

            if (SkillConnection != null)
            {
                _canSave = false;

                if (SkillConnection.Traited != null)
                {
                    foreach (var traitPair in SkillConnection.Traited)
                    {
                        CreateTraitControls(traitPair);
                    }
                }

                _canSave = true;
            }
        }

        private Trait GetTrait(int id)
        {
            foreach (var p in BuildsManager.Data.Professions)
            {
                foreach (var s in p.Value.Specializations)
                {
                    foreach (var t in s.Value.MinorTraits)
                    {
                        if (t.Value.Id == id) return t.Value;
                    }

                    foreach (var t in s.Value.MajorTraits)
                    {
                        if (t.Value.Id == id) return t.Value;
                    }
                }
            }

            return null;
        }

        public static (Label, TraitControl, SkillControl) CreateLabeledControl(Blish_HUD.Controls.Container parent, string text, int labelWidth = 175, int controlWidth = 100, int height = 25)
        {
            var p = new Panel()
            {
                Parent = parent,
                WidthSizingMode = Blish_HUD.Controls.SizingMode.AutoSize,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize,
            };

            var label = new Label()
            {
                Parent = p,
                Width = labelWidth,
                Height = height,
                Text = text,
            };

            var trait = new TraitControl()
            {
                Location = new(label.Right + 5, 0),
                Width = controlWidth,
                Height = height,
                Parent = p,
            };

            var skill = new SkillControl()
            {
                Location = new(trait.Right + 5, 0),
                Width = controlWidth,
                Height = height,
                Parent = p,
            };

            void Disposed(object s, EventArgs e)
            {
                trait.Disposed -= Disposed;
                skill.Disposed -= Disposed;
                label.Disposed -= Disposed;

                trait.Dispose();
                skill.Dispose();
                label.Dispose();
                p.Dispose();
            }

            trait.Disposed += Disposed;
            skill.Disposed += Disposed;
            label.Disposed += Disposed;

            return (label, trait, skill);
        }
    }
}