using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

using System;

using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.Elements;
using ExileCore.PoEMemory.Elements.InventoryElements;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using ImGuiNET;
using SharpDX;
using System.Text;
using Vector2N = System.Numerics.Vector2;

namespace VendorItems
{
    public class VendorItems : BaseSettingsPlugin<VendorItemsSettings>
    {

        private Vector2N drawTextVector3;
        static bool DataExists(List<(string, string, int, int, double, double)> list, (string, string, int, int, double, double) item)
        {
            foreach ((string firstName, string lastName, int age, int score, double pDPS, double eDPS) in list)
            {
                if (firstName == item.Item1 && lastName == item.Item2 && age == item.Item3 && score == item.Item4 && pDPS == item.Item5 && eDPS == item.Item6)
                {
                    return true;
                }
            }
            return false;
        }

        List<(string, string, int, int, double, double)> interestingItems = new List<(string, string, int, int, double, double)>();
        List<(string, string, int, int)> interestingItemsPurchase = new List<(string, string, int, int)>();
        List<string> FourLinkStringList = new List<string>();
        List<string> ThreeLinkStringList = new List<string>();
        List<string> PartialClassNamesToIgnore = new List<string>();
        int inventoryIndex = 1;
        List<string> pathsToReadInventory = new List<string>
            {
            "Metadata/Items/Weapons",
            "Metadata/Items/Armours",
            "Metadata/Items/Rings",
            "Metadata/Items/Amulets",
            "Metadata/Items/Belts",
            "Metadata/Items/Flasks"





        };
        static Tuple<string, int> WrapText(string input, int maxLineLength)
        {
            string[] words = input.Split(' ');

            StringBuilder wrappedText = new StringBuilder();
            int currentLineLength = 0;
            int lines = 0;
            foreach (string word in words)
            {
                if (currentLineLength + word.Length + 1 <= maxLineLength) // +1 for space
                {
                    wrappedText.Append(word + " ");
                    currentLineLength += word.Length + 1;
                }
                else
                {

                    //wrappedText.Append("\n\t " + word + " ");
                    lines += 1;
                    wrappedText.AppendLine();
                    wrappedText.Append(word + " ");
                    currentLineLength = word.Length + 1;
                }
            }


            Tuple<string, int> returnAux = new Tuple<string, int>(wrappedText.ToString(), lines);
            return returnAux;
        }
        public override bool Initialise()
        {


            return true;

        }

        public override void Render()
        {

            updateSettings();
            VendorShop();
            RogShop();
            playerInventory();
            ritualParse();
            printInterestItems();

            interestingItemsPurchase.Clear();
            interestingItems.Clear();

        }


        static List<string> SortLinks(List<string> inputList)
        {
            List<string> result = new List<string>();

            foreach (string item in inputList)
            {
                var aux = String.Concat(item.OrderByDescending(c => c));
                if (!result.Contains(aux))
                {
                    result.Add(aux);
                }

            }

            return result;
        }

        private void updateSettings()
        {
            FourLinkStringList = SortLinks(Settings.FourLinkStrings.Value.Split(',').ToList());
            ThreeLinkStringList = SortLinks(Settings.ThreeLinkStrings.Value.Split(',').ToList());
            PartialClassNamesToIgnore = Settings.ItemClassesToIgnoreModFiltering.Value.Split(',').ToList();
            if (String.IsNullOrWhiteSpace(Settings.ItemClassesToIgnoreModFiltering.Value))
            {
                PartialClassNamesToIgnore.Clear();
            }
        }
        private void printInterestItems()
        {
            var ingameState = GameController.Game.IngameState;
            var auxChildrenFirst = ingameState.IngameUi.PurchaseWindow;
            var haggleWindow = ingameState.IngameUi.HaggleWindow;
            var inventoryIsVisible = ingameState.IngameUi.InventoryPanel[InventoryIndex.PlayerInventory].IsVisible;
            var haggleText = haggleWindow.GetChildFromIndices(6, 2, 0)?.Text;
            var leftPanel = ingameState.IngameUi.OpenLeftPanel;
            var ritualIsVisible = ingameState.IngameUi.RitualWindow.IsVisible;
            //LogMessage("Purchase window " + auxChildrenFirst.IsVisible, 5, Color.Red);
            //LogMessage("haggleWindow  " + haggleWindow.IsVisible, 5, Color.Red);
            //LogMessage("inventoryIsVisible  " + inventoryIsVisible, 5, Color.Red);
            //LogMessage("interestingItems.Count()  " + interestingItems.Count().ToString(), 5, Color.Red);


            if (((auxChildrenFirst.IsVisible || ritualIsVisible || (haggleWindow.IsVisible && haggleText == "Deal")) ||
                (!auxChildrenFirst.IsVisible && !ritualIsVisible && !haggleWindow.IsVisible && inventoryIsVisible && !leftPanel.IsVisible)) && interestingItems.Count() > 0)
            {

                Vector2 newInfoPanel = new Vector2(Settings.ResultX.Value, Settings.ResultY.Value);
                var drawBox = new RectangleF(newInfoPanel.X, newInfoPanel.Y, 306, 200);
                List<(string, string, int, int, double, double)> interestingItemsSorted = interestingItems.FindAll(pair => pair.Item4 > Settings.MinScoreThreshold).OrderByDescending(pair => pair.Item4).ToList();
                var drawBoxFix = drawBox;
                drawBoxFix.Height = (interestingItemsSorted.Count() * Settings.TextSpacing.Value) + 5;
                Color newColorBlack = new Color(Color.Black.R, Color.Black.G, Color.Black.B, Convert.ToByte(200));
                Color newColorWhite = new Color(Color.White.R, Color.White.G, Color.White.B, Convert.ToByte(100));

                var hoveredItem = ingameState.UIHover.AsObject<HoverItemIcon>();

                var hoveredItemTooltip = hoveredItem?.Tooltip;
                var tooltipRect = hoveredItemTooltip?.GetClientRect();
                var canDraw = false;
                if (tooltipRect == null)
                {
                    canDraw = true;
                }
                else
                {
                    canDraw = !checkRectOverlaps(drawBoxFix, (RectangleF)tooltipRect);
                }

                if (canDraw && interestingItemsSorted.Any(x => x.Item4 >= Settings.MinScoreThreshold.Value)) 
                {
                    Graphics.DrawBox(drawBoxFix, newColorBlack, 5);
                    Graphics.DrawFrame(drawBoxFix, newColorWhite, 2);






                    foreach ((string className, string itemType, int inventoryPage, int score, double pDPS, double eDPS) in interestingItemsSorted.FindAll(x => x.Item4 >= Settings.MinScoreThreshold.Value))
                    {
                        var baseColor = Color.White;


                        if (score >= Settings.STierThreshold)
                        {
                            baseColor = Settings.STierThresholdColor;

                        }
                        else if (score >= Settings.ATierThreshold)
                        {
                            baseColor = Settings.ATierThresholdColor;
                        }
                        else if (score >= Settings.BTierThreshold)
                        {
                            baseColor = Settings.BTierThresholdColor;
                        }
                        else if (score >= Settings.CTierThreshold)
                        {
                            baseColor = Settings.CTierThresholdColor;
                        }

                        var textToPrint = inventoryPage.ToString() + " " + itemType + " \t " + score.ToString();
                        Tuple<string, int> textToPrintAux = WrapText(textToPrint, Settings.WordWrapSize);


                        drawTextVector3 = Graphics.DrawText(textToPrintAux.Item1, newInfoPanel, baseColor);

                        newInfoPanel.Y += Settings.TextSpacing.Value + (Settings.TextSpacing.Value * textToPrintAux.Item2);
                    }
                }

            }


        }








        private void VendorShop()
        {
            var ingameState = GameController.Game.IngameState;
            inventoryIndex = 1;
            if (ingameState.ServerData.NPCInventories.Count > 0)
            {

                foreach (var inventory in ingameState.ServerData.NPCInventories)
                {


                    if (inventory.Inventory.CountItems > 0)
                    {
                        var auxaux = inventory.Inventory.InventorySlotItems;

                        HiglightAllVendorShop(auxaux, inventory.Inventory.Rows, inventory.Inventory.Columns);
                    }
                    inventoryIndex = inventoryIndex + 1;

                }
                var inventoryZone = ingameState.IngameUi.InventoryPanel[InventoryIndex.PlayerInventory].VisibleInventoryItems;

            }
        }
        private void playerInventory()
        {
            var ingameState = GameController.Game.IngameState;
            var inventory = ingameState.IngameUi.InventoryPanel[InventoryIndex.PlayerInventory].VisibleInventoryItems;
            var inventoryIsVisible = ingameState.IngameUi.InventoryPanel[InventoryIndex.PlayerInventory].IsVisible;
            var purchaseWindow = ingameState.IngameUi.PurchaseWindow;
            var haggleWindow = ingameState.IngameUi.HaggleWindow;
            var leftPanel = ingameState.IngameUi.OpenLeftPanel;
            var ritualIsVisible = ingameState.IngameUi.RitualWindow.IsVisible;





            var playerLevel = GameController.Player.GetComponent<Player>().Level;
            var hoveredItem = ingameState.UIHover.AsObject<HoverItemIcon>();
            var UIHoverEntity = ingameState.UIHover.Entity;
            var hoveredItemTooltip = hoveredItem?.Tooltip;
            var tooltipRect = hoveredItemTooltip?.GetClientRect();
            var playerLevelOverride = Math.Min(60, playerLevel);
            if (Settings.PlayerLevelOverrideDebug)
            {
                playerLevelOverride = Math.Min(60, Settings.PlayerLevelOverride);
            }
            if (inventoryIsVisible)
            {
                foreach (var item in inventory)
                {
                    List<int> weights = new List<int>();

                    if (item == null) continue;


                    if (!pathsToReadInventory.Any(item.Item.Path.Contains)) continue;

                    var isWeapon = item.Item.Path.Contains("/Weapons/");
                    var drawRect = item.GetClientRect();

                    var itemMods = item?.Item.GetComponent<Mods>();
                    var itemSockets = item?.Item.GetComponent<Sockets>();
                    var itemName = item?.Item.GetComponent<Base>().Name;
                    var baseItemType = GameController.Files.BaseItemTypes.Translate(item.Item.Path);
                    var className = baseItemType.ClassName;

                    bool isRGB = itemSockets?.IsRGB ?? false;
                    int links = itemSockets?.LargestLinkSize ?? 0;
                    int sockets = itemSockets?.NumberOfSockets ?? 0;
                    var stats = itemMods?.HumanStats;
                    var mods = itemMods?.ItemMods;
                    var itemIsVeiled = mods?.Any(s => s.Group.Contains("Veiled")) ?? false;

                    List<string> interestingThings = new List<string>();


                    //var auxChildrenFirst = ingameState?.IngameUi?.PurchaseWindow;

                    //var auxpurchaseWindow = auxChildrenFirst?.GetChildAtIndex(7)?.GetChildFromIndices(1);
                    if (!ritualIsVisible && !haggleWindow.IsVisible && !purchaseWindow.IsVisible)
                    //if (auxpurchaseWindow != null)
                    {
                        //var purchaseWindow = auxpurchaseWindow?.GetChildFromIndices(inventoryIndex - 1);
                        //var squareWidth = (int)purchaseWindow?.GetClientRect().Width / columns;
                        //var squareHeight = (int)purchaseWindow?.GetClientRect().Height / rows;
                        //var initialTradeWindowX = (int)purchaseWindow?.GetClientRect().TopLeft.X;
                        //var initialTradeWindowY = (int)purchaseWindow?.GetClientRect().TopLeft.Y;

                        //var itemRectPosX = initialTradeWindowX + (item.PosX * squareWidth);
                        //var itemRectPosY = initialTradeWindowY + (item.PosY * squareHeight);
                        //var itemRectWidth = squareWidth * item.SizeX;
                        //var itemRectHeight = squareHeight * item.SizeY;

                        //var drawRectFix = new RectangleF(itemRectPosX, itemRectPosY, itemRectWidth, itemRectHeight);


                        //drawRectFix.Top += 7;
                        ////drawRectFix.Bottom += offset ;
                        ////drawRectFix.Right += offset ;
                        //drawRectFix.Left += 7;

                        drawRect.Top += 3;
                        drawRect.Bottom -= 3;
                        drawRect.Right -= 3;
                        drawRect.Left += 3;

                        //bool isPageVisible = false;
                        //if (purchaseWindow != null)
                        //{
                        //    isPageVisible = purchaseWindow.IsVisible;
                        //}
                        bool anyMatch = false;
                        if (PartialClassNamesToIgnore.Count > 0 && Settings.IgnoreFiltering)
                        {
                            anyMatch = PartialClassNamesToIgnore.Any(className.Contains);
                        }




                        //bool anyMatch = PartialClassNamesToIgnore.Any(item => item.Contains(className));
                        double pDPS = 0;
                        double eDPS = 0;
                        var type = "";
                        if (!anyMatch)
                        {

                            var modCheckReturn = modCheck(item.Item);

                            interestingThings.AddRange(modCheckReturn.Item2);
                            weights.AddRange(modCheckReturn.Item1);
                            pDPS = modCheckReturn.Item3;
                            eDPS = modCheckReturn.Item4;
                            type = modCheckReturn.Item5;


                        }


                        if (links == 6)
                        {
                            interestingThings.Add("SIX LINK POGGERS");
                            weights.Add(Settings.SixLinkScore);
                        }
                        else if (links == 5)
                        {
                            interestingThings.Add("FIVE LINK");
                            weights.Add(Settings.FiveLinkScore);
                        }
                        else if (sockets == 6)
                        {
                            interestingThings.Add("SIX SOCKET");
                            weights.Add(Settings.SixSocketScore);
                        }
                        else if (links == 4 && !anyMatch)
                        {

                            var linkColors = itemSockets.SocketGroup.First(s => s.Length == 4);
                            var orderedLinkColors = String.Concat(linkColors.OrderByDescending(c => c));

                            //LogMessage("4L " + linkColors + " ordered " + orderedLinkColors, 5, Color.Green);
                            if (FourLinkStringList.Contains(orderedLinkColors) && playerLevelOverride <= Settings.FourLinkCharacterLevelThreshold)
                            {
                                interestingThings.Add(orderedLinkColors + " 4L " + className);
                                weights.Add(Settings.FourLinkScore);
                            }

                        }
                        else if (links == 3 && !anyMatch)
                        {

                            var linkColors = itemSockets.SocketGroup.First(s => s.Length == 3);
                            var orderedLinkColors = String.Concat(linkColors.OrderByDescending(c => c));

                            //LogMessage("3L " + linkColors + " ordered " + orderedLinkColors, 5, Color.Green);
                            if (ThreeLinkStringList.Contains(orderedLinkColors) && playerLevelOverride <= Settings.ThreeLinkCharacterLevelThreshold)
                            {
                                interestingThings.Add(orderedLinkColors + " 3L " + className);
                                weights.Add(Settings.ThreeLinkScore);
                            }

                        }


                        if (isRGB)
                        {
                            interestingThings.Add("RGB");
                            weights.Add(Settings.RGBScore);

                        }


                        //INTERESTINGTHINGS LIST
                        //WEIGHTS LIST


                        if (interestingThings.Count() > 0)
                        {

                            var scaleFactor = 5.2f - (0.07 * playerLevelOverride);
                            int finalScore = 0;

                            if (Settings.UseScoreLevelScaler)
                            {
                                finalScore = (int)(weights.Sum() * scaleFactor);
                            }
                            else
                            {
                                finalScore = (int)(weights.Sum());
                            }

                            string concatInterestingStuff = type + className + " " + string.Join(",", interestingThings);
                            var newItem = (className, concatInterestingStuff, inventoryIndex, finalScore, pDPS, eDPS);

                            // Check if the item exists before adding
                            if (!DataExists(interestingItems, newItem))
                            {
                                interestingItems.Add(newItem);

                            }

                            var colorBorder = Color.White;
                            var drawBox = new RectangleF(drawRect.X, drawRect.Y - 30, drawRect.Width, drawRect.Height);
                            var drawBoxScour = new RectangleF(drawRect.X + drawRect.Width / 4, drawRect.Y + drawRect.Height / 4, drawRect.Width / 2, drawRect.Height / 2);
                            var drawBox2 = new RectangleF(drawRect.X + 2, drawRect.Y + 2, drawRect.Width - 4, drawRect.Height - 4);



                            if (finalScore >= Settings.STierThreshold)
                            {
                                colorBorder = Settings.STierThresholdColor;

                            }
                            else if (finalScore >= Settings.ATierThreshold)
                            {
                                colorBorder = Settings.ATierThresholdColor;
                            }
                            else if (finalScore >= Settings.BTierThreshold)
                            {
                                colorBorder = Settings.BTierThresholdColor;
                            }
                            else if (finalScore >= Settings.CTierThreshold)
                            {
                                colorBorder = Settings.CTierThresholdColor;
                            }
                            var canDraw = false;
                            if (tooltipRect == null)
                            {
                                canDraw = true;
                            }
                            else
                            {
                                canDraw = !checkRectOverlaps(drawRect, (RectangleF)tooltipRect);
                            }



                            if (inventoryIsVisible && canDraw && !leftPanel.IsVisible && !purchaseWindow.IsVisible && !ritualIsVisible && !haggleWindow.IsVisible && finalScore >= Settings.MinScoreThreshold.Value)
                            {
                                Graphics.DrawFrame(drawRect, colorBorder, 5);
                                if (itemIsVeiled)
                                {
                                    var textPosX = drawRect.BottomLeft.X + 5;
                                    var textPosY = drawRect.BottomLeft.Y - 15;
                                    Graphics.DrawText("UNVEIL", new Vector2N(textPosX, textPosY), Color.Gold);
                                    //Graphics.DrawFrame(drawRect, Color.White, 3);
                                }

                                if (isWeapon)
                                {
                                    var textPosX = drawRect.TopLeft.X + 5;
                                    var textPosY = drawRect.TopLeft.Y + 5;
                                    if (pDPS > 0)
                                    {
                                        Graphics.DrawText("P: " + pDPS.ToString("F0"), new Vector2N(textPosX, textPosY));
                                        textPosY += Settings.TextSpacing;
                                    }
                                    if (eDPS > 0)
                                    {
                                        Graphics.DrawText("E: " + eDPS.ToString("F0"), new Vector2N(textPosX, textPosY), Color.Yellow);
                                    }
                                    //Graphics.DrawFrame(drawRect, Color.White, 3);
                                }
                            }



                        }
                    }


                }

            }

        }

