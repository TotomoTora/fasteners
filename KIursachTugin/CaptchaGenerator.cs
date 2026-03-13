using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace KIursachTugin
{
    public static class CaptchaGenerator
    {
        private static Random rand = new Random();
        public static string CurrentCaptcha { get; private set; }

        /// <summary>
        /// Генерация CAPTCHA
        /// </summary>
        /// <param name="width">Ширина изображения</param>
        /// <param name="height">Высота изображения</param>
        /// <param name="charCount">Количество символов в CAPTCHA</param>
        /// <returns>Bitmap с CAPTCHA</returns>
        public static Bitmap GenerateCaptcha(int width = 150, int height = 50, int charCount = 4)
        {
            Bitmap bmp = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);

            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            char[] captchaChars = new char[charCount];
            for (int i = 0; i < charCount; i++)
                captchaChars[i] = chars[rand.Next(chars.Length)];
            CurrentCaptcha = new string(captchaChars);

            // Расстояние между символами
            float spacing = width / (float)(charCount + 1);

            for (int i = 0; i < charCount; i++)
            {
                // Подбираем размер шрифта по высоте и ширине ячейки
                float maxFontHeight = height - 10;
                float maxFontWidth = spacing;
                float fontSize = Math.Min(maxFontHeight, maxFontWidth);

                Font font = new Font("Arial", fontSize, FontStyle.Bold);
                Brush brush = new SolidBrush(Color.FromArgb(rand.Next(50, 160), rand.Next(50, 160), rand.Next(50, 160)));

                // Координаты символа с небольшим смещением
                float x = spacing * (i + 0.5f) - fontSize / 2 + rand.Next(-3, 3);
                float y = (height - fontSize) / 2 + rand.Next(-3, 3);

                // Поворот символа, чтобы был слегка наклонен
                GraphicsState state = g.Save();
                g.TranslateTransform(x + fontSize / 2, y + fontSize / 2); // центр поворота
                g.RotateTransform(rand.Next(-25, 25));
                g.DrawString(captchaChars[i].ToString(), font, brush, -fontSize / 2, -fontSize / 2);
                g.Restore(state);
            }

            // Добавляем линии и шум
            for (int i = 0; i < charCount * 2; i++)
            {
                Pen pen = new Pen(Color.FromArgb(rand.Next(100, 200), rand.Next(100, 200), rand.Next(100, 200)), 1);
                int x1 = rand.Next(width);
                int y1 = rand.Next(height);
                int x2 = rand.Next(width);
                int y2 = rand.Next(height);
                g.DrawLine(pen, x1, y1, x2, y2);
            }

            // Точки-шум
            for (int i = 0; i < width * height / 50; i++)
            {
                int x = rand.Next(width);
                int y = rand.Next(height);
                bmp.SetPixel(x, y, Color.FromArgb(rand.Next(100, 255), rand.Next(100, 255), rand.Next(100, 255)));
            }

            return bmp;
        }
    }
}