using Gw2Sharp.WebApi.V2.Models;
using System;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Threading;
using Blish_HUD.Content;
using Blish_HUD;
using System.Runtime.Serialization;
using Kenedia.Modules.Core.DataModels;
using System.Diagnostics;
using DirectoryExtension = Kenedia.Modules.Core.Extensions.DirectoryExtension;
using Newtonsoft.Json.Linq;

namespace Kenedia.Modules.FashionManager.Models
{

    public class FashionTemplateConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(FashionTemplate);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Load the JSON for the Result into a JObject
            var jo = JObject.Load(reader);

            // Read the properties which will be used as constructor parameters
            string? name = (string?)jo["Name"];
            string? code = (string?)jo["FashionCode"];
            string? thumbnail = (string?)jo["ThumbnailFileName"];
            int? race = (int?)jo["Race"];
            int? gender = (int?)jo["Gender"];

            var galleryPaths = jo["GalleryPaths"]?.ToObject<List<string>>();

            // Construct the Result object using the non-default constructor
            var result = new FashionTemplate(name, thumbnail, code, (Races)race, (Gender)gender, galleryPaths);

            // (If anything else needs to be populated on the result object, do that here)

            // Return the result
            return result;
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    [DataContract]
    public class FashionTemplate
    {
        private bool _triggerEvents = true;

        private AsyncTexture2D _thumbnail;
        private string _fashionCode;
        private string _name;
        private string _thumbnailFileName;
        private CancellationTokenSource _cancellationTokenSource;
        private SkinBack _back;
        private Races _races = Races.None;
        private Gender _gender = Gender.Unknown;
        private List<AsyncTexture2D> _gallery;

        public FashionTemplate()
        {

        }

        [JsonConstructor]
        public FashionTemplate(string name, string thumbnail, string templateCode, Races? race, Gender? gender, List<string> galleryPaths)
        {
            _name = name;
            _thumbnailFileName = thumbnail;
            _fashionCode = templateCode;
            _races = race ?? _races;
            _gender = gender ?? _gender;
            GalleryPaths = galleryPaths;
        }

        public event EventHandler<EventArgs> Loaded;
        public event EventHandler<FashionTemplateArmorChanged> ArmorChanged;
        public event EventHandler<FashionTemplateGatheringToolChanged> GatheringToolChanged;
        public event EventHandler<FashionTemplateWeaponChanged> WeaponChanged;
        public event EventHandler<FashionTemplateMountChanged> MountChanged;
        public event EventHandler<Core.Models.ValueChangedEventArgs<SkinBack>> BackChanged;

        public string FileName => Common.MakeValidFileName(Name?.Trim());

        public string DirectoryPath => $@"{FashionManager.ModuleInstance.Paths.TemplatesPath}{FileName}\";

        public string FilePath => $@"{DirectoryPath}{FileName}.json";

        [DataMember]
        public string Name { get => _name; set => Common.SetProperty(ref _name, value, OnNameChanged); }

        [DataMember]
        public string ThumbnailFileName { get => _thumbnailFileName; set => Common.SetProperty(ref _thumbnailFileName, value, SetThumbnail); }

        [DataMember]
        public Races Race { get => _races; set => Common.SetProperty(ref _races, value, OnRaceChanged); }

        [DataMember]
        public Gender Gender { get => _gender; set => Common.SetProperty(ref _gender, value, OnGenderChanged); }

        [DataMember]
        public string FashionCode { get => ParseFashionCode(); set => Common.SetProperty(ref _fashionCode, value); }

        public AsyncTexture2D Thumbnail
        {
            get
            {
                if (_thumbnail is null && System.IO.File.Exists($"{DirectoryPath}{ThumbnailFileName}"))
                {
                    GameService.Graphics.QueueMainThreadRender((graphicsDevice) => _thumbnail = TextureUtil.FromStreamPremultiplied(graphicsDevice, new FileStream(DirectoryPath + ThumbnailFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)));
                }

                return _thumbnail;
            }
        }

        [DataMember]
        public List<string> GalleryPaths { get; set; } = new();

