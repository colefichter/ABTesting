using System;
using System.Linq;
using System.Text;

namespace ABTesting
{
    public sealed class MultipleProportionChiSquareTest : AStatisticalTestBase, IStatisticalTest
    {
        //For now, assume a low DF value
        //eg: df=5 means we have 6 alternatives!  that's likely enough for now.
        //format: [degree of freedom - 1][right-tail probability,chi-square value]
        private static readonly double[][,] _tableChi = { 
			new double[,] {{ 0.1, 2.706 }, { 0.05, 3.841 }, { 0.025, 5.024 }, { 0.01, 6.635 }, { 0.005, 7.879 }},
            new double[,] {{ 0.1, 4.605 }, { 0.05, 5.991 }, { 0.025, 7.378 }, { 0.01, 9.210 }, { 0.005, 10.597 }},
            new double[,] {{ 0.1, 6.251 }, { 0.05, 7.815 }, { 0.025, 9.348 }, { 0.01, 11.345 }, { 0.005, 12.838 }},
            new double[,] {{ 0.1, 7.779 }, { 0.05, 9.488 }, { 0.025, 11.143 }, { 0.01, 13.277 }, { 0.005, 14.860 }},
            new double[,] {{ 0.1, 9.236 }, { 0.05, 11.070 }, { 0.025, 12.833 }, { 0.01, 15.086 }, { 0.005, 16.750 }}
		};


        #region IStatisticalTest Members

        public double GetPValue(Experiment test)
        {
            bool notUsed = false;
            return GetPValue(test, out notUsed);
        }

        private double GetPValue(Experiment test, out bool testAssumptionsUpheld)
        {
            double p = 1;

            testAssumptionsUpheld = true;

            int participants = test.Alternatives.Sum(x => x.Participants);

            if (participants > 0)
            {
                //TODO: optimize this

                int successes = test.Alternatives.Sum(x => x.Successes);
                //pHat represents the estimated overall proportion of successes for all the alternatives combined.
                double pHat = (double)successes / participants;
                //qHat is the complement of pHat (the estimated overall proportion of failures for all the alternatives combined).
                double qHat = 1.0 - pHat;


                if (pHat == 0 || qHat == 0)
                {
                    return p; //throw exception?
                }

                // chi^2 = sum_all_cells( (observed - expected)^2 / expected )
                double chiSquare = 0;

                //associative array holds the expected values for each alternative.
                double[] expectedSuccesses = new double[test.Alternatives.Count];
                for (int i = 0; i < expectedSuccesses.Length; i++)
                {
                    //expectedSuccesses[i] = test.Alternatives[i].Participants * pHat;

                    double expected = test.Alternatives[i].Participants * pHat;
                    if (expected < 5)
                    {
                        testAssumptionsUpheld = false;
                    }
                    double observed = (double)test.Alternatives[i].Successes;

                    chiSquare += Math.Pow(observed - expected, 2) / expected;
                }

                double[] expectedFailures = new double[test.Alternatives.Count];
                for (int i = 0; i < expectedFailures.Length; i++)
                {
                    double expected = test.Alternatives[i].Participants * qHat;
                    if (expected < 5)
                    {
                        testAssumptionsUpheld = false;
                    }
                    double observed = (double)test.Alternatives[i].Failures;

                    chiSquare += Math.Pow(observed - expected, 2) / expected;
                }


                p = LookupPValue(chiSquare, test.Alternatives.Count);
            }


            return p;
        }

        public bool IsStatisticallySignificant(Experiment test)
        {
            return IsStatisticallySignificant(test, 0.05);
        }

        public bool IsStatisticallySignificant(Experiment test, double pValue)
        {
            return GetPValue(test) <= pValue;
        }

        public string GetResultDescription(Experiment test)
        {
            double p;
            bool testAssumptionsUpheld = false;
            try
            {
                p = GetPValue(test, out testAssumptionsUpheld);
            }
            catch (Exception e)
            {
                return e.Message;
            }

            StringBuilder builder = new StringBuilder();
            if (!testAssumptionsUpheld)
            {
                builder.Append("Caution: the sample did not conform to the expected cell frequency condition! ");
            }

            ABAlternative best = test.GetBestAlternative();
            ABAlternative worst = test.GetWorstAlternative();

            builder.Append(String.Format(@"
				The best alternative you have is: [{0}], which had 
				{1} conversions from {2} participants 
				({3}). "
                , best.Content
                , best.Conversions
                , best.Participants
                , best.PrettyConversionRate                
                ));

            if (p == 1)
            {
                builder.Append("However, this result is not statistically significant.");
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

        #endregion

        private double LookupPValue(double chiSquare, int numberOfAlternatives)
        {
            //Since we're always dealing with Yes/No outcomes, the first term will always be: 2 - 1 = 1
            int degreesOfFreedom = 1 * (numberOfAlternatives - 1);

            //normalize the df value. For now, we assume that there are at most 6 alternatives, resulting in df = 5
            if (degreesOfFreedom > 5) { degreesOfFreedom = 5; }
            if (degreesOfFreedom < 1) { degreesOfFreedom = 1; }

            double[,] tableRow = _tableChi[degreesOfFreedom - 1];
            int arrayLen = tableRow.GetLength(0) - 1;
            for (int a = arrayLen; a >= 0; a--)
            {
                if (chiSquare > tableRow[a, 1])
                {
                    return tableRow[a, 0];
                }
            }

            return 1;
        }

        public string[] AssumptionsToCheck
        {
            get
            {
                return new string[] { @"Counted data condition", @"Randomization condition", @"10% condition", @"Expected cell frequency condition (checked automatically)" };
            }
        }
    }
}
