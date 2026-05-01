using System.Drawing;

class RectangleShape : Shape
{
    public override void Draw(Graphics g)
    {
        using var brush = new SolidBrush(FillColor);
        using var pen = new Pen(BorderColor, BorderWidth);

        g.FillRectangle(brush, Bounds);
        g.DrawRectangle(pen, Bounds);
    }
}