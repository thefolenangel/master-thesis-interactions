using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DataSetGenerator;

namespace SW9_Project {
    public class Target {
        public int X { get; }
        public int Y { get; }
        public GridSize Size { get;  }
        public JumpLength Length { get; }
        public Target(int x, int y, GridSize size, JumpLength length) {
            X = x;
            Y = y;
            Size = size;
            Length = length;
        }

        public bool IsValid() {
            int xBounds = Size == GridSize.Large ? 10 : 20;
            int yBounds = Size == GridSize.Large ? 5 : 10;
            return !(X >= xBounds || Y >= yBounds);
        }

        public static void Initialize() {
            List<Queue<Target>> list = new List<Queue<Target>>();
            if (GlobalVars.isTargetPractice)
            {
                for (int i = 0; i < 8; i++)
                {
                    list.Add(LoadSequence(i));
                }
            }
            else
            {
                for (int i = 8; i < 12; i++)
                {
                    list.Add(LoadSequence(i));
                }
            }
            list.Shuffle();
            TargetSequences = new Queue<Queue<Target>>(list);

        }

        static Queue<Queue<Target>> TargetSequences;

        public static Queue<Target> GetNextSequence() {
            if(TargetSequences == null || TargetSequences.Count == 0) {
                Initialize();
            }
            return TargetSequences.Dequeue();
        }

        private static Random R;
        public static Queue<Target> GetPracticeTargets() {
            if(R == null) {
                R = new Random();
            }
            Queue<Target> targets = new Queue<Target>();
            targets.Enqueue(new Target(4, 2, GridSize.Large, JumpLength.NA));

            List<int> xPossibilities = new List<int>();
            List<int> yPossibilities = new List<int>();

            for (int i = 0; i < 10; i++) {
                if (i < 4 - 1 || i > 4 + 1) {
                    xPossibilities.Add(i);
                }
            }
            for (int i = 0; i < 5; i++) {
                if (i < 2 - 1 || i > 2 + 1) {
                    yPossibilities.Add(i);
                }
            }
            int x = xPossibilities[R.Next(xPossibilities.Count)], y = yPossibilities[R.Next(yPossibilities.Count)];
            targets.Enqueue(new Target(x, y, GridSize.Large, JumpLength.NA));
            return targets;
        }

        static Queue<Target> LoadSequence(int sequenceNumber) {
            Queue<Target> targets = new Queue<Target>();
            using (StreamReader sr = new StreamReader("sequences/" + sequenceNumber + "_sequence.txt")) {
                string line = "";
                while((line = sr.ReadLine()) != null) { 
                    if(line == "") { break; }
                    string[] targetInfo = line.Split(',');
                    int x = Int32.Parse(targetInfo[0].Trim());
                    int y = Int32.Parse(targetInfo[1].Trim());
                    GridSize size = String.Compare(targetInfo[2].Trim(), "L", true) == 0 ? GridSize.Large : GridSize.Small;
                    string tLength = targetInfo[3].Trim();
                    JumpLength length = JumpLength.Long;
                    switch (tLength) {
                        case "JL": length = JumpLength.Long; break;
                        case "JM": length = JumpLength.Medium; break;
                        case "JS": length = JumpLength.Short; break;
                        default: length = JumpLength.NA; break;
                    }
                    Target t = new Target(x, y, size, length);
                    targets.Enqueue(t);
                }
            }
            return targets;
        }
    }
}