        public List<AsyncTexture2D> Gallery
        {
            get
            {
                if (_gallery is null)
                {
                    _gallery = new List<AsyncTexture2D>();

                    foreach (string fileName in GalleryPaths)
                    {
                        if (System.IO.File.Exists($"{DirectoryPath}{fileName}"))
                            GameService.Graphics.QueueMainThreadRender((graphicsDevice) => _gallery.Add(TextureUtil.FromStreamPremultiplied(graphicsDevice, new FileStream(DirectoryPath + fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))));
                    }
                }

                return _gallery;
            }
        }

        public Dictionary<ArmorSlotType, SkinArmor> Armors { get; } = new()
        {
            {ArmorSlotType.AquaBreather, null },
            {ArmorSlotType.Head, null },
            {ArmorSlotType.Shoulders, null },
            {ArmorSlotType.Chest, null },
            {ArmorSlotType.Hands, null },
            {ArmorSlotType.Legs, null },
            {ArmorSlotType.Feet, null },
        };

        public SkinArmor this[ArmorSlotType slot]
        {
            get => Armors[slot];
            set => Armors[slot] = value;
        }

        public Dictionary<GatheringSlotType, SkinGathering> GatheringTools { get; } = new()
        {
            {GatheringSlotType.Sickle, null },
            {GatheringSlotType.Axe, null },
            {GatheringSlotType.Pick, null },
        };

        public SkinGathering this[GatheringSlotType slot]
        {
            get => GatheringTools[slot];
            set => GatheringTools[slot] = value;
        }

        public Dictionary<WeaponSlotType, SkinWeapon> Weapons { get; } = new()
        {
            {WeaponSlotType.MainHand, null },
            {WeaponSlotType.OffHand, null },
            {WeaponSlotType.AlternateMainHand, null },
            {WeaponSlotType.AlternateOffHand, null },
            {WeaponSlotType.Aquatic, null },
            {WeaponSlotType.AlternateAquatic, null },
        };

        public SkinWeapon this[WeaponSlotType slot]
        {
            get => Weapons[slot];
            set => Weapons[slot] = value;
        }

        public Dictionary<MountSlotType, MountSkin> Mounts { get; } = new()
        {
            {MountSlotType.Raptor, null },
            {MountSlotType.Springer, null },
            {MountSlotType.Skimmer, null },
            {MountSlotType.Jackal, null },
            {MountSlotType.RollerBeetle, null },
            {MountSlotType.Warclaw, null },
            {MountSlotType.Griffon, null },
            {MountSlotType.Skyscale, null },
            {MountSlotType.SiegeTurtle, null },
        };

        public MountSkin this[MountSlotType mount]
        {
            get => Mounts[mount];
            set => Mounts[mount] = value;
        }

        public SkinBack Back { get => _back; set => Common.SetProperty(ref _back, value, OnBackChanged); }

        private void OnBackChanged(object sender, Core.Models.ValueChangedEventArgs<SkinBack> e)
        {
            if (_triggerEvents)
            {
                BackChanged?.Invoke(this, e);
                Save();
            }
        }

        #region Armor

        public SkinArmor AquaBreather
        {
            get => Armors[ArmorSlotType.AquaBreather];
            set => SetDictionaryItem(Armors, ArmorSlotType.AquaBreather, value);
        }

        public SkinArmor Head
        {
            get => Armors[ArmorSlotType.Head];
            set => SetDictionaryItem(Armors, ArmorSlotType.Head, value);
        }

        public SkinArmor Shoulders
        {
            get => Armors[ArmorSlotType.Shoulders];
            set => SetDictionaryItem(Armors, ArmorSlotType.Shoulders, value);
        }

        public SkinArmor Chest
        {
            get => Armors[ArmorSlotType.Chest];
            set => SetDictionaryItem(Armors, ArmorSlotType.Chest, value);
        }

        public SkinArmor Hands
        {
            get => Armors[ArmorSlotType.Hands];
            set => SetDictionaryItem(Armors, ArmorSlotType.Hands, value);
        }

        public SkinArmor Legs
        {
            get => Armors[ArmorSlotType.Legs];
            set => SetDictionaryItem(Armors, ArmorSlotType.Legs, value);
        }

        public SkinArmor Feet
        {
            get => Armors[ArmorSlotType.Feet];
            set => SetDictionaryItem(Armors, ArmorSlotType.Feet, value);
        }

