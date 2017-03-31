using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSetGenerator {
    class Program {
        static void Main(string[] args) {

            var oldTest = DataGenerator.GetTests(DataSource.Old);
            var targetTest = DataGenerator.GetTests(DataSource.Target);
            var fieldTest = DataGenerator.GetTests(DataSource.Field);

            Console.Read();
            
        }
    }
}
