using System;
using System.Collections.Generic;
using System.Linq;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.Elements;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using SharpDX;
using Vector2N = System.Numerics.Vector2;

namespace VendorItems
{
    public class VendorItems : BaseSettingsPlugin<VendorItemsSettings>
    {

        private Vector2N drawTextVector3;
        static bool DataExists(List<(string, string, int,int)> list, (string, string, int,int) item)
        {
            foreach ((string firstName, string lastName, int age,int score) in list)
            {
                if (firstName == item.Item1 && lastName == item.Item2 && age == item.Item3 && score == item.Item4)
                {
                    return true;
                }
            }
            return false;
        }

        List<(string, string, int,int)> interestingItems = new List<(string, string, int,int)>();
        List<(string, string, int, int)> interestingItemsPurchase = new List<(string, string, int, int)>();
        List<string> FourLinkStringList = new List<string>();
        List<string> ThreeLinkStringList = new List<string>();
        List<string> PartialClassNamesToIgnore = new List<string>();
        int inventoryIndex = 1;

        public override bool Initialise()
        {
            return true;
        }

        public override void Render()
        {
            updateSettings();
            VendorShop();
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

        }
        private void printInterestItems()
        {
            var ingameState = GameController.Game.IngameState;
            var auxChildrenFirst = ingameState.IngameUi.PurchaseWindow;
            
            if (auxChildrenFirst.IsVisible && interestingItems.Count() > 0) { 

                Vector2 newInfoPanel = new Vector2(000, 500);
                var drawBox = new RectangleF(newInfoPanel.X, newInfoPanel.Y, 306, 200);
                List<(string, string, int, int)> interestingItemsSorted = interestingItems.FindAll(pair => pair.Item4 > Settings.MinScoreThreshold).OrderByDescending(pair => pair.Item4).ToList();
                var drawBoxFix = drawBox;
                drawBoxFix.Height = (interestingItemsSorted.Count() * 10) + 5;
                Color newColorBlack = new Color(Color.Black.R, Color.Black.G, Color.Black.B, Convert.ToByte(200));
                Color newColorWhite = new Color(Color.White.R, Color.White.G, Color.White.B, Convert.ToByte(100));            

               
                Graphics.DrawBox(drawBoxFix, newColorBlack, 5);
                Graphics.DrawFrame(drawBoxFix, newColorWhite, 2);

                foreach ((string className, string itemType, int inventoryPage, int score) in interestingItemsSorted)
                    {
                    var baseColor = Color.White;
                    if (Settings.DebugMode)
                    {
                        LogMessage("Interesting item : " + className + " IS : " + "" + itemType + " On page: " + inventoryPage.ToString(), 5, Color.Green);
                    }

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

                    drawTextVector3 = Graphics.DrawText(inventoryPage.ToString() + " " + className + " " + itemType + " \t " + score.ToString(), newInfoPanel, baseColor);
                    
                    newInfoPanel.Y += 10;
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

        private void HiglightAllVendorShop(IList<ServerInventory.InventSlotItem> items, int rows, int columns)
        {
            var ingameState = GameController.Game.IngameState;
            var serverData = ingameState.ServerData;
            var bonusComp = serverData.BonusCompletedAreas;
            var comp = serverData.CompletedAreas;
            var shEld = serverData.ShaperElderAreas;
            var playerLevel = GameController.Player.GetComponent<Player>().Level;
            var inventoryItemIcon = ingameState.UIHover.AsObject<HoverItemIcon>();
            var UIHoverEntity = ingameState.UIHover.Entity;
            var tooltip = inventoryItemIcon?.Tooltip;
            
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
                if(auxpurchaseWindow != null) { 
                var purchaseWindow = auxpurchaseWindow?.GetChildFromIndices(inventoryIndex-1);
                var squareWidth = (int)purchaseWindow?.GetClientRect().Width / columns;
                var squareHeight = (int)purchaseWindow?.GetClientRect().Height/ rows;
                var initialTradeWindowX = (int)purchaseWindow?.GetClientRect().TopLeft.X;
                var initialTradeWindowY = (int)purchaseWindow?.GetClientRect().TopLeft.Y;

                var itemRectPosX = initialTradeWindowX + (item.PosX * squareWidth);
                var itemRectPosY = initialTradeWindowY + (item.PosY * squareHeight);
                var itemRectWidth = squareWidth * item.SizeX;
                var itemRectHeight = squareHeight * item.SizeY;

                var drawRectFix = new RectangleF(itemRectPosX, itemRectPosY, itemRectWidth, itemRectHeight);

                drawRectFix.Top += 7 ;
                //drawRectFix.Bottom += offset ;
                //drawRectFix.Right += offset ;
                drawRectFix.Left += 7 ;

                bool isPageVisible = false;
                if (purchaseWindow != null)
                {
                     isPageVisible = purchaseWindow.IsVisible;
                }

                bool anyMatch =  PartialClassNamesToIgnore.Any(className.Contains);
                
                //bool anyMatch = PartialClassNamesToIgnore.Any(item => item.Contains(className));

                if (!anyMatch) { 
                bool existsMovementSpeed = mods.Any(s => s.Group.Contains("MovementVelocity"));

                int movementSpeedMod = 0;
                if (existsMovementSpeed && className == "Boots")
                {
                    movementSpeedMod = mods.Find(s => s.Group.Contains("MovementVelocity")).Values[0];
                    weights.Add(Settings.MovementSpeedScore * movementSpeedMod);

                    interestingThings.Add(movementSpeedMod.ToString() + " Movement Speed");
                }

                bool existsDotMultiplier = mods.Any(s => s.Group.Contains("GlobalDamageOverTimeMultiplier"));
                if (existsDotMultiplier)
                {
                    var Dot = mods.Find(s => s.Group.Contains("GlobalDamageOverTimeMultiplier")).Values[0];
                    interestingThings.Add(Dot.ToString() + " Damage Over Time Multi");
                    weights.Add(Settings.GlobalDotScore * Dot);
                }

                bool existsAddedSpellFire = mods.Any(s => s.Name.Contains("SpellAddedFireDamage") || s.Group.Contains("AddedFireDamageSpellsAndAttacks") );
                if (existsAddedSpellFire)
                {
                    var addedFireSpellDamageAverage = mods?.Find(s => s.Name.Contains("SpellAddedFireDamage"))?.Values?.Average() ?? 0;
                    var addedFireSpellDamageAverageImp = mods?.Find(s => s.Group.Contains("AddedFireDamageSpellsAndAttacks"))?.Values?.Average() ?? 0;
                    var total = addedFireSpellDamageAverage + addedFireSpellDamageAverageImp;
                    interestingThings.Add(total.ToString() + " To spells");
                    weights.Add((int)(Settings.AddedFireToSpellsScore * total));
                }
                
                bool existsAddedSpellCold = mods.Any(s => s.Name.Contains("SpellAddedColdDamage") || s.Group.Contains("AddedColdDamageSpellsAndAttacks"));
                    if (existsAddedSpellCold)
                {
                    var addedColdSpellDamageAverage = mods?.Find(s => s.Name.Contains("SpellAddedColdDamage"))?.Values?.Average() ?? 0;
                    var addedColdSpellDamageAverageImp = mods?.Find(s => s.Group.Contains("AddedColdDamageSpellsAndAttacks"))?.Values?.Average() ?? 0;
                    var total = addedColdSpellDamageAverage + addedColdSpellDamageAverageImp;
                    interestingThings.Add(total.ToString() + " To spells");
                    weights.Add((int)(Settings.AddedColdToSpellsScore * total));
                }
                
                bool existsAddedSpellLight = mods.Any(s => s.Name.Contains("SpellAddedLightningDamage") || s.Group.Contains("AddedLightningDamageSpellsAndAttacks"));
                    if (existsAddedSpellLight)
                {
                    var addedLightningSpellDamageAverage = mods?.Find(s => s.Name.Contains("SpellAddedLightningDamage"))?.Values?.Average() ?? 0;
                    var addedLightningSpellDamageAverageImp = mods?.Find(s => s.Group.Contains("AddedLightningDamageSpellsAndAttacks"))?.Values?.Average() ?? 0;
                    var total = addedLightningSpellDamageAverage + addedLightningSpellDamageAverageImp;
                    interestingThings.Add(total.ToString() + " To spells");
                    weights.Add((int)(Settings.AddedLightningToSpellsScore * total));
                }
                    

                    bool existsSpellDamage = mods.Any(s => s.Name.Contains("SpellDamageOnWeapon") || s.Name.Contains("SpellDamageAndManaOnWeapon"));
                    if (existsSpellDamage)
                    {
                        var spellDamage1 = mods?.FindAll(s => s.Name.Contains("SpellDamageOnWeapon"))?.Sum(item => item?.Values[0] ?? 0);
                        var spellDamage2 = mods?.FindAll(s => s.Name.Contains("SpellDamageAndManaOnWeapon"))?.Sum(item => item?.Values[0] ?? 0);
     
                        //var spellDamage3 = mods?.Find(s => s.Name.Contains("SpellDamageOnWeaponImplicitWand"))?.Values[0] ?? 0;
                        var total = spellDamage1 + spellDamage2;
                        interestingThings.Add(total.ToString() + " SpellDamage");
                        weights.Add((int)(Settings.SpellDamageScore * total));
                    }

                    bool existsSpellCrit = mods.Any(s => s.Name.Contains("SpellCriticalStrikeChance"));
                    if (existsSpellCrit)
                    {
                        var spellCrit = mods.Find(s => s.Name.Contains("SpellCriticalStrikeChance")).Values[0];
                        interestingThings.Add(spellCrit.ToString() + " SpellCrit");
                        weights.Add(Settings.SpellCritChanceScore * spellCrit);
                    }

                    bool existsCritMulti = mods.Any(s => s.Group.Contains("CriticalStrikeMultiplier") ) && !mods.Any(s => s.Name.Contains("CriticalMultiplierImplicitSword1")); ;
                    if (existsCritMulti)
                    {
                        var critMulti = mods.Find(s => s.Group.Contains("CriticalStrikeMultiplier")).Values[0];
                        interestingThings.Add(critMulti.ToString() + " CriticalStrikeMultiplier");
                        weights.Add(Settings.CritMultiScore * critMulti);
                    }

                    bool existsMinionCritChance = mods.Any(s => s.Name.Contains("MinionCriticalStrikeChanceIncrease"));
                    if (existsMinionCritChance)
                    {
                        var minionCrit = mods.Find(s => s.Name.Contains("MinionCriticalStrikeChanceIncrease")).Values[0];
                        interestingThings.Add(minionCrit.ToString() + " Minion Crit");
                        weights.Add(Settings.MinionCritChanceScore * minionCrit);
                    }

                    bool existsMinionCritMulti = mods.Any(s => s.Group.Contains("MinionCriticalStrikeMultiplier"));
                    if (existsMinionCritMulti)
                    {
                        var minionCritMulti = mods.Find(s => s.Group.Contains("MinionCriticalStrikeMultiplier")).Values[0];
                        interestingThings.Add(minionCritMulti.ToString() + " Minion CritMulti");
                        weights.Add(Settings.MinionCriticalMultiplierScore * minionCritMulti);
                    }


                    bool existsMinionIASICS = mods.Any(s => s.Name.Contains("MinionAttackAndCastSpeed"));
                    if (existsMinionIASICS)
                    {
                        var minionIAS = mods.Find(s => s.Name.Contains("MinionAttackAndCastSpeed")).Values[0];
                        interestingThings.Add(minionIAS.ToString() + " Minion IAS ICS");
                        weights.Add(Settings.MinionAttackCastScore * minionIAS);
                    }
                    bool existsWED = mods.Any(s => s.Name.Contains("WeaponElementalDamageOnWeapons"));
                    if (existsWED)
                    {
                        var WED = mods.Find(s => s.Name.Contains("WeaponElementalDamageOnWeapons")).Values[0];
                        interestingThings.Add(WED.ToString() + " Weapon Ele Dmg");
                        weights.Add(Settings.WeaponElementalDamageScore * WED);
                    }

                    bool existsFirePercent = mods.Any(s => s.Name.Contains("FireDamagePrefixOnWeapon") || s.Name.Contains("FireDamagePercent"));
                    if (existsFirePercent)
                    {
                        var existsFirePercent1 = mods.Find(s => s.Name.Contains("FireDamagePrefixOnWeapon"))?.Values[0] ?? 0;
                        var existsFirePercent2 = mods.Find(s => s.Name.Contains("FireDamagePercent"))?.Values[0] ?? 0;
                        var total = existsFirePercent1 + existsFirePercent2;
                        interestingThings.Add(total.ToString() + " Fire Damage");
                        weights.Add((int)(Settings.FireDamageScore * total));
                    }

                    bool existsColdPercent = mods.Any(s => s.Name.Contains("ColdDamagePrefixOnWeapon") || s.Name.Contains("ColdDamagePercent"));
                    if (existsColdPercent)
                    {
                        var existsColdPercent1 = mods.Find(s => s.Name.Contains("ColdDamagePrefixOnWeapon"))?.Values[0] ?? 0;
                        var existsColdPercent2 = mods.Find(s => s.Name.Contains("ColdDamagePercent"))?.Values[0] ?? 0;
                        var total = existsColdPercent1 + existsColdPercent2;
                        interestingThings.Add(total.ToString() + " Cold Damage");
                        weights.Add((int)(Settings.ColdDamageScore * total));

                    }

                    bool existsLightningPercent = mods.Any(s => s.Name.Contains("LightningDamagePrefixOnWeapon") || s.Name.Contains("LightningDamagePercent"));
                    if (existsLightningPercent)
                    {
                        var existsLightningPercent1 = mods.Find(s => s.Name.Contains("LightningDamagePrefixOnWeapon"))?.Values[0] ?? 0;
                        var existsLightningPercent2 = mods.Find(s => s.Name.Contains("LightningDamagePercent"))?.Values[0] ?? 0;
                        var total = existsLightningPercent1 + existsLightningPercent2;
                        interestingThings.Add(total.ToString() + " Lightning Damage");
                        weights.Add((int)(Settings.LightningDamageScore * total));

                    }

                    bool existsBurning = mods.Any(s => s.Name.Contains("BurnDamage"));
                    if (existsBurning)
                    {
                        var burning = mods.Find(s => s.Name.Contains("BurnDamage")).Values[0];
                        interestingThings.Add(burning.ToString() + " Burning DMG");
                        weights.Add(Settings.BurnDamageScore * burning);
                    }

                    bool existsMinionDamage = mods.Any(s => s.Name.Contains("MinionDamageOnWeapon") || s.Name.Contains("MinionDamageAndManaOnWeapon") || s.Name.Contains("MinionDamageImplicitWand") );
                    if (existsMinionDamage)
                    {
                        var minionDamage1 = mods?.FindAll(s => s.Name.Contains("MinionDamageOnWeapon"))?.Sum(item => item?.Values[0] ?? 0);
                        var minionDamage2 = mods?.FindAll(s => s.Name.Contains("MinionDamageAndManaOnWeapon"))?.Sum(item => item?.Values[0] ?? 0);
                        var minionDamage3 = mods?.FindAll(s => s.Name.Contains("MinionDamageImplicitWand"))?.Sum(item => item?.Values[0] ?? 0);
                        var total = minionDamage1 + minionDamage2 + minionDamage3;
                        interestingThings.Add(total.ToString() + " Minion Damage");
                        weights.Add((int)(Settings.MinionDamageScore * total));
                    }

                    bool existsFireDotMultiplier = mods.Any(s => s.Name.Contains("FireDamageOverTimeMultiplier"));
                if (existsFireDotMultiplier)
                {
                    var Dot = mods.Find(s => s.Name.Contains("FireDamageOverTimeMultiplier")).Values[0];
                    interestingThings.Add(Dot.ToString() + " Fire Damage Over Time Multi");
                    weights.Add(Settings.FireDotScore * Dot);
                }
                bool existsColdDotMultiplier = mods.Any(s => s.Name.Contains("ColdDamageOverTimeMultiplier"));
                if (existsColdDotMultiplier)
                {
                    var Dot = mods.Find(s => s.Name.Contains("ColdDamageOverTimeMultiplier")).Values[0];
                    interestingThings.Add(Dot.ToString() + " Cold Damage Over Time Multi");
                    weights.Add(Settings.ColdDotScore * Dot);
                }
                bool existsChaosDotMultiplier = mods.Any(s => s.Name.Contains("ChaosDamageOverTimeMultiplier"));
                if (existsChaosDotMultiplier)
                {
                    var Dot = mods.Find(s => s.Name.Contains("ChaosDamageOverTimeMultiplier")).Values[0];
                    interestingThings.Add(Dot.ToString() + " Chaos Damage Over Time Multi");
                    weights.Add(Settings.ChaosDotScore * Dot);
                }
                bool existsPhysicalDotMultiplier = mods.Any(s => s.Name.Contains("PhysicalDamageOverTimeMultiplier"));
                if (existsPhysicalDotMultiplier)
                {
                    var Dot = mods.Find(s => s.Name.Contains("PhysicalDamageOverTimeMultiplier")).Values[0];
                    interestingThings.Add(Dot.ToString() + " Phy Damage Over Time Multi");
                    weights.Add(Settings.PhysicalDotScore * Dot);
                }

                bool existsPlusOneFire = mods.Any(s => s.Name.Contains("GlobalFireSpellGemsLevel1"));
                if (existsPlusOneFire)                {    
                    interestingThings.Add(  " POG +1 FIRE");
                    weights.Add(Settings.OneHandPlusFireScore);
                }
                bool existsPlusOneCold = mods.Any(s => s.Name.Contains("GlobalColdSpellGemsLevel1"));
                if (existsPlusOneCold)
                {
                    interestingThings.Add("POG +1 cold");
                    weights.Add(Settings.OneHandPlusColdScore);
                }
                bool existsPlusOneLightning = mods.Any(s => s.Name.Contains("GlobalLightningSpellGemsLevel1"));
                if (existsPlusOneLightning)
                {
                    interestingThings.Add("POG +1 LIGHT");
                    weights.Add(Settings.OneHandPlusLightScore);
                }
                bool existsPlusOneChaos = mods.Any(s => s.Name.Contains("GlobalChaosSpellGemsLevel1"));
                if (existsPlusOneChaos)
                {
                    interestingThings.Add("POG +1 chaos");
                    weights.Add(Settings.OneHandPlusChaosScore);
                }

                bool existsPlusOnePhysical = mods.Any(s => s.Name.Contains("GlobalPhysicalSpellGemsLevel1"));
                if (existsPlusOnePhysical)
                {
                    interestingThings.Add("POG +1 PHYS");
                    weights.Add(Settings.OneHandPlusPhysicalScore);
                }
                bool existsPlusOneMinion = mods.Any(s => s.Name.Contains("MinionGemLevel1h1"));
                if (existsPlusOneMinion)
                {
                    interestingThings.Add("POG +1 MINION");
                    weights.Add(Settings.OneHandPlusMinionScore);
                }

                // two handed
                int GemLevel = 0;
                bool existsPlusFire2h = mods.Any(s => s.Name.Contains("GlobalFireSpellGemsLevelTwoHand"));
                if (existsPlusFire2h)
                {
                    GemLevel = mods.Find(s => s.Name.Contains("GlobalFireSpellGemsLevelTwoHand")).Values[0];
                    if(GemLevel > 1){
                        interestingThings.Add("POG +" + GemLevel + " Fire");
                        weights.Add(Settings.TwoHandPlusFireScore * GemLevel);
                    }
                    
                }
                bool existsPlusCold2h = mods.Any(s => s.Name.Contains("GlobalColdSpellGemsLevelTwoHand"));
                if (existsPlusCold2h)
                {
                    GemLevel = mods.Find(s => s.Name.Contains("GlobalColdSpellGemsLevelTwoHand")).Values[0];
                    if (GemLevel > 1)
                    {
                        interestingThings.Add("POG +" + GemLevel + " Cold");
                        weights.Add(Settings.TwoHandPlusColdScore * GemLevel);
                    }

                }

                bool existsPlusLightning2h = mods.Any(s => s.Name.Contains("GlobalLightningSpellGemsLevelTwoHand"));
                if (existsPlusLightning2h)
                {
                    GemLevel = mods.Find(s => s.Name.Contains("GlobalLightningSpellGemsLevelTwoHand")).Values[0];
                    if (GemLevel > 1)
                    {
                        interestingThings.Add("POG +" + GemLevel + " Lightning");
                        weights.Add(Settings.TwoHandPlusLightScore * GemLevel);
                    }
                }

                bool existsPlusChaos2h = mods.Any(s => s.Name.Contains("GlobalChaosSpellGemsLevelTwoHand"));
                if (existsPlusChaos2h)
                {
                    GemLevel = mods.Find(s => s.Name.Contains("GlobalChaosSpellGemsLevelTwoHand")).Values[0];
                    if (GemLevel > 1)
                    {
                        interestingThings.Add("POG +" + GemLevel + " Chaos");
                        weights.Add(Settings.TwoHandPlusChaosScore * GemLevel);
                    }

                }

                bool existsPlusPhysical2h = mods.Any(s => s.Name.Contains("GlobalPhysicalSpellGemsLevelTwoHand"));
                if (existsPlusPhysical2h)
                {
                    GemLevel = mods.Find(s => s.Name.Contains("GlobalPhysicalSpellGemsLevelTwoHand")).Values[0];
                    if (GemLevel > 1)
                    {
                        interestingThings.Add("POG +" + GemLevel + " Phyis");
                        weights.Add(Settings.TwoHandPlusPhysicalScore*GemLevel);
                    }
                }
                
                // amulets
                bool existsPlusFireAmmy = mods.Any(s => s.Name.Contains("GlobalFireGemLevel1"));
                if (existsPlusFireAmmy)
                {
                    interestingThings.Add("POG +1 FIRE");
                    weights.Add(Settings.AmuletPlusFireScore );
                }

                bool existsPlusColdAmmy = mods.Any(s => s.Name.Contains("GlobalColdGemLevel1"));
                if (existsPlusColdAmmy)
                {
                    interestingThings.Add("POG +1 Cold");
                    weights.Add(Settings.AmuletPlusColdScore);
                }

                bool existsPlusLightningAmmy = mods.Any(s => s.Name.Contains("GlobalLightningGemLevel1"));
                if (existsPlusLightningAmmy)
                {
                    interestingThings.Add("POG +1 Light");
                    weights.Add(Settings.AmuletPlusLightScore);
                }

                bool existsPlusChaosAmmy = mods.Any(s => s.Name.Contains("GlobalChaosGemLevel1"));
                if (existsPlusChaosAmmy)
                {
                    interestingThings.Add("POG +1 Chaos");
                    weights.Add(Settings.AmuletPlusChaosScore);
                }
                bool existsPlusPhysicalAmmy = mods.Any(s => s.Name.Contains("GlobalPhysicalGemLevel1"));
                if (existsPlusPhysicalAmmy)
                {
                    interestingThings.Add("POG +1 PHY");
                    weights.Add(Settings.AmuletPlusPhysicalScore);
                }
                bool existsPlusSkillAmmy = mods.Any(s => s.Name.Contains("GlobalSkillGemLevel1"));
                if (existsPlusSkillAmmy)
                {
                    interestingThings.Add("POG +1 ALLLL");
                    weights.Add(Settings.AmuletPlusGlobalScore);
                }

                // helmet minion
                GemLevel = 0;
                bool existsPlusMinionHelm = mods.Any(s => s.Name.Contains("GlobalIncreaseMinionSpellSkillGemLevel"));
                if (existsPlusMinionHelm)
                {
                    GemLevel = mods.Find(s => s.Name.Contains("GlobalIncreaseMinionSpellSkillGemLevel")).Values[0];
                    
                    interestingThings.Add("POG +" + GemLevel + " Minion Helm");
                    weights.Add(Settings.MinionHelmGemScore * GemLevel);
                }
    
                bool anyResistance = mods.Any(s => s.Group.Contains("Resistance")) && !mods.Any(s => s.Name.Contains("ResistanceImplicit")) && !mods.Any(s => s.Name.Contains("ResistImplicit"));
                bool anyLife = mods.Any(s => s.Group.Contains("IncreasedLife")) && !mods.Any(s => s.Name.Contains("LifeImplicit"));
                
                if (anyResistance && !item.Item.Path.Contains("/Weapons/"))
                {
                    var fireRes = mods?.Find(s => s.Group.Contains("FireResistance"))?.Values[0] ?? 0;
                    var coldRes = mods?.Find(s => s.Group.Contains("ColdResistance"))?.Values[0] ?? 0;
                    var lightningRes = mods?.Find(s => s.Group.Contains("LightningResistance"))?.Values[0] ?? 0;
                    var chaosRes = mods?.Find(s => s.Group.Contains("ChaosResistance"))?.Values[0] ?? 0;
                    var allResMods = mods?.Find(s => s.Group.Contains("AllResistance"))?.Values[0] ?? 0;
                    var FireAndLightningResistance = mods?.Find(s => s.Group.Contains("FireAndLightningResistance"))?.Values[0] ?? 0;
                    var ColdAndLightningResistance = mods?.Find(s => s.Group.Contains("ColdAndLightningResistance"))?.Values[0] ?? 0;
                    var FireAndColdResistance = mods?.Find(s => s.Group.Contains("FireAndColdResistance"))?.Values[0] ?? 0;
                    var allResSummed = (FireAndColdResistance + ColdAndLightningResistance + FireAndLightningResistance) * 2 + allResMods * 3 + fireRes + coldRes + lightningRes + chaosRes;
                    interestingThings.Add("Total Res: " + allResSummed);
                    weights.Add(Settings.TotalResistScore * allResSummed);
                }

                if (anyLife && !item.Item.Path.Contains("/Weapons/"))
                {
                    var life = mods.FindAll(s => s.Group.Contains("IncreasedLife"));
                    var lifeSum = life.Sum(element => element.Values[0]);
                    interestingThings.Add("LIFE +" + lifeSum);
                    weights.Add(Settings.LifeScore* lifeSum);
                }

                bool castSpeed = mods.Any(s => s.Group.Contains("IncreasedCastSpeed"));

                if (castSpeed)
                {
                    var castSpeedValue = mods.Find(s => s.Group.Contains("IncreasedCastSpeed")).Values[0];
                    interestingThings.Add("Cast Speed +" + castSpeedValue);
                    weights.Add(Settings.CastSpeedScore * castSpeedValue);
                }

                if (!className.Contains("Sceptre") && !className.Contains("Wand"))
                {
                    bool PhysPercent = mods.Any(s => s.Group.Contains("LocalPhysicalDamagePercent") || s.Group.Contains("LocalIncreasedPhysicalDamagePercentAndAccuracyRating")); //LocalIncreasedPhysicalDamagePercentAndAccuracyRating

                    if (PhysPercent)
                    {
                        var PhysPercentValue = mods?.Find(s => s.Group.Contains("LocalPhysicalDamagePercent"))?.Values[0] ?? 0;
                        var PhysPercentAccValue = mods?.Find(s => s.Group.Contains("LocalIncreasedPhysicalDamagePercentAndAccuracyRating"))?.Values[0] ?? 0;
                        interestingThings.Add("Phy damage +" + (PhysPercentValue + PhysPercentAccValue));
                        weights.Add(Settings.PercPhysScore * (PhysPercentValue + PhysPercentAccValue));
                    }

                    bool FlatPhys = mods.Any(s => s.Group.Contains("LocalAddedPhysicalDamageTwoHand"));

                    if (FlatPhys)
                    {
                        var PhysFlatValue = mods.Find(s => s.Name.Contains("LocalAddedPhysicalDamageTwoHand")).Values.Average();
                        interestingThings.Add("FLAT Phy damage +" + PhysFlatValue);
                        weights.Add((int)(Settings.FlatPhysScore * PhysFlatValue));
                    }
                }

                if (className.Contains("Bow"))
                {
                    bool FlatFireTwoHand = mods.Any(s => s.Name.Contains("LocalAddedFireDamageTwoHand"));

                    if (FlatFireTwoHand)
                    {
                        var FlatFireTwoHandValue = mods.Find(s => s.Name.Contains("LocalAddedFireDamageTwoHand")).Values.Average();
                        interestingThings.Add("FLAT Fire damage +" + FlatFireTwoHandValue);
                        weights.Add((int)(Settings.FlatFireScore * FlatFireTwoHandValue));
                    }

                    bool FlatLightTwoHand = mods.Any(s => s.Name.Contains("LocalAddedLightningDamageTwoHand"));

                    if (FlatLightTwoHand)
                    {
                        var FlatLightTwoHandValue = mods.Find(s => s.Name.Contains("LocalAddedLightningDamageTwoHand")).Values.Average();
                        interestingThings.Add("FLAT Light damage +" + FlatLightTwoHandValue);
                        weights.Add((int)(Settings.FlatLightScore * FlatLightTwoHandValue));
                    }

                    bool FlatColdTwoHand = mods.Any(s => s.Name.Contains("LocalAddedColdDamageTwoHand"));

                    if (FlatColdTwoHand)
                    {
                        var FlatColdTwoHandValue = mods.Find(s => s.Name.Contains("LocalAddedColdDamageTwoHand")).Values.Average();
                        interestingThings.Add("FLAT Cold damage +" + FlatColdTwoHandValue);
                        weights.Add((int)(Settings.FlatColdScore * FlatColdTwoHandValue));
                    }

                    bool BowGem = mods.Any(s => s.Name.Contains("LocalIncreaseSocketedBowGemLevel1"));

                    if (BowGem)
                    {
                        var BowGemValue = mods.Find(s => s.Name.Contains("LocalIncreaseSocketedBowGemLevel")).Values[0];
                        interestingThings.Add("Bow gem +" + BowGemValue);
                        weights.Add((int)(Settings.PlusSocketedBowGems * BowGemValue));
                    }

                    bool socketedGem = mods.Any(s => s.Name.Contains("LocalIncreaseSocketedGemLevel"));

                    if (socketedGem)
                    {
                        var socketedGemValue = mods.Find(s => s.Name.Contains("LocalIncreaseSocketedGemLevel")).Values[0];
                        interestingThings.Add("GEM +" + socketedGemValue);
                        weights.Add((int)(Settings.PlusSocketedGems * socketedGemValue));
                    }
                }
                bool reducedFlaskChargesUsed = mods.Any(s => s.Name.Contains("BeltReducedFlaskChargesUsed"));

                if (reducedFlaskChargesUsed)
                {
                    var reducedFlaskChargesUsedValue = mods.Find(s => s.Name.Contains("BeltReducedFlaskChargesUsed")).Values[0] * -1;
                    
                    interestingThings.Add("Red Flask Charges Used +" + reducedFlaskChargesUsedValue);
                    weights.Add((int)(Settings.ReducedFlaskChargesUsed * reducedFlaskChargesUsedValue));
                }

                bool BeltFlaskDuration = mods.Any(s => s.Group.Contains("BeltFlaskDuration"));

                if (BeltFlaskDuration)
                {
                    var BeltFlaskDurationValue = mods.Find(s => s.Group.Contains("BeltFlaskDuration")).Values[0] ;
                    interestingThings.Add("Flask Duration +" + BeltFlaskDurationValue);
                    weights.Add((int)(Settings.FlaskDuration * BeltFlaskDurationValue));
                }

                bool FlaskChargesGained = mods.Any(s => s.Name.Contains("BeltIncreasedFlaskChargesGained"));

                if (FlaskChargesGained)
                {
                    var BeltIncreasedFlaskChargesGainedValue = mods.Find(s => s.Name.Contains("BeltIncreasedFlaskChargesGained")).Values[0];
                    interestingThings.Add("Flask Duration +" + BeltIncreasedFlaskChargesGainedValue);
                    weights.Add((int)(Settings.IncreasedFlaskChargesGained * BeltIncreasedFlaskChargesGainedValue));
                }

                bool LifeRegeneration = mods.Any(s => s.Group.Contains("LifeRegeneration") && !s.Name.Contains("Implicit"));

                if (LifeRegeneration)
                {
                    var BeltIncreasedFlaskChargesGainedValue = mods.Find(s => s.Group.Contains("LifeRegeneration") && !s.Name.Contains("Implicit")).Values[0] / 60;
                    interestingThings.Add("LifeRegeneration +" + BeltIncreasedFlaskChargesGainedValue);
                    weights.Add((int)(Settings.LifeRegenaration * BeltIncreasedFlaskChargesGainedValue));
                }

                bool ChanceToSuppressSpells = mods.Any(s => s.Group.Contains("ChanceToSuppressSpells") && !s.Name.Contains("Implicit"));
                if (ChanceToSuppressSpells)
                {
                    var ChanceToSuppressSpellsValue = mods.Find(s => s.Group.Contains("ChanceToSuppressSpells") && !s.Name.Contains("Implicit")).Values[0];
                    interestingThings.Add("Suppress +" + ChanceToSuppressSpellsValue);
                    weights.Add((int)(Settings.SuppressSpells * ChanceToSuppressSpellsValue));
                }

                if (!className.Contains("Dagger") && !className.Contains("Claw") && !className.Contains("Sceptre") && !className.Contains("Axe") && !className.Contains("Wand") && !className.Contains("Mace") && !className.Contains("Sword")) 
                {

                    bool AddedFireDamage = mods.Any(s => s.Name.Contains("AddedFireDamage") && !s.Name.Contains("TwoHand") && !s.Name.Contains("Implicit")); 
                    // jewelry and gloves added damage 
                    if (AddedFireDamage)
                    {
                        var AddedFireDamageValue = mods.Find(s => s.Name.Contains("AddedFireDamage") && !s.Name.Contains("TwoHand") && !s.Name.Contains("Implicit")).Values.Average();
                        interestingThings.Add("FLAT Fire damage +" + AddedFireDamageValue);
                        weights.Add((int)(Settings.AddedFireDamageScore * AddedFireDamageValue));
                    }

                    bool AddedColdDamage = mods.Any(s => s.Name.Contains("AddedColdDamage") && !s.Name.Contains("TwoHand") && !s.Name.Contains("Implicit"));
                    // jewelry and gloves added damage 
                    if (AddedColdDamage)
                    {
                        var AddedColdDamageValue = mods.Find(s => s.Name.Contains("AddedColdDamage")  && !s.Name.Contains("TwoHand") && !s.Name.Contains("Implicit")).Values.Average();
                        interestingThings.Add("FLAT Cold damage +" + AddedColdDamageValue);
                                            weights.Add((int)(Settings.AddedColdDamageScore * AddedColdDamageValue));
                    }

                    bool AddedLightDamage = mods.Any(s => s.Name.Contains("AddedLightningDamage") && !s.Name.Contains("TwoHand") && !s.Name.Contains("Implicit"));
                    // jewelry and gloves added damage 
                    if (AddedLightDamage)
                    {
                        var AddedLightDamageValue = mods.Find(s => s.Name.Contains("AddedLightningDamage") && !s.Name.Contains("TwoHand") && !s.Name.Contains("Implicit")).Values.Average();
                        interestingThings.Add("FLAT Light damage +" + AddedLightDamageValue);
                        weights.Add((int)(Settings.AddedLightDamageScore * AddedLightDamageValue));
                    }

                    bool AddedPhysicalDamage = mods.Any(s => s.Name.Contains("AddedPhysicalDamage") && !s.Name.Contains("TwoHand") && !s.Name.Contains("Implicit"));
                    // jewelry and gloves added damage 
                    if (AddedPhysicalDamage)
                    {
                        var AddedPhysicalDamageValue = mods.Find(s => s.Name.Contains("AddedPhysicalDamage") && !s.Name.Contains("TwoHand") && !s.Name.Contains("Implicit")).Values.Average();
                        interestingThings.Add("FLAT Physical damage +" + AddedPhysicalDamageValue);
                        weights.Add((int)(Settings.AddedPhysicalDamageScore * AddedPhysicalDamageValue));
                    }
                }
                // quiver
                bool AddedFireDamageQuiver = mods.Any(s => s.Name.Contains("AddedFireDamageQuiver") && !s.Name.Contains("Implicit"));
                if (AddedFireDamageQuiver)
                {
                    var AddedFireDamageQuiverValue = mods.Find(s => s.Name.Contains("AddedFireDamageQuiver") && !s.Name.Contains("Implicit")).Values.Average();
                    interestingThings.Add("FLAT Fire damage +" + AddedFireDamageQuiverValue);
                    weights.Add((int)(Settings.AddedFireDamageQuiverScore * AddedFireDamageQuiverValue));
                }
                bool AddedLightDamageQuiver = mods.Any(s => s.Name.Contains("AddedLightningDamageQuiver") && !s.Name.Contains("Implicit"));
                if (AddedLightDamageQuiver)
                {
                    var AddedLightDamageQuiverValue = mods.Find(s => s.Name.Contains("AddedLightningDamageQuiver") && !s.Name.Contains("Implicit")).Values.Average();
                    interestingThings.Add("FLAT Light damage +" + AddedLightDamageQuiverValue);
                    weights.Add((int)(Settings.AddedLightDamageQuiverScore * AddedLightDamageQuiverValue));
                }
                bool AddedColdDamageQuiver = mods.Any(s => s.Name.Contains("AddedColdDamageQuiver") && !s.Name.Contains("Implicit"));
                if (AddedColdDamageQuiver)
                {
                    var AddedColdDamageQuiverValue = mods.Find(s => s.Name.Contains("AddedColdDamageQuiver") && !s.Name.Contains("Implicit")).Values.Average();
                    interestingThings.Add("FLAT Cold damage +" + AddedColdDamageQuiverValue);
                    weights.Add((int)(Settings.AddedColdDamageQuiverScore * AddedColdDamageQuiverValue));
                }

                bool AddedPhysicalDamageQuiver = mods.Any(s => s.Name.Contains("AddedPhysicalDamageQuiver") && !s.Name.Contains("Implicit"));
                if (AddedPhysicalDamageQuiver)
                {
                    var AddedPhysicalDamageQuiverValue = mods.Find(s => s.Name.Contains("AddedPhysicalDamageQuiver") && !s.Name.Contains("Implicit")).Values.Average();
                    interestingThings.Add("FLAT Physical damage +" + AddedPhysicalDamageQuiverValue);
                    weights.Add((int)(Settings.AddedPhysicalDamageQuiverScore * AddedPhysicalDamageQuiverValue));
                }

                bool attackSpeed = mods.Any(s => s.Group.Contains("IncreasedAttackSpeed"));
                if (attackSpeed && !item.Item.Path.Contains("/Weapons/"))
                {
                    var attackSpeedValue = mods.Find(s => s.Group.Contains("IncreasedAttackSpeed")).Values.Average();
                    interestingThings.Add("IAS +" + attackSpeedValue);
                    weights.Add((int)(Settings.IncreasedAttackSpeedScore * attackSpeedValue));
                }
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
                        interestingThings.Add(orderedLinkColors + " 4L " + className );
                        weights.Add(Settings.FourLinkScore);
                    }
                    
                }else if (links == 3 && !anyMatch)
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

                    string concatInterestingStuff = string.Join(",", interestingThings);
                    var newItem = (className, concatInterestingStuff, inventoryIndex,finalScore);

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
                            
                    }else if (finalScore >= Settings.ATierThreshold)
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

                    if (isPageVisible)
                    {
                        Graphics.DrawFrame(drawRectFix, colorBorder, 5);
                    }
                }
            }
        }
        }
    }
}