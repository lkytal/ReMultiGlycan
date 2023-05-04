using System;
using System.Collections.Generic;
using System.Text;

namespace COL.ElutionViewer
{
    public class BSpline
    {
        private float[,] MSpointSet3DToArray(MSPointSet3D mspointset, RegionSize PaintRegion)
        {
            RegionSize pointSetRegion = new RegionSize(mspointset.MinX, mspointset.MaxX, mspointset.MaxY, mspointset.MinY);

            float[,] Data = new float[(int)PaintRegion.Width + 1, (int)PaintRegion.Height + 1];
            int[,] DataCount = new int[(int)PaintRegion.Width + 1, (int)PaintRegion.Height + 1];
            for (int i = 0; i < (int)PaintRegion.Width + 1; i++)
            {
                for (int j = 0; j < (int)PaintRegion.Height + 1; j++)
                {
                    Data[i, j] = -1;
                    DataCount[i, j] = 0;
                }
            }

            //foreach (MSPoint3D point3D in mspointset.Points3D)
            for (int i = 0; i < mspointset.Count; i++)
            {
                if (DataCount[(int)Math.Round(CoordinateTrans.TimeToX(pointSetRegion, PaintRegion, mspointset.X(i))), (int)Math.Round(CoordinateTrans.MassToY(pointSetRegion, PaintRegion, mspointset.Y(i)))] == 0)
                {
                    Data[(int)Math.Round(CoordinateTrans.TimeToX(pointSetRegion, PaintRegion, mspointset.X(i))), (int)Math.Round(CoordinateTrans.MassToY(pointSetRegion, PaintRegion, mspointset.Y(i)))] = mspointset.Z(i);
                    DataCount[(int)Math.Round(CoordinateTrans.TimeToX(pointSetRegion, PaintRegion, mspointset.X(i))), (int)Math.Round(CoordinateTrans.MassToY(pointSetRegion, PaintRegion, mspointset.Y(i)))]++;
                }
                else
                {
                    Data[(int)Math.Round(CoordinateTrans.TimeToX(pointSetRegion, PaintRegion, mspointset.X(i))), (int)Math.Round(CoordinateTrans.MassToY(pointSetRegion, PaintRegion, mspointset.Y(i)))] += mspointset.Z(i);
                    DataCount[(int)Math.Round(CoordinateTrans.TimeToX(pointSetRegion, PaintRegion, mspointset.X(i))), (int)Math.Round(CoordinateTrans.MassToY(pointSetRegion, PaintRegion, mspointset.Y(i)))]++;
                }
            }
            for (int i = 0; i < (int)PaintRegion.Width + 1; i++)
            {
                for (int j = 0; j < (int)PaintRegion.Height + 1; j++)
                {
                    if (Data[i, j] != -1)
                    {
                        Data[i, j] = Data[i, j] / DataCount[i, j];
                    }
                }
            }
            return Data;
        }

