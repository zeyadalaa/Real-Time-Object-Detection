using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using AForge;
using AForge.Imaging.Filters;
using AForge.Imaging;
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Math.Geometry;

//Remove ambiguousness between AForge.Image and System.Drawing.Image
using Point = System.Drawing.Point; //Remove ambiguousness between AForge.Point and System.Drawing.Point

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        private FilterInfoCollection VideoCapTureDevices;
        private VideoCaptureDevice Finalvideo;


        public Form1()
        {
            InitializeComponent();
        }

        int R = 250; //variables of trackbar 
        int G = 250;
        int B = 250;
        int R1 = 250;
        int G1 = 250;
        int B1 = 250;
        int Rt;
        int Bt;
        int Gt;



        private void Form1_Load(object sender, EventArgs e)
        {
            VideoCapTureDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo VideoCaptureDevice in VideoCapTureDevices)
            {
                comboBox1.Items.Add(VideoCaptureDevice.Name);
            }

            comboBox1.SelectedIndex = 0;

        }


        private void button1_Click(object sender, EventArgs e)
        {
            Finalvideo = new VideoCaptureDevice(VideoCapTureDevices[comboBox1.SelectedIndex].MonikerString);
            Finalvideo.NewFrame += new NewFrameEventHandler(Finalvideo_NewFrame);
            Finalvideo.DesiredFrameRate = 20;//how many images per second you want. FPS
            Finalvideo.DesiredFrameSize = new Size(320, 240);//images size
            Finalvideo.Start();
        }

        void Finalvideo_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {


            Bitmap image = (Bitmap)eventArgs.Frame.Clone();
            Bitmap image1 = (Bitmap)eventArgs.Frame.Clone();
            Bitmap image2 = (Bitmap)eventArgs.Frame.Clone();
            Bitmap image3 = (Bitmap)eventArgs.Frame.Clone();
            pictureBox1.Image = image;




            EuclideanColorFiltering filter = new EuclideanColorFiltering();
            // set center color and radius
            filter.CenterColor = new RGB(Color.FromArgb(R, G, B));
            filter.Radius = 100;
            filter.ApplyInPlace(image1);

            EuclideanColorFiltering filter1 = new EuclideanColorFiltering();
            filter1.CenterColor = new RGB(Color.FromArgb(R1, G1, B1));
            filter1.Radius = 100;
            filter1.ApplyInPlace(image2);

            Add mfilter = new Add(image1);
            image3 = mfilter.Apply(image2);

            pictureBox2.Image = image3;
            findObjects(image1,image2);
        }

        public void findObjects(Bitmap image1,Bitmap image2) //find object(s)
        {
            BlobCounter blobCounter1 = new BlobCounter();
            blobCounter1.MinWidth = 5;
            blobCounter1.MinHeight = 5;
            blobCounter1.FilterBlobs = true;
            blobCounter1.ObjectsOrder = ObjectsOrder.Size;

            //----------------------------------

            BlobCounter blobCounter2 = new BlobCounter();
            blobCounter2.MinWidth = 5;
            blobCounter2.MinHeight = 5;
            blobCounter2.FilterBlobs = true;
            blobCounter2.ObjectsOrder = ObjectsOrder.Size;

            BitmapData objectsData1 = image1.LockBits(new Rectangle(0, 0, image1.Width, image1.Height), ImageLockMode.ReadOnly, image1.PixelFormat);
            // grayscaling
            Grayscale grayscaleFilter1 = new Grayscale(0.2125, 0.7154, 0.0721);
            UnmanagedImage grayImage1 = grayscaleFilter1.Apply(new UnmanagedImage(objectsData1));
            // unlock image
            image1.UnlockBits(objectsData1);

            BitmapData objectsData2 = image1.LockBits(new Rectangle(0, 0, image1.Width, image1.Height), ImageLockMode.ReadOnly, image1.PixelFormat);
            // grayscaling
            Grayscale grayscaleFilter2 = new Grayscale(0.2125, 0.7154, 0.0721);
            UnmanagedImage grayImage2 = grayscaleFilter2.Apply(new UnmanagedImage(objectsData2));
            // unlock image
            image1.UnlockBits(objectsData2);


            blobCounter1.ProcessImage(image1);
            Rectangle[] rects1 = blobCounter1.GetObjectsRectangles();
            Blob[] blobs1 = blobCounter1.GetObjectsInformation();

            blobCounter2.ProcessImage(image2);
            Rectangle[] rects2 = blobCounter2.GetObjectsRectangles();
            Blob[] blobs2 = blobCounter2.GetObjectsInformation();

            if (rdiobtnsingleobject.Checked)
            {
                //Single object Tracking--------
                Graphics g = pictureBox1.CreateGraphics();
                    if (rects1.Length > 0)
                    {
                        Rectangle objectRect1 = rects1[0];
                        using (Pen pen = new Pen(Color.FromArgb(252, 3, 26), 2))
                        {
                            g.DrawRectangle(pen, objectRect1);
                        }
                    }
                    if(rects2.Length > 0 )
                    {
                        Rectangle objectRect2 = rects2[0];
                        using (Pen pen = new Pen(Color.FromArgb(252, 3, 26), 2))
                        {
                            g.DrawRectangle(pen, objectRect2);
                        }
                    }
                g.Dispose();                   
            }



            if (rdiobtnMultipleObjects.Checked)
            {
                //Multi tracking-------
                for (int i = 0; rects1.Length > i; i++)
                {
                    Rectangle objectRect = rects1[i];
                    Graphics g = pictureBox1.CreateGraphics();
                    using (Pen pen = new Pen(Color.FromArgb(252, 3, 26), 2))
                    {
                        g.DrawRectangle(pen, objectRect);
                        g.DrawString((i + 1).ToString(), new Font("Arial", 12), Brushes.Red, objectRect);
                    }
                    int objectX = objectRect.X + (objectRect.Width / 2);
                    int objectY = objectRect.Y + (objectRect.Height / 2);
                    g.Dispose();

                }
            }

            if (rdiobtnGeoShape.Checked)
            {

                SimpleShapeChecker shapeChecker = new SimpleShapeChecker();

                int circles=0;
                int squares=0;
                int triangles=0;

                Graphics g = pictureBox1.CreateGraphics();
                Pen yellowPen = new Pen(Color.Yellow, 2); // circles
                Pen redPen = new Pen(Color.Red, 2);       // quadrilateral
                Pen brownPen = new Pen(Color.Brown, 2);   // quadrilateral with known sub-type
                Pen greenPen = new Pen(Color.Green, 2);   // known triangle
                Pen bluePen = new Pen(Color.Blue, 2);     // triangle
                for (int i = 0, n = blobs1.Length; i < n; i++)
                {
                    List<IntPoint> edgePoints = blobCounter1.GetBlobsEdgePoints(blobs1[i]);
                    AForge.Point center;
                    float radius;

                    // is circle ?
                    if (shapeChecker.IsCircle(edgePoints, out center, out radius))
                    {
                        g.DrawEllipse(yellowPen,
                            (float)(center.X - radius), (float)(center.Y - radius),
                            (float)(radius * 2), (float)(radius * 2));
                        circles++;
                    }
                    else
                    {   
                        List<IntPoint> corners;

                        // is triangle or quadrilateral
                        if (shapeChecker.IsConvexPolygon(edgePoints, out corners))
                        {
                            // get sub-type
                            PolygonSubType subType = shapeChecker.CheckPolygonSubType(corners);

                            Pen pen;

                            if (subType == PolygonSubType.Unknown)
                            {
                                if (corners.Count == 4)
                                {
                                    pen = redPen;
                                }
                                else
                                {
                                    pen = bluePen;
                                    triangles++;
                                }

                            }
                            else
                            {
                                if (corners.Count == 4)
                                {
                                    pen = brownPen;
                                    squares++;
                                }
                                else
                                {
                                    pen = greenPen;
                                    triangles++;
                                }
                            }

                            g.DrawPolygon(pen, ToPointsArray(corners));
                        }
                    }
                }
                g.DrawString("circles: "+circles.ToString() + " squares: " + squares.ToString()+" triangles: "+triangles.ToString(), new Font("Arial", 12), Brushes.Red, new System.Drawing.Point(0, 0));
                triangles = 0;
                circles = 0;
                squares = 0;
                yellowPen.Dispose();
                redPen.Dispose();
                greenPen.Dispose();
                bluePen.Dispose();
                brownPen.Dispose();
                g.Dispose();  
            }
        }

        // Conver list of AForge.NET's points to array of .NET points
        private Point[] ToPointsArray(List<IntPoint> points)
        {
            Point[] array = new Point[points.Count];

            for (int i = 0, n = points.Count; i < n; i++)
            {
                array[i] = new Point(points[i].X, points[i].Y);
            }

            return array;
        }

        private void button2_Click(object sender, EventArgs e)
        {

            if (Finalvideo.IsRunning)
            {
                Finalvideo.Stop();

            }
        }


        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            Rt = trackBar1.Value;
            label14.Location = new Point(529, Convert.ToInt32(435 - (Rt / 1.85)));
            label14.Text = Convert.ToString(Rt);
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            Gt = trackBar2.Value;
            label13.Location = new Point(589, Convert.ToInt32(435 - (Gt / 1.85)));
            label13.Text = Convert.ToString(Gt);
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            Bt = trackBar3.Value;
            label12.Location = new Point(656, Convert.ToInt32(435 - (Bt / 1.85)));
            label12.Text = Convert.ToString(Bt);
        }

        private void button3_Click(object sender, EventArgs e)
        {

            if (Finalvideo.IsRunning)
            {
                Finalvideo.Stop();
            }

            Application.Exit();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (rdiobtnRed.Checked)
            {
                R = 215;
                B = 0;
                G = 0;
            }

            if (rdiobtnBlue.Checked) 
            {
                R = 0;
                B = 140;
                G = 60;
            }
            if (rdiobtnGreen.Checked) 
            {
                R = 0;
                B = 60;
                G = 140;
            }


            if (rdbtnManual.Checked) 
            {
                R = Rt;
                B = Bt;
                G = Gt;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (rdiobtnRed.Checked)  
            {
                R1 = 215;
                B1 = 0;
                G1 = 0;
            }

            if (rdiobtnBlue.Checked) 
            {
                R1 = 0;
                B1 = 140;
                G1 = 60;
            }
            if (rdiobtnGreen.Checked)
            {
                R1 = 0;
                B1 = 60;
                G1 = 140;
            }
            if (rdbtnManual.Checked) 
            {
                R1 = Rt;
                B1 = Bt;
                G1 = Gt;
            }
        }
        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                Bitmap bmp = new Bitmap(pictureBox1.Image);
                Color c = bmp.GetPixel(e.X, e.Y);

                trackBar1.Value = c.R;
                Rt = trackBar1.Value;
                label14.Location = new Point(529, Convert.ToInt32(435 - (Rt / 1.85)));
                label14.Text = Convert.ToString(Rt);

                trackBar2.Value = c.G;
                Gt = trackBar2.Value;
                label13.Location = new Point(589, Convert.ToInt32(435 - (Gt / 1.85)));
                label13.Text = Convert.ToString(Gt);

                trackBar3.Value = c.B;
                Bt = trackBar3.Value;
                label12.Location = new Point(656, Convert.ToInt32(435 - (Bt / 1.85)));
                label12.Text = Convert.ToString(Bt);


            }
            catch (Exception ex)
            {

            }
        }


    }
}


