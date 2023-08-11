using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
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
        int inventoryIndex = 1;
        int inventoryIndexPurchase = 1;
        

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
                        HiglightAllVendorShop(auxaux);
                    }
                    inventoryIndex = inventoryIndex + 1;

                }
                var inventoryZone = ingameState.IngameUi.InventoryPanel[InventoryIndex.PlayerInventory].VisibleInventoryItems;

            }
        }

        private void HiglightAllVendorShop(IList<ServerInventory.InventSlotItem> items)
        {
            var ingameState = GameController.Game.IngameState;
            var serverData = ingameState.ServerData;
            var bonusComp = serverData.BonusCompletedAreas;
            var comp = serverData.CompletedAreas;
            var shEld = serverData.ShaperElderAreas;
            var playerLevel = GameController.Player.GetComponent<Player>().Level;

            var disableOnHover = false;
            var disableOnHoverRect = new RectangleF();

            var inventoryItemIcon = ingameState.UIHover.AsObject<HoverItemIcon>();
            var UIHoverEntity = ingameState.UIHover.Entity;

            var tooltip = inventoryItemIcon?.Tooltip;



            if (tooltip != null)
            {
                disableOnHover = true;
                disableOnHoverRect = tooltip.GetClientRect();
            }

            foreach (var item in items)
            {
                List<int> weights = new List<int>();
                var itemIsHovered = false;


              




                if (item == null) continue;
      

                if (item.Item.Path.Contains("Currency") || item.Item.Path.Contains("Gems")) continue;


                var drawRect = item.GetClientRect();

                if (disableOnHover && disableOnHoverRect.Intersects(drawRect))
                    continue;

                var offset = 3;
                drawRect.Top += offset - 326;
                drawRect.Bottom -= offset + 326;
                drawRect.Right -= offset + 961;
                drawRect.Left += offset - 961;

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

                
                var auxChildrenFirst = ingameState.IngameUi.PurchaseWindow;
                var auxChildren = auxChildrenFirst?.GetChildAtIndex(7)?.GetChildFromIndices(1)?.GetChildFromIndices(inventoryIndex-1);
                bool isPageVisible = false;
                if (auxChildren != null)
                {
                     isPageVisible = auxChildren.IsVisible;
                }

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


                bool existsAddedSpell = mods.Any(s => s.Group.Contains("SpellAddedElementalDamage"));
                if (existsAddedSpell)
                {
                    var addedSpellDamageAverage = mods.Find(s => s.Group.Contains("SpellAddedElementalDamage")).Values.Average();
                    interestingThings.Add(addedSpellDamageAverage.ToString() + " To spells");
                    weights.Add((int)(Settings.ElementalDamageToSpellsScore * addedSpellDamageAverage));

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


               

                bool anyResistance = mods.Any(s => s.Group.Contains("Resistance")) && !mods.Any(s => s.Name.Contains("ResistImplicit"));
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
                else if (links == 4)
                {

                    var linkColors = itemSockets.SocketGroup.First(s => s.Length == 4);
                    var orderedLinkColors = String.Concat(linkColors.OrderByDescending(c => c));

                    //LogMessage("4L " + linkColors + " ordered " + orderedLinkColors, 5, Color.Green);
                    if (FourLinkStringList.Contains(orderedLinkColors) && playerLevel <= Settings.FourLinkCharacterLevelThreshold)
                    {
                        interestingThings.Add(orderedLinkColors + " 4L " + className );
                        weights.Add(Settings.FourLinkScore);
                    }
                    
                }else if (links == 3)
                {

                    var linkColors = itemSockets.SocketGroup.First(s => s.Length == 3);
                    var orderedLinkColors = String.Concat(linkColors.OrderByDescending(c => c));

                    //LogMessage("3L " + linkColors + " ordered " + orderedLinkColors, 5, Color.Green);
                    if (ThreeLinkStringList.Contains(orderedLinkColors) && playerLevel <= Settings.ThreeLinkCharacterLevelThreshold)
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
                    string concatInterestingStuff = string.Join(",", interestingThings);
                    int finalScore = weights.Sum();
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
                        Graphics.DrawFrame(drawBox, colorBorder, 5);
                    }

                }


            }
        }
        


      

        



    }
}