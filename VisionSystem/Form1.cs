﻿using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO.Ports;
using Emgu.CV;
using Emgu.CV.Structure;
using AForge.Video;

namespace VisionSystem
{
    public partial class AppWin : Form
    {
        #region CameraVariables
        VideoCapture cameraCapture;
        Image<Bgr, Byte> currentFrame;
        Image<Ycc, Byte> yccFrame = null;

        //MJPEGStream stream;
        MJPEGStream stream;
        string camera = "";
        int xLinear = 1;
        int yLinear = 1;
        int number = 3;
        string stop = "stop";
        string run = "run";
        //float scale = 0;
        int neighbour = 0;
        #endregion

        #region RobotVariables 
        string typeOfMove = "";
        #endregion

        #region Classifier
        // run 
        CascadeClassifier fistClassifier = new CascadeClassifier("C:/Emgu/emgucv-windesktop 3.4.1.2976/etc/haarcascades/fist.xml");
        // stop 
        CascadeClassifier palmClassifier = new CascadeClassifier("C:/Emgu/emgucv-windesktop 3.4.1.2976/etc/haarcascades/palm.xml");
        // increase speed
        CascadeClassifier testClassifier = new CascadeClassifier("C:/Users/Zajkos/source/VisionSystem/VisionSystem/classifier/rock-1h.xml");
        // decrease speed
        //CascadeClassifier okClassifier = new CascadeClassifier("C:/Users/Zajkos/source/VisionSystem/VisionSystem/classifier/gesture-rockNEW.xml");
        // xyz move typ

        // join move typ


        Rectangle[] rectanglesFist;
        Rectangle[] rectanglesPalm;
        Rectangle[] rectanglesTest;
        Rectangle[] rectanglesOk;
        #endregion



        #region Contructor
        public AppWin()
        {
            InitializeComponent();
            Size = new Size(1080, 720);
            StartPosition = FormStartPosition.CenterScreen;
            picBoxCameraView.SizeMode = PictureBoxSizeMode.StretchImage;
        }
        #endregion

        #region WindowSet
        private void picBoxCameraView_Paint(object sender, PaintEventArgs e)
        {
            int box_width = Convert.ToInt32(picBoxCameraView.Width);
            int box_height = Convert.ToInt32(picBoxCameraView.Height);
            int rectangle_width = Convert.ToInt32(box_width / 3.5);
            int rectangle_height = Convert.ToInt32(box_height / 3.5);

            Rectangle yp = new Rectangle(box_width / 2 - rectangle_width / 2, 1, rectangle_width, rectangle_height);
            Rectangle ym = new Rectangle(box_width / 2 - rectangle_width / 2, box_height - rectangle_height - 1, rectangle_width, rectangle_height);
            Rectangle xm = new Rectangle(1, box_height / 2 - rectangle_height / 2, rectangle_width, rectangle_height);
            Rectangle xp = new Rectangle(box_width - rectangle_width - 1, box_height / 2 - rectangle_height / 2, rectangle_width, rectangle_height);
            Rectangle zp = new Rectangle(box_width - rectangle_width - 1, 1, rectangle_width, rectangle_height);
            Rectangle zm = new Rectangle(1, box_height - rectangle_height - 1, rectangle_width, rectangle_height);
            Rectangle menu = new Rectangle(box_width / 2 - rectangle_width / 2, box_height / 2 - rectangle_height / 2, rectangle_width, rectangle_height);

            using (Pen pen = new Pen(Color.Blue, 1))
            {
                e.Graphics.DrawRectangle(pen, yp);
                e.Graphics.DrawRectangle(pen, ym);
            }
            using (Pen pen = new Pen(Color.Red, 1))
            {
                e.Graphics.DrawRectangle(pen, xp);
                e.Graphics.DrawRectangle(pen, xm);
            }
            using (Pen pen = new Pen(Color.Green, 1))
            {
                e.Graphics.DrawRectangle(pen, zp);
                e.Graphics.DrawRectangle(pen, zm);
            }
            using (Pen pen = new Pen(Color.Black, 1))
            {
                e.Graphics.DrawRectangle(pen, menu);

            }
        }
        #endregion

        #region WindowEvents
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            stopStream();
        }

        private void IPCamera_Click(object sender, EventArgs e)
        {
            camera = "ip";
        }

        private void EmbededCamera_Click(object sender, EventArgs e)
        {
            camera = "embeded";
        }

        private void stop_btn_Click(object sender, EventArgs e)
        {
            stopStream();
        }

        private void stopStream()
        {
            if (cameraCapture != null)
            {
                cameraCapture = null;
            }
        }

