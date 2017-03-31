using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

using WebDataParser.Models;
using DataSetGenerator;

using Point = DataSetGenerator.Point;

namespace WebDataParser {
    public static class TestDataViewModelFactory {

        static Dictionary<string, Test> Tests = new Dictionary<string, Test>();
        static Dictionary<string, TestDataViewModel> TestViewModels = new Dictionary<string, TestDataViewModel>();
        static List<GestureType> AllGestures = new List<GestureType> { GestureType.Pinch, GestureType.Swipe, GestureType.Throw, GestureType.Tilt };

        private static TestDataViewModel GetTest(DataSource source) {

            if (TestViewModels.ContainsKey("average")) {
                return TestViewModels["average"];
            }
            TestViewModels.Add("average", new TestDataViewModel("average"));


            if (Tests.Count != AttemptRepository.GetTestCount(source)) {
                Tests.Clear();
                foreach(var test in AttemptRepository.GetTests(source)) {
                    Tests.Add(test.ID, test);
                }
            }

            var tests = Tests.Values.ToList();

            var averageHitPercentagePerGesture = GetAverageHitPercentagePerTurn(tests);
            var averageTimePerTargetPerGesture = GetAverageTimePerTarget(tests);


            foreach (var gesture in AllGestures) {
                GestureInfo info = new GestureInfo();
                info.HitData = GetJSPercentageArray(averageHitPercentagePerGesture[gesture], gesture);
                info.TimeData = GetJSTimeArray(averageTimePerTargetPerGesture[gesture], gesture);
                info.HitPercentage = averageHitPercentagePerGesture[gesture].Last() * 100f;

                List<Attempt> attempts = new List<Attempt>();
                foreach(var test in tests) {
                    attempts.AddRange(test.Attempts[gesture]);
                }
                info.Img = DrawHitBox(attempts);

                TestViewModels["average"].GestureInformation[GetGestureTypeString(gesture)] = info;

            }

            return TestViewModels["average"];
        }

        public static MemoryStream GetHitbox(string id, string type) {
            return TestViewModels[id].GestureInformation[type].Img;
        }

        public static TestDataViewModel GetTest(string id, DataSource source) {

            if(id == "average") {
                return GetTest(source);
            }

            if (TestViewModels.ContainsKey(id)) {
                return TestViewModels[id];
            }
            TestViewModels.Add(id, new TestDataViewModel(id));
            TestViewModels[id].GestureInformation = new Dictionary<string, GestureInfo>();

             
            if (!Tests.ContainsKey(id)) {

                Tests[id] = AttemptRepository.GetTest(id, source);
            }
            
            foreach (var gesture in AllGestures) {
                GestureInfo info = new GestureInfo();
                var hitsPerTry = MathHelper.GetHitsPerTry(Tests[id].Attempts[gesture]);
                info.HitData = GetJSPercentageArray(hitsPerTry, gesture);
                info.TimeData = GetJSTimeArray(MathHelper.GetTimePerTarget(Tests[id].Attempts[gesture]), gesture);
                info.HitPercentage = hitsPerTry.Last() * 100f;
                info.Img = DrawHitBox(Tests[id].Attempts[gesture]);

                TestViewModels[id].GestureInformation[GetGestureTypeString(gesture)] = info;
                
            }


            return TestViewModels[id];

        }

        private static string GetGestureTypeString(GestureType type) {
            switch (type) {
                case GestureType.Pinch: return "pinch"; 
                case GestureType.Swipe: return "swipe"; 
                case GestureType.Throw: return "throw";
                case GestureType.Tilt: return "tilt";
            }
            return "exception";
        }

