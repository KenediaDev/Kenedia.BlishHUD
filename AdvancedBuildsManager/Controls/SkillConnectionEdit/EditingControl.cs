using Blish_HUD.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using static Blish_HUD.ContentService;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.DataModels;
using SkillWeaponType = Gw2Sharp.WebApi.V2.Models.SkillWeaponType;
using Blish_HUD;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.AdvancedBuildsManager.Views;
using MonoGame.Extended.Collections;
using Kenedia.Modules.AdvancedBuildsManager.DataModels.Professions;

namespace Kenedia.Modules.AdvancedBuildsManager.Controls.SkillConnectionEdit
{
    public class EditingControl : Panel
    {
        private readonly SkillSelector _skillSelector;
        private readonly PetSelector _petSelector;
        private readonly TraitSelector _traitSelector;
        private readonly FlowPanel _contentPanel;
        private readonly FlowPanel _infosPanel;
        private readonly FlowPanel _singleSkillsPanel;

        private double _created = 0;
        private (Label, Checkbox) _terrestrialFlag;
        private (Label, Checkbox) _aquaticFlag;
        private (Label, Dropdown) _weapon;
        private (Label, Dropdown) _specialization;
        private (Label, SkillControl) _default;
        private (Label, SkillControl) _pvp;
        private (Label, SkillControl) _enviromentalCounterskill;
        private (Label, SkillControl) _unleashed;
        private (Label, SkillControl) _toolbelt;
        private ReflectedSkillControl<Chain> _chain;
        private ReflectedSkillControl<Burst> _burst;
        private ReflectedSkillControl<Stealth> _stealth;
        private ReflectedSkillControl<Transform> _transform;
        private ReflectedSkillControl<Bundle> _bundle;
        private ReflectedSkillControl<FlipSkills> _flipSkills;
        private ReflectedSkillControl<DualSkill> _dualSkill;
        private ReflectedPetControl _pets;
        private ReflectedTraitControl _traited;
        private bool canSave;
        private (Label, Checkbox) _fireFlag;
        private (Label, Checkbox) _waterFlag;
        private (Label, Checkbox) _airFlag;
        private (Label, Checkbox) _earthFlag;

        public EditingControl(SkillConnectionEditor editor)
        {
            HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill;
            Title = "Editing Skill Connection";
            TitleIcon = AsyncTexture2D.FromAssetId(156706);

            _contentPanel = new()
            {
                Location = new(0),
                Parent = this,
                WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleTopToBottom,
                CanScroll = true,
            };

            _skillSelector = new(editor)
            {
                Parent = Graphics.SpriteScreen,
                BackgroundColor = Color.Black * 0.8F,
                BorderWidth = new(2),
                BorderColor = Color.Black,
                ZIndex = int.MaxValue / 2,
                Visible = false,
                ContentPadding = new(10, 5),
                Width = 300,
                Height = 600,
                OnClickAction = SkillSelected,
            };

            _traitSelector = new(editor)
            {
                Parent = Graphics.SpriteScreen,
                BackgroundColor = Color.Black * 0.8F,
                BorderWidth = new(2),
                BorderColor = Color.Black,
                ZIndex = int.MaxValue / 2,
                Visible = false,
                ContentPadding = new(10, 5),
                Width = 300,
                Height = 600,
                OnClickAction = TraitSelected,
                Items = GetTraits(),
            };

            _petSelector = new(editor)
            {
                Parent = Graphics.SpriteScreen,
                BackgroundColor = Color.Black * 0.8F,
                BorderWidth = new(2),
                BorderColor = Color.Black,
                ZIndex = int.MaxValue / 2,
                Visible = false,
                ContentPadding = new(10, 5),
                Width = 300,
                Height = 600,
                OnClickAction = PetSelected,
                Items = AdvancedBuildsManager.Data.Pets,
            };

            _infosPanel = new FlowPanel()
            {
                Parent = _contentPanel,
                Height = 150,
                ContentPadding = new(10, 5),
                ControlPadding = new(15, 5),
                FlowDirection = Blish_HUD.Controls.ControlFlowDirection.TopToBottom,
            };

            _singleSkillsPanel = new()
            {
                Parent = _contentPanel,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize,
                FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleTopToBottom,
                ContentPadding = new(10, 5, 10, 5),
                Title = "Single Skills",
                CanCollapse = true,
            };

            CreateUI();
            _created = Common.Now;
        }

        private void TraitSelected(TraitEntryControl obj, TraitControl anchor)
        {
            anchor.Trait = obj.Trait;
        }

        private void PetSelected(PetEntryControl obj, PetControl anchor)
        {
            anchor.Pet = obj.Pet;
        }

