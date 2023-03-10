using Blish_HUD.Content;
using Blish_HUD.Controls;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using FlowPanel = Kenedia.Modules.Core.Controls.FlowPanel;
using System.Diagnostics;
using SharpDX.MediaFoundation;
using System;
using Gw2Sharp.Models;
using Kenedia.Modules.BuildsManager.Controls.BuildPage.ProfessionSpecific;
using Panel = Kenedia.Modules.Core.Controls.Panel;
using System.Security.Cryptography.X509Certificates;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Image = Kenedia.Modules.Core.Controls.Image;
using Blish_HUD;
using static Blish_HUD.ContentService;
using System.Diagnostics.Contracts;
using Kenedia.Modules.Core.Services;
using Kenedia.Modules.Core.DataModels;
using TextBox = Kenedia.Modules.Core.Controls.TextBox;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage
{
    public class BuildPage : Container
    {
        private readonly DetailedTexture _specsBackground = new(993592);
        private readonly DetailedTexture _skillsBackground = new(155960);
        private readonly DetailedTexture _skillsBackgroundBottomBorder = new(155987);
        private readonly DetailedTexture _skillsBackgroundTopBorder = new(155989);
        private readonly AsyncTexture2D _pve_Toggle = AsyncTexture2D.FromAssetId(2229699);
        private readonly AsyncTexture2D _pve_Toggle_Hovered = AsyncTexture2D.FromAssetId(2229700);
        private readonly AsyncTexture2D _pvp_Toggle = AsyncTexture2D.FromAssetId(2229701);
        private readonly AsyncTexture2D _pvp_Toggle_Hovered = AsyncTexture2D.FromAssetId(2229702);
        private readonly AsyncTexture2D _editFeather = AsyncTexture2D.FromAssetId(2175780);
        private readonly AsyncTexture2D _editFeatherHovered = AsyncTexture2D.FromAssetId(2175779);

        private Template _template;
        private readonly FlowPanel _specializationsPanel;
        private ProfessionSpecifics _professionSpecifics;
        private readonly Panel _professionSpecificsContainer;
        private readonly SkillsBar _skillbar;
        private readonly Dummy _dummy;
        private readonly Dictionary<SpecializationSlot, SpecLine> _specializations = new()
        {
            {SpecializationSlot.Line_1,  new SpecLine(){ Line = SpecializationSlot.Line_1, } },
            {SpecializationSlot.Line_2,  new SpecLine() {Line = SpecializationSlot.Line_2, } },
            {SpecializationSlot.Line_3,  new SpecLine() {Line = SpecializationSlot.Line_3, } },
        };
        private readonly FramedImage _specIcon;
        private readonly FramedImage _raceIcon;
        private readonly TexturesService _texturesService;
        private readonly TextBox _buildCodeBox;
        private readonly ImageButton _copyButton;

        public BuildPage(TexturesService texturesService)
        {
            _texturesService = texturesService;

            ClipsBounds = false;
            WidthSizingMode = SizingMode.Fill;
            HeightSizingMode = SizingMode.AutoSize;

            _copyButton = new()
            {
                Parent = this,
                Texture = AsyncTexture2D.FromAssetId(2208345),
                HoveredTexture = AsyncTexture2D.FromAssetId(2208347),
                Size = new(26),
                ClickAction = (m) =>
                {
                    try
                    {
                        _ = ClipboardUtil.WindowsClipboardService.SetTextAsync(_buildCodeBox.Text);
                    }
                    catch (ArgumentException)
                    {
                        ScreenNotification.ShowNotification("Failed to set the clipboard text!", ScreenNotification.NotificationType.Error);
                    }
                    catch
                    {
                    }
                }
            };

            _buildCodeBox = new()
            {
                Parent = this,
                Location = new(_copyButton.Right + 2, 0),
                EnterPressedAction = (code) =>
                {
                    Template.BuildTemplate.LoadFromCode(code);
                    ApplyTemplate();
                }
            };

            _professionSpecificsContainer = new()
            {
                Parent = this,
                Location = new(0, _buildCodeBox.Bottom + 7),
                //BackgroundColor= Color.White * 0.2F,
                Width = 500,
                Height = 100,
                ZIndex = 13,
            };

            _skillbar = new SkillsBar()
            {
                Parent = this,
                Location = new(5, _professionSpecificsContainer.Bottom),
                Width = Width,
                ZIndex = 12,
            };

            _specIcon = new FramedImage()
            {
                Parent = this,
                Width = Width,
                Size = new(80),
                ZIndex = 15,
            };

            _raceIcon = new FramedImage()
            {
                Parent = this,
                Width = Width,
                TextureSize = new Point(64),
                ZIndex = 15,
                Texture = _texturesService.GetTexture(@"textures\races\pact.png", "pact"),
                Size = new(80),
            };

            _dummy = new Dummy()
            {
                Parent = this,
                Location = new(0, _skillbar.Bottom),
                Width = Width,
                Height = 20,
                //Height = 45,
            };

            _specializationsPanel = new()
            {
                Parent = this,
                Location = new(0, _dummy.Bottom),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                ControlPadding = new(1),
                BackgroundColor = Color.Black * 0.8F,
                AutoSizePadding = new(1),
                ZIndex = 10,
            };

            _specializations.ToList().ForEach(l =>
            {
                l.Value.Parent = _specializationsPanel;
                //l.Value.TraitsChanged += OnBuildAdjusted;
                //l.Value.SpeclineSwapped += SpeclineSwapped;
                l.Value.CanInteract = () => !_skillbar.IsSelecting;
            });
        }

        public Template Template
        {
            get => _template; set
            {
                var temp = _template;
                if (Common.SetProperty(ref _template, value, ApplyTemplate, value != null))
                {
                    if (temp != null) temp.Changed -= TemplateChanged;
                    if (temp != null) temp.Changed -= TemplateChanged;

                    if (_template != null) _template.Changed += TemplateChanged;
                    if (_template != null) _template.Changed += TemplateChanged;
                }
            }
        }

        private void TemplateChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ApplyTemplate();
        }

        public void ApplyTemplate()
        {
            if (Template != null && (int)Template.BuildTemplate.Profession > 0)
            {
                _buildCodeBox.Text = Template?.BuildTemplate.ParseBuildCode();

                _specializations[SpecializationSlot.Line_1].BuildSpecialization = Template.BuildTemplate.Specializations[SpecializationSlot.Line_1];
                _specializations[SpecializationSlot.Line_1].Profession = BuildsManager.Data.Professions[Template.BuildTemplate.Profession];
                _specializations[SpecializationSlot.Line_1].Template = Template;

                _specializations[SpecializationSlot.Line_2].BuildSpecialization = Template.BuildTemplate.Specializations[SpecializationSlot.Line_2];
                _specializations[SpecializationSlot.Line_2].Profession = BuildsManager.Data.Professions[Template.BuildTemplate.Profession];
                _specializations[SpecializationSlot.Line_2].Template = Template;

                _specializations[SpecializationSlot.Line_3].BuildSpecialization = Template.BuildTemplate.Specializations[SpecializationSlot.Line_3];
                _specializations[SpecializationSlot.Line_3].Profession = BuildsManager.Data.Professions[Template.BuildTemplate.Profession];
                _specializations[SpecializationSlot.Line_3].Template = Template;

                _raceIcon.Texture = GetRaceTexture(Template.Race);
                _raceIcon.BasicTooltipText = Template.Race.ToString();

                _specIcon.Texture = Template.EliteSpecialization != null ? Template.EliteSpecialization.ProfessionIconBig : BuildsManager.Data.Professions?[Template.Profession]?.IconBig;
                _specIcon.BasicTooltipText = Template.EliteSpecialization != null ? Template.EliteSpecialization.Name : BuildsManager.Data.Professions?[Template.Profession]?.Name;

                _skillbar.Template = Template;

                if (_professionSpecifics == null || _professionSpecifics.Profession != Template.Profession)
                {
                    CreateSpecifics();
                }

                if (_professionSpecifics != null)
                {
                    _professionSpecifics.Parent = _professionSpecificsContainer;
                    _professionSpecifics.Template = Template;
                    _professionSpecifics.Width = _professionSpecificsContainer.Width;
                    _professionSpecifics.Height = _professionSpecificsContainer.Height;
                }
            }
        }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (_buildCodeBox != null) _buildCodeBox.Width = Width - _buildCodeBox.Left;

            if (_specializationsPanel != null)
            {
                _professionSpecificsContainer.Width = _specializationsPanel.Width;

                _skillbar.Width = _specializationsPanel.Width - 10;
                _dummy.Width = _specializationsPanel.Width;
                _specsBackground.Bounds = new(0, _dummy.Bottom - 55, Width + 15, _dummy.Height + _specializationsPanel.Height + 34);
                _specsBackground.TextureRegion = new(0, 0, 650, 450);

                _specIcon.Location = new(_specializationsPanel.Width - _specIcon.Width - 8, _professionSpecificsContainer.Top + 11);

                _raceIcon.Location = new(_specializationsPanel.Width - _raceIcon.Width - _specIcon.Width - 8 - 10, _professionSpecificsContainer.Top + 11);

                _skillsBackground.Bounds = new(_specializationsPanel.Left, _specializationsPanel.Top, _specializationsPanel.Width, _professionSpecificsContainer.Height + _skillbar.Height + 10);
                _skillsBackground.TextureRegion = new(20, 20, _specializationsPanel.Width, _specializationsPanel.Height + _skillbar.Height);

                _skillsBackgroundTopBorder.Bounds = new(_professionSpecificsContainer.Left - 5, _professionSpecificsContainer.Top - 8, _professionSpecificsContainer.Width / 2, _professionSpecificsContainer.Height + _skillbar.Height + 8 + 10);
                _skillsBackgroundTopBorder.TextureRegion = new(35, 15, _professionSpecificsContainer.Width / 2, _professionSpecificsContainer.Height + _skillbar.Height);

                _skillsBackgroundBottomBorder.Bounds = new(_professionSpecificsContainer.Right - (_professionSpecificsContainer.Width / 2) + 5, _professionSpecificsContainer.Top, (_professionSpecificsContainer.Width / 2) + 16, _professionSpecificsContainer.Height + _skillbar.Height + 12 + 10);
                _skillsBackgroundBottomBorder.TextureRegion = new(108, 275, _professionSpecificsContainer.Width / 2, _professionSpecificsContainer.Height + _skillbar.Height);

                if (_professionSpecifics != null)
                {
                    _professionSpecifics.Width = _professionSpecificsContainer.Width;
                    _professionSpecifics.Height = _professionSpecificsContainer.Height;
                }
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);
            _skillsBackground?.Draw(this, spriteBatch);
            _skillsBackgroundBottomBorder?.Draw(this, spriteBatch);
            _skillsBackgroundTopBorder?.Draw(this, spriteBatch);

            //_specsBackground?.Draw(this, spriteBatch);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            _specializations.ToList().ForEach(l => l.Value.Dispose());
        }

        private void CreateSpecifics()
        {
            _professionSpecifics?.Dispose();

            switch (Template.Profession)
            {
                case ProfessionType.Guardian:
                    _professionSpecifics = new GuardianSpecifics();
                    break;
                case ProfessionType.Warrior:
                    _professionSpecifics = new WarriorSpecifics();
                    break;
                case ProfessionType.Engineer:
                    _professionSpecifics = new EngineerSpecifics();
                    break;
                case ProfessionType.Ranger:
                    _professionSpecifics = new RangerSpecifics();
                    break;
                case ProfessionType.Thief:
                    _professionSpecifics = new ThiefSpecifics();
                    break;
                case ProfessionType.Elementalist:
                    _professionSpecifics = new ElementalistSpecifics();
                    break;
                case ProfessionType.Mesmer:
                    _professionSpecifics = new MesmerSpecifics();
                    break;
                case ProfessionType.Necromancer:
                    _professionSpecifics = new NecromancerSpecifics();
                    break;
                case ProfessionType.Revenant:
                    _professionSpecifics = new RevenantSpecifics();
                    break;
            }
        }

        private Texture2D GetRaceTexture(Races race)
        {
            return race switch
            {
                Races.None => _texturesService.GetTexture(@"textures\races\pact.png", "pact"),
                Races.Asura => _texturesService.GetTexture(@"textures\races\asura.png", "asura"),
                Races.Charr => _texturesService.GetTexture(@"textures\races\charr.png", "charr"),
                Races.Human => _texturesService.GetTexture(@"textures\races\human.png", "human"),
                Races.Norn => _texturesService.GetTexture(@"textures\races\norn.png", "norn"),
                Races.Sylvari => _texturesService.GetTexture(@"textures\races\sylvari.png", "sylvari"),
                _ => _texturesService.GetTexture(@"textures\races\pact.png", "pact"),
            };
        }
    }
}
