using Blish_HUD.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Blish_HUD.ContentService;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.DataModels;
using SkillWeaponType = Gw2Sharp.WebApi.V2.Models.SkillWeaponType;
using Attunement = Gw2Sharp.WebApi.V2.Models.Attunement;
using Blish_HUD;
using Kenedia.Modules.Core.Extensions;
using System.Diagnostics;
using Gw2Sharp;
using System.ComponentModel;
using Kenedia.Modules.BuildsManager.Views;
using MonoGame.Extended.Collections;
using Newtonsoft.Json.Serialization;
using SharpDX.Direct2D1.Effects;
using Newtonsoft.Json.Linq;

namespace Kenedia.Modules.BuildsManager.Controls.SkillConnectionEdit
{
    public class EditingControl : Panel
    {
        private readonly SkillConnectionSelector _skillSelector;
        private readonly PetSelector _petSelector;

        private Dictionary<int, SkillConnection> _connections;
        private readonly FlowPanel _contentPanel;
        private readonly FlowPanel _infosPanel;
        private readonly FlowPanel _singleSkillsPanel;

        private double _created = 0;
        private SkillConnection _skillConnection;
        private FlowPanel _enviromentFlags;
        private (Label, Checkbox) _terrestrialFlag;
        private (Label, Checkbox) _aquaticFlag;
        private (Label, Dropdown) _weapon;
        private (Label, Dropdown) _specialization;
        private (Label, SingleSkillChild) _default;
        private (Label, SingleSkillChild) _enviromentalCounterskill;
        private (Label, SingleSkillChild) _unleashed;
        private (Label, SingleSkillChild) _toolbelt;
        private ReflectedSkillControl<Chain> _chain;
        private ReflectedSkillControl<Burst> _burst;
        private ReflectedSkillControl<Stealth> _stealth;
        private ReflectedSkillControl<Transform> _transform;
        private ReflectedSkillControl<Bundle> _bundle;
        private ReflectedSkillControl<AttunementSkill> _attunement;
        private ReflectedSkillControl<FlipSkills> _flipSkills;
        private ReflectedSkillControl<DualSkill> _dualSkill;
        private FlowPanel _pets;
        private FlowPanel _traited;
        private bool canSave;

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

            _infosPanel = new FlowPanel()
            {
                Parent = _contentPanel,
                Height = 90,
                ContentPadding = new(10, 5),
                ControlPadding = new(15, 5),
                FlowDirection = Blish_HUD.Controls.ControlFlowDirection.TopToBottom,
            };

            _singleSkillsPanel = new()
            {
                Parent = _contentPanel,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize,
                FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleTopToBottom,
                ContentPadding = new(10, 40, 10, 5),
                Title = "Single Skills",
                CanCollapse = true,
            };

            CreateUI();
            _created = Common.Now();
        }

        private void SkillSelected(SkillConnectionEntryControl obj, SingleSkillChild anchor)
        {
            anchor.Skill = obj.Skill;
        }

        public BaseSkill Skill { get; set; }

        public SkillConnection SkillConnection { get => _skillConnection; set => Common.SetProperty(ref _skillConnection, value, ApplySkill); }

        public Action OnChangedAction { get; set; }

        public Dictionary<int, SkillConnection> Connections { get => _connections; set => Common.SetProperty(ref _connections, value, ApplyConnections); }

        private void ApplyConnections()
        {
            _skillSelector.Items = Connections;
        }

