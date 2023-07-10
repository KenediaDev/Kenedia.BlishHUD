using Gw2Sharp.Models;
using AttributeType = Gw2Sharp.WebApi.V2.Models.AttributeType;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.BuildsManager.Extensions;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    [DataContract]
    public class Template : INotifyPropertyChanged
    {
        private ProfessionType _profession;
        private string _description;
        private string _name = "New Template";
        private bool _loaded = true;

        public Template()
        {
            BuildTemplate.BuildCodeChanged += BuildTemplate_BuildCodeChanged;
            GearTemplate.PropertyChanged += TemplateChanged;
        }

        public Template(string? buildCode, string? gearCode) : this()
        {
            if (!string.IsNullOrEmpty(buildCode)) BuildTemplate.LoadFromCode(buildCode);
            if (!string.IsNullOrEmpty(gearCode)) GearTemplate.LoadFromCode(gearCode);
        }

        public event ValueChangedEventHandler<Races> RaceChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableList<string> TextTags { get; private set; } = new();

        public bool AutoSave { get; set; } = false;

        [DataMember]
        public TemplateFlag Tags { get => _tags; set => Common.SetProperty(ref _tags, value, TemplateChanged); }

        [DataMember]
        public EncounterFlag Encounters { get => _encounters; set => Common.SetProperty(ref _encounters, value, TemplateChanged); }

        [DataMember]
        public string Name { get => _name; set => Common.SetProperty(ref _name, value, TemplateChanged); }

        [DataMember]
        public string Description { get => _description; set => Common.SetProperty(ref _description, value, TemplateChanged); }

        [DataMember]
        public string GearCode
        {
            get => GearTemplate?.ParseGearCode();
            set => Gearcode = value;
        }

        [DataMember]
        public string BuildCode
        {
            get => BuildTemplate?.ParseBuildCode();
            set => Buildcode = value;
        }

        public ProfessionType Profession
        {
            get => BuildTemplate.Profession;
            set
            {
                if (Common.SetProperty(ref _profession, value, TemplateChanged))
                {
                    if (BuildTemplate != null)
                    {
                        BuildTemplate.Profession = value;
                        GearTemplate.Profession = value;
                    }
                }
            }
        }

        [DataMember]
        public Races Race { get => _race; set => Common.SetProperty(ref _race, value, OnRaceChanged); }

        private void OnRaceChanged(object sender, ValueChangedEventArgs<Races> e)
        {
            RaceChanged?.Invoke(this, e);
        }

        private bool _pvE = true;
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationTokenSource _eventCancellationTokenSource;
        private TemplateFlag _tags;
        private EncounterFlag _encounters;
        private Races _race = Races.None;

        private string Gearcode
        {
            set
            {
                _loaded = false;
                GearTemplate?.LoadFromCode(value);
                _loaded = true;
            }
        }

        private string Buildcode
        {
            set
            {
                _loaded = false;
                BuildTemplate?.LoadFromCode(value);
                _loaded = true;
            }
        }

        public Specialization EliteSpecialization => BuildTemplate?.Specializations[SpecializationSlot.Line_3]?.Specialization?.Elite == true ? BuildTemplate.Specializations[SpecializationSlot.Line_3].Specialization : null;

        /// <summary>
        /// Active Transform Skill which sets weapon skills to its childs and disables all others
        /// </summary>
        public Skill ActiveTransform { get; set; }

        /// <summary>
        /// Active Bundle Skill which sets weapon skills to its childs
        /// </summary>
        public Skill ActiveBundle { get; set; }

        public BuildTemplate BuildTemplate { get; } = new();

        public GearTemplate GearTemplate { get; } = new();

        public RotationTemplate RotationTemplate { get; } = new();

        public bool PvE { get => _pvE; internal set => Common.SetProperty(ref _pvE, value, TemplateChanged); }

        public string FilePath => @$"{BuildsManager.ModuleInstance.Paths.TemplatesPath}{Common.MakeValidFileName(Name.Trim())}.json";

        [DataMember]
        public Dictionary<string, string> RotationCodes { get; set; } = new();

        private async void TemplateChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);

            if (AutoSave)
            {
                await Save();
            }
        }

        private async void BuildTemplate_BuildCodeChanged(object sender, EventArgs e)
        {
            if (AutoSave)
            {
                await Save();
            }
        }

        private async Task TriggerChanged(object sender, PropertyChangedEventArgs e)
        {
            _eventCancellationTokenSource?.Cancel();
            _eventCancellationTokenSource = new();

            try
            {
                await Task.Delay(1, _eventCancellationTokenSource.Token);
                if (!_eventCancellationTokenSource.IsCancellationRequested)
                {
                    PropertyChanged?.Invoke(sender, e);
                }
            }
            catch (Exception)
            {

            }
        }

        public async Task<bool> ChangeName(string name)
        {
            string path = FilePath;
            bool unlocked = await FileExtension.WaitForFileUnlock(path);

            if (!unlocked)
            {
                return false;
            }

            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch (Exception ex)
            {
                BuildsManager.Logger.Warn(ex.ToString());
            }

            Name = name;

            await Save();
            return true;
        }

        public async Task<bool> Delete()
        {
            bool unlocked = await FileExtension.WaitForFileUnlock(FilePath);

            if (!unlocked)
            {
                return false;
            }

            try
            {
                if (File.Exists(FilePath)) File.Delete(FilePath);
            }
            catch (Exception ex)
            {
                BuildsManager.Logger.Warn(ex.ToString());
            }

            return true;
        }

        public async Task Save()
        {
            if (!_loaded) return;

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await Task.Delay(1000, _cancellationTokenSource.Token);
                if (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    string path = BuildsManager.ModuleInstance.Paths.TemplatesPath;
                    if (!Directory.Exists(path)) _ = Directory.CreateDirectory(path);

                    RotationCodes.Clear();
                    foreach (var rotation in RotationTemplate.Rotations)
                    {
                        RotationCodes.Add(rotation.Name, rotation.RotationCode);
                    }

                    string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                    File.WriteAllText($@"{path}\{Common.MakeValidFileName(Name.Trim())}.json", json);
                }
            }
            catch (Exception ex)
            {
                if (!_cancellationTokenSource.Token.IsCancellationRequested) BuildsManager.Logger.Warn(ex.ToString());
            }
        }

        public IEnumerable<T> GetSlotGroup<T>(GearTemplateSlot slot)
            where T : GearTemplateEntry
        {
            var slots =
                slot.IsArmor() ? GearTemplate?.Armors.Values.Cast<T>() :
                slot.IsWeapon() ? GearTemplate?.Weapons.Values.Cast<T>() :
                slot.IsJuwellery() ? GearTemplate?.Juwellery.Values.Cast<T>() : null;

            return slots;
        }
    }
}
