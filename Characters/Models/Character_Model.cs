using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Gw2Mumble;
using Gw2Sharp.Models;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Characters.Res;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using static Kenedia.Modules.Characters.Services.Data;
using Map = Kenedia.Modules.Core.DataModels.Map;
using File = System.IO.File;
using Kenedia.Modules.Core.DataModels;

namespace Kenedia.Modules.Characters.Models
{
    [DataContract]
    public class Character_Model
    {
        private readonly Action _requestSave;
        private readonly ObservableCollection<Character_Model> _characterModels;
        private readonly Data _data;
        private readonly CharacterSwapping _characterSwapping;

        private AsyncTexture2D _icon;
        private bool _initialized;
        private bool _pathChecked;

        private string _name;
        private int _level;
        private int _map = 0;

        private Gender _gender;
        private RaceType _race;
        private ProfessionType _profession;
        private SpecializationType _specialization;
        private DateTimeOffset _created;
        private DateTime _lastModified;
        private DateTime _lastLogin;
        private string _iconPath;
        private bool _show = true;
        private int _position;
        private int _index;
        private bool _showOnRadial = false;
        private bool _hadBirthday;
        private bool _isCurrentCharacter;
        private bool _markedAsDeleted;
        private DateTime _nextBirthday;
        private bool _beta;

        public Character_Model()
        {
        }

        public Character_Model(Character character, CharacterSwapping characterSwapping, string modulePath, Action requestSave, ObservableCollection<Character_Model> characterModels, Data data)
        {
            _characterSwapping = characterSwapping;
            ModulePath = modulePath;
            _requestSave = requestSave;
            _characterModels = characterModels;
            _data = data;

            _name = character.Name;
            _level = character.Level;
            //_beta = _beta || character.Flags.ToList().Contains(CharacterFlag.Beta);

            _race = (RaceType)Enum.Parse(typeof(RaceType), character.Race);
            _profession = (ProfessionType)Enum.Parse(typeof(ProfessionType), character.Profession);
            _specialization = SpecializationType.None;

            _created = character.Created;
            _lastModified = character.LastModified.UtcDateTime;
            _lastLogin = _lastLogin > character.LastModified.UtcDateTime ? _lastLogin : character.LastModified.UtcDateTime;
            _gender = character.Gender;

            foreach (CharacterCraftingDiscipline disc in character.Crafting.ToList())
            {
                Crafting.Add(new()
                {
                    Id = (int)disc.Discipline.Value,
                    Rating = disc.Rating,
                    Active = disc.Active,
                });
            }

            _initialized = true;
        }

        public Character_Model(Character_Model character, CharacterSwapping characterSwapping, string modulePath, Action requestSave, ObservableCollection<Character_Model> characterModels, Data data)
        {
            _characterSwapping = characterSwapping;
            ModulePath = modulePath;
            _requestSave = requestSave;
            _characterModels = characterModels;
            _data = data;

            _name = character.Name;
            _level = character.Level;
            _map = character.Map;
            _beta = character.Beta;
            Crafting = character.Crafting;
            _race = character.Race;
            _profession = character.Profession;
            _specialization = character.Specialization;
            _created = character.Created;
            _lastModified = character.LastModified;
            _lastLogin = character.LastLogin;
            _iconPath = character.IconPath;
            _show = character.Show;
            Tags = character.Tags;
            _position = character.Position;
            _index = character.Index;
            _gender = character.Gender;
            _showOnRadial = character.ShowOnRadial;
            _hadBirthday = character.HadBirthday;

            _initialized = true;
        }

        public event EventHandler Updated;

        public event EventHandler Deleted;

        public bool IsCurrentCharacter { get => _isCurrentCharacter; set => SetProperty(ref _isCurrentCharacter, value); }

        [DataMember]
        public bool Beta
        {
            get => _beta;
            set => SetProperty(ref _beta, value);
        }

        [DataMember]
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        [DataMember]
        public int Level
        {
            get => _level;
            set => SetProperty(ref _level, value);
        }

        [DataMember]
        public int Map
        {
            get => _map;
            set => SetProperty(ref _map, value);
        }

        [DataMember]
        public List<CharacterCrafting> Crafting { get; } = new List<CharacterCrafting>();

        public List<KeyValuePair<int, CraftingProfession>> CraftingDisciplines
        {
            get
            {
                var list = new List<KeyValuePair<int, CraftingProfession>>();
                if (_data is not null)
                {
                    foreach (CharacterCrafting crafting in Crafting)
                    {
                        CraftingProfession craftingProf = _data.CrafingProfessions.Where(e => e.Value.Id == crafting.Id)?.FirstOrDefault().Value;

                        if (craftingProf is not null)
                        {
                            list.Add(new(crafting.Rating, craftingProf));
                        }
                    }
                }

                return list;
            }
        }

        [DataMember]
        public RaceType Race
        {
            get => _race;
            set => SetProperty(ref _race, value);
        }

        [DataMember]
        public ProfessionType Profession
        {
            get => _profession;
            set => SetProperty(ref _profession, value);
        }

        [DataMember]
        public SpecializationType Specialization
        {
            get => _specialization;
            set => SetProperty(ref _specialization, value);
        }

