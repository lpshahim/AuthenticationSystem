using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

namespace AuthenticationSystem
{
    class User
    {
        static Dictionary<string, string> searchDictionary = new Dictionary<string, string>();
        private HashSet<int> userPin { get; set; }
        private HashSet<int> userHandGeo { get; set; }
        private byte[] pixelValues = new byte[32];
        string startPINkey = "";
        string startUserKey = "";

        public User(string pin, string hashedHandGeo)
        {
            this.userPin = generatePINHash(pin);
            this.userHandGeo = generateUserHash(hashedHandGeo);
        }

        public string getUserHandGeo()
        {
            string handGeo = "";
            foreach(int i in userHandGeo)
            {
                handGeo += Convert.ToString(i) + " ";
            }
            return handGeo;
        }

        public string getUserPIN()
        {
            string PIN = "";
            foreach (int i in userPin)
            {
                PIN += Convert.ToString(i) + " ";
            }
            return PIN;
        }

        public void DisplayOutput()
        {
            Console.WriteLine("\nPin: {0}, \nHand geometry: {1} \n", getUserPIN(), getUserHandGeo());
            /*for (int i = 0; i < userHandGeo.Length; i++) {
                Console.Write(userHandGeo[i] + ", ");
            }*/
        }

        public void ReadDictionary(string pixelId)
        {
            Bitmap bmp = new Bitmap("randomImage2.png");

            //create dictionary
            string[] allLines = File.ReadAllLines("newMatrixMap.txt");
            Dictionary<string, string> map = new Dictionary<string, string>();

            foreach (var i in allLines) {
                string[] val = Regex.Split(i, "; ");
                map.Add(val[0], val[1]);
            }

            List<byte> pixelList = new List<byte>();

            var keysWithMatchingValues = map.Where(p => p.Value == pixelId).Select(p => p.Key);
            string[] keys = new string[8];
            int count = 0;
            foreach (var key in keysWithMatchingValues) {
                Console.WriteLine("\n\n"+key);
                keys[count] = key;
                count++;
                string[] values = Regex.Split(key, ",");
                Color pixel = bmp.GetPixel(Convert.ToInt32(values[0]), Convert.ToInt32(values[1]));
                byte a = pixel.A;
                byte r = pixel.R;
                byte g = pixel.G;
                byte b = pixel.B;
                pixelList.Add(a);
                pixelList.Add(r);
                pixelList.Add(g);
                pixelList.Add(b);
            }
            startUserKey = keys[0];
            

            foreach(var i in pixelList)
            {
                Console.Write(i + " ");
            }
            bmp.Dispose();

        }

        public string authenticatePin(string pin)
        {

            searchDictionary = File.ReadLines("newAuthenticationPairs.txt").Select(line => line.Split(',')).ToDictionary(line => line[0], ValueType => ValueType[1]);
            
            if (searchDictionary.ContainsValue(pin))
            {
                //Console.WriteLine("\nID: " + getPixelId(pin));
                string pixelID = getPixelId(pin);
                ReadDictionary(pixelID);
            }
            else {
                Console.WriteLine("\nFalse");
            }
            return getPixelId(pin);
        }

        public static string getPixelId(string search) {
            return searchDictionary[search];
        }

        public void Log(string pin, string vector, string transformed)
        {
            if (pin != null && vector != null)
            {

                if (File.Exists("Log.txt"))
                {
                    StreamWriter sw = File.AppendText("Log.txt");
                    sw.WriteLine("User with pin: {0} has logged in with hand vector of {1} and transformed vector {2}.\n", pin, vector, transformed);
                    sw.Flush();
                    sw.Close();
                }
                else
                {
                    Console.WriteLine("Log file does not exist!");
                }

            }
            else
            {
                Console.WriteLine("User information is invalid to add to log file.");
            }
        }

        public void addNewEnrolledUser(string newAssignedPin)
        {
            StreamWriter sw = File.AppendText("enrolledUsers.txt");
            sw.WriteLine(newAssignedPin);
            sw.Close();
            Log(newAssignedPin,"" ,getUserHandGeo());

        }

        public HashSet<int> generateUserHash(string transformedHandGeo)
        {
            byte[] hash;
            HashSet<int> hashedHandGeoHashset = new HashSet<int>();

            using (var sha2 = new SHA256CryptoServiceProvider())
            {
                hash = sha2.ComputeHash(Encoding.Unicode.GetBytes(transformedHandGeo));
                foreach (byte b in hash)
                {
                    //Console.Write("\n\nUser hash: " + b + " ");
                    hashedHandGeoHashset.Add(b);
                }
            }
            startUserKey.Trim();
            Console.WriteLine("\n\nSTART PIXEL:" + startUserKey + ".");
            checkPinPixelsStegoImage2(startUserKey);
            //setPixelsStegoImage2(startUserKey, hash);
            return hashedHandGeoHashset;
        }

