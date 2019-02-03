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
// This project makes an extremely simple server to connect to the other  //
// traffic light clients.  Because of the personal firewall on the lab    //
// computers being switched on, the server cannot use a listening socket  //
// accept incomming connections.  So the server to actually connects to a //
// sort of proxy (running in my office) that accepts the incomming        //
// connection.                                                            //    
// By Nigel.                                                              //
//                                                                        //
// Please use this code, such as it is,  for any educational or non       //
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


namespace TrafficLightServer
{

    //New wrapper class.
    public delegate void UI_UpdateHandler(String message);

    public partial class FormServer : Form
    {
        // Nigel's Networking attributes.
        private int serverPort = 5000;
        private int bufferSize = 200;

        private TcpClient socketClient;

        // The network location of the proxy - "eeyore.fost.plymouth.ac.uk"
        private string serverName = "eeyore.fost.plymouth.ac.uk";
        private NetworkStream connectionStream;
        private BinaryReader inStream;
        private BinaryWriter outStream;
        private ThreadConnection threadConnection;

        /// <summary>
        /// This one is needed so that we can post messages back to the form's
        /// thread and don't violate C#'s threading rule that says you can
        /// only touch the UI components from the form's thread.           
        /// </summary>
        private SynchronizationContext uiContext;




        /// <summary>
        /// Initialise the server form.
        /// </summary>
        public FormServer()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Form load.  Display an IP. Or a series of IPs. 
        /// </summary>
        private void Form1_Load(object sender, EventArgs e)
        {
            IPHostEntry localHostInfo;

            // Identify our local IP address from the host computer's available IP numbers.
            localHostInfo = Dns.GetHostEntry(Dns.GetHostName());

            //listBoxOutput.Items.Add("You may have many IP numbers.");
            //listBoxOutput.Items.Add("In the Plymouth labs, use the IP that looks like an IP4 number");
            //listBoxOutput.Items.Add("something like 10.xx.xx.xx.");
            //listBoxOutput.Items.Add("If at home using a VPN use the IP4 number that starts");
            //listBoxOutput.Items.Add("something like 141.163.xx.xx");

            foreach (IPAddress ipAddress in localHostInfo.AddressList)
            {
                // If the IP address is IPV4 then add this to the list box.
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    listBoxOutput.Items.Add("Identified server IPv4 Address: " + ipAddress.ToString());
                }
            }


            // Get the SynchronizationContext for the current thread (the form's thread).                                                         
            uiContext = SynchronizationContext.Current;
            if (uiContext == null)
            {
                listBoxOutput.Items.Add("Debug: No UI context for this thread.");
            }
            else
            {
                listBoxOutput.Items.Add("Debug: We have a UI context.");
                listBoxOutput.Items.Add("");
            }


            listBoxOutput.Items.Add("Press to \"Connect\" to start the server.");
        }


        /// <summary>
        /// The OnClick for the "connect"command button.  Create a new client   
        /// socket. Much of this code is exception processing.                
        /// </summary>
        private void buttonConnect_Click(object sender, EventArgs e)
        {
            try
            {
                socketClient = new TcpClient(serverName, serverPort);
            }
            catch (Exception err)
            {
                //Console is a sealed object; we can't make it, we can just access.
                listBoxOutput.Items.Add("Error in connecting to server");     
                listBoxOutput.Items.Add(err.Message);				 	     
                labelStatus.Text = "Error " + err.Message;
                labelStatus.BackColor = Color.Red;
            }


            if (socketClient == null)
            {
                listBoxOutput.Items.Add("Socket is not connected.");

            }
            else
            {
                // Make some streams. They have rather more        
                // capabilities than just a socket.  With this type 
                // of socket, we can't read from it and write to it 
                // directly.                                        
                connectionStream = socketClient.GetStream();

                // Create the appropriate binary reader/writers given our connection.
                inStream = new BinaryReader(connectionStream);
                outStream = new BinaryWriter(connectionStream);

                listBoxOutput.Items.Add("Socket connected to proxy (" + serverName + ").");
                labelStatus.BackColor = Color.Green;
                labelStatus.Text = "Connected to " + serverName;


                // Discale connect button (we can only connect once) and enable other components.                   
                buttonConnect.Enabled = false;
                comboBoxLightColour.Enabled = true;
                comboBoxLightStatus.Enabled = true;
                sendCommandButton.Enabled = true;
                lightOneIPTextBox.Enabled = true;


                // We have now accepted a connection.                         
                //                                                           
                // There are several ways to do this next bit.   Here I make a
                // network stream and use it to create two other streams, an  
                // input and an output stream.   Life gets easier at that     
                // point.                                                     
                threadConnection = new ThreadConnection(uiContext, socketClient, this);
  
                // Create a new Thread to manage the connection that receives
                // data.  If you are a Java programmer, this looks like a    
                // load of hokum cokum..                                     
                Thread threadRunner = new Thread(new ThreadStart(threadConnection.run));
                threadRunner.Start();

                Console.WriteLine("Debug: Created new connection class.");
            }
        }


