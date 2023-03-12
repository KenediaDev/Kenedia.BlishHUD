using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.Models;
using Gw2Sharp.WebApi;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.DataModels.LegendaryItems;
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
using Kenedia.Modules.BuildsManager.DataModels.ItemUpgrades;
using Pet = Kenedia.Modules.BuildsManager.DataModels.Professions.Pet;
using System.Threading;
using Kenedia.Modules.Core.DataModels;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Gw2Sharp;
using System.IO;
using ApiSkill = Gw2Sharp.WebApi.V2.Models.Skill;
using ApiTraits = Gw2Sharp.WebApi.V2.Models.Trait;
using ApiPet = Gw2Sharp.WebApi.V2.Models.Pet;
using Kenedia.Modules.Core.Extensions;
using System.Diagnostics;
using Gw2Sharp.WebApi.Exceptions;

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

            var armors = _data.Armors;
            var upgrades = _data.Upgrades;
            var trinkets = _data.Trinkets;
            var weapons = _data.Weapons;

            var professions = _data.Professions;
            var races = _data.Races;
            var stats = _data.Stats;
            var sigils = _data.Sigils;
            var runes = _data.Runes;
            var pets = _data.Pets;

            Locale locale = GameService.Overlay.UserLocale.Value;
            //await GetLegendaryItems(_cancellationTokenSource.Token, armors, upgrades, trinkets, weapons);
            //await GetUpgrades(_cancellationTokenSource.Token, sigils, runes);
            await GetProfessions(_cancellationTokenSource.Token, professions, races);
            //await GetStats(_cancellationTokenSource.Token, stats);
            //await GetPets(_cancellationTokenSource.Token, pets);
        }

        public async Task GetLegendaryItems(CancellationToken cancellation, Dictionary<int, LegendaryArmor> armors, Dictionary<int, LegendaryUpgrade> upgrades, Dictionary<int, LegendaryTrinket> trinkets, Dictionary<int, LegendaryWeapon> weapons)
        {
            try
            {
                string json;

                var armor = new List<LegendaryItemId>() {
                        LegendaryItemId.PerfectedEnvoyCowl,
                        LegendaryItemId.PerfectedEnvoyMantle,
                        LegendaryItemId.PerfectedEnvoyVestments,
                        LegendaryItemId.PerfectedEnvoyGloves,
                        LegendaryItemId.PerfectedEnvoyPants,
                        LegendaryItemId.PerfectedEnvoyShoes,
                        LegendaryItemId.PerfectedEnvoyMask,
                        LegendaryItemId.PerfectedEnvoyShoulderpads,
                        LegendaryItemId.PerfectedEnvoyJerkin,
                        LegendaryItemId.PerfectedEnvoyVambraces,
                        LegendaryItemId.PerfectedEnvoyLeggings,
                        LegendaryItemId.PerfectedEnvoyBoots,
                        LegendaryItemId.PerfectedEnvoyHelmet,
                        LegendaryItemId.PerfectedEnvoyPauldrons,
                        LegendaryItemId.PerfectedEnvoyBreastplate,
                        LegendaryItemId.PerfectedEnvoyGauntlets,
                        LegendaryItemId.PerfectedEnvoyTassets,
                        LegendaryItemId.PerfectedEnvoyGreaves,
                    };

                var upgrade = new List<LegendaryItemId>()
                    {
                        LegendaryItemId.LegendarySigil,
                        LegendaryItemId.LegendaryRune,
                    };

                var back = new List<LegendaryItemId>()
                    {
                        LegendaryItemId.LegendaryBackpackAdInfinitum,
                    };

                var trinket = new List<LegendaryItemId>()
                    {
                        LegendaryItemId.Aurora,
                        LegendaryItemId.Vision,
                        LegendaryItemId.Coalescence,
                        LegendaryItemId.Conflux,
                        LegendaryItemId.PrismaticChampionsRegalia,
                    };

                var weapon = new List<LegendaryItemId>()
                    {
                        LegendaryItemId.TheBifrost,
                        LegendaryItemId.Bolt,
                        LegendaryItemId.TheDreamer,
                        LegendaryItemId.TheFlameseekerProphecies,
                        LegendaryItemId.Frenzy,
                        LegendaryItemId.Meteorlogicus,
                        LegendaryItemId.Frostfang,
                        LegendaryItemId.Howler,
                        LegendaryItemId.Incinerator,
                        LegendaryItemId.TheJuggernaut,
                        LegendaryItemId.Kudzu,
                        LegendaryItemId.Kraitkin,
                        LegendaryItemId.KamohoaliiKotaki,
                        LegendaryItemId.TheMinstrel,
                        LegendaryItemId.TheMoot,
                        LegendaryItemId.ThePredator,
                        LegendaryItemId.Quip,
                        LegendaryItemId.Rodgort,
                        LegendaryItemId.Eternity,
                    };

                var api_upgrades = await _gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(upgrade.Cast<int>(), cancellation);
                var api_backs = await _gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(back.Cast<int>(), cancellation);
                var api_weapons = await _gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(weapon.Cast<int>(), cancellation);
                var api_trinkets = await _gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(trinket.Cast<int>(), cancellation);
                var api_armors = await _gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(armor.Cast<int>(), cancellation);
                if (cancellation.IsCancellationRequested) return;

                foreach (var i in api_armors)
                {
                    bool exists = armors.TryGetValue(i.Id, out var item);
                    item ??= new();
                    item.Apply((ItemArmor)i);
                    if (!exists) armors.Add(i.Id, item);
                }
                json = JsonConvert.SerializeObject(armors, Formatting.Indented);
                File.WriteAllText($@"{Paths.ModulePath}\data\Armors.json", json);

                foreach (var i in api_upgrades)
                {
                    bool exists = upgrades.TryGetValue(i.Id, out var item);
                    item ??= new();
                    item.Apply((ItemUpgradeComponent)i);
                    if (!exists) upgrades.Add(i.Id, item);
                }
                json = JsonConvert.SerializeObject(upgrades, Formatting.Indented);
                File.WriteAllText($@"{Paths.ModulePath}\data\Upgrades.json", json);

                foreach (var i in api_backs)
                {
                    bool exists = trinkets.TryGetValue(i.Id, out var item);
                    item ??= new();
                    item.Apply((ItemBack)i);
                    if (!exists) trinkets.Add(i.Id, item);
                }
                foreach (var i in api_trinkets)
                {
                    bool exists = trinkets.TryGetValue(i.Id, out var item);
                    item ??= new();
                    item.Apply((ItemTrinket)i);
                    if (!exists) trinkets.Add(i.Id, item);
                }
                json = JsonConvert.SerializeObject(trinkets, Formatting.Indented);
                File.WriteAllText($@"{Paths.ModulePath}\data\Trinkets.json", json);

                foreach (var i in api_weapons)
                {
                    bool exists = weapons.TryGetValue(i.Id, out var item);
                    item ??= new();
                    item.Apply((ItemWeapon)i);
                    if (!exists) weapons.Add(i.Id, item);
                }
                json = JsonConvert.SerializeObject(weapons, Formatting.Indented);
                File.WriteAllText($@"{Paths.ModulePath}\data\Weapons.json", json);
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

        public async Task GetUpgrades(CancellationToken cancellation, Dictionary<int, Sigil> sigils, Dictionary<int, Rune> runes)
        {
            try
            {
                var api_sigils = await _gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(_data.SigilIds, cancellation);
                var api_runes = await _gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(_data.RuneIds, cancellation);
                if (cancellation.IsCancellationRequested) return;

                foreach (var s in api_sigils.Cast<ItemUpgradeComponent>())
                {
                    bool exists = sigils.TryGetValue(s.Id, out var sigil);
                    sigil ??= new(s);

                    if (!exists)
                    {
                        sigils.Add(s.Id, sigil);
                    }
                    else
                    {
                        sigil.ApplyLanguage(s);
                    }
                }

                foreach (var r in api_runes.Cast<ItemUpgradeComponent>())
                {
                    bool exists = runes.TryGetValue(r.Id, out var rune);
                    rune ??= new(r);

                    if (!exists)
                    {
                        runes.Add(r.Id, rune);
                    }
                    else
                    {
                        rune.ApplyLanguage(r);
                    }
                }

                string json = JsonConvert.SerializeObject(sigils, Formatting.Indented);
                File.WriteAllText($@"{Paths.ModulePath}\data\Sigils.json", json);

                json = JsonConvert.SerializeObject(runes, Formatting.Indented);
                File.WriteAllText($@"{Paths.ModulePath}\data\Runes.json", json);
            }
            catch (Exception ex)
            {
                if (!cancellation.IsCancellationRequested)
                {
                    _logger.Warn($"Failed to fetch Upgrades.");
                    _logger.Warn($"{ex}");
                }
            }
        }

        public async Task GetStats(CancellationToken cancellation, Dictionary<int, Stat> stats)
        {
            try
            {
                var legy = (ItemTrinket)await _gw2ApiManager.Gw2ApiClient.V2.Items.GetAsync(81908, cancellation);

                var apiStats = await _gw2ApiManager.Gw2ApiClient.V2.Itemstats.AllAsync(cancellation);
                if (cancellation.IsCancellationRequested) return;

                foreach (var e in apiStats)
                {
                    if (legy.Details.StatChoices.Contains(e.Id))
                    {
                        bool exists = stats.TryGetValue(e.Id, out var stat);
                        stat ??= new(e);

                        stat.ApplyLanguage(e);
                        if (!exists)
                        {
                            stat.MappedId = stats.Count();
                            stats.Add(e.Id, stat);
                        }
                    }
                }

                string json = JsonConvert.SerializeObject(stats, Formatting.Indented);
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

                _ = itemids.RemoveAll(invalidIds.Contains);
                _ = itemids.RemoveAll(jadetechmodules.Contains);

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

                                if (item.Type == ItemType.Trinket && item.Rarity == ItemRarity.Legendary)
                                {
                                    var trinket = (ItemTrinket)item;

                                    itemMapping.Trinkets.Add(new()
                                    {
                                        Id = trinket.Id,
                                        MappedId = itemMapping.Trinkets.Count,
                                        Name = trinket.Name,
                                    });
                                }

                                if (item.Type == ItemType.Back && item.Rarity == ItemRarity.Legendary)
                                {
                                    var back = (ItemBack)item;

                                    itemMapping.Backs.Add(new()
                                    {
                                        Id = back.Id,
                                        MappedId = itemMapping.Backs.Count,
                                        Name = back.Name,
                                    });
                                }

                                if (item.Type == ItemType.Armor && item.Rarity == ItemRarity.Legendary)
                                {
                                    var armor = (ItemArmor)item;

                                    itemMapping.Armors.Add(new()
                                    {
                                        Id = armor.Id,
                                        MappedId = itemMapping.Armors.Count,
                                        Name = armor.Name,
                                    });
                                }

                                if (item.Type == ItemType.Weapon && item.Rarity == ItemRarity.Legendary)
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

    public class ItemMapping
    {
        public List<ItemMap> Nourishments = new();
        public List<ItemMap> Utilities = new();
        public List<ItemMap> PveRunes = new();
        public List<ItemMap> PvpRunes = new();
        public List<ItemMap> PveSigils = new();
        public List<ItemMap> PvpSigils = new();
        public List<ItemMap> Infusions = new();
        public List<ItemMap> Enrichments = new();
        public List<ItemMap> Trinkets = new();
        public List<ItemMap> Backs = new();
        public List<ItemMap> Weapons = new();
        public List<ItemMap> Armors = new();
        public List<ItemMap> PowerCore = new();
    }

    public class ItemMap
    {
        public int Id { get; set; }

        public int MappedId { get; set; }

        public string Name { get; set; }
    }
}
