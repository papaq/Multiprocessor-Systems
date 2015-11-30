using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfLab
{
    public class Tasks
    {
        readonly public int[] proces = new int[5];
        readonly public long difficulty;

        public Tasks(int pr1, int pr2, int pr3, int pr4, int pr5, int diff)
        {
            proces[0] = pr1;
            proces[1] = pr2;
            proces[2] = pr3;
            proces[3] = pr4;
            proces[4] = pr5;

            difficulty = diff;
        }
    }
}
