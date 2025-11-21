using Blish_HUD.Modules.Managers;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.OverflowTradingAssist.Models;
using Kenedia.Modules.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kenedia.Modules.Core.Utility;
using Newtonsoft.Json;
using Kenedia.Modules.Core.Extensions;
using System.Diagnostics;

namespace Kenedia.Modules.OverflowTradingAssist.Services
{
    public class TradeFileService
    {
        private Paths _paths;
        private Gw2ApiManager _gw2ApiManager;
        private Func<NotificationBadge> _notificationBadge;
        private Func<LoadingSpinner> _loadingSpinner;
        private List<Trade> _trades;

        public bool IsReady { get; private set; }

        public bool IsLoaded { get; internal set; }

        public event EventHandler Loaded;

        private StatusType _fileStatus;
        private double _lastSave;

        public TradeFileService(Paths paths, Gw2ApiManager gw2ApiManager, Func<NotificationBadge> notificationBadge, Func<LoadingSpinner> loadingSpinner, List<Trade> trades)
        {
            _paths = paths;
            _gw2ApiManager = gw2ApiManager;
            _notificationBadge = notificationBadge;
            _loadingSpinner = loadingSpinner;
            _trades = trades;
        }

        public async Task<List<Trade>> Load()
        {
            //if (!OverflowTradingAssist.ModuleInstance.Settings.TradesInitialized.Value)
            //{
            //    if (await InitializeFile() is List<Trade> trades)
            //    {
            //        OverflowTradingAssist.ModuleInstance.Settings.TradesInitialized.Value = true;
            //        return trades;
            //    }
            //}

            return await LoadTrades();
        }

        public async Task<List<Trade>> LoadTrades()
        {
            if (await EnsureFilePath())
            {
                try
                {
                    Debug.WriteLine($"{nameof(LoadTrades)}");
                    if (await FileExtension.WaitForFileUnlock(_paths.TradeFile))
                    {

                        if (File.Exists(_paths.TradeFile))
                        {
                            string json = File.ReadAllText(_paths.TradeFile);

                            var savedTrades = JsonConvert.DeserializeObject<List<Trade>>(json, SerializerSettings.Default);
                            IsLoaded = true;
                            _fileStatus = StatusType.Success;

                            string txt = $"Loaded {_paths.TradeFile}.";
                            OverflowTradingAssist.Logger.Debug(txt);

                            return savedTrades;
                        }
                        else
                        {
                            string txt = $"No file exists at path {_paths.TradeFile}.";
                            OverflowTradingAssist.Logger.Debug(txt);
                            IsLoaded = true;
                            _fileStatus = StatusType.Success;
                        }

                        return [];
                    }
                    else
                    {
                        _fileStatus = StatusType.Warning;
                        string txt = $"Failed to load {_paths.TradeFile}. File is locked.";

                        OverflowTradingAssist.Logger.Warn(txt);

                        if (_notificationBadge() is NotificationBadge notificationBadge)
                        {
                            notificationBadge.AddNotification(new(() => txt, () => _fileStatus == StatusType.Success));
                        }
                    }
                }
                catch (Exception ex)
                {
                    string txt = $"Failed to load {_paths.TradeFile}.";
                    _fileStatus = StatusType.Error;

                    OverflowTradingAssist.Logger.Warn(ex, txt);

                    if (_notificationBadge() is NotificationBadge notificationBadge)
                    {
                        notificationBadge.AddNotification(new(() => txt, () => _fileStatus == StatusType.Success));
                    }
                }
            }

            return null;
        }