        private static Dictionary<GestureType, float[]> GetAverageHitPercentagePerTurn(List<Test> tests) {

            List<GestureType> gestures = new List<GestureType> { GestureType.Pinch, GestureType.Swipe, GestureType.Throw, GestureType.Tilt };

            Dictionary<GestureType, float[]> averageHitPercentagePerGesture = new Dictionary<GestureType, float[]>();

            foreach (var gesture in gestures) {

                float[] avgPercentage = new float[tests[0].Attempts[0].Count];
                List<float[]> percentages = new List<float[]>();

                foreach (var test in tests) {
                    percentages.Add(MathHelper.GetHitsPerTry(test.Attempts[gesture]));
                }

                for (int i = 0; i < avgPercentage.Length; i++) {
                    foreach (var percentage in percentages) {
                        avgPercentage[i] += percentage[i];
                    }
                    avgPercentage[i] /= (float)percentages.Count;
                }
                averageHitPercentagePerGesture.Add(gesture, avgPercentage);
            }

            return averageHitPercentagePerGesture;

        }
        private static Dictionary<GestureType, float[]> GetAverageTimePerTarget(List<Test> tests) {
            List<GestureType> gestures = new List<GestureType> { GestureType.Pinch, GestureType.Swipe, GestureType.Throw, GestureType.Tilt };

            Dictionary<GestureType, float[]> averageTimePerGesture = new Dictionary<GestureType, float[]>();

            foreach (var gesture in gestures) {

                float[] averageTime = new float[tests[0].Attempts[0].Count];
                List<float[]> times = new List<float[]>();

                foreach (var test in tests) {
                    times.Add(MathHelper.GetTimePerTarget(test.Attempts[gesture]));
                }

                for (int i = 0; i < averageTime.Length; i++) {
                    foreach (var time in times) {
                        averageTime[i] += time[i];
                    }
                    averageTime[i] /= (float)times.Count;
                }
                averageTimePerGesture.Add(gesture, averageTime);
            }

            return averageTimePerGesture;
        }

        private static MemoryStream DrawHitBox(List<Attempt> attempts) {

            //61 pixel sized squares, makes it better to look at
            int cellSize = 61;
            int bmsize = cellSize * 3;

            Bitmap hitbox = new Bitmap(bmsize, bmsize);
            Graphics hBGraphic = Graphics.FromImage(hitbox);
            hBGraphic.FillRectangle(Brushes.White, 0, 0, bmsize, bmsize);
            hBGraphic.DrawRectangle(new Pen(Brushes.Black, 1.0f), cellSize, cellSize, cellSize, cellSize);

            foreach (var attempt in attempts) {
                Brush brush = attempt.Size == GridSize.Large ? Brushes.Red : Brushes.Green;
                float scale = attempt.Size == GridSize.Large ? 122.0f : 61.0f;
                Point p = new Point(attempt.TargetCell.X, attempt.TargetCell.Y);
                p.X = p.X * scale; p.Y = p.Y * scale;
                p.X = attempt.Pointer.X - p.X;
                p.Y = attempt.Pointer.Y - p.Y;
                if (attempt.Size == GridSize.Large) {
                    p.X /= 2;
                    p.Y /= 2;
                }

                p.X += cellSize;
                p.Y += cellSize;

                if (!((p.X < 0) && (p.X >= bmsize)) || !((p.Y < 0) && (p.Y >= bmsize))) {
                    hBGraphic.FillRectangle(brush, (float)p.X, (float)p.Y, 2, 2);
                }
            }

            hBGraphic.Save();


            MemoryStream ms = new MemoryStream();

            hitbox.Save(ms, ImageFormat.Png);

            hBGraphic.Dispose();
            hitbox.Dispose();

            return ms;

        }

        private static string GetJSPercentageArray(float[] percentages, GestureType type) {

            //var data = [ [[0, 0], [1, 1], [1,0]] ];

            string array = " [ ";
            for (int i = 0; i < percentages.Length; i++) {
                float percentage = (float)percentages[i] * 100.0f;
                string sPercentage = percentage.ToString().Replace(',', '.');
                array += "[" + (i + 1) + ", " + sPercentage + "], ";
            }

            array = array.Remove(array.Length - 2);
            array += " ];\n";

            return "var " + type + "Data = " + array;
        }

        private static string GetJSTimeArray(float[] times, GestureType type) {

            //var data = [ [[0, 0], [1, 1], [1,0]] ];

            string array = " [ ";
            for (int i = 0; i < times.Length; i++) {
                float time = (float)times[i];
                string sTime = time.ToString().Replace(',', '.');
                array += "[" + (i + 1) + ", " + sTime + "], ";
            }

            array = array.Remove(array.Length - 2);
            array += " ];\n";

            return "var Time" + type + "Data = " + array;
        }
    }
}