        /// <summary>
        /// Allows for the construction of the command message and passes the command 
        /// to send the command to the appropriate traffic light client.
        /// </summary>
        private void sendCommandButton_Click(object sender, EventArgs e)
        {
            if (lightOneIPTextBox.Text.Length > 0)
            {
                // Get the colour and status of the traffic light to change on the client.
                string colour = (string)comboBoxLightColour.SelectedItem;
                string status = (string)comboBoxLightStatus.SelectedItem;

                // Construct the command message to send to the traffic light.
                string serverCommand = colour + " " + status;

                // Send the colour and ip address to switch on/off the light.
                sendString(serverCommand, lightOneIPTextBox.Text);
            }
            else
            {
                MessageBox.Show("Please enter the IP address of the Traffic Light to send message to.",
                    "Traffic Light Server - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        /// <summary>
        /// Send a string to the IP you give.  The string and IP are bundled up  
        /// into one of there rather quirky Nigel style packets.                 
        ///                                                                      
        /// This uses the pre-defined stream outStream.  If this stream doesn't  
        /// exist then this method will bomb.                                    
        ///                                                                      
        /// It also does the networking synchronously, in the form's main        
        /// Thread.  This is not good practise; all networking should really be  
        /// asynchronous.                                                        
        /// </summary>
        /// <param name="commandToSend"></param>
        /// <param name="sendToIP"></param>
        private void sendString(string commandToSend, string sendToIP)
        {
            try
            {
                byte[] packet = new byte[bufferSize];

                // Split with "." as separator.
                string[] ipStrings = sendToIP.Split('.'); 

                // Think about this.  It assumes the user
                // has entered the IP corrrectly, and
                // sends the numbers without the bytes.
                packet[0] = byte.Parse(ipStrings[0]);
                packet[1] = byte.Parse(ipStrings[1]);   
                packet[2] = byte.Parse(ipStrings[2]);   
                packet[3] = byte.Parse(ipStrings[3]);


                // Start assembling message
                int bufferIndex = 4;                    

                //**************************************************************
                // Turn the string into an array of characters.                 
                //**************************************************************
                int length = commandToSend.Length;
                char[] chars = commandToSend.ToCharArray();


                //**************************************************************
                // Then turn each character into a byte and copy into my packet.
                //**************************************************************
                for (int i = 0; i < length; i++)
                {
                    byte b = (byte)chars[i];
                    packet[bufferIndex] = b;
                    bufferIndex++;
                }
                 
                // End of packet (even though it is always 200 bytes).
                packet[bufferIndex] = 0;    

                outStream.Write(packet, 0, bufferSize);
                listBoxOutput.Items.Add("Sent: " + commandToSend);
            }
            catch (Exception err)
            {
                listBoxOutput.Items.Add("An error occurred: " + err.Message);
            }
        }


        /// <summary>
        /// Message was posted back to us. This is to get over the C# threading
        /// rules whereby we can only touch the UI components from the thread   
        /// that created them, which is the form's main thread.                 
        /// </summary>
        /// <param name="received"></param>
        public void MessageReceived(object received)
        {
            // Get the string object which was received and sort it into it's relevant pieces.
            string message = (string)received;
            string[] messageParts = message.Split(' ');

            // Get the event that has been sent to the client.
            string lightEvent = messageParts[1];

            listBoxOutput.Items.Add("Message received: " + message);
            Console.WriteLine(message);

            // Process the message that was received.
            switch (lightEvent)
            {
                case "Start":
                    listBoxOutput.Items.Add("Event: Start - ID Request: " + messageParts[2]);



                    break;

                // Record a car arrived event.
                case "Car":
                    // Update the total number of cars at the traffic light.
                    lightOneNumLabel.Text = messageParts[2];

                    // Get the total number of cars now at the traffic light.
                    listBoxOutput.Items.Add("Event: Car Arrived - light 1 (" + messageParts[0] + "), total number of cars at light: " + messageParts[2]);

                    break;
            }
        }


        /// <summary>
        /// Form closing; if the connection thread was ever created then kill it off.                        
        /// </summary>
        private void FormServer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (threadConnection != null)
            {
                threadConnection.StopThread();
            }
        }
    }   // End of classy class.
}       // End of namespace