        private void SetDictionaryItem(Dictionary<ArmorSlotType, SkinArmor> armors, ArmorSlotType slot, SkinArmor value)
        {
            armors[slot] = value;

            if (_triggerEvents)
            {
                ArmorChanged?.Invoke(this, new(slot, value));
                Save();
            }
        }
        #endregion

        #region Gathering

        public SkinGathering Sickle
        {
            get => GatheringTools[GatheringSlotType.Sickle];
            set => SetDictionaryItem(GatheringTools, GatheringSlotType.Sickle, value);
        }

        public SkinGathering Axe
        {
            get => GatheringTools[GatheringSlotType.Axe];
            set => SetDictionaryItem(GatheringTools, GatheringSlotType.Axe, value);
        }

        public SkinGathering Pick
        {
            get => GatheringTools[GatheringSlotType.Pick];
            set => SetDictionaryItem(GatheringTools, GatheringSlotType.Pick, value);
        }

        private void SetDictionaryItem(Dictionary<GatheringSlotType, SkinGathering> gatheringTools, GatheringSlotType slot, SkinGathering value)
        {
            gatheringTools[slot] = value;

            if (_triggerEvents)
            {
                GatheringToolChanged?.Invoke(this, new(slot, value));
                Save();
            }
        }
        #endregion

        #region Weapons

        public SkinWeapon MainHand
        {
            get => Weapons[WeaponSlotType.MainHand];
            set => SetDictionaryItem(Weapons, WeaponSlotType.MainHand, value);
        }

        public SkinWeapon OffHand
        {
            get => Weapons[WeaponSlotType.OffHand];
            set => SetDictionaryItem(Weapons, WeaponSlotType.OffHand, value);
        }

        public SkinWeapon AlternateMainHand
        {
            get => Weapons[WeaponSlotType.AlternateMainHand];
            set => SetDictionaryItem(Weapons, WeaponSlotType.AlternateMainHand, value);
        }

        public SkinWeapon AlternateOffHand
        {
            get => Weapons[WeaponSlotType.AlternateOffHand];
            set => SetDictionaryItem(Weapons, WeaponSlotType.AlternateOffHand, value);
        }

        public SkinWeapon Aquatic
        {
            get => Weapons[WeaponSlotType.Aquatic];
            set => SetDictionaryItem(Weapons, WeaponSlotType.Aquatic, value);
        }

        public SkinWeapon AlternateAquatic
        {
            get => Weapons[WeaponSlotType.AlternateAquatic];
            set => SetDictionaryItem(Weapons, WeaponSlotType.AlternateAquatic, value);
        }

        private void SetDictionaryItem(Dictionary<WeaponSlotType, SkinWeapon> weapons, WeaponSlotType slot, SkinWeapon value)
        {
            weapons[slot] = value;

            if (_triggerEvents)
            {
                WeaponChanged?.Invoke(this, new(slot, value));
                Save();
            }
        }
        #endregion

        #region Mounts

        public MountSkin Raptor
        {
            get => Mounts[MountSlotType.Raptor];
            set => SetDictionaryItem(Mounts, MountSlotType.Raptor, value);
        }

        public MountSkin Springer
        {
            get => Mounts[MountSlotType.Springer];
            set => SetDictionaryItem(Mounts, MountSlotType.Springer, value);
        }

        public MountSkin Skimmer
        {
            get => Mounts[MountSlotType.Skimmer];
            set => SetDictionaryItem(Mounts, MountSlotType.Skimmer, value);
        }

        public MountSkin Jackal
        {
            get => Mounts[MountSlotType.Jackal];
            set => SetDictionaryItem(Mounts, MountSlotType.Jackal, value);
        }

        public MountSkin Griffon
        {
            get => Mounts[MountSlotType.Griffon];
            set => SetDictionaryItem(Mounts, MountSlotType.Griffon, value);
        }

        public MountSkin RollerBeetle
        {
            get => Mounts[MountSlotType.RollerBeetle];
            set => SetDictionaryItem(Mounts, MountSlotType.RollerBeetle, value);
        }

        public MountSkin Skyscale
        {
            get => Mounts[MountSlotType.Skyscale];
            set => SetDictionaryItem(Mounts, MountSlotType.Skyscale, value);
        }

