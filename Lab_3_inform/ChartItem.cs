using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_informNo3
{
    class ChartItem
    {
        public string Name { get; set; }
        public int Number { get; set; }
        public ChartItem(String _Name, int _Number)
        {
            this.Name = _Name;
            this.Number = _Number;
        }
    }
}