using System.Collections.Generic;
using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using SharpDX;

namespace VendorItems
{
   
    public class VendorItemsSettings : ISettings
    {

        static List<string> SortLinks(List<string> inputList)
        {
            List<string> result = new List<string>();

            foreach (string item in inputList)
            {
                var aux = String.Concat(item.OrderBy(c => c));
                if (!result.Contains(aux))
                {
                    result.Add(aux);
                }

            }

            return result;
        }

        public ToggleNode DebugMode { get; set; }

        public ToggleNode PlayerLevelOverrideDebug { get; set; }
        public ToggleNode UseScoreLevelScaler { get; set; }
        public RangeNode<int> PlayerLevelOverride { get; set; }
        public RangeNode<int> ResultX { get; set; }
        public RangeNode<int> ResultY { get; set; }
        public RangeNode<int> TextSpacing { get; set; }
        public RangeNode<int> WordWrapSize { get; set; }
        public RangeNode<int> MinScoreThreshold { get; set; }

        public ColorNode STierThresholdColor { get; set; }
        public RangeNode<int> STierThreshold { get; set; }
        public ColorNode ATierThresholdColor { get; set; }
        public RangeNode<int> ATierThreshold { get; set; }
        public ColorNode BTierThresholdColor { get; set; }
        public RangeNode<int> BTierThreshold { get; set; }
        public ColorNode CTierThresholdColor { get; set; }
        public RangeNode<int> CTierThreshold { get; set; }



        public TextNode FourLinkStrings { get; set; }
        public RangeNode<int> FourLinkScore { get; set; }
        public RangeNode<int> FourLinkCharacterLevelThreshold { get; set; }
        
        
        public TextNode ThreeLinkStrings { get; set; }
        public RangeNode<int> ThreeLinkScore { get; set; }
        public RangeNode<int> ThreeLinkCharacterLevelThreshold { get; set; }
		public RangeNode<int> RGBScore { get; set; }
		
        [Menu(null, "Put comma separated partial class names to ignore them from being mod filtered, RGB, 6L, 5L and 6s still apply. Doesn't need to be complete, for instance Sword,Axe")]
        public TextNode ItemClassesToIgnoreModFiltering { get; set; }

        public ToggleNode IgnoreFiltering { get; set; }
        
        public RangeNode<int> MovementSpeedScore { get; set; }
		
		
		//caster
		
		public RangeNode<int> CastSpeedScore { get; set; }
		
		public RangeNode<int> AddedFireToSpellsScore { get; set; }
        public RangeNode<int> AddedColdToSpellsScore { get; set; }
        public RangeNode<int> AddedLightningToSpellsScore { get; set; }

        public RangeNode<int> SpellDamageScore { get; set; }
        public RangeNode<int> SpellCritChanceScore { get; set; }
        public RangeNode<int> CritMultiScore { get; set; }
		
		public RangeNode<int> FireDamageScore { get; set; }
        public RangeNode<int> BurnDamageScore { get; set; }
        public RangeNode<int> ColdDamageScore { get; set; }
        public RangeNode<int> LightningDamageScore { get; set; }		
		
		
        public RangeNode<int> GlobalDotScore { get; set; }
        //public RangeNode<int> ElementalDamageToSpellsScore { get; set; }
        public RangeNode<int> FireDotScore { get; set; }
        public RangeNode<int> ColdDotScore { get; set; }
        public RangeNode<int> ChaosDotScore { get; set; }
        public RangeNode<int> PhysicalDotScore { get; set; }
        public RangeNode<int> OneHandPlusFireScore { get; set; }
		public RangeNode<int> OneHandPlusColdScore { get; set; }
        public RangeNode<int> OneHandPlusLightScore { get; set; }        
        public RangeNode<int> OneHandPlusChaosScore { get; set; }
        public RangeNode<int> OneHandPlusPhysicalScore { get; set; }
        

        public RangeNode<int> TwoHandPlusFireScore { get; set; }
		public RangeNode<int> TwoHandPlusColdScore { get; set; }
        public RangeNode<int> TwoHandPlusLightScore { get; set; }        
        public RangeNode<int> TwoHandPlusChaosScore { get; set; }
        public RangeNode<int> TwoHandPlusPhysicalScore { get; set; }


        public RangeNode<int> AmuletPlusFireScore { get; set; }
		public RangeNode<int> AmuletPlusColdScore { get; set; }
        public RangeNode<int> AmuletPlusLightScore { get; set; }        
        public RangeNode<int> AmuletPlusChaosScore { get; set; }
        public RangeNode<int> AmuletPlusPhysicalScore { get; set; }
        public RangeNode<int> AmuletPlusGlobalScore { get; set; }
		
