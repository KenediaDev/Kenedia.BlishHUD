using Blish_HUD.Modules.Managers;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Core.Controls;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Kenedia.Modules.OverflowTradingAssist.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File = System.IO.File;

namespace Kenedia.Modules.OverflowTradingAssist.Services
{
    public class ExcelManipulation
    {
        private static readonly int s_tradePartner = 1;
        private static readonly int s_tradeAmount = 2;
        private static readonly int s_ItemSummary = 3;
        private static readonly int s_reviewLink = 4;
        private static readonly int s_tradeListingLink = 5;
        private static readonly int s_items = 10;
        private static readonly int s_tradeType = 11;

        private readonly Paths _paths;
        private readonly Gw2ApiManager _gw2ApiManager;
        private readonly Func<NotificationBadge> _notificationBadge;
        private readonly Func<LoadingSpinner> _spinner;
        private readonly Func<List<Trade>> _trades;

        private StatusType _fileStatus;
        private StatusType _loadTradeStatus;
        private StatusType _saveTradeStatus;

        public ExcelManipulation(Paths paths, Gw2ApiManager gw2ApiManager, Func<NotificationBadge> notificationBadge, Func<LoadingSpinner> spinner, Func<List<Trade>> trades)
        {
            _paths = paths;
            _gw2ApiManager = gw2ApiManager;
            _notificationBadge = notificationBadge;
            _spinner = spinner;
            _trades = trades;
            _gw2ApiManager.SubtokenUpdated += Gw2ApiManager_SubtokenUpdated;

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public bool IsReady { get; private set; }

        public bool IsLoaded { get; private set; }

        private async void Gw2ApiManager_SubtokenUpdated(object sender, Blish_HUD.ValueEventArgs<IEnumerable<TokenPermission>> e)
        {
            try
            {
                _ = await EnsureFileExists();
            }
            catch
            {
            }
        }

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

        public async void LoadTrades()
        {
            if (_trades?.Invoke() is List<Trade> trades && await EnsureFileExists())
            {
                trades.Clear();

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

                        static List<ItemAmount> GetItems(string items)
                        {
                            var list = new List<ItemAmount>();
                            try
                            {
                                foreach (string sub in items.Split(','))
                                {
                                    string[] s = sub.Split('|');
                                    if (s.Length > 1)
                                    {
                                        int amount = int.TryParse(s[0], out int value) ? value : 0;
                                        var item = int.TryParse(s[1], out int itemId) ? OverflowTradingAssist.Data.Items.Items.FirstOrDefault(i => i.Id == itemId) : null;

                                        if (item is not null)
                                        {
                                            list.Add(new()
                                            {
                                                Amount = amount,
                                                Item = item,
                                            });
                                        }
                                    }
                                }
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
                                Amount = (double)worksheet.Cells[i, s_tradeAmount].Value,
                                ReviewLink = worksheet.Cells[i, s_reviewLink].Text,
                                TradeListingLink = worksheet.Cells[i, s_tradeListingLink].Text,
                                Items = GetItems(worksheet.Cells[i, s_items].Text),
                                TradeType = Enum.TryParse(worksheet.Cells[i, s_tradeType].Text, out TradeType tradeType) ? tradeType : TradeType.None,
                            });
                        }

                        _loadTradeStatus = StatusType.Success;
                        IsLoaded = true;
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
        }

        public async void SaveTrade(Trade trade)
        {
            if (trade?.IsValidTrade() != true)
            {
                return;
            }

            if (_trades?.Invoke() is List<Trade> trades && await EnsureFileExists())
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
                            lastRowIndex++;
                        }

                        // For example, to add a value in column A of the empty row:
                        worksheet.Cells[lastRowIndex, s_tradePartner].Value = trade.TradePartner;
                        worksheet.Cells[lastRowIndex, s_tradeAmount].Value = trade.Amount;
                        worksheet.Cells[lastRowIndex, s_ItemSummary].Value = trade.ItemSummary;
                        worksheet.Cells[lastRowIndex, s_reviewLink].Value = trade.ReviewLink;
                        worksheet.Cells[lastRowIndex, s_tradeListingLink].Value = trade.TradeListingLink;
                        worksheet.Cells[lastRowIndex, s_items].Value = string.Join(", ", trade.Items.Select(e => $"{e.Amount}|{e.Item.Id}"));
                        worksheet.Cells[lastRowIndex, s_tradeType].Value = trade.TradeType.ToString();

                        bool unlocked = await FileExtension.WaitForFileUnlock(excelFilePath);
                        if (!unlocked)
                        {
                            OverflowTradingAssist.Logger.Warn($"File {excelFilePath} is locked. We can't open the file. Please close all applications using it.");
                            return;
                        }

                        // Save the changes to the Excel file
                        package.Save();

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
    }
}
