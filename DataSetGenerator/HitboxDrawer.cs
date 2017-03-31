using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace DataSetGenerator {
    class HitboxDrawer {
        private static void DrawHitBox(List<Attempt> attempts, string fileName) {

            //61 pixel sized squares, makes it better to look at
            int cellSize = 61;
            int bmsize = cellSize * 3;

            Bitmap hitbox = new Bitmap(bmsize, bmsize);
            Graphics hBGraphic = Graphics.FromImage(hitbox);
            hBGraphic.FillRectangle(Brushes.White, 0, 0, bmsize, bmsize);
            hBGraphic.DrawRectangle(new Pen(Brushes.Black, 1.0f), cellSize, cellSize, cellSize, cellSize);

            foreach (var attempt in attempts) {
                Brush brush = Brushes.Red;
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

                if ((p.X > cellSize && p.X < cellSize * 2) && (p.Y > cellSize && p.Y < cellSize * 2)) {
                    brush = Brushes.Green;
                }

                if (!((p.X < 0) && (p.X >= bmsize)) || !((p.Y < 0) && (p.Y >= bmsize))) {
                    hBGraphic.FillRectangle(brush, (float)p.X, (float)p.Y, 2, 2);
                }
            }

            hBGraphic.Save();

            hitbox.Save(DataGenerator.DataDirectory + fileName);

            hBGraphic.Dispose();
            hitbox.Dispose();

        }

        public static void CreateHitboxes(DataSource source) {
            var tests = DataGenerator.GetTests(source);
            if (tests.Count == 0) return;
            Dictionary<GestureType, List<Attempt>> techAttempts = new Dictionary<GestureType, List<Attempt>>();
            Dictionary<GridSize, List<Attempt>> sizeAttempts = new Dictionary<GridSize, List<Attempt>>();
            sizeAttempts.Add(GridSize.Large, new List<Attempt>());
            sizeAttempts.Add(GridSize.Small, new List<Attempt>());
            foreach (var test in tests) {
                foreach (var gesture in DataGenerator.AllTechniques) {
                    if (!techAttempts.ContainsKey(gesture)) {
                        techAttempts.Add(gesture, new List<Attempt>());
                    }
                    techAttempts[gesture].AddRange(test.Attempts[gesture]);
                    sizeAttempts[GridSize.Small].AddRange(from attempt in test.Attempts[gesture]
                                                          where attempt.Size == GridSize.Small
                                                          select attempt);
                    sizeAttempts[GridSize.Large].AddRange(from attempt in test.Attempts[gesture]
                                                          where attempt.Size == GridSize.Large
                                                          select attempt);
                }
            }

            DrawHitBox(techAttempts[GestureType.Pinch], "pinch.png");
            DrawHitBox(techAttempts[GestureType.Swipe], "swipe.png");
            DrawHitBox(techAttempts[GestureType.Throw], "throw.png");
            DrawHitBox(techAttempts[GestureType.Tilt], "tilt.png");

            DrawHitBox(sizeAttempts[GridSize.Large], "large.png");
            DrawHitBox(sizeAttempts[GridSize.Small], "small.png");

            var lp = from attempt in techAttempts[GestureType.Pinch]
                     where attempt.Size == GridSize.Large
                     select attempt;
            DrawHitBox(lp.ToList(), "pinchlarge.png");
            var sp = from attempt in techAttempts[GestureType.Pinch]
                     where attempt.Size == GridSize.Small
                     select attempt;
            DrawHitBox(sp.ToList(), "pinchsmall.png");

            var ls = from attempt in techAttempts[GestureType.Swipe]
                     where attempt.Size == GridSize.Large
                     select attempt;
            DrawHitBox(ls.ToList(), "swipelarge.png");
            var ss = from attempt in techAttempts[GestureType.Swipe]
                     where attempt.Size == GridSize.Small
                     select attempt;
            DrawHitBox(ss.ToList(), "swipesmall.png");

            var lti = from attempt in techAttempts[GestureType.Tilt]
                      where attempt.Size == GridSize.Large
                      select attempt;
            DrawHitBox(lti.ToList(), "tiltlarge.png");
            var sti = from attempt in techAttempts[GestureType.Tilt]
                      where attempt.Size == GridSize.Small
                      select attempt;
            DrawHitBox(sti.ToList(), "tiltsmall.png");

            var lth = from attempt in techAttempts[GestureType.Throw]
                      where attempt.Size == GridSize.Large
                      select attempt;
            DrawHitBox(lth.ToList(), "throwlarge.png");
            var sth = from attempt in techAttempts[GestureType.Throw]
                      where attempt.Size == GridSize.Small
                      select attempt;
            DrawHitBox(sth.ToList(), "throwsmall.png");

        }

    }
}
