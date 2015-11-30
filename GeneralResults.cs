using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace WpfLab
{
    class GeneralResults
    {
        readonly public string[] lines;

        public GeneralResults(int counter, int sumProcCows, long opsCou, long opsProces,
            long sumPower, long workPowerSum, int timeInSecs, Stopwatch sw, int taskEmerg,
            int taskFriq, long lowLimit, long highLimit, int checkTime)
        {
            lines = new string[12];

            lines[0] = String.Format("{0,-30} {1,0}", "Tasks emerged:", counter);
            lines[1] = String.Format("{0,-30} {1,-2} {2,-2} {3,-2} {4,0}", "Tasks done:", sumProcCows.ToString(), " (", (sumProcCows * 100 / counter).ToString(), "% )");
            lines[2] = String.Format("{0,-30} {1,0}", "Operations emerged:", opsCou);
            lines[3] = String.Format("{0,-30} {1,-2} {2,-2} {3,-2} {4,0}", "Operations done:", opsProces.ToString(), " (", (opsProces * 100 / opsCou).ToString(), "% )");
            lines[4] = String.Format("{0,-30} {1,1}", "System efficiency:", (float)(opsProces / (float)(sumPower * timeInSecs)));
            lines[5] = String.Format("{0,-30} {1,1}", "Real system efficiency:", (float)(opsProces / (float)(workPowerSum * timeInSecs)));
            lines[6] = String.Format("{0,-30} {1,0}", "Time elapsed:", sw.Elapsed);
            lines[7] = String.Format("{0,-30} {1,-2} {2,0}", "Tasks probabillity:", taskEmerg.ToString(), "%");
            lines[8] = String.Format("{0,-30} {1,-2} {2,0}", "Emerge every:", taskFriq.ToString(), "milliseconds");
            lines[9] = String.Format("{0,-30} {1,-2} {2,-2} {3,-2} {4,-2}", "Tasks difficulty range:", lowLimit.ToString(), "to", highLimit.ToString(), "operations");
        }

        public GeneralResults(int number, string partTime, int c, int counter, long opsProc, long opsCou, long power, double midOps)
        {
            lines = new string[8];

            lines[0] = String.Format("{0,-1} {1,0} {2,-2}", "Processor", number, ":");
            lines[1] = String.Format("{0,-30} {1,-1}", "Processing time:", partTime);
            lines[2] = String.Format("{0,-30} {1,-2} {2,-2} {3,-2} {4,0}", "Tasks done:", (c).ToString(), " (", (c * 100 / counter).ToString(), "% )");
            lines[3] = String.Format("{0,-30} {1,-2} {2,-2} {3,-2} {4,0}", "Operations done:", (opsProc).ToString(), " (", (opsProc * 100 / opsCou).ToString(), "% )");
            lines[4] = String.Format("{0,-30} {1,-2} {2,0}", "Power:", power.ToString(), "operations per second");
            lines[5] = String.Format("{0,-30} {1,-2} {2,0}", "Average task time:", ((double)(midOps * 1000 / (power))).ToString(), "milliseconds");
        }
    }
}
