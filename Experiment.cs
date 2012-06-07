using System;
using System.Collections.Generic;

namespace ABTesting
{
    public enum TestStatus
    {
        Complete = 0,
        Running = 1        
    }

    public class Experiment
    {
        public static readonly int DEFAULT_MIN_OBSERVATIONS = 200;

        public string TestName { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<ABAlternative> Alternatives { get; set; }

        public TestStatus Status
        {
            get
            {
                //TODO: This may not be the best way to determine if a test is complete... We could look at participants, successes/failures, something else (?)
                int totalObservations = 0;
                foreach (ABAlternative a in Alternatives)
                {
                    totalObservations += a.Conversions;
                }

                if (totalObservations >= DEFAULT_MIN_OBSERVATIONS)
                {
                    return TestStatus.Complete;
                }
                else
                {
                    return TestStatus.Running;
                }
            }
        }

        public bool IsComplete
        {
            get
            {
                return this.Status == TestStatus.Complete;
            }
        }

        public bool AllAlternativesHaveParticipants
        {
            get
            {
                foreach (ABAlternative a in Alternatives)
                {
                    if (a.Participants == 0)
                    {
                        return false;
                    }
                }

                return true;
            }
        }


        /// <summary>
        /// A list of users participating in this test
        /// </summary>
        public int Participants
        {
            get
            {
                int count = 0;
                foreach (ABAlternative alt in Alternatives)
                {
                    count += alt.Participants;
                }

                return count;
            }
        }

        /// <summary>
        /// Count of total conversions for this test, regardless of outcome.
        /// </summary>
        public int Conversions
        {
            get
            {
                int count = 0;
                foreach (ABAlternative alt in Alternatives)
                {
                    count += alt.Conversions;
                }

                return count;
            }
        }

        /// <summary>
        /// Rate of total conversions for this test, regardless of outcome.
        /// </summary>
        public double ConversionRate
        {
            get 
            {
                int p = Participants;
                return  p == 0d ? 0d : (double)Conversions / p; 
            }
        }

        /// <summary>
        /// Rate of total conversions for this test, regardless of outcome, formatted like "3.22%"
        /// </summary>
        public string PrettyConversionRate
        {
            get { return (ConversionRate * 100).ToString("0.##") + "%"; }
        }

        public Experiment()
        {
            CreatedOn = DateTime.Now;
            Alternatives = new List<ABAlternative>();
        }

        public Experiment(string testName)
            : this()
        {
            TestName = testName;
        }

        public Experiment(string testName, params string[] alternatives)
            : this(testName)
        {
            foreach (string alt in alternatives)
            {
                Alternatives.Add(new ABAlternative(alt));
            }
        }

        /// <summary>
        /// Given a userID, return the appropriate alternative.
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public ABAlternative GetUserAlternative(int userID)
        {
            int index = userID % Alternatives.Count;
            ABAlternative choice = Alternatives[index];
            choice.Index = index;

            return choice;
        }
        
        public ABAlternative GetBestAlternative()
        {
            ABAlternative best = null;
            foreach (ABAlternative alt in Alternatives)
            {
                if (best == null || alt.ConversionRate > best.ConversionRate)
                {
                    best = alt;
                }
            }

            return best;
        }

        public ABAlternative GetWorstAlternative()
        {
            ABAlternative best = null;
            foreach (ABAlternative alt in Alternatives)
            {
                if (best == null || alt.ConversionRate <= best.ConversionRate)
                {
                    best = alt;
                }
            }

            return best;
        }

        public double GetPValue()
        {
            IStatisticalTest test = this.SignificanceTest;
            return test.GetPValue(this);
        }

        public string GetResultDescription()
        {
            IStatisticalTest test = this.SignificanceTest;
            return test.GetResultDescription(this);
        }

        public string[] AssumptionsToCheck
        {
            get
            {
                IStatisticalTest test = this.SignificanceTest;
                return test.AssumptionsToCheck;
            }
        }

        public string SignificanceTestName
        {
            get
            {
                int n = Alternatives.Count;

                if (n < 2)
                {
                    return @"None";
                }
                else if (n == 2)
                {
                    return @"Two-proportion z-test";
                }
                else
                {
                    return @"Chi-square test";
                }
            }
        }

        private IStatisticalTest SignificanceTest
        {
            get
            {
                int n = Alternatives.Count;

                if (n < 2)
                {
                    return null;
                }
                else if (n == 2)
                {
                    return new TwoProportionZTest();
                }
                else
                {
                    return new MultipleProportionChiSquareTest();
                }
            }
        }

    }
}
