using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.Models;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Services;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static Kenedia.Modules.Characters.Services.StaticHosting;
using Color = Microsoft.Xna.Framework.Color;
using CraftingDisciplineType = Gw2Sharp.WebApi.V2.Models.CraftingDisciplineType;
using Locale = Gw2Sharp.WebApi.Locale;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Services
{
    public enum ArmorWeight
    {
        Heavy,
        Medium,
        Light,
    }

    public class Data : IDisposable
    {
        const int MAX_LOADING_ATTEMPTS = 5;

        private bool _isDisposed;
        private DateTime _lastLoadingTry = DateTime.MinValue;
        private int _loadingAttempts = 0;

        public Data(ContentsManager contentsManager, PathCollection paths, Gw2ApiManager gw2ApiManager, StaticHosting staticHosting)
        {
            ContentsManager = contentsManager;
            Paths = paths;
            Gw2ApiManager = gw2ApiManager;
            StaticHosting = staticHosting;

            CraftingProfessions = new(Path.Combine(paths.ModuleDataPath, "crafting_disciplines.json"))
            {
                {
                    CraftingDisciplineType.Unknown, new()
                    {
                        Id = CraftingDisciplineType.Unknown,
                        IconAssetId = 154983,
                        MaxRating = 0,
                        Names = new()
                        {
                            { Locale.German, "Unbekannt" },
                            { Locale.English, "Unknown" },
                            { Locale.Spanish, "Desconocido" },
                            { Locale.French, "Inconnu" },
                        },
                    }
                 },

                {
                    CraftingDisciplineType.Artificer, new()
                    {
                        Id = CraftingDisciplineType.Artificer,
                        IconAssetId = 102463,
                        MaxRating = 500,
                        Names = new()
                        {
                            { Locale.German, "Konstrukteur" },
                            { Locale.English, "Artificer" },
                            { Locale.Spanish, "Artificiero" },
                            { Locale.French, "Artificier" },
                        },
                    }
                 },

                {
                    CraftingDisciplineType.Armorsmith, new()
                    {
                        Id = CraftingDisciplineType.Armorsmith,
                        IconAssetId = 102461,
                        MaxRating = 500,
                        Names = new()
                        {
                            { Locale.German, "Rüstungsschmied" },
                            { Locale.English, "Armorsmith" },
                            { Locale.Spanish, "Forjador de armaduras" },
                            { Locale.French, "Forgeron d'armures" },
                        },
                    }
                 },

                {
                    CraftingDisciplineType.Chef, new()
                    {
                        Id = CraftingDisciplineType.Chef,
                        IconAssetId = 102465,
                        MaxRating = 500,
                        Names = new()
                        {
                            { Locale.German, "Chefkoch" },
                            { Locale.English, "Chef" },
                            { Locale.Spanish, "Cocinero" },
                            { Locale.French, "Maître-queux" },
                        },
                    }
                 },

                {
                    CraftingDisciplineType.Jeweler, new()
                    {
                        Id = CraftingDisciplineType.Jeweler,
                        IconAssetId = 102458,
                        MaxRating = 400,
                        Names = new()
                        {
                            { Locale.German, "Juwelier" },
                            { Locale.English, "Jeweler" },
                            { Locale.Spanish, "Joyero" },
                            { Locale.French, "Bijoutier" },
                        },
                    }
                    },

                {
                    CraftingDisciplineType.Huntsman, new()
                    {
                        Id = CraftingDisciplineType.Huntsman,
                        IconAssetId = 102462,
                        MaxRating = 500,
                        Names = new()
                        {
                            { Locale.German, "Waidmann" },
                            { Locale.English, "Huntsman" },
                            { Locale.Spanish, "Cazador" },
                            { Locale.French, "Chasseur" },
                        },
                    }
                 },
                {
                    CraftingDisciplineType.Leatherworker, new()
                    {
                        Id = CraftingDisciplineType.Leatherworker,
                        IconAssetId = 102464,
                        MaxRating = 500,
                        Names = new()
                        {
                            { Locale.German, "Lederer" },
                            { Locale.English, "Leatherworker" },
                            { Locale.Spanish, "Peletero" },
                            { Locale.French, "Travailleur du cuir" },
                        },
                    }
                 },
                {
                    CraftingDisciplineType.Scribe, new()
                    {
                        Id = CraftingDisciplineType.Scribe,
                        IconAssetId = 1293677,
                        MaxRating = 400,
                        Names = new()
                        {
                            { Locale.German, "Schreiber" },
                            { Locale.English, "Scribe" },
                            { Locale.Spanish, "Escriba" },
                            { Locale.French, "Illustrateur" },
                        },
                    }
                 },
                {
                    CraftingDisciplineType.Tailor, new()
                    {
                        Id = CraftingDisciplineType.Tailor,
                        IconAssetId = 102459,
                        MaxRating = 500,
                        Names = new()
                        {
                            { Locale.German, "Schneider" },
                            { Locale.English, "Tailor" },
                            { Locale.Spanish, "Sastre" },
                            { Locale.French, "Tailleur" },
                        },
                    }
                 },
                {
                    CraftingDisciplineType.Weaponsmith, new()
                    {
                        Id = CraftingDisciplineType.Weaponsmith,
                        IconAssetId = 102460,
                        MaxRating = 500,
                        Names = new()
                        {
                            { Locale.German, "Waffenschmied" },
                            { Locale.English, "Weaponsmith" },
                            { Locale.Spanish, "Armero" },
                            { Locale.French, "Forgeron d'armes" },
                        },
                    }
                },
            };
            Races = new(Path.Combine(paths.ModuleDataPath, "races.json"))
            {
                {
                    Core.DataModels.Races.None,
                    new Race()
                    {
                        Id = Core.DataModels.Races.None,
                        Names = new()
                        {
                            { Locale.German, "Unbekannt" },
                            { Locale.English, "Unknown" },
                            { Locale.Spanish, "Desconocido" },
                            { Locale.French, "Inconnu" },
                        },
                    }
                },
                {
                    Core.DataModels.Races.Asura,
                    new()
                    {
                        Id = Core.DataModels.Races.Asura,
                        Names = new()
                        {
                            { Locale.German, "Asura" },
                            { Locale.English, "Asura" },
                            { Locale.Spanish, "Asura" },
                            { Locale.French, "Asura" },
                        },
                    }
                },
                {
                    Core.DataModels.Races.Charr,
                    new()
                    {
                        Id = Core.DataModels.Races.Charr,
                        Names = new()
                        {
                            { Locale.German, "Charr" },
                            { Locale.English, "Charr" },
                            { Locale.Spanish, "Charr" },
                            { Locale.French, "Charr" },
                        },
                    }
                },

                {
                    Core.DataModels.Races.Human,
                    new()
                    {
                        Id = Core.DataModels.Races.Human,
                        Names = new()
                        {
                            { Locale.German, "Mensch" },
                            { Locale.English, "Human" },
                            { Locale.Spanish, "Humano" },
                            { Locale.French, "Humain" },
                        },
                    }
                },

                {
                    Core.DataModels.Races.Norn,
                    new()
                    {
                        Id = Core.DataModels.Races.Norn,
                        Names = new()
                        {
                            { Locale.German, "Norn" },
                            { Locale.English, "Norn" },
                            { Locale.Spanish, "Norn" },
                            { Locale.French, "Norn" },
                        },
                    }
                },

                {
                    Core.DataModels.Races.Sylvari,
                    new()
                    {
                        Id = Core.DataModels.Races.Sylvari,
                        Names = new()
                        {
                            { Locale.German, "Sylvari" },
                            { Locale.English, "Sylvari" },
                            { Locale.Spanish, "Sylvari" },
                            { Locale.French, "Sylvaris" },
                        },
                    }
                },
            };

            Maps = new(Path.Combine(paths.ModuleDataPath, "maps.json"), UpdateMaps);
            Professions = new(Path.Combine(paths.ModuleDataPath, "professions.json"), UpdateProfessions);
            Specializations = new(Path.Combine(paths.ModuleDataPath, "specializations.json"), UpdateSpecializations);
        }

        public event EventHandler<bool> Loaded;
        public event EventHandler<bool> BetaStateChanged;

        public bool IsLoaded { get; private set; }

        public bool StaticContentLoaded { get; private set; }

        public StaticInfo StaticInfo { get; set; }

        public ContentsManager ContentsManager { get; }

        public PathCollection Paths { get; }

        public Gw2ApiManager Gw2ApiManager { get; }

        public StaticHosting StaticHosting { get; }

        public DataDictionary<int, Map> Maps { get; private set; }

        public DataDictionary<CraftingDisciplineType, CraftingProfession> CraftingProfessions { get; }

        public DataDictionary<Races, Race> Races { get; }

        public DataDictionary<int, Specialization> Specializations { get; }

        public DataDictionary<ProfessionType, Profession> Professions { get; }

        public Map GetMapById(int id)
        {
            return Maps.ContainsKey(id) ? Maps[id] : new Map() { Name = "Unknown Map", Id = 0 };
        }

        private async Task UpdateProfessions()
        {
            var professions = await Gw2ApiManager.Gw2ApiClient.V2.Professions.AllAsync();
            foreach (var prof in professions)
            {
                if (Enum.TryParse(prof.Id, out ProfessionType professionType))
                {
                    if (!Professions.ContainsKey(professionType))
                    {
                        Professions[professionType] = new();
                    }

                    Professions[professionType].ApplyApiData(prof);
                }
            }
        }

        private async Task UpdateSpecializations()
        {
            var specializations = await Gw2ApiManager.Gw2ApiClient.V2.Specializations.AllAsync();
            foreach (var spec in specializations)
            {
                if (!spec.Elite)
                {
                    continue;
                }

                if (!Specializations.ContainsKey(spec.Id))
                {
                    Specializations[spec.Id] = new();
                }

                Specializations[spec.Id].ApplyApiData(spec);
            }
        }

        private async Task UpdateMaps()
        {
            var maps = await Gw2ApiManager.Gw2ApiClient.V2.Maps.AllAsync();
            foreach (var map in maps)
            {
                if (!Maps.ContainsKey(map.Id))
                {
                    Maps[map.Id] = new();
                }

                Maps[map.Id].ApplyApiData(map);
            }
        }

        public async Task Load()
        {
            List<IDataDictionary> dataDictionaries = [Professions, Specializations, Maps];
            Versions versions = null;

            try
            {
                if (StaticInfo != null)
                {
                    StaticInfo.BetaStateChanged -= StaticInfo_BetaStateChanged;
                }

                versions = await StaticHosting.GetStaticVersions();
                StaticInfo = await StaticHosting.GetStaticContent<StaticInfo>("static_info.json");
                StaticInfo.BetaStateChanged += StaticInfo_BetaStateChanged;
                StaticContentLoaded = true;
            }
            catch (Exception ex)
            {
                Characters.Logger.Warn($"Failed to retrieve static versions. Use local data: {ex}");
            }

            versions ??= new Versions();

            bool allLoaded = true;

            foreach (var dict in dataDictionaries)
            {
                try
                {
                    string name = dict.FileName;
                    bool loaded = await dict.Load();

                    if (dict.IsOutdated(versions[name]))
                    {
                        Characters.Logger.Info($"Data dictionary '{name}' is outdated. Updating...");
                        await dict.Update(versions[name]);
                        await dict.Save();
                        loaded = true;

                        Characters.Logger.Info($"Data dictionary '{name}' updated to version {dict.Version}.");
                    }

                    allLoaded &= loaded;
                }
                catch (Exception ex)
                {
                    Characters.Logger.Warn($"Failed to load data dictionary from '{dict.FilePath}': {ex}");
                    allLoaded = false;
                }
            }

            IsLoaded = allLoaded && StaticContentLoaded;
            OnIsLoaded();
        }

        private void StaticInfo_BetaStateChanged(object sender, bool e)
        {
            BetaStateChanged?.Invoke(this, e);
        }

        private void OnIsLoaded()
        {
            //Dispatch event through the main thread dispatcher to avoid threading issues.
            Loaded?.Invoke(this, IsLoaded);
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            if (StaticInfo != null)
            {
                StaticInfo.BetaStateChanged -= StaticInfo_BetaStateChanged;
            }

            Professions?.Clear();
            Specializations?.Clear();
            Maps?.Clear();
            Races?.Clear();
            CraftingProfessions?.Clear();
        }

        public async Task UpdateLocale(Blish_HUD.ValueChangedEventArgs<Locale> eventArgs)
        {
            var newValue = eventArgs.NewValue;

            if (newValue is Locale.Korean or Locale.Chinese)
            {
                newValue = Locale.English;
            }

            if (Professions.Values.LastOrDefault()?.Names.TryGetValue(newValue, out string professionName) != true || string.IsNullOrEmpty(professionName))
            {
                Characters.Logger.Info($"Updating Professions for locale {newValue}.");
                await Professions.Update();
                await Professions.Save();
            }

            if (Specializations.Values.LastOrDefault()?.Names.TryGetValue(newValue, out string specializationName) != true || string.IsNullOrEmpty(specializationName))
            {
                Characters.Logger.Info($"Updating Specializations for locale {newValue}.");
                await Specializations.Update();
                await Specializations.Save();
            }

            if (Maps.Values.LastOrDefault()?.Names.TryGetValue(newValue, out string mapName) != true || string.IsNullOrEmpty(mapName))
            {
                Characters.Logger.Info($"Updating Maps for locale {newValue}.");
                await Maps.Update();
                await Maps.Save();

                LocalizingService.OnLocaleChanged(this, eventArgs);
            }
        }

        public async void Update()
        {
            StaticInfo?.CheckBeta();

            if (!IsLoaded && _loadingAttempts < MAX_LOADING_ATTEMPTS)
            {
                if ((DateTime.UtcNow - _lastLoadingTry).TotalSeconds > 60)
                {
                    _loadingAttempts++;
                    await Load();
                    _lastLoadingTry = DateTime.UtcNow;
                }
            }
        }
    }
}