        [DataMember]
        public DateTimeOffset Created
        {
            get => _created;
            set => SetProperty(ref _created, value);
        }

        [DataMember]
        public DateTime LastModified
        {
            get => _lastModified;
            set => SetProperty(ref _lastModified, value);
        }

        public int OrderIndex { get; set; } = 0;

        public int OrderOffset { get; set; } = 0;

        [DataMember]
        public DateTime LastLogin
        {
            get => _lastLogin.AddMilliseconds(-OrderOffset);
            set => SetProperty(ref _lastLogin, value);
        }

        [DataMember]
        public string IconPath
        {
            get => _iconPath;
            set
            {
                _iconPath = value;
                _icon = null;
                _pathChecked = false;
                OnUpdated(true);
            }
        }

        public string MapName => _data is not null && _data.Maps.TryGetValue(Map, out Map map) ? map.Name : string.Empty;

        public string RaceName => _data is not null && _data.Races.TryGetValue(Race, out Data.Race race) ? race.Name : "Unkown Race";

        public string ProfessionName => _data is not null && _data.Professions.TryGetValue(Profession, out Data.Profession profession) ? profession.Name : "Data not loaded.";

        public string SpecializationName => _data is not null && Specialization != SpecializationType.None && Enum.IsDefined(typeof(SpecializationType), Specialization) && _data.Specializations.TryGetValue(Specialization, out Data.Specialization specialization)
                    ? specialization.Name
                    : ProfessionName;

        public AsyncTexture2D SimpleProfessionIcon => _data is not null && _data.Professions.TryGetValue(Profession, out Data.Profession profession) ? profession.Icon : null;

        public AsyncTexture2D ProfessionIcon => _data is not null && _data.Professions.TryGetValue(Profession, out Data.Profession profession) ? profession.IconBig : null;

        public AsyncTexture2D SimpleSpecializationIcon => _data is not null && Specialization != SpecializationType.None && Enum.IsDefined(typeof(SpecializationType), Specialization) && _data.Specializations.TryGetValue(Specialization, out Data.Specialization specialization)
                    ? specialization.Icon.Texture
                    : SimpleProfessionIcon;

        public AsyncTexture2D SpecializationIcon => _data is not null && Specialization != SpecializationType.None && Enum.IsDefined(typeof(SpecializationType), Specialization) && _data.Specializations.TryGetValue(Specialization, out Data.Specialization specialization)
                    ? specialization.IconBig
                    : ProfessionIcon;

        public AsyncTexture2D Icon
        {
            get
            {
                if (!_pathChecked)
                {
                    string path = ModulePath + (IconPath ?? string.Empty);

                    if (IconPath is not null && File.Exists(path))
                    {
                        GameService.Graphics.QueueMainThreadRender((graphicsDevice) => _icon = TextureUtil.FromStreamPremultiplied(graphicsDevice, new FileStream(ModulePath + IconPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)));
                    }

                    _pathChecked = true;
                }

                return _icon ?? SpecializationIcon;
            }

            set
            {
                _icon = value;
                OnUpdated(false);
            }
        }

        public bool HasDefaultIcon => Icon == SpecializationIcon;

        [DataMember]
        public bool Show
        {
            get => _show;
            set => SetProperty(ref _show, value);
        }

        [DataMember]
        public bool MarkedAsDeleted
        {
            get => _markedAsDeleted;
            set => SetProperty(ref _markedAsDeleted, value);
        }

        [DataMember]
        public TagList Tags { get; private set; } = new TagList();

        [DataMember]
        public int Position
        {
            get => _position;
            set
            {
                _position = value;
                Save();
            }
        }

        [DataMember]
        public int Index
        {
            get => _index;
            set
            {
                _index = value;
                OnUpdated();
            }
        }

        [DataMember]
        public Gender Gender
        {
            get => _gender;
            set
            {
                _gender = value;
                Save();
            }
        }

        public int Age
        {
            get
            {
                // Save today's date.
                DateTimeOffset today = DateTimeOffset.UtcNow;

                // Calculate the age.
                int age = today.Year - Created.Year;

                // Go back to the year in which the person was born in case of a leap year
                if (Created.Date > today.AddYears(-age))
                {
                    age--;
                }

                return age;
            }
        }

        [DataMember]
        public bool HadBirthday
        {
            get => _hadBirthday;
            set => SetProperty(ref _hadBirthday, value);
        }

