using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Gw2Sharp.Models;
using Kenedia.Modules.BuildsManager.Controls.ProfessionSpecific;
using Kenedia.Modules.BuildsManager.Controls.Selection;
using Kenedia.Modules.BuildsManager.DataModels;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.Models.Templates;
using Kenedia.Modules.BuildsManager.Services;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using FlowPanel = Kenedia.Modules.Core.Controls.FlowPanel;
using Panel = Kenedia.Modules.Core.Controls.Panel;
using TextBox = Kenedia.Modules.Core.Controls.TextBox;

namespace Kenedia.Modules.BuildsManager.Controls.Tabs
{
    public class BuildTab : Blish_HUD.Controls.Container
    {
        private readonly DetailedTexture _specsBackground = new(993592);
        private readonly DetailedTexture _skillsBackground = new(155960);
        private readonly DetailedTexture _skillsBackgroundBottomBorder = new(155987);
        private readonly DetailedTexture _skillsBackgroundTopBorder = new(155989);

        private readonly ProfessionRaceSelection _professionRaceSelection;
        private readonly FlowPanel _specializationsPanel;

        public SpecLine SpecLine1 { get; }
        public SpecLine SpecLine2 { get; }
        public SpecLine SpecLine3 { get; }

        private readonly Panel _professionSpecificsContainer;

        private readonly SkillsBar _skillbar;
        private readonly Dummy _dummy;

        private readonly ButtonImage _specIcon;
        private readonly ButtonImage _raceIcon;
        private readonly TextBox _buildCodeBox;
        private readonly ImageButton _copyButton;

        private ProfessionSpecifics _professionSpecifics;

