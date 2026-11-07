using System.Drawing.Drawing2D;

namespace CafeManager.Forms;

// Biểu đồ cột doanh thu tự vẽ bằng GDI+ (không dùng thư viện ngoài)
public class ChartPanel : Control
{
    List<(string Label, decimal Value)> _data = new();

    public ChartPanel()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Color.White;
    }

    public void SetData(List<(string, decimal)> data)
    {
        _data = data.Select(d => (d.Item1, d.Item2)).ToList();
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(BackColor);

        using var fTitle = new Font("Segoe UI", 11, FontStyle.Bold);
        using var fSmall = new Font("Segoe UI", 8);
        g.DrawString("Biểu đồ doanh thu theo ngày", fTitle, Brushes.Black, 10, 6);

        if (_data.Count == 0)
        {
            g.DrawString("Không có dữ liệu", fSmall, Brushes.Gray, 20, 40);
            return;
        }

        // Vùng vẽ
        int left = 80, right = 20, top = 40, bottom = 35;
        var plot = new Rectangle(left, top, Width - left - right, Height - top - bottom);
        if (plot.Width < 50 || plot.Height < 50) return;

        // Làm tròn trục Y lên "đẹp"
        double max = (double)_data.Max(d => d.Value);
        if (max <= 0) max = 1;
        double step = Math.Pow(10, Math.Floor(Math.Log10(max)));
        double niceMax = Math.Ceiling(max / step) * step;

        // Lưới ngang + nhãn trục Y
        int lines = 5;
        using var gridPen = new Pen(Color.Gainsboro);
        var rightAlign = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
        for (int i = 0; i <= lines; i++)
        {
            float y = plot.Bottom - (float)i / lines * plot.Height;
            g.DrawLine(gridPen, plot.Left, y, plot.Right, y);
            g.DrawString((niceMax * i / lines).ToString("N0"), fSmall, Brushes.DimGray,
                         new RectangleF(0, y - 10, left - 6, 20), rightAlign);
        }

        // Trục
        g.DrawLine(Pens.Black, plot.Left, plot.Top, plot.Left, plot.Bottom);
        g.DrawLine(Pens.Black, plot.Left, plot.Bottom, plot.Right, plot.Bottom);

        // Các cột
        float slot = (float)plot.Width / _data.Count;
        float barW = Math.Min(slot * 0.6f, 80f);
        var center = new StringFormat { Alignment = StringAlignment.Center };

        for (int i = 0; i < _data.Count; i++)
        {
            float h = (float)((double)_data[i].Value / niceMax * plot.Height);
            float x = plot.Left + i * slot + (slot - barW) / 2;
            var bar = new RectangleF(x, plot.Bottom - h, barW, h);

            using (var brush = new LinearGradientBrush(bar.Height > 0 ? bar : new RectangleF(x, plot.Bottom - 1, barW, 1),
                       Color.CornflowerBlue, Color.SteelBlue, LinearGradientMode.Vertical))
                g.FillRectangle(brush, bar);
            g.DrawRectangle(Pens.SteelBlue, bar.X, bar.Y, bar.Width, bar.Height);

            // giá trị trên đầu cột + nhãn ngày dưới trục
            g.DrawString(_data[i].Value.ToString("N0"), fSmall, Brushes.Black,
                         new RectangleF(x - 20, bar.Y - 16, barW + 40, 14), center);
            g.DrawString(_data[i].Label, fSmall, Brushes.Black,
                         new RectangleF(x - 10, plot.Bottom + 4, barW + 20, 16), center);
        }
    }
}
