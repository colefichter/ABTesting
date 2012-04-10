using System;
using System.Linq;
using System.Text;

namespace ABTesting
{
    public sealed class TwoProportionZTest : AStatisticalTestBase, IStatisticalTest
    {
        private static readonly double[,] ZScores = { { 0.10, 1.29 }, { 0.05, 1.65 }, { 0.01, 2.33 }, { 0.001, 3.08 }, { 0.0000, 3.90 } };        

        #region IStatisticalTest Members

        public string GetResultDescription(Experiment test)
        {
            double p;
            try
            {
                p = GetPValue(test);
            }
            catch (Exception e)
            {
                return e.Message;
            }

            StringBuilder builder = new StringBuilder();

            //CF: This is wrong. The assumption for a two-proportion z-test is that each sample have at least 10 succeses AND 10 failures!
            //if (Alternatives[0].Participants < 10 || Alternatives[1].Participants < 10)
            if (test.Alternatives.Count(x => !x.SampleMeetsTestAssumtions) > 0)
            {
                builder.Append("Take these results with a grain of salt since your samples do not meet the required assumptions: ");
            }

            ABAlternative best = test.GetBestAlternative();
            ABAlternative worst = test.GetWorstAlternative();

            builder.Append(String.Format(@"
				The best alternative you have is: [{0}], which had 
				{1} conversions from {2} participants 
				({3}).  The other alternative was [{4}], 
				which had {5} conversions from {6} participants 
				({7}).  "
                , best.Content
                , best.Conversions
                , best.Participants
                , best.PrettyConversionRate
                , worst.Content
                , worst.Conversions
                , worst.Participants
                , worst.PrettyConversionRate
                ));

            if (p == 1)
            {
                builder.Append("However, this difference is not statistically significant.");
            }
            else
            {
                builder.Append(String.Format(@"
					This difference is <b>{0} likely to be statistically significant (p <= {2})</b>, which means you can be 
					{1} that it is the result of your alternatives actually mattering, rather than 
					being due to random chance.  However, this statistical test can't measure how likely the currently 
					observed magnitude of the difference is to be accurate or not.  It only says ""better"", not ""better 
					by so much"".  ",
                    //Percentages[p],
                    ToPercentageString(p),
                    Descriptions[p],
                    p
                    ));
            }

            return builder.ToString();
        }        

        public double GetPValue(Experiment test)
        {
            double z = GetZScore(test);
            z = Math.Abs(z);

            if (!double.IsPositiveInfinity(z))
            {
                //CF: BUG: any z above 1.29 returns p = 0.1.  Eg: z = 10.4 should be p = 0.0 (highly significant) but instead returns 0.1!
                //for (int a=0; a<ZScores.Length/2; a++)
                int arrayLen = ZScores.GetLength(0) - 1;
                for (int a = arrayLen; a >= 0; a--)
                {
                    if (z >= ZScores[a, 1])
                    {
                        return ZScores[a, 0];
                    }
                }
            }

            return 1;
        }

        public bool IsStatisticallySignificant(Experiment test)
        {
            return IsStatisticallySignificant(test, 0.05);
        }

        public bool IsStatisticallySignificant(Experiment test, double pValue)
        {
            return GetPValue(test) <= pValue;
        }
        #endregion
        
        private double GetZScore(Experiment test)
        {
            if (test.Alternatives.Count != 2)
            {
                throw new Exception("Sorry, can't currently automatically calculate statistics for A/B tests with > 2 alternatives.");
            }
            
            //if (test.Alternatives[0].Participants == 0 || test.Alternatives[1].Participants == 0)
            if (!test.AllAlternativesHaveParticipants)
            {
                throw new Exception("Can't calculate the z score if either of the alternatives lacks participants.");
            }

            /* CF: These variable names are not great. What's happening here is we are performing a Two-Proportion Z-test with a pooled difference of the standard errors 
             * of the two samples. For details, see page 566, "Intro Stats", De Veax, Velleman, and Bock */
            double cr1 = test.Alternatives[0].ConversionRate;
            double cr2 = test.Alternatives[1].ConversionRate;

            double successes1 = test.Alternatives[0].Successes;
            double successes2 = test.Alternatives[1].Successes;

            int n1 = test.Alternatives[0].Participants;
            int n2 = test.Alternatives[1].Participants;
            int n = n1 + n2;

            double pHatPooled = (successes1 + successes2) / n;

            double frac1 = pHatPooled * (1 - pHatPooled) / n1;
            double frac2 = pHatPooled * (1 - pHatPooled) / n2;

            double SE = Math.Sqrt(frac1 + frac2);

            //z-score:
            return (cr1 - cr2) / SE;
        }

        public string[] AssumptionsToCheck
        {
            get
            {
                return new string[] { @"Randomization condition", @"10% condition", @"Independent groups assumption", @"Success/Failure condition (checked automatically)" };
            }
        }
    }
}
