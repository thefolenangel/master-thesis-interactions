using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using DataSetGenerator;

using Point = System.Windows.Point;


namespace SW9_Project {
    interface IDrawingBoard {
        void PointAt(double xFromMid, double yFromMid);
        Point GetPoint(double xFromMid, double yFromMid);
        Cell GetCell(Point p);
        void CreateTarget(Target target);
        void CreateGrid(GridSize size);
        void Clear();
        void CurrentGestureDone();
        void StartNewGesture();
        void PracticeDone();
        void EndTest();
        void LockPointer();
        void UnlockPointer();
        void ResetGyro();
        void LockScreen(GestureType type, GestureDirection direction);
        void UnlockScreen();
    }
}
