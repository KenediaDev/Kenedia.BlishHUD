using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.Models;
using Gw2Sharp.WebApi;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using File = System.IO.File;
using Race = Kenedia.Modules.BuildsManager.DataModels.Race;
using Skill = Kenedia.Modules.BuildsManager.DataModels.Professions.Skill;
using Trait = Kenedia.Modules.BuildsManager.DataModels.Professions.Trait;
using Specialization = Kenedia.Modules.BuildsManager.DataModels.Professions.Specialization;
using Legend = Kenedia.Modules.BuildsManager.DataModels.Professions.Legend;
using System.Threading;
using Kenedia.Modules.Core.DataModels;
using Gw2Sharp;
using ApiSkill = Gw2Sharp.WebApi.V2.Models.Skill;
using ApiPvpAmulet = Gw2Sharp.WebApi.V2.Models.PvpAmulet;
using Kenedia.Modules.Core.Extensions;
using Gw2Sharp.WebApi.Exceptions;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using Kenedia.Modules.Core.Utility;
using PvpAmulet = Kenedia.Modules.BuildsManager.DataModels.Items.PvpAmulet;

namespace Kenedia.Modules.BuildsManager.Services
{
    public class GW2API
    {
        private readonly Logger _logger = Logger.GetLogger(typeof(GW2API));
        private readonly Gw2ApiManager _gw2ApiManager;
        private readonly Data _data;
        private readonly PathCollection _paths;
        private CancellationTokenSource _cancellationTokenSource;

        private readonly List<int> _infusions = new()
        {
            39336, //Mighty Infusion
            39335, //Precise Infusion
            39337, //Malign Infusion
            39339, //Resilient Infusion
            39338, //Vital Infusion
            39340, //Healing Infusion

            39616, //Healing +5 Agony Infusion
            39617, //Resilient +5 Agony Infusion
            39618, //Vital +5 Agony Infusion
            39619, //Malign +5 Agony Infusion
            39620, //Mighty +5 Agony Infusion
            39621, //Precise +5 Agony Infusion
            85971, //Spiteful +5 Agony Infusion
            86338, //Mystical +5 Agony Infusion

            37133, //Resilient +7 Agony Infusion
            37134, //Vital +7 Agony Infusion
            37123, //Healing +7 Agony Infusion
            37127, //Mighty +7 Agony Infusion
            37128, //Precise +7 Agony Infusion
            37129, //Malign +7 Agony Infusion
            85881, //Mystical +7 Agony Infusion
            86150, //Spiteful +7 Agony Infusion

            37130, //Malign +9 Agony Infusion
            37131, //Mighty +9 Agony Infusion
            37132, //Precise +9 Agony Infusion
            37135, //Resilient +9 Agony Infusion
            37136, //Vital +9 Agony Infusion            
            37125, //Healing +9 Agony Infusion
            86180, //Mystical +9 Agony Infusion
            86113, //Spiteful +9 Agony Infusion

            43250, //Healing WvW Infusion
            43251, //Resilient WvW Infusion
            43252, //Vital WvW Infusion
            43253, //Malign WvW Infusion
            43254, //Mighty WvW Infusion
            43255, //Precise WvW Infusion
            86986, //Concentration WvW Infusion
            87218, //Expertise WvW Infusion

            49424, //+1 Agony Infusion
            49425, //+2 Agony Infusion
            49426, //+3 Agony Infusion
            49427, //+4 Agony Infusion
            49428, //+5 Agony Infusion
            49429, //+6 Agony Infusion
            49430, //+7 Agony Infusion
            49431, //+8 Agony Infusion
            49432, //+9 Agony Infusion
            49433, //+10 Agony Infusion
            49434, //+11 Agony Infusion
            49435, //+12 Agony Infusion
            49436, //+13 Agony Infusion
            49437, //+14 Agony Infusion
            49438, //+15 Agony Infusion
            49439, //+16 Agony Infusion
            49440, //+17 Agony Infusion
            49441, //+18 Agony Infusion
            49442, //+19 Agony Infusion
            49443, //+20 Agony Infusion
            49444, //+21 Agony Infusion
            49445, //+22 Agony Infusion
            49446, //+23 Agony Infusion
            49447, //+24 Agony Infusion
            
            87528, //Swim-Speed Infusion +10
            87518, //Swim-Speed Infusion +11
            87493, //Swim-Speed Infusion +12
            87503, //Swim-Speed Infusion +13
            87526, //Swim-Speed Infusion +14
            87496, //Swim-Speed Infusion +15
            87497, //Swim-Speed Infusion +16
            87508, //Swim-Speed Infusion +17
            87516, //Swim-Speed Infusion +18
            87532, //Swim-Speed Infusion +19
            87495, //Swim-Speed Infusion +20
            87525, //Swim-Speed Infusion +21
            87511, //Swim-Speed Infusion +22
            87512, //Swim-Speed Infusion +23
            87527, //Swim-Speed Infusion +24
            87502, //Swim-Speed Infusion +25
            87538, //Swim-Speed Infusion +26
            87504, //Swim-Speed Infusion +27
            //87504, //Swim-Speed Infusion +28
            //87504, //Swim-Speed Infusion +29
            //87504, //Swim-Speed Infusion +30
        };

