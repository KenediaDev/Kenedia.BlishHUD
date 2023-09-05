using Blish_HUD.Modules.Managers;
using Kenedia.Modules.Core.Attributes;
using Kenedia.Modules.OverflowTradingAssist.DataEntries;
using Kenedia.Modules.OverflowTradingAssist.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.OverflowTradingAssist.DataModels;

namespace Kenedia.Modules.OverflowTradingAssist.Services
{
    public class Data
    {
        private readonly Gw2ApiManager _gw2ApiManager;
        private readonly Func<NotificationBadge> _notificationBadge;
        private readonly Func<LoadingSpinner> _spinner;
        private readonly Paths _paths;
        private CancellationTokenSource _cancellationTokenSource;
                
        public Data(Paths paths, Gw2ApiManager gw2ApiManager, Func<Core.Controls.NotificationBadge> notificationBadge, Func<LoadingSpinner> spinner)
        {
            _paths = paths;
            _gw2ApiManager = gw2ApiManager;
            _notificationBadge = notificationBadge;
            _spinner = spinner;
        }

        public event EventHandler Loaded;

        [EnumeratorMember]
        public static DataEntry<Item> Items { get; set; } = new();

        public bool IsLoaded { get; internal set; }
        public double LastLoadAttempt { get; private set; }

        public IEnumerator<(string name, DataEntry<object> map)> GetEnumerator()
        {
            var propertiesToEnumerate = GetType()
                .GetProperties()
                .Where(property => property.GetCustomAttribute<EnumeratorMemberAttribute>() != null);

            foreach (var property in propertiesToEnumerate)
            {
                yield return (property.Name, property.GetValue(this) as DataEntry<object>);
            }
        }

        public async Task<bool> Load()
        {
            if (Common.Now - LastLoadAttempt <= 180000)
            {
                return false;
            }

            LoadingSpinner spinner = _spinner?.Invoke();

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
                    bool success = await map?.LoadAndUpdate(name, versions[name], path, _gw2ApiManager, _cancellationTokenSource.Token);
                    failed = failed || !success;

                    loadStatus += $"{Environment.NewLine}{name}: {success} [{map?.Version?.ToString() ?? "0.0.0"} | {versions[name]}] ";
                }

                if (!failed)
                {
                    OverflowTradingAssist.Logger.Info("All data loaded!");
                    Loaded?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    if (_notificationBadge?.Invoke() is NotificationBadge badge)
                    {
                        badge.Show();
                        string txt = $"Failed to load some data. Click to retry.{Environment.NewLine}Automatic retry at {DateTime.Now.AddMinutes(3):T}{loadStatus}";
                        badge.SetLocalizedText = () => txt;
                        OverflowTradingAssist.Logger.Info(txt);
                    }
                }

                spinner?.Hide();
                return true;
            }
            catch
            {
            }

            return false;
        }

        public async Task<bool> Load(bool force)
        {
            if (force)
            {
                LastLoadAttempt = double.MinValue;
            }

            return await Load();
        }
    }
}