        private void ritualParse()
        {
            var ingameState = GameController.Game.IngameState;

            var inventory = ingameState.IngameUi.RitualWindow.Items;
            var inventoryIsVisible = ingameState.IngameUi.RitualWindow.IsVisible;
            var purchaseWindow = ingameState.IngameUi.PurchaseWindow;
            var haggleWindow = ingameState.IngameUi.HaggleWindow;
            var leftPanel = ingameState.IngameUi.OpenLeftPanel;



            var inventoryFix = ingameState.IngameUi.RitualWindow.GetChildAtIndex(11).GetChildrenAs<NormalInventoryItem>().Skip(1).ToList() ?? new List<NormalInventoryItem>();



            var playerLevel = GameController.Player.GetComponent<Player>().Level;
            var hoveredItem = ingameState.UIHover.AsObject<HoverItemIcon>();
            var UIHoverEntity = ingameState.UIHover.Entity;
            var hoveredItemTooltip = hoveredItem?.Tooltip;
            var tooltipRect = hoveredItemTooltip?.GetClientRect();
            var playerLevelOverride = Math.Min(60, playerLevel);
            if (Settings.PlayerLevelOverrideDebug)
            {
                playerLevelOverride = Math.Min(60, Settings.PlayerLevelOverride);
            }
        
            if (inventoryIsVisible)
            {
                foreach (var item in inventoryFix)
                {
                    List<int> weights = new List<int>();

                    if (item == null) continue;


                    if (!pathsToReadInventory.Any(item.Item.Path.Contains)) continue;

                    var isWeapon = item.Item.Path.Contains("/Weapons/");
                    var drawRect = item.GetClientRect();

                    var itemMods = item?.Item.GetComponent<Mods>();
                    var itemSockets = item?.Item.GetComponent<Sockets>();
                    var itemName = item?.Item.GetComponent<Base>().Name;
                    var baseItemType = GameController.Files.BaseItemTypes.Translate(item.Item.Path);
                    var className = baseItemType.ClassName;

                    bool isRGB = itemSockets?.IsRGB ?? false;
                    int links = itemSockets?.LargestLinkSize ?? 0;
                    int sockets = itemSockets?.NumberOfSockets ?? 0;
                    var stats = itemMods?.HumanStats;
                    var mods = itemMods?.ItemMods;
                    var itemIsVeiled = mods?.Any(s => s.Group.Contains("Veiled")) ?? false;

                    List<string> interestingThings = new List<string>();


                    //var auxChildrenFirst = ingameState?.IngameUi?.PurchaseWindow;

                    //var auxpurchaseWindow = auxChildrenFirst?.GetChildAtIndex(7)?.GetChildFromIndices(1);
                    if (true)
                    //if (auxpurchaseWindow != null)
                    {
                        //var purchaseWindow = auxpurchaseWindow?.GetChildFromIndices(inventoryIndex - 1);
                        //var squareWidth = (int)purchaseWindow?.GetClientRect().Width / columns;
                        //var squareHeight = (int)purchaseWindow?.GetClientRect().Height / rows;
                        //var initialTradeWindowX = (int)purchaseWindow?.GetClientRect().TopLeft.X;
                        //var initialTradeWindowY = (int)purchaseWindow?.GetClientRect().TopLeft.Y;

                        //var itemRectPosX = initialTradeWindowX + (item.PosX * squareWidth);
                        //var itemRectPosY = initialTradeWindowY + (item.PosY * squareHeight);
                        //var itemRectWidth = squareWidth * item.SizeX;
                        //var itemRectHeight = squareHeight * item.SizeY;

                        //var drawRectFix = new RectangleF(itemRectPosX, itemRectPosY, itemRectWidth, itemRectHeight);


                        //drawRectFix.Top += 7;
                        ////drawRectFix.Bottom += offset ;
                        ////drawRectFix.Right += offset ;
                        //drawRectFix.Left += 7;

                        drawRect.Top += 3;
                        drawRect.Bottom -= 3;
                        drawRect.Right -= 3;
                        drawRect.Left += 3;

                        //bool isPageVisible = false;
                        //if (purchaseWindow != null)
                        //{
                        //    isPageVisible = purchaseWindow.IsVisible;
                        //}
                        bool anyMatch = false;
                        if (PartialClassNamesToIgnore.Count > 0 && Settings.IgnoreFiltering)
                        {
                            anyMatch = PartialClassNamesToIgnore.Any(className.Contains);
                        }




                        //bool anyMatch = PartialClassNamesToIgnore.Any(item => item.Contains(className));
                        double pDPS = 0;
                        double eDPS = 0;
                        var type = "";
                        if (!anyMatch)
                        {

                            var modCheckReturn = modCheck(item.Item);

                            interestingThings.AddRange(modCheckReturn.Item2);
                            weights.AddRange(modCheckReturn.Item1);
                            pDPS = modCheckReturn.Item3;
                            eDPS = modCheckReturn.Item4;
                            type = modCheckReturn.Item5;


                        }


                        if (links == 6)
                        {
                            interestingThings.Add("SIX LINK POGGERS");
                            weights.Add(Settings.SixLinkScore);
                        }
                        else if (links == 5)
                        {
                            interestingThings.Add("FIVE LINK");
                            weights.Add(Settings.FiveLinkScore);
                        }
                        else if (sockets == 6)
                        {
                            interestingThings.Add("SIX SOCKET");
                            weights.Add(Settings.SixSocketScore);
                        }
                        else if (links == 4 && !anyMatch)
                        {

                            var linkColors = itemSockets.SocketGroup.First(s => s.Length == 4);
                            var orderedLinkColors = String.Concat(linkColors.OrderByDescending(c => c));

                            //LogMessage("4L " + linkColors + " ordered " + orderedLinkColors, 5, Color.Green);
                            if (FourLinkStringList.Contains(orderedLinkColors) && playerLevelOverride <= Settings.FourLinkCharacterLevelThreshold)
                            {
                                interestingThings.Add(orderedLinkColors + " 4L " + className);
                                weights.Add(Settings.FourLinkScore);
                            }

                        }
                        else if (links == 3 && !anyMatch)
                        {

                            var linkColors = itemSockets.SocketGroup.First(s => s.Length == 3);
                            var orderedLinkColors = String.Concat(linkColors.OrderByDescending(c => c));

                            //LogMessage("3L " + linkColors + " ordered " + orderedLinkColors, 5, Color.Green);
                            if (ThreeLinkStringList.Contains(orderedLinkColors) && playerLevelOverride <= Settings.ThreeLinkCharacterLevelThreshold)
                            {
                                interestingThings.Add(orderedLinkColors + " 3L " + className);
                                weights.Add(Settings.ThreeLinkScore);
                            }

                        }


                        if (isRGB)
                        {
                            interestingThings.Add("RGB");
                            weights.Add(Settings.RGBScore);

                        }


                        //INTERESTINGTHINGS LIST
                        //WEIGHTS LIST


                        if (interestingThings.Count() > 0)
                        {

                            var scaleFactor = 5.2f - (0.07 * playerLevelOverride);
                            int finalScore = 0;

                            if (Settings.UseScoreLevelScaler)
                            {
                                finalScore = (int)(weights.Sum() * scaleFactor);
                            }
                            else
                            {
                                finalScore = (int)(weights.Sum());
                            }

                            string concatInterestingStuff = type + className + " " + string.Join(",", interestingThings);
                            var newItem = (className, concatInterestingStuff, inventoryIndex, finalScore, pDPS, eDPS);

                            // Check if the item exists before adding
                            if (!DataExists(interestingItems, newItem))
                            {
                                interestingItems.Add(newItem);

                            }

                            var colorBorder = Color.White;
                            var drawBox = new RectangleF(drawRect.X, drawRect.Y - 30, drawRect.Width, drawRect.Height);
                            var drawBoxScour = new RectangleF(drawRect.X + drawRect.Width / 4, drawRect.Y + drawRect.Height / 4, drawRect.Width / 2, drawRect.Height / 2);
                            var drawBox2 = new RectangleF(drawRect.X + 2, drawRect.Y + 2, drawRect.Width - 4, drawRect.Height - 4);



                            if (finalScore >= Settings.STierThreshold)
                            {
                                colorBorder = Settings.STierThresholdColor;

                            }
                            else if (finalScore >= Settings.ATierThreshold)
                            {
                                colorBorder = Settings.ATierThresholdColor;
                            }
                            else if (finalScore >= Settings.BTierThreshold)
                            {
                                colorBorder = Settings.BTierThresholdColor;
                            }
                            else if (finalScore >= Settings.CTierThreshold)
                            {
                                colorBorder = Settings.CTierThresholdColor;
                            }
                            var canDraw = false;
                            if (tooltipRect == null)
                            {
                                canDraw = true;
                            }
                            else
                            {
                                canDraw = !checkRectOverlaps(drawRect, (RectangleF)tooltipRect);
                            }



                            if (inventoryIsVisible && canDraw && finalScore >= Settings.MinScoreThreshold.Value)
                            {
                                Graphics.DrawFrame(drawRect, colorBorder, 5);
                                if (itemIsVeiled)
                                {
                                    var textPosX = drawRect.BottomLeft.X + 5;
                                    var textPosY = drawRect.BottomLeft.Y - 15;
                                    Graphics.DrawText("UNVEIL", new Vector2N(textPosX, textPosY));
                                    //Graphics.DrawFrame(drawRect, Color.White, 3);
                                }

                                if (isWeapon)
                                {
                                    var textPosX = drawRect.TopLeft.X + 5;
                                    var textPosY = drawRect.TopLeft.Y + 5;
                                    if (pDPS > 0)
                                    {
                                        Graphics.DrawText("P: " + pDPS.ToString("F0"), new Vector2N(textPosX, textPosY));
                                        textPosY += Settings.TextSpacing;
                                    }
                                    if (eDPS > 0)
                                    {
                                        Graphics.DrawText("E: " + eDPS.ToString("F0"), new Vector2N(textPosX, textPosY), Color.Yellow);
                                    }
                                    //Graphics.DrawFrame(drawRect, Color.White, 3);
                                }
                            }



                        }
                    }


                }

            }

        }