        private void start_btn_Click(object sender, EventArgs e)
        {
            if (camera == "embeded")
            {
                if (cameraCapture == null)
                {
                    cameraCapture = new VideoCapture(0);
                    cameraCapture.QueryFrame();
                    Application.Idle += Capture_ImageGrabbed;

                }
            }
            else if (camera == "ip")
            {
                stream = new MJPEGStream("http://192.168.0.80:4747/video");
                stream.NewFrame += Stream_NewFrame;
                stream.Start();
            }
        }

        private void Stream_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap bmp = (Bitmap)eventArgs.Frame.Clone();
            pictureBox1.Image = bmp;
        }

        private void Capture_ImageGrabbed(object sender, EventArgs e)
        {
            try
            {
                currentFrame = cameraCapture.QueryFrame().ToImage<Bgr, Byte>();
                yccFrame = currentFrame.Convert<Ycc, Byte>();
                pictureBox1.Image = yccFrame.ToBitmap();
                Image<Gray, Byte> grayFrame = yccFrame.Convert<Gray, Byte>();
                pictureBox2.Image = grayFrame.ToBitmap();
                
                rectanglesFist = fistClassifier.DetectMultiScale(grayFrame, scaleFactor: 1.2, minNeighbors: 12);
                rectanglesPalm = palmClassifier.DetectMultiScale(grayFrame, scaleFactor: 1.2, minNeighbors: 12);
                rectanglesTest = testClassifier.DetectMultiScale(grayFrame, scaleFactor: 1.10, minNeighbors: neighbour, minSize :default(Size), maxSize : default(Size));
                
                foreach (var rectangle in rectanglesFist)
                {
                    yccFrame.Draw(rectangle, new Ycc(122,222,12));
                   
                   
                    string moveTo = "";

                    if (rectangle.X > 0 & rectangle.X < 75 & rectangle.Y > 150 & rectangle.Y < 225)
                    {
                        richTextBox2.Text = String.Empty + "X minus";
                        moveTo = "1;1;" + typeOfMove + "; 00;01;00;00";
                        richTextBox2.Text = moveTo;

                    }
                    else if (rectangle.X > 425 & rectangle.X < 500 & rectangle.Y > 150 & rectangle.Y < 225)
                    {
                        richTextBox2.Text = String.Empty + "X plus";
                        moveTo = "1;1;" + typeOfMove + ";00;01;00";
                        richTextBox2.Text = moveTo;

                    }
                    else if (rectangle.X > 225 & rectangle.X < 300 & rectangle.Y > 300 & rectangle.Y < 375)
                    {
                        richTextBox2.Text = String.Empty + "y minus";
                        moveTo = "1;1;" + typeOfMove + ";00;02;00;00";
                        richTextBox2.Text = moveTo;

                    }
                    else if (rectangle.X > 225 & rectangle.X < 300 & rectangle.Y > 0 & rectangle.Y < 75)
                    {
                        richTextBox2.Text = String.Empty + "y plus";
                        moveTo = "1;1;" + typeOfMove + ";00;02;00";
                        richTextBox2.Text = moveTo;

                    }
                    else if (rectangle.X > 425 & rectangle.X < 500 & rectangle.Y > 0 & rectangle.Y < 75)
                    {
                        richTextBox2.Text = String.Empty + "z minus";
                        moveTo = "1;1;" + typeOfMove + ";00;04;00;00";
                        richTextBox2.Text = moveTo;

                    }
                    else if (rectangle.X > 0 & rectangle.X < 75 & rectangle.Y > 300 & rectangle.Y < 375)
                    {
                        richTextBox2.Text = String.Empty + "z plus";
                        moveTo = "1;1;" + typeOfMove + ";00;04;00";
                        richTextBox2.Text = moveTo;

                    }
                    else if (rectangle.X > 225 & rectangle.X < 300 & rectangle.Y > 150 & rectangle.Y < 225)
                    {
                        richTextBox2.Text = String.Empty + "menu";
                        servoOnBtn_Click(sender, e);

                    }

                    richTextBox3.Text = "FIST : x:" + rectangle.X + ",y:" + rectangle.Y;
                }

                foreach (var rectangle2 in rectanglesPalm)
                {
                   
                    yccFrame.Draw(rectangle2, new Ycc(1, 2, 255));

                    string moveTo = "";

                    if (rectangle2.X > 0 & rectangle2.X < 75 & rectangle2.Y >150 & rectangle2.Y <225)
                    {

                    } else if (rectangle2.X > 425 & rectangle2.X < 500 & rectangle2.Y > 150 & rectangle2.Y < 225)
                    {

                    }
                    else if (rectangle2.X > 225 & rectangle2.X < 300 & rectangle2.Y > 300 & rectangle2.Y < 375)
                    {
                        
                    }
                    else if (rectangle2.X > 225 & rectangle2.X < 300 & rectangle2.Y > 0 & rectangle2.Y < 75)
                    {
                        
                    }
                    else if (rectangle2.X > 425 & rectangle2.X < 500 & rectangle2.Y > 0 & rectangle2.Y < 75)
                    {
                    }
                    else if (rectangle2.X > 0 & rectangle2.X < 75 & rectangle2.Y > 300 & rectangle2.Y < 375)
                    {
                        

                    }
                    else if (rectangle2.X > 225 & rectangle2.X < 300 & rectangle2.Y > 150 & rectangle2.Y < 225)
                    {
                        servoOffBtn_Click(sender, e);

                    }

                    richTextBox3.Text = "PALM : x:" + rectangle2.X + ",y:" + rectangle2.Y;
                    
                }

                
                foreach (var rectangle3 in rectanglesTest)
                {

                    yccFrame.Draw(rectangle3, new Ycc(44, 44, 44));
                    
                    string moveTo = "";

                    if (rectangle3.X > 0 & rectangle3.X < 75 & rectangle3.Y > 150 & rectangle3.Y < 225)
                    {

                    }
                    else if (rectangle3.X > 425 & rectangle3.X < 500 & rectangle3.Y > 150 & rectangle3.Y < 225)
                    {

                    }
                    else if (rectangle3.X > 225 & rectangle3.X < 300 & rectangle3.Y > 300 & rectangle3.Y < 375)
                    {//decrease speed
                        trackBar1.Value = trackBar1.Value - 1;

                    }
                    else if (rectangle3.X > 225 & rectangle3.X < 300 & rectangle3.Y > 0 & rectangle3.Y < 75)
                    {//increase speed
                        trackBar1.Value = trackBar1.Value + 1;
                    }
                    else if (rectangle3.X > 425 & rectangle3.X < 500 & rectangle3.Y > 0 & rectangle3.Y < 75)
                    {
                    }
                    else if (rectangle3.X > 0 & rectangle3.X < 75 & rectangle3.Y > 300 & rectangle3.Y < 375)
                    {


                    }
                    else if (rectangle3.X > 225 & rectangle3.X < 300 & rectangle3.Y > 150 & rectangle3.Y < 225)
                    {
                        //apply speed 
                        trackBar1_Scroll(sender, e);

                    }

                    richTextBox3.Text = "TEST : x:" + rectangle3.X + "y:" + rectangle3.Y;

                    //}
                }
                trackBar3_Scroll(sender, e);

                Bitmap bitmap = yccFrame.ToBitmap();
                bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                picBoxCameraView.Image = yccFrame.ToBitmap();

            }
            catch (Exception)
            {

            }
        }
        #endregion

        public void whereIsHand()
        {


        }    

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            //open tab rozpoznawanie
        }

        private void rozpoznawaniePictureBox_Click(object sender, EventArgs e)
        {
            //add event after click open rozpoznawanie tab
        }

        private void parametryPictureBox_Click(object sender, EventArgs e)
        {
            //add event after click open parametry tab
        }

        private void ustawieniaPictureBox_Click(object sender, EventArgs e)
        {
            //add event after click open ustawenia tab
        }

        private void pomocPictureBox_Click(object sender, EventArgs e)
        {
            //add event after click open pomoc tab
        }

        private void servoOnBtn_Click(object sender, EventArgs e)
        {
            //R3 send servo off comand

            string openComunication = "1;1;OPEN=melfa " + "\r\n";
            string servoOff = "1;1;SRVON" + "\r\n";
            richTextBox2.Text = openComunication + servoOff;

        }

        private void servoOffBtn_Click(object sender, EventArgs e)
        {
            //R3 send servo off comand
            string servoOff = "1;1;SRVOFF" + "\r\n";
            string closeCommunication = "1;1;CLOSE" + "\r\n";
            richTextBox2.Text = servoOff + closeCommunication;
        }

        private void jointMoveBtn_Click(object sender, EventArgs e)
        {
            //R3 send joint move comand
            typeOfMove = "J00";
            richTextBox2.Text = typeOfMove;
        }

        private void xyzMoveBtn_Click(object sender, EventArgs e)
        {
            //R3 send xyz move comand
            typeOfMove = "J01";
            richTextBox2.Text = typeOfMove;

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            int actualSpeed = trackBar1.Value;
            string speed = "1;1;OVRD=" + actualSpeed + "\r\n";
            richTextBox2.Text =  speed;

        }


        private void glowne_Click(object sender, EventArgs e)
        {

        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
       //     scale = 1 + (trackBar3.Value / 100);
       //     richTextBox2.Text = scale + "," + neighbour;
       }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            neighbour = trackBar3.Value;
            //string xx = neighbour + "";
            //richTextBox2.Text = xx;
        }
    }
}
