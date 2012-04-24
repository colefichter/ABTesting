using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.UI;

using ABTesting.Controls;
using ABTesting.Helpers;

namespace ABTesting
{
    public class FairlyCertain
    {
        private TestFileType _fileType = TestFileType.Automatic;

        /// <summary>
        /// These are bots that we want to ignore.  If any of these strings shows up in a useragent, we'll show the default option and not score the request.
        /// </summary>
        public static List<string> Bots = new List<string> { "Googlebot", "Slurp", "msnbot", "nagios", "Baiduspider", "Sogou", "SiteUptime.com", "Python", "DotBot", "Feedfetcher", "Jeeves", };

        private FileHelper Helper
        {
            get { return FileHelper.GetInstance(_fileType); }
        }
        
        public SerializableDictionary<string, Experiment> GetTests()
        {
            return Helper.Load();
        }

        public FairlyCertain()
        {
            _fileType = TestFileType.Automatic;
        }

        public FairlyCertain(TestFileType fileType)
        {
            _fileType = fileType;
        }
        
        private void SaveTests(SerializableDictionary<string, Experiment> tests)
        {
            Helper.Save(tests);
        }
        
        /// <summary>
        /// Create a new test, or load an existing one.
        /// </summary>
        /// <param name="testName"></param>
        /// <param name="alternatives"></param>
        /// <returns></returns>
        public Experiment GetOrCreateTest(string testName, ControlCollection alternatives)
        {
            SerializableDictionary<string, Experiment> tests = GetTests();
            Experiment test;
            if (tests.ContainsKey(testName))
            {
                test = tests[testName];
            }
            else
            {
                string[] altNames = new string[alternatives.Count];
                for (int a = 0; a < alternatives.Count; a++)
                {
                    Alternative alt = (Alternative)alternatives[a];
                    if (!String.IsNullOrEmpty(alt.Name))
                    {
                        altNames[a] = alt.Name;
                    }
                    else
                    {
                        altNames[a] = "Alternative " + (a + 1);
                    }
                }

                test = new Experiment(testName, altNames);
                tests.Add(testName, test);

                SaveTests(tests);
            }

            return test;
        }

        /// <summary>
        /// For the specified test, pick an alternative to always show this user, and return that alternative.
        /// </summary>
        /// <param name="test"></param>
        /// <returns></returns>
        public ABAlternative GetUserAlternative(Experiment test)
        {
            //complete an experiment as soon as we reach our required sample size...
            if (test.IsComplete)
            {
                return test.GetBestAlternative();
            }

            ABUser user = IdentifyUser();
            ABAlternative choice = test.GetUserAlternative(user.ID);

            if (!user.Tests.Contains(test.TestName) && !IsBotRequest()) //don't score the participation more than once for an identified user (don't score for bots either)
            {
                choice.ScoreParticipation();
                user.Tests.Add(test.TestName);
                user.SaveToCookie();

                //persist the new participant count to the file store...
                ScoreParticipation(test.TestName, choice);
            }

            return choice;
        }
        
        private void ScoreParticipation(string testName, ABAlternative choice)
        {
            SerializableDictionary<string, Experiment> tests = GetTests();
            if (tests.ContainsKey(testName))
            {
                Experiment t = tests[testName];
                foreach (ABAlternative a in t.Alternatives)
                {
                    if (a.Content == choice.Content)
                    {
                        a.Participants += 1;
                        break;
                    }
                }

                SaveTests(tests);
            }
        }

        /// <summary>
        /// Mark this user as having converted for the specified tests.
        /// </summary>
        public void ScoreConversion(string testName)
        {
            ABUser user = IdentifyUser();
            if (!user.Tests.Contains(testName) || user.Conversions.Contains(testName))
            {
                // not part of the test or already scored.
                return;
            }

            SerializableDictionary<string, Experiment> tests = GetTests();
            if (tests.ContainsKey(testName))
            {
                Experiment t = tests[testName];
                
                ABAlternative choice = t.GetUserAlternative(user.ID);
                choice.ScoreConversion();

                user.Conversions.Add(testName);
                user.SaveToCookie();

                SaveTests(tests);
            }
        }

        //Backwards compat!
        public static void Score(string testName)
        {
            (new FairlyCertain()).ScoreConversion(testName);
        }
        
        #region private helpers
        /// <summary>
        /// Check the current request against our list of known Bot useragent signatures
        /// </summary>
        /// <returns></returns>
        private static bool IsBotRequest()
        {
            if (HttpContext.Current == null
                || HttpContext.Current.Request == null
                || String.IsNullOrEmpty(HttpContext.Current.Request.UserAgent))
            {
                return true;
            }

            string userAgent = HttpContext.Current.Request.UserAgent;
            foreach (string botIdentifier in Bots)
            {
                if (userAgent.Contains(botIdentifier))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// If we've seen this user before, pull his info from the cookie.  If not, start tracking him.
        /// </summary>
        /// <returns></returns>
        private static ABUser IdentifyUser()
        {
            ABUser user = ABUser.LoadFromCookie();
            return user;
        }
        #endregion

        public void DeleteTest(string name)
        {
            SerializableDictionary<string, Experiment> tests = GetTests();
            if (tests.ContainsKey(name))
            {
                tests.Remove(name);
                SaveTests(tests);
            }
        }
    }
}