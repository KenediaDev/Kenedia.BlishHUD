﻿using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Gw2Sharp.Models;
using Kenedia.Modules.BuildsManager.Controls.BuildPage.ProfessionSpecific;
using Kenedia.Modules.BuildsManager.Controls.Selection;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FlowPanel = Kenedia.Modules.Core.Controls.FlowPanel;
using Panel = Kenedia.Modules.Core.Controls.Panel;
using TextBox = Kenedia.Modules.Core.Controls.TextBox;

namespace Kenedia.Modules.BuildsManager.Controls.BuildPage
{
    public class BuildPage : Container
    {
        private readonly DetailedTexture _specsBackground = new(993592);
        private readonly DetailedTexture _skillsBackground = new(155960);
        private readonly DetailedTexture _skillsBackgroundBottomBorder = new(155987);
        private readonly DetailedTexture _skillsBackgroundTopBorder = new(155989);

        private readonly ProfessionRaceSelection _professionRaceSelection;
        private readonly FlowPanel _specializationsPanel;
        private readonly Panel _professionSpecificsContainer;
        private readonly SkillsBar _skillbar;
        private readonly Dummy _dummy;
        private readonly Dictionary<SpecializationSlot, SpecLine> _specializations;
        private readonly FramedImage _specIcon;
        private readonly FramedImage _raceIcon;
        private readonly TexturesService _texturesService;
        private readonly TextBox _buildCodeBox;
        private readonly ImageButton _copyButton;

        private ProfessionSpecifics _professionSpecifics;

        public BuildPage(TexturesService texturesService, TemplatePresenter _templatePresenter)
        {
            _texturesService = texturesService;
            TemplatePresenter = _templatePresenter;

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
                EnterPressedAction = (txt) => TemplatePresenter.Template?.LoadBuildFromCode(txt, true)
            };

            _professionSpecificsContainer = new()
            {
                Parent = this,
                Location = new(0, _buildCodeBox.Bottom + 7),
                Width = 500,
                Height = 100,
                ZIndex = 13,
            };

            _skillbar = new SkillsBar(TemplatePresenter)
            {
                Parent = this,
                Location = new(5, _professionSpecificsContainer.Bottom),
                Width = Width,
                ZIndex = 12,
            };

            _specIcon = new FramedImage()
            {
                Parent = this,
                Size = new(40),
                ZIndex = 15,
            };

            _raceIcon = new FramedImage()
            {
                Parent = this,
                TextureSize = new Point(32),
                ZIndex = 15,
                Texture = _texturesService.GetTexture(@"textures\races\pact.png", "pact"),
                Size = new(40),
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

            _specializations = new()
            {
                {SpecializationSlot.Line_1,  new SpecLine(SpecializationSlot.Line_1, TemplatePresenter) { Parent = _specializationsPanel, CanInteract = () => !_skillbar.IsSelecting } },
                {SpecializationSlot.Line_2,  new SpecLine(SpecializationSlot.Line_2, TemplatePresenter) { Parent = _specializationsPanel, CanInteract = () => !_skillbar.IsSelecting } },
                {SpecializationSlot.Line_3,  new SpecLine(SpecializationSlot.Line_3, TemplatePresenter) { Parent = _specializationsPanel, CanInteract = () => !_skillbar.IsSelecting } },
            };

            _professionRaceSelection = new()
            {
                Parent = this,
                Visible = false,
                ClipsBounds = false,
                ZIndex = 16,
            };

            TemplatePresenter.BuildCodeChanged += TemplatePresenter_BuildCodeChanged;
            TemplatePresenter.ProfessionChanged += BuildTemplate_ProfessionChanged;
            TemplatePresenter.RaceChanged += TemplatePresenter_RaceChanged;
            TemplatePresenter.EliteSpecializationChanged += BuildTemplate_EliteSpecChanged;
            TemplatePresenter.LoadedBuildFromCode += BuildTemplate_Loaded;
            TemplatePresenter.TemplateChanged += TemplatePresenter_TemplateChanged;

            SetRaceAndProfessionAndSpec();
        }

        public TemplatePresenter TemplatePresenter { get; private set; }