        private void SkillSelected(SkillEntryControl obj, SkillControl anchor)
        {
            anchor.Skill = obj.Skill;
        }

        public BaseSkill Skill { get; set; }

        public OldSkillConnection SkillConnection { get; set => Common.SetProperty(ref field, value, ApplySkill); }

        public Action OnChangedAction { get; set; }

        public Dictionary<int, OldSkillConnection> Connections { get; set => Common.SetProperty(ref field, value, ApplyConnections); }

        private void ApplyConnections()
        {
            _skillSelector.Items = Connections;
        }

        private void Save()
        {
            if (canSave)
            {
                _ = AdvancedBuildsManager.Data.Save();
            }
        }

        private Dictionary<int, Trait> GetTraits()
        {
            var dic = new Dictionary<int, Trait>();

            foreach (var p in AdvancedBuildsManager.Data.Professions)
            {
                foreach (var s in p.Value.Specializations)
                {
                    foreach (var t in s.Value.MinorTraits)
                    {
                        dic.Add(t.Value.Id, t.Value);
                    }

                    foreach (var t in s.Value.MajorTraits)
                    {
                        dic.Add(t.Value.Id, t.Value);
                    }
                }
            }

            return dic;
        }

        private void CreateUI()
        {
            canSave = false;

            _ = new Label()
            {
                Text = "Enviroment Flags",
                Parent = _infosPanel,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
            };
            _terrestrialFlag = UI.CreateLabeledControl<Checkbox>(_infosPanel, "Terrestrial");
            _terrestrialFlag.Item2.CheckedChangedAction = (c) =>
            {
                if (SkillConnection is not null)
                {
                    if (c)
                    {
                        SkillConnection.Enviroment = SkillConnection.Enviroment | Enviroment.Terrestrial;
                    }
                    else
                    {
                        SkillConnection.Enviroment &= ~Enviroment.Terrestrial;
                    }

                    Save();
                }
            };
            _aquaticFlag = UI.CreateLabeledControl<Checkbox>(_infosPanel, "Aquatic");
            _aquaticFlag.Item2.CheckedChangedAction = (c) =>
            {
                if (SkillConnection is not null)
                {
                    if (c)
                    {
                        SkillConnection.Enviroment = SkillConnection.Enviroment | Enviroment.Aquatic;
                    }
                    else
                    {
                        SkillConnection.Enviroment &= ~Enviroment.Aquatic;
                    }

                    Save();
                }
            };

            _weapon = UI.CreateLabeledControl<Dropdown>(_infosPanel, "Weapon", 100, 175);
            Enum.GetValues(typeof(SkillWeaponType))
                .Cast<SkillWeaponType>()
                .ToList()
                .ForEach(s => _weapon.Item2.Items.Add(s.ToString()));

            _weapon.Item2.ValueChangedAction = (num) =>
            {
                SkillConnection.Weapon = (SkillWeaponType?)Enum.Parse(typeof(SkillWeaponType), num);
                Save();
            };

            _specialization = UI.CreateLabeledControl<Dropdown>(_infosPanel, "Specialization", 100, 175);
            Enum.GetValues(typeof(SpecializationType))
                .Cast<SpecializationType>()
                .ToList()
                .ForEach(s => _specialization.Item2.Items.Add(s.ToString()));

            _specialization.Item2.ValueChangedAction = (num) =>
            {
                SkillConnection.Specialization = (SpecializationType?)Enum.Parse(typeof(SpecializationType), num);
                Save();
            };

            _ = new Label()
            {
                Text = "Attunements",
                Parent = _infosPanel,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
            };

            _fireFlag = UI.CreateLabeledControl<Checkbox>(_infosPanel, "Fire");
            _fireFlag.Item2.CheckedChangedAction = (c) =>
            {
                if (SkillConnection is not null)
                {
                    if (c)
                    {
                        SkillConnection.Attunement = SkillConnection.Attunement | AttunementType.Fire;
                    }
                    else
                    {
                        SkillConnection.Attunement &= ~AttunementType.Fire;
                    }

                    Save();
                }
            };

            _waterFlag = UI.CreateLabeledControl<Checkbox>(_infosPanel, "Water");
            _waterFlag.Item2.CheckedChangedAction = (c) =>
            {
                if (SkillConnection is not null)
                {
                    if (c)
                    {
                        SkillConnection.Attunement = SkillConnection.Attunement | AttunementType.Water;
                    }
                    else
                    {
                        SkillConnection.Attunement &= ~AttunementType.Water;
                    }

                    Save();
                }
            };

            _airFlag = UI.CreateLabeledControl<Checkbox>(_infosPanel, "Air");
            _airFlag.Item2.CheckedChangedAction = (c) =>
            {
                if (SkillConnection is not null)
                {
                    if (c)
                    {
                        SkillConnection.Attunement = SkillConnection.Attunement | AttunementType.Air;
                    }
                    else
                    {
                        SkillConnection.Attunement &= ~AttunementType.Air;
                    }

                    Save();
                }
            };

            _earthFlag = UI.CreateLabeledControl<Checkbox>(_infosPanel, "Earth");
            _earthFlag.Item2.CheckedChangedAction = (c) =>
            {
                if (SkillConnection is not null)
                {
                    if (c)
                    {
                        SkillConnection.Attunement = SkillConnection.Attunement | AttunementType.Earth;
                    }
                    else
                    {
                        SkillConnection.Attunement &= ~AttunementType.Earth;
                    }

                    Save();
                }
            };

            _default = UI.CreateLabeledControl<SkillControl>(_singleSkillsPanel, "Default", 100, 400, 32);
            _default.Item2.OnChangedAction = (prevId, newId) =>
            {
                SkillConnection.Default = newId;
                Save();
            };
            _default.Item2.OnIconAction = (skill) =>
            {
                _skillSelector.Location = _default.Item2.AbsoluteBounds.Add(64, 32, 0, 0).Location;
                _skillSelector.Anchor = _default.Item2;
                _skillSelector.Show();
            };

            _pvp = UI.CreateLabeledControl<SkillControl>(_singleSkillsPanel, "Pvp", 100, 400, 32);
            _pvp.Item2.OnChangedAction = (prevId, newId) =>
            {
                SkillConnection.Default = newId;
                Save();
            };
            _pvp.Item2.OnIconAction = (skill) =>
            {
                _skillSelector.Location = _pvp.Item2.AbsoluteBounds.Add(64, 32, 0, 0).Location;
                _skillSelector.Anchor = _pvp.Item2;
                _skillSelector.Show();
            };

            _enviromentalCounterskill = UI.CreateLabeledControl<SkillControl>(_singleSkillsPanel, "Env. Counter", 100, 400, 32);
            _enviromentalCounterskill.Item2.OnChangedAction = (prevId, newId) =>
            {
                SkillConnection.EnviromentalCounterskill = newId;
                Save();
            };
            _enviromentalCounterskill.Item2.OnIconAction = (skill) =>
            {
                _skillSelector.Location = _enviromentalCounterskill.Item2.AbsoluteBounds.Add(64, 32, 0, 0).Location;
                _skillSelector.Anchor = _enviromentalCounterskill.Item2;
                _skillSelector.Show();
            };

            _unleashed = UI.CreateLabeledControl<SkillControl>(_singleSkillsPanel, "Unleashed", 100, 400, 32);
            _unleashed.Item2.OnChangedAction = (prevId, newId) =>
            {
                SkillConnection.Unleashed = newId;
                Save();
            };
            _unleashed.Item2.OnIconAction = (skill) =>
            {
                _skillSelector.Location = _unleashed.Item2.AbsoluteBounds.Add(64, 32, 0, 0).Location;
                _skillSelector.Anchor = _unleashed.Item2;
                _skillSelector.Show();
            };

            _toolbelt = UI.CreateLabeledControl<SkillControl>(_singleSkillsPanel, "Toolbelt", 100, 400, 32);
            _toolbelt.Item2.OnChangedAction = (prevId, newId) =>
            {
                SkillConnection.Toolbelt = newId;
                Save();
            };
            _toolbelt.Item2.OnIconAction = (skill) =>
            {
                _skillSelector.Location = _toolbelt.Item2.AbsoluteBounds.Add(64, 32, 0, 0).Location;
                _skillSelector.Anchor = _toolbelt.Item2;
                _skillSelector.Show();
            };

            _chain = new("Chain", _skillSelector) { Parent = _contentPanel };
            _burst = new("Burst", _skillSelector) { Parent = _contentPanel };
            _stealth = new("Stealth", _skillSelector) { Parent = _contentPanel };
            _transform = new("Transform", _skillSelector) { Parent = _contentPanel };
            _bundle = new("Bundle", _skillSelector) { Parent = _contentPanel };
            _flipSkills = new("FlipSkills", _skillSelector) { Parent = _contentPanel };
            _dualSkill = new("DualSkill", _skillSelector) { Parent = _contentPanel };

            _pets = new("Pets", _petSelector) { Parent = _contentPanel };
            _traited = new("Traited", _traitSelector, _skillSelector) { Parent = _contentPanel };

            canSave = true;
        }

