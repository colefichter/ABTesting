
using System.Xml.Serialization;

namespace ABTesting
{
	public class ABAlternative
	{
		public string Content { get; set; }
		public int Participants { get; set; }
		public int Conversions { get; set; }
        
        /// <summary>
        /// Alias for "Participants".
        /// </summary>
        public int Successes
        {
            get
            {
                return Conversions;
            }
        }

        /// <summary>
        /// Alias for "Participants - Conversions".
        /// </summary>
        public int Failures
        {
            get
            {
                return Participants - Conversions;
            }
        }

		[XmlIgnore]
		public int Index { get; set; }

        /// <summary>
        /// In statistics, we'd call this the success proportion.
        /// </summary>
        public double ConversionRate
        {
            get
            {
                int p = Participants;
                return p == 0d ? 0d : (double)Conversions / p;
            }
        }

		public string PrettyConversionRate
		{
			get { return (ConversionRate*100).ToString("0.##") + "%"; }
		}


		public ABAlternative()
		{
		}

		public ABAlternative(string content)
		{
			Content = content;
		}

		public void ScoreParticipation()
		{
			Participants++;
		}

		public void ScoreConversion()
		{
			Conversions++;
		}

        /// <summary>
        /// If this alternative (ie: sample) has at least 10 failures and 10 successes, then the sample size assumption of the two-proportion z-test is satisfied.
        /// </summary>
        public bool SampleMeetsTestAssumtions
        {
            get
            {  
                //CF:If we wanted to get really strict about this, we could move this into the Experiment class and compute the pooled expected failure/success rate
                //to accomodate the case where we may have one alternative with low success rates. See the Gardasil/HPV example in "Intro Stats" page 566.
                //For our needs, checking for 10 success & failures seems fine.
                return Successes >= 10 && Failures >= 10;
            }
        }
	}
}
