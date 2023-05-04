using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
namespace COL.ElutionViewer
{
    public class EViewerHandler
    {
        private MSPointSet3D originalPointset;
        private MSPointSet3D currentPointset;
        private MSPointSet3D rawDataSet3D;
      
        //private ChargeQuant _chargeQuant;

        private RegionSize _bmpRegion;
        
        private bool renew_plot;
        private float X_start;
        private float Y_start;
        private float X_end;
        private float Y_end;
      
        private float X_min, X_max, Y_min, Y_max, Z_min, Z_max;
        private float point_size;
        private bool _gray;
        private bool _color;
        private bool _isScaled;
        private bool _isSmoothed;
        private bool _isresize;
        private int _scaleMax;
        private int _scaleMin;
        private int color_number;
        private int BaseColor;        
        private int[,] palette;
        private Bitmap bitmap1;
        private int noPoints;
        private float scalesize;
        

        //private string _message;
        //private List<IMessageObserver> _observers;

        //public string Message
        //{
        //    get { return _message; }
        //    set 
        //    { 
        //        _message = value;
        //        Notify();
        //    }
        //}

        public float ScaleSize
        {
            get { return scalesize; }
            set { scalesize = value; }
        }
        public int NoPoints
        {
            get { return noPoints; }
            set { noPoints = value; }
        }

        public float Xstart
        {
            get { return X_start; }
            set { X_start = value; }
        }
        public float Ystart
        {
            get { return Y_start; }
            set { Y_start = value; }
        }
        public float Xend
        {
            get { return X_end; }
            set { X_end = value; }
        }
        public float Yend
        {
            get { return Y_end; }
            set { Y_end = value; }
        }

        public bool IsResize
        {
            get { return _isresize; }
            set { _isresize = value; }
        }
        public int ScaleMax
        {
            get { return _scaleMax; }
            set { _scaleMax = value; }
        }
        public int ScaleMin
        {
            get { return _scaleMin; }
            set { _scaleMin = value; }
        }
        public bool IsScaled
        {
            get { return _isScaled; }
            set { _isScaled = value; }
        }
        public bool IsGray
        {
            get { return _gray; }
            set { _gray = value; }
        }
        public bool IsColor
        {
            get { return _color; }
            set { _color = value; }
        }
        public RegionSize BmpRegion
        {
            get { return _bmpRegion; }
            set { _bmpRegion = value; }
        }
        public bool Renew_plot
        {
            get { return renew_plot; }
            set { renew_plot = value; }
        }
        public bool IsSmoothed
        {
            get { return _isSmoothed; }
            set { _isSmoothed = value; }
        }

        public MSPointSet3D MyOriginalPointSet3D
        {
            get { return originalPointset; }
            set { originalPointset = value; }

        }
        public MSPointSet3D MyCurrentPointSet3D
        {
            get { return currentPointset; }
            set { currentPointset = value; }
        }

        public MSPointSet3D RawDataPointSet3D
        {
            get { return rawDataSet3D; }
            set { rawDataSet3D = value; }
        }
        public void SetColor()
        {
            IsColor = true;
            IsGray = false;
        }
        public void SetGray()
        {
            IsGray = true;
            IsColor = false;
        }

        public static EViewerHandler Create3DHandler(MSPointSet3D argMSPointSet3D)
        {
            EViewerHandler _EViewerHandler = new EViewerHandler(argMSPointSet3D);

            return _EViewerHandler;
        }

        public void initialize()
        {
            MyCurrentPointSet3D = RawDataPointSet3D;
            MyOriginalPointSet3D = RawDataPointSet3D;
            NoPoints = MyCurrentPointSet3D.Count;           
            IsSmoothed = false;
            renew_plot = true;
            ScaleSize = 1;
            BmpRegion = new RegionSize(0, 300, 200, 0);
        }
        private EViewerHandler(MSPointSet3D argMSPointSet3D)
        {
            //_observers = new List<IMessageObserver>();
            MyOriginalPointSet3D = argMSPointSet3D;
            RawDataPointSet3D = argMSPointSet3D;
            initialize();
            SetColor();
            color_number = 180;
            BaseColor = 4;
            palette = new int[color_number, 3];

            for (int i = 0; i < (color_number / BaseColor); i++)
            {
                palette[i, 0] = color_number;
                palette[i, 1] = i * BaseColor;
                palette[i, 2] = 0;

                palette[i + color_number / BaseColor, 0] = color_number - (i * BaseColor);
                palette[i + color_number / BaseColor, 1] = color_number;
                palette[i + color_number / BaseColor, 2] = 0;

                palette[i + (color_number / BaseColor) * 2, 0] = 0;
                palette[i + (color_number / BaseColor) * 2, 1] = color_number;
                palette[i + (color_number / BaseColor) * 2, 2] = i * BaseColor;

                palette[i + (color_number / BaseColor) * 3, 0] = 0;
                palette[i + (color_number / BaseColor) * 3, 1] = color_number - (i * BaseColor);
                palette[i + (color_number / BaseColor) * 3, 2] = color_number;
            }
        }
        //private EViewerHandler(List<MSPointSet3D> argMSPointSet3D)
        //{
        //    //_observers = new List<IMessageObserver>();
        //    originalPointsetLst = argMSPointSet3D;
        //    rawDataSet3DLst = argMSPointSet3D;
        //    initialize();
        //    SetColor();
        //    color_number = 180;
        //    BaseColor = 4;
        //    palette = new int[color_number, 3];

