using Blish_HUD.Modules.Managers;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.Core.Utility;
using Kenedia.Modules.OverflowTradingAssist.Models;
using OfficeOpenXml;
using OfficeOpenXml.LoadFunctions.Params;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File = System.IO.File;

namespace Kenedia.Modules.OverflowTradingAssist.Services
{
    public class ExcelService
    {
        private static readonly int s_tradePartner = 1;
        private static readonly int s_tradeAmount = 2;
        private static readonly int s_ItemSummary = 3;
        private static readonly int s_reviewLink = 4;
        private static readonly int s_tradeListingLink = 5;
        private static readonly int s_guuid = 10;

        private readonly Paths _paths;
        private readonly Gw2ApiManager _gw2ApiManager;
        private readonly Func<NotificationBadge> _notificationBadge;
        private readonly Func<LoadingSpinner> _spinner;
        private readonly List<Trade> _trades;

        private StatusType _fileStatus;
        private StatusType _loadTradeStatus;
        private StatusType _saveTradeStatus;

        private double _lastSave = double.MinValue;

        public ExcelService(Paths paths, Gw2ApiManager gw2ApiManager, Func<NotificationBadge> notificationBadge, Func<LoadingSpinner> spinner, List<Trade> trades)
        {
            _paths = paths;
            _gw2ApiManager = gw2ApiManager;
            _notificationBadge = notificationBadge;
            _spinner = spinner;
            _trades = trades;

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public event EventHandler Loaded;

        public bool IsReady { get; private set; }

        public bool IsLoaded { get; private set; }

        public async Task<bool> EnsureFileExists()
        {
            if (!IsReady)
            {
                try
                {
                    var account = await _gw2ApiManager.Gw2ApiClient.V2.Account.GetAsync();

                    if (account?.Name is not null)
                    {
                        _paths.AccountName = account.Name;

                        string path = $@"{_paths.AccountPath}Overflow_Trade_Rep_Template.xlsx";

                        if (!File.Exists(_paths.RepSheet))
                        {
                            using Stream target = File.Create(_paths.RepSheet);
                            Stream source = OverflowTradingAssist.ModuleInstance.ContentsManager.GetFileStream(@"data\Overflow_Trade_Rep_Template.xlsx");
                            _ = source.Seek(0, SeekOrigin.Begin);
                            source.CopyTo(target);
                        }

                        IsReady = File.Exists(_paths.RepSheet);
                        _fileStatus = StatusType.Success;
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

                    _fileStatus = StatusType.Error;

                    if (_notificationBadge() is NotificationBadge notificationBadge)
                    {
                        notificationBadge.AddNotification(new(() => "Failed to update subtoken.", () => _fileStatus == StatusType.Success));
                        return false;
                    }
                }
            }
            else
            {
                return true;
            }

            return IsReady;
        }

        public async void Test() { }

        public async Task<List<Trade>> Load()
        {
            if (!OverflowTradingAssist.ModuleInstance.Settings.SheetInitialized.Value)
            {
                if (await InitializeFile() is List<Trade> trades)
                {
                    OverflowTradingAssist.ModuleInstance.Settings.SheetInitialized.Value = true;
                    return trades;
                }
            }

            return await LoadTrades();
        }

        public async Task<List<Trade>> LoadTrades()
        {
            if (await EnsureFileExists())
            {
                var trades = new List<Trade>();

                try
                {
                    // Specify the path to the Excel file
                    string excelFilePath = _paths.RepSheet; // Replace with your actual file path

                    // Check if the file exists
                    if (File.Exists(excelFilePath))
                    {
                        // Create a FileInfo object for the Excel file
                        var excelFile = new FileInfo(excelFilePath);

                        // Load the Excel package from the file
                        using var package = new ExcelPackage(excelFile);
                        // Access the worksheets in the Excel package
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0]; // You can specify the worksheet index or name

                        static ObservableCollection<ItemAmount> GetItems(string items)
                        {
                            var list = new ObservableCollection<ItemAmount>();
                            try
                            {

                            }
                            catch
                            {

                            }

                            return list;
                        }

                        int lastRowIndex = 1;
                        while (!string.IsNullOrEmpty(worksheet.Cells[lastRowIndex, 1].Text))
                        {
                            lastRowIndex++;
                        }

                        for (int i = 2; i < lastRowIndex; i++)
                        {
                            Trade t;
                            //Load Trades
                            trades.Add(t = new()
                            {
                                TradePartner = worksheet.Cells[i, s_tradePartner].Text,
                                ReviewLink = worksheet.Cells[i, s_reviewLink].Text,
                                TradeListingLink = worksheet.Cells[i, s_tradeListingLink].Text,
                                Id = worksheet.Cells[i, s_guuid].Text is string guid ? Guid.Parse(guid) : Guid.NewGuid(),
                                Value = decimal.Parse(worksheet.Cells[i, s_tradeAmount].Value.ToString()),
                            });

                            t.Items.Add(new() { Item = DataModels.Item.UnkownItem, Value = decimal.Parse(worksheet.Cells[i, s_tradeAmount].Value.ToString()) });
                            t.Payment.Add(new() { Item = DataModels.Item.Coin, Value = decimal.Parse(worksheet.Cells[i, s_tradeAmount].Value.ToString()) });
                        }

                        _loadTradeStatus = StatusType.Success;
                        IsLoaded = true;
                        return trades;
                    }
                }
                catch (Exception ex)
                {
                    string text = $"Failed to load  trades from Excel file: {ex.Message}";
                    OverflowTradingAssist.Logger.Warn(ex, text);
                    _loadTradeStatus = StatusType.Error;

                    if (_notificationBadge() is NotificationBadge notificationBadge)
                    {
                        notificationBadge.AddNotification(new(() => text, () => _loadTradeStatus == StatusType.Success));
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

                if (_trades is List<Trade> trades)
                {
                    var tradesToRemove = trades.Where(e => e.ExcelDeleteRequested).ToDictionary(e => e.Id.ToString(), e => e);
                    var tradesToUpdate = trades.Where(e => e.ExcelSaveRequested).ToDictionary(e => e.Id.ToString(), e => e);

                    if ((tradesToUpdate.Count > 0 || tradesToRemove.Count > 0) && await EnsureFileExists())
                    {
                        try
                        {
                            // Specify the path to the Excel file
                            string excelFilePath = _paths.RepSheet; // Replace with your actual file path

                            // Check if the file exists
                            if (File.Exists(excelFilePath))
                            {
                                // Create a FileInfo object for the Excel file
                                var excelFile = new FileInfo(excelFilePath);

                                // Load the Excel package from the file
                                using var package = new ExcelPackage(excelFile);
                                // Access the worksheets in the Excel package
                                ExcelWorksheet worksheet = package.Workbook.Worksheets[0]; // You can specify the worksheet index or name

                                var rowsToRemove = new List<int>();

                                // Find the first empty row in column A by using LINQ
                                int row = 1;
                                while (!string.IsNullOrEmpty(worksheet.Cells[row, 1].Text))
                                {
                                    if (worksheet.Cells[row, s_guuid].Value is string guuid)
                                    {
                                        if (tradesToRemove.ContainsKey(guuid))
                                        {
                                            tradesToRemove[guuid].ExcelDeleteRequested = false;
                                            tradesToRemove[guuid].ExcelSaveRequested = false;

                                            rowsToRemove.Add(row);
                                            continue;
                                        }
                                        else if (tradesToUpdate.ContainsKey(guuid))
                                        {
                                            Trade trade = tradesToUpdate[guuid];

                                            worksheet.Cells[row, s_tradePartner].Value = trade.TradePartner;
                                            worksheet.Cells[row, s_tradeAmount].Value = trade.ItemValue;
                                            worksheet.Cells[row, s_ItemSummary].Value = trade.ItemSummary;
                                            worksheet.Cells[row, s_reviewLink].Value = trade.ReviewLink;
                                            worksheet.Cells[row, s_tradeListingLink].Value = trade.TradeListingLink;
                                            worksheet.Cells[row, s_guuid].Value = trade.Id;

                                            trade.ExcelDeleteRequested = false;
                                            trade.ExcelSaveRequested = false;
                                        }
                                    }

                                    row++;
                                }

                                rowsToRemove.OrderByDescending(e => e).ToList().ForEach(worksheet.DeleteRow);

                                bool unlocked = await FileExtension.WaitForFileUnlock(excelFilePath);
                                if (!unlocked)
                                {
                                    OverflowTradingAssist.Logger.Warn($"File {excelFilePath} is locked. We can't open the file. Please close all applications using it.");
                                    return;
                                }

                                // Save the changes to the Excel file
                                package.Save();
                            }
                            else
                            {
                                // Handle the case where the file doesn't exist
                                Console.WriteLine("The Excel file does not exist.");
                            }

                            _saveTradeStatus = StatusType.Success;
                        }
                        catch (Exception ex)
                        {
                            string text = $"Failed to update Excel file: {ex.Message}";
                            OverflowTradingAssist.Logger.Warn(ex, text);
                            _saveTradeStatus = StatusType.Error;

                            if (_notificationBadge() is NotificationBadge notificationBadge)
                            {
                                notificationBadge.AddNotification(new(() => text, () => _saveTradeStatus == StatusType.Success));
                            }
                        }
                    }
                }
            }
        }

        public async Task SaveTrade(Trade trade)
        {
            if (trade?.IsValidTrade() != true)
            {
                Debug.WriteLine($"INVALID TRADE!");
                return;
            }

            if (_trades is List<Trade> trades && await EnsureFileExists())
            {
                try
                {
                    // Specify the path to the Excel file
                    string excelFilePath = _paths.RepSheet; // Replace with your actual file path

                    // Check if the file exists
                    if (File.Exists(excelFilePath))
                    {
                        // Create a FileInfo object for the Excel file
                        var excelFile = new FileInfo(excelFilePath);

                        // Load the Excel package from the file
                        using var package = new ExcelPackage(excelFile);
                        // Access the worksheets in the Excel package
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0]; // You can specify the worksheet index or name

                        // Find the first empty row in column A by using LINQ
                        int lastRowIndex = 1;
                        while (!string.IsNullOrEmpty(worksheet.Cells[lastRowIndex, 1].Text))
                        {
                            if (worksheet.Cells[lastRowIndex, s_guuid].Value is string guuid && guuid == trade.Id.ToString())
                                break;

                            lastRowIndex++;
                        }

                        // For example, to add a value in column A of the empty row:
                        worksheet.Cells[lastRowIndex, s_tradePartner].Value = trade.TradePartner;
                        worksheet.Cells[lastRowIndex, s_tradeAmount].Value = trade.ItemValue;
                        worksheet.Cells[lastRowIndex, s_ItemSummary].Value = trade.ItemSummary;
                        worksheet.Cells[lastRowIndex, s_reviewLink].Value = trade.ReviewLink;
                        worksheet.Cells[lastRowIndex, s_tradeListingLink].Value = trade.TradeListingLink;
                        worksheet.Cells[lastRowIndex, s_guuid].Value = trade.Id;

                        bool unlocked = await FileExtension.WaitForFileUnlock(excelFilePath);
                        if (!unlocked)
                        {
                            OverflowTradingAssist.Logger.Warn($"File {excelFilePath} is locked. We can't open the file. Please close all applications using it.");
                            return;
                        }

                        // Save the changes to the Excel file
                        package.Save();

                        if (!trades.Contains(trade))
                            trades.Add(trade);
                    }
                    else
                    {
                        // Handle the case where the file doesn't exist
                        Console.WriteLine("The Excel file does not exist.");
                    }

                    _saveTradeStatus = StatusType.Success;
                }
                catch (Exception ex)
                {
                    string text = $"Failed to update Excel file: {ex.Message}";
                    OverflowTradingAssist.Logger.Warn(ex, text);
                    _saveTradeStatus = StatusType.Error;

                    if (_notificationBadge() is NotificationBadge notificationBadge)
                    {
                        notificationBadge.AddNotification(new(() => text, () => _saveTradeStatus == StatusType.Success));
                    }
                }
            }
        }

        public async Task<List<Trade>> InitializeFile()
        {
            if (await LoadTrades() is List<Trade> trades)
            {
                // Check if the file exists
                if (File.Exists(_paths.RepSheet))
                {
                    // Create a FileInfo object for the Excel file
                    var excelFile = new FileInfo(_paths.RepSheet);

                    // Load the Excel package from the file
                    using var package = new ExcelPackage(excelFile);
                    // Access the worksheets in the Excel package
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0]; // You can specify the worksheet index or name

                    for (int i = 0; i < trades.Count; i++)
                    {
                        Trade trade = trades[i];

                        worksheet.Cells[2 + i, s_tradePartner].Value = trade.TradePartner;
                        worksheet.Cells[2 + i, s_tradeAmount].Value = trade.ItemValue;
                        worksheet.Cells[2 + i, s_ItemSummary].Value = trade.ItemSummary;
                        worksheet.Cells[2 + i, s_reviewLink].Value = trade.ReviewLink;
                        worksheet.Cells[2 + i, s_tradeListingLink].Value = trade.TradeListingLink;
                        worksheet.Cells[2 + i, s_guuid].Value = trade.Id;
                    }

                    bool unlocked = await FileExtension.WaitForFileUnlock(_paths.RepSheet);
                    if (!unlocked)
                    {
                        OverflowTradingAssist.Logger.Warn($"File {_paths.RepSheet} is locked. We can't access the file to save our trades. Please close all applications using it.");
                        return null;
                    }

                    // Save the changes to the Excel file
                    package.Save();
                    OverflowTradingAssist.ModuleInstance.Settings.SheetInitialized.Value = true;

                    return trades;
                }
            }

            return null;
        }
    }
}