        private readonly Dictionary<int, int?> _skinDictionary = new()
                {
                    { 85105, 5013 }, //Axe
                    { 85017, 4997 }, //Dagger
                    { 85251, 4995 }, //Greatsword
                    { 85060, 5022 }, //Hammer
                    { 85267, 5005 }, //Mace
                    { 85360, 5018 }, //Shield
                    { 85250, 5020 }, //Sword
                    { 84899, 5164 }, //Spear
                    { 85052, 5000 }, //Shortbow
                    { 84888, 4998 }, //Longbow
                    { 85010, 5008 }, //Pistol
                    { 85262, 5021 }, //Rifle
                    { 85307, 5001 }, //Warhorn
                    { 85323, 4992 }, //Torch
                    { 85341, 4990 }, //Harpoon Gun
                    { 84872, 4994 }, //Focus
                    { 85117, 4989 }, //Scepter
                    { 85026, 5019 }, //Staff
                    { 85265, 5129 }, //Trident
                    
                    { 79895, 854 }, //Aqua Breather (Heavy)
                    { 85193, 818 }, // Helm (Heavy)
                    { 84875, 808 }, // Shoulder (Heavy)
                    { 85084, 807 }, // Coat (Heavy)
                    { 85140, 812 }, // Gloves (Heavy)
                    { 84887, 797 }, // Leggings (Heavy)
                    { 85055, 801 },  // Boots (Heavy)
                    
                    { 79838, 856 }, //Aqua Breather (Medium)
                    { 80701, 817 }, // Helm (Medium)
                    { 80825 , 805 }, // Shoulder (Medium)
                    { 84977, 806 }, // Coat (Medium)
                    { 85169, 811 }, // Gloves (Medium)
                    { 85264, 796 }, // Leggings (Medium)
                    { 80836, 799 }, // Boots (Medium)
                    
                    { 79873, 855 }, //Aqua Breather (Light)
                    { 85128, 819 }, // Helm (Light)
                    { 84918, 810 }, // Shoulder (Light)
                    { 85333, 809 }, // Coat (Light)
                    { 85070, 813 }, // Gloves (Light)
                    { 85362, 798 }, // Leggings (Light)
                    { 80815, 803 },  // Boots (Light)                    
   
                    { 94947, 10161 }, //Back
                    { 79980, null }, // Amulet
                    { 80002, null }, // Accessory
                    { 80058, null },  // Ring

                    //{ 0, null },  // Relic
                };

        public GW2API(Gw2ApiManager gw2ApiManager, Data data, PathCollection paths)
        {
            _gw2ApiManager = gw2ApiManager;
            _data = data;
            _paths = paths;
        }

        public PathCollection Paths { get; set; }

        public void Cancel()
        {
            _cancellationTokenSource?.Cancel();
        }

        public bool IsCanceled()
        {
            return _cancellationTokenSource is not null && _cancellationTokenSource.IsCancellationRequested;
        }

        public async Task UpdateData()
        {
            if (Paths == null)
            {
                _logger.Info($"No Paths set for {nameof(UpdateData)}!");
                return;
            }

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new();

            _logger.Info($"{nameof(UpdateData)}: Fetch data ...");

            LocalizedString state = new();

            Locale locale = GameService.Overlay.UserLocale.Value;
            await GetItems(_cancellationTokenSource.Token);
            await GetProfessions(_cancellationTokenSource.Token);
            await GetPets(_cancellationTokenSource.Token);
        }

