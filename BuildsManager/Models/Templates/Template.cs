using Gw2Sharp.Models;
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
using Kenedia.Modules.BuildsManager.Extensions;
using Blish_HUD;

namespace Kenedia.Modules.BuildsManager.Models.Templates
{
    [DataContract]
    public class Template : IDisposable
    {
        private bool _pvE = true;
        private bool _disposed = false;
        private bool _triggerEvents = true;

        private Races _race = Races.None;
        private TemplateFlag _tags = TemplateFlag.None;
        private EncounterFlag _encounters = EncounterFlag.None;
        private ProfessionType _profession = ProfessionType.Guardian;

        private string _name = "New Template";
        private string _description;

        private CancellationTokenSource _cancellationTokenSource;
        private CancellationTokenSource _eventCancellationTokenSource;

        public Template()
        {
            BuildTemplate.ProfessionChanged += BuildTemplate_ProfessionChanged;
            GearTemplate.ProfessionChanged += GearTemplate_ProfessionChanged;

            BuildTemplate.BuildCodeChanged += BuildTemplate_BuildCodeChanged;
            GearTemplate.PropertyChanged += TemplateChanged;

            Profession = GameService.Gw2Mumble.PlayerCharacter?.Profession ?? Profession;
        }

        public event EventHandler GearChanged;

        public event EventHandler BuildChanged;

        public event EventHandler GearDisplayChanged;

        public event EventHandler BuildDisplayChanged;

        public event EventHandler LoadedGearFromCode;

        public event EventHandler LoadedBuildFromCode;

        public event ValueChangedEventHandler<Races> RaceChanged;

        public event ValueChangedEventHandler<ProfessionType> ProfessionChanged;

        public Template(string? buildCode, string? gearCode) : this()
        {
            if (!string.IsNullOrEmpty(buildCode)) BuildTemplate.LoadFromCode(buildCode);
            if (!string.IsNullOrEmpty(gearCode)) GearTemplate.LoadFromCode(gearCode);

            Profession = BuildTemplate?.Profession ?? Profession;

            Debug.WriteLine($"Profession {Profession}");
        }

        public ObservableList<string> TextTags { get; private set; } = new();

        public bool AutoSave { get; set; } = false;

        public string FilePath => @$"{BuildsManager.ModuleInstance.Paths.TemplatesPath}{Common.MakeValidFileName(Name.Trim())}.json";

        [DataMember]
        public TemplateFlag Tags { get => _tags; set => Common.SetProperty(ref _tags, value, TemplateChanged, _triggerEvents); }

        [DataMember]
        public EncounterFlag Encounters { get => _encounters; set => Common.SetProperty(ref _encounters, value, TemplateChanged, _triggerEvents); }

        [DataMember]
        public string Name { get => _name; set => Common.SetProperty(ref _name, value, TemplateChanged, _triggerEvents); }

        [DataMember]
        public string Description { get => _description; set => Common.SetProperty(ref _description, value, TemplateChanged, _triggerEvents); }

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
            set => Common.SetProperty(ref _profession, value, OnProfessionChanged);
        }

        [DataMember]
        public Races Race { get => _race; set => Common.SetProperty(ref _race, value, OnRaceChanged); }

        public string Gearcode
        {
            get => GearTemplate.ParseGearCode();
            set => GearTemplate?.LoadFromCode(value);
        }

        public string Buildcode
        {
            get => BuildTemplate?.ParseBuildCode();
            set => BuildTemplate?.LoadFromCode(value);
        }

        public Specialization EliteSpecialization => BuildTemplate?.Specializations[SpecializationSlot.Line_3]?.Specialization?.Elite == true ? BuildTemplate.Specializations[SpecializationSlot.Line_3].Specialization : null;

        public BuildTemplate BuildTemplate { get; } = new();

        public GearTemplate GearTemplate { get; } = new();

        public bool PvE { get => _pvE; internal set => Common.SetProperty(ref _pvE, value, ValueChanged); }

        private void OnProfessionChanged(object sender, Core.Models.ValueChangedEventArgs<ProfessionType> e)
        {
            if (BuildTemplate != null)
            {
                BuildTemplate.Profession = e.NewValue;
                GearTemplate.Profession = e.NewValue;
            }

            ProfessionChanged?.Invoke(this, e);
        }

        private void OnRaceChanged(object sender, Core.Models.ValueChangedEventArgs<Races> e)
        {
            RaceChanged?.Invoke(this, e);
        }

        private void GearTemplate_ProfessionChanged(object sender, Core.Models.ValueChangedEventArgs<ProfessionType> e)
        {
            Profession = GearTemplate?.Profession ?? Profession;
        }

        private void BuildTemplate_ProfessionChanged(object sender, Core.Models.ValueChangedEventArgs<ProfessionType> e)
        {
            Profession = BuildTemplate?.Profession ?? Profession;
        }

        private void ValueChanged<T>(object sender, Core.Models.ValueChangedEventArgs<T> e)
        {

        }

        private async void TemplateChanged(object sender, PropertyChangedEventArgs e)
        {
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
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await Task.Delay(1000, _cancellationTokenSource.Token);
                if (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    string path = BuildsManager.ModuleInstance.Paths.TemplatesPath;
                    if (!Directory.Exists(path)) _ = Directory.CreateDirectory(path);

                    string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                    File.WriteAllText($@"{path}\{Common.MakeValidFileName(Name.Trim())}.json", json);
                }
            }
            catch (Exception ex)
            {
                if (!_cancellationTokenSource.Token.IsCancellationRequested) BuildsManager.Logger.Warn(ex.ToString());
            }
        }

        public IEnumerable<T> GetSlotGroup<T>(TemplateSlot slot)
            where T : GearTemplateEntry
        {
            var slots =
                slot.IsArmor() ? GearTemplate?.Armors.Values.Cast<T>() :
                slot.IsWeapon() ? GearTemplate?.Weapons.Values.Cast<T>() :
                slot.IsJuwellery() ? GearTemplate?.Juwellery.Values.Cast<T>() : null;

            return slots;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            BuildTemplate?.Dispose();
            GearTemplate?.Dispose();
        }

        public async void PauseEvents(int ms = 500)
        {
            _triggerEvents = false;
            await Task.Delay(ms);
            _triggerEvents = true;
        }
    }
}