        public HashSet<int> generatePINHash(string pin)
        {
            byte[] hash;
            HashSet<int> hashedPINHashset = new HashSet<int>();

            using (var sha2 = new SHA256CryptoServiceProvider())
            {
                hash = sha2.ComputeHash(Encoding.Unicode.GetBytes(pin));
                foreach (byte b in hash)
                {
                    //Console.Write("\n\nUser hash: " + b + " ");
                    hashedPINHashset.Add(b);
                }
            }
            startPINkey.Trim();
            Console.WriteLine("\n\nSTART PIN PIXEL:" + startPINkey + ".");
            checkPixelsStegoImage1(startPINkey);

            return hashedPINHashset;
        }

        public void checkPixelsStegoImage1(string pixel)
        {
            Bitmap bmp = new Bitmap("randomImage2.png");
            HashSet<int> pixelValues = new HashSet<int>();
            int comma = pixel.IndexOf(',');
            Console.WriteLine("INDEXOF " + comma);
            string subX = pixel.Split(',')[0];
            string subY = pixel.Substring(comma + 1);
            Console.WriteLine("X {0} Y {1} ", subX, subY);
            //string[] xy = pixel.Split(',').Select(sValue => sValue.Trim()).ToArray();
            int x;
            int y;
            if (int.TryParse(subX, out x) && int.TryParse(subY, out y))
            {

                for (int i = 0; i < 32; i += 4)
                {
                    Color pVals = bmp.GetPixel(x, y);
                    pixelValues.Add(pVals.A);
                    pixelValues.Add(pVals.R);
                    pixelValues.Add(pVals.G);
                    pixelValues.Add(pVals.B);
                    x++;
                }

                string hashSet1 = "";
                string hashSet2 = "";

                foreach (int i in pixelValues)
                {
                    hashSet1 += Convert.ToString(i) + " ";
                }

                bmp.Dispose();
                foreach (int i in userPin)
                {
                    hashSet2 += Convert.ToString(i) + " ";
                }

                Console.WriteLine("\nHashset1 of pixels in StegoImage1 is: {0}\nHashset 2 is {1}\n", hashSet1, hashSet2);

                bool yesNo = pixelValues.SetEquals(userPin);
                Console.WriteLine(yesNo);
            }
        }

        public void checkPinPixelsStegoImage2(string pixel)
        {
            Bitmap bmp = new Bitmap("stegoImage2.png");
            HashSet<int> pixelValues = new HashSet<int>();
            int comma = pixel.IndexOf(',');
            Console.WriteLine("INDEXOF " + comma);
            string subX = pixel.Split(',')[0];
            string subY = pixel.Substring(comma+1);
            //return combo pixel to start setting
            Console.WriteLine("X {0} Y {1} ", subX, subY);
            //string[] xy = pixel.Split(',').Select(sValue => sValue.Trim()).ToArray();
            int x;
            int y;
            if (int.TryParse(subX, out x) && int.TryParse(subY, out y))
            {
                for (int i = 0; i < 32; i += 4)
                {
                    Color pVals = bmp.GetPixel(x, y);
                    pixelValues.Add(pVals.A);
                    pixelValues.Add(pVals.R);
                    pixelValues.Add(pVals.G);
                    pixelValues.Add(pVals.B);
                    x++;
                }

                string hashSet1 = "";
                string hashSet2 = "";

                foreach (int i in pixelValues)
                {
                    hashSet1 += Convert.ToString(i) + " ";
                }

                bmp.Dispose();
                foreach (int i in userHandGeo)
                {
                    hashSet2 += Convert.ToString(i) + " ";
                }

                Console.WriteLine("\nHashset1 of pixels in StegoImage2 is: {0}\nHashset 2 is {1}\n", hashSet1, hashSet2);

                bool yesNo = pixelValues.SetEquals(userHandGeo);
                Console.WriteLine(yesNo);
            }
            
        }

        public void setPixelsStegoImage2(string pixel, byte[] h)
        {

            int x = Convert.ToInt16(pixel.Split(',')[0]);
            int y = Convert.ToInt16(pixel.Split(',')[1]);
            
             Console.WriteLine("\n\n X value is: " + x + "\nY value is: " + y + "\n"); 
                
            
            using (FileStream fileStream = new FileStream("stegoImage2.png", FileMode.Open, FileAccess.ReadWrite))
            {
               Image img = Image.FromStream(fileStream);
               Bitmap bmp = new Bitmap(img);
                for (int i = 0; i < h.Length; i += 4)
                {
                    //Console.WriteLine(h[i] + " " + h[i+1] + " " + h[i+2] + " " + h[i+3] + "\n");
                    bmp.SetPixel(x, y, Color.FromArgb(h[i], h[i + 1], h[i + 2], h[i + 3]));
                    x++;
                }
                fileStream.Close();
                bmp.Save("stegoImage2.png", System.Drawing.Imaging.ImageFormat.Png);
                bmp.Dispose();
            }     


        }

        private void SaveBMP(ref Bitmap bmp) // now 'ref' parameter
        {
            try
            {
                bmp.Save("stegoImage2.png");
            }
            catch
            {
                Bitmap bitmap = new Bitmap(bmp.Width, bmp.Height, bmp.PixelFormat);
                bmp.Dispose();
                bitmap.Save(("stegoImage2.png"));
                bmp = bitmap; // preserve clone        
            }
        }
    }
}