        public async Task GetItems(CancellationToken cancellation)
        {
            try
            {
                string json;

                var skins = await _gw2ApiManager.Gw2ApiClient.V2.Skins.ManyAsync(_skinDictionary.Where(e => e.Value is not null).Select(e => (int)e.Value));
                var api_armors = await _gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(_data.ItemMap.Armors.Select(e => e.Id), cancellation);
                var api_weapons = await _gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(_data.ItemMap.Weapons.Select(e => e.Id), cancellation);
                var api_backs = await _gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(_data.ItemMap.Backs.Select(e => e.Id), cancellation);
                var api_trinkets = await _gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(_data.ItemMap.Trinkets.Select(e => e.Id), cancellation);
                var api_amulets = await _gw2ApiManager.Gw2ApiClient.V2.Pvp.Amulets.AllAsync(cancellation);

                var api_enrichments = await _gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(_data.ItemMap.Enrichments.Select(e => e.Id), cancellation);
                var api_infusions = await _gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(_infusions, cancellation);

                var api_nourishments = await _gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(_data.ItemMap.Nourishments.Select(e => e.Id), cancellation);
                var api_utility = await _gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(_data.ItemMap.Utilities.Select(e => e.Id), cancellation);

                var api_pveRunes = await _gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(_data.ItemMap.PveRunes.Select(e => e.Id), cancellation);
                var api_pvpRunes = await _gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(_data.ItemMap.PvpRunes.Select(e => e.Id), cancellation);
                var api_pveSigils = await _gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(_data.ItemMap.PveSigils.Select(e => e.Id), cancellation);
                var api_pvpSigils = await _gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(_data.ItemMap.PvpSigils.Select(e => e.Id), cancellation);
                var statChoices = new List<int>();

                if (cancellation.IsCancellationRequested) return;

                void ApplyAmuletData(IReadOnlyList<ApiPvpAmulet> amulets, Dictionary<int, PvpAmulet> targetlist, string file, List<ItemMap> map)
                {
                    BuildsManager.Logger.Info($"Saving {file} ...");

                    foreach (var i in amulets)
                    {
                        bool exists = targetlist.TryGetValue(i.Id, out PvpAmulet amulet);
                        amulet ??= new(i);
                        amulet.Apply(i);

                        if (!exists) targetlist.Add(i.Id, amulet);
                    }

                    string json = JsonConvert.SerializeObject(targetlist, Formatting.Indented);
                    File.WriteAllText($@"{Paths.ModulePath}\data\{file}.json", json);
                }

                void ApplyData<T, TT>(IReadOnlyList<Item> items, Dictionary<int, TT> targetlist, string file, List<ItemMap> map, bool hasStatChoices = false) where T : Item where TT : BaseItem, new()
                {
                    BuildsManager.Logger.Info($"Saving {file} ...");
                    var skinIds = skins.Select(e => e.Id);

                    foreach (var i in items)
                    {
                        bool exists = targetlist.TryGetValue(i.Id, out TT item);
                        item ??= new();
                        item.Apply((T)i);

                        var mappedItem = map.Find(e => e.Id == i.Id);
                        if (mappedItem is not null)
                        {
                            item.MappedId = mappedItem.MappedId;
                        }

                        // Adjust Skins
                        if (_skinDictionary.ContainsKey(item.Id))
                        {
                            if(_skinDictionary[item.Id] is not null)
                            {
                                if (skinIds.Contains((int)_skinDictionary[item.Id]))
                                {
                                    var skin = skins.First(e => e.Id == _skinDictionary[item.Id]);

                                    if (skin is not null)
                                    {
                                        item.Name = skin.Name;
                                        item.SetAssetId(skin.Icon.GetAssetIdFromRenderUrl());
                                    }
                                }
                            }
                        }

                        if (!exists) targetlist.Add(i.Id, (TT)item);

                        if (hasStatChoices)
                        {
                            statChoices.AddRange((item as EquipmentItem).StatChoices.Except(statChoices));
                        }
                    }

                    string json = JsonConvert.SerializeObject(targetlist, Formatting.Indented);
                    File.WriteAllText($@"{Paths.ModulePath}\data\{file}.json", json);
                }

                ApplyData<ItemArmor, Armor>(api_armors, _data.Armors, "Armors", _data.ItemMap.Armors, true);
                ApplyData<ItemWeapon, Weapon>(api_weapons, _data.Weapons, "Weapons", _data.ItemMap.Weapons, true);
                ApplyData<ItemBack, Trinket>(api_backs, _data.Backs, "Backs", _data.ItemMap.Backs, true);
                ApplyData<ItemTrinket, Trinket>(api_trinkets, _data.Trinkets, "Trinkets", _data.ItemMap.Trinkets, true);

                ApplyData<ItemUpgradeComponent, Enrichment>(api_enrichments, _data.Enrichments, "Enrichments", _data.ItemMap.Enrichments);
                ApplyData<ItemUpgradeComponent, Infusion>(api_infusions, _data.Infusions, "Infusions", _data.ItemMap.Infusions);
                ApplyData<ItemConsumable, Nourishment>(api_nourishments, _data.Nourishments, "Nourishments", _data.ItemMap.Nourishments);
                ApplyData<ItemConsumable, DataModels.Items.Utility>(api_utility, _data.Utilities, "Utilities", _data.ItemMap.Utilities);
                ApplyData<ItemUpgradeComponent, Rune>(api_pveRunes, _data.PveRunes, "PveRunes", _data.ItemMap.PveRunes);
                ApplyData<ItemUpgradeComponent, Rune>(api_pvpRunes, _data.PvpRunes, "PvpRunes", _data.ItemMap.PvpRunes);
                ApplyData<ItemUpgradeComponent, Sigil>(api_pveSigils, _data.PveSigils, "PveSigils", _data.ItemMap.PveSigils);
                ApplyData<ItemUpgradeComponent, Sigil>(api_pvpSigils, _data.PvpSigils, "PvpSigils", _data.ItemMap.PvpSigils);

                ApplyAmuletData(api_amulets, _data.PvpAmulets, "PvpAmulets", _data.ItemMap.PvpAmulets);

                //Get Stats
                await GetStats(cancellation, statChoices);
            }
            catch (Exception ex)
            {
                if (!cancellation.IsCancellationRequested)
                {
                    _logger.Warn($"Failed to fetch armory items.");
                    _logger.Warn($"{ex}");
                }
            }
        }

        public async Task GetStats(CancellationToken cancellation, List<int> statIds)
        {
            try
            {
                var apiStats = await _gw2ApiManager.Gw2ApiClient.V2.Itemstats.AllAsync(cancellation);
                if (cancellation.IsCancellationRequested) return;

                foreach (var e in apiStats)
                {
                    if (statIds.Contains(e.Id))
                    {
                        bool exists = _data.Stats.TryGetValue(e.Id, out var stat);
                        stat ??= new(e);
                        stat.Apply(e);

                        if (!exists)
                        {
                            stat.MappedId = (byte)(_data.Stats.Count + 1);
                            _data.Stats.Add(e.Id, stat);
                        }
                    }
                }

                BuildsManager.Logger.Info($"Saving {"Stats"} ...");
                string json = JsonConvert.SerializeObject(_data.Stats, Formatting.Indented);
                File.WriteAllText($@"{Paths.ModulePath}\data\Stats.json", json);
            }
            catch (Exception ex)
            {
                if (!cancellation.IsCancellationRequested)
                {
                    _logger.Warn($"Failed to fetch Stats.");
                    _logger.Warn($"{ex}");
                }
            }
        }

