using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace WinAutoDarkMode.Core
{
    public static class IconGenerator
    {
        // Lucide Sun Data
        private static readonly string SunPath = "M12,12 m-4,0 a4,4 0 1,0 8,0 a4,4 0 1,0 -8,0 M12,2 v2 M12,20 v2 M4.93,4.93 l1.41,1.41 M17.66,17.66 l1.41,1.41 M2,12 h2 M20,12 h2 M6.34,17.66 l-1.41,1.41 M19.07,4.93 l-1.41,1.41";
        
        // Lucide Moon Data
        private static readonly string MoonPath = "M12,3 a9,9 0 1,0 9,9 a9,9 0 0,0 -9,-9 Z";

        public static Icon CreateThemeIcon(bool isDark)
        {
            using var bitmap = new Bitmap(32, 32);
            using var g = Graphics.FromImage(bitmap);
            
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);

            // 根据模式选择颜色：深色模式用白色图标，浅色模式用黑色/深蓝图标
            var color = isDark ? Color.White : Color.FromArgb(32, 32, 32);
            using var pen = new Pen(color, 2.5f);
            pen.StartCap = LineCap.Round;
            pen.EndCap = LineCap.Round;

            if (isDark)
            {
                // 绘制月亮
                DrawMoon(g, pen);
            }
            else
            {
                // 绘制太阳
                DrawSun(g, pen);
            }

            return Icon.FromHandle(bitmap.GetHicon());
        }

        private static void DrawSun(Graphics g, Pen pen)
        {
            // 简单化处理：直接用 API 绘制，因为 Path Data 解析太复杂
            g.DrawEllipse(pen, 10, 10, 12, 12); // 中心圆
            
            // 绘制射线
            float length = 4;
            float inner = 8;
            for (int i = 0; i < 8; i++)
            {
                double angle = i * Math.PI / 4;
                float x1 = 16 + (float)(Math.Cos(angle) * inner);
                float y1 = 16 + (float)(Math.Sin(angle) * inner);
                float x2 = 16 + (float)(Math.Cos(angle) * (inner + length));
                float y2 = 16 + (float)(Math.Sin(angle) * (inner + length));
                g.DrawLine(pen, x1, y1, x2, y2);
            }
        }

        private static void DrawMoon(Graphics g, Pen pen)
        {
            // 使用弧线模拟 Lucide Moon
            var path = new GraphicsPath();
            path.AddArc(6, 6, 20, 20, -10, 290);
            path.AddArc(11, 8, 16, 16, 260, -240);
            path.CloseFigure();
            g.DrawPath(pen, path);
        }
    }
}