        //    for (int i = 0; i < (color_number / BaseColor); i++)
        //    {
        //        palette[i, 0] = color_number;
        //        palette[i, 1] = i * BaseColor;
        //        palette[i, 2] = 0;

        //        palette[i + color_number / BaseColor, 0] = color_number - (i * BaseColor);
        //        palette[i + color_number / BaseColor, 1] = color_number;
        //        palette[i + color_number / BaseColor, 2] = 0;

        //        palette[i + (color_number / BaseColor) * 2, 0] = 0;
        //        palette[i + (color_number / BaseColor) * 2, 1] = color_number;
        //        palette[i + (color_number / BaseColor) * 2, 2] = i * BaseColor;

        //        palette[i + (color_number / BaseColor) * 3, 0] = 0;
        //        palette[i + (color_number / BaseColor) * 3, 1] = color_number - (i * BaseColor);
        //        palette[i + (color_number / BaseColor) * 3, 2] = color_number;
        //    }

        //    smoothedSetLst = new List<MSPointSet3D>();
        //    foreach (MSPointSet3D msp3d in RawDataPointSet3DList)
        //    {
        //        BSpline BS = new BSpline();
        //        MSPointSet MSP = new MSPointSet();
        //        MSP.AddMSPoints(msp3d._x, msp3d._z);
        //        MSPointSet smoothedMSP = BS.BSpline2D(MSP, new RegionSize(MSP.X(0),MSP.X(MSP.Count-1),MSP.Y(MSP.MaxIntensityIdx),MSP.Y(MSP.MinIntensityIdx)), _bmpRegion);
        //        List<float> mz = new List<float>();
        //        for(int  i =0;i<smoothedMSP.XLst.Count;i++)
        //        {
        //            mz.Add(msp3d._y[0]);
        //        }
        //        smoothedSetLst.Add(new MSPointSet3D(smoothedMSP.XLst, mz, smoothedMSP.YLst));
        //    }
        //}
        public MSPointSet3D smoothing(MSPointSet3D current)
        {
            MSPointSet3D result3D = new MSPointSet3D();
            BSpline BS = new BSpline();
            result3D = BS.BSpline3DofPepImage(current, BmpRegion);
            return result3D;
        }


        public void GetSubPointSet(RegionSize SR)
        {
            if (SubPointSetExtract(SR).Count == 0)
            {
                return;
            }
            MyCurrentPointSet3D = SubPointSetExtract(SR);
            //MyOriginalPointSet3D = MyCurrentPointSet3D;
            NoPoints = MyCurrentPointSet3D.Count;
        }

        public MSPointSet3D SubPointSetExtract(RegionSize SR)
        {
            MSPointSet3D tempPointSet = new MSPointSet3D();

            //foreach (MSPoint3D D in MyCurrentPointSet3D.)
            for (int i = 0; i < RawDataPointSet3D.Count; i++)
            {
                if (RawDataPointSet3D._x[i] >= SR.LeftBound && RawDataPointSet3D._x[i] <= SR.RightBound && RawDataPointSet3D._y[i] <= SR.TopBound && RawDataPointSet3D._y[i] >= SR.BottomBound)
                {
                    tempPointSet.Add(RawDataPointSet3D._x[i], RawDataPointSet3D._y[i], RawDataPointSet3D.Z(i));
                }
            }
            return tempPointSet;
        }

        //public MSPointSet Resize(float width, float height)
        //{
        //    MSPointSet mspoint = new MSPointSet();

        //    return mspoint;
        //}

