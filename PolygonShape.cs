using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

public class PolygonShape : Shape
{
    // Using lists so we can keep adding points while the user is drawing
    public List<int> PX { get; set; } = new List<int>();
    public List<int> PY { get; set; } = new List<int>();

    public Point[] GetPoints()
    {
        Point[] pts = new Point[PX.Count];
        for (int i = 0; i < PX.Count; i++)
            pts[i] = new Point(PX[i], PY[i]);
        return pts;
    }

    public void AddPoint(Point p)
    {
        PX.Add(p.X);
        PY.Add(p.Y);
    }

    public override Rectangle Bounds
    {
        get
        {
            if (PX.Count == 0) return Rectangle.Empty;

            int minX = PX[0], maxX = PX[0];
            int minY = PY[0], maxY = PY[0];

            for (int i = 1; i < PX.Count; i++)
            {
                if (PX[i] < minX) minX = PX[i];
                if (PX[i] > maxX) maxX = PX[i];
                if (PY[i] < minY) minY = PY[i];
                if (PY[i] > maxY) maxY = PY[i];
            }

            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }
    }

    public override void Draw(Graphics g)
    {
        Point[] pts = GetPoints();
        if (pts.Length < 2) return;

        SolidBrush brush = new SolidBrush(FillColor);
        Pen pen = new Pen(BorderColor, BorderWidth);

        if (pts.Length >= 3)
            g.FillPolygon(brush, pts);

        g.DrawPolygon(pen, pts);

        brush.Dispose();
        pen.Dispose();
    }

    // Ray casting algorithm to check if a point is inside the polygon
    public override bool HitTest(Point p)
    {
        Point[] pts = GetPoints();
        if (pts.Length < 3) return false;

        bool inside = false;
        int j = pts.Length - 1;

        for (int i = 0; i < pts.Length; i++)
        {
            if ((pts[i].Y > p.Y) != (pts[j].Y > p.Y))
            {
                double intersectX = (pts[j].X - pts[i].X) * (double)(p.Y - pts[i].Y)
                                  / (pts[j].Y - pts[i].Y) + pts[i].X;

                if (p.X < intersectX)
                    inside = !inside;
            }
            j = i;
        }

        return inside;
    }

    public override void Move(int dx, int dy)
    {
        for (int i = 0; i < PX.Count; i++)
        {
            PX[i] += dx;
            PY[i] += dy;
        }
    }

    // Each vertex is its own handle
    public override List<ShapeHandle> GetHandles()
    {
        Point[] pts = GetPoints();
        List<ShapeHandle> handles = new List<ShapeHandle>();
        for (int i = 0; i < pts.Length; i++)
            handles.Add(new ShapeHandle(i, pts[i], Cursors.SizeAll));
        return handles;
    }

    public override Point[] GetHandlePoints()
    {
        return GetPoints();
    }

    public override void ApplyHandle(int index, Point newPoint, Point[] originalPoints)
    {
        if (index >= 0 && index < PX.Count)
        {
            PX[index] = newPoint.X;
            PY[index] = newPoint.Y;
        }
    }

    public override object SaveGeometry()
    {
        return (new List<int>(PX), new List<int>(PY));
    }

    public override void RestoreGeometry(object savedState)
    {
        var state = ((List<int>, List<int>))savedState;
        PX = new List<int>(state.Item1);
        PY = new List<int>(state.Item2);
    }
}