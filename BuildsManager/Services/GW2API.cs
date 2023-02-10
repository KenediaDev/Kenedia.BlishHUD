﻿using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.Models;
using Gw2Sharp.WebApi;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.DataModels;
using Kenedia.Modules.BuildsManager.DataModels.LegendaryItems;
using Kenedia.Modules.BuildsManager.DataModels.Professions;
using Kenedia.Modules.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File = System.IO.File;
using Profession = Kenedia.Modules.BuildsManager.DataModels.Professions.Profession;
using APIProfession = Gw2Sharp.WebApi.V2.Models.Profession;
using APISpecialization = Gw2Sharp.WebApi.V2.Models.Specialization;
using APITrait = Gw2Sharp.WebApi.V2.Models.Trait;
using APISkill = Gw2Sharp.WebApi.V2.Models.Skill;
using Skill = Kenedia.Modules.BuildsManager.DataModels.Professions.Skill;
using Trait = Kenedia.Modules.BuildsManager.DataModels.Professions.Trait;
using Specialization = Kenedia.Modules.BuildsManager.DataModels.Professions.Specialization;

namespace Kenedia.Modules.BuildsManager.Services
{
    public class GW2API
    {
        private readonly Gw2ApiManager _gw2ApiManager;

        public GW2API(Gw2ApiManager gw2ApiManager)
        {
            _gw2ApiManager = gw2ApiManager;

        }

        public PathCollection Paths { get; set; }

        public async void GetAllDataAllLocales()
        {
            if (Paths == null)
            {
                Debug.WriteLine($"No Paths set for {nameof(GetAllDataAllLocales)}!");
                return;
            }

            Debug.WriteLine($"{nameof(GetAllDataAllLocales)}: Fetch data ...");

            LocalizedString state = new();

            bool done = state[Locale.English] != null && state[Locale.Spanish] != null && state[Locale.German] != null && state[Locale.French] != null;

            var armors = new Dictionary<int, LegendaryArmor>();
            var upgrades = new Dictionary<int, LegendaryUpgrade>();
            var trinkets = new Dictionary<int, LegendaryTrinket>();
            var weapons = new Dictionary<int, LegendaryWeapon>();

            var professions = new Dictionary<ProfessionType, Profession>();

            while (!done)
            {
                done = state[Locale.English] != null && state[Locale.Spanish] != null && state[Locale.German] != null && state[Locale.French] != null;

                Locale locale = GameService.Overlay.UserLocale.Value;

                if (state[locale] == null)
                {
                    //await GetLegendaryItems(armors, upgrades, trinkets, weapons);
                    await GetUpgrades();
                    await GetProfessions(professions);

                    state[locale] = "Done";
                }

                await Task.Delay(500);

                Debug.WriteLine($"English: {state[Locale.English] ?? "Not Done"} | Spanish: {state[Locale.Spanish] ?? "Not Done"} | German: {state[Locale.German] ?? "Not Done"} | French: {state[Locale.French] ?? "Not Done"} ");
            }
        }

        public async Task GetLegendaryItems(Dictionary<int, LegendaryArmor> armors, Dictionary<int, LegendaryUpgrade> upgrades, Dictionary<int, LegendaryTrinket> trinkets, Dictionary<int, LegendaryWeapon> weapons)
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

                var api_armors = await _gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(armor.Cast<int>());
                foreach (var i in api_armors)
                {
                    bool exists = armors.TryGetValue(i.Id, out var item);
                    item ??= new();
                    item.Apply((ItemArmor)i);
                    if (!exists) armors.Add(i.Id, item);
                }
                json = JsonConvert.SerializeObject(armors, Formatting.Indented);
                File.WriteAllText($@"{Paths.ModulePath}\armors.json", json);

                var api_upgrades = await _gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(upgrade.Cast<int>());
                foreach (var i in api_upgrades)
                {
                    bool exists = upgrades.TryGetValue(i.Id, out var item);
                    item ??= new();
                    item.Apply((ItemUpgradeComponent)i);
                    if (!exists) upgrades.Add(i.Id, item);
                }
                json = JsonConvert.SerializeObject(upgrades, Formatting.Indented);
                File.WriteAllText($@"{Paths.ModulePath}\upgrades.json", json);

                var api_backs = await _gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(back.Cast<int>());
                foreach (var i in api_backs)
                {
                    bool exists = trinkets.TryGetValue(i.Id, out var item);
                    item ??= new();
                    item.Apply((ItemBack)i);
                    if (!exists) trinkets.Add(i.Id, item);
                }
                var api_trinkets = await _gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(trinket.Cast<int>());
                foreach (var i in api_trinkets)
                {
                    bool exists = trinkets.TryGetValue(i.Id, out var item);
                    item ??= new();
                    item.Apply((ItemTrinket)i);
                    if (!exists) trinkets.Add(i.Id, item);
                }
                json = JsonConvert.SerializeObject(trinkets, Formatting.Indented);
                File.WriteAllText($@"{Paths.ModulePath}\trinkets.json", json);

                var api_weapons = await _gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(weapon.Cast<int>());
                foreach (var i in api_weapons)
                {
                    weapons.Add(i.Id, new((ItemWeapon)i));
                    bool exists = weapons.TryGetValue(i.Id, out var item);
                    item ??= new();
                    item.Apply((ItemWeapon)i);
                    if (!exists) weapons.Add(i.Id, item);
                }
                json = JsonConvert.SerializeObject(weapons, Formatting.Indented);
                File.WriteAllText($@"{Paths.ModulePath}\weapons.json", json);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        // TODO add getting Upgrades
        public async Task GetUpgrades()
        {
            await Task.Delay(1);
        }

        // TODO add geting Stats
        public async Task GetStats()
        {
            var stats = await _gw2ApiManager.Gw2ApiClient.V2.Itemstats.AllAsync();
        }

        public async Task GetProfessions(Dictionary<ProfessionType, Profession> professions)
        {
            //try
            //{
            var apiSkills = await _gw2ApiManager.Gw2ApiClient.V2.Skills.AllAsync();
            var apiTraits = await _gw2ApiManager.Gw2ApiClient.V2.Traits.AllAsync();
            var apiSpecializations = await _gw2ApiManager.Gw2ApiClient.V2.Specializations.AllAsync();
            var apiProfessions = await _gw2ApiManager.Gw2ApiClient.V2.Professions.AllAsync();

            var skills = new Dictionary<int, Skill>();
            foreach (var skill in apiSkills)
            {
                skills.Add(skill.Id, new(skill));
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

            // TODO adding Revenant Legends
            foreach (var e in apiProfessions)
            {
                if (Enum.TryParse(e.Id, out ProfessionType professionType))
                {
                    bool exists = professions.TryGetValue(professionType, out var profession);
                    profession ??= new();
                    profession.Apply(e, specializations, traits, skills);
                    if (!exists) professions.Add(professionType, profession);
                }
            }

            string json = JsonConvert.SerializeObject(professions, Formatting.Indented);
            File.WriteAllText($@"{Paths.ModulePath}\professions.json", json);
            //}
            //catch (Exception e)
            //{
            //    Debug.WriteLine(e);
            //}
        }
    }
}
