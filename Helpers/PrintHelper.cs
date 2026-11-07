using System.Drawing.Printing;
using CafeManager.Models;

namespace CafeManager.Helpers;

public static class PrintHelper
{
    public static void PrintBill(string tableName, List<BillItem> items, decimal subtotal,
                                 int discount, decimal total, string cashier)
    {
        var doc = new PrintDocument();
        doc.DefaultPageSettings.PaperSize = new PaperSize("Bill80", 315, 800); // ~80mm
        doc.PrintPage += (s, e) =>
        {
            var g = e.Graphics;
            using var fTitle = new Font("Segoe UI", 14, FontStyle.Bold);
            using var fBold = new Font("Segoe UI", 9, FontStyle.Bold);
            using var fText = new Font("Segoe UI", 9);
            float x = 10, y = 10, w = 295;
            var center = new StringFormat { Alignment = StringAlignment.Center };
            var right = new StringFormat { Alignment = StringAlignment.Far };

            g.DrawString("QUÁN CÀ PHÊ", fTitle, Brushes.Black, new RectangleF(x, y, w, 28), center); y += 30;
            g.DrawString("HÓA ĐƠN THANH TOÁN", fBold, Brushes.Black, new RectangleF(x, y, w, 20), center); y += 24;
            g.DrawString($"{tableName}    {DateTime.Now:dd/MM/yyyy HH:mm}", fText, Brushes.Black, x, y); y += 18;
            g.DrawString($"Thu ngân: {cashier}", fText, Brushes.Black, x, y); y += 22;
            g.DrawLine(Pens.Black, x, y, x + w, y); y += 4;

            g.DrawString("Món", fBold, Brushes.Black, x, y);
            g.DrawString("SL", fBold, Brushes.Black, x + 150, y);
            g.DrawString("T.Tiền", fBold, Brushes.Black, new RectangleF(x + 180, y, w - 180, 18), right); y += 20;

            foreach (var it in items)
            {
                g.DrawString(it.FoodName, fText, Brushes.Black, new RectangleF(x, y, 145, 18));
                g.DrawString(it.Quantity.ToString(), fText, Brushes.Black, x + 150, y);
                g.DrawString(it.Total.ToString("N0"), fText, Brushes.Black, new RectangleF(x + 180, y, w - 180, 18), right);
                y += 18;
            }
            y += 4; g.DrawLine(Pens.Black, x, y, x + w, y); y += 6;

            g.DrawString("Tạm tính:", fText, Brushes.Black, x, y);
            g.DrawString(subtotal.ToString("N0"), fText, Brushes.Black, new RectangleF(x, y, w, 18), right); y += 18;
            g.DrawString($"Giảm giá ({discount}%):", fText, Brushes.Black, x, y);
            g.DrawString("-" + (subtotal - total).ToString("N0"), fText, Brushes.Black, new RectangleF(x, y, w, 18), right); y += 20;
            g.DrawString("TỔNG CỘNG:", fBold, Brushes.Black, x, y);
            g.DrawString(total.ToString("N0") + " đ", fBold, Brushes.Black, new RectangleF(x, y, w, 18), right); y += 34;
            g.DrawString("Cảm ơn quý khách – Hẹn gặp lại!", fText, Brushes.Black, new RectangleF(x, y, w, 20), center);
        };

        using var preview = new PrintPreviewDialog { Document = doc, Width = 500, Height = 700 };
        preview.ShowDialog();   // trong cửa sổ preview có nút In
    }
}
