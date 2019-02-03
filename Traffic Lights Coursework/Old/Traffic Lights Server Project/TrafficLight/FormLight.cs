using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;


//************************************************************************//
// This project makes an extremely simple traffic light.  Because of the  //
// personal firewall on the lab computers being switched on, this         //
// actually connects to a sort of proxy (running in my office) that       //
// accepts the incomming  connection.                                     //    
// By Nigel.                                                              //
//                                                                        //
// Please use this code, sich as it is,  for any educational or non       //
// profit making research purposes on the conditions that.                //
//                                                                        //
// 1.    You may only use it for educational and related research         //
//      purposes.                                                         //
//                                                                        //
// 2.   You leave my name on it.                                          //
//                                                                        //
// 3.   You correct at least 10% of the typing and spelling mistakes.      //
//                                                                        //
// © Nigel Barlow nigel@soc.plymouth.ac.uk 2018                           //
//************************************************************************//

namespace TrafficLight
{
    public partial class FormTrafficLight : Form
    {
        // Nigel Networking attributes.
        private int serverPort = 5000;
        private int bufferSize = 200;

        private TcpClient socketClient;
        // A computer in my office; "eeyore.fost.plymouth.ac.uk"
        private string serverName = "eeyore.fost.plymouth.ac.uk";  
        private NetworkStream connectionStream;
        private BinaryReader inStream;
        private BinaryWriter outStream;
        private ThreadConnection threadConnection;
        private int lightID=1;

        // This one is needed so that we can post messages back to the form's
        // thread and don't violate C#'s threading rule that says you can    
        // only touch the UI components from the form's thread.              
        SynchronizationContext uiContext;

        // The number of that have arrived at the traffic light.
        private int numCars = 0;


        /// <summary>
        /// Initialise the client form.
        /// </summary>
        public FormTrafficLight()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Form load.  Display an IP. Or a series of IPs.         
        /// </summary>                        
        private void Form1_Load(object sender, EventArgs e)
        {
            // Find out IPV4 number of traffic light client.
            IPHostEntry localHostInfo = Dns.GetHostEntry(Dns.GetHostName());

            //listBoxOutput.Items.Add("You may have many IP numbers.");
            //listBoxOutput.Items.Add("In the Plymouth labs, use the IP that looks like an IP4 number");
            //listBoxOutput.Items.Add("something like 10.xx.xx.xx.");
            //listBoxOutput.Items.Add("If at home using a VPN use the IP4 number that starts");
            //listBoxOutput.Items.Add("something like 141.163.xx.xx");


            foreach (IPAddress ipAddress in localHostInfo.AddressList)
            {
                //listBoxOutput.Items.Add(address.ToString());

                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    listBoxOutput.Items.Add("Identified client IPv4 address: " + ipAddress.ToString());
                }
            }


            // Get the SynchronizationContext for the current thread (the form's  thread).                                                         
            uiContext = SynchronizationContext.Current;
            if (uiContext == null)
            {
                listBoxOutput.Items.Add("DEBUG: No context for this thread.");
            }
            else
            {
                listBoxOutput.Items.Add("DEBUG: We have a UI context.");
                listBoxOutput.Items.Add("");
            }


            listBoxOutput.Items.Add("Press \"Connect\" to start the traffic light.");
        }


