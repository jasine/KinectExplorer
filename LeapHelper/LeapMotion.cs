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
        public bool IsConnected { get; private set; }

        public LeapListener Listener
        {
            get { return listener; }
        }

        public LeapMotinn()
        {
            
            controller = new Controller();           
            if (controller.IsConnected)
            {
                IsConnected = true;
                listener = new LeapListener(); 
                controller.AddListener(listener);
            }

        }
        
        public void Close()
        {
            if(listener!=null)
                controller.RemoveListener(listener);
            controller.Dispose();
        }
    }
}
