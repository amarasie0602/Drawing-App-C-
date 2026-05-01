using System.Drawing;

class CircleShape : Shape
{
    public override void Draw(Graphics g)
    {
        using var brush = new SolidBrush(FillColor);
        using var pen = new Pen(BorderColor, BorderWidth);

        g.FillEllipse(brush, Bounds);
        g.DrawEllipse(pen, Bounds);
    }
}