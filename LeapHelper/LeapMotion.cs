using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Leap;

namespace LeapHelper
{
    public class LeapMotinn
    {
        private LeapListener listener;
        private Controller controller;

        public LeapListener Listener
        {
            get { return listener; }
        }

        public LeapMotinn()
        {
            listener = new LeapListener(); 
            controller = new Controller();
            controller.AddListener(listener);

        }
        
        public void Close()
        {
            controller.RemoveListener(listener);
            controller.Dispose();
        }
    }
}