        public async Task GetProfessions(CancellationToken cancellation)
        {
            try
            {
                var apiSkills = await _gw2ApiManager.Gw2ApiClient.V2.Skills.AllAsync(cancellation);
                var apiTraits = await _gw2ApiManager.Gw2ApiClient.V2.Traits.AllAsync(cancellation);
                var apiSpecializations = await _gw2ApiManager.Gw2ApiClient.V2.Specializations.AllAsync(cancellation);
                var apiProfessions = await _gw2ApiManager.Gw2ApiClient.V2.Professions.AllAsync(cancellation);
                var apiLegends = await _gw2ApiManager.Gw2ApiClient.V2.Legends.AllAsync(cancellation);
                var apiRaces = await _gw2ApiManager.Gw2ApiClient.V2.Races.AllAsync(cancellation);

                var adjustedApiLegends = apiLegends.Append(new()
                {
                    Id = "Legend7",
                    Swap = 62891,
                    Heal = 62719,
                    Elite = 62942,
                    Utilities = new List<int>()
                    {
                        62832,
                        62962,
                        62878,
                    }
                });

                if (cancellation.IsCancellationRequested) return;

                var paletteBySkills = new Dictionary<int, int>();
                foreach (var prof in apiProfessions)
                {
                    foreach (var pair in prof.SkillsByPalette)
                    {
                        if (!paletteBySkills.ContainsKey(pair.Value)) paletteBySkills.Add(pair.Value, pair.Key);
                    }
                }

                var skills = new Dictionary<int, Skill>();
                foreach (var skill in apiSkills)
                {
                    skills.Add(skill.Id, new(skill, paletteBySkills));
                }

                if (!_data.Races.Any(e => e.Key == Races.None))
                {
                    _data.Races.Add(Races.None, new() { Name = "None", Id = Races.None });
                }

                foreach (var apiRace in apiRaces)
                {
                    if (Enum.TryParse(apiRace.Id, out Races raceType))
                    {
                        bool exists = _data.Races.TryGetValue(raceType, out Race race);

                        race ??= new Race(apiRace, skills);

                        if (!exists)
                        {
                            _data.Races.Add(race.Id, race);
                        }
                        else
                        {
                            race.UpdateLanguage(apiRace, skills);
                        }
                    }
                }

                var traits = new Dictionary<int, Trait>();
                foreach (var trait in apiTraits)
                {
                    traits.Add(trait.Id, new(trait, skills));
                }

                var specializations = new Dictionary<int, Specialization>();
                foreach (var specialization in apiSpecializations)
                {
                    specializations.Add(specialization.Id, new(specialization, traits));
                }

                var legends = new Dictionary<int, Legend>();
                foreach (var l in adjustedApiLegends)
                {
                    var legend = new Legend(l, skills);
                    legends.Add(legend.Id, new(l, skills));
                }

                foreach (var e in apiProfessions)
                {
                    if (Enum.TryParse(e.Id, out ProfessionType professionType))
                    {
                        bool exists = _data.Professions.TryGetValue(professionType, out var profession);
                        profession ??= new();
                        profession.Apply(e, specializations, traits, skills, legends, _data.Races);
                        if (!exists) _data.Professions.Add(professionType, profession);
                    }
                }

                string json = JsonConvert.SerializeObject(_data.Professions, Formatting.Indented);
                File.WriteAllText($@"{Paths.ModulePath}\data\Professions.json", json);

                json = JsonConvert.SerializeObject(paletteBySkills, Formatting.Indented);
                File.WriteAllText($@"{Paths.ModulePath}\data\PaletteBySkills.json", json);

                json = JsonConvert.SerializeObject(_data.Races, Formatting.Indented);
                File.WriteAllText($@"{Paths.ModulePath}\data\Races.json", json);
            }
            catch (Exception ex)
            {
                if (!cancellation.IsCancellationRequested)
                {
                    _logger.Warn($"Failed to fetch Professions.");
                    _logger.Warn($"{ex}");
                }
            }
        }