        public MSPointSet3D BSpline3DofPepImage(MSPointSet3D mspointset, RegionSize PaintRegion)
        {
            MSPointSet3D interpolatedPointSet3D = new MSPointSet3D();
            RegionSize pointSetRegion = new RegionSize(mspointset.MinX, mspointset.MaxX, mspointset.MaxY, mspointset.MinY);

            if (pointSetRegion.Width * pointSetRegion.Height * PaintRegion.Width * PaintRegion.Height == 0)
            {
                return mspointset;
            }
            float[,] Data;
            Data = MSpointSet3DToArray(mspointset, PaintRegion);

            for (int i = 0; i < (int)PaintRegion.Width + 1; i++)
            {
                MSPointSet pointset2D = new MSPointSet();
                float MaxIntensity = -1;
                bool empty = true;

                for (int j = 0; j < (int)PaintRegion.Height + 1; j++)
                {
                    if (Data[i, j] != -1)
                    {
                        empty = false;
                        if (Data[i, j] > MaxIntensity)
                        {
                            MaxIntensity = Data[i, j];
                        }
                    }
                }

                if (empty == false)
                {
                    for (int j = 0; j < (int)PaintRegion.Height + 1; j++)
                    {
                        if (Data[i, j] == -1)
                        {
                            Data[i, j] = 0;
                        }
                    }
                }
            }

            for (int i = 0; i < (int)PaintRegion.Height + 1; i++)
            {
                MSPointSet pointset2D = new MSPointSet();
                float MaxIntensity = 0;
                for (int j = 0; j < (int)PaintRegion.Width + 1; j++)
                {
                    if (Data[j, i] > MaxIntensity)
                    {
                        MaxIntensity = Data[j, i];
                    }
                }
                if (MaxIntensity > 0/*mspointset.MaxZ/1*/)
                {
                    for (int j = 0; j < (int)PaintRegion.Width + 1; j++)
                    {
                        if (Data[j, i] != -1)
                        {
                            pointset2D.Add(CoordinateTrans.XToTime(PaintRegion, pointSetRegion, j), Data[j, i]);
                        }
                    }
                    pointset2D = BSpline2D(pointset2D, pointSetRegion, PaintRegion, "horizontal");

                    for (int x = 0; x < pointset2D.Count; x++)
                    {
                        if (Data[(int)Math.Round((CoordinateTrans.TimeToX(pointSetRegion, PaintRegion, pointset2D.X(x)))), i] == -1)
                        {
                            Data[(int)Math.Round((CoordinateTrans.TimeToX(pointSetRegion, PaintRegion, pointset2D.X(x)))), i] = pointset2D.Y(x);
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < (int)PaintRegion.Width + 1; j++)
                    {
                        if (Data[j, i] == -1)
                        {
                            Data[j, i] = 0;
                        }
                    }

                }
            }

            Data = NoiseFiltering(Data, (int)PaintRegion.Height + 1, (int)PaintRegion.Width + 1);

            for (int i = 0; i < (int)PaintRegion.Width + 1; i++)
            {
                for (int j = 0; j < (int)PaintRegion.Height + 1; j++)
                {
                    if (Data[i, j] != -1)
                    {
                        interpolatedPointSet3D.Add(CoordinateTrans.XToTime(PaintRegion, pointSetRegion, i), CoordinateTrans.YToMass(PaintRegion, pointSetRegion, j), Data[i, j]);
                    }
                    else
                    {
                        interpolatedPointSet3D.Add(CoordinateTrans.XToTime(PaintRegion, pointSetRegion, i), CoordinateTrans.YToMass(PaintRegion, pointSetRegion, j), 0);

                    }
                }
            }
            return interpolatedPointSet3D;
        }

        public MSPointSet BSpline2D(MSPointSet pointset2D, RegionSize pointSetRegion, RegionSize targetRegion, string type)
        {
            MSPointSet interpolatedPointSet = new MSPointSet();
            int NoExist = pointset2D.Count;
            int NoPoints = 0;
            if (type == "horizontal")
            {
                NoPoints = (int)(targetRegion.Width);
            }
            if (type == "vertical")
            {
                NoPoints = (int)(targetRegion.Height);
            }
            int degree = 2;

            float[] Tk = new float[NoExist];
            float[,] point_exist = new float[NoExist, 2];
            float[] Uk = new float[NoExist + degree + 1];
            float[] sum = new float[NoExist];
            float[,] Nk = new float[NoExist, NoExist];
            float[,] P = new float[NoExist, 2];
            float[] P_temp = new float[NoExist];
            float[] points = new float[NoPoints + 1];

            for (int i = 0; i < NoPoints; i++)
            {
                points[i] = -1;
            }
            for (int i = 0; i < pointset2D.Count; i++)
            {
                if (type == "horizontal")
                {
                    points[(int)Math.Round(CoordinateTrans.TimeToX(pointSetRegion, targetRegion, pointset2D.X(i)))] = pointset2D.Y(i);
                }
                if (type == "vertical")
                {
                    points[(int)Math.Round(CoordinateTrans.MassToY(pointSetRegion, targetRegion, pointset2D.X(i)))] = pointset2D.Y(i);
                }
            }

            if (NoExist <= 3)
            {
                for (int i = 0; i < NoPoints; i++)
                {
                    if (points[i] == -1)
                    {
                        points[i] = 0;
                    }
                }
            }
            else
            {
                int cnt = 0;
                for (int i = 0; i < NoPoints; i++)
                {
                    if (points[i] != -1)
                    {
                        point_exist[cnt, 0] = i;
                        point_exist[cnt, 1] = points[i];
                        cnt++;
                    }
                }
                sum[0] = 0;
                for (int i = 1; i < NoExist; i++)
                {
                    sum[i] = sum[i - 1] + (float)Math.Pow((Math.Abs(point_exist[i, 0] - point_exist[i - 1, 0]) * Math.Abs(point_exist[i - 1, 0] - point_exist[i, 0]) + Math.Abs(point_exist[i - 1, 1] - point_exist[i, 1]) * Math.Abs(point_exist[i - 1, 1] - point_exist[i, 1])), 0.5);
                }
                Tk[0] = 0;
                for (int i = 1; i < NoExist; i++)
                {
                    Tk[i] = sum[i] / sum[NoExist - 1];
                }

                for (int i = 0; i < NoExist + degree + 1; i++)
                {
                    if (i <= degree)
                    {
                        Uk[i] = 0;
                    }
                    if (i > degree && i < NoExist)
                    {
                        Uk[i] = 0;
                        for (int j = i - degree; j < i; j++)
                        {
                            Uk[i] += Tk[j];
                        }
                        Uk[i] /= degree;
                    }
                    if (i >= NoExist)
                    {
                        Uk[i] = 1;
                    }
                }
                float[,] Temp = new float[NoExist, NoExist + 1];

                for (int i = 0; i < NoExist; i++)
                {
                    float[] Nk_temp = new float[NoExist];
                    for (int j = 0; j < NoExist; j++)
                    {
                        Nk_temp[j] = 0;
                    }
                    Compute_Nk_fast(Nk_temp, NoExist, degree, Tk[i], Uk);

                    for (int j = 0; j < NoExist; j++)
                    {
                        Nk[i, j] = Nk_temp[j];
                    }
                }

                for (int n = 0; n < 2; n++)
                {
                    for (int i = 0; i < NoExist; i++)
                    {
                        for (int j = 0; j < NoExist; j++)
                        {
                            Temp[i, j] = Nk[i, j];
                        }
                    }

                    for (int i = 0; i < NoExist; i++)
                    {
                        Temp[i, NoExist] = point_exist[i, n];
                        P_temp[i] = 0;
                    }
                    Gaussian_eliminate(Temp, NoExist, ref P_temp);

                    for (int i = 0; i < NoExist; i++)
                    {
                        P[i, n] = P_temp[i];
                    }
                }

                int PredictNo = NoPoints * 10;
                float[,] points_temp = new float[NoPoints, 2];
                for (int i = 0; i < NoPoints; i++)
                {
                    points_temp[i, 0] = 0;
                    points_temp[i, 1] = 0;
                }
                for (int i = 0; i < PredictNo; i++)
                {
                    float temp_x = 0;
                    float temp_y = 0;
                    float[] Nk_temp = new float[NoExist];

                    for (int j = 0; j < NoExist; j++)
                    {
                        Nk_temp[j] = 0;
                    }
                    Compute_Nk_fast(Nk_temp, NoExist, degree, ((float)1 / PredictNo) * i, Uk);

                    for (int j = 0; j < NoExist; j++)
                    {
                        temp_x += Nk_temp[j] * P[j, 0];
                        temp_y += Nk_temp[j] * P[j, 1];
                    }


                    if (temp_x < 0)
                    {
                        temp_x = 0;
                    }
                    if (temp_y < 0)
                    {
                        temp_y = 0;
                    }
                    if ((int)Math.Round(temp_x) <= NoPoints - 1)
                    {
                        if (points[(int)Math.Round(temp_x)] == -1)
                        {
                            if (temp_y > points_temp[(int)Math.Round(temp_x), 0])
                            {
                                points_temp[(int)Math.Round(temp_x), 0] = temp_y;
                            }
                        }
                    }
                }

                points = linearInterpolation(points);
                /*//use linear interpolation to prevent that the value was nonpredicted
                bool again = false;
                bool first = true;
                int pivot_start = 0;
                int pivot_end = 0;
                float value_start = 0;
                float value_end = 0;
                do
                {
                    again = false;
                    first = true;
                    for (int i = 0; i < NoPoints && again == false; i++)
                    {
                        if (points[i] == -1 && first == true)
                        {
                            if (i == 0)
                            {
                                points[i] = 0;
                            }
                            else
                            {
                                if (i == NoPoints - 1)
                                {
                                    points[NoPoints - 1] = points[NoPoints - 2];
                                }
                                else
                                {

                                    first = false;
                                    pivot_start = i - 1;
                                    value_start = points[i - 1];
                                }

                            }
                        }
                        if (points[i] != -1 && first == false)
                        {
                            pivot_end = i;
                            value_end = points[i];
                            points[(int)((pivot_start + pivot_end) / 2)] = (value_end + value_start) / 2;
                            again = true;
                        }
                        if (i == (NoPoints - 1) && first == false && again == false)
                        {
                            pivot_end = i;
                            value_end = 0;
                            points[(int)((pivot_start + pivot_end) / 2)] = (value_end + value_start) / 2;
                            again = true;
                        }
                    }

                } while (again);*/
            }

            for (int i = 0; i < NoPoints; i++)
            {
                if (type == "horizontal")
                {
                    interpolatedPointSet.Add(CoordinateTrans.XToTime(targetRegion, pointSetRegion, i), points[i]);
                }
                if (type == "vertical")
                {
                    interpolatedPointSet.Add(CoordinateTrans.YToMass(targetRegion, pointSetRegion, i), points[i]);
                }
            }

            return interpolatedPointSet;
        }
        public MSPointSet BSpline2D(MSPointSet argPointset2D, RegionSize argPointSetRegion, RegionSize argTargetRegion)
        {
            MSPointSet interpolatedPointSet = new MSPointSet();
            int NoExist = argPointset2D.Count;
            int NoPoints = (int)(argTargetRegion.Width);

            int degree = 2;

            float[] Tk = new float[NoExist];
            float[,] point_exist = new float[NoExist, 2];
            float[] Uk = new float[NoExist + degree + 1];
            float[] sum = new float[NoExist];
            float[,] Nk = new float[NoExist, NoExist];
            float[,] P = new float[NoExist, 2];
            float[] P_temp = new float[NoExist];
            float[] points = new float[NoPoints + 1];

            for (int i = 0; i < NoPoints; i++)
            {
                points[i] = -1;
            }
            for (int i = 0; i < argPointset2D.Count; i++)
            {
                    points[(int)Math.Round(CoordinateTrans.TimeToX(argPointSetRegion, argTargetRegion, argPointset2D.X(i)))] = argPointset2D.Y(i); 
            }

            if (NoExist <= 3)
            {
                for (int i = 0; i < NoPoints; i++)
                {
                    if (points[i] == -1)
                    {
                        points[i] = 0;
                    }
                }
            }
            else
            {
                int cnt = 0;
                for (int i = 0; i < NoPoints; i++)
                {
                    if (points[i] != -1)
                    {
                        point_exist[cnt, 0] = i;
                        point_exist[cnt, 1] = points[i];
                        cnt++;
                    }
                }
                sum[0] = 0;
                for (int i = 1; i < NoExist; i++)
                {
                    sum[i] = sum[i - 1] + (float)Math.Pow((Math.Abs(point_exist[i, 0] - point_exist[i - 1, 0]) * Math.Abs(point_exist[i - 1, 0] - point_exist[i, 0]) + Math.Abs(point_exist[i - 1, 1] - point_exist[i, 1]) * Math.Abs(point_exist[i - 1, 1] - point_exist[i, 1])), 0.5);
                }
                Tk[0] = 0;
                for (int i = 1; i < NoExist; i++)
                {
                    Tk[i] = sum[i] / sum[NoExist - 1];
                }

                for (int i = 0; i < NoExist + degree + 1; i++)
                {
                    if (i <= degree)
                    {
                        Uk[i] = 0;
                    }
                    if (i > degree && i < NoExist)
                    {
                        Uk[i] = 0;
                        for (int j = i - degree; j < i; j++)
                        {
                            Uk[i] += Tk[j];
                        }
                        Uk[i] /= degree;
                    }
                    if (i >= NoExist)
                    {
                        Uk[i] = 1;
                    }
                }
                float[,] Temp = new float[NoExist, NoExist + 1];

                for (int i = 0; i < NoExist; i++)
                {
                    float[] Nk_temp = new float[NoExist];
                    for (int j = 0; j < NoExist; j++)
                    {
                        Nk_temp[j] = 0;
                    }
                    Compute_Nk_fast(Nk_temp, NoExist, degree, Tk[i], Uk);

                    for (int j = 0; j < NoExist; j++)
                    {
                        Nk[i, j] = Nk_temp[j];
                    }
                }

                for (int n = 0; n < 2; n++)
                {
                    for (int i = 0; i < NoExist; i++)
                    {
                        for (int j = 0; j < NoExist; j++)
                        {
                            Temp[i, j] = Nk[i, j];
                        }
                    }

                    for (int i = 0; i < NoExist; i++)
                    {
                        Temp[i, NoExist] = point_exist[i, n];
                        P_temp[i] = 0;
                    }
                    Gaussian_eliminate(Temp, NoExist, ref P_temp);

                    for (int i = 0; i < NoExist; i++)
                    {
                        P[i, n] = P_temp[i];
                    }
                }

                int PredictNo = NoPoints * 10;
                float[,] points_temp = new float[NoPoints, 2];
                for (int i = 0; i < NoPoints; i++)
                {
                    points_temp[i, 0] = 0;
                    points_temp[i, 1] = 0;
                }
                for (int i = 0; i < PredictNo; i++)
                {
                    float temp_x = 0;
                    float temp_y = 0;
                    float[] Nk_temp = new float[NoExist];

                    for (int j = 0; j < NoExist; j++)
                    {
                        Nk_temp[j] = 0;
                    }
                    Compute_Nk_fast(Nk_temp, NoExist, degree, ((float)1 / PredictNo) * i, Uk);

                    for (int j = 0; j < NoExist; j++)
                    {
                        temp_x += Nk_temp[j] * P[j, 0];
                        temp_y += Nk_temp[j] * P[j, 1];
                    }


                    if (temp_x < 0)
                    {
                        temp_x = 0;
                    }
                    if (temp_y < 0)
                    {
                        temp_y = 0;
                    }
                    if ((int)Math.Round(temp_x) <= NoPoints - 1)
                    {
                        if (points[(int)Math.Round(temp_x)] == -1)
                        {
                            if (temp_y > points_temp[(int)Math.Round(temp_x), 0])
                            {
                                points_temp[(int)Math.Round(temp_x), 0] = temp_y;
                            }
                        }
                    }
                }

                points = linearInterpolation(points);
             
            }

            for (int i = 0; i < NoPoints; i++)
            {
                    interpolatedPointSet.Add(CoordinateTrans.XToTime(argTargetRegion, argPointSetRegion, i), points[i]);

            }

            return interpolatedPointSet;
        }
        private float[] linearInterpolation(float[] points)
        {
            int NoPoints = points.Length;
            bool again = false;
            bool first = true;
            int pivot_start = 0;
            int pivot_end = 0;
            float value_start = 0;
            float value_end = 0;
            do
            {
                again = false;
                first = true;
                for (int i = 0; i < NoPoints && again == false; i++)
                {
                    if (points[i] == -1 && first == true)
                    {
                        if (i == 0)
                        {
                            points[i] = 0;
                        }
                        else
                        {
                            if (i == NoPoints - 1)
                            {
                                points[NoPoints - 1] = points[NoPoints - 2];
                            }
                            else
                            {

                                first = false;
                                pivot_start = i - 1;
                                value_start = points[i - 1];
                            }

                        }
                    }
                    if (points[i] != -1 && first == false)
                    {
                        pivot_end = i;
                        value_end = points[i];
                        points[(int)((pivot_start + pivot_end) / 2)] = (value_end + value_start) / 2;
                        again = true;
                    }
                    if (i == (NoPoints - 1) && first == false && again == false)
                    {
                        pivot_end = i;
                        value_end = 0;
                        points[(int)((pivot_start + pivot_end) / 2)] = (value_end + value_start) / 2;
                        again = true;
                    }
                }

            } while (again);
            return points;
        }

        private void Compute_Nk_fast(float[] Nk_temp, int NoExist, int degree, float tk, float[] uk)
        {
            if (tk == uk[0])
            {
                Nk_temp[0] = 1;
            }
            else
            {
                if (tk == uk[NoExist + degree])
                {
                    Nk_temp[NoExist - 1] = 1;
                }
                else
                {
                    int pivot = 0;
                    for (int k = 0; k < NoExist + degree; k++)
                    {
                        if (tk >= uk[k] && tk < uk[k + 1])
                        {
                            pivot = k;
                            break;
                        }
                    }
                    Nk_temp[pivot] = 1;

                    for (int k = 1; k < degree + 1; k++)
                    {
                        Nk_temp[pivot - k] = ((uk[pivot + 1] - tk) / (uk[pivot + 1] - uk[pivot - k + 1])) * Nk_temp[pivot - k + 1];
                        for (int i = pivot - k + 1; i < pivot; i++)
                        {
                            Nk_temp[i] = ((tk - uk[i]) / (uk[i + k] - uk[i]) * Nk_temp[i]) + ((uk[i + k + 1] - tk) / (uk[i + k + 1] - uk[i + 1]) * Nk_temp[i + 1]);
                        }
                        Nk_temp[pivot] = (tk - uk[pivot]) / (uk[pivot + k] - uk[pivot]) * Nk_temp[pivot];
                    }
                }
            }
        }

        private float Compute_Nk(int i, int p, float tk, float[] uk)
        {
            if (p == 0)
            {
                if (uk[i] <= tk && tk <= uk[i + 1])
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                float a, b;
                if (uk[i + p] - uk[i] == 0 || tk - uk[i] == 0)
                {
                    a = 0;
                }
                else
                {
                    a = (tk - uk[i]) / (uk[i + p] - uk[i]) * Compute_Nk(i, p - 1, tk, uk);
                }
                if (uk[i + p + 1] - tk == 0 || uk[i + p + 1] - uk[i + 1] == 0)
                {
                    b = 0;
                }
                else
                {
                    b = (uk[i + p + 1] - tk) / (uk[i + p + 1] - uk[i + 1]) * Compute_Nk(i + 1, p - 1, tk, uk);
                }
                return a + b;
            }
        }

        private void Gaussian_eliminate(float[,] A, int N, ref float[] X)
        {

            for (int i = 0; i < N; i++)
            {
                // find row with maximum in column i
                int max_row = i;
                for (int j = i; j < N; j++)
                {
                    if (Math.Abs(A[j, i]) > Math.Abs(A[max_row, i]))
                    {
                        max_row = j;
                    }
                }

                // swap max row with row i of [A:y]
                for (int k = i; k < N + 1; k++)
                {
                    float tmp = A[i, k];
                    A[i, k] = A[max_row, k];
                    A[max_row, k] = tmp;
                }

                // eliminate lower diagonal elements of [A]
                for (int j = i + 1; j < N; j++)
                {
                    for (int k = N; k > i; k--)
                    {
                        if (A[i, i] == 0.0)
                        {
                            //MessageBox.Show("Gaussian Elimination Failed");
                            return;
                        }
                        else
                        {
                            A[j, k] = A[j, k] - A[i, k] * A[j, i] / A[i, i];
                        }
                    }
                }
            }

            for (int j = N - 1; j >= 0; j--)
            {
                float sum = 0;
                for (int k = j + 1; k < N; k++)
                {
                    sum += A[j, k] * X[k];
                }
                X[j] = (A[j, N] - sum) / A[j, j];
            }
        }

        public float[,] NoiseFiltering(float[,] data, int width, int height)
        {
            for (int i = 1; i < height - 1; i++)
            {
                for (int j = 1; j < width - 1; j++)
                {
                    data[i, j] = (data[i - 1, j - 1] + data[i - 1, j] + data[i - 1, j + 1] + data[i, j - 1] + data[i, j] + data[i, j + 1] + data[i + 1, j - 1] + data[i + 1, j] + data[i + 1, j + 1]) / 9;
                }
            }
            return data;
        }
    }
}