        /// <summary>
        /// Form closing.  
        /// If the connection thread was ever created then kill it off.       
        /// </summary>                                                    
        private void FormTrafficLight_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (threadConnection != null)
            {
                threadConnection.StopThread();
            }
        }


        /// <summary>
        /// Message was posted back to us.  This is to get over the C# threading
        /// rules whereby we can only touch the UI components from the thread  
        /// that created them, which is the form's main thread.              
        /// </summary>
        /// <param name="received"></param>
        public void MessageReceived(object received)
        {
            string message = (string)received;
            listBoxOutput.Items.Add(message);
            ChangeLights(message);

            Console.WriteLine(message);
        }


        /// <summary>
        /// Change the status of the lights.     
        /// </summary>
        /// <param name="command"></param>
        private void ChangeLights(string command)
        {
            // Turn on/off the appropriate light given the contents of the command.
            if (command != null)
            {
                if (command.Contains("Red On"))
                {
                    labelRed.Visible = true;
                }
                else if (command.Contains("Red Off"))
                {
                    labelRed.Visible = false;
                }

                if (command.Contains("Amber On"))
                {
                    labelAmber.Visible = true;
                }
                else if (command.Contains("Amber Off"))
                {
                    labelAmber.Visible = false;
                }

                if (command.Contains("Green On"))
                {
                    labelGreen.Visible = true;
                }
                else if (command.Contains("Green Off"))
                {
                    labelGreen.Visible = false;
                }
            }
            else
            {
                listBoxOutput.Items.Add("Debug: The command received was null/not recognised.");
            }
        }


        /// <summary>
        ///  The OnClick for the "connect"command button.  Create a new client
        ///  socket. Much of this code is exception processing.   
        /// </summary>
        private void buttonConnect_Click(object sender, EventArgs e)
        {
            try
            {
                socketClient = new TcpClient(serverName, serverPort);
            }
            catch (Exception err)
            {
                //Console is a sealed object; we can't make it, we can just access
                listBoxOutput.Items.Add("Error in connecting to server.");     
                listBoxOutput.Items.Add(err.Message);				 	     
                labelStatus.Text = "Error " + err.Message;
                labelStatus.BackColor = Color.Red;
            }

            if (socketClient == null)
            {
                listBoxOutput.Items.Add("Socket not connected.");
            }
            else
            {
                // Make some streams.  They have rather more       
                // capabilities than just a socket.  With this type 
                // of socket, we can't read from it and write to it 
                // directly.                                        
                connectionStream = socketClient.GetStream();

                inStream = new BinaryReader(connectionStream);
                outStream = new BinaryWriter(connectionStream);

                listBoxOutput.Items.Add("Socket connected to " + serverName);
                labelStatus.BackColor = Color.Green;
                labelStatus.Text = "Connected to Server (" + serverName + ")";


                // Disable connect button (we can only connect once) and    
                // enable other components.                                 
                buttonConnect.Enabled = false;
                buttonCarArrived.Enabled = true;


                // We have now accepted a connection:                                                  
                //There are several ways to do this next bit. Here I make a
                //network stream and use it to create two other streams, an  
                //input and an output stream.   Life gets easier at that     
                //point.                                                     
                threadConnection = new ThreadConnection(uiContext, socketClient, this);


                // Create a new Thread to manage the connection that receives
                // data.  If you are a Java programmer, this looks like a    
                // load of hokum cokum..                                     
                Thread threadRunner = new Thread(new ThreadStart(threadConnection.Run));
                threadRunner.Start();

                Console.WriteLine("Debug: Created new connection class.");
            }
        }


        /// <summary>
        /// Button click for the car arrived button.  All it does is send the
        /// string "Car" to the server. 
        /// </summary>
        private void buttonCarArrived_Click(object sender, EventArgs e)
        {
            numCars++;
            carsArrivedLabel.Text = numCars.ToString();
           
            sendString("Car " + numCars.ToString(), textBoxLightIP.Text);
        }

        private void SendStartPacket()
        {
           
            sendString("Start "+ lightID, textBoxLightIP.Text);

        }

        /// <summary>
        /// Send a string to the IP you give.  The string and IP are bundled up
        /// into one of there rather quirky Nigel style packets.                 
        /// This uses the pre-defined stream outStream.  If this strean doesn't 
        /// exist then this method will bomb.  
        /// 
        /// It also does the networking synchronously, in the form's main  
        /// Thread.  This is not good practise; all networking should really be 
        /// asynchronous.
        /// </summary>
        /// <param name="stringToSend"></param>
        /// <param name="sendToIP"></param>
        private void sendString(string stringToSend, string sendToIP)
        {
            try
            {
                byte[] packet = new byte[bufferSize];

                // Split with . as separator.
                string[] ipStrings = sendToIP.Split('.');

                // Think about this.  It assumes the user has entered the IP corrrectly,
                // and sends the numbers without the bytes.
                packet[0] = byte.Parse(ipStrings[0]);
                packet[1] = byte.Parse(ipStrings[1]);   
                packet[2] = byte.Parse(ipStrings[2]);   
                packet[3] = byte.Parse(ipStrings[3]);

                // Start assembling message
                int bufferIndex = 4;                    

                // Turn the string into an array of characters.
                int length = stringToSend.Length;
                char[] chars = stringToSend.ToCharArray();

            
                // Then turn each character into a byte and copy into my packet.
                for (int i = 0; i < length; i++)
                {
                    //listBoxOutput.Items.Add("Debug: Current character: " + chars[i]);
                    byte b = (byte)chars[i];

                    //listBoxOutput.Items.Add("Debug: Character as byte: " + b);
                    packet[bufferIndex] = b;

                    bufferIndex++;
                }

                // End of packet (even though it is always 200 bytes).
                packet[bufferIndex] = 0;    


                // Show the contents of the packet before transmitting.
                listBoxOutput.Items.Add(string.Join(", ", packet));
                
                // Write the packet into our outgoing binary stream.
                outStream.Write(packet, 0, bufferSize);
                listBoxOutput.Items.Add("Debug: Sent " + stringToSend);
            }
            catch (Exception err)
            {
                listBoxOutput.Items.Add("An error occurred: " + err.Message);
            }
        }

        private void textCarArrived_TextChanged(object sender, EventArgs e)
        {

          

        }

        private void textBoxLightIP_TextChanged(object sender, EventArgs e)
        {

        }
    }   // End of classy class.
}   // End of namespace
