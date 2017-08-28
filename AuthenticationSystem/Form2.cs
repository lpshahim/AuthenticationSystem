using Leap;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System;
using System.Threading;
using System.Security.Cryptography;
using System.Text;
namespace AuthenticationSystem
{
    public partial class Form2 : Form
    {

        #region MyVariablesRegion
        //GLOBAL VARIABLES
        /// <summary>

        private byte[] imagedata = new byte[1];
        private Controller controller = new Controller();
        Bitmap bitmap = new Bitmap(640, 480, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
        //Bitmap bitmap = new Bitmap(196, 246, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

        int timeLeft;
        int fingerReadingCount = 0;
        int boneReadingCount = 0;
        int indexBonesCount, middleBonesCount, ringBonesCount, pinkyBonesCount, thumbBonesCount = 0;
        int thumbMetaCount, thumbProxCount, thumbIntCount = 0;
        int indexMetaCount, indexProxCount, indexIntCount, indexDistCount = 0;
        int middleMetaCount, middleProxCount, middleIntCount, middleDistCount = 0;
        int ringMetaCount, ringProxCount, ringIntCount, ringDistCount = 0;
        int pinkyMetaCount, pinkyProxCount, pinkyIntCount, pinkyDistCount = 0;


        private decimal[] thumbMeasurements = new decimal[3];
        private decimal[] pinkyMeasurements = new decimal[4];
        private decimal[] indexMeasurements = new decimal[4];
        private decimal[] middleMeasurements = new decimal[4];
        private decimal[] ringMeasurements = new decimal[4];

        //temp arrays of strings
        //string[] tempBoneMeasurements;
        //string[] tempFingerMeasurements;
        //List<String> tempBoneMeasurements = new List<String>();

        ArrayList thumbBoneList = new ArrayList();
        ArrayList indexBoneList = new ArrayList();
        ArrayList middleBoneList = new ArrayList();
        ArrayList ringBoneList = new ArrayList();
        ArrayList pinkyBoneList = new ArrayList();

        //THUMB
        ArrayList thumbMetaList = new ArrayList();
        ArrayList thumbProxList = new ArrayList();
        ArrayList thumbIntList = new ArrayList();

        //INDEX
        ArrayList indexMetaList = new ArrayList();
        ArrayList indexProxList = new ArrayList();
        ArrayList indexIntList = new ArrayList();
        ArrayList indexDistList = new ArrayList();

        //MIDDLE
        ArrayList middleMetaList = new ArrayList();
        ArrayList middleProxList = new ArrayList();
        ArrayList middleIntList = new ArrayList();
        ArrayList middleDistList = new ArrayList();

        //RING
        ArrayList ringMetaList = new ArrayList();
        ArrayList ringProxList = new ArrayList();
        ArrayList ringIntList = new ArrayList();
        ArrayList ringDistList = new ArrayList();

        //PINKY
        ArrayList pinkyMetaList = new ArrayList();
        ArrayList pinkyProxList = new ArrayList();
        ArrayList pinkyIntList = new ArrayList();
        ArrayList pinkyDistList = new ArrayList();


        //HASH TABLES INSTEAD OF ARRAYLIST

        //THUMB
        Hashtable thumbMetaTable = new Hashtable();
        Hashtable thumbProxTable = new Hashtable();
        Hashtable thumbIntTable = new Hashtable();

        //INDEX
        Hashtable indexMetaTable = new Hashtable();
        Hashtable indexProxTable = new Hashtable();
        Hashtable indexIntTable = new Hashtable();
        Hashtable indexDistTable = new Hashtable();

        //MIDDLE
        Hashtable middleMetaTable = new Hashtable();
        Hashtable middleProxTable = new Hashtable();
        Hashtable middleIntTable = new Hashtable();
        Hashtable middleDistTable = new Hashtable();

        //RING
        Hashtable ringMetaTable = new Hashtable();
        Hashtable ringProxTable = new Hashtable();
        Hashtable ringIntTable = new Hashtable();
        Hashtable ringDistTable = new Hashtable();

        //PINKY
        Hashtable pinkyMetaTable = new Hashtable();
        Hashtable pinkyProxTable = new Hashtable();
        Hashtable pinkyIntTable = new Hashtable();
        Hashtable pinkyDistTable = new Hashtable();

        //file writing variables
        string userFileName;
        string directory;
        string bonesFilePath;
        string fingersFilePath;
        string bonesCSVPath;
        string fingersCSVPath;
        string matrixCSVPath;
        //******************** 
        #endregion
            
        public Form2()
        {
            InitializeComponent();
            //INITIALIZE CONTROLLER
            controller.EventContext = WindowsFormsSynchronizationContext.Current;
            //controller.FrameReady += newFrameHandler;
            controller.ImageReady += onImageReady;
            controller.ImageRequestFailed += onImageRequestFailed;

            //set greyscale palette for image Bitmap object
            ColorPalette grayscale = bitmap.Palette;
            for (int i = 0; i < 256; i++)
            {
                grayscale.Entries[i] = Color.FromArgb((int)255, i, i, i);
            }
            bitmap.Palette = grayscale;
            //********************
        }

        //LMC FRAME HANDLER
        void newFrameHandler(object sender, FrameEventArgs eventArgs)
        {
            Frame frame = eventArgs.frame;
            //The following are Label controls added in design view for the form
            //this.displayID.Text = frame.Id.ToString();
            //this.displayTimestamp.Text = frame.Timestamp.ToString();
            this.lblFPS.Text = frame.CurrentFramesPerSecond.ToString();
            // this.displayHandCount.Text = frame.Hands.Count.ToString(); 

            controller.RequestImages(frame.Id, Leap.Image.ImageType.DEFAULT, imagedata);

            //hand data
            string tempFingerMeasurements = "";
            //string tempBoneMeasurements = "";

            //boneWriter = new StreamWriter(bonesCSVPath);
            //fingerWriter = new StreamWriter(fingersCSVPath);

            List<Hand> allHands = frame.Hands;

            foreach (Hand hand in allHands)
            {
                if (hand.IsRight == true)
                {
                    List<Finger> fingers = hand.Fingers;
                    foreach (Finger finger in fingers)
                    {

                        tempFingerMeasurements = finger.Type.ToString() + "," + finger.Length.ToString() + "," + finger.Width.ToString();
                        using (StreamWriter fingerWriter = new StreamWriter(fingersCSVPath, true))
                        {
                            fingerWriter.WriteLine(tempFingerMeasurements);

                            //countReadings after write
                            fingerReadingCount++;

                            fingerWriter.Close();
                        }

                        //Console.Write(temp);
                        //Console.Write(finger.Type + "\n" + finger.Length + "\n" + finger.Width);

                        String ftype = finger.Type.ToString();
                        int fingerID = 0;

                        switch (ftype)
                        {
                            case "TYPE_INDEX":
                                fingerID = 1;
                                break;
                            case "TYPE_MIDDLE":
                                fingerID = 2;
                                break;
                            case "TYPE_RING":
                                fingerID = 3;
                                break;
                            case "TYPE_PINKY":
                                fingerID = 4;
                                break;
                            case "TYPE_THUMB":
                                fingerID = 5;
                                break;
                        }

                        int countBones = 0;

                        foreach (Bone.BoneType boneType in (Bone.BoneType[])Enum.GetValues(typeof(Bone.BoneType)))
                        {

                            if (boneType >= 0)
                            {
                                Bone boneLength = finger.Bone(boneType);

                                string typeOfBone = boneLength.Type.ToString();

                                switch (fingerID)
                                {
                                    case 1:
                                        indexMeasurements[countBones] = (decimal)boneLength.Length;// + (decimal)boneLength.Width;
                                        indexBoneList.Add(boneLength.Type + ", " + indexMeasurements[countBones]);
                                        //Console.Write(boneList[countBones]);
                                        indexBonesCount++;
                                        //CASE CHECK MIDDLE BONE AND WRITE TO HASHTABLE
                                        switch (typeOfBone)
                                        {
                                            case "TYPE_METACARPAL":
                                                indexMetaCount++;
                                                indexMetaTable.Add(indexBonesCount, indexMeasurements[countBones]);
                                                indexMetaList.Add(indexMeasurements[countBones]);

                                                break;

                                            case "TYPE_PROXIMAL":
                                                indexProxCount++;
                                                indexProxTable.Add(indexBonesCount, indexMeasurements[countBones]);
                                                indexProxList.Add(indexMeasurements[countBones]);
                                                break;

                                            case "TYPE_INTERMEDIATE":
                                                indexIntCount++;
                                                indexIntTable.Add(indexBonesCount, indexMeasurements[countBones]);
                                                indexIntList.Add(indexMeasurements[countBones]);
                                                break;

                                            case "TYPE_DISTAL":
                                                indexDistCount++;
                                                indexDistTable.Add(indexBonesCount, indexMeasurements[countBones]);
                                                indexDistList.Add(indexMeasurements[countBones]);
                                                break;
                                        }

                                        using (StreamWriter boneWriter = new StreamWriter(bonesCSVPath, true))
                                        {
                                            boneWriter.WriteLine(boneLength.Type + ", Index, " + indexMeasurements[countBones]);
                                            //indexBonesCount++;
                                            boneReadingCount++;
                                            boneWriter.Close();
                                        }
                                        break;
                                    case 2:
                                        middleMeasurements[countBones] = (decimal)boneLength.Length;// + (decimal)boneLength.Width;
                                        middleBoneList.Add(boneLength.Type + ", " + middleMeasurements[countBones]);
                                        middleBonesCount++;
                                        //CASE CHECK MIDDLE BONE AND WRITE TO HASHTABLE
                                        switch (typeOfBone)
                                        {
                                            case "TYPE_METACARPAL":
                                                middleMetaCount++;
                                                middleMetaTable.Add(middleBonesCount, middleMeasurements[countBones]);
                                                middleMetaList.Add(middleMeasurements[countBones]);
                                                break;

                                            case "TYPE_PROXIMAL":
                                                middleProxCount++;
                                                middleProxTable.Add(middleBonesCount, middleMeasurements[countBones]);
                                                middleProxList.Add(middleMeasurements[countBones]);
                                                break;

                                            case "TYPE_INTERMEDIATE":
                                                middleIntCount++;
                                                middleIntTable.Add(middleBonesCount, middleMeasurements[countBones]);
                                                middleIntList.Add(middleMeasurements[countBones]);
                                                break;

                                            case "TYPE_DISTAL":
                                                middleDistCount++;
                                                middleDistTable.Add(middleBonesCount, middleMeasurements[countBones]);
                                                middleDistList.Add(middleMeasurements[countBones]);
                                                break;
                                        }

                                        using (StreamWriter boneWriter = new StreamWriter(bonesCSVPath, true))
                                        {
                                            boneWriter.WriteLine(boneLength.Type + ", Middle, " + middleMeasurements[countBones]);
                                            //middleBonesCount++;
                                            boneReadingCount++;
                                            boneWriter.Close();
                                        }
                                        break;
                                    case 3:
                                        ringMeasurements[countBones] = (decimal)boneLength.Length;// + (decimal)boneLength.Width;
                                        ringBoneList.Add(boneLength.Type + ", " + ringMeasurements[countBones]);
                                        ringBonesCount++;
                                        //CASE CHECK RING BONE AND WRITE TO HASHTABLE
                                        switch (typeOfBone)
                                        {
                                            case "TYPE_METACARPAL":
                                                ringMetaCount++;
                                                ringMetaTable.Add(ringBonesCount, ringMeasurements[countBones]);
                                                ringMetaList.Add(ringMeasurements[countBones]);
                                                break;

                                            case "TYPE_PROXIMAL":
                                                ringProxCount++;
                                                ringProxTable.Add(ringBonesCount, ringMeasurements[countBones]);
                                                ringProxList.Add(ringMeasurements[countBones]);
                                                break;

                                            case "TYPE_INTERMEDIATE":
                                                ringIntCount++;
                                                ringIntTable.Add(ringBonesCount, ringMeasurements[countBones]);
                                                ringIntList.Add(ringMeasurements[countBones]);
                                                break;

                                            case "TYPE_DISTAL":
                                                ringDistCount++;
                                                ringDistTable.Add(ringBonesCount, ringMeasurements[countBones]);
                                                ringDistList.Add(ringMeasurements[countBones]);
                                                break;
                                        }

                                        using (StreamWriter boneWriter = new StreamWriter(bonesCSVPath, true))
                                        {
                                            boneWriter.WriteLine(boneLength.Type + ", Ring, " + ringMeasurements[countBones]);
                                            //ringBonesCount++;
                                            boneReadingCount++;
                                            boneWriter.Close();
                                        }
                                        break;
                                    case 4:
                                        pinkyMeasurements[countBones] = (decimal)boneLength.Length;// + (decimal)boneLength.Width;
                                        pinkyBoneList.Add(boneLength.Type + ", " + pinkyMeasurements[countBones]);
                                        pinkyBonesCount++;
                                        //CASE CHECK pinky BONE AND WRITE TO HASHTABLE
                                        switch (typeOfBone)
                                        {
                                            case "TYPE_METACARPAL":
                                                pinkyMetaCount++;
                                                pinkyMetaTable.Add(pinkyBonesCount, pinkyMeasurements[countBones]);
                                                pinkyMetaList.Add(pinkyMeasurements[countBones]);
                                                break;

                                            case "TYPE_PROXIMAL":
                                                pinkyProxCount++;
                                                pinkyProxTable.Add(pinkyBonesCount, pinkyMeasurements[countBones]);
                                                pinkyProxList.Add(pinkyMeasurements[countBones]);
                                                break;

                                            case "TYPE_INTERMEDIATE":
                                                pinkyIntCount++;
                                                pinkyIntTable.Add(pinkyBonesCount, pinkyMeasurements[countBones]);
                                                pinkyIntList.Add(pinkyMeasurements[countBones]);
                                                break;

                                            case "TYPE_DISTAL":
                                                pinkyDistCount++;
                                                pinkyDistTable.Add(pinkyBonesCount, pinkyMeasurements[countBones]);
                                                pinkyDistList.Add(pinkyMeasurements[countBones]);
                                                break;
                                        }

                                        using (StreamWriter boneWriter = new StreamWriter(bonesCSVPath, true))
                                        {
                                            boneWriter.WriteLine(boneLength.Type + ", Pinky, " + pinkyMeasurements[countBones]);
                                            //pinkyBonesCount++;
                                            boneReadingCount++;
                                            boneWriter.Close();
                                        }
                                        break;
                                    case 5:
                                        {
                                            if (countBones >= 0 && countBones < 3)
                                            {
                                                thumbMeasurements[countBones] = (decimal)boneLength.Length;// + (decimal)boneLength.Width;
                                                thumbBoneList.Add(boneLength.Type + ", " + thumbMeasurements[countBones]);
                                                //thumbBoneList.Add(thumbMeasurements[countBones]);
                                                thumbBonesCount++;
                                                //thumbBoneTable.Add(thumbBonesCount, thumbMeasurements[countBones]);


                                                //CASE CHECK THUMB BONE AND WRITE TO HASHTABLE
                                                switch (typeOfBone)
                                                {
                                                    case "TYPE_METACARPAL":
                                                        thumbMetaCount++;
                                                        thumbMetaTable.Add(thumbBonesCount, thumbMeasurements[countBones]);
                                                        thumbMetaList.Add(thumbMeasurements[countBones]);
                                                        break;

                                                    case "TYPE_PROXIMAL":
                                                        thumbProxCount++;
                                                        thumbProxTable.Add(thumbBonesCount, thumbMeasurements[countBones]);
                                                        thumbProxList.Add(thumbMeasurements[countBones]);
                                                        break;

                                                    case "TYPE_INTERMEDIATE":
                                                        thumbIntCount++;
                                                        thumbIntTable.Add(thumbBonesCount, thumbMeasurements[countBones]);
                                                        thumbIntList.Add(thumbMeasurements[countBones]);
                                                        break;
                                                }



                                                using (StreamWriter boneWriter = new StreamWriter(bonesCSVPath, true))
                                                {
                                                    boneWriter.WriteLine(boneLength.Type + ", Thumb, " + thumbMeasurements[countBones]);
                                                    //thumbBonesCount++;
                                                    boneReadingCount++;
                                                    boneWriter.Close();
                                                }

                                            }
                                            else { }
                                            break;
                                        }
                                }
                                countBones++;
                            }
                        }
                    }
                } 
                else
                {
                    MessageBox.Show("WRONG HAND USED. Please use right hand.");
                }


            }

        }

        void onImageRequestFailed(object sender, ImageRequestFailedEventArgs e)
        {
            if (e.reason == Leap.Image.RequestFailureReason.Insufficient_Buffer)
            {
                imagedata = new byte[e.requiredBufferSize];
            }

        }

        void onImageReady(object sender, ImageEventArgs e)
        {
            Rectangle lockArea = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData bitmapData = bitmap.LockBits(lockArea, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            byte[] rawImageData = imagedata;
            System.Runtime.InteropServices.Marshal.Copy(rawImageData, 0, bitmapData.Scan0, e.image.Width * e.image.Height * 2 * e.image.BytesPerPixel);
            bitmap.UnlockBits(bitmapData);
            //pictureBox1.Image = bitmap;
        }

        void displayArrays()
        {
            String vector = "";

            //LIST ITERATION
            thumbBoneList.Sort();
            indexBoneList.Sort();
            middleBoneList.Sort();
            ringBoneList.Sort();
            pinkyBoneList.Sort();

            //THUMB
            Console.Write("\nThumb\n");
            decimal thumbMetaAverage = 0;
            foreach (decimal values in thumbMetaTable.Values)
            {
                thumbMetaAverage += values;
            }
            thumbMetaAverage = Math.Round(thumbMetaAverage / thumbMetaCount, 2);
            Console.Write("\n" + thumbMetaAverage);
            vector += thumbMetaAverage + ", ";

            decimal thumbProxAverage = 0;
            foreach (decimal values in thumbProxTable.Values)
            {
                thumbProxAverage += values;
            }
            thumbProxAverage = Math.Round(thumbProxAverage / thumbProxCount, 2);
            Console.Write("\n" + thumbProxAverage);
            vector += thumbProxAverage + ", ";

            decimal thumbIntAverage = 0;
            foreach (decimal values in thumbIntTable.Values)
            {
                thumbIntAverage += values;
            }
            thumbIntAverage = Math.Round(thumbIntAverage / thumbIntCount, 2);
            Console.Write("\n" + thumbIntAverage);
            vector += thumbIntAverage + ", ";

            
            //INDEX
            Console.Write("\nIndex\n");
            decimal indexMetaAverage = 0;
            foreach (decimal values in indexMetaTable.Values)
            {
                indexMetaAverage += values;
            }
            indexMetaAverage = Math.Round(indexMetaAverage / indexMetaCount, 2);
            Console.Write("\n" + indexMetaAverage);
            vector += indexMetaAverage + ", ";

            decimal indexProxAverage = 0;
            foreach (decimal values in indexProxTable.Values)
            {
                indexProxAverage += values;
            }
            indexProxAverage = Math.Round(indexProxAverage / indexProxCount, 2);
            Console.Write("\n" + indexProxAverage);
            vector += indexProxAverage + ", ";

            decimal indexIntAverage = 0;
            foreach (decimal values in indexIntTable.Values)
            {
                indexIntAverage += values;
            }
            indexIntAverage = Math.Round(indexIntAverage / indexIntCount, 2);
            Console.Write("\n" + indexIntAverage);
            vector += indexIntAverage + ", ";

            decimal indexDistAverage = 0;
            foreach (decimal values in indexDistTable.Values)
            {
                indexDistAverage += values;
            }
            indexDistAverage = Math.Round(indexDistAverage / indexDistCount, 2);
            Console.Write("\n" + indexDistAverage);
            vector += indexDistAverage + ", ";



            //MIDDLE
            Console.Write("\nMiddle\n");
            decimal middleMetaAverage = 0;
            foreach (decimal values in middleMetaTable.Values)
            {
                middleMetaAverage += values;
            }
            middleMetaAverage = Math.Round(middleMetaAverage / middleMetaCount, 2);
            Console.Write("\n" + middleMetaAverage);
            vector += middleMetaAverage + ", ";

            decimal middleProxAverage = 0;
            foreach (decimal values in middleProxTable.Values)
            {
                middleProxAverage += values;
            }
            middleProxAverage = Math.Round(middleProxAverage / middleProxCount, 2);
            Console.Write("\n" + middleProxAverage);
            vector += middleProxAverage + ", ";

            decimal middleIntAverage = 0;
            foreach (decimal values in middleIntTable.Values)
            {
                middleIntAverage += values;
            }
            middleIntAverage = Math.Round(middleIntAverage / middleIntCount, 2);
            Console.Write("\n" + middleIntAverage);
            vector += middleIntAverage + ", ";

            decimal middleDistAverage = 0;
            foreach (decimal values in middleDistTable.Values)
            {
                middleDistAverage += values;
            }
            middleDistAverage = Math.Round(middleDistAverage / middleDistCount, 2);
            Console.Write("\n" + middleDistAverage);
            vector += middleDistAverage + ", ";



            //RING
            Console.Write("\nRing\n");
            decimal ringMetaAverage = 0;
            foreach (decimal values in ringMetaTable.Values)
            {
                ringMetaAverage += values;
            }
            ringMetaAverage = Math.Round(ringMetaAverage / ringMetaCount, 2);
            Console.Write("\n" + ringMetaAverage);
            vector += ringMetaAverage + ", ";

            decimal ringProxAverage = 0;
            foreach (decimal values in ringProxTable.Values)
            {
                ringProxAverage += values;
            }
            ringProxAverage = Math.Round(ringProxAverage / ringProxCount, 2);
            Console.Write("\n" + ringProxAverage);
            vector += ringProxAverage + ", ";

            decimal ringIntAverage = 0;
            foreach (decimal values in ringIntTable.Values)
            {
                ringIntAverage += values;
            }
            ringIntAverage = Math.Round(ringIntAverage / ringIntCount, 2);
            Console.Write("\n" + ringIntAverage);
            vector += ringIntAverage + ", ";

            decimal ringDistAverage = 0;
            foreach (decimal values in ringDistTable.Values)
            {
                ringDistAverage += values;
            }
            ringDistAverage = Math.Round(ringDistAverage / ringDistCount, 2);
            Console.Write("\n" + ringDistAverage);
            vector += ringDistAverage + ", ";



            //pinky
            Console.Write("\nPinky\n");
            decimal pinkyMetaAverage = 0;
            foreach (decimal values in pinkyMetaTable.Values)
            {
                pinkyMetaAverage += values;
            }
            pinkyMetaAverage = Math.Round(pinkyMetaAverage / pinkyMetaCount, 2);
            Console.Write("\n" + pinkyMetaAverage);
            vector += pinkyMetaAverage + ", ";

            decimal pinkyProxAverage = 0;
            foreach (decimal values in pinkyProxTable.Values)
            {
                pinkyProxAverage += values;
            }
            pinkyProxAverage = Math.Round(pinkyProxAverage / pinkyProxCount, 2);
            Console.Write("\n" + pinkyProxAverage);
            vector += pinkyProxAverage + ", ";

            decimal pinkyIntAverage = 0;
            foreach (decimal values in pinkyIntTable.Values)
            {
                pinkyIntAverage += values;
            }
            pinkyIntAverage = Math.Round(pinkyIntAverage / pinkyIntCount, 2);
            Console.Write("\n" + pinkyIntAverage);
            vector += pinkyIntAverage + ", ";

            decimal pinkyDistAverage = 0;
            foreach (decimal values in pinkyDistTable.Values)
            {
                pinkyDistAverage += values;
            }
            pinkyDistAverage = Math.Round(pinkyDistAverage / pinkyDistCount, 2);
            Console.Write("\n" + pinkyDistAverage);
            vector += pinkyDistAverage;

            Console.WriteLine("\n\n" + vector);
            generateHash(vector);
            
        }

        List<string> getCSV(List<ArrayList> _list)
        {
            int colCount = 0;
            int rowcCount = 0;

            List<string> lines = new List<string>();
            for (rowcCount = 0; rowcCount < _list[1].Count; rowcCount++)
            {
                string tmpLine = "";
                for (colCount = 0; colCount < _list.Count; colCount++)
                {
                    tmpLine += _list[colCount][rowcCount].ToString() + ",";
                }
                tmpLine = tmpLine.Substring(0, tmpLine.Length - 1);
                lines.Add(tmpLine);
            }

            lines.Sort();
            return lines;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (timeLeft > 0)
            {
                timeLeft = timeLeft - 1;
                lblTimer.Text = Convert.ToString(timeLeft);
            }
            else
            {
                timer1.Stop();
                controller.StopConnection();
                MessageBox.Show("Scan Stopped");
                //txtName.Clear();
                lblFPS.Text = "";
                lblTimer.Text = "Done";
                //btnArrays.Enabled = true;
                MessageBox.Show("Number of finger readings: " + Convert.ToString(fingerReadingCount) + "\nNumber of bone readings: " + Convert.ToString(boneReadingCount));

                List<ArrayList> _listOfLists = new List<ArrayList>();

                _listOfLists.Add(thumbMetaList);
                _listOfLists.Add(thumbProxList);
                _listOfLists.Add(thumbIntList);

                _listOfLists.Add(indexMetaList);
                _listOfLists.Add(indexProxList);
                _listOfLists.Add(indexIntList);
                _listOfLists.Add(indexDistList);

                _listOfLists.Add(middleMetaList);
                _listOfLists.Add(middleProxList);
                _listOfLists.Add(middleIntList);
                _listOfLists.Add(middleDistList);

                _listOfLists.Add(ringMetaList);
                _listOfLists.Add(ringProxList);
                _listOfLists.Add(ringIntList);
                _listOfLists.Add(ringDistList);

                _listOfLists.Add(pinkyMetaList);
                _listOfLists.Add(pinkyProxList);
                _listOfLists.Add(pinkyIntList);
                _listOfLists.Add(pinkyDistList);

                List<string> csv = getCSV(_listOfLists);

                StreamWriter wr = new StreamWriter(matrixCSVPath, true);
                wr.WriteLine("");
                foreach (string l in csv)
                    wr.WriteLine(l);

                wr.Close();

            }
        }

        private void btnAuthenticate_Click(object sender, EventArgs e)
        {
            //user files
            userFileName = txtPin.Text;
            directory = @"C:\Users\23509384\Desktop\" + userFileName;
            bonesFilePath = @"C:\Users\23509384\Desktop\" + userFileName + "\\Bones";
            fingersFilePath = @"C:\Users\23509384\Desktop\" + userFileName + "\\Fingers";
            bonesCSVPath = bonesFilePath + "\\" + userFileName + ".csv";
            fingersCSVPath = fingersFilePath + "\\" + userFileName + ".csv";
            matrixCSVPath = directory + "\\Matrix.csv";

            if (userFileName != "" && userFileName.Length == 4)
            {

                if (!File.Exists(bonesFilePath) && (!File.Exists(fingersFilePath)))
                {

                    Directory.CreateDirectory(bonesFilePath);
                    Directory.CreateDirectory(fingersFilePath);

                    FileStream boneStream = File.Create(bonesCSVPath);
                    boneStream.Close();

                    FileStream fingerStream = File.Create(fingersCSVPath);
                    fingerStream.Close();

                    timeLeft = 5;
                    timer1.Start();
                    controller.StartConnection();
                    controller.FrameReady += newFrameHandler;

                    Thread vectorThread = new Thread(new ThreadStart(ThreadFunction));
                    vectorThread.Start();
                }
                else
                {
                    MessageBox.Show("File Already Exists!");
                }

            }
            else
            {
                MessageBox.Show("Please enter a valid PIN");
            }
        }

        private void ThreadFunction() {
            try
            {
                displayArrays();
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
            
        }

        private void showVectorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            displayArrays();
        }

        private void generateHash(string input) {
            byte[] hash;

            using (var sha2 = new SHA256CryptoServiceProvider()) {
                hash = sha2.ComputeHash(Encoding.Unicode.GetBytes(input));
                foreach (byte b in hash) {
                    Console.Write(b + " ");
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoginForm login = new LoginForm();
            login.Show();
            this.Hide();
        }
    }
}