        public MountSkin Warclaw
        {
            get => Mounts[MountSlotType.Warclaw];
            set => SetDictionaryItem(Mounts, MountSlotType.Warclaw, value);
        }

        public MountSkin SiegeTurtle
        {
            get => Mounts[MountSlotType.SiegeTurtle];
            set => SetDictionaryItem(Mounts, MountSlotType.SiegeTurtle, value);
        }

        private void SetDictionaryItem(Dictionary<MountSlotType, MountSkin> mounts, MountSlotType slot, MountSkin value)
        {
            mounts[slot] = value;

            if (_triggerEvents)
            {
                MountChanged?.Invoke(this, new(slot, value));
                Save();
            }
        }
        #endregion

        private void OnNameChanged(object sender, Core.Models.ValueChangedEventArgs<string> e)
        {
            Debug.WriteLine($"Name changed from: {e.OldValue} to {e.NewValue}");

            Debug.WriteLine($"DirectoryPath {DirectoryPath}");
            if (!string.IsNullOrEmpty(e.OldValue) && !string.IsNullOrEmpty(e.NewValue))
            {
                string oldFileName = Common.MakeValidFileName(e.OldValue.Trim());
                string newFileName = Common.MakeValidFileName(e.NewValue.Trim());
                string oldPath = $@"{FashionManager.ModuleInstance.Paths.TemplatesPath}{oldFileName}";
                string newPath = $@"{FashionManager.ModuleInstance.Paths.TemplatesPath}{newFileName}";

                if (Directory.Exists(oldPath))
                {
                    System.IO.File.Delete($@"{oldPath}\{oldFileName}.json");

                    if (!Directory.Exists(newPath))
                    {
                        Directory.Move(oldPath, newPath);
                    }
                    else
                    {
                        DirectoryExtension.MoveFiles(oldPath, newPath);
                        Directory.Delete(oldPath, true);
                    }
                }
            }

            if (_triggerEvents)
            {
                Save();
            }
        }

        private void SetThumbnail(object sender, Core.Models.ValueChangedEventArgs<string> e)
        {
            _thumbnail = null;

            if (_triggerEvents)
            {
                Save();
            }
        }

        private void OnRaceChanged(object sender, Core.Models.ValueChangedEventArgs<Races> e)
        {

            Debug.WriteLine($"Race changed from {e.OldValue} to {e.NewValue}");
            if (_triggerEvents)
            {
                Save();
            }
        }

        private void OnGenderChanged(object sender, Core.Models.ValueChangedEventArgs<Gender> e)
        {
            if (_triggerEvents)
            {
                Save();
            }
        }

        public string ParseFashionCode()
        {
            var ids = new List<short>()
            {
                (short) (Back?.Id ?? 0),
            };

            ids.AddRange(Armors.Values.Select(s => (short)(s?.Id ?? 0)).ToList());
            ids.AddRange(Weapons.Values.Select(s => (short)(s?.Id ?? 0)).ToList());
            ids.AddRange(GatheringTools.Values.Select(s => (short)(s?.Id ?? 0)).ToList());
            ids.AddRange(Mounts.Values.Select(s => (short)(s?.Id ?? 0)).ToList());

            short[] shorts = ids.ToArray();
            for (int i = 0; i < shorts.Length; i++)
            {
                shorts[i] = short.MaxValue;
            }

            byte[] byteArray = new byte[shorts.Length * 2];
            for (int i = 0; i < shorts.Length; i++)
            {
                byte[] shortBytes = BitConverter.GetBytes(shorts[i]);
                Array.Copy(shortBytes, 0, byteArray, i * 2, 2);
            }

            return $"[&{Convert.ToBase64String(byteArray)}]";
        }

        public void Save(int timeToWait = 500)
        {
            _ = Task.Run(async () =>
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();

                try
                {
                    await Task.Delay(timeToWait, _cancellationTokenSource.Token);
                    if (!_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        string fileName = Common.MakeValidFileName(Name.Trim());
                        string path = $@"{FashionManager.ModuleInstance.Paths.TemplatesPath}\{fileName}";
                        if (!Directory.Exists(path)) _ = Directory.CreateDirectory(path);

                        string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                        System.IO.File.WriteAllText($@"{path}\{fileName}.json", json);
                    }
                }
                catch (Exception ex)
                {
                }
            });
        }
    }
}
