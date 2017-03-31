using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using DataSetGenerator;

using Point = System.Windows.Point;

namespace SW9_Project.Logging
{
    class Logger
    {
        private static string directory;
        StreamWriter testStreamWriter; 

        int userID = 0;

        public static Logger CurrentLogger;

        private static int sgHeight, sgWidth, lgHeight, lgWidth;
        private static double canvasHeight, canvasWidth;

        public bool DebugMode { get; set; }

        public static void Intialize(int sHeight, int sWidth, int lHeight, int lWidth, double cnvasHeight, double cnvasWidth, DataSource source) {
            directory = DataGenerator.TestFileDirectory(source);
            if (CurrentLogger == null) {
                CurrentLogger = new Logger();
                CurrentLogger.DebugMode = false;
            }
            if (!Directory.Exists(directory)) {
                Directory.CreateDirectory(directory);
            }
            sgHeight = sHeight;
            sgWidth = sWidth;
            lgHeight = lHeight;
            lgWidth = lWidth;
            canvasHeight = cnvasHeight;
            canvasWidth = cnvasWidth;
        }

        private bool IsDirectoryEmpty(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }

        /// <summary>
        /// Generate userId. Files exist in log directory, get highest id and add 1 to the current userID
        /// </summary>
        /// <returns>userId</returns>
        public int NewUser()
        {
            var tests = Directory.GetFiles(directory, "*.test");
            userID = tests.Count() + 1;
            if (!DebugMode) {
                testStreamWriter = new StreamWriter(directory + userID + ".test", true);
            }
            return userID;
        }

        /// <summary>
        /// The user is finished and we are done writing to the file(s)
        /// </summary>
        public void EndUser()
        {
            string message = "Test session ended.";
            Log(message);
            testStreamWriter.Close();
            testStreamWriter.Dispose();
        }

        /// <summary>
        /// Log the creation of a new size test
        /// </summary>
        /// <param name="gridHeight"></param>
        /// <param name="gridWidth"></param>
        /// <param name="cellHeight"></param>
        /// <param name="cellWidth"></param>
        private void ChangeSize(int gridHeight, int gridWidth, double cellHeight, double cellWidth)
        {
            string message = "Changed grid size." +
                             " Grid height: " + gridHeight +
                             " Grid width: " + gridWidth +
                             " Cell height: " + cellHeight.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                             " Cell width: " + cellWidth.ToString(System.Globalization.CultureInfo.InvariantCulture);
            Log(message);
        }

        public void ChangeSize(GridSize size) {
            if(size == GridSize.Large) {
                ChangeSize(lgHeight, lgWidth, canvasHeight / lgHeight, canvasWidth / lgWidth);
            } else {
                ChangeSize(sgHeight, sgWidth, canvasHeight / sgHeight, canvasWidth / sgWidth);
            }
        }

        /// <summary>
        /// Log that new gesture test has started
        /// </summary>
        /// <param name="gestureType"></param>
        /// <param name="gestureDirection"></param>
        public void StartNewgestureTest(GestureType gestureType, GestureDirection gestureDirection)
        {
            string message = "Started new gesture test." + 
                             " Type: " + gestureType.ToString() + 
                             " Direction: " + gestureDirection.ToString();
            Log(message);
        }

        public void StartPracticeTime(GestureType type, GestureDirection direction) {
            string message = "Started new gesture practice." +
                             " Type: " + type.ToString() +
                             " Direction: " + direction.ToString();
            Log(message);

        }

        /// <summary>
        /// Log that the current gesture test has ended.
        /// </summary>
        public void EndCurrentGestureTest()
        {
            string message = "Gesture test ended.";
            Log(message);
        }

        /// <summary>
        /// Log that target has been hit
        /// </summary>
        public void CurrentTargetHit(bool hit, Cell target, Point p, Cell pointer, bool correctShape, JumpLength length) {
            string result = hit ? "Target: Hit  " : "Target: Miss ";
            string shape = correctShape ? "Shape: Correct " : "Shape: Wrong   ";
            string cells = "TC: (" + target.X.ToString("D2") + "," + target.Y.ToString("D2") + ")" + " CC: (" + pointer.X.ToString("D2") + ", " + pointer.Y.ToString("D2") + ") ";
            string jLength = "JL: " + length + " ";
            string pString = "Pointer position: (" + p.X.ToString("F1", System.Globalization.CultureInfo.InvariantCulture) +"," + p.Y.ToString("F1", System.Globalization.CultureInfo.InvariantCulture) + ").";
            string message = result + shape + cells + jLength + pString;
            Log(message);
        }

        /// <summary>
        /// Log a kinect gesture
        /// </summary>
        /// <param name="gesture"></param>
        public void AddNewKinectGesture(KinectGesture gesture, Cell cell)
        {
            string message = "KINECT GESTURE" + 
                             " Type: " + gesture.Type.ToString() +
                             " Direction: " + gesture.Direction.ToString() +
                             " Shape: " + gesture.Shape + 
                             " Pointer: X = " + gesture.Pointer.X.ToString(System.Globalization.CultureInfo.InvariantCulture) + " Y = " + gesture.Pointer.Y.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                             " Cell: X = " + cell.X + " Y = " + cell.Y;
            
        }

        /// <summary>
        /// Log a mobile gesture
        /// </summary>
        /// <param name="gesture"></param>
        public void AddNewMobileGesture(MobileGesture gesture)
        {
            string message = "MOBILE GESTURE" +
                             " Type: " + gesture.Type.ToString() +
                             " Direction: " + gesture.Direction.ToString() +
                             " Shape: " + gesture.Shape;

            LogComment(message);
            
        }

        /// <summary>
        /// Write message to the log file
        /// </summary>
        /// <param name="msg"></param>
        private void Log(string msg)
        {
            if (!DebugMode) {
                const int MAX_RETRY = 10;
                const int DELAY_MS = 1000;
                bool result = false;
                int retry = 0;
                bool keepRetry = true;

                while (keepRetry && !result && retry < MAX_RETRY) {
                    try {
                        if (msg.Length > 0) {
                            testStreamWriter.WriteLine("[{0}]: {1}", DateTime.Now.ToString("HH:mm:ss"), msg);
                            testStreamWriter.Flush();
                            result = true;
                        }
                    } catch (IOException e) {
                        Console.WriteLine(e.ToString());
                        Thread.Sleep(DELAY_MS);
                        retry++;
                    } catch (Exception ex) {
                        Console.WriteLine(ex.Message);
                        keepRetry = false;
                    }
                }
            }
        }

        /// <summary>
        /// Write comment to log file
        /// </summary>
        /// <param name="comment"></param>
        public void LogComment(string comment)
        {
            if (!DebugMode) {
                try {
                    if (comment.Length > 0) {
                        if (userID != 0) {
                            string path = directory + userID + ".comment";
                            using (StreamWriter sw = new StreamWriter(path, true)) {
                                sw.WriteLine("[{0} {1}]: {2}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), comment);
                                sw.Flush();
                            }
                        }
                    }
                } catch (Exception) { } 
            }
        }

        //private static void flush()
        //{
        //    File.WriteAllText(Logger.testFilePath, string.Empty);
        //}

    }
}
