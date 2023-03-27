﻿using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Res;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Utility;
using Microsoft.Xna.Framework;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Linq;
using static Blish_HUD.ContentService;
using static Kenedia.Modules.Characters.Services.Settings;
using FlowPanel = Kenedia.Modules.Core.Controls.FlowPanel;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Gw2Sharp.WebApi;
using Kenedia.Modules.Core.Interfaces;
using Kenedia.Modules.Core.Services;
using System.Diagnostics;

namespace Kenedia.Modules.Characters.Controls
{
    public class CharacterLabels : IDisposable
    {
        private readonly IconLabel _nameLabel;
        private readonly IconLabel _levelLabel;
        private readonly IconLabel _professionLabel;
        private readonly IconLabel _raceLabel;
        private readonly IconLabel _genderLabel;
        private readonly IconLabel _mapLabel;
        private readonly CraftingControl _craftingControl;
        private readonly IconLabel _lastLoginLabel;
        private readonly IconLabel _nextBirthdayLabel;
        private readonly IconLabel _ageLabel;
        private readonly IconLabel _customIndex;
        private Character_Model _character;
        private readonly List<Tag> _tags = new();
        private readonly bool _created;

        private TextureManager _textureManager;
        private Data _data;
        private Settings _settings;

        public CharacterLabels(FlowPanel parent)
        {
            Parent = parent;

            _nameLabel = new IconLabel()
            {
                Parent = parent,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
                TextColor = Colors.ColonialWhite,
            };

            _levelLabel = new IconLabel()
            {
                Parent = parent,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
                Icon = AsyncTexture2D.FromAssetId(157085),
                TextureRectangle = new Rectangle(2, 2, 28, 28),
            };

            _genderLabel = new IconLabel()
            {
                Parent = parent,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };

            _raceLabel = new IconLabel()
            {
                Parent = parent,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };
            _professionLabel = new IconLabel()
            {
                Parent = parent,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };
            _mapLabel = new IconLabel()
            {
                Parent = parent,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
                Icon = AsyncTexture2D.FromAssetId(358406),
                TextureRectangle = new Rectangle(2, 2, 28, 28),
            };

            _craftingControl = new CraftingControl()
            {
                Parent = parent,
                Width = parent.Width,
                Height = 20,
                Character = Character,
            };

            _ageLabel = new IconLabel()
            {
                Parent = parent,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
                Icon = AsyncTexture2D.FromAssetId(1424243),
                TextureRectangle = new Rectangle(2, 2, 28, 28),
            };

            _nextBirthdayLabel = new IconLabel()
            {
                Parent = parent,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
                Icon = AsyncTexture2D.FromAssetId(593864),
                TextureRectangle = new Rectangle(2, 2, 28, 28),
            };

            _lastLoginLabel = new IconLabel()
            {
                Parent = parent,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
                Icon = AsyncTexture2D.FromAssetId(155035),
                TextureRectangle = new Rectangle(10, 10, 44, 44),
            };

            _customIndex = new IconLabel()
            {
                Parent = parent,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
                TextureRectangle = new Rectangle(2, 2, 28, 28),
                Icon = AsyncTexture2D.FromAssetId(156909),
            };

            TagPanel = new()
            {
                Parent = parent,
                Font = _lastLoginLabel.Font,
                FlowDirection = ControlFlowDirection.LeftToRight,
                ControlPadding = new Vector2(3, 2),
                HeightSizingMode = SizingMode.AutoSize,
                Visible = false,
            };

            DataControls = new()
            {
                _nameLabel,
                _customIndex,
                _levelLabel,
                _genderLabel,
                _raceLabel,
                _professionLabel,
                _mapLabel,
                _nextBirthdayLabel,
                _ageLabel,
                _lastLoginLabel,
                _craftingControl,
                TagPanel,
            };

            LocalizingService.LocaleChanged += UserLocale_SettingChanged;
            UserLocale_SettingChanged(null, null);
            UpdateDataControlsVisibility();

            _created = true;
        }

        public TagFlowPanel TagPanel { get; }

        public BitmapFont NameFont { get; set; } = GameService.Content.DefaultFont14;

        public BitmapFont Font { get; set; } = GameService.Content.DefaultFont14;

        public Character_Model Character
        {
            get => _character;
            set => Common.SetProperty(ref _character, value);
        }

        public List<Control> DataControls { get; } = new();

        public Data Data
        {
            get => _data;
            set => Common.SetProperty(ref _data, value, OnDataChanged);
        }

