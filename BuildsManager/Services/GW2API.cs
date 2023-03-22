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
using Profession = Kenedia.Modules.BuildsManager.DataModels.Professions.Profession;
using Skill = Kenedia.Modules.BuildsManager.DataModels.Professions.Skill;
using Trait = Kenedia.Modules.BuildsManager.DataModels.Professions.Trait;
using Specialization = Kenedia.Modules.BuildsManager.DataModels.Professions.Specialization;
using Legend = Kenedia.Modules.BuildsManager.DataModels.Professions.Legend;
using Kenedia.Modules.BuildsManager.DataModels.Stats;
using Pet = Kenedia.Modules.BuildsManager.DataModels.Professions.Pet;
using System.Threading;
using Kenedia.Modules.Core.DataModels;
using Gw2Sharp;
using ApiSkill = Gw2Sharp.WebApi.V2.Models.Skill;
using Kenedia.Modules.Core.Extensions;
using System.Diagnostics;
using Gw2Sharp.WebApi.Exceptions;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.DataModels.Items;

namespace Kenedia.Modules.BuildsManager.Services
{
    public class GW2API
    {
        private readonly Logger _logger = Logger.GetLogger(typeof(GW2API));
        private readonly Gw2ApiManager _gw2ApiManager;
        private readonly Data _data;
        private readonly PathCollection _paths;
        private CancellationTokenSource _cancellationTokenSource;

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
            return _cancellationTokenSource != null && _cancellationTokenSource.IsCancellationRequested;
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

            var professions = _data.Professions;
            var races = _data.Races;
            var stats = _data.Stats;
            var pets = _data.Pets;

            Locale locale = GameService.Overlay.UserLocale.Value;
            await GetItems(_cancellationTokenSource.Token);
            //await GetProfessions(_cancellationTokenSource.Token, professions, races);
            //await GetPets(_cancellationTokenSource.Token, pets);
        }