        public BuildTab(TemplatePresenter templatePresenter, Data data)
        {
            TemplatePresenter = templatePresenter;
            Data = data;

            ClipsBounds = false;
            //WidthSizingMode = SizingMode.Fill;
            HeightSizingMode = SizingMode.Fill;

            _copyButton = new()
            {
                Parent = this,
                Texture = AsyncTexture2D.FromAssetId(2208345),
                HoveredTexture = AsyncTexture2D.FromAssetId(2208347),
                Size = new(26),
                ClickAction = async (m) =>
                {
                    try
                    {
                        if (_buildCodeBox.Text is string s && !string.IsNullOrEmpty(s))
                        {
                            _ = await ClipboardUtil.WindowsClipboardService.SetTextAsync(s);
                        }
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
                Location = new(0, _buildCodeBox.Bottom + 15),
                Width = 500,
                Height = 100,
                ZIndex = 13,
            };

            _skillbar = new(templatePresenter, data)
            {
                Parent = this,
                Location = new(5, _professionSpecificsContainer.Bottom + 10),
                Width = 500,
                ZIndex = 12,
            };

            _specIcon = new()
            {
                Parent = this,
                Size = new(40),
                ZIndex = 15,
            };

            _raceIcon = new()
            {
                Parent = this,
                TextureSize = new Point(32),
                ZIndex = 15,
                Texture = TexturesService.GetTextureFromRef(@"textures\races\none.png", "none"),
                Size = new(40),
            };

            _dummy = new Dummy()
            {
                Parent = this,
                Location = new(0, _skillbar.Bottom + 20),
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

            SpecLine1 = new SpecLine(SpecializationSlotType.Line_1, TemplatePresenter, Data) { Parent = _specializationsPanel };

            SpecLine2 = new SpecLine(SpecializationSlotType.Line_2, TemplatePresenter, Data) { Parent = _specializationsPanel };

            SpecLine3 = new SpecLine(SpecializationSlotType.Line_3, TemplatePresenter, Data) { Parent = _specializationsPanel };

            _professionRaceSelection = new()
            {
                Parent = this,
                Visible = false,
                ClipsBounds = false,
                ZIndex = 16,
            };

            SetSelectionTextures();

            TemplatePresenter.TemplateChanged += TemplatePresenter_TemplateChanged;
            TemplatePresenter.BuildCodeChanged += TemplatePresenter_BuildCodeChanged;
            TemplatePresenter.ProfessionChanged += TemplatePresenter_ProfessionChanged;
            TemplatePresenter.RaceChanged += TemplatePresenter_RaceChanged;
            TemplatePresenter.EliteSpecializationChanged += TemplatePresenter_EliteSpecializationChanged;
        }

        public TemplatePresenter TemplatePresenter { get; }

        public Data Data { get; }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            if (_buildCodeBox is not null) _buildCodeBox.Width = Width - _buildCodeBox.Left;

            if (_specializationsPanel is not null)
            {
                _professionSpecificsContainer.Width = _specializationsPanel.Width;

                _skillbar.Width = _specializationsPanel.Width - 10;
                _dummy.Width = _specializationsPanel.Width;
                _specsBackground.Bounds = new(0, _dummy.Bottom - 55, Width + 15, _dummy.Height + _specializationsPanel.Height + 34);
                _specsBackground.TextureRegion = new(0, 0, 650, 450);

                _specIcon.Location = new(_specializationsPanel.Width - _specIcon.Width - 8, _professionSpecificsContainer.Top + 8);

                _raceIcon.Location = new(_specializationsPanel.Width - _raceIcon.Width - 8, _specIcon.Bottom + 4);

                _skillsBackground.Bounds = new(0, 40, Width, 210);
                _skillsBackground.TextureRegion = new(20, 20, Width - 100, 210);

                _skillsBackgroundTopBorder.Bounds = new(_professionSpecificsContainer.Left - 5, 20, _professionSpecificsContainer.Width / 2, 22);
                _skillsBackgroundTopBorder.TextureRegion = new(35, 5, _professionSpecificsContainer.Width / 2, 22);

                _skillsBackgroundBottomBorder.Bounds = new(0, _skillsBackground.Bounds.Bottom - 4, Width, 22);
                _skillsBackgroundBottomBorder.TextureRegion = new Rectangle(0, _skillsBackgroundBottomBorder.Texture.Height - 26, _skillsBackgroundBottomBorder.Texture.Width - 36, 22);

                if (_professionSpecifics is not null)
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

            foreach (var c in Children)
            {
                c?.Dispose();
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if (_specIcon?.MouseOver == true)
            {
                _professionRaceSelection.Visible = true;
                _professionRaceSelection.Type = ProfessionRaceSelection.SelectionType.Profession;
                _professionRaceSelection.Location = RelativeMousePosition;
                _professionRaceSelection.OnClickAction = (value) => TemplatePresenter?.Template?.SetProfession((ProfessionType)value);
            }

            if (_raceIcon?.MouseOver == true)
            {
                _professionRaceSelection.Visible = true;
                _professionRaceSelection.Type = ProfessionRaceSelection.SelectionType.Race;
                _professionRaceSelection.Location = RelativeMousePosition;
                _professionRaceSelection.OnClickAction = (value) => TemplatePresenter.Template?.SetRace((Races)value);
            }
        }

        private void TemplatePresenter_BuildCodeChanged(object sender, EventArgs e)
        {
            _buildCodeBox.Text = TemplatePresenter?.Template?.ParseBuildCode();
        }

        private void TemplatePresenter_TemplateChanged(object sender, Core.Models.ValueChangedEventArgs<Template> e)
        {
            SetSelectionTextures();
            SetProfessionSpecifics();

            _buildCodeBox.Text = TemplatePresenter?.Template?.ParseBuildCode();
        }

        private void TemplatePresenter_EliteSpecializationChanged(object sender, SpecializationChangedEventArgs e)
        {
            SetSpecIcons();
            SetProfessionSpecifics();
        }

        private void TemplatePresenter_RaceChanged(object sender, Core.Models.ValueChangedEventArgs<Races> e)
        {
            SetRaceIcons();
        }

        private void TemplatePresenter_ProfessionChanged(object sender, Core.Models.ValueChangedEventArgs<ProfessionType> e)
        {
            SetSpecIcons();
            SetProfessionSpecifics();
        }

        private void SetProfessionSpecifics()
        {
            _professionSpecifics?.Dispose();

            if (TemplatePresenter?.Template is not null)
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

                if (_professionSpecifics is not null)
                {
                    _professionSpecifics.Parent = _professionSpecificsContainer;
                    _professionSpecifics.Width = _professionSpecificsContainer?.Width ?? 0;
                    _professionSpecifics.Height = _professionSpecificsContainer?.Height ?? 0;
                }
            }
        }

        private void SetSelectionTextures()
        {
            SetSpecIcons();
            SetRaceIcons();
        }

        private void SetRaceIcons()
        {
            _raceIcon.Texture = Data.Races?.TryGetValue(TemplatePresenter?.Template?.Race ?? Races.None, out Race raceIcon) == true ? raceIcon.Icon : null;
            _raceIcon.BasicTooltipText = Data.Races?.TryGetValue(TemplatePresenter?.Template?.Race ?? Races.None, out Race raceName) == true ? raceName.Name : null;
        }

        private void SetSpecIcons()
        {
            _specIcon.Texture = TemplatePresenter?.Template?.EliteSpecialization?.ProfessionIconBig ?? (Data.Professions?.TryGetValue(TemplatePresenter?.Template?.Profession ?? ProfessionType.Guardian, out Profession professionForIcon) == true ? professionForIcon.IconBig : null);
            _specIcon.BasicTooltipText = TemplatePresenter?.Template?.EliteSpecialization?.Name ?? (Data.Professions?.TryGetValue(TemplatePresenter?.Template?.Profession ?? ProfessionType.Guardian, out Profession professionForName) == true ? professionForName.Name : null);
        }
    }
}
