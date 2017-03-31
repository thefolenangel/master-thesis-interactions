using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SW9_Project.Logging;
using System.Windows;
using DataSetGenerator;

using Point = System.Windows.Point;

namespace SW9_Project {
    class TestSuite {

        private static int sgHeight, sgWidth, lgHeight, lgWidth;
        private static double canvasHeight, canvasWidth;

        public static void Intialize(int sHeight, int sWidth, int lHeight, int lWidth, double cnvasHeight, double cnvasWidth) {
            sgHeight = sHeight;
            sgWidth = sWidth;
            lgHeight = lHeight;
            lgWidth = lWidth;
            canvasHeight = cnvasHeight;
            canvasWidth = cnvasWidth;
        }

        IDrawingBoard board;
        DataSource source;

        public TestSuite(IDrawingBoard board, DataSource source) {
            this.source = source;
            this.board = board;
            UserID = Logger.CurrentLogger.NewUser();
        }

        public int UserID { get; }

        bool firstDirectionRun = false, done = false;

        public void StartDebugTest(GestureType type) {
            gestureTypeList = new Queue<GestureType>();
            gestureTypeList.Enqueue(type);
            ChangeGesture();
        }

        public void StartTest(GestureDirection direction) {
            GestureParser.SetDirectionContext(direction);
            gestureTypeList = GetRandomGestureList();
            ChangeGesture();
        }

        Queue<Target> targetSequence = new Queue<Target>();
        Queue<Target> practiceSequence = new Queue<Target>();
        bool practiceDone = false;

        public void TargetHit(bool hit, bool correctShape, Cell target, Point pointer, Cell pointerCell, JumpLength length) {
            if(practiceDone) {
                Logger.CurrentLogger.CurrentTargetHit(hit, target, pointer, pointerCell, correctShape, length);
            }
            if (practiceSequence.Count != 0) {
                board.CreateTarget(practiceSequence.Dequeue());
                return;
            }
            else if (!practiceDone){
                practiceDone = true;
                Logger.CurrentLogger.StartNewgestureTest(GestureParser.GetTypeContext(), GestureParser.GetDirectionContext());
                board.PracticeDone();
            }

            if(targetSequence.Count != 0) {
                board.CreateTarget(targetSequence.Dequeue());
            }
            else {
                board.CurrentGestureDone();
                if (gestureTypeList.Count == 0 && done) {
                    Finish();
                }
                else if (gestureTypeList.Count == 0) {
                    if(!firstDirectionRun) { firstDirectionRun = true; }
                }
            }
        }

        private void Finish() {
            Logger.CurrentLogger.EndUser();
            Test currentTest = new Test(UserID, source);
            AttemptRepository.SaveTestToDatabase(currentTest);
            board.EndTest();
        }
        
        public bool ChangeGesture() {
            if(gestureTypeList.Count == 0 && firstDirectionRun) {
                GestureDirection direction = GestureParser.GetDirectionContext() == GestureDirection.Pull ? GestureDirection.Push : GestureDirection.Pull;
                StartTest(direction);
                done = true;
                return true;
            }
            practiceDone = false;
            targetSequence = Target.GetNextSequence();
            practiceSequence = Target.GetPracticeTargets();
            
            practiceSequence.Enqueue(targetSequence.Dequeue());

            board.Clear();
            board.StartNewGesture();
            board.CreateTarget(practiceSequence.Dequeue());
            GestureParser.SetTypeContext(gestureTypeList.Dequeue());

            Logger.CurrentLogger.StartPracticeTime(GestureParser.GetTypeContext(), GestureParser.GetDirectionContext());
            VideoWindow.PlayVideo(GestureParser.GetDirectionContext(), GestureParser.GetTypeContext());
            Console.WriteLine($"Changed to gesture: {GestureParser.GetTypeContext()} {GestureParser.GetDirectionContext()}");
            return true;
        }

        Queue<GestureType> gestureTypeList;

        private Queue<GestureType> GetRandomGestureList() {
            
            List<GestureType> types = new List<GestureType> { GestureType.Pinch , GestureType.Swipe,  GestureType.Throw, GestureType.Tilt };
            types.Shuffle();
            return new Queue<GestureType>(types);
        }
    }
}
