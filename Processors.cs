using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfLab
{
    class Processors
    {

        readonly public int number = -1;

        public long power = 1,
            opsInQueue = 0;
        public int tasksCounter = 0;
        public long opsCounter = 0;
        public bool weak = false;
        public bool strong = false;

        public Processors(int n)
        {
            number = n;
        }
    }
}
