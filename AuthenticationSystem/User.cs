using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationSystem
{
    class User
    {
        private string userPin { get; set; }
        private string userHandGeo { get; set; }

        public User(string pin, string handGeo)
        {
            this.userPin = pin;
            this.userHandGeo = handGeo;
        }

        public void DisplayOutput()
        {
            Console.WriteLine("Pin: {0}, \nHand geometry: {1}", userPin, userHandGeo);
        }
    }
}
