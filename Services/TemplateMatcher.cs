using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace AudioToMicWPF.Services
{
    /// <summary>
    /// 高性能纯 C# 模板匹配器（Normalized Cross Correlation / TM_CCOEFF_NORMED）
    /// 完全替代庞大的 OpenCvSharp 原生库，实现 0 外部原生依赖，体积缩减 95%+
    /// </summary>
    public static class TemplateMatcher
    {
        public struct MatchResult
        {
            public bool Success;
            public double Score;
            public Point Location;
            public Point Center;
            public int Width;
            public int Height;
        }

        public static unsafe MatchResult Match(Bitmap sourceBmp, Bitmap templateBmp, double threshold = 0.70)
        {
            MatchResult result = new MatchResult { Success = false, Score = 0 };

            int srcW = sourceBmp.Width;
            int srcH = sourceBmp.Height;
            int tplW = templateBmp.Width;
            int tplH = templateBmp.Height;

            if (tplW <= 0 || tplH <= 0 || srcW < tplW || srcH < tplH)
                return result;

            result.Width = tplW;
            result.Height = tplH;

            // 1. 提取模板并转为灰度 (32bpp ARGB -> 8bpp 灰度)
            byte[] tplGray = new byte[tplW * tplH];
            BitmapData tplData = templateBmp.LockBits(
                new Rectangle(0, 0, tplW, tplH),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            try
            {
                byte* pTpl = (byte*)tplData.Scan0;
                int stride = tplData.Stride;
                for (int y = 0; y < tplH; y++)
                {
                    byte* row = pTpl + y * stride;
                    int rowOffset = y * tplW;
                    for (int x = 0; x < tplW; x++)
                    {
                        byte b = row[x * 4];
                        byte g = row[x * 4 + 1];
                        byte r = row[x * 4 + 2];
                        // 灰度加权: (R*77 + G*150 + B*29) >> 8
                        tplGray[rowOffset + x] = (byte)((r * 77 + g * 150 + b * 29) >> 8);
                    }
                }
            }
            finally
            {
                templateBmp.UnlockBits(tplData);
            }

            // 计算模板统计量 (零均值化与范数)
            int N = tplW * tplH;
            long tplSum = 0;
            for (int i = 0; i < N; i++) tplSum += tplGray[i];
            double tplMean = (double)tplSum / N;

            float[] tplZeroMean = new float[N];
            double tplDenomSq = 0;
            for (int i = 0; i < N; i++)
            {
                float diff = (float)(tplGray[i] - tplMean);
                tplZeroMean[i] = diff;
                tplDenomSq += diff * diff;
            }

            if (tplDenomSq <= 1e-6)
                return result; // 模板无有效纹理变化

            double tplDenom = Math.Sqrt(tplDenomSq);

            // 2. 提取源图像并转为灰度
            byte[] srcGray = new byte[srcW * srcH];
            BitmapData srcData = sourceBmp.LockBits(
                new Rectangle(0, 0, srcW, srcH),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            try
            {
                byte* pSrc = (byte*)srcData.Scan0;
                int stride = srcData.Stride;
                for (int y = 0; y < srcH; y++)
                {
                    byte* row = pSrc + y * stride;
                    int rowOffset = y * srcW;
                    for (int x = 0; x < srcW; x++)
                    {
                        byte b = row[x * 4];
                        byte g = row[x * 4 + 1];
                        byte r = row[x * 4 + 2];
                        srcGray[rowOffset + x] = (byte)((r * 77 + g * 150 + b * 29) >> 8);
                    }
                }
            }
            finally
            {
                sourceBmp.UnlockBits(srcData);
            }

            // 3. 构建 2D 积分图 (Sum & Squared Sum)
            int intW = srcW + 1;
            int intH = srcH + 1;
            long[] sumTable = new long[intW * intH];
            double[] sqSumTable = new double[intW * intH];

            for (int y = 0; y < srcH; y++)
            {
                long rowSum = 0;
                double rowSqSum = 0;
                int yIdx = (y + 1) * intW;
                int prevYIdx = y * intW;
                int srcRowOffset = y * srcW;

                for (int x = 0; x < srcW; x++)
                {
                    byte val = srcGray[srcRowOffset + x];
                    rowSum += val;
                    rowSqSum += val * val;

                    sumTable[yIdx + (x + 1)] = sumTable[prevYIdx + (x + 1)] + rowSum;
                    sqSumTable[yIdx + (x + 1)] = sqSumTable[prevYIdx + (x + 1)] + rowSqSum;
                }
            }

            // 4. 多线程并行模板匹配 (NCC / CCoeffNormed)
            int outW = srcW - tplW + 1;
            int outH = srcH - tplH + 1;

            double maxScore = -1.0;
            int bestX = -1;
            int bestY = -1;
            object lockObj = new object();

            Parallel.For(0, outH, () => (-1.0, -1, -1), (y, state, localMax) =>
            {
                int yPlusH = y + tplH;
                int yIntIdx = y * intW;
                int yPlusHIntIdx = yPlusH * intW;

                for (int x = 0; x < outW; x++)
                {
                    int xPlusW = x + tplW;

                    // O(1) 积分图查询当前窗口的和与平方和
                    long sumI = sumTable[yPlusHIntIdx + xPlusW]
                              - sumTable[yIntIdx + xPlusW]
                              - sumTable[yPlusHIntIdx + x]
                              + sumTable[yIntIdx + x];

                    double sqSumI = sqSumTable[yPlusHIntIdx + xPlusW]
                                  - sqSumTable[yIntIdx + xPlusW]
                                  - sqSumTable[yPlusHIntIdx + x]
                                  + sqSumTable[yIntIdx + x];

                    double varI = sqSumI - ((double)sumI * sumI) / N;
                    if (varI <= 1e-6)
                        continue;

                    double srcDenom = Math.Sqrt(varI);

                    // 计算相关项 sum(T' * I)
                    double dot = 0;
                    for (int j = 0; j < tplH; j++)
                    {
                        int srcOffset = (y + j) * srcW + x;
                        int tplOffset = j * tplW;
                        for (int i = 0; i < tplW; i++)
                        {
                            dot += tplZeroMean[tplOffset + i] * srcGray[srcOffset + i];
                        }
                    }

                    double score = dot / (tplDenom * srcDenom);
                    if (score > localMax.Item1)
                    {
                        localMax = (score, x, y);
                    }
                }
                return localMax;
            },
            localMax =>
            {
                lock (lockObj)
                {
                    if (localMax.Item1 > maxScore)
                    {
                        maxScore = localMax.Item1;
                        bestX = localMax.Item2;
                        bestY = localMax.Item3;
                    }
                }
            });

            result.Score = maxScore;
            if (bestX >= 0 && bestY >= 0)
            {
                result.Location = new Point(bestX, bestY);
                result.Center = new Point(bestX + tplW / 2, bestY + tplH / 2);
                result.Success = maxScore >= threshold;
            }

            return result;
        }
    }
}
