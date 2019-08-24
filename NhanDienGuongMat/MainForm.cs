﻿
//Multiple face detection and recognition in real time
//Using EmguCV cross platform .Net wrapper to the Intel OpenCV image processing library for C#.Net
//Writed by Sergio Andrés Guitérrez Rojas
//"Serg3ant" for the delveloper comunity
// Sergiogut1805@hotmail.com
//Regards from Bucaramanga-Colombia ;)

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.IO;
using System.Diagnostics;

using AForge.Video;
using AForge.Video.DirectShow;

namespace MultiFaceRec
{
    public partial class FrmPrincipal : Form
    {
        //Declararation of all variables, vectors and haarcascades
        Image<Bgr, Byte> currentFrame;
        Capture grabber;
        HaarCascade face;
        HaarCascade eye;
        MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_TRIPLEX, 0.5d, 0.5d);
        Image<Gray, byte> result, TrainedFace = null;
        Image<Gray, byte> gray = null;
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        List<string> labels= new List<string>();
        List<string> NamePersons = new List<string>();
        int ContTrain, NumLabels, t;
        string name, names = null;

        bool cohieu;
        private FilterInfoCollection dscamera;
        private VideoCaptureDevice cam;

        public FrmPrincipal()
        {
            InitializeComponent();
            btn_dung.Visible = false;
            btn_tieptuc.Visible = false;
            btn_tieptuc.Enabled = false;
            button2.Visible = false;

            dscamera = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach(FilterInfo i in dscamera)
            {
                cbx_mayanh.Items.Add(i.Name);
            }
            cbx_mayanh.SelectedIndex = 0;
            //MessageBox.Show(cbx_mayanh.Items[0].ToString());
            //Load haarcascades for face detection
            face = new HaarCascade("haarcascade_frontalface_default.xml");
            //eye = new HaarCascade("haarcascade_eye.xml");
            try
            {
                //Load of previus trainned faces and labels for each image
                string Labelsinfo = File.ReadAllText(Application.StartupPath + "/TrainedFaces/TrainedLabels.txt");
                string[] Labels = Labelsinfo.Split('%');
                NumLabels = Convert.ToInt16(Labels[0]);
                ContTrain = NumLabels;
                string LoadFaces;

                for (int tf = 1; tf < NumLabels+1; tf++)
                {
                    LoadFaces = "face" + tf + ".bmp";
                    trainingImages.Add(new Image<Gray, byte>(Application.StartupPath + "/TrainedFaces/" + LoadFaces));
                    labels.Add(Labels[tf]);
                }
            
            }
            catch(Exception e)
            {
                //MessageBox.Show(e.ToString());
                MessageBox.Show("Không có gì trong cơ sở dữ liệu nhị phân, vui lòng thêm ít nhất một khuôn mặt (Đơn giản chỉ cần huấn luyện nguyên mẫu bằng nút Thêm khuôn mặt).", "học gương mặt", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

        }

        private int kiemtra(string tencam)
        {
            int vitri=0,i=cbx_mayanh.Items.Count;

            for (int y = 0; y < i;y++ )
            {
                if (tencam == cbx_mayanh.Items[y].ToString())
                {
                    vitri = y;
                    break;
                }
            }
            
            return vitri;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if(cbx_mayanh.Text!="")
            {
                cam = new VideoCaptureDevice(dscamera[cbx_mayanh.SelectedIndex].MonikerString);
                
                //Initialize the capture device
                grabber = new Capture(kiemtra(cbx_mayanh.Text));
                grabber.QueryFrame();
                //Initialize the FrameGraber event
                Application.Idle += new EventHandler(FrameGrabber);
                button1.Enabled = false;
                cohieu = true;
                cbx_mayanh.Enabled = false;
                button1.Visible = false;
                btn_dung.Visible = true;
                btn_tieptuc.Visible = true;
                button2.Visible = true;
            }
            
        }


        private void button2_Click(object sender, System.EventArgs e)
        {
            try
            {
                if (textBox1.Text != "Tên người cần nhận diện")
                {
                    //Trained face counter
                    ContTrain = ContTrain + 1;

                    //Get a gray frame from capture device
                    gray = grabber.QueryGrayFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);

                    //Face Detector
                    MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(
                    face,
                    1.2,
                    10,
                    Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                    new Size(20, 20));

                    //Action for each element detected
                    foreach (MCvAvgComp f in facesDetected[0])
                    {
                        TrainedFace = currentFrame.Copy(f.rect).Convert<Gray, byte>();
                        break;
                    }

                    //resize face detected image for force to compare the same size with the 
                    //test image with cubic interpolation type method
                    TrainedFace = result.Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                    trainingImages.Add(TrainedFace);
                    labels.Add(textBox1.Text);

                    //Show face added in gray scale
                    imageBox1.Image = TrainedFace;

                    //Write the number of triained faces in a file text for further load
                    File.WriteAllText(Application.StartupPath + "/TrainedFaces/TrainedLabels.txt", trainingImages.ToArray().Length.ToString() + "%");

                    //Write the labels of triained faces in a file text for further load
                    for (int i = 1; i < trainingImages.ToArray().Length + 1; i++)
                    {
                        trainingImages.ToArray()[i - 1].Save(Application.StartupPath + "/TrainedFaces/face" + i + ".bmp");
                        File.AppendAllText(Application.StartupPath + "/TrainedFaces/TrainedLabels.txt", labels.ToArray()[i - 1] + "%");
                    }

                    MessageBox.Show(textBox1.Text + " gương mặt cẩn nhận diện đã được thêm vào thành công :)", "học gương mặt", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("vui lòng nhập tên của người cần nhận diện vào");
                }
            }
            catch
            {
                MessageBox.Show("không thể thêm gương mặt do thiếu ánh sáng hoặc không phải là gương mặt !!!", "học gương mặt", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }


        void FrameGrabber(object sender, EventArgs e)
        {
            label3.Text = "0";
            //label4.Text = "";
            NamePersons.Add("");


            //Get the current frame form capture device
            currentFrame = grabber.QueryFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);

                    //Convert it to Grayscale
                    gray = currentFrame.Convert<Gray, Byte>();

                    //Face Detector
                    MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(
                  face,
                  1.2,
                  10,
                  Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                  new Size(20, 20));

                    //Action for each element detected
                    foreach (MCvAvgComp f in facesDetected[0])
                    {
                        t = t + 1;
                        result = currentFrame.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                        //draw the face detected in the 0th (gray) channel with blue color
                        currentFrame.Draw(f.rect, new Bgr(Color.Red), 2);


                        if (trainingImages.ToArray().Length != 0)
                        {
                            //TermCriteria for face recognition with numbers of trained images like maxIteration
                        MCvTermCriteria termCrit = new MCvTermCriteria(ContTrain, 0.001);

                        //Eigen face recognizer
                        EigenObjectRecognizer recognizer = new EigenObjectRecognizer(
                           trainingImages.ToArray(),
                           labels.ToArray(),
                           3000,
                           ref termCrit);

                        name = recognizer.Recognize(result);

                            //Draw the label for each face detected and recognized
                        currentFrame.Draw(name, ref font, new Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.LightGreen));

                        }

                            NamePersons[t-1] = name;
                            NamePersons.Add("");


                        //Set the number of faces detected on the scene
                        label3.Text = facesDetected[0].Length.ToString();
                       
                        /*
                        //Set the region of interest on the faces
                        
                        gray.ROI = f.rect;
                        MCvAvgComp[][] eyesDetected = gray.DetectHaarCascade(
                           eye,
                           1.1,
                           10,
                           Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                           new Size(20, 20));
                        gray.ROI = Rectangle.Empty;

                        foreach (MCvAvgComp ey in eyesDetected[0])
                        {
                            Rectangle eyeRect = ey.rect;
                            eyeRect.Offset(f.rect.X, f.rect.Y);
                            currentFrame.Draw(eyeRect, new Bgr(Color.Blue), 2);
                        }
                         */

                    }
                        t = 0;

                        //Names concatenation of persons recognized
                    for (int nnn = 0; nnn < facesDetected[0].Length; nnn++)
                    {
                        names = names + NamePersons[nnn] + ", ";
                    }
                    //Show the faces procesed and recognized
                    imageBoxFrameGrabber.Image = currentFrame;
                    label4.Text = names;
                    if(label4.Text!="")
                    {
                        if(cohieu==true)
                        {
                            cohieu = false;
                            Process.Start("C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe", "https://www.facebook.com/");
                            btn_tieptuc.Enabled = true;
                            btn_dung.Enabled = false;
                        }
                    }
                    names = "";
                    //Clear the list(vector) of names
                    NamePersons.Clear();

                }

        private void button3_Click_1(object sender, EventArgs e)
        {
            cohieu = true;
            btn_tieptuc.Enabled = false;
            imageBoxFrameGrabber.Visible = true;
            btn_dung.Enabled = true;
            button2.Enabled = true;
        }

        private void btn_dung_Click(object sender, EventArgs e)
        {
            cohieu = false;
            imageBoxFrameGrabber.Visible = false;
            btn_tieptuc.Enabled = true;
            btn_dung.Enabled = false;
            button2.Enabled = false;
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            if(textBox1.Text=="Tên người cần nhận diện")
            {
                textBox1.Text = "";
            }
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                textBox1.Text = "Tên người cần nhận diện";
            }
        }

    }
}