        public void ImageDraw(Graphics g, RegionSize PictureBoxRegion)
        {
            if(PictureBoxRegion.Width*PictureBoxRegion.Height==0)
            {
                //Message = "The data you selected aren't enough for drawing, please reselect again.";
                return;
            }
            //Plot Size
            X_start = 60;
            Y_start = PictureBoxRegion.TopBound - 40;
            X_end = PictureBoxRegion.RightBound - 150;
            Y_end = 60;

            X_max = MyCurrentPointSet3D.MaxXwWhiteBoarder;
            X_min = MyCurrentPointSet3D.MinXwWhiteBoarder;
            Y_max = MyCurrentPointSet3D.MaxYwWhiteBoarder;
            Y_min = MyCurrentPointSet3D.MinYwWhiteBoarder;
            Z_max = MyCurrentPointSet3D.MaxZ;
            Z_min = MyCurrentPointSet3D.MinZ;

            if (IsScaled)
            {
                Z_max = ScaleMax;
                Z_min = ScaleMin;
            }


            point_size = Math.Min(PictureBoxRegion.Width / 300, PictureBoxRegion.Height / 300);

            SolidBrush RedBrush = new SolidBrush(Color.Red);
            SolidBrush BlackBrush = new SolidBrush(Color.Black);
            Pen BlackPen = new Pen(Color.Black);
            Pen RedPen = new Pen(Color.Red, 3);


            Pen PrecursorPen = null;

            if(IsGray)
            {
                PrecursorPen = new Pen(Color.FromArgb(90, 0, 0, 255));
            }
            if(IsColor)
            {
                PrecursorPen = new Pen(Color.Black);
            }
            Font font8Bold = new Font("Time New Roman", 8, FontStyle.Bold);
            Font font8Regular = new Font("Time New Roman", 8, FontStyle.Regular);

            //PrecursorL = (float)(Y_start - ((MyChargeQuant.MyCharge.LightReagent.MsChart.PrecursorMZ - Y_min) * Math.Abs(Y_end - Y_start) / (Y_max - Y_min)));
            //PrecursorH = (float)(Y_start - ((MyChargeQuant.MyCharge.HeavyReagent.MsChart.PrecursorMZ - Y_min) * Math.Abs(Y_end - Y_start) / (Y_max - Y_min)));

            int X_tag_number = 10;
            int Y_tag_number = 10;
            
            int color_shift = 20;
            int Rc, Gc, Bc;
            Rc = Bc = Gc = 0;
            float[] X_tag = new float[X_tag_number];
            float[] Y_tag = new float[Y_tag_number];
            float[] X_co = new float[X_tag_number];
            float[] Y_co = new float[Y_tag_number];
            float[] Z_tag = new float[color_number];

            
            g.DrawLine(BlackPen, X_start, Y_start, X_end, Y_start);//Draw X_axis
            g.DrawLine(BlackPen, X_start, Y_start, X_start, Y_end);//Draw Y_axis
            g.DrawString("Elution time (Min)", font8Regular, BlackBrush, X_end, (Y_start + 5));//Draw Text(X_axis)
            g.DrawString("m/z (Da)", font8Regular, BlackBrush, (X_start - 30), (Y_end - 20));//Draw Text(X_axis)
            g.DrawString("Intensity", font8Regular, BlackBrush, (X_end + 40), (Y_start - 50));//Draw Text(X_axis)
            g.DrawString(Convert.ToString((int)Z_max), font8Regular, BlackBrush, (X_end + 80), (Y_start - 55 - (color_number * point_size))); // Intensity Max
            g.DrawString(Convert.ToString((int)Z_min), font8Regular, BlackBrush, (X_end + 80), (Y_start - 65)); // Intensity Min
           // g.DrawString("Spectrum number: " + MyChargeQuant.MyCharge.MyPeptideIdentification.SpecNumber + ", " + MyChargeQuant.MyCharge.MyPeptideIdentification.ModifiedPeptideSequence, font8Regular, BlackBrush, 5, 5);
            //g.DrawString("Charge state: +" + MyChargeQuant.MyCharge.ChargeNo, font8Regular, BlackBrush, 5, 20);


            //Color Bar
            for (int i = 0; i < color_number; i++)
            {
                if (IsGray)
                {
                    Rc = color_number - 1 - i + color_shift;
                    Gc = color_number - 1 - i + color_shift;
                    Bc = color_number - 1 - i + color_shift;
                }
                if (IsColor)
                {
                    Rc = palette[(color_number - 1 - i), 0];
                    Gc = palette[(color_number - 1 - i), 1];
                    Bc = palette[(color_number - 1 - i), 2];
                }
                SolidBrush peletteBrush = new SolidBrush(Color.FromArgb(Rc, Gc, Bc));
                RectangleF Rt = new RectangleF(X_end + 60, (Y_start - 55 - (i * point_size)), 15, point_size);
                g.FillRectangle(peletteBrush, Rt);
            }

            //X_axis number
            for (int i = 0; i < X_tag_number; i++)  
            {
                X_tag[i] = (float)Math.Round((X_min + (((X_max - X_min) / X_tag_number) * i)), 2);
                X_co[i] = (float)(X_start + ((Math.Abs((X_end - X_start)) / X_tag_number) * i));

                g.DrawString(Convert.ToString(X_tag[i]), font8Regular, BlackBrush, X_co[i] - 10, (Y_start + 15));//Draw Text(X_start)

            }

            //Y_axis number
            for (int i = 0; i < Y_tag_number; i++)
            {
                Y_tag[i] = (float)Math.Round((Y_min + (((Y_max - Y_min) / Y_tag_number) * i)), 2);
                Y_co[i] = (float)(Y_start - ((Math.Abs((Y_end - Y_start)) / Y_tag_number) * i));
                g.DrawString(Convert.ToString(Y_tag[i]), font8Regular, BlackBrush, (X_start - 45), Y_co[i] - 10);

            }
                
            for (int i = 0; i < color_number; i++)
            {
                Z_tag[i] = (float)(Z_min + (((Z_max - Z_min) / color_number) * i));
            }

            if (renew_plot)
            {
                
                bitmap1 = new Bitmap((int)BmpRegion.RightBound + 1, (int)BmpRegion.TopBound + 1, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        
                int temp_z = 0;
                
                Graphics bitmapG = Graphics.FromImage(bitmap1);
                bitmapG.Clear(Color.White);

                float temp_x, temp_y;
                temp_x = temp_y = temp_z = 0;

                RegionSize DataRegion = new RegionSize(MyCurrentPointSet3D.MinXwWhiteBoarder, MyCurrentPointSet3D.MaxXwWhiteBoarder, MyCurrentPointSet3D.MaxYwWhiteBoarder, MyCurrentPointSet3D.MinYwWhiteBoarder);
                
                if (DataRegion.Width * DataRegion.Height == 0)
                {                 
                    return;
                }
                //foreach (MSPoint3D p in MyCurrentPointSet3D.Points3D)
                for (int x = 0; x < MyCurrentPointSet3D.Count; x++)
                {
                    if (MyCurrentPointSet3D.Z(x) == -1)
                    {
                        Rc = 255;
                        Gc = 255;
                        Bc = 255;
                    }
                    else
                    {
                        for (int i = 0; i < color_number; i++)
                        {
                            if (MyCurrentPointSet3D.Z(x) >= Z_tag[i])
                            {
                                temp_z = i;
                            }
                            else
                            {
                                if (i == 0)
                                {
                                    temp_z = 0;
                                }
                                break;
                            }
                        }
                        if (IsGray)
                        {
                            IsColor = false;
                            Rc = color_number - 1 - temp_z + color_shift;
                            Gc = color_number - 1 - temp_z + color_shift;
                            Bc = color_number - 1 - temp_z + color_shift;
                        }

                        if (IsColor)
                        {
                            Rc = palette[color_number - 1 - temp_z, 0];
                            Gc = palette[color_number - 1 - temp_z, 1];
                            Bc = palette[color_number - 1 - temp_z, 2];
                        }
                    }

                    bitmap1.SetPixel((int)Math.Round(CoordinateTrans.TimeToX(DataRegion, BmpRegion, MyCurrentPointSet3D.X(x))),
                                                  (int)BmpRegion.Height - (int)Math.Round(CoordinateTrans.MassToY(DataRegion, BmpRegion, MyCurrentPointSet3D.Y(x))), 
                                                  Color.FromArgb(Rc, Gc, Bc));
                    renew_plot = false;
                }
            }

            g.DrawImage(bitmap1, X_start+1, Y_end, X_end - X_start, Y_start - Y_end);


            for (int i = 0; i < X_tag_number; i++)
            {
                g.DrawLine(BlackPen, X_co[i], Y_start, X_co[i], Y_start - 5);
            }

            for (int i = 0; i < Y_tag_number; i++)
            {
                g.DrawLine(BlackPen, X_start, Y_co[i], X_start + 5, Y_co[i]);
            }

            //PrecursorPen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDotDot;

            //g.DrawLine(PrecursorPen, X_start, PrecursorL, X_end, PrecursorL);
            //g.DrawLine(PrecursorPen, X_start, PrecursorH, X_end, PrecursorH);
            
            //SolidBrush Precursorbrush = new SolidBrush(Color.Red);
            //Font font2 = new Font("Time New Roman", 8, FontStyle.Bold);
            //g.DrawString("L: " + Math.Round(MyChargeQuant.MyCharge.LightReagent.MsChart.PrecursorMZ, 2)+" Da", font2, Precursorbrush, X_end + 5, PrecursorL - 10);
            //g.DrawString("H: " + Math.Round(MyChargeQuant.MyCharge.HeavyReagent.MsChart.PrecursorMZ, 2)+" Da", font2, Precursorbrush, X_end + 5, PrecursorH - 10);
            //if (MyChargeQuant.MyCharge.Heavy2Reagent != null)
            //{
            //    g.DrawString("H: " + Math.Round(MyChargeQuant.MyCharge.Heavy2Reagent.MsChart.PrecursorMZ, 2) + " Da", font2, Precursorbrush, X_end + 5, MyChargeQuant.MyCharge.Heavy2Reagent.MsChart.PrecursorMZ - 10);
            //}

            //Pen TimePen = new Pen(Color.Red);
            //SolidBrush brush3 = new SolidBrush(Color.Red);
            //TimePen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDotDot;
            //float Time = (float)(X_start + ((MyChargeQuant.MyCharge.MyPeptideIdentification.ElutionTime - X_min) * Math.Abs(X_end - X_start) / (X_max - X_min)));
            //g.DrawLine(TimePen, Time, Y_start, Time, Y_end);

            //g.DrawString("" + Math.Round(MyChargeQuant.MyCharge.MyPeptideIdentification.ElutionTime, 2), font2, brush3, Time - 5, Y_end-15);
            //g.DrawString("Elution Time:", font2, brush3, Time-10, (Y_end - 25));

                                   
        }

        public void SmoothingofSmoothed(float X_min, float X_max, float Y_max, float Y_min)
        {
            MyCurrentPointSet3D = MyOriginalPointSet3D;
            GetSubPointSet(new RegionSize(X_min, X_max, Y_max, Y_min));
            ScaleSize = ScaleSize * Math.Min((X_max - X_min) / (MyCurrentPointSet3D.MaxX - MyCurrentPointSet3D.MinX), (Y_max - Y_min) / (MyCurrentPointSet3D.MaxY - MyCurrentPointSet3D.MinY));
            BmpRegion = new RegionSize(0, (int)(BmpRegion.RightBound * ScaleSize), (int)(BmpRegion.TopBound * ScaleSize), 0);
            MyCurrentPointSet3D = smoothing(MyCurrentPointSet3D);
        }

        public void GetOriginalDate()
        {
            MyCurrentPointSet3D = MyOriginalPointSet3D;
            Renew_plot = true;
            IsSmoothed = false;
        }
        public void SmoothAndSave(string filename, RegionSize size, System.Drawing.Imaging.ImageFormat format, PixelFormat pixelFormat)
        {
            BSpline bs=new BSpline();
            MyCurrentPointSet3D = bs.BSpline3DofPepImage(MyOriginalPointSet3D, BmpRegion);
            SaveImage(filename, size, format, pixelFormat);
            
        }

        public void SmoothAndSave(string filename, RegionSize size)
        {
            SmoothAndSave(filename, size, System.Drawing.Imaging.ImageFormat.Tiff, PixelFormat.Format4bppIndexed);
        }

        public void SaveImage(string filename, RegionSize size, System.Drawing.Imaging.ImageFormat format, PixelFormat pixelFormat)
        {
            Bitmap bmp = new Bitmap(Convert.ToInt32(size.Width), Convert.ToInt32(size.Height), pixelFormat);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.White);

            this.ImageDraw(g, size);
            bmp.Save(filename, format);
            bmp.Dispose();
            g.Dispose();
            
        }
        public void SaveImage(string filename, RegionSize size)
        {
            SaveImage(filename, size, System.Drawing.Imaging.ImageFormat.Tiff, PixelFormat.Format16bppRgb555);
        }

        public void SaveImageOnlyData(string filename, RegionSize size, System.Drawing.Imaging.ImageFormat format)
        {
            bitmap1.Save(filename, format);
        }

        public void SaveImageOnlyData(string filename, RegionSize size)
        {
            SaveImageOnlyData(filename, size, System.Drawing.Imaging.ImageFormat.Tiff);
        }

        //#region IMessageSubject Members

        //public void Attach(IMessageObserver observer)
        //{
        //    _observers.Add(observer);
        //}

        //public void Detach(IMessageObserver observer)
        //{
        //    _observers.Remove(observer);
        //}

        //public void Notify()
        //{
        //    foreach (IMessageObserver observer in _observers)
        //    {
        //        observer.Update(this);
        //    }
        //}

        //#endregion
    }
}
