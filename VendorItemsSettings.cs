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

        public List<string> FourLinkList { get; }
        public List<string> ThreeLinkList { get; }

        public RangeNode<int> MovementSpeedScore { get; set; }
        public RangeNode<int> GlobalDotScore { get; set; }
        public RangeNode<int> ElementalDamageToSpellsScore { get; set; }
        public RangeNode<int> FireDotScore { get; set; }
        public RangeNode<int> ColdDotScore { get; set; }
        public RangeNode<int> ChaosDotScore { get; set; }
        public RangeNode<int> PhysicalDotScore { get; set; }
        public RangeNode<int> OneHandPlusFireScore { get; set; }
        public RangeNode<int> OneHandPlusLightScore { get; set; }
        public RangeNode<int> OneHandPlusColdScore { get; set; }
        public RangeNode<int> OneHandPlusChaosScore { get; set; }
        public RangeNode<int> OneHandPlusPhysicalScore { get; set; }
        public RangeNode<int> OneHandPlusMinionScore { get; set; }

        public RangeNode<int> TwoHandPlusFireScore { get; set; }
        public RangeNode<int> TwoHandPlusLightScore { get; set; }
        public RangeNode<int> TwoHandPlusColdScore { get; set; }
        public RangeNode<int> TwoHandPlusChaosScore { get; set; }
        public RangeNode<int> TwoHandPlusPhysicalScore { get; set; }


        public RangeNode<int> AmuletPlusFireScore { get; set; }
        public RangeNode<int> AmuletPlusLightScore { get; set; }
        public RangeNode<int> AmuletPlusColdScore { get; set; }
        public RangeNode<int> AmuletPlusChaosScore { get; set; }
        public RangeNode<int> AmuletPlusPhysicalScore { get; set; }

        public RangeNode<int> AmuletPlusGlobalScore { get; set; }
        public RangeNode<int> MinionHelmGemScore { get; set; }

        public RangeNode<int> LifeScore { get; set; }

        public RangeNode<int> TotalResistScore { get; set; }

        public RangeNode<int> CastSpeedScore { get; set; }


        public RangeNode<int> SixLinkScore { get; set; }
        public RangeNode<int> FiveLinkScore { get; set; }


        public RangeNode<int> SixSocketScore { get; set; }

        public RangeNode<int> RGBScore { get; set; }

        public ToggleNode Enable { get; set; } = new ToggleNode(false);

        public VendorItemsSettings()
        {

            DebugMode = new ToggleNode(false);


            MinScoreThreshold = new RangeNode<int>(1, 0, 50000);



            FourLinkStrings = new TextNode("BBBB,BBBR");
            FourLinkScore = new RangeNode<int>(300, 0, 5000);            
            FourLinkCharacterLevelThreshold = new RangeNode<int>(60, 0, 100);

            ThreeLinkStrings = new TextNode("BBB,BBR");
            ThreeLinkScore = new RangeNode<int>(200, 0, 5000);            
            ThreeLinkCharacterLevelThreshold = new RangeNode<int>(35, 0, 100);

            MovementSpeedScore = new RangeNode<int>(100, 0, 1000);
            GlobalDotScore = new RangeNode<int>(100, 0, 1000);
            ElementalDamageToSpellsScore = new RangeNode<int>(400, 0, 1000);
            FireDotScore = new RangeNode<int>(100, 0, 1000);
            ColdDotScore = new RangeNode<int>(80, 0, 1000);
            ChaosDotScore = new RangeNode<int>(80, 0, 1000);
            PhysicalDotScore = new RangeNode<int>(80, 0, 1000);
            OneHandPlusFireScore = new RangeNode<int>(2000, 0, 5000);
            OneHandPlusLightScore = new RangeNode<int>(2000, 0, 5000);
            OneHandPlusColdScore = new RangeNode<int>(2000, 0, 5000);
            OneHandPlusChaosScore = new RangeNode<int>(2000, 0, 5000);
            OneHandPlusPhysicalScore = new RangeNode<int>(2000, 0, 5000);
            OneHandPlusMinionScore = new RangeNode<int>(2000, 0, 5000);
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
            MinionHelmGemScore = new RangeNode<int>(1000, 0, 2000);
            LifeScore = new RangeNode<int>(10, 0, 1000);
            TotalResistScore = new RangeNode<int>(150, 0, 1000);
            CastSpeedScore = new RangeNode<int>(80, 0, 1000);
            SixLinkScore = new RangeNode<int>(9999, 1000, 10000);
            FiveLinkScore = new RangeNode<int>(5000, 500, 10000);
            SixSocketScore = new RangeNode<int>(3000, 0, 5000);
            RGBScore = new RangeNode<int>(100, 0, 5000);


            STierThresholdColor = new ColorNode(Color.DeepPink);
            STierThreshold = new RangeNode<int>(1000, 0, 50000);
            ATierThresholdColor = new ColorNode(Color.Orange);
            ATierThreshold = new RangeNode<int>(500, 0, 5000);
            BTierThresholdColor = new ColorNode(Color.Yellow);
            BTierThreshold = new RangeNode<int>(250, 0, 5000);
            CTierThresholdColor = new ColorNode(Color.White);
            CTierThreshold = new RangeNode<int>(1, 0, 5000);





        }
    }

}