		// minions
        public RangeNode<int> OneHandPlusMinionScore { get; set; }
		public RangeNode<int> MinionHelmGemScore { get; set; }
		public RangeNode<int> MinionDamageScore { get; set; }
        public RangeNode<int> MinionCritChanceScore { get; set; }
        public RangeNode<int> MinionAttackCastScore { get; set; }
        public RangeNode<int> MinionCriticalMultiplierScore { get; set; }



        //defenses
        public ToggleNode IgnoreResistsWeaponsQuivers { get; set; } = new ToggleNode(false);
        public RangeNode<int> LifeScore { get; set; }
        public RangeNode<int> TotalResistScore { get; set; }
		public RangeNode<int> SuppressSpells { get; set; }
        public RangeNode<int> LifeRegeneration { get; set; }
        public RangeNode<int> LifeRecoup { get; set; }

		//weapons
		
		
		
		public RangeNode<int> IncreasedAttackSpeedWeaponScore { get; set; }
		
        public RangeNode<int> FlatPhysScore { get; set; }
        public RangeNode<int> PercPhysScore { get; set; }        
        public RangeNode<int> FlatFireScore { get; set; }
		public RangeNode<int> FlatColdScore { get; set; }
        public RangeNode<int> FlatLightScore { get; set; }
        
 
		

		// attack items
		public RangeNode<int> IncreasedAttackSpeedScore { get; set; }
        public RangeNode<int> AddedFireDamageScore { get; set; }
        public RangeNode<int> AddedColdDamageScore { get; set; }
        public RangeNode<int> AddedLightDamageScore { get; set; }		
        public RangeNode<int> AddedPhysicalDamageScore { get; set; }		
		public RangeNode<int> WeaponElementalDamageScore { get; set; }				
		public RangeNode<int> accuracyScore { get; set; }
        public RangeNode<int> accuracyPercentScore { get; set; }
		
		// quiver specific
		
        
        //public RangeNode<int> AddedFireDamageQuiverScore { get; set; }
        //public RangeNode<int> AddedColdDamageQuiverScore { get; set; }
        //public RangeNode<int> AddedLightDamageQuiverScore { get; set; }
        //public RangeNode<int> AddedPhysicalDamageQuiverScore { get; set; }
		public RangeNode<int> damageWithBowSkillsScore { get; set; }
		public RangeNode<int> critWithBowSkillsScore { get; set; }
		public RangeNode<int> critMultiWithBowSkillsScore { get; set; }
		public RangeNode<int> dotMultiBowsScore { get; set; }
		public RangeNode<int> plusArrowsScore { get; set; }
		
        public RangeNode<int> PlusSocketedBowGems { get; set; }
        public RangeNode<int> PlusSocketedGems { get; set; }
		
		//belt

        public RangeNode<int> ReducedFlaskChargesUsed { get; set; }
        public RangeNode<int> IncreasedFlaskChargesGained { get; set; }
        public RangeNode<int> FlaskDuration { get; set; }
        public RangeNode<int> FlaskEffect { get; set; }






        //attributes
        
        public ToggleNode IgnoreAttributesWeapons { get; set; } = new ToggleNode(false);
        
        public RangeNode<int> StrScore { get; set; }
		public RangeNode<int> DexScore { get; set; }
		public RangeNode<int> IntScore { get; set; }
		
		public RangeNode<int> maxFireResScore { get; set; }
		public RangeNode<int> maxColdResScore { get; set; }		
		public RangeNode<int> maxLightningResScore { get; set; }
		
		// flasks
		
		public RangeNode<int> corruptingBloodImmunityScore { get; set; }
		public RangeNode<int> freezeRemovalScore { get; set; }
		public RangeNode<int> instantFlaskScore { get; set; }
		public RangeNode<int> instantLowLifeScore { get; set; }
		

        public RangeNode<int> SixLinkScore { get; set; }
        public RangeNode<int> FiveLinkScore { get; set; }


        public RangeNode<int> SixSocketScore { get; set; }

        
		
		

        public ToggleNode Enable { get; set; } = new ToggleNode(false);

