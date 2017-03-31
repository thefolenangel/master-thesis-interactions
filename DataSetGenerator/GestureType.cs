using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSetGenerator {
    public enum GestureType {
        Pinch, Swipe, Throw, Tilt
    }
    public enum GestureDirection {
        Push, Pull
    }
    public enum JumpLength {
        Short, Medium, Long, NA
    }
    public enum GridSize {
        Small, Large
    }

    public enum DatabaseSaveStatus {
        Saving, Success, Failed
    }

    public enum DataSource {
        Target, Field, Old
    }
}
