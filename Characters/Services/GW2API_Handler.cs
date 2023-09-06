using Blish_HUD.Modules.Managers;
using Kenedia.Modules.Characters.Res;
using Gw2Sharp.WebApi.V2;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Characters.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using File = System.IO.File;
using Gw2Sharp.WebApi;
using Map = Kenedia.Modules.Core.DataModels.Map;
using Blish_HUD;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Characters.Views;
using Kenedia.Modules.Core.Utility;
using System.ComponentModel;
using Gw2Sharp.WebApi.Exceptions;
using Kenedia.Modules.Core.Controls;
using System.Text.RegularExpressions;
using Kenedia.Modules.Core.Res;
using LoadingSpinner = Kenedia.Modules.Core.Controls.LoadingSpinner;

namespace Kenedia.Modules.Characters.Services
{
    public class GW2API_Handler
    {
        private readonly Logger _logger = Logger.GetLogger(typeof(GW2API_Handler));
        private readonly Gw2ApiManager _gw2ApiManager;
        private readonly Action<IApiV2ObjectList<Character>> _callBack;
        private readonly Data _data;
        private readonly Func<Core.Controls.NotificationBadge> _notificationBadge;
        private readonly Func<LoadingSpinner> _getSpinner;
        private readonly PathCollection _paths;
        private readonly string _accountFilePath;
        private double _lastApiCheck = double.MinValue;

        private CancellationTokenSource _cancellationTokenSource;

        public GW2API_Handler(Gw2ApiManager gw2ApiManager, Action<IApiV2ObjectList<Character>> callBack, Func<LoadingSpinner> getSpinner, PathCollection paths, Data data, Func<Core.Controls.NotificationBadge> notificationBadge)
        {
            _gw2ApiManager = gw2ApiManager;
            _callBack = callBack;
            _getSpinner = getSpinner;
            _paths = paths;
            _accountFilePath = paths.ModulePath + @"\accounts.json";
            _data = data;
            _notificationBadge = notificationBadge;
        }

        public MainWindow MainWindow { get; set; }

        public event PropertyChangedEventHandler AccountChanged;

        private Account _account;
        private Exception _lastException;

        public Account Account
        {
            get => _account;
            set
            {
                var temp = _account;
                if (Common.SetProperty(ref _account, value, AccountChanged))
                {
                    _paths.AccountName = value?.Name;
                    Characters.Logger.Info($"Account changed from {temp?.Name ?? "No Account"} to {value?.Name ?? "No Account"}!");
                }
            }
        }

        private void UpdateAccountsList(Account account, IApiV2ObjectList<Character> characters)
        {
            try
            {
                List<AccountSummary> accounts = new();
                AccountSummary accountEntry;

                if (File.Exists(_accountFilePath))
                {
                    string content = File.ReadAllText(_accountFilePath);
                    accounts = JsonConvert.DeserializeObject<List<AccountSummary>>(content, SerializerSettings.Default);
                    accountEntry = accounts.Find(e => e.AccountName == account.Name);

                    if (accountEntry is not null)
                    {
                        accountEntry.AccountName = account.Name;
                        accountEntry.CharacterNames = new();
                        characters.ToList().ForEach(c => accountEntry.CharacterNames.Add(c.Name));
                    }
                    else
                    {
                        accounts.Add(accountEntry = new()
                        {
                            AccountName = account.Name,
                            CharacterNames = new(),
                        });
                        characters.ToList().ForEach(c => accountEntry.CharacterNames.Add(c.Name));
                    }
                }
                else
                {
                    accounts.Add(accountEntry = new()
                    {
                        AccountName = account.Name,
                        CharacterNames = new(),
                    });
                    characters.ToList().ForEach(c => accountEntry.CharacterNames.Add(c.Name));
                }

                string json = JsonConvert.SerializeObject(accounts, SerializerSettings.Default);
                File.WriteAllText(_accountFilePath, json);
            }
            catch { }
        }

        private void Reset(CancellationToken cancellationToken, bool hideSpinner = false)
        {
            if (hideSpinner) _getSpinner?.Invoke()?.Hide();
            if (cancellationToken != null && cancellationToken.IsCancellationRequested)
            {
                Characters.Logger.Info($"Canceled API Data fetch!");
            }

            _cancellationTokenSource = null;
        }

