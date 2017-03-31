namespace DataSetGenerator {
    public class Point {
        public double X { get; set; }
        public double Y { get; set; }

        public override string ToString() {
            return $"({X},{Y})";
        }

        public Point(double x, double y) {
            X = x;
            Y = y;
        }

        public Point() { }
    }
}