using Blish_HUD.Modules.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kenedia.Modules.BuildsManager.Models;
using Kenedia.Modules.BuildsManager.DataModels.Items;
using System.Reflection;
using WeaponItem = Kenedia.Modules.BuildsManager.DataModels.Items.Weapon;
using Kenedia.Modules.Core.Utility;
using System.Threading;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Attributes;

namespace Kenedia.Modules.BuildsManager.Services
{
    public class Data : IDisposable
    {
        public static readonly Dictionary<int, int?> SkinDictionary = new()
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

        private readonly Paths _paths;
        private readonly Gw2ApiManager _gw2ApiManager;
        private readonly Func<NotificationBadge> _notificationBadge;
        private readonly Func<LoadingSpinner> _spinner;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isDisposed;

        public Data(Paths paths, Gw2ApiManager gw2ApiManager, Func<Core.Controls.NotificationBadge> notificationBadge, Func<LoadingSpinner> spinner)
        {
            _paths = paths;
            _gw2ApiManager = gw2ApiManager;
            _notificationBadge = notificationBadge;
            _spinner = spinner;
        }

        public event EventHandler Loaded;

        public bool IsLoaded
        {
            get
            {
                foreach (var (name, map) in this)
                {
                    if (!map.IsLoaded)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public double LastLoadAttempt { get; private set; } = double.MinValue;

        [EnumeratorMember]
        public ProfessionDataEntry Professions { get; } = new();

        [EnumeratorMember]
        public RaceDataEntry Races { get; } = new();

        [EnumeratorMember]
        public PetDataEntry Pets { get; } = new();

        [EnumeratorMember]
        public StatMappedDataEntry Stats { get; } = new();

        [EnumeratorMember]
        public PvpAmuletMappedDataEntry PvpAmulets { get; } = new();

        [EnumeratorMember]
        public ItemMappedDataEntry<Armor> Armors { get; } = new();

        [EnumeratorMember]
        public ItemMappedDataEntry<WeaponItem> Weapons { get; } = new();

        [EnumeratorMember]
        public ItemMappedDataEntry<Trinket> Trinkets { get; } = new();

        [EnumeratorMember]
        public ItemMappedDataEntry<Trinket> Backs { get; } = new();

        [EnumeratorMember]
        public ItemMappedDataEntry<Sigil> PvpSigils { get; } = new();

        [EnumeratorMember]
        public ItemMappedDataEntry<Sigil> PveSigils { get; } = new();

        [EnumeratorMember]
        public ItemMappedDataEntry<Rune> PvpRunes { get; } = new();

        [EnumeratorMember]
        public ItemMappedDataEntry<Rune> PveRunes { get; } = new();

        [EnumeratorMember]
        public ItemMappedDataEntry<PowerCore> PowerCores { get; } = new();

        [EnumeratorMember]
        public ItemMappedDataEntry<Relic> Relics { get; } = new();

        [EnumeratorMember]
        public ItemMappedDataEntry<Enhancement> Enhancements { get; } = new();

        [EnumeratorMember]
        public ItemMappedDataEntry<Nourishment> Nourishments { get; } = new();

        [EnumeratorMember]
        public ItemMappedDataEntry<Infusion> Infusions { get; } = new();

        [EnumeratorMember]
        public ItemMappedDataEntry<Enrichment> Enrichments { get; } = new();

        public IEnumerator<(string name, BaseMappedDataEntry map)> GetEnumerator()
        {
            var propertiesToEnumerate = GetType()
                .GetProperties()
                .Where(property => property.GetCustomAttribute<EnumeratorMemberAttribute>() != null);

            foreach (var property in propertiesToEnumerate)
            {
                yield return (property.Name, property.GetValue(this) as BaseMappedDataEntry);
            }
        }

        public async Task<bool> Load(bool force)
        {
            if (force)
            {
                LastLoadAttempt = double.MinValue;
            }

            return await Load();
        }

        public async Task<bool> Load()
        {
            // Don't try to load more than once every 3 minutes
            if (Common.Now - LastLoadAttempt <= 180000)
            {
                return false;
            }

            LoadingSpinner spinner = _spinner?.Invoke();
            LastLoadAttempt = Common.Now;

            BuildsManager.Logger.Info("Loading data");

            try
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();

                StaticVersion versions = await StaticHosting.GetStaticVersion();
                if (versions is null)
                {
                    if (_notificationBadge?.Invoke() is NotificationBadge badge)
                    {
                        badge.Show();
                        badge.SetLocalizedText = () => $"Failed to get the version file. Retry at {DateTime.Now.AddMinutes(3):T}";
                    }

                    spinner?.Hide();
                    return false;
                }

                bool failed = false;

                string loadStatus = string.Empty;
                foreach (var (name, map) in this)
                {
                    string path = Path.Combine(_paths.ModuleDataPath, $"{name}.json");
                    bool success = await map.LoadAndUpdate(name, versions[name], path, _gw2ApiManager, _cancellationTokenSource.Token);
                    failed = failed || !success;

                    loadStatus += $"{Environment.NewLine}{name}: {success} [{map?.Version?.ToString() ?? "0.0.0"} | {versions[name]}] ";
                }

                if (!failed)
                {
                    BuildsManager.Logger.Info("All data loaded!");
                    Loaded?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    if (_notificationBadge?.Invoke() is NotificationBadge badge)
                    {
                        badge.Show();
                        string txt = $"Failed to load some data. Click to retry.{Environment.NewLine}Automatic retry at {DateTime.Now.AddMinutes(3):T}{loadStatus}";
                        badge.SetLocalizedText = () => txt;
                        BuildsManager.Logger.Info(txt);
                    }
                }

                spinner?.Hide();

                return !failed;
            }
            catch
            {

            }

            return false;
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            foreach (var (_, map) in this)
            {
                map?.Dispose();
            }
        }
    }
}
