using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DataSetGenerator {
    public class Test {
        
        public String ID { get; set; }

        public Dictionary<GestureType, List<Attempt>> Attempts { get; set; }

        private Test() {
            Attempts = new Dictionary<GestureType, List<Attempt>>();
        }
        
        public Test(int id, DataSource source) : this() {
            string path = DataGenerator.TestFileDirectory(source) + id + ".test";
            StreamReader sr = new StreamReader(path);
            ID = path.Split('/').Last().Split('.')[0];
            
            TimeSpan attemptTime = default(TimeSpan);

            using (sr) {
                string line = "";
                GridSize size = GridSize.Large;
                GestureType type = GestureType.Pinch;
                GestureDirection direction = GestureDirection.Push;
                TimeSpan currentTime = TimeSpan.Zero;
                while ((line = sr.ReadLine()) != null) {
                    if(line == "") { continue; }
                    string[] time = line.Trim().Split('[', ']')[1].Split(':');
                    TimeSpan entryTime = new TimeSpan(Int32.Parse(time[0]), Int32.Parse(time[1]), Int32.Parse(time[2]));
                    if (line.Contains("Started new gesture test.")) {

                        string tobesearched = "Type: ";
                        string toBefound = line.Substring(line.IndexOf(tobesearched) + tobesearched.Length).Split(' ')[0];
                        switch (toBefound) {
                            case "Throw": type = GestureType.Throw; break;
                            case "Tilt": type = GestureType.Tilt; break;
                            case "Swipe": type = GestureType.Swipe; break;
                            case "Pinch": type = GestureType.Pinch; break;
                        }

                        tobesearched = "Direction: ";
                        toBefound = line.Substring(line.IndexOf(tobesearched) + tobesearched.Length).Split(' ')[0];
                        direction = toBefound == "Push" ? GestureDirection.Push : GestureDirection.Pull;
                        if (!Attempts.ContainsKey(type)) {
                            Attempts.Add(type, new List<Attempt>());
                        }
                        
                        currentTime = entryTime;
                    }
                    else if (line.Contains("Grid height: 10")) {
                        size = GridSize.Small;
                    }
                    else if (line.Contains("Grid height: 5")) {
                        size = GridSize.Large;
                    }
                    else if (line.Contains("Target")) {
                        if (line.Contains("JL: NA")) {
                            currentTime = entryTime;
                            continue;
                        }
                        attemptTime = entryTime - currentTime;
                        currentTime = entryTime;
                        

                        Attempt attempt = new Attempt(ID, line, attemptTime, size, direction, type, source);
                        Attempts[type].Add(attempt);
                    }
                }
            }

            if(source == DataSource.Old) {
                FixTest();
            }
        }

        public Test(List<Attempt> attempts) : this() {
            ID = attempts[0].ID;
            foreach(var technique in DataGenerator.AllTechniques) {
                var techniqueQuery = from attempt in attempts
                                     where attempt.Type == technique
                                     select attempt;
                Attempts[technique] = techniqueQuery.ToList();
            }
        }



        private void FixTest() {
            switch (ID) {
                case "1":
                    Attempts[GestureType.Tilt][14].Time = TimeSpan.FromSeconds(6);
                    Attempts[GestureType.Throw][4].Time = TimeSpan.FromSeconds(8);
                    Attempts[GestureType.Throw][4].Size = GridSize.Small;
                    Attempts[GestureType.Swipe][5].Time = TimeSpan.FromSeconds(6);
                    Attempts[GestureType.Swipe][5].Size = GridSize.Large;
                    Attempts[GestureType.Swipe][11].Time = TimeSpan.FromSeconds(6);
                    Attempts[GestureType.Swipe][11].Size = GridSize.Large;
                    Attempts[GestureType.Swipe][13].Time = TimeSpan.FromSeconds(6);
                    Attempts[GestureType.Swipe][13].Size = GridSize.Large;
                    break;
                case "2":
                    Attempts[GestureType.Swipe][1].Time = TimeSpan.FromSeconds(6);
                    Attempts[GestureType.Swipe][13].Time = TimeSpan.FromSeconds(6);
                    break;
                case "4":
                    Attempts[GestureType.Throw][17].Time = TimeSpan.FromSeconds(7);
                    Attempts[GestureType.Throw][17].Size = GridSize.Large;
                    Attempts[GestureType.Tilt][12].Time = TimeSpan.FromSeconds(5);
                    Attempts[GestureType.Tilt][14].Time = TimeSpan.FromSeconds(6);
                    Attempts[GestureType.Tilt][14].Size = GridSize.Small;
                    break;
                case "5":
                    Attempts[GestureType.Swipe][14].Time = TimeSpan.FromSeconds(4);
                    break;
                case "8":
                    Attempts[GestureType.Throw][4].Time = TimeSpan.FromSeconds(8);
                    Attempts[GestureType.Throw][4].Size = GridSize.Small;
                    break;
                default:
                    break;
            }
        }
    }
}
