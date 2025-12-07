using Blish_HUD.Modules.Managers;
using Kenedia.Modules.Core.Attributes;
using Kenedia.Modules.OverflowTradingAssist.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.Core.Models;

namespace Kenedia.Modules.OverflowTradingAssist.Services
{
    public class Data
    {
        private readonly Gw2ApiManager _gw2ApiManager;
        private readonly Func<NotificationBadge> _notificationBadge;
        private readonly Func<LoadingSpinner> _spinner;
        private readonly Paths _paths;
        private CancellationTokenSource _cancellationTokenSource;

        private StatusType _dataStatus = StatusType.None;

        public Data(Paths paths, Gw2ApiManager gw2ApiManager, Func<Core.Controls.NotificationBadge> notificationBadge, Func<LoadingSpinner> spinner, StaticHosting staticHosting)
        {
            _paths = paths;
            _gw2ApiManager = gw2ApiManager;
            _notificationBadge = notificationBadge;
            _spinner = spinner;
            StaticHosting = staticHosting;
        }

        public event EventHandler Loaded;

        [EnumeratorMember]
        public ItemsData Items { get; set; } = new();
        
        public bool IsLoaded
        {
            get
            {
                foreach (var (_, map) in this)
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
        public StaticHosting StaticHosting { get; }

        public IEnumerator<(string name, ItemsData map)> GetEnumerator()
        {
            var propertiesToEnumerate = GetType()
                .GetProperties()
                .Where(property => property.GetCustomAttribute<EnumeratorMemberAttribute>() != null);

            foreach (var property in propertiesToEnumerate)
            {
                yield return (property.Name, property.GetValue(this) as ItemsData);
            }
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

            OverflowTradingAssist.Logger.Info("Loading data");

            try
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();

                StaticVersion versions = await StaticHosting.GetStaticVersion();

                if (versions is null)
                {
                    if (_notificationBadge?.Invoke() is NotificationBadge badge)
                    {
                        var endTime = DateTime.Now.AddMinutes(3);
                        badge.AddNotification(new($"Failed to get the version file. Retry at {DateTime.Now.AddMinutes(3):T}", () => DateTime.Now >= endTime || _dataStatus == StatusType.Success));
                    }

                    _dataStatus = StatusType.Error;
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
                    _dataStatus = StatusType.Success;
                }
                else
                {
                    if (_notificationBadge?.Invoke() is NotificationBadge badge)
                    {
                        _dataStatus = StatusType.Error;
                        string txt = $"Failed to load some data. Click to retry.{Environment.NewLine}Automatic retry at {DateTime.Now.AddMinutes(3):T}{loadStatus}";

                        var endTime = DateTime.Now.AddMinutes(3);
                        badge.AddNotification(new(txt, () => DateTime.Now >= endTime || _dataStatus == StatusType.Success));

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