        private void RogShop()
        {
            var ingameState = GameController.Game.IngameState;
            var haggleWindow = ingameState.IngameUi.HaggleWindow;
            var haggleDealVisible = haggleWindow.GetChildFromIndices(12, 0).IsVisible;
            if (haggleWindow.IsVisible)
            {
                var inventory = haggleWindow?.GetChildFromIndices(8, 1, 0, 0);


                var itemList = inventory?.GetChildrenAs<NormalInventoryItem>().Skip(1).ToList() ?? new List<NormalInventoryItem>();
                var haggleText = haggleWindow.GetChildFromIndices(6, 2, 0)?.Text;
                var playerLevel = GameController.Player.GetComponent<Player>().Level;
                var hoveredItem = ingameState.UIHover.AsObject<HoverItemIcon>();
                var UIHoverEntity = ingameState.UIHover.Entity;
                var hoveredItemTooltip = hoveredItem?.Tooltip;
                var tooltipRect = hoveredItemTooltip?.GetClientRect();
                var playerLevelOverride = Math.Min(60, playerLevel);
                if (Settings.PlayerLevelOverrideDebug)
                {
                    playerLevelOverride = Math.Min(60, Settings.PlayerLevelOverride);
                }
                if (haggleText == "Deal")
                {
                    foreach (var item in itemList)
                    {
                        List<int> weights = new List<int>();

                        if (item == null) continue;


                        if (item.Item.Path.Contains("Currency") || item.Item.Path.Contains("Gems")) continue;


                        var drawRect = item.GetClientRect();

                        var itemMods = item?.Item.GetComponent<Mods>();
                        var itemSockets = item?.Item.GetComponent<Sockets>();
                        var itemName = item?.Item.GetComponent<Base>().Name;
                        var baseItemType = GameController.Files.BaseItemTypes.Translate(item.Item.Path);
                        var className = baseItemType.ClassName;

                        bool isRGB = itemSockets?.IsRGB ?? false;
                        int links = itemSockets?.LargestLinkSize ?? 0;
                        int sockets = itemSockets?.NumberOfSockets ?? 0;
                        var stats = itemMods?.HumanStats;
                        var mods = itemMods?.ItemMods;
                        var type = "";
                        List<string> interestingThings = new List<string>();


                        //var auxChildrenFirst = ingameState?.IngameUi?.PurchaseWindow;

                        //var auxpurchaseWindow = auxChildrenFirst?.GetChildAtIndex(7)?.GetChildFromIndices(1);
                        if (true)
                        //if (auxpurchaseWindow != null)
                        {
                            //var purchaseWindow = auxpurchaseWindow?.GetChildFromIndices(inventoryIndex - 1);
                            //var squareWidth = (int)purchaseWindow?.GetClientRect().Width / columns;
                            //var squareHeight = (int)purchaseWindow?.GetClientRect().Height / rows;
                            //var initialTradeWindowX = (int)purchaseWindow?.GetClientRect().TopLeft.X;
                            //var initialTradeWindowY = (int)purchaseWindow?.GetClientRect().TopLeft.Y;

                            //var itemRectPosX = initialTradeWindowX + (item.PosX * squareWidth);
                            //var itemRectPosY = initialTradeWindowY + (item.PosY * squareHeight);
                            //var itemRectWidth = squareWidth * item.SizeX;
                            //var itemRectHeight = squareHeight * item.SizeY;

                            //var drawRectFix = new RectangleF(itemRectPosX, itemRectPosY, itemRectWidth, itemRectHeight);


                            //drawRectFix.Top += 7;
                            ////drawRectFix.Bottom += offset ;
                            ////drawRectFix.Right += offset ;
                            //drawRectFix.Left += 7;

                            drawRect.Top += 5;
                            drawRect.Left += 8;

                            //bool isPageVisible = false;
                            //if (purchaseWindow != null)
                            //{
                            //    isPageVisible = purchaseWindow.IsVisible;
                            //}
                            bool anyMatch = false;
                            if (PartialClassNamesToIgnore.Count > 0 && Settings.IgnoreFiltering)
                            {
                                anyMatch = PartialClassNamesToIgnore.Any(className.Contains);
                            }




                            //bool anyMatch = PartialClassNamesToIgnore.Any(item => item.Contains(className));

                            if (!anyMatch)
                            {
                                var modCheckReturn = modCheck(item.Item);

                                interestingThings.AddRange(modCheckReturn.Item2);
                                weights.AddRange(modCheckReturn.Item1);
                                var pDPS = modCheckReturn.Item3;
                                var eDPS = modCheckReturn.Item4;
                                type = modCheckReturn.Item5;

                            }

                            if (links == 6)
                            {
                                interestingThings.Add("SIX LINK POGGERS");
                                weights.Add(Settings.SixLinkScore);
                            }
                            else if (links == 5)
                            {
                                interestingThings.Add("FIVE LINK");
                                weights.Add(Settings.FiveLinkScore);
                            }
                            else if (sockets == 6)
                            {
                                interestingThings.Add("SIX SOCKET");
                                weights.Add(Settings.SixSocketScore);
                            }
                            else if (links == 4 && !anyMatch)
                            {

                                var linkColors = itemSockets.SocketGroup.First(s => s.Length == 4);
                                var orderedLinkColors = String.Concat(linkColors.OrderByDescending(c => c));

                                //LogMessage("4L " + linkColors + " ordered " + orderedLinkColors, 5, Color.Green);
                                if (FourLinkStringList.Contains(orderedLinkColors) && playerLevelOverride <= Settings.FourLinkCharacterLevelThreshold)
                                {
                                    interestingThings.Add(orderedLinkColors + " 4L " + className);
                                    weights.Add(Settings.FourLinkScore);
                                }

                            }
                            else if (links == 3 && !anyMatch)
                            {

                                var linkColors = itemSockets.SocketGroup.First(s => s.Length == 3);
                                var orderedLinkColors = String.Concat(linkColors.OrderByDescending(c => c));

                                //LogMessage("3L " + linkColors + " ordered " + orderedLinkColors, 5, Color.Green);
                                if (ThreeLinkStringList.Contains(orderedLinkColors) && playerLevelOverride <= Settings.ThreeLinkCharacterLevelThreshold)
                                {
                                    interestingThings.Add(orderedLinkColors + " 3L " + className);
                                    weights.Add(Settings.ThreeLinkScore);
                                }

                            }


                            if (isRGB)
                            {
                                interestingThings.Add("RGB");
                                weights.Add(Settings.RGBScore);

                            }


                            //INTERESTINGTHINGS LIST
                            //WEIGHTS LIST


                            if (interestingThings.Count() > 0)
                            {

                                var scaleFactor = 5.2f - (0.07 * playerLevelOverride);
                                int finalScore = 0;
                                double pDPS = 0;
                                double eDPS = 0;
                                if (Settings.UseScoreLevelScaler)
                                {
                                    finalScore = (int)(weights.Sum() * scaleFactor);
                                }
                                else
                                {
                                    finalScore = (int)(weights.Sum());
                                }

                                string concatInterestingStuff = type + className + " " + string.Join(",", interestingThings);
                                var newItem = (className, concatInterestingStuff, inventoryIndex, finalScore, pDPS, eDPS);

                                // Check if the item exists before adding
                                if (!DataExists(interestingItems, newItem))
                                {
                                    interestingItems.Add(newItem);
                                }

                                var colorBorder = Color.White;
                                var drawBox = new RectangleF(drawRect.X, drawRect.Y - 30, drawRect.Width, drawRect.Height);
                                var drawBoxScour = new RectangleF(drawRect.X + drawRect.Width / 4, drawRect.Y + drawRect.Height / 4, drawRect.Width / 2, drawRect.Height / 2);
                                var drawBox2 = new RectangleF(drawRect.X + 2, drawRect.Y + 2, drawRect.Width - 4, drawRect.Height - 4);



                                if (finalScore >= Settings.STierThreshold)
                                {
                                    colorBorder = Settings.STierThresholdColor;

                                }
                                else if (finalScore >= Settings.ATierThreshold)
                                {
                                    colorBorder = Settings.ATierThresholdColor;
                                }
                                else if (finalScore >= Settings.BTierThreshold)
                                {
                                    colorBorder = Settings.BTierThresholdColor;
                                }
                                else if (finalScore >= Settings.CTierThreshold)
                                {
                                    colorBorder = Settings.CTierThresholdColor;
                                }
                                var canDraw = false;
                                if (tooltipRect == null)
                                {
                                    canDraw = true;
                                }
                                else
                                {
                                    canDraw = !checkRectOverlaps(drawRect, (RectangleF)tooltipRect);
                                }


                                if (true && canDraw && !haggleDealVisible && finalScore >= Settings.MinScoreThreshold.Value)
                                {
                                    Graphics.DrawFrame(drawRect, colorBorder, 5);
                                    var isWeapon = item.Item.Path.Contains("/Weapons/");
                                    if (isWeapon)
                                    {
                                        var textPosX = drawRect.TopLeft.X + 5;
                                        var textPosY = drawRect.TopLeft.Y + 5;
                                        if (pDPS > 0)
                                        {
                                            Graphics.DrawText("P: " + pDPS.ToString("F0"), new Vector2N(textPosX, textPosY));
                                            textPosY += Settings.TextSpacing;
                                        }
                                        if (eDPS > 0)
                                        {
                                            Graphics.DrawText("E: " + eDPS.ToString("F0"), new Vector2N(textPosX, textPosY), Color.Yellow);
                                        }
                                        //Graphics.DrawFrame(drawRect, Color.White, 3);
                                    }
                                }

                            }



                        }
                    }
                }

            }
        }

        private bool checkRectOverlaps(RectangleF rect1, RectangleF rect2)
        {

            if (rect1.BottomRight.X < rect2.TopLeft.X || rect2.BottomRight.X < rect1.TopLeft.X)
                return false;

            // Check if one rectangle is above the other
            if (rect1.BottomRight.Y < rect2.TopLeft.Y || rect2.BottomRight.Y < rect1.TopLeft.Y)
                return false;

            return true;
        }

