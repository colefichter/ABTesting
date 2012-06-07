using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABTesting
{
    internal static class NaivePowerAnalysis
    {
        public static int EstimateSampleSize(float estimatedProportion)
        {
            float power = (float)0.8;
            float significanceLevel = (float)0.05;

            float sensitivity = GetSensitivity(estimatedProportion);

            float p2 = Math.Max((float)0, Math.Min((float)100, estimatedProportion * (1 + sensitivity)));



            //TODO: the calculation...
            double n = 1; 






            return (int) Math.Ceiling(n);
        }

        private static float GetSensitivity(float estimatedProportion)
        {
            //Scale sensitivity based on the estimated proportion size. We have a fairly low-traffic site, and this prevents the number of observations
            //from exploding for small values of estimatedProportion.
            if (estimatedProportion <= 0.03)
            {
                return (float) 0.5;
            }
            else if (estimatedProportion > 0.03 && estimatedProportion <= 0.05)
            {
                return (float)0.4;
            }
            else if (estimatedProportion > 0.05 && estimatedProportion <= 0.08)
            {
                return (float) 0.3;
            }
            else if (estimatedProportion > 0.08 && estimatedProportion <= 0.1)
            {
                return (float)0.2;
            }
            else
            {
                return (float)0.1;
            }
        }
    }
}
