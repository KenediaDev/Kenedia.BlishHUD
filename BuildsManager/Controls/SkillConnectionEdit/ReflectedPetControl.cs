using System;
using System.Collections.Generic;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.DataModels;
using Blish_HUD;
using Kenedia.Modules.Core.Extensions;
using MonoGame.Extended.Collections;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Blish_HUD.Content;
using Gw2Sharp.WebApi.V2.Models;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Kenedia.Modules.BuildsManager.Controls.SkillConnectionEdit
{
    public class ReflectedPetControl : FlowPanel
    {
        private readonly ImageButton _addButton;
        private Pets _item;
        private bool _canSave = true;
        private readonly Dictionary<int, (Label, PetControl)> _controls = new();
        private readonly PetSelector _selector;

        private OldSkillConnection _skillConnection;

        public ReflectedPetControl(string title, PetSelector selector)
        {
            HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize;
            FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleTopToBottom;
            ContentPadding = new(10, 5, 10, 5);
            Title = title;
            CanCollapse = true;

            _selector = selector;

            _addButton = new ImageButton()
            {
                Parent = this,
                Texture = AsyncTexture2D.FromAssetId(155902),
                HoveredTexture = AsyncTexture2D.FromAssetId(155904),
                Size = new(32),
                ClickAction = (m) =>
                {
                    if (SkillConnection != null)
                    {
                        CreatePetControls(SkillConnection.Pets == null ? 0 : SkillConnection.Pets.Count, null);
                        var ctrls = _controls.LastOrDefault();
                        _selector.Location = _addButton.AbsoluteBounds.Add(64, 32, 0, 0).Location;
                        _selector.Anchor = ctrls.Value.Item2;
                        _selector.Show();
                    }
                }
            };
        }

        public OldSkillConnection SkillConnection
        {
            get => _skillConnection;
            set => Common.SetProperty(ref _skillConnection, value, OnConnectionChanged);
        }

        public Pets Item { get => _item; set => Common.SetProperty(ref _item, value, ApplyItem); }

        private void ApplyItem()
        {
            _canSave = false;

            _canSave = true;
        }

        private void Save()
        {
            if (_canSave)
            {
                _ = BuildsManager.Data.Save();
            }
        }

        private void CreatePetControls(int i, int? petid)
        {
            var temp = UI.CreateLabeledControl<PetControl>(this, $"Pet {i}", 100, 400, 32);
            if (petid != null && BuildsManager.Data.Pets.TryGetValue((int)petid, out DataModels.Professions.Pet pet))
            {
                temp.Item2.Pet = pet;
            }

            temp.Item2.OnChangedAction = (id) =>
            {
                Item ??= new();

                if (!Item.Contains(id))
                {
                    _ = Item.Remove(petid);
                    if (id != null) Item.Add(id);
                }

                if (Item.HasValues())
                {
                    SkillConnection.Pets ??= Item;
                }
                else if (SkillConnection.Pets != null)
                {
                    SkillConnection.Pets = null;
                }

                Save();
            };

            temp.Item2.OnIconAction = (id) =>
            {
                _selector.Location = temp.Item2.AbsoluteBounds.Add(64, 32, 0, 0).Location;
                _selector.Anchor = temp.Item2;
                _selector.Show();
            };

            temp.Item2.OnDeleteAction = (id) =>
            {
                _ = Item.Remove(id);
                temp.Item1.Dispose();
            };

            _controls[i] = temp;
        }

        private void OnConnectionChanged()
        {
            foreach (var item in _controls)
            {
                item.Value.Item1.Dispose();
            }

            if (SkillConnection != null)
            {
                _canSave = false;

                if (SkillConnection.Pets != null)
                {
                    for (int i = 0; i < SkillConnection.Pets.Count; i++)
                    {
                        int? pet = SkillConnection.Pets[i];
                        CreatePetControls(i, pet);
                    }
                }

                _canSave = true;
            }
        }
    }
}