        public async Task GetItems(CancellationToken cancellation)
        {
            try
            {
                string json;

                var api_armors = await _gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(_data.ItemMap.Armors.Select(e => e.Id), cancellation);
                var api_weapons = await _gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(_data.ItemMap.Weapons.Select(e => e.Id), cancellation);
                var api_backs = await _gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(_data.ItemMap.Backs.Select(e => e.Id), cancellation);
                var api_trinkets = await _gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(_data.ItemMap.Trinkets.Select(e => e.Id), cancellation);

                var api_enrichments = await _gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(_data.ItemMap.Enrichments.Select(e => e.Id), cancellation);
                var api_infusions = await _gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(_data.ItemMap.Infusions.Select(e => e.Id), cancellation);

                var api_nourishments = await _gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(_data.ItemMap.Nourishments.Select(e => e.Id), cancellation);
                var api_utility = await _gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(_data.ItemMap.Utilities.Select(e => e.Id), cancellation);

                var api_pveRunes = await _gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(_data.ItemMap.PveRunes.Select(e => e.Id), cancellation);
                var api_pvpRunes = await _gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(_data.ItemMap.PvpRunes.Select(e => e.Id), cancellation);
                var api_pveSigils = await _gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(_data.ItemMap.PveSigils.Select(e => e.Id), cancellation);
                var api_pvpSigils = await _gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(_data.ItemMap.PvpSigils.Select(e => e.Id), cancellation);
                var statChoices = new List<int>();

                if (cancellation.IsCancellationRequested) return;

                void ApplyData<T, TT>(IReadOnlyList<Item> items, Dictionary<int, TT> targetlist, string file, List<ItemMap> map, bool hasStatChoices = false) where T : Item where TT : BaseItem, new()
                {
                    BuildsManager.Logger.Info($"Saving {file} ...");
                    foreach (var i in items)
                    {
                        bool exists = targetlist.TryGetValue(i.Id, out TT item);
                        item ??= new();
                        item.Apply((T)i);

                        var mappedItem = map.Find(e => e.Id == i.Id);
                        if (mappedItem != null)
                        {
                            item.MappedId = mappedItem.MappedId;
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
                            stat.MappedId = _data.Stats.Count;
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

        public async Task GetProfessions(CancellationToken cancellation, Dictionary<ProfessionType, Profession> professions, Dictionary<Races, Race> races)
        {
            try
            {
                var apiSkills = await _gw2ApiManager.Gw2ApiClient.V2.Skills.AllAsync(cancellation);
                var apiTraits = await _gw2ApiManager.Gw2ApiClient.V2.Traits.AllAsync(cancellation);
                var apiSpecializations = await _gw2ApiManager.Gw2ApiClient.V2.Specializations.AllAsync(cancellation);
                var apiProfessions = await _gw2ApiManager.Gw2ApiClient.V2.Professions.AllAsync(cancellation);
                var apiLegends = await _gw2ApiManager.Gw2ApiClient.V2.Legends.AllAsync(cancellation);
                var apiRaces = await _gw2ApiManager.Gw2ApiClient.V2.Races.AllAsync(cancellation);

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

                foreach (var apiRace in apiRaces)
                {
                    if (Enum.TryParse(apiRace.Id, out Races raceType))
                    {
                        bool exists = races.TryGetValue(raceType, out Race race);

                        race ??= new Race(apiRace, skills);

                        if (!exists)
                        {
                            races.Add(race.Id, race);
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
                foreach (var l in apiLegends)
                {
                    var legend = new Legend(l, skills);
                    legends.Add(legend.Id, new(l, skills));
                }

                foreach (var e in apiProfessions)
                {
                    if (Enum.TryParse(e.Id, out ProfessionType professionType))
                    {
                        bool exists = professions.TryGetValue(professionType, out var profession);
                        profession ??= new();
                        profession.Apply(e, specializations, traits, skills, legends, races);
                        if (!exists) professions.Add(professionType, profession);
                    }
                }

                string json = JsonConvert.SerializeObject(professions, Formatting.Indented);
                File.WriteAllText($@"{Paths.ModulePath}\data\Professions.json", json);

                json = JsonConvert.SerializeObject(paletteBySkills, Formatting.Indented);
                File.WriteAllText($@"{Paths.ModulePath}\data\PaletteBySkills.json", json);

                json = JsonConvert.SerializeObject(races, Formatting.Indented);
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

        public async Task GetPets(CancellationToken cancellation, Dictionary<int, Pet> pets)
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
                    bool exists = pets.TryGetValue(e.Id, out var pet);
                    pet ??= new(e, petSkills);
                    pet.ApplyLanguage(e, petSkills);
                    if (!exists) pets.Add(e.Id, pet);
                }

                string json = JsonConvert.SerializeObject(pets, Formatting.Indented);
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

                if (targetSkill.NextChain != null)
                {
                    var s = skills.Find(e => e != targetSkill && e.Id == targetSkill.NextChain);
                    if (s != null && !(ids?.Contains(s.Id) == true))
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

                if (targetSkill.NextChain != null)
                {
                    var s = skills.Find(e => e != targetSkill && e.Id == targetSkill.NextChain);
                    if (s != null && !(ids?.Contains(s.Id) == true))
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
                        Specialization = skill.Specialization != null ? (SpecializationType)skill.Specialization : null,
                        Enviroment = skill.Flags.Count() > 0 && skill.Flags.Aggregate((x, y) => x |= y.ToEnum()).Value.HasFlag(SkillFlag.NoUnderwater) ? Enviroment.Terrestrial : Enviroment.Terrestrial | Enviroment.Aquatic,
                    };

                    if (skill.ToolbeltSkill != null)
                    {
                        connection.Toolbelt = skill.ToolbeltSkill;
                    }

                    if (skill.NextChain != null)
                    {
                        connection.Chain = getChain(skill);
                    }

                    if (skill.BundleSkills != null)
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

                    if (skill.TransformSkills != null)
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

                    if (skill.FlipSkill != null)
                    {
                        connection.FlipSkills = getFlips(skill);
                    }

                    if (skill.TraitedFacts != null)
                    {
                        foreach (var t in skill.TraitedFacts)
                        {
                            if (t.RequiresTrait != null)
                            {
                                var trait = traits.Find(e => e.Id == t.RequiresTrait);
                                if (trait != null && trait.Skills != null)
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
                (e.Toolbelt != null && e.Toolbelt == connection.Value.Id)
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
                Debug.WriteLine($"RequestException {ex}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception {ex}");
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

                var ascended = new List<int>()
                {
                    //Aquabreather
                    79873,
                    79838,
                    79895,

                    //Light
                    85128,
                    84918,
                    85333,
                    85070,
                    85362,
                    80815,

                    //Medium
                    80701,
                    80825,
                    84977,
                    85169,
                    85264,
                    80836,
                    
                    //Heavy
                    85193,
                    84875,
                    85084,
                    85140,
                    84887,
                    85055,

                    //Weapons
                    85105,
                    85017,
                    85251,
                    85060,
                    85267,
                    85360,
                    85250,
                    84899,
                    85052,
                    84888,
                    85010,
                    85262,
                    85307,
                    85323,
                    85341,
                    84872,
                    85117,
                    85026,
                    85265,

                    //Back
                    94947,

                    //Trinkets
                    79980,
                    80002,
                    80058,
                };

                var manualItems = new List<int>();
                manualItems.AddRange(ascended);
                manualItems.AddRange(exotic);

                var itemid_lists = itemids.ChunkBy(200);
                var itemMapping = new ItemMapping();
                int count = 0;

                Debug.WriteLine($"Fetching a total of {raw_itemids.Count} items.");

                foreach (var ids in itemid_lists)
                {
                    if (ids != null)
                    {
                        count++;
                        IReadOnlyList<Item> items = null;

                        Debug.WriteLine($"Fetching chunk {count}/{itemid_lists.Count}");
                        //Debug.WriteLine($"ID: {ids.FirstOrDefault()}");

                        try
                        {
                            items = await _gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(ids);
                        }
                        catch (RequestException ex)
                        {
                            Debug.WriteLine($"Exception {ex}");
                            invalid.AddRange(ids);

                            string json = JsonConvert.SerializeObject(invalid, Formatting.Indented);
                            File.WriteAllText($@"{Paths.ModulePath}\data\InvalidItemIds.json", json);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Exception {ex}");
                            invalid.AddRange(ids);

                            string json = JsonConvert.SerializeObject(invalid, Formatting.Indented);
                            File.WriteAllText($@"{Paths.ModulePath}\data\InvalidItemIds.json", json);
                        }

                        if (items != null)
                        {
                            foreach (var item in items)
                            {
                                if (item.Type == ItemType.PowerCore)
                                {
                                    itemMapping.PowerCore.Add(new()
                                    {
                                        Id = item.Id,
                                        MappedId = itemMapping.PowerCore.Count,
                                        Name = item.Name,
                                    });
                                }

                                if (item.Type == ItemType.Trinket && (item.Rarity == ItemRarity.Legendary || (manualItems.Contains(item.Id))))
                                {
                                    var trinket = (ItemTrinket)item;

                                    itemMapping.Trinkets.Add(new()
                                    {
                                        Id = trinket.Id,
                                        MappedId = itemMapping.Trinkets.Count,
                                        Name = trinket.Name,
                                    });
                                }

                                if (item.Type == ItemType.Back && (item.Rarity == ItemRarity.Legendary || (manualItems.Contains(item.Id))))
                                {
                                    var back = (ItemBack)item;

                                    itemMapping.Backs.Add(new()
                                    {
                                        Id = back.Id,
                                        MappedId = itemMapping.Backs.Count,
                                        Name = back.Name,
                                    });
                                }

                                if (item.Type == ItemType.Armor && (item.Rarity == ItemRarity.Legendary || (manualItems.Contains(item.Id))))
                                {
                                    var armor = (ItemArmor)item;

                                    itemMapping.Armors.Add(new()
                                    {
                                        Id = armor.Id,
                                        MappedId = itemMapping.Armors.Count,
                                        Name = armor.Name,
                                    });
                                }

                                if (item.Type == ItemType.Weapon && (item.Rarity == ItemRarity.Legendary || (manualItems.Contains(item.Id))))
                                {
                                    var weapon = (ItemWeapon)item;

                                    itemMapping.Weapons.Add(new()
                                    {
                                        Id = weapon.Id,
                                        MappedId = itemMapping.Weapons.Count,
                                        Name = weapon.Name,
                                    });
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
                                                MappedId = itemMapping.Nourishments.Count,
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
                                                MappedId = itemMapping.Utilities.Count,
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
                                                MappedId = itemMapping.PvpRunes.Count,
                                                Name = upgrade.Name,
                                            });
                                        }
                                        else if (upgrade.GameTypes.ToList().Contains(ItemGameType.Pve))
                                        {
                                            itemMapping.PveRunes.Add(new()
                                            {
                                                Id = upgrade.Id,
                                                MappedId = itemMapping.PveRunes.Count,
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
                                                MappedId = itemMapping.PvpSigils.Count,
                                                Name = upgrade.Name,
                                            });
                                        }
                                        else if (upgrade.GameTypes.ToList().Contains(ItemGameType.Pve))
                                        {
                                            itemMapping.PveSigils.Add(new()
                                            {
                                                Id = upgrade.Id,
                                                MappedId = itemMapping.PveSigils.Count,
                                                Name = upgrade.Name,
                                            });
                                        }
                                    }

                                    //Infusions
                                    if (upgrade.Details.InfusionUpgradeFlags.ToList().Contains(ItemInfusionFlag.Infusion))
                                    {
                                        itemMapping.Infusions.Add(new()
                                        {
                                            Id = upgrade.Id,
                                            MappedId = itemMapping.Infusions.Count,
                                            Name = upgrade.Name,
                                        });
                                    }

                                    //Enrichments
                                    if (upgrade.Details.InfusionUpgradeFlags.ToList().Contains(ItemInfusionFlag.Enrichment))
                                    {
                                        itemMapping.Enrichments.Add(new()
                                        {
                                            Id = upgrade.Id,
                                            MappedId = itemMapping.Enrichments.Count,
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

                Debug.WriteLine($"All {raw_itemids.Count} items fetched!");

                string finaljson = JsonConvert.SerializeObject(itemMapping, Formatting.Indented);
                File.WriteAllText($@"{Paths.ModulePath}\data\ItemMapping.json", finaljson);
            }
            catch (RequestException ex)
            {

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception {ex}");
            }
        }
    }
}