        private Tuple<List<int>, List<string>, double, double, string> modCheck(Entity item)
        {
            List<string> interestingThings = new List<string>();
            List<int> weights = new List<int>();

            var hasSpiritualAid = GameController.IngameState.ServerData.PassiveSkillIds.Contains(4177);
            var hasSpiritualCommand = GameController.IngameState.ServerData.PassiveSkillIds.Contains(43689);


            List<string> interestingThingsAttack = new List<string>();
            List<string> interestingThingsAttackEle = new List<string>();
            List<string> interestingThingsCaster = new List<string>();
            List<string> interestingThingsMinion = new List<string>();
            List<int> weightsAttack = new List<int>();
            List<int> weightsAttackEle = new List<int>();
            List<int> weightsCaster = new List<int>();
            List<int> weightsMinion = new List<int>();


            var isWeapon = item.Path.Contains("/Weapons/");
            //LogMessage("entity: " + item.RenderName, 5, Color.Red);
            var itemMods = item?.GetComponent<Mods>();
            var itemSockets = item?.GetComponent<Sockets>();
            var itemName = item?.GetComponent<Base>().Name;
            var baseItemType = GameController.Files.BaseItemTypes.Translate(item.Path);
            var itemDropLevel = baseItemType.DropLevel;

            var className = baseItemType.ClassName;

            bool isRGB = itemSockets?.IsRGB ?? false;
            int links = itemSockets?.LargestLinkSize ?? 0;
            int sockets = itemSockets?.NumberOfSockets ?? 0;
            var stats = itemMods?.HumanStats;
            var mods = itemMods?.ItemMods;

            bool existsMovementSpeed = mods.Any(s => s.Group.Contains("MovementVelocity"));

            int movementSpeedMod = 0;
            if (existsMovementSpeed && className == "Boots")
            {
                movementSpeedMod = mods.Find(s => s.Group.Contains("MovementVelocity")).Values[0];
                weights.Add(Settings.MovementSpeedScore * movementSpeedMod);

                interestingThings.Add(movementSpeedMod.ToString() + " MoveSpeed");
            }
            int globalDot = 0;
            bool existsDotMultiplier = mods.Any(s => s.Group.Contains("GlobalDamageOverTimeMultiplier"));
            if (existsDotMultiplier)
            {
                globalDot = mods.Find(s => s.Group.Contains("GlobalDamageOverTimeMultiplier")).Values[0];
                interestingThings.Add(globalDot.ToString() + "Dot Multi");
                weights.Add(Settings.GlobalDotScore * globalDot);
            }

            double addedFireSpellDamageAverage = 0f;
            double addedFireSpellDamageAverageImp = 0f;
            bool existsAddedSpellFire = mods.Any(s => s.Name.Contains("SpellAddedFireDamage") || s.Group.Contains("AddedFireDamageSpellsAndAttacks"));
            if (existsAddedSpellFire)
            {
                addedFireSpellDamageAverage = mods?.Find(s => s.Name.Contains("SpellAddedFireDamage"))?.Values?.Average() ?? 0;
                addedFireSpellDamageAverageImp = mods?.Find(s => s.Group.Contains("AddedFireDamageSpellsAndAttacks"))?.Values?.Average() ?? 0;
                var total = addedFireSpellDamageAverage + addedFireSpellDamageAverageImp;
                interestingThingsCaster.Add(total.ToString() + " Fire spells");
                weightsCaster.Add((int)(Settings.AddedFireToSpellsScore * total));

            }

            double addedColdSpellDamageAverage = 0f;
            double addedColdSpellDamageAverageImp = 0f;
            bool existsAddedSpellCold = mods.Any(s => s.Name.Contains("SpellAddedColdDamage") || s.Group.Contains("AddedColdDamageSpellsAndAttacks"));
            if (existsAddedSpellCold)
            {
                addedColdSpellDamageAverage = mods?.Find(s => s.Name.Contains("SpellAddedColdDamage"))?.Values?.Average() ?? 0;
                addedColdSpellDamageAverageImp = mods?.Find(s => s.Group.Contains("AddedColdDamageSpellsAndAttacks"))?.Values?.Average() ?? 0;
                var total = addedColdSpellDamageAverage + addedColdSpellDamageAverageImp;
                interestingThingsCaster.Add(total.ToString() + " Cold spells");
                weightsCaster.Add((int)(Settings.AddedColdToSpellsScore * total));

            }
            double addedLightningSpellDamageAverage = 0f;
            double addedLightningSpellDamageAverageImp = 0f;
            bool existsAddedSpellLight = mods.Any(s => s.Name.Contains("SpellAddedLightningDamage") || s.Group.Contains("AddedLightningDamageSpellsAndAttacks"));
            if (existsAddedSpellLight)
            {
                addedLightningSpellDamageAverage = mods?.Find(s => s.Name.Contains("SpellAddedLightningDamage"))?.Values?.Average() ?? 0;
                addedLightningSpellDamageAverageImp = mods?.Find(s => s.Group.Contains("AddedLightningDamageSpellsAndAttacks"))?.Values?.Average() ?? 0;
                var total = addedLightningSpellDamageAverage + addedLightningSpellDamageAverageImp;
                interestingThingsCaster.Add(total.ToString() + " Light spells");
                weightsCaster.Add((int)(Settings.AddedLightningToSpellsScore * total));

            }

            int spellDamage = 0;
            int spellDamageMana = 0;
            int spellDamageImp = 0;
            bool existsSpellDamage = mods.Any(s => s.Name.Contains("SpellDamageOnWeapon") || s.Name.Contains("SpellDamageAndManaOnWeapon"));
            if (existsSpellDamage)
            {
                spellDamage = mods?.Find(s => s.Name.Contains("SpellDamageOnWeapon"))?.Values[0] ?? 0;
                spellDamageMana = mods?.Find(s => s.Name.Contains("SpellDamageAndManaOnWeapon"))?.Values[0] ?? 0;

                spellDamageImp = mods?.Find(s => s.Name.Contains("SpellDamageOnWeaponImplicitWand"))?.Values[0] ?? 0;
                var total = spellDamage + spellDamageMana + spellDamageImp;
                interestingThingsCaster.Add(total.ToString() + " SpellDmg");
                weightsCaster.Add((int)(Settings.SpellDamageScore * total));

            }
            int spellCrit = 0;
            bool existsSpellCrit = mods.Any(s => s.Name.Contains("SpellCriticalStrikeChance"));
            if (existsSpellCrit)
            {
                spellCrit = mods.Find(s => s.Name.Contains("SpellCrit")).Values[0];
                interestingThingsCaster.Add(spellCrit.ToString() + " SpellCrit");
                weightsCaster.Add(Settings.SpellCritChanceScore * spellCrit);
            }

            bool existsCritMulti = mods.Any(s => s.Group.Contains("CriticalStrikeMultiplier")) && !mods.Any(s => s.Name.Contains("CriticalMultiplierImplicit"));

            int critMulti = 0;

            if (existsCritMulti)
            {
                critMulti = mods.Find(s => s.Group.Contains("CriticalStrikeMultiplier")).Values[0];
                interestingThings.Add(critMulti.ToString() + " CritMulti");
                weights.Add(Settings.CritMultiScore * critMulti);
            }

            bool existsMinionCritChance = mods.Any(s => s.Name.Contains("MinionCriticalStrikeChanceIncrease"));
            int minionCrit = 0;
            if (existsMinionCritChance)
            {
                minionCrit = mods.Find(s => s.Name.Contains("MinionCriticalStrikeChanceIncrease")).Values[0];
                interestingThingsMinion.Add(minionCrit.ToString() + " MinionCrit");
                weightsMinion.Add(Settings.MinionCritChanceScore * minionCrit);
            }

            bool existsMinionCritMulti = mods.Any(s => s.Group.Contains("MinionCriticalStrikeMultiplier"));
            int minionCritMulti = 0;
            if (existsMinionCritMulti)
            {
                minionCritMulti = mods.Find(s => s.Group.Contains("MinionCriticalStrikeMultiplier")).Values[0];
                interestingThingsMinion.Add(minionCritMulti.ToString() + " Minion CritMulti");
                weightsMinion.Add(Settings.MinionCriticalMultiplierScore * minionCritMulti);
            }


            bool existsMinionIASICS = mods.Any(s => s.Name.Contains("MinionAttackAndCastSpeed"));
            int minionIAS = 0;
            if (existsMinionIASICS)
            {
                minionIAS = mods.Find(s => s.Name.Contains("MinionAttackAndCastSpeed")).Values[0];
                interestingThingsMinion.Add(minionIAS.ToString() + " Minion IAS ICS");
                weightsMinion.Add(Settings.MinionAttackCastScore * minionIAS);
                if (hasSpiritualCommand)
                {
                    interestingThings.Add(minionIAS.ToString() + " Minion IAS ICS");
                    weights.Add(Settings.IncreasedAttackSpeedScore * minionIAS);
                }
            }
            bool existsWED = mods.Any(s => s.Name.Contains("WeaponElementalDamageOnWeapons"));
            int WED = 0;
            if (existsWED)
            {
                WED = mods.Find(s => s.Name.Contains("WeaponElementalDamageOnWeapons")).Values[0];
                interestingThingsAttack.Add(WED.ToString() + " WED");
                weightsAttack.Add(Settings.WeaponElementalDamageScore * WED);
                interestingThingsAttackEle.Add(WED.ToString() + " WED");
                weightsAttackEle.Add(Settings.WeaponElementalDamageScore * WED);
            }

            bool existsFirePercent = mods.Any(s => s.Name.Contains("FireDamagePrefixOnWeapon") || s.Name.Contains("FireDamagePercent"));

            int firePercentPrefix = 0;
            int firePercentSuffix = 0;
            if (existsFirePercent)
            {
                firePercentPrefix = mods.Find(s => s.Name.Contains("FireDamagePrefixOnWeapon"))?.Values[0] ?? 0;
                firePercentSuffix = mods.Find(s => s.Name.Contains("FireDamagePercent"))?.Values[0] ?? 0;
                var total = firePercentPrefix + firePercentSuffix;
                interestingThingsCaster.Add(total.ToString() + " Fire%");
                weightsCaster.Add((int)(Settings.FireDamageScore * total));
                interestingThingsAttackEle.Add(total.ToString() + " Fire%");
                weightsAttackEle.Add((int)(Settings.FireDamageScore * total));

            }

            int coldPercentPrefix = 0;
            int coldPercentSuffix = 0;
            bool existsColdPercent = mods.Any(s => s.Name.Contains("ColdDamagePrefixOnWeapon") || s.Name.Contains("ColdDamagePercent"));
            if (existsColdPercent)
            {
                coldPercentPrefix = mods.Find(s => s.Name.Contains("ColdDamagePrefixOnWeapon"))?.Values[0] ?? 0;
                coldPercentSuffix = mods.Find(s => s.Name.Contains("ColdDamagePercent"))?.Values[0] ?? 0;
                var total = coldPercentPrefix + coldPercentSuffix;
                interestingThingsAttackEle.Add(total.ToString() + " Cold %");
                weightsAttackEle.Add((int)(Settings.ColdDamageScore * total));
                interestingThingsCaster.Add(total.ToString() + " Cold %");
                weightsCaster.Add((int)(Settings.ColdDamageScore * total));

            }

            bool existsLightningPercent = mods.Any(s => s.Name.Contains("LightningDamagePrefixOnWeapon") || s.Name.Contains("LightningDamagePercent"));
            int lightningPercentPrefix = 0;
            int lightningPercentSuffix = 0;
            if (existsLightningPercent)
            {
                lightningPercentPrefix = mods.Find(s => s.Name.Contains("LightningDamagePrefixOnWeapon"))?.Values[0] ?? 0;
                lightningPercentSuffix = mods.Find(s => s.Name.Contains("LightningDamagePercent"))?.Values[0] ?? 0;
                var total = lightningPercentPrefix + lightningPercentSuffix;
                interestingThingsCaster.Add(total.ToString() + " Lightning %");
                weightsCaster.Add((int)(Settings.LightningDamageScore * total));
                interestingThingsAttackEle.Add(total.ToString() + " Lightning %");
                weightsAttackEle.Add((int)(Settings.LightningDamageScore * total));

            }

            bool existsBurning = mods.Any(s => s.Name.Contains("BurnDamage"));
            int burnDmg = 0;
            if (existsBurning)
            {
                burnDmg = mods.Find(s => s.Name.Contains("BurnDamage")).Values[0];
                interestingThingsCaster.Add(burnDmg.ToString() + " Burn %");
                weightsCaster.Add(Settings.BurnDamageScore * burnDmg);
            }

            bool existsMinionDamage = mods.Any(s => s.Name.Contains("MinionDamageOnWeapon") || s.Name.Contains("MinionDamageAndManaOnWeapon") || s.Name.Contains("MinionDamageImplicitWand"));

            int minionDamage = 0;
            int minionDamageMana = 0;
            int minionDamageImp = 0;
            if (existsMinionDamage)
            {

                minionDamage = mods?.Find(s => s.Name.Contains("MinionDamageOnWeapon"))?.Values[0] ?? 0;
                minionDamageMana = mods?.Find(s => s.Name.Contains("MinionDamageAndManaOnWeapon"))?.Values[0] ?? 0;
                minionDamageImp = mods?.Find(s => s.Name.Contains("MinionDamageImplicitWand"))?.Values[0] ?? 0;
                var total = minionDamage + minionDamageMana + minionDamageImp;
                interestingThingsMinion.Add(total.ToString() + " MinionDmg%");
                weightsMinion.Add((int)(Settings.MinionDamageScore * total));

                if (hasSpiritualAid)
                {
                    interestingThings.Add(total.ToString() + " MinionDmg%");
                    weights.Add((int)(Math.Max(Settings.MinionDamageScore,
                        Math.Max(Settings.SpellDamageScore,
                        Math.Max(Settings.WeaponElementalDamageScore,
                        Math.Max(Settings.FireDamageScore,
                        Math.Max(Settings.ColdDamageScore, Settings.LightningDamageScore))))))
                        * total);
                }

            }
            int FireDotMultiplier = 0;
            bool existsFireDotMultiplier = mods.Any(s => s.Name.Contains("FireDamageOverTimeMultiplier"));
            if (existsFireDotMultiplier)
            {
                FireDotMultiplier = mods.Find(s => s.Name.Contains("FireDamageOverTimeMultiplier")).Values[0];
                interestingThings.Add(FireDotMultiplier.ToString() + " Fire Dot Multi");
                weights.Add(Settings.FireDotScore * FireDotMultiplier);
            }
            bool existsColdDotMultiplier = mods.Any(s => s.Name.Contains("ColdDamageOverTimeMultiplier"));
            int ColdDotMultiplier = 0;
            if (existsColdDotMultiplier)
            {
                ColdDotMultiplier = mods.Find(s => s.Name.Contains("ColdDamageOverTimeMultiplier")).Values[0];
                interestingThingsCaster.Add(ColdDotMultiplier.ToString() + " Cold Dot Multi");
                weightsCaster.Add(Settings.ColdDotScore * ColdDotMultiplier);

            }
            bool existsChaosDotMultiplier = mods.Any(s => s.Name.Contains("ChaosDamageOverTimeMultiplier"));
            int ChaosDotMultiplier = 0;
            if (existsChaosDotMultiplier)
            {
                ChaosDotMultiplier = mods.Find(s => s.Name.Contains("ChaosDamageOverTimeMultiplier")).Values[0];
                interestingThingsCaster.Add(ChaosDotMultiplier.ToString() + " Chaos Dot Multi");
                weightsCaster.Add(Settings.ChaosDotScore * ChaosDotMultiplier);
            }
            bool existsPhysicalDotMultiplier = mods.Any(s => s.Name.Contains("PhysicalDamageOverTimeMultiplier"));
            int PhysicalDotMultiplier = 0;
            if (existsPhysicalDotMultiplier)
            {
                PhysicalDotMultiplier = mods.Find(s => s.Name.Contains("PhysicalDamageOverTimeMultiplier")).Values[0];
                interestingThingsCaster.Add(PhysicalDotMultiplier.ToString() + " Phy Dot Multi");
                weightsCaster.Add(Settings.PhysicalDotScore * PhysicalDotMultiplier);
            }





            bool existsPlusOneFire = mods.Any(s => s.Name.Contains("GlobalFireSpellGemsLevel1"));
            int globalFireSpellGems = 0;
            if (existsPlusOneFire)
            {

                interestingThingsCaster.Add(" POG +1 FIRE");
                weightsCaster.Add(Settings.OneHandPlusFireScore);
            }
            bool existsPlusOneCold = mods.Any(s => s.Name.Contains("GlobalColdSpellGemsLevel1"));
            int globalColdSpellGems = 0;
            if (existsPlusOneCold)
            {
                globalFireSpellGems++;
                interestingThingsCaster.Add("POG +1 cold");
                weightsCaster.Add(Settings.OneHandPlusColdScore);
            }
            bool existsPlusOneLightning = mods.Any(s => s.Name.Contains("GlobalLightningSpellGemsLevel1"));
            int globalLightningSpellGems = 0;
            if (existsPlusOneLightning)
            {
                globalLightningSpellGems++;
                interestingThingsCaster.Add("POG +1 LIGHT");
                weightsCaster.Add(Settings.OneHandPlusLightScore);
            }
            bool existsPlusOneChaos = mods.Any(s => s.Name.Contains("GlobalChaosSpellGemsLevel1"));
            int globalChaosSpellGems = 0;
            if (existsPlusOneChaos)
            {
                globalChaosSpellGems++;
                interestingThingsCaster.Add("POG +1 chaos");
                weightsCaster.Add(Settings.OneHandPlusChaosScore);
            }

            bool existsPlusOnePhysical = mods.Any(s => s.Name.Contains("GlobalPhysicalSpellGemsLevel1"));
            int globalPhysicalSpellGems = 0;
            if (existsPlusOnePhysical)
            {
                globalPhysicalSpellGems++;
                interestingThingsCaster.Add("POG +1 PHYS");
                weightsCaster.Add(Settings.OneHandPlusPhysicalScore);
            }
            bool existsPlusOneMinion = mods.Any(s => s.Name.Contains("MinionGemLevel1h1"));
            int globalMinionSpellGems = 0;
            if (existsPlusOneMinion)
            {
                globalMinionSpellGems++;
                interestingThingsMinion.Add("POG +1 MINION");
                weightsMinion.Add(Settings.OneHandPlusMinionScore);
            }

            // two handed
            int GemLevel = 0;

            bool existsPlusFire2h = mods.Any(s => s.Name.Contains("GlobalFireSpellGemsLevelTwoHand"));
            if (existsPlusFire2h)
            {
                GemLevel = mods.Find(s => s.Name.Contains("GlobalFireSpellGemsLevelTwoHand")).Values[0];
                globalFireSpellGems = globalFireSpellGems + GemLevel;
                interestingThingsCaster.Add("POG +" + GemLevel + " Fire");
                weightsCaster.Add(Settings.TwoHandPlusFireScore * GemLevel);


            }
            bool existsPlusCold2h = mods.Any(s => s.Name.Contains("GlobalColdSpellGemsLevelTwoHand"));
            if (existsPlusCold2h)
            {
                GemLevel = mods.Find(s => s.Name.Contains("GlobalColdSpellGemsLevelTwoHand")).Values[0];
                globalColdSpellGems = globalColdSpellGems + GemLevel;
                interestingThingsCaster.Add("POG +" + GemLevel + " Cold");
                weightsCaster.Add(Settings.TwoHandPlusColdScore * GemLevel);


            }

            bool existsPlusLightning2h = mods.Any(s => s.Name.Contains("GlobalLightningSpellGemsLevelTwoHand"));
            if (existsPlusLightning2h)
            {
                GemLevel = mods.Find(s => s.Name.Contains("GlobalLightningSpellGemsLevelTwoHand")).Values[0];
                globalLightningSpellGems = globalLightningSpellGems + GemLevel;
                interestingThingsCaster.Add("POG +" + GemLevel + " Lightning");
                weightsCaster.Add(Settings.TwoHandPlusLightScore * GemLevel);


            }



            bool existsPlusChaos2h = mods.Any(s => s.Name.Contains("GlobalChaosSpellGemsLevelTwoHand"));
            if (existsPlusChaos2h)
            {
                GemLevel = mods.Find(s => s.Name.Contains("GlobalChaosSpellGemsLevelTwoHand")).Values[0];
                globalChaosSpellGems = globalChaosSpellGems + GemLevel;
                interestingThingsCaster.Add("POG +" + GemLevel + " Chaos");
                weightsCaster.Add(Settings.TwoHandPlusChaosScore * GemLevel);


            }


            bool existsPlusPhysical2h = mods.Any(s => s.Name.Contains("GlobalPhysicalSpellGemsLevelTwoHand"));
            if (existsPlusPhysical2h)
            {
                GemLevel = mods.Find(s => s.Name.Contains("GlobalPhysicalSpellGemsLevelTwoHand")).Values[0];
                globalPhysicalSpellGems = globalPhysicalSpellGems + GemLevel;
                interestingThingsCaster.Add("POG +" + GemLevel + " Phyis");
                weightsCaster.Add(Settings.TwoHandPlusPhysicalScore * GemLevel);


            }



            // amulets


            bool existsPlusFireAmmy = mods.Any(s => s.Name.Contains("GlobalFireGemLevel1"));
            int PlusFireAmmy = 0;
            if (existsPlusFireAmmy)
            {
                PlusFireAmmy++;
                interestingThingsCaster.Add("POG +1 FIRE");
                weightsCaster.Add(Settings.AmuletPlusFireScore);
            }

            bool existsPlusColdAmmy = mods.Any(s => s.Name.Contains("GlobalColdGemLevel1"));
            int PlusColdAmmy = 0;
            if (existsPlusColdAmmy)
            {
                PlusColdAmmy++;
                interestingThingsCaster.Add("POG +1 Cold");
                weightsCaster.Add(Settings.AmuletPlusColdScore);
            }

            bool existsPlusLightningAmmy = mods.Any(s => s.Name.Contains("GlobalLightningGemLevel1"));
            int PlusLightningAmmy = 0;
            if (existsPlusLightningAmmy)
            {
                PlusLightningAmmy++;
                interestingThingsCaster.Add("POG +1 Light");
                weightsCaster.Add(Settings.AmuletPlusLightScore);
            }

            bool existsPlusChaosAmmy = mods.Any(s => s.Name.Contains("GlobalChaosGemLevel1"));
            int PlusChaosAmmy = 0;
            if (existsPlusChaosAmmy)
            {
                PlusChaosAmmy++;
                interestingThingsCaster.Add("POG +1 Chaos");
                weightsCaster.Add(Settings.AmuletPlusChaosScore);

            }
            bool existsPlusPhysicalAmmy = mods.Any(s => s.Name.Contains("GlobalPhysicalGemLevel1"));
            int PlusPhysicalAmmy = 0;
            if (existsPlusPhysicalAmmy)
            {
                PlusPhysicalAmmy++;
                interestingThingsCaster.Add("POG +1 PHY");
                weightsCaster.Add(Settings.AmuletPlusPhysicalScore);

            }
            int PlusSkillAmmy = 0;
            bool existsPlusSkillAmmy = mods.Any(s => s.Name.Contains("GlobalSkillGemLevel1"));
            if (existsPlusSkillAmmy)
            {

                interestingThingsCaster.Add("POG +1 ALLLL");
                weightsCaster.Add(Settings.AmuletPlusGlobalScore);

            }


            // helmet minion
            int PlusMinionHelm = 0;
            bool existsPlusMinionHelm = mods.Any(s => s.Name.Contains("GlobalIncreaseMinionSpellSkillGemLevel"));
            if (existsPlusMinionHelm)
            {
                PlusMinionHelm = mods.Find(s => s.Name.Contains("GlobalIncreaseMinionSpellSkillGemLevel")).Values[0];
                interestingThingsMinion.Add("POG +" + PlusMinionHelm + " Minion Helm");
                weightsMinion.Add(Settings.MinionHelmGemScore * PlusMinionHelm);


            }


            bool isWeaponOrQuiver = item.Path.Contains("/Weapons/") || item.Path.Contains("/Quiver/");

            bool anyResistance = mods.Any(s => s.Group.Contains("Resistance")) && !mods.Any(s => s.Name.Contains("ResistanceImplicit")) && !mods.Any(s => s.Name.Contains("ResistImplicit"));
            bool anyLife = mods.Any(s => s.Group.Contains("IncreasedLife")) && !mods.Any(s => s.Name.Contains("LifeImplicit"));



            int fireRes = 0;
            int coldRes = 0;
            int lightningRes = 0;
            int chaosRes = 0;
            int allResMods = 0;
            int FireAndLightningResistance = 0;
            int ColdAndLightningResistance = 0;
            int FireAndColdResistance = 0;
            int allResSummed = 0;
            if (anyResistance && (!Settings.IgnoreResistsWeaponsQuivers || !isWeaponOrQuiver))
            {
                fireRes = mods?.Find(s => s.Group.Contains("FireResistance"))?.Values[0] ?? 0;
                coldRes = mods?.Find(s => s.Group.Contains("ColdResistance"))?.Values[0] ?? 0;
                lightningRes = mods?.Find(s => s.Group.Contains("LightningResistance"))?.Values[0] ?? 0;
                chaosRes = mods?.Find(s => s.Group.Contains("ChaosResistance"))?.Values[0] ?? 0;
                allResMods = mods?.Find(s => s.Group.Contains("AllResistance"))?.Values[0] ?? 0;
                FireAndLightningResistance = mods?.Find(s => s.Group.Contains("FireAndLightningResistance"))?.Values[0] ?? 0;
                ColdAndLightningResistance = mods?.Find(s => s.Group.Contains("ColdAndLightningResistance"))?.Values[0] ?? 0;
                FireAndColdResistance = mods?.Find(s => s.Group.Contains("FireAndColdResistance"))?.Values[0] ?? 0;







                allResSummed = (FireAndColdResistance + ColdAndLightningResistance + FireAndLightningResistance) * 2 + allResMods * 3 + fireRes + coldRes + lightningRes + chaosRes;
                interestingThings.Add("TotalRes: " + allResSummed);
                weights.Add(Settings.TotalResistScore * allResSummed);
            }

            int lifeSum = 0;
            if (anyLife && !item.Path.Contains("/Weapons/"))
            {
                var life = mods.FindAll(s => s.Group.Contains("IncreasedLife"));
                lifeSum = life.Sum(element => element.Values[0]);
                interestingThings.Add("LIFE +" + lifeSum);
                weights.Add(Settings.LifeScore * lifeSum);
            }

            bool castSpeed = mods.Any(s => s.Group.Contains("IncreasedCastSpeed"));
            int castSpeedValue = 0;
            if (castSpeed)
            {
                castSpeedValue = mods.Find(s => s.Group.Contains("IncreasedCastSpeed")).Values[0];
                interestingThingsCaster.Add("Cast Speed +" + castSpeedValue);
                weightsCaster.Add(Settings.CastSpeedScore * castSpeedValue);
            }


            int PhysPercentValue = 0;
            int PhysPercentAccValue = 0;
            if (true)
            {
                bool PhysPercent = mods.Any(s => s.Group.Contains("LocalPhysicalDamagePercent") || s.Group.Contains("LocalIncreasedPhysicalDamagePercentAndAccuracyRating")); //LocalIncreasedPhysicalDamagePercentAndAccuracyRating

                if (PhysPercent)
                {
                    PhysPercentValue = mods?.Find(s => s.Group.Contains("LocalPhysicalDamagePercent"))?.Values[0] ?? 0;
                    PhysPercentAccValue = mods?.Find(s => s.Group.Contains("LocalIncreasedPhysicalDamagePercentAndAccuracyRating"))?.Values[0] ?? 0;
                    interestingThingsAttack.Add("Phy % +" + (PhysPercentValue + PhysPercentAccValue));
                    weightsAttack.Add(Settings.PercPhysScore * (PhysPercentValue + PhysPercentAccValue));
                }

                
                double oneHandMultiplier = 1;
                bool FlatPhys = mods.Any(s => s.Group.Contains("LocalAddedPhysicalDamage"));
                double PhysFlatValue = 0f;
                if (!mods.Any(s => s.Name.Contains("TwoHand")))
                {
                    oneHandMultiplier = 1.85;
                }
                if (FlatPhys)
                {
                    double AttackTime = item.GetComponent<Weapon>().AttackTime;
                    double weaponAPS = 1000f / item.GetComponent<Weapon>().AttackTime;
                    PhysFlatValue = mods.Find(s => s.Name.Contains("LocalAddedPhysicalDamage")).Values.Average() * oneHandMultiplier * weaponAPS;
                    interestingThingsAttack.Add("FLAT Phy +" + PhysFlatValue);
                    weightsAttack.Add((int)(Settings.FlatPhysScore * PhysFlatValue));
                }
            }
            double FlatFireValue = 0f;
            double FlatLightValue = 0f;
            double FlatColdValue = 0f;
            int socketedBowSkillsGemValue = 0;
            int socketedBowGemValue = 0;

            if (className.Contains("Bow") || className.Contains("Claw") || className.Contains("Dagger")) 
            {

                double AttackTime = item.GetComponent<Weapon>().AttackTime;
                double weaponAPS = 1000f / item.GetComponent<Weapon>().AttackTime;
                double oneHandMultiplier = 1;
                bool FlatFire = mods.Any(s => s.Name.Contains("LocalAddedFireDamage"));
                
                if (!mods.Any(s => s.Name.Contains("TwoHand"))){
                    oneHandMultiplier = 1.85;
                }

                
                if (FlatFire)
                {
                    FlatFireValue = mods.Find(s => s.Name.Contains("LocalAddedFireDamage")).Values.Average() * oneHandMultiplier* weaponAPS;
                    interestingThingsAttackEle.Add("FLAT Fire +" + FlatFireValue);
                    weightsAttackEle.Add((int)(Settings.FlatFireScore * FlatFireValue));
                }

                bool FlatLight = mods.Any(s => s.Name.Contains("LocalAddedLightningDamage"));

                if (FlatLight)
                {
                    FlatLightValue = mods.Find(s => s.Name.Contains("LocalAddedLightningDamage")).Values.Average() * oneHandMultiplier * weaponAPS;
                    interestingThingsAttackEle.Add("FLAT Light +" + FlatLightValue);
                    weightsAttackEle.Add((int)(Settings.FlatLightScore * FlatLightValue));
                }


                bool FlatCold = mods.Any(s => s.Name.Contains("LocalAddedColdDamage"));

                if (FlatCold)
                {
                    FlatColdValue = mods.Find(s => s.Name.Contains("LocalAddedColdDamage")).Values.Average() * oneHandMultiplier * weaponAPS;
                    interestingThingsAttackEle.Add("FLAT Cold +" + FlatColdValue);
                    weightsAttackEle.Add((int)(Settings.FlatColdScore * FlatColdValue));
                }

                // onehand



                bool BowGem = mods.Any(s => s.Name.Contains("LocalIncreaseSocketedBowGemLevel1"));

                if (BowGem)
                {
                    socketedBowSkillsGemValue = mods.Find(s => s.Name.Contains("LocalIncreaseSocketedBowGemLevel")).Values[0];
                    interestingThingsAttack.Add("Bow gem +" + socketedBowSkillsGemValue);
                    weightsAttack.Add((int)(Settings.PlusSocketedBowGems * socketedBowSkillsGemValue));
                }

                bool socketedGem = mods.Any(s => s.Name.Contains("LocalIncreaseSocketedGemLevel"));

                if (socketedGem)
                {
                    socketedBowGemValue = mods.Find(s => s.Name.Contains("LocalIncreaseSocketedGemLevel")).Values[0];
                    interestingThingsAttack.Add("GEM +" + socketedBowGemValue);
                    weightsAttack.Add((int)(Settings.PlusSocketedGems * socketedBowGemValue));
                }
            }
            bool reducedFlaskChargesUsed = mods.Any(s => s.Name.Contains("BeltReducedFlaskChargesUsed"));
            int reducedFlaskChargesUsedValue = 0;
            if (reducedFlaskChargesUsed)
            {
                reducedFlaskChargesUsedValue = mods.Find(s => s.Name.Contains("BeltReducedFlaskChargesUsed")).Values[0] * -1;

                interestingThings.Add("RedCharges +" + reducedFlaskChargesUsedValue);
                weights.Add((int)(Settings.ReducedFlaskChargesUsed * reducedFlaskChargesUsedValue));
            }

            bool BeltFlaskDuration = mods.Any(s => s.Group.Contains("BeltFlaskDuration"));
            int BeltFlaskDurationValue = 0;
            if (BeltFlaskDuration)
            {
                BeltFlaskDurationValue = mods.Find(s => s.Group.Contains("BeltFlaskDuration")).Values[0];
                interestingThings.Add("FlaskDur +" + BeltFlaskDurationValue);
                weights.Add((int)(Settings.FlaskDuration * BeltFlaskDurationValue));
            }

            bool BeltFlaskEffect = mods.Any(s => s.Group.Contains("BeltFlaskEffect"));
            int BeltFlaskEffectValue = 0;
            if (BeltFlaskEffect)
            {
                BeltFlaskEffectValue = mods.Find(s => s.Group.Contains("BeltFlaskEffect")).Values[0];
                interestingThings.Add("FlaskEffect +" + BeltFlaskEffectValue);
                weights.Add((int)(Settings.FlaskEffect * BeltFlaskEffectValue));
            }

            bool FlaskChargesGained = mods.Any(s => s.Name.Contains("BeltIncreasedFlaskChargesGained"));
            int BeltIncreasedFlaskChargesGainedValue = 0;
            if (FlaskChargesGained)
            {
                BeltIncreasedFlaskChargesGainedValue = mods.Find(s => s.Name.Contains("BeltIncreasedFlaskChargesGained")).Values[0];
                interestingThings.Add("FlaskGained +" + BeltIncreasedFlaskChargesGainedValue);
                weights.Add((int)(Settings.IncreasedFlaskChargesGained * BeltIncreasedFlaskChargesGainedValue));
            }

            bool LifeRegeneration = mods.Any(s => s.Group.Contains("LifeRegeneration") && !s.Name.Contains("Implicit"));
            int LifeRegenerationValue = 0;
            if (LifeRegeneration)
            {
                LifeRegenerationValue = mods.Find(s => s.Group.Contains("LifeRegeneration") && !s.Name.Contains("Implicit")).Values[0] / 60;
                interestingThings.Add("LifeRegeneration +" + LifeRegenerationValue);
                weights.Add((int)(Settings.LifeRegeneration * LifeRegenerationValue));
            }

            bool LifeRecoup = mods.Any(s => s.Group.Contains("LifeRecoup"));
            int LifeRecoupValue = 0;
            if (LifeRecoup)
            {
                LifeRecoupValue = mods.Find(s => s.Group.Contains("LifeRecoup")).Values[0];
                interestingThings.Add("LifeRecoup +" + LifeRecoupValue);
                weights.Add((int)(Settings.LifeRecoup * LifeRecoupValue));
            }


            int ChanceToSuppressSpellsValue = 0;
            bool ChanceToSuppressSpells = mods.Any(s => s.Group.Contains("ChanceToSuppressSpells") && !s.Name.Contains("Implicit"));
            if (ChanceToSuppressSpells)
            {
                ChanceToSuppressSpellsValue = mods.Find(s => s.Group.Contains("ChanceToSuppressSpells") && !s.Name.Contains("Implicit")).Values[0];
                interestingThings.Add("Suppress +" + ChanceToSuppressSpellsValue);
                weights.Add((int)(Settings.SuppressSpells * ChanceToSuppressSpellsValue));
            }
            double AddedFireDamageValue = 0;
            double AddedColdDamageValue = 0;
            double AddedLightDamageValue = 0;
            double AddedPhysicalDamageValue = 0;
            //if (!className.Contains("Dagger") && !className.Contains("Claw") && !className.Contains("Sceptre") && !className.Contains("Axe") && !className.Contains("Wand") && !className.Contains("Mace") && !className.Contains("Sword"))
            if (!item.Path.Contains("/Weapons/"))
            {


                bool AddedFireDamage = mods.Any(s => s.Name.Contains("AddedFireDamage") && !s.Name.Contains("Spell") && !s.Name.Contains("TwoHand") && !s.Name.Contains("Implicit"));
                // jewelry and gloves added damage 
                if (AddedFireDamage)
                {
                    AddedFireDamageValue = mods.Find(s => s.Name.Contains("AddedFireDamage") && !s.Name.Contains("Spell") && !s.Name.Contains("TwoHand") && !s.Name.Contains("Implicit")).Values.Average();
                    interestingThingsAttackEle.Add("FLAT Fire +" + AddedFireDamageValue);
                    weightsAttackEle.Add((int)(Settings.AddedFireDamageScore * AddedFireDamageValue));
                }


                bool AddedColdDamage = mods.Any(s => s.Name.Contains("AddedColdDamage") && !s.Name.Contains("Spell") && !s.Name.Contains("TwoHand") && !s.Name.Contains("Implicit"));
                // jewelry and gloves added damage 
                if (AddedColdDamage)
                {
                    AddedColdDamageValue = mods.Find(s => s.Name.Contains("AddedColdDamage") && !s.Name.Contains("Spell") && !s.Name.Contains("TwoHand") && !s.Name.Contains("Implicit")).Values.Average();
                    interestingThingsAttackEle.Add("FLAT Cold +" + AddedColdDamageValue);
                    weightsAttackEle.Add((int)(Settings.AddedColdDamageScore * AddedColdDamageValue));
                }


                bool AddedLightDamage = mods.Any(s => s.Name.Contains("AddedLightningDamage") && !s.Name.Contains("Spell") && !s.Name.Contains("TwoHand") && !s.Name.Contains("Implicit"));
                // jewelry and gloves added damage 
                if (AddedLightDamage)
                {

                    AddedLightDamageValue = mods.Find(s => s.Name.Contains("AddedLightningDamage") && !s.Name.Contains("Spell") && !s.Name.Contains("TwoHand") && !s.Name.Contains("Implicit")).Values.Average();
                    interestingThingsAttackEle.Add("FLAT Light +" + AddedLightDamageValue);
                    weightsAttackEle.Add((int)(Settings.AddedLightDamageScore * AddedLightDamageValue));
                }


                bool AddedPhysicalDamage = mods.Any(s => s.Name.Contains("AddedPhysicalDamage") && !s.Name.Contains("Spell") && !s.Name.Contains("TwoHand") && !s.Name.Contains("Implicit"));
                // jewelry and gloves added damage 
                if (AddedPhysicalDamage)
                {
                    AddedPhysicalDamageValue = mods.Find(s => s.Name.Contains("AddedPhysicalDamage") && !s.Name.Contains("Spell") && !s.Name.Contains("TwoHand") && !s.Name.Contains("Implicit")).Values.Average();
                    interestingThingsAttack.Add("FLAT Phys +" + AddedPhysicalDamageValue);
                    weightsAttack.Add((int)(Settings.AddedPhysicalDamageScore * AddedPhysicalDamageValue));
                }
                
            }
            double AddedFireDamageQuiverValue = 0;
            double AddedLightDamageQuiverValue = 0;
            double AddedColdDamageQuiverValue = 0;
            double AddedPhysicalDamageQuiverValue = 0;
            // quiver
            //bool AddedFireDamageQuiver = mods.Any(s => s.Name.Contains("AddedFireDamageQuiver") && !s.Name.Contains("Implicit"));
            //if (AddedFireDamageQuiver)
            //{
            //    AddedFireDamageQuiverValue = mods.Find(s => s.Name.Contains("AddedFireDamageQuiver") && !s.Name.Contains("Implicit")).Values.Average();
            //    interestingThings.Add("FLAT Fire +" + AddedFireDamageQuiverValue);
            //    weights.Add((int)(Settings.AddedFireDamageQuiverScore * AddedFireDamageQuiverValue));
            //}
            //bool AddedLightDamageQuiver = mods.Any(s => s.Name.Contains("AddedLightningDamageQuiver") && !s.Name.Contains("Implicit"));
            //if (AddedLightDamageQuiver)
            //{
            //    AddedLightDamageQuiverValue = mods.Find(s => s.Name.Contains("AddedLightningDamageQuiver") && !s.Name.Contains("Implicit")).Values.Average();
            //    interestingThings.Add("FLAT Light +" + AddedLightDamageQuiverValue);
            //    weights.Add((int)(Settings.AddedLightDamageQuiverScore * AddedLightDamageQuiverValue));
            //}
            //bool AddedColdDamageQuiver = mods.Any(s => s.Name.Contains("AddedColdDamageQuiver") && !s.Name.Contains("Implicit"));
            //if (AddedColdDamageQuiver)
            //{
            //    AddedColdDamageQuiverValue = mods.Find(s => s.Name.Contains("AddedColdDamageQuiver") && !s.Name.Contains("Implicit")).Values.Average();
            //    interestingThings.Add("FLAT Cold +" + AddedColdDamageQuiverValue);
            //    weights.Add((int)(Settings.AddedColdDamageQuiverScore * AddedColdDamageQuiverValue));
            //}

            //bool AddedPhysicalDamageQuiver = mods.Any(s => s.Name.Contains("AddedPhysicalDamageQuiver") && !s.Name.Contains("Implicit"));
            //if (AddedPhysicalDamageQuiver)
            //{
            //    AddedPhysicalDamageQuiverValue = mods.Find(s => s.Name.Contains("AddedPhysicalDamageQuiver") && !s.Name.Contains("Implicit")).Values.Average();
            //    interestingThings.Add("FLAT Physical +" + AddedPhysicalDamageQuiverValue);
            //    weights.Add((int)(Settings.AddedPhysicalDamageQuiverScore * AddedPhysicalDamageQuiverValue));
            //}

            // bow specific

            bool damageWithBowSkills = mods.Any(s => s.Name.Contains("damageWithBowSkills"));
            int damageWithBowSkillsValue = 0;
            if (damageWithBowSkills)
            {
                damageWithBowSkillsValue = mods.Find(s => s.Name.Contains("damageWithBowSkills")).Values[0];
                interestingThingsAttack.Add("dmgBows +" + damageWithBowSkillsValue);
                weightsAttack.Add((int)(Settings.damageWithBowSkillsScore * damageWithBowSkillsValue));
            }


            bool critWithBowSkills = mods.Any(s => s.Name.Contains("critWithBowSkills"));
            int critWithBowSkillsValue = 0;
            if (critWithBowSkills)
            {
                critWithBowSkillsValue = mods.Find(s => s.Name.Contains("critWithBowSkills")).Values[0];
                interestingThingsAttack.Add("critBows +" + critWithBowSkillsValue);
                weightsAttack.Add((int)(Settings.critWithBowSkillsScore * critWithBowSkillsValue));
            }

            bool critMultiWithBowSkills = mods.Any(s => s.Name.Contains("critMultiWithBowSkills"));
            int critMultiWithBowSkillsValue = 0;
            if (critMultiWithBowSkills)
            {
                critMultiWithBowSkillsValue = mods.Find(s => s.Name.Contains("critMultiWithBowSkills")).Values[0];
                interestingThingsAttack.Add("critMultiBows +" + critMultiWithBowSkillsValue);
                weightsAttack.Add((int)(Settings.critMultiWithBowSkillsScore * critMultiWithBowSkillsValue));
            }

            bool dotMultiWithBowSkills = mods.Any(s => s.Name.Contains("dotMultiWithBowSkills"));
            int dotMultiWithBowSkillsValue = 0;
            if (dotMultiWithBowSkills)
            {
                dotMultiWithBowSkillsValue = mods.Find(s => s.Name.Contains("dotMultiWithBowSkills")).Values[0];
                interestingThingsAttack.Add("dotMultiBows +" + dotMultiWithBowSkillsValue);
                weightsAttack.Add((int)(Settings.dotMultiBowsScore * dotMultiWithBowSkillsValue));
            }

            bool plusArrows = mods.Any(s => s.Name.Contains("plusArrows"));
            int plusArrowsValue = 0;
            if (plusArrows)
            {
                plusArrowsValue = mods.Find(s => s.Name.Contains("plusArrows")).Values[0];
                interestingThingsAttack.Add("plusArrows +" + plusArrowsValue);
                weightsAttack.Add((int)(Settings.plusArrowsScore * plusArrowsValue));
            }

            //




            int attackSpeedValue = 0;
            bool attackSpeed = mods.Any(s => s.Group.Contains("IncreasedAttackSpeed"));
            if (attackSpeed && !item.Path.Contains("/Weapons/"))
            {
                attackSpeedValue = mods.Find(s => s.Group.Contains("IncreasedAttackSpeed")).Values[0];
                interestingThingsAttack.Add("IAS +" + attackSpeedValue);
                weightsAttack.Add((Settings.IncreasedAttackSpeedScore * attackSpeedValue));
                interestingThingsAttackEle.Add("IAS +" + attackSpeedValue);
                weightsAttackEle.Add((Settings.IncreasedAttackSpeedScore * attackSpeedValue));
            }
            int attackSpeedWeaponValue = 0;
            if (attackSpeed && !item.Path.Contains("/Weapons/"))
            {
                attackSpeedValue = mods.Find(s => s.Group.Contains("IncreasedAttackSpeed")).Values[0];
                interestingThingsAttack.Add("IAS +" + attackSpeedWeaponValue);
                weightsAttack.Add((Settings.IncreasedAttackSpeedWeaponScore * attackSpeedWeaponValue));
                interestingThingsAttackEle.Add("IAS +" + attackSpeedWeaponValue);
                weightsAttackEle.Add((Settings.IncreasedAttackSpeedWeaponScore * attackSpeedWeaponValue));
            }

            //accuracy
            int accuracyValue = 0;
            bool accuracy = mods.Any(s => s.Name.Contains("LocalIncreasedAccuracy"));
            bool accuracyPercentPhys = mods.Any(s => s.Group.Contains("LocalIncreasedPhysicalDamagePercentAndAccuracyRating"));
            if (accuracy || accuracyPercentPhys)
            {
                accuracyValue = mods?.Find(s => s.Name.Contains("LocalIncreasedAccuracy"))?.Values[0] ?? 0;
                PhysPercentAccValue = mods?.Find(s => s.Group.Contains("LocalIncreasedPhysicalDamagePercentAndAccuracyRating"))?.Values[1] ?? 0;
                interestingThingsAttack.Add("Flat acc +" + accuracyValue);
                weightsAttack.Add((Settings.accuracyScore * accuracyValue));
                interestingThingsAttackEle.Add("Flat acc +" + accuracyValue);
                weightsAttackEle.Add((Settings.accuracyScore * accuracyValue));
            }
            int accuracyPercentValue = 0;
            bool accuracyPercent = mods.Any(s => s.Group.Contains("AccuracyPercent"));
           

            
            if (accuracyPercent)
            {
                accuracyPercentValue = mods.Find(s => s.Group.Contains("AccuracyPercent")).Values[0];
                interestingThingsAttack.Add("Acc% +" + accuracyPercentValue);
                weightsAttack.Add((Settings.accuracyPercentScore * accuracyPercentValue));
                interestingThingsAttackEle.Add("Acc% +" + accuracyPercentValue);
                weightsAttackEle.Add((Settings.accuracyPercentScore * accuracyPercentValue));
            }
            // attributes
            int strenghtValue = 0;
            int allAttributesValue = 0;
            bool allAttributes = mods.Any(s => s.Group.Contains("AllAttributes"));

            if (allAttributes)
            {
                allAttributesValue = mods?.Find(s => s.Group.Contains("AllAttributes")).Values[0] ?? 0;
            }


            bool strenght = mods.Any(s => s.Group.Contains("Strength"));
            if (strenght && (Settings.IgnoreAttributesWeapons || !item.Path.Contains("/Weapons/")))
            {
                strenghtValue = mods.Find(s => s.Group.Contains("Strength")).Values[0];
                interestingThings.Add("Str +" + (strenghtValue + allAttributesValue));
                weights.Add((Settings.StrScore * (strenghtValue + allAttributesValue)));
            }

            int dexValue = 0;
            bool dexterity = mods.Any(s => s.Group.Contains("Dexterity"));
            if (dexterity && (Settings.IgnoreAttributesWeapons || !item.Path.Contains("/Weapons/")))
            {
                dexValue = mods.Find(s => s.Group.Contains("Dexterity")).Values[0];
                interestingThings.Add("Dex +" + (dexValue + allAttributesValue));
                weights.Add((Settings.DexScore * (dexValue + allAttributesValue)));
            }
            int intValue = 0;
            bool intelligence = mods.Any(s => s.Group.Contains("Intelligence"));
            if (intelligence && (Settings.IgnoreAttributesWeapons || !item.Path.Contains("/Weapons/")))
            {
                intValue = mods.Find(s => s.Group.Contains("Intelligence")).Values[0];
                interestingThings.Add("Int +" + (intValue + allAttributesValue));
                var total = intValue + allAttributesValue;

                weights.Add((Settings.IntScore * total));
            }

            // max res

            bool allMaxRes = mods.Any(s => s.Group.Contains("AllMaxRes"));
            int allMaxResValue = 0;

            if (allMaxRes)
            {
                allMaxResValue = mods?.Find(s => s.Group.Contains("AllMaxRes")).Values[0] ?? 0;
            }
            int maxColdResValue = 0;
            bool maxColdRes = mods.Any(s => s.Group.Contains("maxColdRes"));
            if (maxColdRes)
            {
                maxColdResValue = mods.Find(s => s.Group.Contains("maxColdRes")).Values[0];
                interestingThings.Add("maxColdRes +" + (maxColdResValue + allMaxResValue));
                weights.Add((Settings.maxColdResScore * (maxColdResValue + allMaxResValue)));
            }

            int maxFireResValue = 0;
            bool maxFireRes = mods.Any(s => s.Group.Contains("maxFireRes"));
            if (maxFireRes)
            {
                maxFireResValue = mods.Find(s => s.Group.Contains("maxFireRes")).Values[0];
                interestingThings.Add("maxFireRes +" + (maxFireResValue + allMaxResValue));
                weights.Add((Settings.maxFireResScore * (maxFireResValue + allMaxResValue)));
            }

            int maxLightningResValue = 0;
            bool maxLightningRes = mods.Any(s => s.Group.Contains("maxLightningRes"));
            if (maxLightningRes)
            {
                maxLightningResValue = mods.Find(s => s.Group.Contains("maxLightningRes")).Values[0];
                interestingThings.Add("maxLightningRes +" + (maxLightningResValue + allMaxResValue));
                weights.Add((Settings.maxLightningResScore * (maxLightningResValue + allMaxResValue)));
            }




            int playerLevel = GameController.Player.GetComponent<Player>().Level;
            float flaskRelevancy = 0;

            bool corruptingBloodImmunity = mods.Any(s => s.Group.Contains("corruptingBloodImmunity"));
            if (corruptingBloodImmunity)
            {
                flaskRelevancy = itemDropLevel / playerLevel;
                interestingThings.Add("CB Flask ");
                weights.Add((int)(Settings.corruptingBloodImmunityScore * flaskRelevancy));
            }


            bool freezeRemoval = mods.Any(s => s.Group.Contains("FreezeRemoval"));
            if (freezeRemoval)
            {

                interestingThings.Add("Freeze Flask");
                weights.Add((int)(Settings.freezeRemovalScore * flaskRelevancy));
            }

            bool instantFlask = mods.Any(s => s.Group.Contains("instantFlask"));
            if (instantFlask)
            {

                interestingThings.Add("Instant");
                weights.Add((int)(Settings.instantFlaskScore * flaskRelevancy));
            }

            bool InstantLowFlask = mods.Any(s => s.Group.Contains("instantFlaskLowLife"));
            if (InstantLowFlask)
            {

                interestingThings.Add("Instant LowLife");
                weights.Add((int)(Settings.instantLowLifeScore * flaskRelevancy));
            }

            double pDPS = 0;
            double eDPS = 0;
            if (isWeapon)
            {

                double baseMin = item.GetComponent<Weapon>().DamageMin;
                double baseMax = item.GetComponent<Weapon>().DamageMax;

                double elemental = AddedFireDamageValue + AddedColdDamageValue + AddedLightDamageValue;
                double physicalBase = AddedPhysicalDamageValue + ((baseMin + baseMax) / 2);
                double physPercentSum = (PhysPercentValue + PhysPercentAccValue);
                //LogMessage(physPercentSum + "physPercentSum", 5, Color.Red);
                double physicalIncrease = (physPercentSum / 100f) + 1f;
                double AttackTime = item.GetComponent<Weapon>().AttackTime;
                //LogMessage(AttackTime + "AttackTime", 5, Color.Red);
                double weaponAPS = 1000f / item.GetComponent<Weapon>().AttackTime;
                //LogMessage(AddedPhysicalDamageValue + "added phys", 5, Color.Red);
                //LogMessage(physicalIncrease.ToString("F"+2) + " physicalIncrease ", 5, Color.Red);
                //LogMessage(weaponAPS.ToString("F" + 2) + " weaponAPS ", 5, Color.Red);
                pDPS = physicalBase * physicalIncrease * weaponAPS;
                eDPS = elemental * weaponAPS;




            }
            var weightsAttackSum = weightsAttack.Sum();
            var weightsAttackEleSum = weightsAttackEle.Sum();
            var weightsCasterSum = weightsCaster.Sum();
            var weightsMinionSum = weightsMinion.Sum();

            if (Settings.DebugMode)
            {
                foreach (var interestingThing in interestingThings)
                {
                    LogMessage("General: " + interestingThing, 5, Color.Red);

                }
                foreach (var interestingThingMinion in interestingThingsMinion)
                {
                    LogMessage("Minion: " + interestingThingMinion, 5, Color.Red);

                }
                foreach (var interestingThingCaster in interestingThingsCaster)
                {
                    LogMessage("Caster: " + interestingThingCaster, 5, Color.Red);

                }
                foreach (var interestingThingAttack in interestingThingsAttack)
                {
                    LogMessage("Attack: " + interestingThingAttack, 5, Color.Red);

                }
                foreach (var interestingThingAttackEle in interestingThingsAttackEle)
                {
                    LogMessage("AttackEle: " + interestingThingAttackEle, 5, Color.Red);

                }



                LogMessage("Attack weights: " + weightsAttackSum + " Count: " + weightsAttack.Count, 5, Color.Yellow);
                LogMessage("AttackEle weights: " + weightsAttackEleSum + " Count: " + weightsAttackEle.Count, 5, Color.Yellow);
                LogMessage("Caster weights: " + weightsCasterSum + " Count: " + weightsCaster.Count, 5, Color.Yellow);
                LogMessage("Minion weights: " + weightsMinionSum + " Count: " + weightsMinion.Count, 5, Color.Yellow);

            }
            // fixing some weights depending on the weapon
            // no one wants ele sceptre or ele mace

            if (className.Contains("Mace") || className.Contains("taff") || className.Contains("Sword"))
            {
                weightsAttackEleSum = 0;
            }

            if (className.Contains("Sceptre") || className.Contains("Rune"))
            {
                weightsAttackEleSum = 0;
                weightsAttackSum = 0;
            }


            if (className.Contains("Mace") || className.Contains("taff") || className.Contains("Sword"))
            {
                weightsCasterSum = 0;
                weightsMinionSum = 0;
            }


            var type = "";

            if (weightsAttackSum == weightsCasterSum && weightsAttackSum == weightsMinionSum && weightsAttackSum == weightsAttackEleSum)
            {
                type = "";

            }
            else if (weightsAttackSum >= weightsCasterSum && weightsAttackSum >= weightsMinionSum && weightsAttackSum >= weightsAttackEleSum)
            {
                type = "Attack ";
                interestingThings.AddRange(interestingThingsAttack);
                weights.AddRange(weightsAttack);
            }
            else

           if (weightsCasterSum >= weightsAttackSum && weightsCasterSum >= weightsMinionSum && weightsCasterSum >= weightsAttackEleSum)
            {
                type = "Caster ";
                interestingThings.AddRange(interestingThingsCaster);
                weights.AddRange(weightsCaster);
            }
            else
           if (weightsMinionSum >= weightsAttackSum && weightsMinionSum >= weightsCasterSum && weightsMinionSum >= weightsAttackEleSum)
            {
                type = "Minion ";
                interestingThings.AddRange(interestingThingsMinion);
                weights.AddRange(weightsMinion);
            }
            else if (weightsAttackEleSum >= weightsCasterSum && weightsAttackEleSum >= weightsMinionSum && weightsAttackEleSum >= weightsAttackSum)
            {
                type = "AttackEle ";
                interestingThings.AddRange(interestingThingsAttackEle);
                weights.AddRange(weightsAttackEle);
            }
            if (Settings.DebugMode)
                LogMessage("Type: " + type, 5, Color.Green);

            var pairOfLists = new Tuple<List<int>, List<string>, double, double, string>(weights, interestingThings, pDPS, eDPS, type);
            return pairOfLists;
        }