        private void Save()
        {
            if (canSave)
            {
                _ = BuildsManager.Data.Save();
            }
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
                if (SkillConnection != null)
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
                if (SkillConnection != null)
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

            _ = new Label()
            {
                Text = "Requirements",
                Parent = _infosPanel,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
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
            Enum.GetValues(typeof(Specializations))
                .Cast<Specializations>()
                .ToList()
                .ForEach(s => _specialization.Item2.Items.Add(s.ToString()));

            _specialization.Item2.ValueChangedAction = (num) =>
            {
                SkillConnection.Specialization = (Specializations?)Enum.Parse(typeof(Specializations), num);
                Save();
            };

            _default = UI.CreateLabeledControl<SingleSkillChild>(_singleSkillsPanel, "Default", 100, 400, 32);
            _default.Item2.OnChangedAction = (id) =>
            {
                SkillConnection.Default = id;
                Save();
            };
            _default.Item2.OnSkillAction = (skill) =>
            {
                _skillSelector.Location = _default.Item2.AbsoluteBounds.Add(64, 32, 0, 0).Location;
                _skillSelector.Anchor = _default.Item2;
                _skillSelector.Show();
            };

            _enviromentalCounterskill = UI.CreateLabeledControl<SingleSkillChild>(_singleSkillsPanel, "Env. Counter", 100, 400, 32);
            _enviromentalCounterskill.Item2.OnChangedAction = (id) =>
            {
                SkillConnection.EnviromentalCounterskill = id;
                Save();
            };
            _enviromentalCounterskill.Item2.OnSkillAction = (skill) =>
            {
                _skillSelector.Location = _enviromentalCounterskill.Item2.AbsoluteBounds.Add(64, 32, 0, 0).Location;
                _skillSelector.Anchor = _enviromentalCounterskill.Item2;
                _skillSelector.Show();
            };

            _unleashed = UI.CreateLabeledControl<SingleSkillChild>(_singleSkillsPanel, "Unleashed", 100, 400, 32);
            _unleashed.Item2.OnChangedAction = (id) =>
            {
                SkillConnection.Unleashed = id;
                Save();
            };
            _unleashed.Item2.OnSkillAction = (skill) =>
            {
                _skillSelector.Location = _unleashed.Item2.AbsoluteBounds.Add(64, 32, 0, 0).Location;
                _skillSelector.Anchor = _unleashed.Item2;
                _skillSelector.Show();
            };

            _toolbelt = UI.CreateLabeledControl<SingleSkillChild>(_singleSkillsPanel, "Toolbelt", 100, 400, 32);
            _toolbelt.Item2.OnChangedAction = (id) =>
            {
                SkillConnection.Toolbelt = id;
                Save();
            };
            _toolbelt.Item2.OnSkillAction = (skill) =>
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
            _attunement = new("Attunement", _skillSelector) { Parent = _contentPanel };
            _flipSkills = new("FlipSkills", _skillSelector) { Parent = _contentPanel };
            _dualSkill = new("DualSkill", _skillSelector) { Parent = _contentPanel };

            _pets = CreateFlowPanel("Pets");
            _traited = CreateFlowPanel("Traited");

            canSave = true;
        }

        private void ApplySkill()
        {
            if (SkillConnection == null) return;

            canSave = false;

            Skill = BuildsManager.Data.BaseSkills.GetValueOrDefault(SkillConnection.Id);
            TitleIcon = Skill?.Icon;
            Title = $"{Skill?.Name} [{Skill?.Id}]";

            _terrestrialFlag.Item2.Checked = SkillConnection.Enviroment.HasFlag(Enviroment.Terrestrial);
            _aquaticFlag.Item2.Checked = SkillConnection.Enviroment.HasFlag(Enviroment.Aquatic);

            _weapon.Item2.SelectedItem = SkillConnection.Weapon == null ? SkillWeaponType.None.ToString() : SkillConnection.Weapon.ToString();
            _specialization.Item2.SelectedItem = SkillConnection.Specialization == null ? Specializations.None.ToString() : SkillConnection.Specialization.ToString();

            _default.Item2.Skill = SkillConnection.Default != null ? BuildsManager.Data.BaseSkills.GetValueOrDefault((int)SkillConnection.Default) : null;
            _enviromentalCounterskill.Item2.Skill = SkillConnection.EnviromentalCounterskill != null ? BuildsManager.Data.BaseSkills.GetValueOrDefault((int)SkillConnection.EnviromentalCounterskill) : null;
            _unleashed.Item2.Skill = SkillConnection.Unleashed != null ? BuildsManager.Data.BaseSkills.GetValueOrDefault((int)SkillConnection.Unleashed) : null;
            _toolbelt.Item2.Skill = SkillConnection.Toolbelt != null ? BuildsManager.Data.BaseSkills.GetValueOrDefault((int)SkillConnection.Toolbelt) : null;

            _singleSkillsPanel.Collapsed = false;

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

            _attunement.SkillConnection = SkillConnection;
            _attunement.Item = SkillConnection.AttunementSkill;
            _attunement.Collapsed = !(SkillConnection.AttunementSkill?.HasValues() == true);

            _dualSkill.SkillConnection = SkillConnection;
            _dualSkill.Item = SkillConnection.DualSkill;
            _dualSkill.Collapsed = !(SkillConnection.DualSkill?.HasValues() == true);

            _pets.Collapsed = true;
            _traited.Collapsed = true;

            canSave = true;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (_contentPanel != null)
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
            if (_created != 0 && Common.Now() - _created >= 150)
            {
                RecalculateLayout();
                _created = 0;
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            spriteBatch.DrawOnCtrl(this, Textures.Pixel, ContentRegion, Microsoft.Xna.Framework.Color.Black * 0.3F);
        }

        private FlowPanel CreateFlowPanel(string? title)
        {
            return new()
            {
                Parent = _contentPanel,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize,
                FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleTopToBottom,
                ContentPadding = new(10, 40, 10, 5),
                Title = title,
                CanCollapse = true,
            };
        }
    }
}
