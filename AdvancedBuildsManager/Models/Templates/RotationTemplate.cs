using Blish_HUD.Content;
using Kenedia.Modules.AdvancedBuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.Utility;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Kenedia.Modules.AdvancedBuildsManager.Models.Templates
{
    public class RotationElement : INotifyPropertyChanged
    {
        private Skill _skill;
        private int _repetition = 1;

        public Skill Skill
        {
            get => _skill;
            set => Common.SetProperty(ref _skill, value, PropertyChanged);
        }

        public int Repetition
        {
            get => _repetition; set
            {
                if (value > 0)
                {
                    _ = Common.SetProperty(ref _repetition, value, PropertyChanged);
                }
            }
        }

        public string DisplayText => Skill?.Name + (Repetition > 1 ? " x " + Repetition.ToString() : string.Empty);

        public AsyncTexture2D Icon => Skill?.Icon;

        public event PropertyChangedEventHandler PropertyChanged;
    }

    [DataContract]
    public class Rotation : ObservableCollection<RotationElement>, INotifyPropertyChanged
    {
        private string _name = "Rotation Name";

        public Rotation()
        {

        }

        public Rotation(string code)
        {
            LoadFromCode(code);
        }

        public Rotation(string name, string code)
        {
            _name = name;
            LoadFromCode(code);
        }

        public event PropertyChangedEventHandler NameChanged;
        public new event PropertyChangedEventHandler PropertyChanged;

        [DataMember]
        public string Name { get => _name; set => Common.SetProperty(ref _name, value, NameChanged); }

        [DataMember]
        public string RotationCode => GenerateRotationCode();

        private string GenerateRotationCode()
        {
            string code = "";
            foreach (var item in this)
            {
                if (item.Skill is not null) code += "[" + item.Repetition + "|" + item.Skill.Id + "]";
            }

            return code.IsNullOrEmpty() ? "" : code.Substring(0, code.Length);
        }

        public void Delete()
        {

        }

        public void RemoveSkill(RotationElement element)
        {
            _ = Remove(element);
        }

        public RotationElement AddSkill()
        {
            RotationElement element = new();
            Add(element);

            element.PropertyChanged += Element_PropertyChanged;
            OnPropertyChanged(new(nameof(Rotation)));

            return element;
        }

        private void Element_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(new(nameof(Rotation)));
            PropertyChanged?.Invoke(sender, e);
        }

        public void LoadFromCode(string code)
        {
            foreach (string substring in code.Split(']'))
            {
                if (substring.Length > 0)
                {
                    string[] info = substring.Remove(0, 1).Split('|');

                    if (info.Length > 1)
                    {
                        RotationElement element = new()
                        {
                            Skill = int.TryParse(info[1], out int skillid) ? AdvancedBuildsManager.Data.GetSkillById(skillid) : null,
                            Repetition = int.TryParse(info[0], out int repetition) ? repetition : 1,
                        };

                        element.PropertyChanged += Element_PropertyChanged;
                        Add(element);
                    }
                }
            }
        }
    }

    [DataContract]
    public class RotationTemplate : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [DataMember]
        public ObservableCollection<Rotation> Rotations { get; set; } = new();

        public RotationTemplate()
        {
            Rotations.CollectionChanged += Rotations_CollectionChanged;
        }

        private void Rotations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Rotation item in e.NewItems)
                    {
                        item.NameChanged += RotationTemplate_PropertyChanged;
                    }

                    break;

                case NotifyCollectionChangedAction.Replace:
                    foreach (Rotation item in e.OldItems)
                    {
                        item.NameChanged -= RotationTemplate_PropertyChanged;
                    }

                    foreach (Rotation item in e.NewItems)
                    {
                        item.NameChanged += RotationTemplate_PropertyChanged;
                    }

                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (Rotation item in e.OldItems)
                    {
                        item.NameChanged -= RotationTemplate_PropertyChanged;
                    }

                    break;
            }

            PropertyChanged?.Invoke(sender, new(nameof(Rotations)));
        }

        private void RotationTemplate_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }

        public void LoadFromCode(string rotationCode)
        {

        }

        public Rotation AddRotation()
        {
            var rotation = new Rotation();
            Rotations.Add(rotation);
            rotation.CollectionChanged += RotationChanged;
            rotation.PropertyChanged += RotationChanged;

            return rotation;
        }

        public void RegisterEvents()
        {
            foreach(Rotation rotation in Rotations)
            {
                rotation.CollectionChanged += RotationChanged;
                rotation.PropertyChanged += RotationChanged;
            }
        }

        private void RotationChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, new(nameof(Rotations)));
        }

        private void RotationChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, new(nameof(Rotations)));
        }

        public void RemoveRotation(Rotation rotation)
        {
            _ = Rotations.Remove(rotation);
        }

        public void Dispose()
        {
            Rotations.CollectionChanged -= Rotations_CollectionChanged;
        }
    }
}