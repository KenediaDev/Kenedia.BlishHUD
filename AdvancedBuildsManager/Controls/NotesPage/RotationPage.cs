using Blish_HUD.Content;
using Kenedia.Modules.AdvancedBuildsManager.Controls.Selection;
using Kenedia.Modules.AdvancedBuildsManager.Models.Templates;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Services;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Kenedia.Modules.AdvancedBuildsManager.Controls.NotesPage
{
    public class RotationElementControl : Panel
    {
        private RotationElement _rotationElement = new();
        private readonly Image _skillImage;
        private readonly Label _skillText;

        public RotationElementControl()
        {
            HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize;
            WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill;
            BackgroundColor = Color.Black * 0.2F;
            BorderWidth = new(2);
            BorderColor = Color.Black;
            AutoSizePadding = new(2);

            _skillImage = new()
            {
                Parent = this,
                Size = new(48),
                BackgroundColor = Color.Black * 0.6F,
                HoveredBackgroundColor = Color.Gray * 0.2F,
                Location = new(2),
            };

            _skillText = new()
            {
                Parent = this,
                Location = new(_skillImage.Right + 10, _skillImage.Top),
                Size = new(Width - (_skillImage.Right + 10), _skillImage.Height),
            };

            Menu = new();
            //_ = Menu.AddMenuItem(new ContextMenuItem(() => "Up", null));
            //_ = Menu.AddMenuItem(new ContextMenuItem(() => "Down", null));
            _ = Menu.AddMenuItem(new ContextMenuItem(() => "Delete", Delete));
            _ = Menu.AddMenuItem(new ContextMenuItem(() => "Reset", Reset));
            _ = Menu.AddMenuItem(new ContextMenuItem(() => "Repeat More", () => _rotationElement.Repetition++));
            _ = Menu.AddMenuItem(new ContextMenuItem(() => "Repeat Less", () => _rotationElement.Repetition--));
        }

        private void Reset()
        {
            _rotationElement.Skill = null;
            _rotationElement.Repetition = 1;
        }

        private void Delete()
        {
            Rotation?.RemoveSkill(RotationElement);
            Dispose();
        }

        public RotationElement RotationElement
        {
            get => _rotationElement; set
            {
                var temp = _rotationElement;

                if (Common.SetProperty(ref _rotationElement, value, SetElement))
                {
                    if (temp is not null) temp.PropertyChanged -= RotationElement_PropertyChanged;
                    if (_rotationElement is not null) _rotationElement.PropertyChanged += RotationElement_PropertyChanged;
                }
            }
        }

        public Rotation Rotation { get; set; }

        private void RotationElement_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SetElement();
        }

        private void SetElement()
        {
            _skillImage.Texture = RotationElement?.Skill?.Icon;
            _skillText.Text = RotationElement?.DisplayText;
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            _skillText?.SetLocation(_skillImage.Right + 10, _skillImage.Top);
            _skillText?.SetSize(Width - (_skillImage.Right + 10), _skillImage.Height);
        }
    }

    public class RotationPanel : Panel
    {
        private readonly Label _headerLabel;
        private readonly TextBox _headerEditBox;
        private readonly ImageButton _addSkill;
        private readonly FlowPanel _flowPanel;
        private Rotation _rotation;

        public SelectionPanel SelectionPanel { get; set; }

        public RotationPanel()
        {
            Width = 254;
            HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize;
            BorderColor = Color.Gray * 0.4F;
            BorderWidth = new(2);

            Menu = new();
            _ = Menu.AddMenuItem(new ContextMenuItem(() => "Delete", Delete));

            var headerPanel = new Panel()
            {
                Parent = this,
                WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                Height = 32
            };

            _headerEditBox = new()
            {
                Parent = headerPanel,
                Text = "Rotation Name",
                Width = Width - 4 - 32 - 2,
                Height = 32,
                Location = new(4, 2),
                Font = Content.DefaultFont16,
                Visible = false,
                EnterPressedAction = (txt) =>
                {
                    _headerLabel.Text = txt;
                    _headerLabel.Visible = true;
                    _headerEditBox.Visible = false;

                    Rotation.Name = txt;
                }
            };

            _headerLabel = new()
            {
                Parent = headerPanel,
                Text = "Rotation Name",
                Width = Width - 4 - 32 - 2,
                Height = 32,
                Location = new(6, 2),
                Font = Content.DefaultFont16,
            };
            _headerLabel.Click += HeaderLabel_Click;

            _addSkill = new()
            {
                Parent = headerPanel,
                Texture = AsyncTexture2D.FromAssetId(155902),
                HoveredTexture = AsyncTexture2D.FromAssetId(155904),
                Size = new(32),
                Location = new(Width - 32, 0),
                SetLocalizedTooltip = () => "Add Skill",
                ClickAction = (m) => AddRotationElementControl(null)
            };

            _flowPanel = new()
            {
                Parent = this,
                HeightSizingMode = Blish_HUD.Controls.SizingMode.AutoSize,
                WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                FlowDirection = Blish_HUD.Controls.ControlFlowDirection.SingleTopToBottom,
                Location = new Point(0, _addSkill.Bottom + 2),
                ContentPadding = new(2),
                ControlPadding = new(2),
            };
        }

        private void Delete()
        {
            OnDelete?.Invoke();
            Dispose();
        }

        private void HeaderLabel_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            _headerEditBox.Visible = true;
            _headerEditBox.Text = _headerLabel.Text;
            _headerLabel.Visible = false;
        }

        private void SkillImage_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            SelectionPanel?.SetSkillAnchor((RotationElementControl)sender);
        }

        public void AddRotationElementControl(RotationElement element = null)
        {
            if (Rotation is not null)
            {
                var ctrl = new RotationElementControl()
                {
                    Width = 250,
                    Parent = _flowPanel,
                    RotationElement = element ?? Rotation?.AddSkill(),
                    Rotation = Rotation,
                };

                ctrl.Click += SkillImage_Click;

                if(element == null) {
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(1);
                        SelectionPanel?.SetSkillAnchor(ctrl);
                    });
                }
            }
        }

        public Rotation Rotation { get => _rotation; set => Common.SetProperty(ref _rotation, value, ApplyRotation); }

        private void ApplyRotation(object sender, PropertyChangedEventArgs e)
        {
            _headerLabel.Text = Rotation.Name;
            
            foreach(var item in Rotation)
            {
                AddRotationElementControl(item);
            }
        }

        public Action OnDelete { get; internal set; }
    }

    public class RotationPage : Blish_HUD.Controls.Container
    {
        private readonly bool created = false;
        private TexturesService _texturesService;
        private readonly Button _addRotation;
        private readonly FlowPanel _contentPanel;

        private Template _template;

        public RotationPage(TexturesService texturesService)
        {
            _texturesService = texturesService;

            _addRotation = new()
            {
                Parent = this,
                SetLocalizedText = () => "Add Rotation",
                ClickAction = () => AddRotationPanel(null),
            };

            _contentPanel = new()
            {
                Parent = this,
                Location = new(_addRotation.Left, _addRotation.Bottom),
                HeightSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                WidthSizingMode = Blish_HUD.Controls.SizingMode.Fill,
                ControlPadding = new(2),
                CanScroll = true,
            };

            created = true;
        }

        public SelectionPanel SelectionPanel { get; set; }

        public Template Template
        {
            get => _template; set
            {
                var temp = _template;
                if (Common.SetProperty(ref _template, value, ApplyTemplate))
                {
                    if (temp is not null) temp.PropertyChanged -= TemplateChanged;
                    if (_template is not null) _template.PropertyChanged += TemplateChanged;
                }
            }
        }

        private void AddRotationPanel(Rotation rotation = null)
        {
            if (Template is not null && Template.RotationTemplate is not null)
            {              
                var ctrl = new RotationPanel()
                {
                    Parent = _contentPanel,
                    SelectionPanel = SelectionPanel,
                    Rotation = rotation ?? Template.RotationTemplate.AddRotation(),
                };

                ctrl.OnDelete = () => Template.RotationTemplate.RemoveRotation(ctrl.Rotation);
            }
        }

        private void ApplyTemplate()
        {
            _contentPanel.ClearChildren();

            foreach (Rotation rotation in _template?.RotationTemplate?.Rotations)
            {
                AddRotationPanel(rotation);
            }
        }

        private void TemplateChanged(object sender, PropertyChangedEventArgs e)
        {
            ApplyTemplate();
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();
            if (!created) return;

        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            if (_template is not null) _template.PropertyChanged -= TemplateChanged;
        }
    }
}