        private void HiglightAllVendorShop(IList<ServerInventory.InventSlotItem> items, int rows, int columns)
        {
            var ingameState = GameController.Game.IngameState;
            var serverData = ingameState.ServerData;
            var bonusComp = serverData.BonusCompletedAreas;


            var playerLevel = GameController.Player.GetComponent<Player>().Level;

            var hoveredItem = ingameState.UIHover.AsObject<HoverItemIcon>();
            var UIHoverEntity = ingameState.UIHover.Entity;
            var hoveredItemTooltip = hoveredItem?.Tooltip;
            var tooltipRect = hoveredItemTooltip?.GetClientRect();


            var playerLevelOverride = Math.Min(60, playerLevel);
            if (Settings.PlayerLevelOverrideDebug)
            {
                playerLevelOverride = Math.Min(60, Settings.PlayerLevelOverride);
            }



            foreach (var item in items)
            {
                List<int> weights = new List<int>();

                if (item == null) continue;


                if (item.Item.Path.Contains("Currency") || item.Item.Path.Contains("Gems")) continue;


                var drawRect = item.GetClientRect();

                var itemMods = item?.Item.GetComponent<Mods>();
                var itemSockets = item?.Item.GetComponent<Sockets>();
                var itemName = item?.Item.GetComponent<Base>().Name;
                var baseItemType = GameController.Files.BaseItemTypes.Translate(item.Item.Path);
                var className = baseItemType.ClassName;

                bool isRGB = itemSockets?.IsRGB ?? false;
                int links = itemSockets?.LargestLinkSize ?? 0;
                int sockets = itemSockets?.NumberOfSockets ?? 0;
                var stats = itemMods?.HumanStats;
                var mods = itemMods?.ItemMods;

                List<string> interestingThings = new List<string>();


                var auxChildrenFirst = ingameState?.IngameUi?.PurchaseWindow;

                var auxpurchaseWindow = auxChildrenFirst?.GetChildAtIndex(7)?.GetChildFromIndices(1);
                if (auxpurchaseWindow != null)
                {
                    var purchaseWindow = auxpurchaseWindow?.GetChildFromIndices(inventoryIndex - 1);
                    var squareWidth = (int)purchaseWindow?.GetClientRect().Width / columns;
                    var squareHeight = (int)purchaseWindow?.GetClientRect().Height / rows;
                    var initialTradeWindowX = (int)purchaseWindow?.GetClientRect().TopLeft.X;
                    var initialTradeWindowY = (int)purchaseWindow?.GetClientRect().TopLeft.Y;

                    var itemRectPosX = initialTradeWindowX + (item.PosX * squareWidth);
                    var itemRectPosY = initialTradeWindowY + (item.PosY * squareHeight);
                    var itemRectWidth = squareWidth * item.SizeX;
                    var itemRectHeight = squareHeight * item.SizeY;

                    var drawRectFix = new RectangleF(itemRectPosX, itemRectPosY, itemRectWidth, itemRectHeight);


                    drawRectFix.Top += 5;
                    //drawRectFix.Bottom += offset ;
                    //drawRectFix.Right += offset ;
                    drawRectFix.Left += 5;

                    bool isPageVisible = false;
                    if (purchaseWindow != null)
                    {
                        isPageVisible = purchaseWindow.IsVisible;
                    }


                    bool anyMatch = PartialClassNamesToIgnore.Any(className.Contains);


                    //bool anyMatch = PartialClassNamesToIgnore.Any(item => item.Contains(className));


                    var type = "";
                    if (!anyMatch)
                    {
                        var modCheckReturn = modCheck(item.Item);

                        interestingThings.AddRange(modCheckReturn.Item2);
                        weights.AddRange(modCheckReturn.Item1);
                        var pDPS = modCheckReturn.Item3;
                        var eDPS = modCheckReturn.Item4;
                        type = modCheckReturn.Item5;
                    }

                    if (links == 6)
                    {
                        interestingThings.Add("SIX LINK POGGERS");
                        weights.Add(Settings.SixLinkScore);
                    }
                    else if (links == 5)
                    {
                        interestingThings.Add("FIVE LINK");
                        weights.Add(Settings.FiveLinkScore);
                    }
                    else if (sockets == 6)
                    {
                        interestingThings.Add("SIX SOCKET");
                        weights.Add(Settings.SixSocketScore);
                    }
                    else if (links == 4 && !anyMatch)
                    {

                        var linkColors = itemSockets.SocketGroup.First(s => s.Length == 4);
                        var orderedLinkColors = String.Concat(linkColors.OrderByDescending(c => c));

                        //LogMessage("4L " + linkColors + " ordered " + orderedLinkColors, 5, Color.Green);
                        if (FourLinkStringList.Contains(orderedLinkColors) && playerLevelOverride <= Settings.FourLinkCharacterLevelThreshold)
                        {
                            interestingThings.Add(orderedLinkColors + " 4L " + className);
                            weights.Add(Settings.FourLinkScore);
                        }

                    }
                    else if (links == 3 && !anyMatch)
                    {

                        var linkColors = itemSockets.SocketGroup.First(s => s.Length == 3);
                        var orderedLinkColors = String.Concat(linkColors.OrderByDescending(c => c));

                        //LogMessage("3L " + linkColors + " ordered " + orderedLinkColors, 5, Color.Green);
                        if (ThreeLinkStringList.Contains(orderedLinkColors) && playerLevelOverride <= Settings.ThreeLinkCharacterLevelThreshold)
                        {
                            interestingThings.Add(orderedLinkColors + " 3L " + className);
                            weights.Add(Settings.ThreeLinkScore);
                        }

                    }


                    if (isRGB)
                    {
                        interestingThings.Add("RGB");
                        weights.Add(Settings.RGBScore);

                    }


                    //INTERESTINGTHINGS LIST
                    //WEIGHTS LIST


                    if (interestingThings.Count() > 0)
                    {

                        var scaleFactor = 5.2f - (0.07 * playerLevelOverride);
                        int finalScore = 0;
                        double pDPS = 0;
                        double eDPS = 0;
                        if (Settings.UseScoreLevelScaler)
                        {
                            finalScore = (int)(weights.Sum() * scaleFactor);
                        }
                        else
                        {
                            finalScore = (int)(weights.Sum());
                        }

                        string concatInterestingStuff = type + className + " " + string.Join(",", interestingThings);
                        var newItem = (className, concatInterestingStuff, inventoryIndex, finalScore, pDPS, eDPS);

                        // Check if the item exists before adding
                        if (!DataExists(interestingItems, newItem))
                        {
                            interestingItems.Add(newItem);
                        }

                        var colorBorder = Color.White;
                        var drawBox = new RectangleF(drawRect.X, drawRect.Y - 30, drawRect.Width, drawRect.Height);
                        var drawBoxScour = new RectangleF(drawRect.X + drawRect.Width / 4, drawRect.Y + drawRect.Height / 4, drawRect.Width / 2, drawRect.Height / 2);
                        var drawBox2 = new RectangleF(drawRect.X + 2, drawRect.Y + 2, drawRect.Width - 4, drawRect.Height - 4);



                        if (finalScore >= Settings.STierThreshold)
                        {
                            colorBorder = Settings.STierThresholdColor;

                        }
                        else if (finalScore >= Settings.ATierThreshold)
                        {
                            colorBorder = Settings.ATierThresholdColor;
                        }
                        else if (finalScore >= Settings.BTierThreshold)
                        {
                            colorBorder = Settings.BTierThresholdColor;
                        }
                        else if (finalScore >= Settings.CTierThreshold)
                        {
                            colorBorder = Settings.CTierThresholdColor;
                        }
                        var canDraw = false;
                        if (tooltipRect == null)
                        {
                            canDraw = true;
                        }
                        else
                        {
                            canDraw = !checkRectOverlaps(drawRectFix, (RectangleF)tooltipRect);
                        }


                        if (isPageVisible && canDraw && finalScore >= Settings.MinScoreThreshold.Value)
                        {
                            Graphics.DrawFrame(drawRectFix, colorBorder, 5);
                            var isWeapon = item.Item.Path.Contains("/Weapons/");
                            if (isWeapon)
                            {
                                var textPosX = drawRectFix.TopLeft.X + 5;
                                var textPosY = drawRectFix.TopLeft.Y + 5;
                                if (pDPS > 0)
                                {
                                    Graphics.DrawText("P: " + pDPS.ToString("F0"), new Vector2N(textPosX, textPosY));
                                    textPosY += Settings.TextSpacing;
                                }
                                if (eDPS > 0)
                                {
                                    Graphics.DrawText("E: " + eDPS.ToString("F0"), new Vector2N(textPosX, textPosY), Color.Yellow);
                                }
                                //Graphics.DrawFrame(drawRect, Color.White, 3);
                            }
                        }

                    }
                }


            }
        }










    }
}