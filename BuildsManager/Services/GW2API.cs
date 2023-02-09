using Blish_HUD;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.WebApi;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.BuildsManager.DataModels;
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

            while (!done)
            {
                done = state[Locale.English] != null && state[Locale.Spanish] != null && state[Locale.German] != null && state[Locale.French] != null;

                Locale locale = GameService.Overlay.UserLocale.Value;

                if (state[locale] == null)
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
                        item.Apply((ItemArmor) i);
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

                    state[locale] = "Done";
                }

                await Task.Delay(500);

                Debug.WriteLine($"English: {state[Locale.English] ?? "Not Done"} | Spanish: {state[Locale.Spanish] ?? "Not Done"} | German: {state[Locale.German] ?? "Not Done"} | French: {state[Locale.French] ?? "Not Done"} ");
            }
        }
    }
}
