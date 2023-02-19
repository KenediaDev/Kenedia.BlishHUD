using Blish_HUD.Controls;
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

namespace Kenedia.Modules.Characters.Services
{
    public class GW2API_Handler
    {
        private readonly Logger _logger = Logger.GetLogger(typeof(GW2API_Handler));
        private readonly Gw2ApiManager _gw2ApiManager;
        private readonly Action<IApiV2ObjectList<Character>> _callBack;
        private readonly Data _data;
        private readonly Func<LoadingSpinner> _getSpinner;
        private readonly PathCollection _paths;
        private readonly string _accountFilePath;

        private CancellationTokenSource _cancellationTokenSource;

        public GW2API_Handler(Gw2ApiManager gw2ApiManager, Action<IApiV2ObjectList<Character>> callBack, Func<LoadingSpinner> getSpinner, PathCollection paths, Data data)
        {
            _gw2ApiManager = gw2ApiManager;
            _callBack = callBack;
            _getSpinner = getSpinner;
            _paths = paths;
            _accountFilePath = paths.ModulePath + @"\accounts.json";
            _data = data;
        }

        private Account _account;

        public Account Account
        {
            get => _account;
            set
            {
                _paths.AccountName = value.Name;
                _account = value;
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
                    accounts = JsonConvert.DeserializeObject<List<AccountSummary>>(content);
                    accountEntry = accounts.Find(e => e.AccountName == account.Name);

                    if (accountEntry != null)
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

                string json = JsonConvert.SerializeObject(accounts, Formatting.Indented);
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
                        ScreenNotification.ShowNotification("[Characters]: " + strings.Error_InvalidPermissions, ScreenNotification.NotificationType.Error);
                        Characters.Logger.Error(strings.Error_InvalidPermissions);
                    }

                    Reset(cancellationToken, !cancellationToken.IsCancellationRequested);
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    Characters.Logger.Warn(ex, strings.Error_FailedAPIFetch);
                }

                Reset(cancellationToken, !cancellationToken.IsCancellationRequested);
                return false;
            }
        }

        public async Task FetchLocale(Locale? locale = null, bool force = false)
        {
            locale ??= GameService.Overlay.UserLocale.Value;

            if (force || _data.Maps.Count == 0 || !_data.Maps.FirstOrDefault().Value.Names.TryGetValue((Locale) locale, out string name) || string.IsNullOrEmpty(name))
            {
                Characters.Logger.Info($"No data for {(Locale)locale} loaded yet. Fetching new data from the API.");
                await GetMaps();
            }
        }

        public async Task GetMaps()
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

            string json = JsonConvert.SerializeObject(_maps, Formatting.Indented);
            File.WriteAllText($@"{_paths.ModuleDataPath}\Maps.json", json);
        }
    }
}
