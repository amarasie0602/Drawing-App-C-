using System;
using System.Drawing;

public class CircleShape : RectangleShape
{
    public override void Draw(Graphics g)
    {
        if (W < 1 || H < 1) return;

        SolidBrush brush = new SolidBrush(FillColor);
        Pen pen = new Pen(BorderColor, BorderWidth);

        g.FillEllipse(brush, Bounds);
        g.DrawEllipse(pen, Bounds);

        brush.Dispose();
        pen.Dispose();
    }

    // Check if the point is inside the ellipse, not just the bounding box
    public override bool HitTest(Point p)
    {
        if (W < 1 || H < 1) return false;

        double rx = W / 2.0;
        double ry = H / 2.0;
        double centerX = X + rx;
        double centerY = Y + ry;

        double dx = (p.X - centerX) / rx;
        double dy = (p.Y - centerY) / ry;

        return (dx * dx + dy * dy) <= 1.0;
    }
}