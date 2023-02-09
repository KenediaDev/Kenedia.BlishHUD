using Blish_HUD.Gw2WebApi;
using Blish_HUD;
using Gw2Sharp;
using Gw2Sharp.WebApi;
using Gw2Sharp.WebApi.Caching;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kenedia.Modules.Core.Models
{
    public class CustomApiManager
    {
        private readonly Connection _internalConnection;

        public IConnection Connection => _internalConnection;

        private readonly IGw2WebApiClient _internalClient;

        public CustomApiManager(string accessToken, TokenComplianceMiddleware tokenComplianceMiddle, ICacheMethod webApiCache, ICacheMethod renderCache = null, TimeSpan? renderCacheDuration = null)
        {
            string ua = $"kenedia/{Program.OverlayVersion}";

            _internalConnection = new Connection(accessToken,
                                                 GameService.Overlay.UserLocale.Value,
                                                 webApiCache,
                                                 renderCache,
                                                 renderCacheDuration ?? TimeSpan.MaxValue,
                                                 ua);

            _internalConnection.Middleware.Add(tokenComplianceMiddle);

            _internalClient = new Gw2Client(_internalConnection).WebApi;

            SetupListeners();
        }

        private void SetupListeners()
        {
            GameService.Overlay.UserLocale.SettingChanged += UserLocaleOnSettingChanged;
        }

        private void UserLocaleOnSettingChanged(object sender, ValueChangedEventArgs<Locale> e)
        {
            _internalConnection.Locale = e.NewValue;

        }

        public bool SetApiKey(string apiKey)
        {
            if (string.Equals(_internalConnection.AccessToken, apiKey)) return false;

            _internalConnection.AccessToken = apiKey;

            return true;
        }
    }
}
