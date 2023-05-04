using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace COL.ElutionViewer
{
    public partial class EluctionViewer : UserControl
    {
        EViewerHandler _eViewerHandler;
        private BufferedGraphicsContext _context;
        private BufferedGraphics _bufferedGraphics;
        public event EventHandler StatusUpdated;
        private bool X_click;
        private bool Y_click;
        private bool plot_click;
        private bool mouse_down;
        protected bool zoom_event;
        private float x_temp, x_mouse;
        private float y_temp, y_mouse;
        protected float X_min, X_max, Y_min, Y_max, Z_min, Z_max;
        private bool isGetAbundance;
     
        public EluctionViewer()
        {
            InitializeComponent();
            X_click = false;
            Y_click = false;
            plot_click = false;
            zoom_event = false;
            isGetAbundance = false;

            Graphics g = pictureBox1.CreateGraphics();
            
            _context = BufferedGraphicsManager.Current;

            _bufferedGraphics = _context.Allocate(g, new Rectangle(new Point(0, 0), pictureBox1.Size));
        }
        public void SetData(EViewerHandler argEViewHandler)
        {
            _eViewerHandler = argEViewHandler;
            this.Refresh();
        }
        public bool UseMousrGetAbundance
        {
            set
            {
                isGetAbundance = value;
            }
        }
        private void ReNewBufferedGraphics()
        {
            Graphics g = pictureBox1.CreateGraphics();
            _bufferedGraphics = _context.Allocate(g, new Rectangle(new Point(0, 0), pictureBox1.Size));
            _bufferedGraphics.Graphics.FillRectangle(new SolidBrush(pictureBox1.BackColor), new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height));

            if (_eViewerHandler != null)
            {
                _eViewerHandler.ImageDraw(_bufferedGraphics.Graphics, new RegionSize(0, pictureBox1.Width, pictureBox1.Height, 0));
            }
        }
        private void pictureBox1_Resize(object sender, EventArgs e)
        {

            if (_eViewerHandler != null)
            {
                this.Refresh();
            }

        }

        public void Smooth()
        {
            if (_eViewerHandler.IsSmoothed == true)
            {
                return;
            }
            else
            {
                _eViewerHandler.IsSmoothed = true;
               // _eViewerHandler.MyCurrentPointSet3D = _eViewerHandler.SmoothedPointSet3D;
                _eViewerHandler.MyCurrentPointSet3D = _eViewerHandler.smoothing(_eViewerHandler.MyCurrentPointSet3D);
                _eViewerHandler.Renew_plot = true;
                this.Refresh();
            }
        }

        public void GetoriginalData()
        {
            _eViewerHandler.GetOriginalDate();
            this.Refresh();
        }

        public void colorclick()
        {
            if (_eViewerHandler.IsColor)
            {
                return;
            }
            else
            {
                _eViewerHandler.SetColor();
                _eViewerHandler.Renew_plot = true;
                this.Refresh();
            }
        }

        public void grayclick()
        {
            if (_eViewerHandler.IsGray)
            {
                return;
            }
            else
            {
                _eViewerHandler.SetGray();
                _eViewerHandler.Renew_plot = true;
                this.Refresh();
            }

        }

        public void Scaling(int max, int min)
        {
            _eViewerHandler.IsScaled = true;
            _eViewerHandler.ScaleMax = max;
            _eViewerHandler.ScaleMin = min;
            _eViewerHandler.Renew_plot = true;
            this.Refresh();
            _eViewerHandler.IsScaled = false;
        }

        public void SaveImage(string filename)
        {
            _eViewerHandler.SaveImage(filename, new RegionSize(0, 800, 600, 0));

        }

        public void SaveImageRawData(string filename)
        {
            _eViewerHandler.SaveImageOnlyData(filename, new RegionSize(0, 800, 600, 0));
        }


        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (_eViewerHandler == null)
            {
                return;
            }
            if (e.X <= _eViewerHandler.Xstart && e.Y <= _eViewerHandler.Ystart && e.Y >= _eViewerHandler.Yend)//Click Y軸座標 
            {
                Y_click = true;
                y_temp = e.Y;
                mouse_down = true;
            }
            if (e.X >= _eViewerHandler.Xstart && e.X <= _eViewerHandler.Xend && e.Y >= _eViewerHandler.Ystart)//Click X軸座標
            {
                X_click = true;
                x_temp = e.X;
                mouse_down = true;
            }
            if (e.X >= _eViewerHandler.Xstart && e.X <= _eViewerHandler.Xend && e.Y <= _eViewerHandler.Ystart && e.Y >= _eViewerHandler.Yend) //Click Plot裡面
            {
                plot_click = true;
                x_temp = e.X;
                y_temp = e.Y;
                mouse_down = true;
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (_eViewerHandler == null)
            {
                return;
            }

            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                if (_eViewerHandler.IsColor)
                {
                    ((ToolStripMenuItem)contextMenuStrip1.Items["tsmColor"]).Checked = true;
                }

                contextMenuStrip1.Show(pictureBox1, e.X, e.Y);
            }

            if (mouse_down)
            {
                //up Y軸座標 
                if (e.X <= _eViewerHandler.Xstart && e.Y <= _eViewerHandler.Ystart && e.Y >= _eViewerHandler.Yend && e.Y != y_temp && Y_click == true)
                {
                    zoom_event = true;
                    mouse_down = false;
                    if (y_temp < e.Y)
                    {
                        Y_min = (float)(_eViewerHandler.MyCurrentPointSet3D.MinYwWhiteBoarder + ((_eViewerHandler.Ystart - e.Y) * (_eViewerHandler.MyCurrentPointSet3D.MaxYwWhiteBoarder - _eViewerHandler.MyCurrentPointSet3D.MinYwWhiteBoarder) / Math.Abs(_eViewerHandler.Yend - _eViewerHandler.Ystart)));
                        Y_max = (float)(_eViewerHandler.MyCurrentPointSet3D.MinYwWhiteBoarder + ((_eViewerHandler.Ystart - y_temp) * (_eViewerHandler.MyCurrentPointSet3D.MaxYwWhiteBoarder - _eViewerHandler.MyCurrentPointSet3D.MinYwWhiteBoarder) / Math.Abs(_eViewerHandler.Yend - _eViewerHandler.Ystart)));
                    }
                    else
                    {
                        Y_max = (float)(_eViewerHandler.MyCurrentPointSet3D.MinYwWhiteBoarder + ((_eViewerHandler.Ystart - e.Y) * (_eViewerHandler.MyCurrentPointSet3D.MaxYwWhiteBoarder - _eViewerHandler.MyCurrentPointSet3D.MinYwWhiteBoarder) / Math.Abs(_eViewerHandler.Yend - _eViewerHandler.Ystart)));
                        Y_min = (float)(_eViewerHandler.MyCurrentPointSet3D.MinYwWhiteBoarder + ((_eViewerHandler.Ystart - y_temp) * (_eViewerHandler.MyCurrentPointSet3D.MaxYwWhiteBoarder - _eViewerHandler.MyCurrentPointSet3D.MinYwWhiteBoarder) / Math.Abs(_eViewerHandler.Yend - _eViewerHandler.Ystart)));
                    }
                    X_min = _eViewerHandler.MyCurrentPointSet3D.MinXwWhiteBoarder;
                    X_max = _eViewerHandler.MyCurrentPointSet3D.MaxXwWhiteBoarder;
                }

                //up X軸座標
                if (e.X >= _eViewerHandler.Xstart && e.X <= _eViewerHandler.Xend && e.Y >= _eViewerHandler.Ystart && e.X != x_temp && X_click == true)
                {
                    //MessageBox.Show("UP X axis");
                    zoom_event = true;
                    mouse_down = false;
                    if (x_temp < e.X)
                    {
                        X_min = (float)(_eViewerHandler.MyCurrentPointSet3D.MinXwWhiteBoarder + ((x_temp - _eViewerHandler.Xstart) * (_eViewerHandler.MyCurrentPointSet3D.MaxXwWhiteBoarder - _eViewerHandler.MyCurrentPointSet3D.MinXwWhiteBoarder) / Math.Abs(_eViewerHandler.Xend - _eViewerHandler.Xstart)));
                        X_max = (float)(_eViewerHandler.MyCurrentPointSet3D.MinXwWhiteBoarder + ((e.X - _eViewerHandler.Xstart) * (_eViewerHandler.MyCurrentPointSet3D.MaxXwWhiteBoarder - _eViewerHandler.MyCurrentPointSet3D.MinXwWhiteBoarder) / Math.Abs(_eViewerHandler.Xend - _eViewerHandler.Xstart)));
                    }
                    else
                    {
                        X_min = (float)(_eViewerHandler.MyCurrentPointSet3D.MinXwWhiteBoarder + ((e.X - _eViewerHandler.Xstart) * (_eViewerHandler.MyCurrentPointSet3D.MaxXwWhiteBoarder - _eViewerHandler.MyCurrentPointSet3D.MinXwWhiteBoarder) / Math.Abs(_eViewerHandler.Xend - _eViewerHandler.Xstart)));
                        X_max = (float)(_eViewerHandler.MyCurrentPointSet3D.MinXwWhiteBoarder + ((x_temp - _eViewerHandler.Xstart) * (_eViewerHandler.MyCurrentPointSet3D.MaxXwWhiteBoarder - _eViewerHandler.MyCurrentPointSet3D.MinXwWhiteBoarder) / Math.Abs(_eViewerHandler.Xend - _eViewerHandler.Xstart)));
                    }
                    Y_min = _eViewerHandler.MyCurrentPointSet3D.MinYwWhiteBoarder;
                    Y_max = _eViewerHandler.MyCurrentPointSet3D.MaxYwWhiteBoarder;
                }

                //up Plot裡面
                if (e.X >= _eViewerHandler.Xstart && e.X <= _eViewerHandler.Xend && e.Y <= _eViewerHandler.Ystart && e.Y >= _eViewerHandler.Yend && e.X != x_temp && e.Y != y_temp && plot_click == true) 
                {
                    //MessageBox.Show("UP plot axis");
                    zoom_event = true;
                    mouse_down = false;
                    if (x_temp < e.X && y_temp > e.Y)
                    {
                        X_min = (float)(_eViewerHandler.MyCurrentPointSet3D.MinXwWhiteBoarder + ((x_temp - _eViewerHandler.Xstart) * (_eViewerHandler.MyCurrentPointSet3D.MaxXwWhiteBoarder - _eViewerHandler.MyCurrentPointSet3D.MinXwWhiteBoarder) / Math.Abs(_eViewerHandler.Xend - _eViewerHandler.Xstart)));
                        X_max = (float)(_eViewerHandler.MyCurrentPointSet3D.MinXwWhiteBoarder + ((e.X - _eViewerHandler.Xstart) * (_eViewerHandler.MyCurrentPointSet3D.MaxXwWhiteBoarder - _eViewerHandler.MyCurrentPointSet3D.MinXwWhiteBoarder) / Math.Abs(_eViewerHandler.Xend - _eViewerHandler.Xstart)));
                        Y_max = (float)(_eViewerHandler.MyCurrentPointSet3D.MinYwWhiteBoarder + ((_eViewerHandler.Ystart - e.Y) * (_eViewerHandler.MyCurrentPointSet3D.MaxYwWhiteBoarder - _eViewerHandler.MyCurrentPointSet3D.MinYwWhiteBoarder) / Math.Abs(_eViewerHandler.Yend - _eViewerHandler.Ystart)));
                        Y_min = (float)(_eViewerHandler.MyCurrentPointSet3D.MinYwWhiteBoarder + ((_eViewerHandler.Ystart - y_temp) * (_eViewerHandler.MyCurrentPointSet3D.MaxYwWhiteBoarder - _eViewerHandler.MyCurrentPointSet3D.MinYwWhiteBoarder) / Math.Abs(_eViewerHandler.Yend - _eViewerHandler.Ystart)));
                    }
                    if (x_temp < e.X && y_temp < e.Y)
                    {
                        X_min = (float)(_eViewerHandler.MyCurrentPointSet3D.MinXwWhiteBoarder + ((x_temp - _eViewerHandler.Xstart) * (_eViewerHandler.MyCurrentPointSet3D.MaxXwWhiteBoarder - _eViewerHandler.MyCurrentPointSet3D.MinXwWhiteBoarder) / Math.Abs(_eViewerHandler.Xend - _eViewerHandler.Xstart)));
                        X_max = (float)(_eViewerHandler.MyCurrentPointSet3D.MinXwWhiteBoarder + ((e.X - _eViewerHandler.Xstart) * (_eViewerHandler.MyCurrentPointSet3D.MaxXwWhiteBoarder - _eViewerHandler.MyCurrentPointSet3D.MinXwWhiteBoarder) / Math.Abs(_eViewerHandler.Xend - _eViewerHandler.Xstart)));
                        Y_min = (float)(_eViewerHandler.MyCurrentPointSet3D.MinYwWhiteBoarder + ((_eViewerHandler.Ystart - e.Y) * (_eViewerHandler.MyCurrentPointSet3D.MaxYwWhiteBoarder - _eViewerHandler.MyCurrentPointSet3D.MinYwWhiteBoarder) / Math.Abs(_eViewerHandler.Yend - _eViewerHandler.Ystart)));
                        Y_max = (float)(_eViewerHandler.MyCurrentPointSet3D.MinYwWhiteBoarder + ((_eViewerHandler.Ystart - y_temp) * (_eViewerHandler.MyCurrentPointSet3D.MaxYwWhiteBoarder - _eViewerHandler.MyCurrentPointSet3D.MinYwWhiteBoarder) / Math.Abs(_eViewerHandler.Yend - _eViewerHandler.Ystart))); ;
                    }
                    if (x_temp > e.X && y_temp > e.Y)
                    {
                        X_min = (float)(_eViewerHandler.MyCurrentPointSet3D.MinXwWhiteBoarder + ((e.X - _eViewerHandler.Xstart) * (_eViewerHandler.MyCurrentPointSet3D.MaxXwWhiteBoarder - _eViewerHandler.MyCurrentPointSet3D.MinXwWhiteBoarder) / Math.Abs(_eViewerHandler.Xend - _eViewerHandler.Xstart)));
                        X_max = (float)(_eViewerHandler.MyCurrentPointSet3D.MinXwWhiteBoarder + ((x_temp - _eViewerHandler.Xstart) * (_eViewerHandler.MyCurrentPointSet3D.MaxXwWhiteBoarder - _eViewerHandler.MyCurrentPointSet3D.MinXwWhiteBoarder) / Math.Abs(_eViewerHandler.Xend - _eViewerHandler.Xstart)));
                        Y_max = (float)(_eViewerHandler.MyCurrentPointSet3D.MinYwWhiteBoarder + ((_eViewerHandler.Ystart - e.Y) * (_eViewerHandler.MyCurrentPointSet3D.MaxYwWhiteBoarder - _eViewerHandler.MyCurrentPointSet3D.MinYwWhiteBoarder) / Math.Abs(_eViewerHandler.Yend - _eViewerHandler.Ystart)));
                        Y_min = (float)(_eViewerHandler.MyCurrentPointSet3D.MinYwWhiteBoarder + ((_eViewerHandler.Ystart - y_temp) * (_eViewerHandler.MyCurrentPointSet3D.MaxYwWhiteBoarder - _eViewerHandler.MyCurrentPointSet3D.MinYwWhiteBoarder) / Math.Abs(_eViewerHandler.Yend - _eViewerHandler.Ystart)));
                    }
                    if (x_temp > e.X && y_temp < e.Y)
                    {
                        X_min = (float)(_eViewerHandler.MyCurrentPointSet3D.MinXwWhiteBoarder + ((e.X - _eViewerHandler.Xstart) * (_eViewerHandler.MyCurrentPointSet3D.MaxXwWhiteBoarder - _eViewerHandler.MyCurrentPointSet3D.MinXwWhiteBoarder) / Math.Abs(_eViewerHandler.Xend - _eViewerHandler.Xstart)));
                        X_max = (float)(_eViewerHandler.MyCurrentPointSet3D.MinXwWhiteBoarder + ((x_temp - _eViewerHandler.Xstart) * (_eViewerHandler.MyCurrentPointSet3D.MaxXwWhiteBoarder - _eViewerHandler.MyCurrentPointSet3D.MinXwWhiteBoarder) / Math.Abs(_eViewerHandler.Xend - _eViewerHandler.Xstart)));
                        Y_min = (float)(_eViewerHandler.MyCurrentPointSet3D.MinYwWhiteBoarder + ((_eViewerHandler.Ystart - e.Y) * (_eViewerHandler.MyCurrentPointSet3D.MaxYwWhiteBoarder - _eViewerHandler.MyCurrentPointSet3D.MinYwWhiteBoarder) / Math.Abs(_eViewerHandler.Yend - _eViewerHandler.Ystart)));
                        Y_max = (float)(_eViewerHandler.MyCurrentPointSet3D.MinYwWhiteBoarder + ((_eViewerHandler.Ystart - y_temp) * (_eViewerHandler.MyCurrentPointSet3D.MaxYwWhiteBoarder - _eViewerHandler.MyCurrentPointSet3D.MinYwWhiteBoarder) / Math.Abs(_eViewerHandler.Yend - _eViewerHandler.Ystart)));
                    }  

                }
                if (X_max - X_min <= 0.01 || Y_max - Y_min <= 0.5)
                {
                    zoom_event = false;
                }
                if (zoom_event && !isGetAbundance)
                {
                    _eViewerHandler.Renew_plot = true;
                    if (_eViewerHandler.IsSmoothed)
                    {
                        _eViewerHandler.SmoothingofSmoothed(X_min, X_max, Y_max, Y_min);
                    }
                    else
                    {
                        _eViewerHandler.GetSubPointSet(new RegionSize(X_min, X_max, Y_max, Y_min));
                    }
                    X_click = false;
                    Y_click = false;
                    plot_click = false;
                }
                else
                {
                    if (X_max - X_min >= 0.01 && Y_max - Y_min >= 0.5)
                    {
                        MSPointSet3D tmp3Dset = _eViewerHandler.SubPointSetExtract(new RegionSize(X_min, X_max, Y_max, Y_min));
                        float SumIntensity = 0.0f;
                        Dictionary<float, MSPointSet> Intergral = new Dictionary<float, MSPointSet>();
                        for (int i = 0; i < tmp3Dset.Count; i++)
                        {
                            float mz = tmp3Dset.Y(i);
                            float time = tmp3Dset.X(i);
                            float intensity = tmp3Dset.Z(i);
                            SumIntensity = SumIntensity + intensity;
                            if (!Intergral.ContainsKey(mz))
                            {
                                Intergral.Add(mz, new MSPointSet());
                            }
                            Intergral[mz].Add(time, intensity);
                        }
                        double area = 0;
                        foreach (float mzKey in Intergral.Keys)
                        {
                            if (Intergral[mzKey].Count <= 3)
                            {
                                continue;
                            }
                            Intergral[mzKey].Sort();
                            area = IntegralArea(Intergral[mzKey]);
                        }
                        MessageBox.Show("Time:" + X_min.ToString() + "~" + X_max.ToString() + "\n" +
                                   "m/z:" + Y_min.ToString() + "~" + Y_max.ToString() + "\n" +
                                   "Sum Intensity:" + SumIntensity.ToString() + "\n" +
                                   "Area:" + area.ToString());
                    }
                    X_click = false;
                    Y_click = false;
                    plot_click = false;

                }

                zoom_event = false;
                if (this.StatusUpdated != null)
                {
                    this.StatusUpdated(new object(), new RegionSize(X_min, X_max, Y_max, Y_min));
                }
                this.Refresh();
            }

        }
        private double IntegralArea(MSPointSet argMSP)
        {
            double AreaOfCurve = 0.0;

            for (int i = 0; i < argMSP.Count - 1; i++)
            {
                AreaOfCurve = AreaOfCurve + ((argMSP.X(i + 1) - argMSP.X(i)) * ((argMSP.Y(i + 1) + argMSP.Y(i)) / 2));

            }
            return AreaOfCurve;
        }
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouse_down)
            {
                x_mouse = e.X;
                y_mouse = e.Y;
                this.Refresh();
            }
        }

        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            _eViewerHandler.initialize();
            mouse_down = false;
            Y_click = false;
            X_click = false;
            plot_click = false;
            this.Refresh();
        }

        private void ShowPaint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            if (mouse_down == false)
            {
                ReNewBufferedGraphics();

                X_click = false;
                Y_click = false;
                plot_click = false;
                zoom_event = false;
            }

            _bufferedGraphics.Render(g);

            SolidBrush figure_brush = new SolidBrush(Color.Red);
            Pen Insidepen = new Pen(Color.Red, 3);
            Font font = new Font("Time New Roman", 8, FontStyle.Bold);


            //if (_eViewerHandler.IsError)
            //{
            //    MessageBox.Show("The data you selected aren't enough for drawing, please reselect again.");
            //    _eViewerHandler.IsError = false;
            //}
             

            if (X_click)
            {
                g.DrawLine(Insidepen, x_mouse, _eViewerHandler.Ystart, x_temp, _eViewerHandler.Ystart);
                g.DrawString("(" + Convert.ToString(Math.Round((_eViewerHandler.MyCurrentPointSet3D.MinXwWhiteBoarder + ((x_temp - _eViewerHandler.Xstart) * (_eViewerHandler.MyCurrentPointSet3D.MaxXwWhiteBoarder - _eViewerHandler.MyCurrentPointSet3D.MinXwWhiteBoarder) / Math.Abs(_eViewerHandler.Xend - _eViewerHandler.Xstart))), 2)) + ")", font, figure_brush, (x_temp - 10), (_eViewerHandler.Ystart + 5));
                g.DrawString("(" + Convert.ToString(Math.Round((_eViewerHandler.MyCurrentPointSet3D.MinXwWhiteBoarder + ((x_mouse - _eViewerHandler.Xstart) * (_eViewerHandler.MyCurrentPointSet3D.MaxXwWhiteBoarder - _eViewerHandler.MyCurrentPointSet3D.MinXwWhiteBoarder) / Math.Abs(_eViewerHandler.Xend - _eViewerHandler.Xstart))), 2)) + ")", font, figure_brush, (x_mouse - 10), (_eViewerHandler.Ystart + 5));

            }
            if (Y_click)
            {
                g.DrawLine(Insidepen, _eViewerHandler.Xstart, y_mouse, _eViewerHandler.Xstart, y_temp);
                g.DrawString("(" + Convert.ToString(Math.Round((_eViewerHandler.MyCurrentPointSet3D.MinYwWhiteBoarder + ((_eViewerHandler.Ystart - y_temp) * (_eViewerHandler.MyCurrentPointSet3D.MaxYwWhiteBoarder - _eViewerHandler.MyCurrentPointSet3D.MinYwWhiteBoarder) / Math.Abs(_eViewerHandler.Yend - _eViewerHandler.Ystart))), 2)) + ")", font, figure_brush, (_eViewerHandler.Xstart + 10), (y_temp));
                g.DrawString("(" + Convert.ToString(Math.Round((_eViewerHandler.MyCurrentPointSet3D.MinYwWhiteBoarder + ((_eViewerHandler.Ystart - y_mouse) * (_eViewerHandler.MyCurrentPointSet3D.MaxYwWhiteBoarder - _eViewerHandler.MyCurrentPointSet3D.MinYwWhiteBoarder) / Math.Abs(_eViewerHandler.Yend - _eViewerHandler.Ystart))), 2)) + ")", font, figure_brush, (_eViewerHandler.Xstart + 10), (y_mouse));

            }
            if (plot_click)
            {
                if (x_temp < x_mouse && y_temp > y_mouse)
                {
                    g.DrawRectangle(Insidepen, x_temp, y_mouse, x_mouse - x_temp, y_temp - y_mouse);
                }
                if (x_temp < x_mouse && y_temp < y_mouse)
                {
                    g.DrawRectangle(Insidepen, x_temp, y_temp, x_mouse - x_temp, y_mouse - y_temp);
                }
                if (x_temp > x_mouse && y_temp > y_mouse)
                {
                    g.DrawRectangle(Insidepen, x_mouse, y_mouse, x_temp - x_mouse, y_temp - y_mouse);
                }
                if (x_temp > x_mouse && y_temp < y_mouse)
                {
                    g.DrawRectangle(Insidepen, x_mouse, y_temp, x_temp - x_mouse, y_mouse - y_temp);
                }
                g.DrawString("(" + Convert.ToString(Math.Round((_eViewerHandler.MyCurrentPointSet3D.MinXwWhiteBoarder + ((x_temp - _eViewerHandler.Xstart) * (_eViewerHandler.MyCurrentPointSet3D.MaxXwWhiteBoarder - _eViewerHandler.MyCurrentPointSet3D.MinXwWhiteBoarder) / Math.Abs(_eViewerHandler.Xend - _eViewerHandler.Xstart))), 2)) + " , " +
                                   Convert.ToString(Math.Round((_eViewerHandler.MyCurrentPointSet3D.MinYwWhiteBoarder + ((_eViewerHandler.Ystart - y_temp) * (_eViewerHandler.MyCurrentPointSet3D.MaxYwWhiteBoarder - _eViewerHandler.MyCurrentPointSet3D.MinYwWhiteBoarder) / Math.Abs(_eViewerHandler.Yend - _eViewerHandler.Ystart))), 2)) + ")", font, figure_brush, x_temp, y_temp);
                g.DrawString("(" + Convert.ToString(Math.Round((_eViewerHandler.MyCurrentPointSet3D.MinXwWhiteBoarder + ((x_mouse - _eViewerHandler.Xstart) * (_eViewerHandler.MyCurrentPointSet3D.MaxXwWhiteBoarder - _eViewerHandler.MyCurrentPointSet3D.MinXwWhiteBoarder) / Math.Abs(_eViewerHandler.Xend - _eViewerHandler.Xstart))), 2)) + " , " +
                                   Convert.ToString(Math.Round((_eViewerHandler.MyCurrentPointSet3D.MinYwWhiteBoarder + ((_eViewerHandler.Ystart - y_mouse) * (_eViewerHandler.MyCurrentPointSet3D.MaxYwWhiteBoarder - _eViewerHandler.MyCurrentPointSet3D.MinYwWhiteBoarder) / Math.Abs(_eViewerHandler.Yend - _eViewerHandler.Ystart))), 2)) + ")", font, figure_brush, x_mouse, y_mouse);
            }
        }
        public void Reset()
        {
            _eViewerHandler.initialize();
            mouse_down = false;
            Y_click = false;
            X_click = false;
            plot_click = false;
            this.Refresh();
        }
        private void tsmReset_Click(object sender, EventArgs e)
        {
            _eViewerHandler.initialize();
            mouse_down = false;
            Y_click = false;
            X_click = false;
            plot_click = false;
            this.Refresh();
        }

        private void tsmSave_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "Image files (*.bmp)|*.bmp";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.SaveImage(saveFileDialog1.FileName);
            }
        }

        private void tsmColor_Click(object sender, EventArgs e)
        {
            if (_eViewerHandler.IsColor)
            {
                _eViewerHandler.SetGray();
            }
            else
            {
                _eViewerHandler.SetColor();
            }
            _eViewerHandler.initialize();
            mouse_down = false;
            Y_click = false;
            X_click = false;
            plot_click = false;
            this.Refresh();
        }

        private void tsmSmooth_Click(object sender, EventArgs e)
        {
            if (!_eViewerHandler.IsSmoothed)
            {
                this.Smooth();
                this.Refresh();
            }
        }

    }
}