        public VendorItemsSettings()
        {

            DebugMode = new ToggleNode(false);
            PlayerLevelOverrideDebug = new ToggleNode(false);
            UseScoreLevelScaler = new ToggleNode(false);
            PlayerLevelOverride = new RangeNode<int>(1, 1, 100);


			ResultX = new RangeNode<int>(0, 0, 3000);
            ResultY = new RangeNode<int>(500, 0, 3000);
            TextSpacing = new RangeNode<int>(10, 0, 30);
            WordWrapSize = new RangeNode<int>(40, 0, 100);
            
            MinScoreThreshold = new RangeNode<int>(1, 0, 10000);

            STierThresholdColor = new ColorNode(Color.DeepPink);
            STierThreshold = new RangeNode<int>(1000, 0, 50000);
            ATierThresholdColor = new ColorNode(Color.Orange);
            ATierThreshold = new RangeNode<int>(500, 0, 5000);
            BTierThresholdColor = new ColorNode(Color.Yellow);
            BTierThreshold = new RangeNode<int>(250, 0, 5000);
            CTierThresholdColor = new ColorNode(Color.White);
            CTierThreshold = new RangeNode<int>(1, 0, 5000);


            FourLinkStrings = new TextNode("BBBB,BBBR");
            FourLinkScore = new RangeNode<int>(300, 0, 5000);            
            FourLinkCharacterLevelThreshold = new RangeNode<int>(60, 0, 100);

            ThreeLinkStrings = new TextNode("BBB,BBR");
            ThreeLinkScore = new RangeNode<int>(200, 0, 5000);            
            ThreeLinkCharacterLevelThreshold = new RangeNode<int>(35, 0, 100);
            RGBScore = new RangeNode<int>(500, 0, 5000);

            IgnoreFiltering = new ToggleNode(false);
            ItemClassesToIgnoreModFiltering = new TextNode("Mace,Claw");

            MovementSpeedScore = new RangeNode<int>(40, 0, 1000);
            CastSpeedScore = new RangeNode<int>(18, 0, 1000);
            AddedFireToSpellsScore = new RangeNode<int>(20, 1, 2000);
            AddedColdToSpellsScore = new RangeNode<int>(13, 1, 2000);
            AddedLightningToSpellsScore = new RangeNode<int>(13, 1, 2000);

            SpellDamageScore = new RangeNode<int>(5, 1, 2000);
            SpellCritChanceScore = new RangeNode<int>(4, 1, 2000);
            CritMultiScore = new RangeNode<int>(20, 1, 2000);

            FireDamageScore = new RangeNode<int>(8, 1, 2000);
            BurnDamageScore = new RangeNode<int>(6, 1, 2000);
            ColdDamageScore = new RangeNode<int>(8, 1, 2000);
            LightningDamageScore = new RangeNode<int>(8, 1, 2000);

            GlobalDotScore = new RangeNode<int>(30, 0, 1000);
            //ElementalDamageToSpellsScore = new RangeNode<int>(20, 0, 1000);
            FireDotScore = new RangeNode<int>(30, 0, 1000);
            ColdDotScore = new RangeNode<int>(20, 0, 1000);
            ChaosDotScore = new RangeNode<int>(20, 0, 1000);
            PhysicalDotScore = new RangeNode<int>(20, 0, 1000);
            OneHandPlusFireScore = new RangeNode<int>(2000, 0, 5000);
            OneHandPlusLightScore = new RangeNode<int>(2000, 0, 5000);
            OneHandPlusColdScore = new RangeNode<int>(2000, 0, 5000);
            OneHandPlusChaosScore = new RangeNode<int>(2000, 0, 5000);
            OneHandPlusPhysicalScore = new RangeNode<int>(2000, 0, 5000);
            

            TwoHandPlusFireScore = new RangeNode<int>(1000, 0, 3000);
            TwoHandPlusLightScore = new RangeNode<int>(1000, 0, 3000);
            TwoHandPlusColdScore = new RangeNode<int>(1000, 0, 3000);
            TwoHandPlusChaosScore = new RangeNode<int>(1000, 0, 3000);
            TwoHandPlusPhysicalScore = new RangeNode<int>(1000, 0, 3000);

            AmuletPlusFireScore = new RangeNode<int>(3000, 0, 5000);
            AmuletPlusLightScore = new RangeNode<int>(3000, 0, 5000);
            AmuletPlusColdScore = new RangeNode<int>(3000, 0, 5000);
            AmuletPlusChaosScore = new RangeNode<int>(3000, 0, 5000);
            AmuletPlusPhysicalScore = new RangeNode<int>(3000, 0, 5000);
            AmuletPlusGlobalScore = new RangeNode<int>(3000, 0, 5000);

            OneHandPlusMinionScore = new RangeNode<int>(2000, 0, 5000);
            MinionHelmGemScore = new RangeNode<int>(1000, 0, 2000);
            MinionDamageScore = new RangeNode<int>(5, 1, 2000);
            MinionCritChanceScore = new RangeNode<int>(4, 1, 2000);
            MinionAttackCastScore = new RangeNode<int>(10, 1, 2000);
            MinionCriticalMultiplierScore = new RangeNode<int>(20, 1, 2000);

            IgnoreResistsWeaponsQuivers = new ToggleNode(true);
            LifeScore = new RangeNode<int>(10, 0, 1000);
            TotalResistScore = new RangeNode<int>(15, 0, 1000);          
            SuppressSpells = new RangeNode<int>(100, 1, 2000);
            LifeRegeneration = new RangeNode<int>(7, 1, 2000);
            LifeRecoup = new RangeNode<int>(35, 1, 2000);

            IncreasedAttackSpeedWeaponScore = new RangeNode<int>(100, 1, 2000);
            FlatPhysScore = new RangeNode<int>(30, 1, 2000);
            PercPhysScore = new RangeNode<int>(10, 1, 2000);
            FlatLightScore = new RangeNode<int>(7, 1, 2000);
            FlatColdScore = new RangeNode<int>(7, 1, 2000);
            FlatFireScore = new RangeNode<int>(7, 1, 2000);

            IncreasedAttackSpeedScore = new RangeNode<int>(100, 1, 2000);
            AddedFireDamageScore = new RangeNode<int>(25, 1, 2000);
            AddedColdDamageScore = new RangeNode<int>(25, 1, 2000);
            AddedLightDamageScore = new RangeNode<int>(25, 1, 2000);
            AddedPhysicalDamageScore = new RangeNode<int>(50, 1, 2000);
            WeaponElementalDamageScore = new RangeNode<int>(15, 1, 2000);
            accuracyScore = new RangeNode<int>(1, 1, 500);
            accuracyPercentScore = new RangeNode<int>(40, 1, 500);

            //AddedFireDamageQuiverScore = new RangeNode<int>(15, 1, 2000);
            //AddedColdDamageQuiverScore = new RangeNode<int>(15, 1, 2000);
            //AddedLightDamageQuiverScore = new RangeNode<int>(15, 1, 2000);
            //AddedPhysicalDamageQuiverScore = new RangeNode<int>(30, 1, 2000);
            damageWithBowSkillsScore = new RangeNode<int>(12, 1, 2000);
            critWithBowSkillsScore = new RangeNode<int>(10, 1, 2000);
            critMultiWithBowSkillsScore = new RangeNode<int>(20, 1, 2000);
            dotMultiBowsScore = new RangeNode<int>(15, 1, 2000);
            plusArrowsScore = new RangeNode<int>(900, 1, 2000);







            PlusSocketedBowGems = new RangeNode<int>(1000, 1, 5000);
            PlusSocketedGems = new RangeNode<int>(1000, 1, 5000);

            ReducedFlaskChargesUsed = new RangeNode<int>(70, 1, 2000);
            IncreasedFlaskChargesGained = new RangeNode<int>(75, 1, 2000);
            FlaskDuration = new RangeNode<int>(85, 1, 2000);
            FlaskEffect = new RangeNode<int>(100, 1, 2000);

            

            IgnoreAttributesWeapons = new ToggleNode(true);

            StrScore = new RangeNode<int>(5, 0, 1000);            
            DexScore = new RangeNode<int>(5, 0, 1000);
            IntScore = new RangeNode<int>(5, 0, 1000);

            maxFireResScore = new RangeNode<int>(800, 0, 5000);
            maxColdResScore = new RangeNode<int>(800, 0, 5000);
            maxLightningResScore = new RangeNode<int>(800, 0, 5000);

            corruptingBloodImmunityScore = new RangeNode<int>(800, 0, 5000);
            freezeRemovalScore = new RangeNode<int>(800, 0, 5000);
            instantFlaskScore = new RangeNode<int>(800, 0, 5000);
            instantLowLifeScore = new RangeNode<int>(800, 0, 5000);


            SixLinkScore = new RangeNode<int>(9999, 1000, 10000);
            FiveLinkScore = new RangeNode<int>(5000, 500, 10000);
            SixSocketScore = new RangeNode<int>(3000, 0, 5000);
            



        }
    }

}