        public async Task<bool> CheckAPI()
        {
            _cancellationTokenSource?.Cancel();

            _cancellationTokenSource = new();
            var cancellationToken = _cancellationTokenSource.Token;
            _getSpinner?.Invoke()?.Show();

            if (_notificationBadge() is NotificationBadge notificationBadge)
            {
                notificationBadge.Visible = false;
            }

            try
            {
                Characters.Logger.Info($"Fetching new API Data ...");

                if (_gw2ApiManager.HasPermissions(new[] { TokenPermission.Account, TokenPermission.Characters }))
                {
                    Account account = await _gw2ApiManager.Gw2ApiClient.V2.Account.GetAsync(cancellationToken);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        Reset(cancellationToken, !cancellationToken.IsCancellationRequested);
                        return false;
                    }

                    Account = account;
                    Characters.Logger.Info($"Fetching characters for '{Account.Name}' ...");

                    IApiV2ObjectList<Character> characters = await _gw2ApiManager.Gw2ApiClient.V2.Characters.AllAsync(cancellationToken);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        Reset(cancellationToken, !cancellationToken.IsCancellationRequested);
                        return false;
                    }

                    UpdateAccountsList(account, characters);

                    _callBack?.Invoke(characters);
                    Reset(cancellationToken, !cancellationToken.IsCancellationRequested);
                    return true;
                }
                else
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        Characters.Logger.Warn(strings.Error_InvalidPermissions);
                        MainWindow?.SendAPIPermissionNotification();
                        HandleAPIExceptions(new Gw2ApiInvalidPermissionsException());
                    }

                    Reset(cancellationToken, !cancellationToken.IsCancellationRequested);
                    return false;
                }
            }
            catch (UnexpectedStatusException ex)
            {
                HandleAPIExceptions(ex);
                MainWindow?.SendAPITimeoutNotification();
                Characters.Logger.Warn(ex, strings.APITimeoutNotification);
                Reset(cancellationToken, !cancellationToken.IsCancellationRequested);
                return false;
            }
            catch (Exception ex)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    _logger.Warn(ex, strings.Error_FailedAPIFetch);
                }

                HandleAPIExceptions(ex);

                Reset(cancellationToken, !cancellationToken.IsCancellationRequested);
                return false;
            }
        }

        public async Task FetchLocale(Locale? locale = null, bool force = false)
        {
            locale ??= GameService.Overlay.UserLocale.Value;

            if (force || _data.Maps.Count == 0 || !_data.Maps.FirstOrDefault().Value.Names.TryGetValue((Locale)locale, out string name) || string.IsNullOrEmpty(name))
            {
                Characters.Logger.Info($"No data for {(Locale)locale} loaded yet. Fetching new data from the API.");
                await GetMaps();
            }
        }

        public async Task GetMaps()
        {
            if (_notificationBadge() is NotificationBadge notificationBadge)
            {
                notificationBadge.Visible = false;
            }

            try
            {
                var _maps = _data.Maps;
                var maps = await _gw2ApiManager.Gw2ApiClient.V2.Maps.AllAsync();

                foreach (var m in maps)
                {
                    bool exists = _maps.TryGetValue(m.Id, out Map map);

                    map ??= new Map(m);
                    map.Name = m.Name;

                    if (!exists) _maps.Add(m.Id, map);
                }

                string json = JsonConvert.SerializeObject(_maps, SerializerSettings.Default);
                File.WriteAllText($@"{_paths.ModuleDataPath}\Maps.json", json);
            }
            catch (Exception ex)
            {
                _logger.Warn($"Failed to fetch armory items.");
                _logger.Warn($"{ex}");
                HandleAPIExceptions(ex);
            }
        }

        private async void HandleAPIExceptions(Exception ex)
        {
            if (_notificationBadge() is NotificationBadge notificationBadge)
            {
                notificationBadge.Visible = true;

                static string? GetExceptionMessage(Exception ex)
                {
                    string lineBreakPattern = @"<\/h[0-9]>";
                    string lineBreakReplacement = Environment.NewLine;
                    string result = Regex.Replace(ex.Message ?? string.Empty, lineBreakPattern, lineBreakReplacement);

                    string pattern = @"<[^>]+>";
                    string replacement = "";

                    result = Regex.Replace(result, pattern, replacement);
                    return string.IsNullOrEmpty(result) ? null : $"\n\n{result}";
                }

                switch (ex)
                {
                    case Gw2ApiInvalidPermissionsException:
                        ex = await TestAPI() ?? ex;
                        break;
                }

                notificationBadge.SetLocalizedText = ex switch
                {
                    ServiceUnavailableException => () => $"{strings_common.GW2API_Unavailable}{GetExceptionMessage(ex)}",
                    RequestException => () => $"{strings_common.GW2API_RequestFailed}{GetExceptionMessage(ex)}",
                    RequestException<string> => () => $"{strings_common.GW2API_RequestFailed}{GetExceptionMessage(ex)}",
                    Gw2ApiInvalidPermissionsException => () => $"{strings.Error_InvalidPermissions}\nIf you have a valid API Key added there are probably issues with the API currently.",
                    _ => () => $"{GetExceptionMessage(ex)}",
                };
            }

            _lastException = ex;
        }

        private async Task<Exception> TestAPI()
        {
            try
            {
                _lastApiCheck = Common.Now;
                var b = await _gw2ApiManager.Gw2ApiClient.V2.Build.GetAsync();

                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
    }
}