        public async Task<List<Trade>> InitializeFile()
        {
            if (await EnsureFilePath())
            {
                try
                {
                    Debug.WriteLine($"{nameof(InitializeFile)}");
                    var trades = new List<Trade>();

                    foreach (var trade in trades)
                    {
                        if (trade.Items.Count == 0)
                        {
                            trade.Items.Add(new ItemAmount()
                            {
                                Value = trade.Value,
                            });
                        }

                        if (trade.Payment.Count == 0)
                        {
                            trade.Payment.Add(new ItemAmount()
                            {
                                Value = trade.Value,
                            });
                        }
                    }

                    string json = JsonConvert.SerializeObject(trades, SerializerSettings.Default);
                    File.WriteAllText(_paths.TradeFile, json);

                    string txt = $"Saved changes to {_paths.TradeFile}.";
                    OverflowTradingAssist.Logger.Debug(txt);
                    OverflowTradingAssist.ModuleInstance.Settings.TradesInitialized.Value = true;

                    _fileStatus = StatusType.Success;
                    return trades;
                }
                catch (Exception ex)
                {
                    string txt = $"Failed to initialize trades from {_paths.TradeFile}.";
                    _fileStatus = StatusType.Error;

                    OverflowTradingAssist.Logger.Warn(ex, txt);

                    if (_notificationBadge() is NotificationBadge notificationBadge)
                    {
                        notificationBadge.AddNotification(new(() => txt, () => _fileStatus == StatusType.Success));
                    }
                }
            }

            return null;
        }

        public async Task SaveChanges()
        {
            if (Common.Now - _lastSave > 1000)
            {
                _lastSave = Common.Now;

                if (_trades is List<Trade> trades && await EnsureFilePath())
                {
                    var tradesToRemove = trades.Where(e => e.TradeDeleteRequested).ToDictionary(e => e.Id.ToString(), e => e);
                    var tradesToUpdate = trades.Where(e => e.TradeSaveRequested).ToDictionary(e => e.Id.ToString(), e => e);

                    if (tradesToUpdate.Count > 0 || tradesToRemove.Count > 0)
                    {
                        try
                        {
                            Debug.WriteLine($"Json - {nameof(SaveChanges)}");
                            Debug.WriteLine($"Json - Remove {tradesToRemove.Count} trades");
                            Debug.WriteLine($"Json - Update {tradesToUpdate.Count} trades");

                            _ = trades.RemoveAll(tradesToRemove.Values.Contains);

                            if (await FileExtension.WaitForFileUnlock(_paths.TradeFile))
                            {
                                string json = JsonConvert.SerializeObject(trades, SerializerSettings.Default);
                                File.WriteAllText(_paths.TradeFile, json);

                                string txt = $"Saved changes to {_paths.TradeFile}.";
                                OverflowTradingAssist.Logger.Debug(txt);

                                trades.ForEach(e => e.TradeSaveRequested = false);
                                _fileStatus = StatusType.Success;
                            }
                            else
                            {
                                _fileStatus = StatusType.Warning;
                                string txt = $"Failed to save changes to {_paths.TradeFile}. File is locked.";

                                OverflowTradingAssist.Logger.Warn(txt);

                                if (_notificationBadge() is NotificationBadge notificationBadge)
                                {
                                    notificationBadge.AddNotification(new(() => txt, () => _fileStatus == StatusType.Success));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            string txt = $"Failed to save changes to {_paths.TradeFile}.";
                            _fileStatus = StatusType.Error;

                            OverflowTradingAssist.Logger.Warn(ex, txt);

                            if (_notificationBadge() is NotificationBadge notificationBadge)
                            {
                                notificationBadge.AddNotification(new(() => txt, () => _fileStatus == StatusType.Success));
                            }
                        }
                    }
                }
            }
        }

        public async Task<bool> EnsureFilePath()
        {
            if (!IsReady)
            {
                try
                {
                    Debug.WriteLine($"{nameof(EnsureFilePath)}");
                    var account = await _gw2ApiManager.Gw2ApiClient.V2.Account.GetAsync();

                    if (account?.Name is not null)
                    {
                        _paths.AccountName = account.Name;
                        IsReady = true;
                        return IsReady;
                    }
                    else
                    {
                        OverflowTradingAssist.Logger.Warn("Failed to update account name. Account is null.");
                    }
                }
                catch (Exception ex)
                {
                    OverflowTradingAssist.Logger.Warn(ex, "Failed to update subtoken.");

                    if (_notificationBadge() is NotificationBadge notificationBadge)
                    {
                        notificationBadge.AddNotification(new(() => "Failed to update subtoken.", () => _fileStatus == StatusType.Success));
                        return false;
                    }
                }
            }

            return IsReady;
        }
    }
}
