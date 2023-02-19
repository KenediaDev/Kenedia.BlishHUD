using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Input;
using Gw2Sharp;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Res;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Views;
using Kenedia.Modules.Dev.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.TextureAtlases;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Blish_HUD.ContentService;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Dev.Views
{
    class SkillListItem : Blish_HUD.Controls.Control
    {
        private Skill _skill;
        private AsyncTexture2D _skillIcon;
        private string _skillName;
        private string _skillId;
        private Rectangle _iconBounds;
        private Rectangle _idBounds;
        private Rectangle _nameBounds;

        public Skill Skill { get => _skill; set => Common.SetProperty(ref _skill, value, ApplySkill); }
        public KeyValuePair<int, SkillConnection> Connection { get; internal set; }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int padding = 2;
            _iconBounds = new(padding, padding, Height - (padding * 2), Height - (padding * 2));
            _idBounds = new(_iconBounds.Right + 5, 0, 75, Height - padding);
            _nameBounds = new(_idBounds.Right + 5, 0, Width - _idBounds.Right + 5, Height - padding);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            var color = MouseOver ? Colors.ColonialWhite : Color.White;

            if (MouseOver)
            {
                var borderColor = Colors.ColonialWhite;
                spriteBatch.DrawOnCtrl(this, Textures.Pixel, bounds, Color.Black * 0.3F);

                // Top
                spriteBatch.DrawOnCtrl(this, Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 2), Rectangle.Empty, borderColor * 0.8f);

                // Bottom
                spriteBatch.DrawOnCtrl(this, Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 2, bounds.Width, 2), Rectangle.Empty, borderColor * 0.8f);

                // Left
                spriteBatch.DrawOnCtrl(this, Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 2, bounds.Height), Rectangle.Empty, borderColor * 0.8f);

                // Right
                spriteBatch.DrawOnCtrl(this, Textures.Pixel, new Rectangle(bounds.Right - 2, bounds.Top, 2, bounds.Height), Rectangle.Empty, borderColor * 0.8f);
            }

            if (_skillIcon != null) spriteBatch.DrawOnCtrl(this, _skillIcon, _iconBounds, Color.White);
            if (_skillId != null) spriteBatch.DrawStringOnCtrl(this, _skillId, Content.DefaultFont16, _idBounds, color);
            if (_skillName != null) spriteBatch.DrawStringOnCtrl(this, _skillName, Content.DefaultFont16, _nameBounds, color);
        }

        private void ApplySkill()
        {
            _skillIcon = Skill?.Icon.GetAssetFromRenderUrl();
            _skillName = Skill?.Name;
            _skillId = Skill?.Id.ToString();

            BasicTooltipText = _skillName;
        }
    }

    class SkillChild : Blish_HUD.Controls.Control
    {
        private readonly DetailedTexture _skillIcon = new()
        {
            FallBackTexture = AsyncTexture2D.FromAssetId(157154),
            TextureRegion = new(14, 14, 100, 100),
        };

        private readonly DetailedTexture _up = new(2175788, 2175790);
        private readonly DetailedTexture _down = new(2175788, 2175790);
        private readonly DetailedTexture _delete = new(2175783, 2175784);

        private int _index;
        private Skill _skill;

        private Rectangle _nameBounds;
        private Rectangle _idBounds;

        public SkillChild()
        {
            Height = 32;
            Width = 400;
        }

        public SkillChild(int skillid) : this()
        {
            Skill = Dev.Skills.Find(e => e.Id == skillid);
        }

        public Skill Skill { get => _skill; set => Common.SetProperty(ref _skill, value, ApplySkill); }

        public SkillConnection Connection { get; set; }

        public Action<int> OnSkillAction { get; set; }

        public Action<int> OnDeleteAction { get; set; }

        public Action<int> OnUpAction { get; set; }

        public Action<int> OnDownAction { get; set; }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            int padding = 4;
            int sizePadding = padding * 2;

            _delete.Bounds = new(0, 0, Height - sizePadding, Height - sizePadding);
            _up.Bounds = new(_delete.Bounds.Right + 5, 0, Height - sizePadding, Height - sizePadding);
            _down.Bounds = new(_up.Bounds.Right + 5, 0, Height - sizePadding, Height - sizePadding);
            _skillIcon.Bounds = new(_down.Bounds.Right + 5, 0, Height - sizePadding, Height - sizePadding);
            _idBounds = new(_skillIcon.Bounds.Right + 10, 0, 50, Height - sizePadding);
            _nameBounds = new(_idBounds.Right + 5, 0, Math.Max(0, Width - _idBounds.Right), Height - sizePadding);
        }

        private void ApplySkill()
        {
            _skillIcon.Texture = Skill?.Icon.GetAssetFromRenderUrl();
            _ = Dev.ModuleInstance.Save();
        }

        private void MoveUp()
        {

        }

        private void MoveDown()
        {

        }

        private void Delete()
        {

        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if (_delete.Hovered)
            {
                OnDeleteAction?.Invoke(Skill.Id);
                Dispose();
            }

            if (_up.Hovered) OnUpAction?.Invoke(Skill.Id);
            if (_down.Hovered) OnDownAction?.Invoke(Skill.Id);
            if (_skillIcon.Hovered) OnSkillAction?.Invoke(Skill.Id);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            string txt = string.Empty;
            _delete.Draw(this, spriteBatch, RelativeMousePosition);
            if (_delete.Hovered) txt = "Delete";

            _up.Draw(this, spriteBatch, SpriteEffects.FlipVertically, RelativeMousePosition);
            if (_up.Hovered) txt = "Move Up";

            _down.Draw(this, spriteBatch, RelativeMousePosition);
            if (_down.Hovered) txt = "Move Down";

            if (Skill != null)
            {
                _skillIcon.Draw(this, spriteBatch);
                spriteBatch.DrawStringOnCtrl(this, $"{Skill.Id}", Content.DefaultFont16, _idBounds, Color.White);
                spriteBatch.DrawStringOnCtrl(this, Skill.Name, Content.DefaultFont16, _nameBounds, Color.White);
            }

            BasicTooltipText = txt;
        }
    }

    class SkillSelector : Panel
    {
        private readonly FilterBox _filter;
        private readonly FlowPanel _selectionPanel;
        private Dictionary<int, SkillConnection> _connections = new();
        private readonly List<SkillListItem> _connectionItems = new();

        public SkillSelector()
        {
            _filter = new()
            {
                Parent = this,
                TextChangedAction = FilterConnections,
                Location = new(0, 0),
                BackgroundColor = Color.Gray * 0.5F,
                Width = 250,
            };

            _selectionPanel = new FlowPanel()
            {
                Parent = this,
                Location = new(0, _filter.Bottom + 5),
                FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleTopToBottom,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                Width = 300,
                CanScroll = true,
                ControlPadding = new(2),
                BorderColor = Color.Black,
                BorderWidth = new(2),
                ContentPadding = new(2),
            };

            Input.Mouse.LeftMouseButtonPressed += Mouse_LeftMouseButtonPressed;
        }

        protected override Blish_HUD.Controls.CaptureType CapturesInput()
        {
            return Blish_HUD.Controls.CaptureType.Mouse;
        }

        private async void Mouse_LeftMouseButtonPressed(object sender, MouseEventArgs e)
        {
            if (Parent == Graphics.SpriteScreen && Visible && !MouseOver)
            {
                await DelayedHide();
            }
        }

        public event EventHandler<Skill> SkillClicked;

        public Dictionary<int, SkillConnection> Connections { get => _connections; set => Common.SetProperty(ref _connections, value, CreateUI); }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (_filter != null) _filter.Width = Width;
            if (_selectionPanel != null) _selectionPanel.Width = Width;

            foreach (var item in _connectionItems)
            {
                item.Width = _selectionPanel.ContentRegion.Width - 20;
            }
        }

        private void FilterConnections(string obj)
        {
            foreach (var connection in _connectionItems)
            {
                connection.Visible = connection.Skill?.Name.ToLower().Contains(obj.ToLower()) == true || connection.Skill?.Id.ToString().ToLower().Contains(obj.ToLower()) == true;
            }

            _selectionPanel?.Invalidate();
        }

        private async Task DelayedHide()
        {
            Hide();
            await Task.Delay(125);
        }

        public void CreateUI()
        {
            foreach (var connection in Connections.OrderBy(e => e.Value.Specialization).ToList())
            {
                SkillListItem cn;
                _connectionItems.Add(cn = new()
                {
                    Parent = _selectionPanel,
                    Height = 32,
                    Skill = Dev.Skills.Find(e => e.Id == connection.Value.Id),
                    Connection = connection,
                });

                cn.Click += Clicked;
            }

            _selectionPanel.SortChildren((SkillListItem a, SkillListItem b) =>
            {
                int r1 = a.Skill.Name.CompareTo(b.Skill.Name);
                int r2 = a.Skill.Id.CompareTo(b.Skill.Id);
                return r1 == 0 ? r2 - r1 : r1;
            });

            RecalculateLayout();
        }

        private async void Clicked(object sender, MouseEventArgs e)
        {
            SkillClicked?.Invoke(sender, (sender as SkillListItem).Skill);
            if (Parent == Graphics.SpriteScreen)
            {
                await DelayedHide();
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (_connectionItems.Count == 0)
            {
                CreateUI();
            }

            RecalculateLayout();

            _filter.Focused = true;
        }
    }

    class SkillChildsPanel : Panel
    {
        private readonly SkillSelector _skillSelector;
        private readonly ImageButton _add;
        private readonly FlowPanel _skillsPanel;
        private List<int> _skills = new();
        private Dictionary<int, SkillConnection> _connections;
        private readonly string _clearTitle;

        public SkillChildsPanel(string title)
        {
            HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize;
            CanCollapse = true;
            Title = title;
            _clearTitle = title;

            _add = new()
            {
                Parent = this,
                Texture = AsyncTexture2D.FromAssetId(155902),
                HoveredTexture = AsyncTexture2D.FromAssetId(155904),
                ClickAction = (m) =>
                {
                    _skillSelector.Location = new(_add.AbsoluteBounds.Center.X, _add.AbsoluteBounds.Bottom + 3);
                    _ = _skillSelector.ToggleVisibility();
                },
                BasicTooltipText = "Add",
                Size = new(48),
            };

            _skillSelector = new()
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
            };

            _skillSelector.SkillClicked += SkillSelected;

            _skillsPanel = new()
            {
                Parent = this,
                Location = new(_add.Right + 5, 0),
                WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize,
                ContentPadding = new(10, 5),
            };
        }


        public Action<int> OnSkillAction { get; set; }

        public Action<int> OnDeleteAction { get; set; }

        public Action<int> OnUpAction { get; set; }

        public Action<int> OnDownAction { get; set; }

        public Action<int> OnAddAction { get; set; }

        public Action<int> OnChangedAction{ get; set; }

        public Dictionary<int, SkillConnection> Connections { get => _connections; set => Common.SetProperty(ref _connections, value, CreateUI); }

        public List<int> Skills
        {
            get => _skills;
            set
            {
                var newValue = value ?? new();
                _skills = newValue;
                ApplySkills();
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

        }

        private void ApplySkills()
        {
            _skillsPanel.Children.Clear();

            foreach (int skill in Skills)
            {
                var ctrl = new SkillChild(skill)
                {
                    Parent = _skillsPanel,
                    Height = 32,
                    Width = Width - 20,
                    OnDownAction = OnDown,
                    OnUpAction = OnUp,
                    OnDeleteAction = OnDelete,
                    OnSkillAction = OnSkill,
                };
            }

            Title = $"{_clearTitle} [{Skills.Count}]";
        }

        private void CreateUI()
        {
            _skillSelector.Connections = Connections;
        }

        private void SkillSelected(object sender, Skill e)
        {
            AddSkill(e.Id);
        }

        private void OnSkill(int id)
        {

        }

        private void OnDelete(int id)
        {
            RemoveSkill(id);
            _ = Dev.ModuleInstance.Save();
        }

        private void OnUp(int id)
        {
            int index = Skills.FindIndex(e => e == id);

            if (index > 0)
            {
                _ = Skills.Remove(id);
                Skills.Insert(index - 1, id);
                ApplySkills();
                OnUpAction?.Invoke(id);
                OnChangedAction?.Invoke(id);
                _ = Dev.ModuleInstance.Save();
            }
        }

        private void OnDown(int id)
        {
            int index = Skills.FindIndex(e => e == id);

            if (id != Skills.Last())
            {
                _ = Skills.Remove(id);
                Skills.Insert(index + 1, id);
                ApplySkills();

                OnDownAction?.Invoke(id);
                OnChangedAction?.Invoke(id);
                _ = Dev.ModuleInstance.Save();
            }
        }

        public void AddSkill(int id)
        {
            if (!Skills.Contains(id))
            {
                Skills.Add(id);
                ApplySkills();

                OnAddAction?.Invoke(id);
                OnChangedAction?.Invoke(id);
                _ = Dev.ModuleInstance.Save();
            }
        }

        public void AddSkills(List<int> ids)
        {
            Skills.AddRange(ids.Except(Skills));
            ApplySkills();

            OnAddAction?.Invoke(0);
            OnChangedAction?.Invoke(0);
            _ = Dev.ModuleInstance.Save();
        }

        public void RemoveSkill(int id)
        {
            if (Skills.Contains(id))
            {
                _ = Skills.Remove(id);
                ApplySkills();
                OnDeleteAction?.Invoke(id);
                OnChangedAction?.Invoke(id);
                _ = Dev.ModuleInstance.Save();
            }
        }

        public void RemoveSkills(List<int> ids)
        {
            _ = Skills.RemoveAll(ids.Contains);
            ApplySkills();
            OnDeleteAction?.Invoke(0);
            OnChangedAction?.Invoke(0);
            _ = Dev.ModuleInstance.Save();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();
            _skillSelector?.Dispose();
        }
    }

    class ConnectionControl : Panel
    {
        private Skill _skill;
        private KeyValuePair<int, SkillConnection> _connection;
        private Dictionary<int, SkillConnection> _connections;
        private readonly Image _image;
        private readonly Label _label;
        private readonly FlowPanel _infosPanel;
        private readonly FlowPanel _skillsPanel;
        private readonly SkillChildsPanel _parent;
        private readonly SkillChildsPanel _chain;
        private readonly SkillChildsPanel _bundle;
        private readonly SkillChildsPanel _transform;
        private readonly SkillChildsPanel _flipskills;
        private readonly SkillChildsPanel _traited;
        private readonly SkillChildsPanel _ambush;
        private readonly SkillChildsPanel _stealth;
        private readonly SkillChildsPanel _toolbelt;
        private readonly SkillChildsPanel _adrenalin;

        private readonly (Label, NumberBox) _weapon;
        private readonly (Label, NumberBox) _dualWeapon;
        private readonly (Label, NumberBox) _attunement;
        private readonly (Label, NumberBox) _dualAttunement;
        private readonly (Label, NumberBox) _specialization;
        private readonly (Label, NumberBox) _adrenalinCharges;
        private readonly (Label, NumberBox) _enviroment;

        private double _created = 0;

        public ConnectionControl()
        {
            HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill;
            Title = "Editing Skill Connection";
            TitleIcon = AsyncTexture2D.FromAssetId(156706);
            CanScroll = true;

            _infosPanel = new FlowPanel()
            {
                Parent = this,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize,
                WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                ContentPadding = new(10, 5),
                ControlPadding = new(5),
            };

            _specialization = CreateLabeledNumber(_infosPanel, () => "Specialization");
            _specialization.Item2.MinValue = (int)Enum.GetValues(typeof(Specializations)).Cast<Specializations>().Min();
            _specialization.Item2.MaxValue = (int)Enum.GetValues(typeof(Specializations)).Cast<Specializations>().Max();
            _specialization.Item2.ValueChangedAction = (num) =>
            {
                _specialization.Item1.Text = string.Format("Specialization: {0}", ((Specializations)num).ToString());
                Connection.Value.Specialization = (Specializations)num;
                _ = Dev.ModuleInstance.Save();
            };

            _enviroment = CreateLabeledNumber(_infosPanel, () => "Enviroment");
            _enviroment.Item2.MinValue = (int)Enum.GetValues(typeof(Enviroment)).Cast<Enviroment>().Min();
            _enviroment.Item2.MaxValue = (int)Enum.GetValues(typeof(Enviroment)).Cast<Enviroment>().Max();
            _enviroment.Item2.ValueChangedAction = (num) =>
            {
                _enviroment.Item1.Text = $"Enviroment: {(Enviroment)num}";
                Connection.Value.Enviroment = (Enviroment)num;
                _ = Dev.ModuleInstance.Save();
            };

            _weapon = CreateLabeledNumber(_infosPanel, () => "Weapon");
            _weapon.Item2.MinValue = (int)Enum.GetValues(typeof(SkillWeaponType)).Cast<SkillWeaponType>().Min();
            _weapon.Item2.MaxValue = (int)Enum.GetValues(typeof(SkillWeaponType)).Cast<SkillWeaponType>().Max();
            _weapon.Item2.ValueChangedAction = (num) =>
            {
                _weapon.Item1.Text = string.Format("Weapon: {0}", (SkillWeaponType)num);
                Connection.Value.Weapon = (SkillWeaponType)num;
                _ = Dev.ModuleInstance.Save();
            };

            _dualWeapon = CreateLabeledNumber(_infosPanel, () => "Dual Weapon");
            _dualWeapon.Item2.MinValue = (int)Enum.GetValues(typeof(SkillWeaponType)).Cast<SkillWeaponType>().Min();
            _dualWeapon.Item2.MaxValue = (int)Enum.GetValues(typeof(SkillWeaponType)).Cast<SkillWeaponType>().Max();
            _dualWeapon.Item2.ValueChangedAction = (num) =>
            {
                _dualWeapon.Item1.Text = string.Format("Dual Weapon: {0}", ((SkillWeaponType)num).ToString());
                Connection.Value.DualWeapon = (SkillWeaponType)num;
                _ = Dev.ModuleInstance.Save();
            };

            _attunement = CreateLabeledNumber(_infosPanel, () => "Attunement");
            _attunement.Item2.MinValue = (int)Enum.GetValues(typeof(Attunement)).Cast<Attunement>().Min();
            _attunement.Item2.MaxValue = (int)Enum.GetValues(typeof(Attunement)).Cast<Attunement>().Max();
            _attunement.Item2.ValueChangedAction = (num) =>
            {
                _attunement.Item1.Text = string.Format("Attunement: {0}", ((Attunement)num).ToString());
                Connection.Value.Attunement = (Attunement)num;
                _ = Dev.ModuleInstance.Save();
            };

            _dualAttunement = CreateLabeledNumber(_infosPanel, () => "Dual Attunement");
            _dualAttunement.Item2.MinValue = (int)Enum.GetValues(typeof(Attunement)).Cast<Attunement>().Min();
            _dualAttunement.Item2.MaxValue = (int)Enum.GetValues(typeof(Attunement)).Cast<Attunement>().Max();
            _dualAttunement.Item2.ValueChangedAction = (num) =>
            {
                _dualAttunement.Item1.Text = string.Format("Dual Attunement: {0}", ((Attunement)num).ToString());
                Connection.Value.DualAttunement = (Attunement)num;
                _ = Dev.ModuleInstance.Save();
            };

            _adrenalinCharges = CreateLabeledNumber(_infosPanel, () => "AdrenalinCharges");
            _adrenalinCharges.Item2.MinValue = 0;
            _adrenalinCharges.Item2.MaxValue = 3;
            _adrenalinCharges.Item2.ValueChangedAction = (num) =>
            {
                _adrenalinCharges.Item1.Text = $"AdrenalinCharges: {num}";
                Connection.Value.AdrenalinCharges = num;
                _ = Dev.ModuleInstance.Save();
            };

            _skillsPanel = new()
            {
                Parent = this,
                Location = new(0, _infosPanel.Bottom + 5),
                WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize,
                FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleTopToBottom,
            };

            _parent = new("Parent")
            {
                Width = 350,
                Parent = _skillsPanel,
            };
            _chain = new("Chain")
            {
                Width = 350,
                Parent = _skillsPanel,
                Collapsed = true,
            };
            _bundle = new("Bundle")
            {
                Width = 350,
                Parent = _skillsPanel,
                Collapsed = true,
            };
            _transform = new("Transform")
            {
                Width = 350,
                Parent = _skillsPanel,
                Collapsed = true,
            };
            _flipskills = new("FlipSkills")
            {
                Width = 350,
                Parent = _skillsPanel,
                Collapsed = true,
            };
            _traited = new("Traited")
            {
                Width = 350,
                Parent = _skillsPanel,
                Collapsed = true,
            };
            _ambush = new("Ambush")
            {
                Width = 350,
                Parent = _skillsPanel,
                Collapsed = true,
            };
            _stealth = new("Stealth")
            {
                Width = 350,
                Parent = _skillsPanel,
                Collapsed = true,
            };
            _toolbelt = new("Toolbelt")
            {
                Width = 350,
                Parent = _skillsPanel,
                Collapsed = true,
            };
            _adrenalin = new("Adrenalin")
            {
                Width = 350,
                Parent = _skillsPanel,
                Collapsed = true,
            };

            _created = Common.Now();
        }

        public Skill Skill { get => _skill; set => Common.SetProperty(ref _skill, value, ApplySkill); }

        public Action OnChangedAction { get; set; }

        public KeyValuePair<int, SkillConnection> Connection { get => _connection; internal set => Common.SetProperty(ref _connection, value, ApplyConnection); }

        public Dictionary<int, SkillConnection> Connections { get => _connections; set => Common.SetProperty(ref _connections, value, CreateUI); }

        private void CreateUI()
        {
            _parent.Connections = Connections;
            _chain.Connections = Connections;
            _bundle.Connections = Connections;
            _transform.Connections = Connections;
            _flipskills.Connections = Connections;
            _traited.Connections = Connections;
            _ambush.Connections = Connections;
            _stealth.Connections = Connections;
            _toolbelt.Connections = Connections;
            _adrenalin.Connections = Connections;

            void _parentAction(int id)
            {
                Connection.Value.Parent = _parent.Skills.FirstOrDefault();
                Connection.Value.Parent = Connection.Value.Parent == 0 ? null : Connection.Value.Parent;
            }
            _parent.OnChangedAction = _parentAction;

            void _adrenalinAction(int id)
            {
                Connection.Value.AdrenalinStages = Connection.Value.AdrenalinStages ?? new();

                Connection.Value.AdrenalinStages.Clear();
                Connection.Value.AdrenalinStages.AddRange(_adrenalin.Skills);

                if (Connection.Value.AdrenalinStages.Count == 0) Connection.Value.AdrenalinStages = null;
            }
            _adrenalin.OnChangedAction = _adrenalinAction;

            void _chainAction(int id)
            {
                Connection.Value.Chain = Connection.Value.Chain ?? new();

                Connection.Value.Chain.Clear();
                Connection.Value.Chain.AddRange(_chain.Skills);

                if (Connection.Value.Chain.Count == 0) Connection.Value.Chain = null;
            }
            _chain.OnChangedAction = _chainAction;

            void _bundleAction(int id)
            {
                Connection.Value.AdrenalinStages = Connection.Value.AdrenalinStages ?? new();

                Connection.Value.Bundle.Clear();
                Connection.Value.Bundle.AddRange(_bundle.Skills);

                if (Connection.Value.Bundle.Count == 0) Connection.Value.Bundle = null;
            }
            _bundle.OnChangedAction = _bundleAction;

            void _transformAction(int id)
            {
                Connection.Value.AdrenalinStages = Connection.Value.AdrenalinStages ?? new();

                Connection.Value.Transform.Clear();
                Connection.Value.Transform.AddRange(_transform.Skills);

                if (Connection.Value.Transform.Count == 0) Connection.Value.Transform = null;
            }
            _transform.OnChangedAction = _transformAction;

            void _flipskillsAction(int id)
            {
                Connection.Value.AdrenalinStages = Connection.Value.AdrenalinStages ?? new();

                Connection.Value.FlipSkills.Clear();
                Connection.Value.FlipSkills.AddRange(_flipskills.Skills);

                if (Connection.Value.FlipSkills.Count == 0) Connection.Value.FlipSkills = null;
            }
            _flipskills.OnChangedAction = _flipskillsAction;

            void _traitedAction(int id)
            {
                //Connection.Value.Traited = Connection.Value.Traited ?? new();
                //Connection.Value.Traited.Clear();
                //Connection.Value.Traited.AddRange(_traited.Skills);
            }
            _traited.OnChangedAction = _traitedAction;

            void _ambushAction(int id)
            {
                Connection.Value.Ambush = _parent.Skills.FirstOrDefault();
                Connection.Value.Ambush = Connection.Value.Ambush == 0 ? null : Connection.Value.Ambush;
            }
            _ambush.OnChangedAction = _ambushAction;

            void _stealthAction(int id)
            {
                Connection.Value.Stealth = _parent.Skills.FirstOrDefault();
                Connection.Value.Stealth = Connection.Value.Stealth == 0 ? null : Connection.Value.Stealth;
            }
            _stealth.OnChangedAction = _stealthAction;

            void _toolbeltAction(int id)
            {
                Connection.Value.Toolbelt = _parent.Skills.FirstOrDefault();
                Connection.Value.Toolbelt = Connection.Value.Toolbelt == 0 ? null : Connection.Value.Toolbelt;
            }
            _toolbelt.OnChangedAction = _toolbeltAction;
        }

        private void ApplySkill()
        {
            TitleIcon = Skill?.Icon.GetAssetFromRenderUrl();
            Title = $"{Skill?.Name} [{Skill?.Id}]";
        }

        private void ApplyConnection()
        {
            Skill = Dev.Skills.Find(e => e.Id == Connection.Value?.Id);

            if (Skill != null)
            {
                _specialization.Item2.Value = Connection.Value.Specialization == null ? 0 : (int)Connection.Value.Specialization;
                _enviroment.Item2.Value = (int)Connection.Value.Enviroment;
                _weapon.Item2.Value = Connection.Value.Weapon == null ? 0 : (int)Connection.Value.Weapon;
                _dualWeapon.Item2.Value = Connection.Value.DualWeapon == null ? 0 : (int)Connection.Value.DualWeapon;
                _attunement.Item2.Value = Connection.Value.Attunement == null ? 0 : (int)Connection.Value.Attunement;
                _dualAttunement.Item2.Value = Connection.Value.DualAttunement == null ? 0 : (int)Connection.Value.DualAttunement;
                _adrenalinCharges.Item2.Value = Connection.Value.AdrenalinCharges == null ? 0 : (int)Connection.Value.AdrenalinCharges;

                _parent.Skills = Connection.Value.Parent != null ? new List<int>() { (int)Connection.Value.Parent, } : null;
                _parent.Collapsed = _parent.Skills.Count == 0;

                _chain.Skills = Connection.Value.Chain ?? null;
                _chain.Collapsed = _chain.Skills.Count == 0;

                _bundle.Skills = Connection.Value.Bundle ?? null;
                _bundle.Collapsed = _bundle.Skills.Count == 0;

                _flipskills.Skills = Connection.Value.FlipSkills ?? null;
                _flipskills.Collapsed = _flipskills.Skills.Count == 0;

                //_traited.Skills = Connection.Value.Chain ?? null;
                _ambush.Skills = Connection.Value.Ambush != null ? new List<int>() { (int)Connection.Value.Ambush, } : null;
                _ambush.Collapsed = _ambush.Skills.Count == 0;

                _stealth.Skills = Connection.Value.Ambush != null ? new List<int>() { (int)Connection.Value.Stealth, } : null;
                _stealth.Collapsed = _stealth.Skills.Count == 0;

                _toolbelt.Skills = Connection.Value.Ambush != null ? new List<int>() { (int)Connection.Value.Toolbelt, } : null;
                _toolbelt.Collapsed = _toolbelt.Skills.Count == 0;
                //_adrenalin.Skills = Connection.Value.Chain ?? null;
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (_skillsPanel != null)
            {
                _skillsPanel.Width = Width;
                foreach (var c in _skillsPanel.Children)
                {
                    c.Width = _skillsPanel.ContentRegion.Width - 20;
                }

                _skillsPanel.Location = new(0, _infosPanel.Bottom + 5);
            }
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);
            if (_created != 0 && Common.Now() - _created >= 5)
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

        private (Label, NumberBox) CreateLabeledNumber(Blish_HUD.Controls.Container parent, Func<string> setLocalizedText, Func<string> setLocalizedTooltip = null)
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
                Width = 175,
                Height = 20,
                SetLocalizedText = setLocalizedText,
                SetLocalizedTooltip = setLocalizedTooltip,
            };

            var num = new NumberBox()
            {
                Location = new(label.Right + 25, 0),
                Width = 100,
                Parent = p,
                MinValue = 0,
                SetLocalizedTooltip = setLocalizedTooltip,
            };

            return (label, num);
        }
    }

    public class MainWindow : StandardWindow
    {
        private Dictionary<int, SkillConnection> _connections = new();
        private readonly ConnectionControl _connectionEdit;
        private readonly SkillSelector _selector;

        public Dictionary<int, SkillConnection> Connections { get => _connections; set => Common.SetProperty(ref _connections, value, CreateUI); }

        public MainWindow(AsyncTexture2D background, Rectangle windowRegion, Rectangle contentRegion) : base(background, windowRegion, contentRegion)
        {
            _selector = new()
            {
                Parent = this,
                Location = new(0, 0),
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                Width = 300,
            };
            _selector.SkillClicked += Selector_SkillClicked;

            _connectionEdit = new()
            {
                Parent = this,
                Location = new(_selector.Right + 10, 0),
                BorderColor = Color.Black,
                BorderWidth = new(2),
                ContentPadding = new(2),
                WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
            };
        }

        private void Selector_SkillClicked(object sender, Skill e)
        {
            _connectionEdit.Connection = (sender as SkillListItem).Connection;
        }

        private void CreateUI()
        {
            _connectionEdit.Connections = Connections;
            _selector.Connections = Connections;
            RecalculateLayout();
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();
        }
    }
}
