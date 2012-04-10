using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace ABTesting
{
	public class ABUser
	{
		public int ID { get; set; }
		public string Key 
		{ 
			get { return ID.ToString(); }
		}
		public List<string> Tests { get; set; }
		public List<string> Conversions { get; set; }

		public ABUser()
		{
			ID = new Random().Next();
			Tests = new List<string>();
			Conversions = new List<string>();
		}

		public static ABUser LoadFromCookie()
		{
			ABUser user = new ABUser();

			HttpCookie inCookie = HttpContext.Current.Request.Cookies["ab"];
			if (inCookie != null)
			{
				try
				{
					user.LoadFromINI(inCookie.Value);
				}
				catch
				{
					// user messed with his cookie.  No worries for us.
				}

				user.SaveToCookie();
			}

			return user;
		}

		public void SaveToCookie()
		{
			HttpCookie cookie = new HttpCookie("ab", ToINI());
			cookie.Expires = DateTime.Now.AddYears(1);

			// first remove, then add, in case we've already added this cookie as part of a previous save during this page load.
			HttpContext.Current.Response.Cookies.Remove("ab");
			HttpContext.Current.Response.Cookies.Add(cookie);

			// fix up the incoming cookie so that it will load correctly if we need it again during this page load.
			HttpContext.Current.Request.Cookies.Remove("ab");
			HttpContext.Current.Request.Cookies.Add(cookie);
		}

		public void LoadFromINI(string ini)
		{
			string[] lines = ini.Split('|');
			foreach (string line in lines)
			{
				if (String.IsNullOrEmpty(line))
				{
					continue;
				}
				string[] tokens = line.Split('=');
				if (tokens.Length == 2)
				{
					string key = tokens[0].Trim().ToLower();
					string value = tokens[1].Trim();

					switch (key)
					{
						case "id":
                            ID = Helpers.StringConvert.ToInt32(value, ID);
							break;

						case "tests":
							Tests = ParseCSV(value);
							break;

						case "conversions":
							Conversions = ParseCSV(value);
							break;
					}
				}
			}
		}

		private List<string> ParseCSV(string csv)
		{
			string[] tokens = csv.Split(',');
			List<string> list = new List<string>();

			foreach (string token in tokens)
			{
				string value = token.Trim();
				if (!String.IsNullOrEmpty(value))
				{
					if (!list.Contains(value))
					{
						list.Add(value);
					}
				}
			}

			return list;
		}

		private string StringListToCSV(List<string> list)
		{
			StringBuilder builder = new StringBuilder();
			foreach (string item in list)
			{
				if (builder.Length != 0)
				{
					builder.Append(",");
				}

				builder.Append(String.Format("{0}"
					, item.Replace(",", " ").Replace("=", " ").Replace("|", " ")
					));

			}
			return builder.ToString();
		}

		public string ToINI()
		{
			return String.Format(@"ID={0}|Tests={1}|Conversions={2}"
				, ID
				, StringListToCSV(Tests)
				, StringListToCSV(Conversions)
				);
		}
	}
}
