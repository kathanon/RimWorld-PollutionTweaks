using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PollutionTweaks {
    public static class Strings {
        public const string ID = "kathanon.PollutionTweaks";
        public const string Name = "Pollution Tweaks";

        // World creation
        public static readonly string PollutionTip       = (ID + ".PollutionTip"      ).Translate();
        public static readonly string PollutionRange     = (ID + ".PollutionRange"    ).Translate();
        public static readonly string PollutionRangeTip  = (ID + ".PollutionRangeTip" ).Translate();
        public static readonly string PollutionSpread    = (ID + ".PollutionSpread"   ).Translate();
        public static readonly string PollutionSpreadTip = (ID + ".PollutionSpreadTip").Translate();

        // Settings
        public static readonly string SpreadPollution         = (ID + ".SpreadPollution"        ).Translate();
        public static readonly string NearbyPollutionWeighing = (ID + ".NearbyPollutionWeighing").Translate();
        public static readonly string NearbyPollutionTitle    = (ID + ".NearbyPollutionTitle"   ).Translate();
        public static readonly string WeightVertical          = (ID + ".WeightVertical"         ).Translate();
        public static readonly string Distance                = (ID + ".Distance"               ).Translate();
        public static readonly string SmogTitle               = (ID + ".SmogTitle"              ).Translate();
        public static readonly string Amount                  = (ID + ".Amount"                 ).Translate();
        public static readonly string AmountTip               = (ID + ".AmountTip"              ).Translate();
        public static readonly string IntervalTip             = (ID + ".IntervalTip"            ).Translate();
        public static readonly string Interval                = (ID + ".Interval"               ).Translate();
        public static readonly string MaxWeight               = (ID + ".MaxWeight"              ).Translate();
        public static readonly string Reset                   = (ID + ".Reset"                  ).Translate();
        public static readonly string AddPoint                = (ID + ".AddPoint"               ).Translate();
        public static readonly string RemovePoint             = (ID + ".RemovePoint"            ).Translate();
    }
}
