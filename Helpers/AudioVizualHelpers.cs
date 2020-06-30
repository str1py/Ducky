using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Ducky.Helpers
{
    class AudioVizualHelpers
    {
        private readonly double selectedBuffer = 0.3;
        private readonly int blurMult = 7;
        private readonly int shadowMult = 90;
        private readonly int imageMult = 15;

        //item1 - BlurRadius , item2 - ShadowRadius, item3 - colorName
        public (double, double,  double) AudioVizual(float[] buffer, double BlurRadius, double ShadowRadius, Visibility MinimalMusicGridVisibility, double ImageSize, WindowState state)
        {
            float valueFromBuf = buffer[1];
            if (MinimalMusicGridVisibility == Visibility.Visible)
            {
                if (state == WindowState.Normal && ImageSize < 440)
                    ImageSize = 440;
                else if (state == WindowState.Maximized && ImageSize < 600)
                    ImageSize = 600;

                if (state == WindowState.Normal)
                {
                    if (valueFromBuf > selectedBuffer)
                        ImageSize =  440 + Math.Round(valueFromBuf, 2) * imageMult;
                    else if (ImageSize > 440)
                        ImageSize -=0.5;
                }
                else
                {
                    if (valueFromBuf > selectedBuffer)
                        ImageSize = 600 + Math.Round(valueFromBuf, 2) * imageMult;
                    else if (ImageSize > 600)
                        ImageSize -=1;
                }
            }

            if (valueFromBuf > selectedBuffer )
                return (Math.Round(valueFromBuf, 2) * blurMult, Math.Round(valueFromBuf, 2) * shadowMult, ImageSize);
            else
                return (BlurRadius -= 0.5, ShadowRadius -= 0.5, ImageSize);
        }

        public string GetShadowColor(BitmapImage img)
        {
            Bitmap theBitMap; 
            var MostUsedColor = Color.Empty;
            var dctColorIncidence = new Dictionary<int, int>();
            var TenMostUsedColors = new List<Color>();
            var TenMostUsedColorIncidences = new List<int>();
            int pixelColor;

            using (var outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(img));
                enc.Save(outStream);
                theBitMap = new Bitmap(outStream);
            }

            for (int row = 0; row < theBitMap.Size.Width; row++)
            {
                for (int col = 0; col < theBitMap.Size.Height; col++)
                {
                    pixelColor = theBitMap.GetPixel(row, col).ToArgb();

                    try
                    {
                        if (dctColorIncidence.Keys.Contains(pixelColor) == true)
                            dctColorIncidence[pixelColor]++;
                        else if (dctColorIncidence.Keys.Contains(pixelColor) == false)
                            dctColorIncidence.Add(pixelColor, 1);
                    }
                    catch { }
                }
            }

            var dctSortedByValueHighToLow = dctColorIncidence.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            foreach (KeyValuePair<int, int> kvp in dctSortedByValueHighToLow.Take(10))
            {
                TenMostUsedColors.Add(Color.FromArgb(kvp.Key));
                TenMostUsedColorIncidences.Add(kvp.Value);
            }

            MostUsedColor = Color.FromArgb(dctSortedByValueHighToLow.First().Key);
            return "#" + MostUsedColor.Name;
        }
    }
}
