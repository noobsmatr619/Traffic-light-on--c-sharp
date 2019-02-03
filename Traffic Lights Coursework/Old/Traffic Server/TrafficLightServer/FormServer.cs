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

        // A count of the number of clients connected to the crossroad.
        private int clientCount = 1;

        // Store the data regarding the crossroad; 
        // key = clientID (unique to each connected client), value = int[associated light number, total number of cars at client].
        private Dictionary<string, int[]> crossroadData;

        //
        private Bitmap backgroundImage;

        //
        private Image crossroadImage;


        private bool trafficTimerInProgress = false;


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

            // Initialise the crossroad data dictionary.
            crossroadData = new Dictionary<string, int[]>();

            // Set up the drawing panel on the form.
            CreateBackgroundImage();

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
        /// 
        /// </summary>
        private void CreateBackgroundImage()
        {
            // Draw the initial red lights onto the crossroad.
            // NOTE: Add boolean values to state which already has been drawn?
            // NOTE: One draw light method stating which light number?

            int imageWidth;
            int imageHeight;

            imageWidth = drawingPanel.Width;
            imageHeight = drawingPanel.Height;

            //
            crossroadImage = Image.FromFile("crossroad.png");

            // Set the background image as the bitmap of the crossroad image.
            backgroundImage = new Bitmap("crossroad.png");

            // Set the image onto the panel and make it stretch to fit the whole panel.
            drawingPanel.BackgroundImage = backgroundImage;
            drawingPanel.BackgroundImageLayout = ImageLayout.Stretch;
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
                comboBoxLightState.Enabled = true;
                sendCommandButton.Enabled = true;

                lightOneIPTextBox.Enabled = true;
                lightTwoIPTextBox.Enabled = true;
                lightThreeIPTextBox.Enabled = true;
                lightFourIPTextBox.Enabled = true;


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

                Console.WriteLine("DEBUG: Created new connection class.");


                trafficTimer.Start();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="receivedIP"></param>
        /// <returns></returns>
        private bool ValidateClientIP(string receivedIP)
        {
            bool validClientIP = false;

            // Test to see if the IP address we have received a connection message from 
            // is part of the current crossroad network.
            if (receivedIP == lightOneIPTextBox.Text) 
            {
                validClientIP = true;
            }
            else if (receivedIP == lightTwoIPTextBox.Text)
            {
                validClientIP = true;
            }
            else if (receivedIP == lightThreeIPTextBox.Text)
            {
                validClientIP = true;
            }
            else if (receivedIP == lightFourIPTextBox.Text)
            {
                validClientIP = true;
            }

            return validClientIP;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="receivedIP"></param>
        /// <param name="clientID"></param>
        private void SendAcceptClientMessage(string receivedIP, string clientID)
        {
            // Create a client information array to keep track of the information
            // the client sends to the server.
            int[] clientInfo = new int[2];

            // Store the light number this client will represent (the nth traffic light in the crossroad).
            clientInfo[0] = clientCount;
            
            // Initialise the number of cars that are currently at the road which
            // that traffic light is currently positioned at.
            clientInfo[1] = 0;

            // Add the new connection to our crossroad dictionary.
            crossroadData.Add(clientID, clientInfo);

            // Acknowledge the client's information we have received.
            sendString("Accept", clientID, receivedIP);

            listBoxOutput.Items.Add("Accepted client connection & information. ID assigned as: " + clientID + " (light #" + clientCount + ").");

            // Increment the client count to allow for a new connection (maximum of 4).
            clientCount++;
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
                string lightColour = (string)comboBoxLightColour.SelectedItem;
                string lightState = (string)comboBoxLightState.SelectedItem;

                // Send the colour and ip address to switch on/off the light.
                sendString(lightColour, lightState, lightOneIPTextBox.Text);
            }
            else
            {
                MessageBox.Show("Please enter the IP address of the Traffic Light to send message to.",
                    "Traffic Light Server - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        /// <summary>
        /// The initial traffic light sequence to go from red to green.
        /// </summary>
        /// <param name="receivedIP"></param>
        /// <param name="clientID"></param>
        private void SendLightSequence(string clientID)
        {
            string sendToIP;

            // Get the IP address based on the client.
            int associatedLightNum = crossroadData[clientID][0];
            Console.WriteLine("Found associated light number for client ({0}) light sequence: {1}", clientID, associatedLightNum);

            // Match the light number with the IP address entered for the client.
            switch (associatedLightNum)
            {
                case 1:
                    sendToIP = lightOneIPTextBox.Text;
                    break;

                case 2:
                    sendToIP = lightTwoIPTextBox.Text;
                    break;

                case 3:
                    sendToIP = lightThreeIPTextBox.Text;
                    break;

                case 4:
                    sendToIP = lightFourIPTextBox.Text;
                    break;

                default:
                    sendToIP = "";
                    break;
            }


            // Send the traffic light sequence.
            if (sendToIP != "")
            {
                listBoxOutput.Items.Add("Light Sequence for client " + clientID + " at " + sendToIP + ".");

                // Turn the red light on/off for the client.
                sendString("Light", "Red On " + clientID, sendToIP);

                sendString("Light", "Red Off " + clientID, sendToIP);

                // Turn the amber light on/off for the client.
                sendString("Light", "Amber On " + clientID, sendToIP);

                sendString("Light", "Amber Off " + clientID, sendToIP);

                // Turn the green light on/off for the client.
                sendString("Light", "Green On " + clientID, sendToIP);

                sendString("Light", "Green Off " + clientID, sendToIP);

                listBoxOutput.Items.Add("Light Sequence completed (client - " + clientID + ", IP - " + sendToIP + ")");
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
            string clientID;
            string message = (string)received;

            string[] messageParts = message.Split(' ');

            // NOTE: The event is the only item we read from the array 
            //       before we switch-case, prevents a loop when running client 
            //       & server on the same computer.

            // Get the event that has been sent to the client.
            string lightEvent = messageParts[1];

            // Only allow IP addresses currently entered in the server.
            if (ValidateClientIP(messageParts[0]))
            {
                // Process the message that was received.
                switch (lightEvent)
                {
                    case "Start":

                        // Get the id of the client which sent the message.
                        clientID = messageParts[messageParts.Length - 1];

                        // Ensure that we don't have more than four client connections.
                        if (clientCount < 5)
                        {
                            // A traffic light is online and has requested an ID to work with.
                            listBoxOutput.Items.Add("Event: Start - Client ID: " + messageParts[2]);

                            // Send the avaialable id back to the client.
                            SendAcceptClientMessage(messageParts[0], messageParts[2]);

                            // Set the initial traffic light to red for the client.
                            string lightOptions = "Red On " + clientID;
                            sendString("Light", lightOptions, messageParts[0]);
                        }
                        else
                        {
                            // Send a reject message to the client who requested a connection.
                            sendString("Reject", clientID, messageParts[0]);
                            listBoxOutput.Items.Add("Rejected connection attempt. Please make sure the client's IP address is entered in appropriate fields.");
                        }

                        break;


                    case "Car":

                        // Get the id of the client which sent the message.
                        clientID = messageParts[messageParts.Length - 1];

                        // Update entries in the crossroad data to reflect the message.
                        HandleCarEvent(Convert.ToInt32(messageParts[2]), clientID);

                        // Get the total number of cars now at the traffic light.
                        listBoxOutput.Items.Add("Event: Car Arrived - Light (" + clientID + ") - total number of cars at light: " + messageParts[2]);

                        break;


                    case "LightDone":

                        // Get the id of the client which sent the message.
                        clientID = messageParts[messageParts.Length - 1];

                        // Handle the light done event.
                        HandleLightDoneEvent(messageParts[2], messageParts[3], clientID);

                        listBoxOutput.Items.Add("Event: LightDone - Light " + messageParts[2] + " turned " + messageParts[3] + " for client: " + clientID);

                        break;
                }
            }
            //else
            //{
            //    // Get the id of the client which sent the message.
            //    clientID = messageParts[messageParts.Length - 1];

            //    // Send a reject message to the client who requested a connection.
            //    sendString("Reject", clientID, messageParts[0]);
            //    listBoxOutput.Items.Add("Rejected connection attempt. Please make sure the client's IP address is entered in appropriate fields.");
            //}
        }


        private void HandleCarEvent(int totalCars, string clientID)
        {
            // Update the number of cars at the traffic light in
            // the crossroad data.
            if (crossroadData.Count > 0)
            {
                // Find the associated id and using that access the array to store the 
                // total number of cars.
                crossroadData[clientID][1] = totalCars;

                // Update the total number of cars at the traffic light for the associated light.
                switch (crossroadData[clientID][0])
                {
                    case 1:
                        lightOneNumLabel.Text = totalCars.ToString();
                        break;

                    case 2:
                        lightTwoNumLabel.Text = totalCars.ToString();
                        break;

                    case 3:
                        lightThreeNumLabel.Text = totalCars.ToString();
                        break;

                    case 4:
                        lightFourNumLabel.Text = totalCars.ToString();
                        break;
                }
            }
        }


        private void HandleLightDoneEvent(string lightColour, string lightState, string clientID)
        {
            if (crossroadData.Count > 0)
            {
                // Get the light this client is associated with.
                int associatedLightNum = crossroadData[clientID][0];
                listBoxOutput.Items.Add("Associated light done: " + associatedLightNum.ToString());

                switch (associatedLightNum)
                {
                    case 1:

                        // Handle light event for light number 1.
                        if (lightColour == "Red")
                        {
                            if (lightState == "On")
                            {
                                Console.WriteLine("Turning on light red on.");
                                DrawLightOne(true, false, false);
                                Console.WriteLine("Finished drawing.");
                            }
                            else if (lightState == "Off")
                            {
                                DrawLightOne(false, false, false);
                            }
                        }
                        else if (lightColour == "Amber")
                        {
                            if (lightState == "On")
                            {
                                DrawLightOne(false, true, false);
                            }
                            else if (lightState == "Off")
                            {
                                DrawLightOne(false, false, false);
                            }
                        }
                        else if (lightColour == "Green")
                        {
                            if (lightState == "On")
                            {
                                DrawLightOne(false, false, true);
                            }
                            else if (lightState == "Off")
                            {
                                DrawLightOne(false, false, false);
                            }
                        }

                        break;


                    case 2:

                        // Handle light event for light number 2.
                        if (lightColour == "Red")
                        {
                            if (lightState == "On")
                            {
                                DrawLightTwo(true, false, false);
                            }
                            else if (lightState == "Off")
                            {
                                DrawLightTwo(false, false, false);
                            }
                        }
                        else if (lightColour == "Amber")
                        {
                            if (lightState == "On")
                            {
                                DrawLightTwo(false, true, false);
                            }
                            else if (lightState == "Off")
                            {
                                DrawLightTwo(false, false, false);
                            }
                        }
                        else if (lightColour == "Green")
                        {
                            if (lightState == "On")
                            {
                                DrawLightTwo(false, false, true);
                            }
                            else if (lightState == "Off")
                            {
                                DrawLightTwo(false, false, false);
                            }
                        }

                        break;

                    case 3:

                        // Handle light event for light number 3.
                        if (lightColour == "Red")
                        {
                            if (lightState == "On")
                            {
                                DrawLightThree(true, false, false);
                            }
                            else if (lightState == "Off")
                            {
                                DrawLightThree(false, false, false);
                            }
                        }
                        else if (lightColour == "Amber")
                        {
                            if (lightState == "On")
                            {
                                DrawLightThree(false, true, false);
                            }
                            else if (lightState == "Off")
                            {
                                DrawLightThree(false, false, false);
                            }
                        }
                        else if (lightColour == "Green")
                        {
                            if (lightState == "On")
                            {
                                DrawLightThree(false, false, true);
                            }
                            else if (lightState == "Off")
                            {
                                DrawLightThree(false, false, false);
                            }
                        }

                        break;

                    case 4:

                        // Handle light event for light number 4.
                        if (lightColour == "Red")
                        {
                            if (lightState == "On")
                            {
                                DrawLightFour(true, false, false);
                            }
                            else if (lightState == "Off")
                            {
                                DrawLightFour(false, false, false);
                            }
                        }
                        else if (lightColour == "Amber")
                        {
                            if (lightState == "On")
                            {
                                DrawLightFour(false, true, false);
                            }
                            else if (lightState == "Off")
                            {
                                DrawLightFour(false, false, false);
                            }
                        }
                        else if (lightColour == "Green")
                        {
                            if (lightState == "On")
                            {
                                DrawLightFour(false, false, true);
                            }
                            else if (lightState == "Off")
                            {
                                DrawLightFour(false, false, false);
                            }
                        }

                        break;
                }
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
        /// <param name="serverEvent"></param>
        /// <param name="serverEventOptions"></param>
        /// <param name="sendToIP"></param>
        private void sendString(string serverEvent, string serverEventOptions, string sendToIP)
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

                // Construct the final message to send.
                string finalMessage = serverEvent;

                // Add the server event information to the final message.
                if (serverEventOptions.Length > 0)
                {
                    finalMessage += " " + serverEventOptions;
                }

                // Start assembling message
                int bufferIndex = 4;                    

                //**************************************************************
                // Turn the string into an array of characters.                 
                //**************************************************************
                int length = finalMessage.Length;
                char[] chars = finalMessage.ToCharArray();


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
                listBoxOutput.Items.Add("Sent: " + finalMessage);
            }
            catch (Exception err)
            {
                listBoxOutput.Items.Add("An error occurred: " + err.Message);
            }
        }


        //private void DrawRedLights()
        //{
        //    Brush solidBrush = new SolidBrush(Color.Red);

        //    // Draw the red light.
        //    using (Graphics g = Graphics.FromImage(backgroundImage))
        //    {
        //        // Draw the red light at their appropriate locations.
        //        g.FillEllipse(solidBrush, lightOneX, lightOneY, radius, radius);

        //        // Draw the original background image back onto the screen.
        //        //g.DrawImage(crossroadImage, 0, 0);

        //        g.FillEllipse(solidBrush, lightTwoX, lightTwoY, radius, radius);
        //        g.FillEllipse(solidBrush, lightThreeX, lightThreeY, radius, radius);
        //        g.FillEllipse(solidBrush, lightFourX, lightFourY, radius, radius);
        //    }

        //    using (Graphics g = drawingPanel.CreateGraphics())
        //    {
        //        g.DrawImage(backgroundImage, 0, 0, drawingPanel.Width, drawingPanel.Height);
        //    }

        //    solidBrush.Dispose();
        //}


        private void DrawLightOne(bool redLight, bool amberLight, bool greenLight)
        {
            // The size of the light.
            int radius = 20;

            // The X, Y locations for the red lights.
            int RedX = 141;
            int RedY = 150;

            // The X, Y locations for the amber lights.
            int AmberX = 170;
            int AmberY = 150;

            // The X, Y locations for the green lights.
            int GreenX = 200;
            int GreenY = 150;

            // Declare the basic brushes to use.
            Brush solidBrush;
            Brush blackBrush;

            // Used to clear the traffic lights.
            blackBrush = new SolidBrush(Color.Black);


            using (Graphics g = Graphics.FromImage(backgroundImage))
            {
                // If the red light is turned on.
                if (redLight == true)
                {
                    Console.WriteLine("Drawing red light on.");

                    solidBrush = new SolidBrush(Color.Red);

                    g.FillEllipse(solidBrush, RedX, RedY, radius, radius);
                }
                else
                {
                    Console.WriteLine("Clearing red light.");
                    g.FillEllipse(blackBrush, RedX, RedY, radius, radius);
                }


                // If the amber light is turned on.
                if (amberLight == true)
                {
                    solidBrush = new SolidBrush(Color.Yellow);

                    g.FillEllipse(solidBrush, AmberX, AmberY, radius, radius);
                }
                else
                {
                    g.FillEllipse(blackBrush, AmberX, AmberY, radius, radius);
                }


                // If the green light is turned on.
                if (greenLight == true)
                {
                    solidBrush = new SolidBrush(Color.Green);

                    g.FillEllipse(solidBrush, GreenX, GreenY, radius, radius);
                }
                else
                {
                    g.FillEllipse(blackBrush, GreenX, GreenY, radius, radius);
                }
            }

            // Refresh the panel again.
            drawingPanel.Refresh();

            // Dispose of the black brush.
            blackBrush.Dispose();
        }


        private void DrawLightTwo(bool redLight, bool amberLight, bool greenLight)
        {
            // The size of the light.
            int radius = 20;

            // The X, Y locations for the red lights.
            int RedX = 382;
            int RedY = 110;

            // The X, Y locations for the amber lights.
            int AmberX = 382;
            int AmberY = 132;

            // The X, Y locations for the green lights.
            int GreenX = 382;
            int GreenY = 155;

            // Declare the basic brushes to use.
            Brush solidBrush;
            Brush blackBrush;

            // Used to clear the traffic lights.
            blackBrush = new SolidBrush(Color.Black);


            using (Graphics g = Graphics.FromImage(backgroundImage))
            {
                // If the red light is turned on.
                if (redLight)
                {
                    solidBrush = new SolidBrush(Color.Red);

                    g.FillEllipse(solidBrush, RedX, RedY, radius, radius);
                }
                else
                {
                    g.FillEllipse(blackBrush, RedX, RedY, radius, radius);
                }


                // If the amber light is turned on.
                if (amberLight)
                {
                    solidBrush = new SolidBrush(Color.Yellow);

                    g.FillEllipse(solidBrush, AmberX, AmberY, radius, radius);
                }
                else
                {
                    g.FillEllipse(blackBrush, AmberX, AmberY, radius, radius);
                }


                // If the green light is turned on.
                if (greenLight)
                {
                    solidBrush = new SolidBrush(Color.Green);

                    g.FillEllipse(solidBrush, GreenX, GreenY, radius, radius);
                }
                else
                {
                    g.FillEllipse(blackBrush, GreenX, GreenY, radius, radius);
                }
            }

            // Refresh the panel again.
            drawingPanel.Refresh();

            // Dispose of the black brush.
            blackBrush.Dispose();
        }


        private void DrawLightFour(bool redLight, bool amberLight, bool greenLight)
        {
            // The size of the light.
            int radius = 20;

            // The X, Y locations for the red lights.
            int RedX = 191;
            int RedY = 275;

            // The X, Y locations for the amber lights.
            int AmberX = 191;
            int AmberY = 300;

            // The X, Y locations for the green lights.
            int GreenX = 191;
            int GreenY = 323;

            // Declare the basic brushes to use.
            Brush solidBrush;
            Brush blackBrush;

            // Used to clear the traffic lights.
            blackBrush = new SolidBrush(Color.Black);


            using (Graphics g = Graphics.FromImage(backgroundImage))
            {
                // If the red light is turned on.
                if (redLight)
                {
                    solidBrush = new SolidBrush(Color.Red);

                    g.FillEllipse(solidBrush, RedX, RedY, radius, radius);
                }
                else
                {
                    g.FillEllipse(blackBrush, RedX, RedY, radius, radius);
                }


                // If the amber light is turned on.
                if (amberLight)
                {
                    solidBrush = new SolidBrush(Color.Yellow);

                    g.FillEllipse(solidBrush, AmberX, AmberY, radius, radius);
                }
                else
                {
                    g.FillEllipse(blackBrush, AmberX, AmberY, radius, radius);
                }


                // If the green light is turned on.
                if (greenLight)
                {
                    solidBrush = new SolidBrush(Color.Green);

                    g.FillEllipse(solidBrush, GreenX, GreenY, radius, radius);
                }
                else
                {
                    g.FillEllipse(blackBrush, GreenX, GreenY, radius, radius);
                }
            }

            // Refresh the panel again.
            drawingPanel.Refresh();

            // Dispose of the black brush.
            blackBrush.Dispose();
        }


        private void DrawLightThree(bool redLight, bool amberLight, bool greenLight)
        {
            // The size of the light.
            int radius = 20;

            // The X, Y locations for the red lights.
            int RedX = 380;
            int RedY = 280;

            // The X, Y locations for the amber lights.
            int AmberX = 407;
            int AmberY = 280;

            // The X, Y locations for the green lights.
            int GreenX = 435;
            int GreenY = 280;

            // Declare the basic brushes to use.
            Brush solidBrush;
            Brush blackBrush;

            // Used to clear the traffic lights.
            blackBrush = new SolidBrush(Color.Black);


            using (Graphics g = Graphics.FromImage(backgroundImage))
            {
                // If the red light is turned on.
                if (redLight)
                {
                    solidBrush = new SolidBrush(Color.Red);

                    g.FillEllipse(solidBrush, RedX, RedY, radius, radius);
                }
                else
                {
                    g.FillEllipse(blackBrush, RedX, RedY, radius, radius);
                }


                // If the amber light is turned on.
                if (amberLight)
                {
                    solidBrush = new SolidBrush(Color.Yellow);

                    g.FillEllipse(solidBrush, AmberX, AmberY, radius, radius);
                }
                else
                {
                    g.FillEllipse(blackBrush, AmberX, AmberY, radius, radius);
                }


                // If the green light is turned on.
                if (greenLight)
                {
                    solidBrush = new SolidBrush(Color.Green);

                    g.FillEllipse(solidBrush, GreenX, GreenY, radius, radius);
                }
                else
                {
                    g.FillEllipse(blackBrush, GreenX, GreenY, radius, radius);
                }
            }

            // Refresh the panel again.
            drawingPanel.Refresh();

            // Dispose of the black brush.
            blackBrush.Dispose();
        }


        /// <summary>
        /// Paint event to re-draw the panel with the current state of each traffic light.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void drawingPanel_Paint(object sender, PaintEventArgs e)
        //{
        //    // The size of the light.
        //    int radius = 20;

        //    // The brush that will be used to draw the traffic lights.
        //    Brush drawingBrush;

        //    using (Graphics g = Graphics.FromImage(backgroundImage))
        //    {

        //    }


        //    // Dispose of brush after drawing.
        //    //drawingBrush.Dispose();
        //}

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


        private void trafficTimer_Tick(object sender, EventArgs e)
        {
            if (crossroadData.Count > 0 && !trafficTimerInProgress)
            {
                listBoxOutput.Items.Add("Checking for traffic light status.");

                trafficTimerInProgress = true;

                // Loop through the traffic lights and detect if any of them
                // currently have roads where the number of cars is greater than or equal to 10.
                foreach(KeyValuePair<string, int[]> data in crossroadData)
                {
                    // Check if there are 10 or more cars waiting at the road related to that particular traffic light.
                    if (data.Value[1] >= 10)
                    {
                        listBoxOutput.Items.Add("Queue of 10+ cars at client: " + data.Key);
                        SendLightSequence(data.Key);
                    }
                }

                trafficTimerInProgress = false;

                listBoxOutput.Items.Add("Completed traffic light status.");
            }
        }
    }   // End of classy class.
}       // End of namespace