        public async Task GetPets(CancellationToken cancellation)
        {
            try
            {
                var apiPets = await _gw2ApiManager.Gw2ApiClient.V2.Pets.AllAsync();
                var petSkillIds = (from pet in apiPets
                                   from skill in pet.Skills
                                   select skill.Id).ToList();

                var apiPetSkills = await _gw2ApiManager.Gw2ApiClient.V2.Skills.ManyAsync(petSkillIds, cancellation);
                var petSkills = new List<Skill>();

                var apiProfession = await _gw2ApiManager.Gw2ApiClient.V2.Professions.GetAsync(ProfessionType.Guardian, cancellation);

                if (cancellation.IsCancellationRequested) return;

                var paletteBySkills = new Dictionary<int, int>();
                foreach (var pair in apiProfession?.SkillsByPalette)
                {
                    if (!paletteBySkills.ContainsKey(pair.Value)) paletteBySkills.Add(pair.Value, pair.Key);
                }

                foreach (var skill in apiPetSkills)
                {
                    petSkills.Add(new(skill, paletteBySkills));
                }

                foreach (var e in apiPets)
                {
                    bool exists = _data.Pets.TryGetValue(e.Id, out var pet);
                    pet ??= new(e, petSkills);
                    pet.ApplyLanguage(e, petSkills);
                    if (!exists) _data.Pets.Add(e.Id, pet);
                }

                string json = JsonConvert.SerializeObject(_data.Pets, Formatting.Indented);
                File.WriteAllText($@"{Paths.ModulePath}\data\Pets.json", json);
            }
            catch (Exception ex)
            {
                if (!cancellation.IsCancellationRequested)
                {
                    _logger.Warn($"Failed to fetch Ranger Pets.");
                    _logger.Warn($"{ex}");
                }
            }
        }

        public async Task GetSkillConnections(CancellationToken cancellation)
        {
            var apiSkills = await _gw2ApiManager.Gw2ApiClient.V2.Skills.AllAsync(cancellation);
            var apiTraits = await _gw2ApiManager.Gw2ApiClient.V2.Traits.AllAsync(cancellation);
            if (cancellation.IsCancellationRequested) return;

            var traits = apiTraits.ToList();
            var skills = apiSkills.ToList();

            Chain getChain(ApiSkill targetSkill, List<int> ids = null)
            {
                if (targetSkill == null) return null;

                ids ??= new();
                ids.Add(targetSkill.Id);

                if (targetSkill.NextChain is not null)
                {
                    var s = skills.Find(e => e != targetSkill && e.Id == targetSkill.NextChain);
                    if (s is not null && !(ids?.Contains(s.Id) == true))
                    {
                        _ = getChain(s, ids);
                    }
                }

                return new()
                {
                    First = ids.Count > 0 ? ids[0] : null,
                    Second = ids.Count > 1 ? ids[1] : null,
                    Third = ids.Count > 2 ? ids[2] : null,
                    Fourth = ids.Count > 3 ? ids[3] : null,
                    Fifth = ids.Count > 4 ? ids[4] : null,
                };
            }

            FlipSkills getFlips(ApiSkill targetSkill, List<int> ids = null)
            {
                if (targetSkill == null) return null;

                ids ??= new List<int>();
                ids.Add(targetSkill.Id);

                if (targetSkill.NextChain is not null)
                {
                    var s = skills.Find(e => e != targetSkill && e.Id == targetSkill.NextChain);
                    if (s is not null && !(ids?.Contains(s.Id) == true))
                    {
                        _ = getFlips(s, ids);
                    }
                }

                return new()
                {
                    Default = ids.Count > 0 ? ids[0] : null,
                    State1 = ids.Count > 1 ? ids[1] : null,
                    State2 = ids.Count > 2 ? ids[2] : null,
                    State3 = ids.Count > 3 ? ids[3] : null,
                    State4 = ids.Count > 4 ? ids[4] : null,
                };
            }

            BuildsManager.Data.OldConnections = new Dictionary<int, OldSkillConnection>();
            foreach (var skill in skills)
            {
                if (skill.Type != SkillType.Monster && skill.Professions.Count > 0)
                {
                    var connection = new OldSkillConnection()
                    {
                        Id = skill.Id,
                        Weapon = skill.WeaponType?.ToEnum() ?? null,
                        Specialization = skill.Specialization is not null ? (SpecializationType)skill.Specialization : null,
                        Enviroment = skill.Flags.Count() > 0 && skill.Flags.Aggregate((x, y) => x |= y.ToEnum()).Value.HasFlag(SkillFlag.NoUnderwater) ? Enviroment.Terrestrial : Enviroment.Terrestrial | Enviroment.Aquatic,
                    };

                    if (skill.ToolbeltSkill is not null)
                    {
                        connection.Toolbelt = skill.ToolbeltSkill;
                    }

                    if (skill.NextChain is not null)
                    {
                        connection.Chain = getChain(skill);
                    }

                    if (skill.BundleSkills is not null)
                    {
                        connection.Bundle = new()
                        {
                            Weapon_1 = skill.BundleSkills.Count() > 0 ? skill.BundleSkills[0] : null,
                            Weapon_2 = skill.BundleSkills.Count() > 1 ? skill.BundleSkills[1] : null,
                            Weapon_3 = skill.BundleSkills.Count() > 2 ? skill.BundleSkills[2] : null,
                            Weapon_4 = skill.BundleSkills.Count() > 3 ? skill.BundleSkills[3] : null,
                            Weapon_5 = skill.BundleSkills.Count() > 4 ? skill.BundleSkills[4] : null,
                        };
                    }

                    if (skill.TransformSkills is not null)
                    {
                        connection.Transform = new()
                        {
                            Weapon_1 = skill.TransformSkills.Count() > 0 ? skill.TransformSkills[0] : null,
                            Weapon_2 = skill.TransformSkills.Count() > 1 ? skill.TransformSkills[1] : null,
                            Weapon_3 = skill.TransformSkills.Count() > 2 ? skill.TransformSkills[2] : null,
                            Weapon_4 = skill.TransformSkills.Count() > 3 ? skill.TransformSkills[3] : null,
                            Weapon_5 = skill.TransformSkills.Count() > 4 ? skill.TransformSkills[4] : null,
                        };
                    }

                    if (skill.FlipSkill is not null)
                    {
                        connection.FlipSkills = getFlips(skill);
                    }

                    if (skill.TraitedFacts is not null)
                    {
                        foreach (var t in skill.TraitedFacts)
                        {
                            if (t.RequiresTrait is not null)
                            {
                                var trait = traits.Find(e => e.Id == t.RequiresTrait);
                                if (trait is not null && trait.Skills is not null)
                                {
                                    connection.Traited ??= new();
                                    foreach (int s in trait.Skills.Select(e => e.Id).ToList())
                                    {
                                        connection.Traited[s] = trait.Id;
                                    }
                                }
                            }
                        }
                    }

                    if (skill.Slot == SkillSlot.Weapon1 && skill.Professions.Contains("Thief"))
                    {
                        connection.Chain.Stealth = skills.Find(e => e.Slot == skill.Slot && e.WeaponType == skill.WeaponType && e.Categories?.Contains("StealthAttack") == true)?.Id;
                    }

                    if (skill.Slot == SkillSlot.Weapon1 && skill.Professions.Contains("Mesmer"))
                    {
                        connection.Chain.Ambush = skills.Find(e => e.Slot == skill.Slot && e.WeaponType == skill.WeaponType && e.Description?.Contains("Ambush") == true)?.Id;
                    }

                    BuildsManager.Data.OldConnections.Add(skill.Id, connection);
                }
            }

            var cnts = BuildsManager.Data.OldConnections.Values.ToList();
            foreach (var connection in BuildsManager.Data.OldConnections)
            {
                connection.Value.Default = cnts.Find(e =>
                e.Chain?.Contains(connection.Value.Id) == true ||
                e.Bundle?.Contains(connection.Value.Id) == true ||
                e.Transform?.Contains(connection.Value.Id) == true ||
                e.FlipSkills?.Contains(connection.Value.Id) == true ||
                e.Traited?.ContainsKey(connection.Value.Id) == true ||
                (e.Toolbelt is not null && e.Toolbelt == connection.Value.Id)
                )?.Id;
            }

            string json = JsonConvert.SerializeObject(BuildsManager.Data.SkillConnections, Formatting.Indented);
            File.WriteAllText($@"{_paths.ModuleDataPath}\SkillConnections.json", json);
        }