        public bool HasBirthdayPresent
        {
            get
            {
                if (HadBirthday) return true;

                for (int i = 1; i < 100; i++)
                {
                    DateTime birthDay = Created.AddDays(365 * i).DateTime;

                    if (birthDay <= DateTime.UtcNow)
                    {
                        if (birthDay > LastLogin)
                        {
                            HadBirthday = true;
                            return true;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }

                return false;
            }
        }

        public TimeSpan UntilNextBirthday => NextBirthday.Subtract(DateTime.UtcNow);

        public int SecondsUntilNextBirthday => (int)UntilNextBirthday.TotalSeconds;

        public DateTime NextBirthday
        {
            get
            {
                for (int i = 1; i < 100; i++)
                {
                    DateTime birthDay = Created.AddDays(365 * i).DateTime;

                    if((birthDay.Year == DateTime.UtcNow.Year && birthDay >= DateTime.UtcNow) || birthDay.Year == DateTime.UtcNow.Year + 1)
                    {
                        _nextBirthday = birthDay;
                        return _nextBirthday;
                    }
                }

                return _nextBirthday;
            }
        }

        public string ModulePath { get; private set; }

        [DataMember]
        public bool ShowOnRadial
        {
            get => _showOnRadial;
            set => SetProperty(ref _showOnRadial, value);
        }

        public int TimeSinceLogin => (int)DateTimeOffset.UtcNow.Subtract(LastLogin).TotalSeconds;

        public void Delete()
        {
            Deleted?.Invoke(null, null);
            _ = _characterModels.Remove(this);
            Save();
        }

        protected bool SetProperty<T>(ref T property, T newValue, [CallerMemberName] string caller = "")
        {
            if (Equals(property, newValue))
            {
                return false;
            }

            property = newValue;
            if (_initialized && caller != nameof(LastLogin) && caller != nameof(LastModified) && caller != nameof(LastLogin))
            {
                OnUpdated();
            }

            return true;
        }

        public void UpdateTags(TagList tags)
        {
            for (int i = Tags.Count - 1; i >= 0; i--)
            {
                if (!tags.Contains(Tags[i]))
                {
                    Tags.RemoveAt(i);
                }
            }

            OnUpdated();
        }

        public void AddTag(string tag, bool update = true)
        {
            if (!Tags.Contains(tag))
            {
                if (update) OnUpdated();
                Tags.Add(tag);
            }
        }

        public void RemoveTag(string tag)
        {
            if (Tags.Remove(tag)) OnUpdated();
        }

        public (string, int, int, int, bool) NameMatches(string name)
        {
            var distances = new List<(string, int, int, int, bool)>();

            foreach (Character_Model c in _characterModels)
            {
                int distance = name.LevenshteinDistance(c.Name);
                int listDistance = c.Position.Difference(Position);
                if (listDistance < 5)
                {
                    distances.Add((c.Name, distance, listDistance, listDistance + distance, c.Name == Name));
                }
            }

            distances.Sort((a, b) => a.Item4.CompareTo(b.Item4));
            (string, int, int, int, bool)? bestMatch = distances?.FirstOrDefault();

            if (bestMatch is not null && bestMatch.HasValue)
            {
                foreach ((string, int, int, int, bool) match in distances.Where(e => e.Item4 == bestMatch.Value.Item4))
                {
                    if (match.Item1 == Name)
                    {
                        return match;
                    }
                }
            }

            return ((string, int, int, int, bool))bestMatch;
        }

        public void Save()
        {
            if (_initialized)
            {
                _requestSave?.Invoke();
            }
        }

        public void Swap(bool ignoreOCR = false)
        {
            if (!GameService.Gw2Mumble.CurrentMap.IsPvpMap())
            {
                Save();                
                _characterSwapping?.Start(this, ignoreOCR);
                Characters.Logger.Info(string.Format(strings.CharacterSwap_SwitchTo, Name));
            }
            else
            {
                ScreenNotification.ShowNotification("[Characters]: " + strings.Error_Competivive, ScreenNotification.NotificationType.Error);
            }
        }

        public void UpdateCharacter(PlayerCharacter character = null)
        {
            character ??= GameService.Gw2Mumble.PlayerCharacter;
            
            if (character is not null && character.Name == Name)
            {
                Specialization = (SpecializationType)character.Specialization;
                Map = GameService.Gw2Mumble.CurrentMap.IsCommonMap() ? GameService.Gw2Mumble.CurrentMap.Id : Map;
                LastLogin = DateTime.UtcNow;
            }
        }

        public void UpdateCharacter(Character character)
        {
            _name = character.Name;
            _level = character.Level;

            //_beta = _beta || character.Flags.ToList().Contains(CharacterFlag.Beta);

            _race = (RaceType)Enum.Parse(typeof(RaceType), character.Race);
            _profession = (ProfessionType)Enum.Parse(typeof(ProfessionType), character.Profession);

            _created = character.Created;
            _lastModified = character.LastModified.UtcDateTime;
            _lastLogin = _lastLogin > character.LastModified.UtcDateTime ? _lastLogin : character.LastModified.UtcDateTime;
            _gender = character.Gender;

            foreach (CharacterCraftingDiscipline disc in character.Crafting.ToList())
            {
                CharacterCrafting craft = Crafting.Find(e => e.Id == (int)disc.Discipline.Value);
                bool craftFound = craft is not null;
                craft ??= new();
                craft.Id = (int)disc.Discipline.Value;
                craft.Rating = disc.Rating;
                craft.Active = disc.Active;

                if (!craftFound)
                {
                    Crafting.Add(craft);
                }
            }
        }

        public void SetIndex(int index)
        {
            _index = index;
        }

        private void OnUpdated(bool save = true)
        {
            Updated?.Invoke(this, EventArgs.Empty);
            if (save) Save();
        }
    }
}