        private void TemplatePresenter_RaceChanged(object sender, Core.Models.ValueChangedEventArgs<Races> e)
        {
            SetRaceAndProfessionAndSpec();
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

                _specIcon.Location = new(_specializationsPanel.Width - _specIcon.Width - 8, _professionSpecificsContainer.Top + 8);

                _raceIcon.Location = new(_specializationsPanel.Width - _raceIcon.Width - 8, _specIcon.Bottom + 4);

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
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            _specializations.ToList().ForEach(l => l.Value.Dispose());

            TemplatePresenter.BuildCodeChanged -= TemplatePresenter_BuildCodeChanged;
            TemplatePresenter.ProfessionChanged -= BuildTemplate_ProfessionChanged;
            TemplatePresenter.EliteSpecializationChanged -= BuildTemplate_EliteSpecChanged;
            TemplatePresenter.LoadedBuildFromCode -= BuildTemplate_Loaded;
            TemplatePresenter.TemplateChanged -= TemplatePresenter_TemplateChanged;
            TemplatePresenter = null;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if (_specIcon?.MouseOver == true)
            {
                _professionRaceSelection.Visible = true;
                _professionRaceSelection.Type = ProfessionRaceSelection.SelectionType.Profession;
                _professionRaceSelection.Location = RelativeMousePosition;
                _professionRaceSelection.OnClickAction = (value) => TemplatePresenter?.SetProfession((ProfessionType)value);
            }

            if (_raceIcon?.MouseOver == true)
            {
                _professionRaceSelection.Visible = true;
                _professionRaceSelection.Type = ProfessionRaceSelection.SelectionType.Race;
                _professionRaceSelection.Location = RelativeMousePosition;
                _professionRaceSelection.OnClickAction = (value) => TemplatePresenter.SetRace((Races)value);
            }
        }

        private void TemplatePresenter_BuildCodeChanged(object sender, EventArgs e)
        {
            _buildCodeBox.Text = TemplatePresenter.Template?.ParseBuildCode();
        }

        private void BuildTemplate_Loaded(object sender, EventArgs e)
        {
            SetRaceAndProfessionAndSpec();
        }

        private void TemplatePresenter_TemplateChanged(object sender, Core.Models.ValueChangedEventArgs<VTemplate> e)
        {
            _buildCodeBox.Text = TemplatePresenter.Template?.ParseBuildCode();
            SetRaceAndProfessionAndSpec();
        }

        private void BuildTemplate_ProfessionChanged(object sender, EventArgs e)
        {
            SetRaceAndProfessionAndSpec();
        }

        private void BuildTemplate_EliteSpecChanged(object sender, EventArgs e)
        {
            SetRaceAndProfessionAndSpec();
        }

        private void SetRaceAndProfessionAndSpec()
        {
            _specIcon.Texture = TemplatePresenter.Template?.EliteSpecialization != null ? TemplatePresenter.Template.EliteSpecialization.ProfessionIconBig : BuildsManager.Data.Professions?[TemplatePresenter.Template?.Profession ?? ProfessionType.Guardian]?.IconBig;
            _specIcon.BasicTooltipText = TemplatePresenter.Template?.EliteSpecialization != null ? TemplatePresenter.Template.EliteSpecialization.Name : BuildsManager.Data.Professions?[TemplatePresenter.Template?.Profession ?? ProfessionType.Guardian]?.Name;

            _raceIcon.Texture = BuildsManager.Data.Races[TemplatePresenter.Template?.Race ?? Races.None].Icon;
            _raceIcon.BasicTooltipText = BuildsManager.Data.Races[TemplatePresenter.Template?.Race ?? Races.None].Name;

            _professionSpecifics?.Dispose();

            if (TemplatePresenter?.Template != null)
            {
                switch (TemplatePresenter?.Template.Profession)
                {
                    case ProfessionType.Guardian:
                        _professionSpecifics = new GuardianSpecifics(TemplatePresenter);
                        break;
                    case ProfessionType.Warrior:
                        _professionSpecifics = new WarriorSpecifics(TemplatePresenter);
                        break;
                    case ProfessionType.Engineer:
                        _professionSpecifics = new EngineerSpecifics(TemplatePresenter);
                        break;
                    case ProfessionType.Ranger:
                        _professionSpecifics = new RangerSpecifics(TemplatePresenter);
                        break;
                    case ProfessionType.Thief:
                        _professionSpecifics = new ThiefSpecifics(TemplatePresenter);
                        break;
                    case ProfessionType.Elementalist:
                        _professionSpecifics = new ElementalistSpecifics(TemplatePresenter);
                        break;
                    case ProfessionType.Mesmer:
                        _professionSpecifics = new MesmerSpecifics(TemplatePresenter);
                        break;
                    case ProfessionType.Necromancer:
                        _professionSpecifics = new NecromancerSpecifics(TemplatePresenter);
                        break;
                    case ProfessionType.Revenant:
                        _professionSpecifics = new RevenantSpecifics(TemplatePresenter);
                        break;
                }

                if (_professionSpecifics != null)
                {
                    _professionSpecifics.Parent = _professionSpecificsContainer;
                    _professionSpecifics.Width = _professionSpecificsContainer?.Width ?? 0;
                    _professionSpecifics.Height = _professionSpecificsContainer?.Height ?? 0;
                }
            }
        }
    }
}