        public Settings Settings
        {
            get => _settings;
            set
            {
                var temp = _settings;
                if (Common.SetProperty(ref _settings, value, OnSettingsChanged))
                {
                    if (temp != null) { temp.AppearanceSettingChanged -= Settings_AppearanceSettingChanged; }
                    if (_settings != null) { _settings.AppearanceSettingChanged += Settings_AppearanceSettingChanged; }
                }
            }
        }

        private void Settings_AppearanceSettingChanged(object sender, EventArgs e)
        {
            UpdateDataControlsVisibility();
        }

        private void OnSettingsChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _craftingControl.Settings = _settings;

            if (_settings != null)
            {
                RecalculateBounds();
            }
        }

        private void OnDataChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _craftingControl.Data = _data;
        }

        public TextureManager TextureManager
        {
            get => _textureManager;
            set => Common.SetProperty(ref _textureManager, value, OnTextureManagerAdded);
        }

        public Func<Character_Model> CurrentCharacter { get; set; }

        public FlowPanel Parent { get; }

        private void OnTextureManagerAdded(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_genderLabel != null) _genderLabel.Icon = _textureManager?.GetIcon(TextureManager.Icons.Gender);
        }

        public void RecalculateBounds()
        {
            UpdateDataControlsVisibility();
            TagPanel.FitWidestTag(DataControls.Max(e => e.Visible && e != TagPanel ? e.Width : 0));
        }

        public void Dispose()
        {
            DataControls?.DisposeAll();
            LocalizingService.LocaleChanged -= UserLocale_SettingChanged;
        }

        public void UpdateCharacterInfo()
        {
            if (Character == null) return;

            _nameLabel.Text = Character.Name;

            _levelLabel.Text = string.Format(strings.LevelAmount, Character.Level);

            _professionLabel.Icon = Character.SpecializationIcon;
            _professionLabel.Text = Character.SpecializationName;

            if (_professionLabel.Icon != null)
            {
                _professionLabel.TextureRectangle = _professionLabel.Icon.Width == 32 ? new Rectangle(2, 2, 28, 28) : new Rectangle(4, 4, 56, 56);
            }

            _genderLabel.Text = Character.Gender.ToString();

            _raceLabel.Text = _data.Races[Character.Race].Name;
            _raceLabel.Icon = _data.Races[Character.Race].Icon;

            _mapLabel.Text = _data.GetMapById(Character.Map).Name;
            _customIndex.Text = string.Format(strings.CustomIndex + " {0}", Character.Index);

            var tagLlist = _tags.Select(e => e.Text);
            var characterTags = Character.Tags.ToList();

            var deleteTags = tagLlist.Except(characterTags);
            var addTags = characterTags.Except(tagLlist);

            bool tagChanged = deleteTags.Any() || addTags.Any();

            if (tagChanged)
            {
                var deleteList = new List<Tag>();
                foreach (string tag in deleteTags)
                {
                    var t = _tags.FirstOrDefault(e => e.Text == tag);
                    if (t != null) deleteList.Add(t);
                }

                foreach (var t in deleteList)
                {
                    t.Dispose();
                    _ = _tags.Remove(t);
                }

                foreach (string tag in addTags)
                {
                    _tags.Add(new Tag()
                    {
                        Parent = TagPanel,
                        Text = tag,
                        Active = true,
                        ShowDelete = false,
                        CanInteract = false,
                    });
                }

                TagPanel.FitWidestTag(DataControls.Max(e => e.Visible && e != TagPanel ? e.Width : 0));
            }

            _craftingControl.Character = Character;
        }

        public void UpdateDataControlsVisibility(bool tooltip = false)
        {
            if (_settings == null) return;
            var settings = _settings.DisplayToggles.Value;

            NameFont = GetFont(true);
            Font = GetFont();

            _nameLabel.Visible = !settings.TryGetValue("Name", out var name) || (tooltip ? name.ShowTooltip : name.Show);

            _nameLabel.Font = NameFont;

            _levelLabel.Visible = !settings.TryGetValue("Level", out var level) || (tooltip ? level.ShowTooltip : level.Show);
            _levelLabel.Font = Font;

            _genderLabel.Visible = !settings.TryGetValue("Gender", out var gender) || (tooltip ? gender.ShowTooltip : gender.Show);
            _genderLabel.Font = Font;

            _raceLabel.Visible = !settings.TryGetValue("Race", out var race) ||( tooltip ? race.ShowTooltip : race.Show);
            _raceLabel.Font = Font;

            _professionLabel.Visible = !settings.TryGetValue("Profession", out var profession) || (tooltip ? profession.ShowTooltip : profession.Show);
            _professionLabel.Font = Font;

            _lastLoginLabel.Visible = !settings.TryGetValue("LastLogin", out var lastlogin) || (tooltip ? lastlogin.ShowTooltip : lastlogin.Show);
            _lastLoginLabel.Font = Font;

            _ageLabel.Visible = !settings.TryGetValue("Age", out var age) || (tooltip ? age.ShowTooltip : age.Show);
            _ageLabel.Font = Font;

            _nextBirthdayLabel.Visible = !settings.TryGetValue("NextBirthday", out var nextbirthday) || (tooltip ? nextbirthday.ShowTooltip : nextbirthday.Show);
            _nextBirthdayLabel.Font = Font;

            _mapLabel.Visible = !settings.TryGetValue("Map", out var map) || (tooltip ? map.ShowTooltip : map.Show);
            _mapLabel.Font = Font;

            _craftingControl.Visible = !settings.TryGetValue("CraftingProfession", out var craftingprofession) || (tooltip ? craftingprofession.ShowTooltip : craftingprofession.Show);
            _craftingControl.Font = Font;

            _customIndex.Visible = !settings.TryGetValue("CustomIndex", out var customindex) || (tooltip ? customindex.ShowTooltip : customindex.Show);
            _customIndex.Font = Font;

            TagPanel.Visible = (!settings.TryGetValue("Tags", out var tags) || (tooltip ? tags.ShowTooltip : tags.Show)) && (Character?.Tags.Count ?? 0) > 0;
            TagPanel.Font = Font;

            _craftingControl.Height = Font.LineHeight + 2;

            if (Parent != null)
            {
                Parent.ControlPadding = new(Font.LineHeight / 10, Font.LineHeight / 10);
                Parent?.Invalidate();
            }
        }

        private BitmapFont GetFont(bool nameFont = false)
        {
            FontSize fontSize = FontSize.Size8;
            if (_settings == null) return GameService.Content.DefaultFont12;

            switch (_settings.PanelSize.Value)
            {
                case PanelSizes.Small:
                    fontSize = nameFont ? FontSize.Size16 : FontSize.Size12;
                    break;

                case PanelSizes.Normal:
                    fontSize = nameFont ? FontSize.Size18 : FontSize.Size14;
                    break;

                case PanelSizes.Large:
                    fontSize = nameFont ? FontSize.Size22 : FontSize.Size18;
                    break;

                case PanelSizes.Custom:
                    fontSize = nameFont ? (FontSize)_settings.CustomCharacterNameFontSize.Value : (FontSize)_settings.CustomCharacterFontSize.Value;
                    break;
            }

            return GameService.Content.GetFont(FontFace.Menomonia, fontSize, FontStyle.Regular);
        }

        internal void Update()
        {
            if (Character != null && _lastLoginLabel.Visible)
            {
                if (CurrentCharacter?.Invoke() != Character)
                {
                    TimeSpan ts = DateTimeOffset.UtcNow.Subtract(Character.LastLogin);
                    _lastLoginLabel.Text = string.Format("{1} {0} {2:00}:{3:00}:{4:00}", strings.Days, Math.Floor(ts.TotalDays), ts.Hours, ts.Minutes, ts.Seconds);
                }
                else
                {
                    _lastLoginLabel.Text = string.Format("{1} {0} {2:00}:{3:00}:{4:00}", strings.Days, 0, 0, 0, 0);
                }
            }

            if (Character != null && _nextBirthdayLabel.Visible)
            {
                TimeSpan ts = Character.UntilNextBirthday;
                _nextBirthdayLabel.Text = string.Format("{1} {0} {2:00}:{3:00}:{4:00}", strings.Days, Math.Floor(ts.TotalDays), ts.Hours, ts.Minutes, ts.Seconds);
            }

            if (Character != null && _ageLabel.Visible)
            {
                _ageLabel.Text = string.Format("{1} ({0} Years)", Character.Age, Character.Created.Date.ToString("d"));
            }

            if (_created && Parent?.Visible == true)
            {
                UpdateCharacterInfo();
            }
        }

        void UserLocale_SettingChanged(object sender, ValueChangedEventArgs<Locale> e)
        {
            UpdateCharacterInfo();
        }
    }
}