        internal async Task FetchBaseSkills(CancellationToken cancellation)
        {
            var skills = await _gw2ApiManager.Gw2ApiClient.V2.Skills.AllAsync(cancellation);
            if (cancellation.IsCancellationRequested) return;
            var baseSkills = skills.ToList().ToDictionary(skill => skill.Id, skill => new BaseSkill(skill));

            string json = JsonConvert.SerializeObject(baseSkills, Formatting.Indented);
            File.WriteAllText($@"{Paths.ModulePath}\data\BaseSkills.json", json);

            await _data.LoadBaseSkills();
        }

        public async Task FetchItems(CancellationToken cancellation)
        {
            try
            {
                if (cancellation.IsCancellationRequested) return;
            }
            catch (RequestException ex)
            {
                BuildsManager.Logger.Warn($"RequestException {ex}");
            }
            catch (Exception ex)
            {
                BuildsManager.Logger.Warn($"Exception {ex}");
            }
        }

        public async Task CreateItemMap(CancellationToken cancellation)
        {
            try
            {
                var invalid = new List<int>();
                //var raw_itemids = new List<int>(){};
                var raw_itemids = await _gw2ApiManager.Gw2ApiClient.V2.Items.IdsAsync(cancellation);
                var itemids = raw_itemids.ToList();

                if (cancellation.IsCancellationRequested) return;

                //Remove JadeBot stuff
                var invalidIds = new List<int>()
                {
                    45016,
                    45017,
                    11126, // Corrupted
                    93128, // Slumbering Conflux
                    93140, // Slumbering Transcendence
                    88567, // Bifrost Axe

                    30703, // Sunrise, else to many GS
                    30704, // Twilight, else to many GS

                    63366, //Nameless Rune
                };

                var jadetechmodules = new List<int>() {
                    95923,
                    97553,
                    96654,
                    97572,
                    96826,
                    97410,
                    96871,
                    97775,
                    97107,
                    95904,
                    96853,
                    96919,
                    96758,
                    95893,
                    95647,
                    96477,
                    95802,
                    96360,
                    97289,
                    97656,
                    96450,
                    96992,
                    96618,
                    96590,
                    95800,
                    95801,
                    97686,
                    97535,
                    95582,
                    95948,
                    97101,
                    96474,
                    95582,
                    95647,
                    95800,
                    95801,
                    95802,
                    95864,
                    95893,
                    95904,
                    95923,
                    95948,
                    96070,
                    96299,
                    96360,
                    96450,
                    96467,
                    96474,
                    96477,
                    96590,
                    96613,
                    96618,
                    96628,
                    96654,
                    96758,
                    96826,
                    96853,
                    96871,
                    96919,
                    96992,
                    97020,
                    97041,
                    97101,
                    97107,
                    97284,
                    97289,
                    97339,
                    97410,
                    97535,
                    97553,
                    97572,
                    97656,
                    97686,
                    97775
                };
                var commonfeasts = new List<int>()
                {
                    82657,
                    83171,
                    75053,
                    72761,
                    71940,
                    71176,
                    70628,
                    76944,
                    76388,
                    83545,
                    83591,
                    91943,
                    98924,
                    12585,
                    12856,
                    12587,
                    12588,
                    12589,
                    12590,
                    12591,
                    12592,
                    12593,
                    12633,
                    12634,
                    12635,
                    12636,
                    12637,
                    12638,
                    12639,
                    12640,
                    12641,
                    12642,
                    12678,
                    12679,
                    12680,
                    12681,
                    12682,
                    12683,
                    12684,
                    12685,
                    12686,
                    12720,
                    12721,
                    12722,
                    12723,
                    12724,
                    12725,
                    12726,
                    12727,
                    12728,
                    66525,
                    66532,
                    66533,
                    66534,
                    66535,
                    71797,
                    71826,
                    73221,
                };
                _ = itemids.RemoveAll(invalidIds.Contains);
                _ = itemids.RemoveAll(commonfeasts.Contains);
                _ = itemids.RemoveAll(jadetechmodules.Contains);

                //Ascended and Exotic
                var exotic = new List<int>()
                {
                    //Aquabreather                 
                    68357,
                    68356,
                    68359,
                    
                    //Light
                    75625,
                    73202,
                    71738,
                    71149,
                    70899,
                    75646,
                    
                    //Medium
                    75967,
                    75368,
                    76182,
                    74543,
                    71759,
                    70719,

                    //Heavy
                    70597,
                    75380,
                    74957,
                    74006,
                    73850,
                    71786,

                    //Back
                    74623,

                    //Ring
                    96699,

                    //Accessory
                    97239,

                    //Amulet
                    97687,

                    //Weapons
                    73380,
                    75881,
                    75835,
                    74862,
                    76598,
                    76257,
                    71232,
                    72542,
                    76974,
                    70638,
                    73091,
                    71670,
                    73269,
                    71164,
                    72165,
                    70785,
                    74760,
                    75830,
                    77196,
                };

                var manualItems = new List<int>();
                manualItems.AddRange(_skinDictionary.Keys);
                //manualItems.AddRange(exotic);

                var itemid_lists = itemids.ChunkBy(200);
                var itemMapping = new ItemMapping();
                int count = 0;

                BuildsManager.Logger.Info($"Fetching a total of {raw_itemids.Count} items.");

                foreach (var ids in itemid_lists)
                {
                    if (ids is not null)
                    {
                        count++;
                        IReadOnlyList<Item> items = null;

                        BuildsManager.Logger.Info($"Fetching chunk {count}/{itemid_lists.Count}");
                        //Debug.WriteLine($"ID: {ids.FirstOrDefault()}");

                        try
                        {
                            items = await _gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(ids);
                        }
                        catch (RequestException ex)
                        {
                            BuildsManager.Logger.Warn($"Exception {ex}");
                            invalid.AddRange(ids);

                            string json = JsonConvert.SerializeObject(invalid, Formatting.Indented);
                            File.WriteAllText($@"{Paths.ModulePath}\data\InvalidItemIds.json", json);
                        }
                        catch (Exception ex)
                        {
                            BuildsManager.Logger.Warn($"Exception {ex}");
                            invalid.AddRange(ids);

                            string json = JsonConvert.SerializeObject(invalid, Formatting.Indented);
                            File.WriteAllText($@"{Paths.ModulePath}\data\InvalidItemIds.json", json);
                        }

                        if (items is not null)
                        {
                            foreach (var item in items)
                            {
                                if (item.Type == ItemType.PowerCore)
                                {
                                    itemMapping.PowerCore.Add(new()
                                    {
                                        Id = item.Id,
                                        MappedId = (byte)(itemMapping.PowerCore.Count + 1),
                                        Name = item.Name,
                                    });
                                }

                                if (manualItems.Contains(item.Id))
                                {
                                    switch (item.Type.Value)
                                    {
                                        case ItemType.Trinket:
                                            var trinket = (ItemTrinket)item;

                                            itemMapping.Trinkets.Add(new()
                                            {
                                                Id = trinket.Id,
                                                MappedId = (byte)(itemMapping.Trinkets.Count + 1),
                                                Name = trinket.Name,
                                            });
                                            break;
                                        case ItemType.Back:
                                            var back = (ItemBack)item;

                                            itemMapping.Backs.Add(new()
                                            {
                                                Id = back.Id,
                                                MappedId = (byte)(itemMapping.Backs.Count + 1),
                                                Name = back.Name,
                                            });
                                            break;
                                        case ItemType.Armor:
                                            var armor = (ItemArmor)item;

                                            itemMapping.Armors.Add(new()
                                            {
                                                Id = armor.Id,
                                                MappedId = (byte)(itemMapping.Armors.Count + 1),
                                                Name = armor.Name,
                                            });
                                            break;
                                        case ItemType.Weapon:
                                            var weapon = (ItemWeapon)item;

                                            itemMapping.Weapons.Add(new()
                                            {
                                                Id = weapon.Id,
                                                MappedId = (byte)(itemMapping.Weapons.Count + 1),
                                                Name = weapon.Name,
                                            });
                                            break;
                                    }
                                }

                                if (item.Type == ItemType.Consumable)
                                {
                                    var consumable = (ItemConsumable)item;

                                    // Nourishment
                                    if (consumable.Details.Type == ItemConsumableType.Food)
                                    {
                                        if (consumable.Rarity == ItemRarity.Ascended || consumable.Level == 80)
                                        {
                                            itemMapping.Nourishments.Add(new()
                                            {
                                                Id = consumable.Id,
                                                MappedId = (byte)(itemMapping.Nourishments.Count + 1),
                                                Name = consumable.Name,
                                            });
                                        }
                                    }

                                    // Utility
                                    if (consumable.Details.Type == ItemConsumableType.Utility)
                                    {
                                        if (consumable.Level == 80)
                                        {
                                            itemMapping.Utilities.Add(new()
                                            {
                                                Id = consumable.Id,
                                                MappedId = (byte)(itemMapping.Utilities.Count + 1),
                                                Name = consumable.Name,
                                            });
                                        }
                                    }
                                }

                                if (item.Type == ItemType.UpgradeComponent)
                                {
                                    var upgrade = (ItemUpgradeComponent)item;

                                    bool isRune = upgrade.Details.Flags.ToList().Contains(ItemUpgradeComponentFlag.HeavyArmor) && !upgrade.Details.Flags.ToList().Contains(ItemUpgradeComponentFlag.Trinket) && !upgrade.Details.Flags.ToList().Contains(ItemUpgradeComponentFlag.Sword);
                                    bool isSigil = !upgrade.Details.Flags.ToList().Contains(ItemUpgradeComponentFlag.HeavyArmor) && !upgrade.Details.Flags.ToList().Contains(ItemUpgradeComponentFlag.Trinket) && upgrade.Details.Flags.ToList().Contains(ItemUpgradeComponentFlag.Sword);

                                    //Rune
                                    if (upgrade.Rarity == ItemRarity.Exotic && isRune)
                                    {
                                        if (upgrade.GameTypes.ToList().Contains(ItemGameType.Pvp))
                                        {
                                            itemMapping.PvpRunes.Add(new()
                                            {
                                                Id = upgrade.Id,
                                                MappedId = (byte)(itemMapping.PvpRunes.Count + 1),
                                                Name = upgrade.Name,
                                            });
                                        }
                                        else if (upgrade.GameTypes.ToList().Contains(ItemGameType.Pve))
                                        {
                                            itemMapping.PveRunes.Add(new()
                                            {
                                                Id = upgrade.Id,
                                                MappedId = (byte)(itemMapping.PveRunes.Count + 1),
                                                Name = upgrade.Name,
                                            });
                                        }
                                    }

                                    //Sigil
                                    if (upgrade.Rarity == ItemRarity.Exotic && isSigil)
                                    {
                                        if (upgrade.GameTypes.ToList().Contains(ItemGameType.Pvp))
                                        {
                                            itemMapping.PvpSigils.Add(new()
                                            {
                                                Id = upgrade.Id,
                                                MappedId = (byte)(itemMapping.PvpSigils.Count + 1),
                                                Name = upgrade.Name,
                                            });
                                        }
                                        else if (upgrade.GameTypes.ToList().Contains(ItemGameType.Pve))
                                        {
                                            itemMapping.PveSigils.Add(new()
                                            {
                                                Id = upgrade.Id,
                                                MappedId = (byte)(itemMapping.PveSigils.Count + 1),
                                                Name = upgrade.Name,
                                            });
                                        }
                                    }

                                    //Infusions
                                    if (upgrade.Details.InfusionUpgradeFlags.ToList().Contains(ItemInfusionFlag.Infusion) && _infusions.Contains(upgrade.Id))
                                    {
                                        itemMapping.Infusions.Add(new()
                                        {
                                            Id = upgrade.Id,
                                            MappedId = (byte)(itemMapping.Infusions.Count + 1),
                                            Name = upgrade.Name,
                                        });
                                    }

                                    //Enrichments
                                    if (upgrade.Details.InfusionUpgradeFlags.ToList().Contains(ItemInfusionFlag.Enrichment))
                                    {
                                        itemMapping.Enrichments.Add(new()
                                        {
                                            Id = upgrade.Id,
                                            MappedId = (byte)(itemMapping.Enrichments.Count + 1),
                                            Name = upgrade.Name,
                                        });
                                    }
                                }
                            }

                            string json = JsonConvert.SerializeObject(itemMapping, Formatting.Indented);
                            File.WriteAllText($@"{Paths.ModulePath}\data\ItemMapping.json", json);
                        }
                    }
                }

                BuildsManager.Logger.Info($"All {raw_itemids.Count} items fetched!");

                string finaljson = JsonConvert.SerializeObject(itemMapping, Formatting.Indented);
                File.WriteAllText($@"{Paths.ModulePath}\data\ItemMapping.json", finaljson);
            }
            catch (RequestException ex)
            {

            }
            catch (Exception ex)
            {
                BuildsManager.Logger.Warn($"Exception {ex}");
            }
        }
    }
}