        private void ApplySkill()
        {
            if (SkillConnection == null) return;

            canSave = false;

            Skill = AdvancedBuildsManager.Data.BaseSkills.GetValueOrDefault(SkillConnection.Id);
            TitleIcon = Skill?.Icon;
            Title = $"{Skill?.Name} [{Skill?.Id}]";

            _terrestrialFlag.Item2.Checked = SkillConnection.Enviroment.HasFlag(Enviroment.Terrestrial);
            _aquaticFlag.Item2.Checked = SkillConnection.Enviroment.HasFlag(Enviroment.Aquatic);

            _fireFlag.Item2.Checked = SkillConnection.Attunement.HasFlag(AttunementType.Fire);
            _waterFlag.Item2.Checked = SkillConnection.Attunement.HasFlag(AttunementType.Water);
            _airFlag.Item2.Checked = SkillConnection.Attunement.HasFlag(AttunementType.Air);
            _earthFlag.Item2.Checked = SkillConnection.Attunement.HasFlag(AttunementType.Earth);

            _weapon.Item2.SelectedItem = SkillConnection.Weapon == null ? SkillWeaponType.None.ToString() : SkillConnection.Weapon.ToString();
            _specialization.Item2.SelectedItem = SkillConnection.Specialization == null ? SpecializationType.None.ToString() : SkillConnection.Specialization.ToString();

            _default.Item2.Skill = SkillConnection.Default is not null ? AdvancedBuildsManager.Data.BaseSkills.GetValueOrDefault((int)SkillConnection.Default) : null;
            _pvp.Item2.Skill = SkillConnection.Pvp is not null ? AdvancedBuildsManager.Data.BaseSkills.GetValueOrDefault((int)SkillConnection.Pvp) : null;
            _enviromentalCounterskill.Item2.Skill = SkillConnection.EnviromentalCounterskill is not null ? AdvancedBuildsManager.Data.BaseSkills.GetValueOrDefault((int)SkillConnection.EnviromentalCounterskill) : null;
            _unleashed.Item2.Skill = SkillConnection.Unleashed is not null ? AdvancedBuildsManager.Data.BaseSkills.GetValueOrDefault((int)SkillConnection.Unleashed) : null;
            _toolbelt.Item2.Skill = SkillConnection.Toolbelt is not null ? AdvancedBuildsManager.Data.BaseSkills.GetValueOrDefault((int)SkillConnection.Toolbelt) : null;

            //_singleSkillsPanel.Collapsed = false;

            _chain.SkillConnection = SkillConnection;
            _chain.Item = SkillConnection.Chain;
            _chain.Collapsed = !(SkillConnection.Chain?.HasValues() == true);

            _flipSkills.SkillConnection = SkillConnection;
            _flipSkills.Item = SkillConnection.FlipSkills;
            _flipSkills.Collapsed = !(SkillConnection.FlipSkills?.HasValues() == true);

            _burst.SkillConnection = SkillConnection;
            _burst.Item = SkillConnection.Burst;
            _burst.Collapsed = !(SkillConnection.Burst?.HasValues() == true);

            _stealth.SkillConnection = SkillConnection;
            _stealth.Item = SkillConnection.Stealth;
            _stealth.Collapsed = !(SkillConnection.Stealth?.HasValues() == true);

            _transform.SkillConnection = SkillConnection;
            _transform.Item = SkillConnection.Transform;
            _transform.Collapsed = !(SkillConnection.Transform?.HasValues() == true);

            _bundle.SkillConnection = SkillConnection;
            _bundle.Item = SkillConnection.Bundle;
            _bundle.Collapsed = !(SkillConnection.Bundle?.HasValues() == true);

            _dualSkill.SkillConnection = SkillConnection;
            _dualSkill.Item = SkillConnection.DualSkill;
            _dualSkill.Collapsed = !(SkillConnection.DualSkill?.HasValues() == true);

            _pets.SkillConnection = SkillConnection;
            _pets.Item = SkillConnection.Pets;
            _pets.Collapsed = !(SkillConnection.Pets?.HasValues() == true);

            _traited.Collapsed = true;
            _traited.SkillConnection = SkillConnection;
            _traited.Item = SkillConnection.Traited;
            _traited.Collapsed = !(SkillConnection.Traited?.HasValues() == true);

            canSave = true;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (_contentPanel is not null)
            {
                _contentPanel.Width = Width;
                foreach (var c in _contentPanel.Children)
                {
                    c.Width = _contentPanel.ContentRegion.Width - 20;
                }
            }
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);
            if (_created != 0 && Common.Now - _created >= 150)
            {
                RecalculateLayout();
                _created = 0;
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            spriteBatch.DrawOnCtrl(this, Textures.Pixel, ContentRegion, Color.Black * 0.3F);
        }
    }
}
