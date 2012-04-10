using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using ABTesting.Controls;
using ABTesting.Helpers;

namespace ABTesting
{
    public class FairlyCertain
    {
        private static SerializableDictionary<string, Experiment> _tests;
        private static object syncRoot = new Object();

        /// <summary>
        /// These are bots that we want to ignore.  If any of these strings shows up in a useragent, we'll show the default option and not score the request.
        /// </summary>
        public static List<string> Bots = new List<string> { "Googlebot", "Slurp", "msnbot", "nagios", "Baiduspider", "Sogou", "SiteUptime.com", "Python", "DotBot", "Feedfetcher", "Jeeves", };

        /// <summary>
        /// All tests, in progress or otherwise
        /// </summary>
        public static SerializableDictionary<string, Experiment> Tests
        {
            get
            {
                lock (syncRoot)
                {
                    if (_tests == null)
                    {
                        _tests = FileHelper.GetInstance().Load();
                    }
                }

                return _tests;
            }
            set
            {
                lock (syncRoot)
                {
                    _tests = value;
                }
            }
        }

        static FairlyCertain()
        {
        }

        #region public interface (for end users)

        /// <summary>
        /// This is the meat of the whole library, and likely the only method you'll need to call.
        /// Given a set of alternatives, pick one to always show for this user and show it.
        /// Remember this test by name.
        /// </summary>
        /// <param name="testName"></param>
        /// <param name="alternatives"></param>
        /// <returns></returns>
        public static string Test(string testName, params string[] alternatives)
        {
            // TODO - short circuit

            Experiment test = GetOrCreateTest(testName, alternatives);
            ABAlternative choice = GetUserAlternative(test);

            return choice.Content;
        }

        /// <summary>
        /// Special case for when you just want to switch between two alternates in an "if" block.
        /// </summary>
        /// <param name="testName"></param>
        /// <returns></returns>
        public static bool Test(string testName)
        {
            Experiment test = GetOrCreateTest(testName, "true", "false");
            ABAlternative choice = GetUserAlternative(test);
            return bool.Parse(choice.Content);
        }

        /// <summary>
        /// Mark this user as having converted for the specified tests.
        /// </summary>
        /// <param name="testNames"></param>
        public static void Score(params string[] testNames)
        {
            foreach (string name in testNames)
            {
                ABUser user = IdentifyUser();

                if (!user.Tests.Contains(name) || user.Conversions.Contains(name))
                {
                    // not part of the test or already scored.
                    return;
                }

                if (Tests.ContainsKey(name))
                {
                    Tests[name].Score(user);

                    user.Conversions.Add(name);
                    user.SaveToCookie();
                }
            }
        }

        #endregion

        #region public helpers (for library code)

        public void ForceSave()
        {
            lock (syncRoot)
            {
                FileHelper.GetInstance().ForceSave(_tests);
            }
        }

        public void ForceReload()
        {
            lock (syncRoot)
            {
                _tests = FileHelper.GetInstance().Load();
            }
        }

        public void ForceReload(TestFileType fileType)
        {
            lock (syncRoot)
            {
                _tests = FileHelper.GetInstance(fileType).Load();
            }
        }

        /// <summary>
        /// Create a new test, or load an existing one.
        /// </summary>
        /// <param name="testName"></param>
        /// <param name="alternatives"></param>
        /// <returns></returns>
        public static Experiment GetOrCreateTest(string testName, params string[] alternatives)
        {
            Experiment test;
            if (Tests.ContainsKey(testName))
            {
                test = Tests[testName];
            }
            else
            {
                test = new Experiment(testName, alternatives);
                Tests.Add(testName, test);
            }

            return test;
        }

        /// <summary>
        /// Create a new test, or load an existing one.
        /// </summary>
        /// <param name="testName"></param>
        /// <param name="alternatives"></param>
        /// <returns></returns>
        public static Experiment GetOrCreateTest(string testName, ControlCollection alternatives)
        {
            Experiment test;
            if (Tests.ContainsKey(testName))
            {
                test = Tests[testName];
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
                Tests.Add(testName, test);
            }

            return test;
        }

        /// <summary>
        /// Create a new test, or load an existing one.
        /// </summary>
        /// <param name="testName"></param>
        /// <param name="altCount"></param>
        /// <returns></returns>
        public static Experiment GetOrCreateTest(string testName, int altCount)
        {
            Experiment test;
            if (Tests.ContainsKey(testName))
            {
                test = Tests[testName];
            }
            else
            {
                string[] alternatives = new string[altCount];
                for (int a = 0; a < altCount; a++)
                {
                    alternatives[a] = "Alternative " + (a + 1);
                }
                test = new Experiment(testName, alternatives);
                Tests.Add(testName, test);
            }

            return test;
        }

        /// <summary>
        /// For the specified test, pick an alternative to always show this user, and return that alternative.
        /// </summary>
        /// <param name="test"></param>
        /// <returns></returns>
        public static ABAlternative GetUserAlternative(Experiment test)
        {
            //CF: complete an experiment as soon as we reach our required sample size...
            if (test.IsComplete)
            {
                return test.GetBestAlternative();
            }

            ABUser user = IdentifyUser();
            ABAlternative choice = test.GetUserAlternative(user.ID);

            if (!user.Tests.Contains(test.TestName) && !IsBotRequest())
            {
                choice.ScoreParticipation();

                // NOTE: If this runs into concurrency issues in high traffic, we'll probably want to move it out to a timer of some form.
                // For now though, it's probably safe here for most sites, since it's balling up changes and saving infrequently.
                FileHelper.GetInstance().Save(_tests);

                user.Tests.Add(test.TestName);
                user.SaveToCookie();
            }

            return choice;
        }
        #endregion

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


        public static bool DeleteTest(string name)
        {
            bool found = false;

            lock (syncRoot)
            {
                if (Tests.ContainsKey(name))
                {
                    found = true;
                    found = Tests.Remove(name);
                }
            }

            if (found)
            {
                (new FairlyCertain()).ForceSave();
            }

            return found;
        }
    }
}