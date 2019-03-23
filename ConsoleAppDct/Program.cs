using Accord.Imaging.Converters;
using Accord.Math;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace ConsoleAppDct
{
    class Program
    {
        public static int[,] Q50 =
        {
            { 16,  11,  10,  16,  24,  40,  51,  61 },
            { 12,  12,  14,  19,  26,  58,  60,  55 },
            { 14,  13,  16,  24,  40,  57,  69,  56 },
            { 14,  17,  22,  29,  51,  87,  80,  62 },
            { 18,  22,  37,  56,  68, 109, 103,  77 },
            { 24,  35,  55,  64,  81, 104, 113,  92 },
            { 49,  64,  78,  87, 103, 121, 120, 101 },
            { 72,  92,  95,  98, 112, 100, 103,  99 }
        };

        public static byte[] Standard_Chromiance_Quantization_Table =
        {
            17,  18,  24,  47,  99,  99,  99,  99,
            18,  21,  26,  66,  99,  99,  99,  99,
            24,  26,  56,  99,  99,  99,  99,  99,
            47,  66,  99,  99,  99,  99,  99,  99,
            99,  99,  99,  99,  99,  99,  99,  99,
            99,  99,  99,  99,  99,  99,  99,  99,
            99,  99,  99,  99,  99,  99,  99,  99,
            99,  99,  99,  99,  99,  99,  99,  99
        };

        private static double[,] GetDctMatrix()
        {
            int len = 8;
            double[,] result = new double[len, len];
            for (int i = 0; i < len; i++)
            {
                if (i == 0)
                {
                    for (int j = 0; j < len; j++)
                    {
                        result[i, j] = 1d / Math.Sqrt(len);
                    }
                }
                else
                {
                    for (int j = 0; j < len; j++)
                    {
                        result[i, j] = Math.Sqrt(2d / len) * Math.Cos((2 * j + 1) * i * Math.PI / (2d * len));
                    }
                }
            }

            return result;
        }

        static void Main(string[] args)
        {
            var qualityLevels = new Dictionary<int, int[,]>
            {
                { 50, Q50 }
            };

            foreach (int level in new[] { 10, 20, 30, 40 })
            {
                int[,] newQ = new int[8, 8];
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        newQ[i, j] = (int)Math.Min(Q50[i, j] * (50d / level), 256d);
                    }
                }

                qualityLevels.Add(level, newQ);
            }
            foreach (int level in new[] { 60, 70, 80, 90, 95 })
            {
                int[,] newQ = new int[8, 8];
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        newQ[i, j] = (int)Math.Min(Q50[i, j] * ((100d - level) / 50d), 256d);
                    }
                }

                qualityLevels.Add(level, newQ);
            }

            var imageToMatrix = new ImageToMatrix();

            var bitmap = new Bitmap(File.OpenRead("sample_blackwhite.bmp"));

            double[,] output;
            imageToMatrix.Convert(bitmap, out output);

            var dct = GetDctMatrix();

            var matrixMultiplied = Matrix.Dot(output, dct);

            foreach (var q in qualityLevels)
            {
                double[,] rounded = new double[8, 8];
                for (int row = 0; row < 8; row++)
                {
                    for (int column = 0; column < 8; column++)
                    {
                        int divided = (int)Math.Round(matrixMultiplied[row, column] * 256d / q.Value[row, column], 0);
                        int multipliedAgain = divided * q.Value[row, column];

                        rounded[row, column] = multipliedAgain / 256d;
                    }
                }

                var newMatrix = Matrix.Divide(rounded, dct);

                var mtoi = new MatrixToImage();

                Bitmap bitmap2;
                mtoi.Convert(newMatrix, out bitmap2);
                bitmap2.Save($"sample_blackwhite_q{q.Key}.bmp");
            }
        }
    }
}

/*
RGB to Y Cb Cr

Y  =  0.2989 R + 0.5866 G + 0.1145 B
Cb = -0.1687 R - 0.3312 G + 0.5000 B
Cr =  0.5000 R - 0.4183 G - 0.0816 B
 */
