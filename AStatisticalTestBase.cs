using System;
using System.Collections.Generic;

namespace ABTesting
{
    public abstract class AStatisticalTestBase
    {
        //protected static readonly Dictionary<double, string> Percentages = new Dictionary<double, string> { { 0.10, "90%" }, { 0.05, "95%" }, { 0.025, "97.5%" }, { 0.01, "99%" }, { 0.001, "99.9%" }, { 0.0, @"100%" } };
        protected static readonly Dictionary<double, string> Descriptions = new Dictionary<double, string> { { 0.10, "fairly confident" }, { 0.05, "confident" }, { 0.025, "very confident" }, { 0.01, "very confident" }, { 0.001, "extremely confident" },  { 0.005, "extremely confident" }, { 0.0, "completely confident" } };

        public string ToPercentageString(double p)
        {
            return Convert.ToString((1.0 - p) * 100) + @"%";
        }
    }
}
