using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Text.RegularExpressions;

namespace AuthenticationSystem
{
    class User
    {
        static Dictionary<string, string> searchDictionary = new Dictionary<string, string>();
        private string userPin { get; set; }
        private string[] userHandGeo { get; set; }

        public User(string pin, string[] handGeo)
        {
            this.userPin = pin;
            this.userHandGeo = handGeo;
        }

        public void DisplayOutput()
        {
            Console.WriteLine("\nPin: {0}, \n\n", userPin);
            Console.Write("Hand geometry : \n");
            for (int i = 0; i < userHandGeo.Length; i++) {
                Console.Write(userHandGeo[i] + ", ");
            }
        }

        public void ReadDictionary(string pixelId)
        {
            Bitmap bmp = new Bitmap("randomImage.png");

            //create dictionary
            string[] allLines = File.ReadAllLines("newMatrixMap.txt");
            Dictionary<string, string> map = new Dictionary<string, string>();

            foreach (var i in allLines) {
                string[] val = Regex.Split(i, "; ");
                map.Add(val[0], val[1]);
            }

           /* foreach (var i in map) {
                Console.WriteLine(i);
            }*/
            
            var keysWithMatchingValues = map.Where(p => p.Value == pixelId).Select(p => p.Key);

            foreach (var key in keysWithMatchingValues) {
                Console.WriteLine("\n\n"+key);
                string[] values = Regex.Split(key, ",");
                Color pixel = bmp.GetPixel(Convert.ToInt32(values[0]), Convert.ToInt32(values[1]));
                Console.WriteLine("Pixel Values:\n");
                Console.Write(pixel.A + ", " + pixel.R + ", " + pixel.G + ", " + pixel.B);
            }
        }

        public string authenticatePin(string pin) {

            searchDictionary = File.ReadLines("authenticationPairs.txt").Select(line => line.Split(',')).ToDictionary(line => line[0], ValueType => ValueType[1]);
            
            if (searchDictionary.ContainsValue(pin))
            {
                Console.WriteLine("\nID: " + getPixelId(pin));
                string pixelID = getPixelId(pin);
                ReadDictionary(pixelID);
            }
            else {
                Console.WriteLine("\nFalse");
            }
            return searchDictionary[pin];
        }

        public static string getPixelId(string search) {
            return searchDictionary[search];
        }
    }
}
