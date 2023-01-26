using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Characters.Enums;
using Kenedia.Modules.Characters.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using static Kenedia.Modules.Characters.Services.Data;
using File = System.IO.File;

namespace Kenedia.Modules.Characters.Models
{
    [DataContract]
    public class Character_Model
    {
        private string _name;
        private int _level;
        private int _map = 0;
        private Gender _gender;
        private Gw2Sharp.Models.RaceType _race;
        private Gw2Sharp.Models.ProfessionType _profession;
        private SpecializationType _specialization;
        private DateTimeOffset _created;
        private DateTime _lastModified;
        private DateTime _lastLogin;
        private string _iconPath;
        private AsyncTexture2D _icon;
        private bool _show = true;
        private int _position;
        private int _index;
        private bool _initialized;
        private bool _pathChecked;

        public Character_Model()
        {
        }

        public event EventHandler Updated;

        public event EventHandler Deleted;

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
        public List<CharacterCrafting> Crafting { get; set; } = new List<CharacterCrafting>();

        public List<KeyValuePair<int, CrafingProfession>> CraftingDisciplines
        {
            get
            {
                var list = new List<KeyValuePair<int, CrafingProfession>>();
                foreach (CharacterCrafting crafting in Crafting)
                {
                    CrafingProfession craftingProf = Characters.ModuleInstance.Data.CrafingProfessions.Where(e => e.Value.Id == crafting.Id)?.FirstOrDefault().Value;

                    if (craftingProf != null)
                    {
                        list.Add(new(crafting.Rating, craftingProf));
                    }
                }

                return list;
            }
        }

        [DataMember]
        public Gw2Sharp.Models.RaceType Race
        {
            get => _race;
            set => SetProperty(ref _race, value);
        }

        [DataMember]
        public Gw2Sharp.Models.ProfessionType Profession
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
                OnUpdated();
            }
        }

        public string MapName => (Map != null && Characters.ModuleInstance.Data.Maps[Map] != null) ? Characters.ModuleInstance.Data.Maps[Map].Name : string.Empty;

        public string RaceName => Characters.ModuleInstance.Data.Races[Race].Name;

        public string ProfessionName => Characters.ModuleInstance.Data.Professions[Profession].Name;

        public string SpecializationName => Specialization != SpecializationType.None && Enum.IsDefined(typeof(SpecializationType), Specialization)
                    ? Characters.ModuleInstance.Data.Specializations[Specialization].Name
                    : ProfessionName;

        public AsyncTexture2D ProfessionIcon => Characters.ModuleInstance.Data.Professions[Profession].IconBig;

        public AsyncTexture2D SpecializationIcon => Specialization != SpecializationType.None && Enum.IsDefined(typeof(SpecializationType), Specialization)
                    ? Characters.ModuleInstance.Data.Specializations[Specialization].IconBig
                    : ProfessionIcon;

        public AsyncTexture2D Icon
        {
            get
            {
                if (!_pathChecked)
                {
                    string path = Characters.ModuleInstance.BasePath + (IconPath ?? string.Empty);

                    if (IconPath != null && File.Exists(path))
                    {
                        GameService.Graphics.QueueMainThreadRender((graphicsDevice) => _icon = TextureUtil.FromStreamPremultiplied(graphicsDevice, new FileStream(Characters.ModuleInstance.BasePath + IconPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)));
                    }

                    _pathChecked = true;
                }

                return _icon ?? SpecializationIcon;
            }

            set
            {
                _icon = value;
                Updated?.Invoke(this, null);
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
                Save();
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

        public bool HasBirthdayPresent
        {
            get
            {
                for (int i = 1; i < 100; i++)
                {
                    DateTime birthDay = Created.AddYears(i).DateTime;

                    if (birthDay <= DateTime.UtcNow)
                    {
                        if (birthDay > LastLogin)
                        {
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

        public void Delete()
        {
            Deleted?.Invoke(null, null);
            _ = Characters.ModuleInstance.CharacterModels.Remove(this);
            Save();
        }

        public void Initialize()
        {
            _initialized = true;
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

        public void AddTag(string tag)
        {
            Tags.Add(tag);
            OnUpdated();
        }

        public void RemoveTag(string tag)
        {
            if (Tags.Remove(tag)) OnUpdated();
        }

        public (string, int, int, int, bool) NameMatches(string name)
        {
            var distances = new List<(string, int, int, int, bool)>();

            foreach (Character_Model c in Characters.ModuleInstance.CharacterModels)
            {
                int distance = name.LevenshteinDistance(c.Name);
                int listDistance = c.Position.Difference(Position);

                distances.Add((c.Name, distance, listDistance, listDistance + distance, c.Name == Name));
            }

            distances.Sort((a, b) => a.Item4.CompareTo(b.Item4));
            (string, int, int, int, bool)? bestMatch = distances?.FirstOrDefault();

            if(bestMatch != null && bestMatch.HasValue)
            {
                foreach ((string, int, int, int, bool) match in distances.Where(e => e.Item4 == bestMatch.Value.Item4))
                {
                    if(match.Item1 == Name)
                    {
                        return match;
                    }
                }
            }

            return ((string, int, int, int, bool))bestMatch;
        }

        private void OnUpdated()
        {
            Updated?.Invoke(this, EventArgs.Empty);
            Save();
        }

        private void Save()
        {
            Characters.ModuleInstance.SaveCharacters = true;
        }
    }
}
