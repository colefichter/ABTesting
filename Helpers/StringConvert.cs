using System;

namespace ABTesting.Helpers
{

	/// <summary>
	/// Converts strings to other data types, with no fear of repercussions from bad data.
	/// </summary>
	public class StringConvert
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="s"></param>
		/// <param name="fallbackValue"></param>
		/// <returns></returns>
		public static int ToInt32(string s, int fallbackValue)
		{
			// TryParse is actually kinda expensive.  Let's test for the most common miss case first:
			if (String.IsNullOrEmpty(s))
			{
				return fallbackValue;
			}

			int result;
			if (Int32.TryParse(s, out result))
			{
				return result;
			}

			return fallbackValue;
		}

	}
}