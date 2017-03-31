using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SW9_Project {
    public class Cell {

        public Rectangle GridCell { get; set; }
        public Shape Shape { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public Cell(Rectangle cell) {
            GridCell = cell;
        }
    }
}
