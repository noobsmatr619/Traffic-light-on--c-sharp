using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;


//************************************************************************//
// This project makes an extremely simple server to connect to the other  //
// traffic light clients.  Because of the personal firewall on the lab    //
// computers being switched on, the server cannot use a listening socket  //
// accept incoming connections.  So the server actually connects to a     //
// sort of proxy (running in my office) that accepts the incoming         //
// connection.                                                            //    
// By Nigel.                                                              //
//                                                                        //
// Please use this code, such as it is, for any educational or non        //
// profit making research purposes on the conditions that:                //
//                                                                        //
// 1.   You may only use it for educational and related research          //
//      purposes.                                                         //
//                                                                        //
// 2.   You leave my name on it.                                          //
//                                                                        //
// 3.   You correct at least 10% of the typing and spelling mistakes.     //
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

        // Store the data regarding the crossroad; 
        // key = clientID (unique to each connected client), value = int[associated light number, total number of cars at client].
        private Dictionary<string, int[]> crossroadData;

        // The list of cars for the traffic lights.
        private List<int[]> lightOneCars;
        private List<int[]> lightTwoCars;
        private List<int[]> lightThreeCars;
        private List<int[]> lightFourCars;

        // The background image of the crossroad.
        private Bitmap backgroundImage;

        // The images of the different cars at
        // each traffic light.
        private Bitmap lightOneCarImage;
        private Bitmap lightTwoCarImage;
        private Bitmap lightThreeCarImage;
        private Bitmap lightFourCarImage;

        // A count of the number of clients connected to the crossroad.
        private int clientCount = 0;

        // True for all if all the IP addresses entered
        // have a valid connection to a traffic client.
        private bool lightOneCheck = false;
        private bool lightTwoCheck = false;
        private bool lightThreeCheck = false;
        private bool lightFourCheck = false;

        // Whether the timer controlling the traffic
        // flow is currently in progress.
        private bool trafficTimerInProgress = false;

        // Counter and client ID array to keep track
        // of the light changes going on and where it is taking place.
        private int lightSequenceCount = 0; 
        private string[] lightSequenceClientIDs;
   
        // The boolean values controlling whether a specific 
        // light colour is turned on or off.
        private bool lightOneRed = false;
        private bool lightOneAmber = false;
        private bool lightOneGreen = false;

        private bool lightTwoRed = false;
        private bool lightTwoAmber = false;
        private bool lightTwoGreen = false;

        private bool lightThreeRed = false;
        private bool lightThreeAmber = false;
        private bool lightThreeGreen = false;

        private bool lightFourRed = false;
        private bool lightFourAmber = false;
        private bool lightFourGreen = false;

        // The stop and destination points for each light.
        private int lightOneStopPoint = 195;
        private int lightOneDestination = 590;

        private int lightTwoStopPoint = 159;
        private int lightTwoDestination = 500;

        private int lightThreeStopPoint = 375;
        private int lightThreeDestination = -10;

        private int lightFourStopPoint = 270;
        private int lightFourDestination = 0;

        // The gap between the cars.
        private int carGap = 50;


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

            // Initialise the cars lists.
            lightOneCars = new List<int[]>();
            lightTwoCars = new List<int[]>();
            lightThreeCars = new List<int[]>();
            lightFourCars = new List<int[]>();

            // The array to store the clients with light sequences in progresses.
            lightSequenceClientIDs = new string[2];

            // Load the drawing panel on the form.
            LoadPanelImages();


            // Identify our local IP address from the host computer's available IP numbers.
            localHostInfo = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress ipAddress in localHostInfo.AddressList)
            {
                // If the IP address is IPV4 then add this to the list box.
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    // Display the server's ip address to use with clients.
                    serverIPLabel.Text = ipAddress.ToString();

                    listBoxOutput.Items.Add("Identified server IPv4 Address: " + ipAddress.ToString());
                }
            }

            // Get the SynchronizationContext for the current thread (the form's thread).                                                         
            uiContext = SynchronizationContext.Current;
            if (uiContext == null)
            {
                listBoxOutput.Items.Add("DEBUG: No UI context for this thread.");
            }
            else
            {
                listBoxOutput.Items.Add("DEBUG: We have a UI context.");
                listBoxOutput.Items.Add("");
            }

            listBoxOutput.Items.Add("Press \"Connect\" to connect to proxy to start the server.");
            listBoxOutput.Items.Add("Enter the Traffic Client IP addresses in the fields to allow incoming connections.");
        }


        /// <summary>
        /// Load the panel image resources to use on the drawing panel.
        /// </summary>
        private void LoadPanelImages()
        {
            int imageWidth;
            int imageHeight;

            imageWidth = drawingPanel.Width;
            imageHeight = drawingPanel.Height;

            // Load the car images.
            lightOneCarImage = new Bitmap("light-one-car.png");
            lightTwoCarImage = new Bitmap("light-two-car.png");
            lightThreeCarImage = new Bitmap("light-three-car.png");
            lightFourCarImage = new Bitmap("light-four-car.png");

            // Set the background image as the bitmap of the crossroad image.
            backgroundImage = new Bitmap("crossroad.png");
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

                // Start the traffic timer to manage the traffic light sequence.
                trafficTimer.Start();

                // Start the crossroad timer which controls the order in which traffic will flow.
                crossroadTimer.Start();
            }
        }


        /// <summary>
        /// Check if the IP address that was received is at least a match with what the server allows.
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
        /// Send accept message to the client which requested to connect by sending "Start".
        /// This will also store the new traffic light client's details in the server.
        /// </summary>
        /// <param name="receivedIP"></param>
        /// <param name="clientID"></param>
        private void SendAcceptClientMessage(string receivedIP, string clientID)
        {
            // Create a client information array to keep track of the information
            // the client sends to the server.
            int[] clientInfo = new int[3];

            // Store the light number this client will represent (the nth traffic light in the crossroad).
            // And also store the client traffic lights opposite road light number.
            //clientInfo[0] = clientCount;
            if (!lightOneCheck && receivedIP == lightOneIPTextBox.Text)
            {
                clientInfo[0] = 1;
                clientInfo[2] = 3;

                // Confirm the light one ip address and prevent any adjustment.
                lightOneCheck = true;
                lightOneIPTextBox.Enabled = false;
            }
            else if (!lightTwoCheck && receivedIP == lightTwoIPTextBox.Text)
            {
                clientInfo[0] = 2;
                clientInfo[2] = 4;

                // Confirm the light two ip address and prevent any adjustment.
                lightTwoCheck = true;
                lightTwoIPTextBox.Enabled = false;
            }
            else if (!lightThreeCheck && receivedIP == lightThreeIPTextBox.Text)
            {
                clientInfo[0] = 3;
                clientInfo[2] = 1;

                // Confirm the light three ip address and prevent any adjustment.
                lightThreeCheck = true;
                lightThreeIPTextBox.Enabled = false;
            }
            else if (!lightFourCheck && receivedIP == lightFourIPTextBox.Text)
            {
                clientInfo[0] = 4;
                clientInfo[2] = 2;

                // Confirm the light four ip address and prevent any adjustment.
                lightFourCheck = true;
                lightFourIPTextBox.Enabled = false;
            }
            
            // Initialise the number of cars that are currently at the road which
            // that traffic light is currently positioned at.
            clientInfo[1] = 0;

            // Add the new connection to our crossroad dictionary.
            crossroadData.Add(clientID, clientInfo);

            // Acknowledge the client's information we have received.
            sendString("Accept", clientID, receivedIP);

            listBoxOutput.Items.Add("Accepted client connection & information. ID assigned as: " + clientID + " (light #" + clientInfo[0] + ").");

            // Increment the client count to allow for a new connection (maximum of 4).
            clientCount++;
        }


        /// <summary>
        /// The initial traffic light sequence to go from red to green 
        /// and back to red after a set time.
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
                sendString("LightSequence", clientID, sendToIP);

                listBoxOutput.Items.Add("Light Sequence for client " + clientID + " at " + sendToIP + ".");
            }
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientID"></param>
        private void SendUpdateTotalCars(string clientID)
        {
            string sendToIP;
            int updatedTotalCars;

            // Get the IP address based on the client.
            int associatedLightNum = crossroadData[clientID][0];
            Console.WriteLine("Found associated light number for client ({0}) light sequence: {1}", clientID, associatedLightNum);

            // Match the light number with the IP address entered for the client.
            switch (associatedLightNum)
            {
                case 1:
                    sendToIP = lightOneIPTextBox.Text;

                    // Get the number of cars still at light 1.
                    updatedTotalCars = lightOneCars.Count;

                    // Update this information on server.
                    lightOneNumLabel.Text = updatedTotalCars.ToString();

                    break;

                case 2:
                    sendToIP = lightTwoIPTextBox.Text;

                    // Get the number of cars still at light 2.
                    updatedTotalCars = lightTwoCars.Count;

                    lightTwoNumLabel.Text = updatedTotalCars.ToString();

                    break;

                case 3:
                    sendToIP = lightThreeIPTextBox.Text;

                    // Get the number of cars still at light 3.
                    updatedTotalCars = lightThreeCars.Count;

                    lightThreeNumLabel.Text = updatedTotalCars.ToString();

                    break;

                case 4:
                    sendToIP = lightFourIPTextBox.Text;

                    // Get the number of cars still at light 4.
                    updatedTotalCars = lightFourCars.Count;

                    lightFourNumLabel.Text = updatedTotalCars.ToString();

                    break;

                default:
                    // An error occured otherwise when trying to read the latest information.
                    sendToIP = "";
                    updatedTotalCars = -1;
                    break;
            }


            // Send the car update information to the client.
            if (sendToIP != "")
            {
                string carUpdateOptions = updatedTotalCars.ToString() + " " + clientID;
                sendString("CarUpdate", carUpdateOptions, sendToIP);

                listBoxOutput.Items.Add("Update Total Car count for client " + clientID + " at " + sendToIP + ".");
                listBoxOutput.Items.Add("Number of cars now at client " + clientID + ": " + updatedTotalCars.ToString());
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
            //
            //       However, this does not work due to an issue when running all clients
            //       on one computer and a server on another computer.

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
                        if (clientCount < 4)
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


                    case "LightSequenceDone":

                        // Get the id of the client which sent the message.
                        clientID = messageParts[messageParts.Length - 1];

                        lightSequenceCount -= 1;

                        listBoxOutput.Items.Add("Light Sequence completed (client - " + clientID + ", IP - " + messageParts[0] + "), remaining sequences: " + lightSequenceCount);

                        // Send the total number of cars now at the traffic light to the client.
                        SendUpdateTotalCars(clientID);

                        break;
                }
            }
        }


        /// <summary>
        /// Handle when a car message is sent to the server alerting the 
        /// server of how many cars are at the traffic light for a client.
        /// </summary>
        /// <param name="totalCars"></param>
        /// <param name="clientID"></param>
        private void HandleCarEvent(int totalCars, string clientID)
        {
            // The car data will contain int[associated light number, x co-ordinate, y co-ordinate].
            int[] carData = new int[4];

            // Update the number of cars at the traffic light in the crossroad data.
            if (crossroadData.Count > 0)
            {
                // Find the associated id and using that access the array to store the 
                // total number of cars.
                crossroadData[clientID][1] = totalCars;

                // Update the total number of cars at the traffic light for the associated light.
                switch (crossroadData[clientID][0])
                {
                    case 1:
                        // Update light label.
                        lightOneNumLabel.Text = totalCars.ToString();

                        if (totalCars > 0)
                        {
                            // Add relevant car data array.
                            carData[0] = 1;
                            carData[1] = -10;
                            carData[2] = 195;
                            carData[3] = lightOneStopPoint;

                            lightOneCars.Add(carData);
                        }

                        break;

                    case 2:
                        // Update light label.
                        lightTwoNumLabel.Text = totalCars.ToString();

                        if (totalCars > 0)
                        {
                            // Add relevant car data array.
                            carData[0] = 2;
                            carData[1] = 320;
                            carData[2] = -10;
                            carData[3] = lightTwoStopPoint;

                            lightTwoCars.Add(carData);
                        }

                        break;

                    case 3:
                        // Update light label.
                        lightThreeNumLabel.Text = totalCars.ToString();

                        if (totalCars > 0)
                        {
                            // Add relevant car data array.
                            carData[0] = 3;
                            carData[1] = 589;
                            carData[2] = 240;
                            carData[3] = lightThreeStopPoint;

                            lightThreeCars.Add(carData);
                        }
                        
                        break;

                    case 4:
                        // Update light label.
                        lightFourNumLabel.Text = totalCars.ToString();

                        if (totalCars > 0)
                        {
                            // Add relevant car data array.
                            carData[0] = 4;
                            carData[1] = 240;
                            carData[2] = 500;
                            carData[3] = lightFourStopPoint;

                            lightFourCars.Add(carData);
                        }

                        break;
                }
            }

        }


        /// <summary>
        /// Handle when a light has been rendered on the client.
        /// </summary>
        /// <param name="lightColour"></param>
        /// <param name="lightState"></param>
        /// <param name="clientID"></param>
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
                                lightOneRed = true;
                            }
                            else if (lightState == "Off")
                            {
                                lightOneRed = false;
                            }
                        }
                        else if (lightColour == "Amber")
                        {
                            if (lightState == "On")
                            {
                                lightOneAmber = true;
                            }
                            else if (lightState == "Off")
                            {
                                lightOneAmber = false;
                            }
                        }
                        else if (lightColour == "Green")
                        {
                            if (lightState == "On")
                            {
                                lightOneGreen = true;
                            }
                            else if (lightState == "Off")
                            {
                                lightOneGreen = false;
                            }
                        }

                        break;


                    case 2:

                        // Handle light event for light number 2.
                        if (lightColour == "Red")
                        {
                            if (lightState == "On")
                            {
                                lightTwoRed = true;
                            }
                            else if (lightState == "Off")
                            {
                                lightTwoRed = false;
                            }
                        }
                        else if (lightColour == "Amber")
                        {
                            if (lightState == "On")
                            {
                                lightTwoAmber = true;
                            }
                            else if (lightState == "Off")
                            {
                                lightTwoAmber = false;
                            }
                        }
                        else if (lightColour == "Green")
                        {
                            if (lightState == "On")
                            {
                                lightTwoGreen = true;
                            }
                            else if (lightState == "Off")
                            {
                                lightTwoGreen = false;
                            }
                        }

                        break;

                    case 3:

                        // Handle light event for light number 3.
                        if (lightColour == "Red")
                        {
                            if (lightState == "On")
                            {
                                lightThreeRed = true;
                            }
                            else if (lightState == "Off")
                            {
                                lightThreeRed = false;
                            }
                        }
                        else if (lightColour == "Amber")
                        {
                            if (lightState == "On")
                            {
                                lightThreeAmber = true;
                            }
                            else if (lightState == "Off")
                            {
                                lightThreeAmber = false;
                            }
                        }
                        else if (lightColour == "Green")
                        {
                            if (lightState == "On")
                            {
                                lightThreeGreen = true;
                            }
                            else if (lightState == "Off")
                            {
                                lightThreeGreen = false;
                            }
                        }

                        break;

                    case 4:

                        // Handle light event for light number 4.
                        if (lightColour == "Red")
                        {
                            if (lightState == "On")
                            {
                                lightFourRed = true;
                            }
                            else if (lightState == "Off")
                            {
                                lightFourRed = false;
                            }
                        }
                        else if (lightColour == "Amber")
                        {
                            if (lightState == "On")
                            {
                                lightFourAmber = true;
                            }
                            else if (lightState == "Off")
                            {
                                lightFourAmber = false;
                            }
                        }
                        else if (lightColour == "Green")
                        {
                            if (lightState == "On")
                            {
                                lightFourGreen = true;
                            }
                            else if (lightState == "Off")
                            {
                                lightFourGreen = false;
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


        /// <summary>
        /// 
        /// </summary>
        private void DrawCrossroadObjects()
        {
            int lightRadius;

            // The X, Y locations for the light 1.
            int lightOneRedX = 141;
            int lightOneRedY = 150;

            int lightOneAmberX = 170;
            int lightOneAmberY = 150;

            int lightOneGreenX = 200;
            int lightOneGreenY = 150;


            // The X, Y locations for the light 2.
            int lightTwoRedX = 382;
            int lightTwoRedY = 110;

            int lightTwoAmberX = 382;
            int lightTwoAmberY = 132;

            int lightTwoGreenX = 382;
            int lightTwoGreenY = 155;


            // The X, Y locations for the light 3.
            int lightThreeRedX = 380;
            int lightThreeRedY = 280;

            int lightThreeAmberX = 407;
            int lightThreeAmberY = 280;

            int lightThreeGreenX = 435;
            int lightThreeGreenY = 280;


            // The X, Y locations for the light 4.
            int lightFourRedX = 191;
            int lightFourRedY = 275;

            int lightFourAmberX = 191;
            int lightFourAmberY = 300;

            int lightFourGreenX = 191;
            int lightFourGreenY = 323;


            int carLight;
            int carXPosition;
            int carYPosition;
            //int carSize;
            int carWidth;
            int carHeight;

            int[] carData;

            Brush solidBrush;

            lightRadius = 20;
            //carSize = 5;
            carWidth = 35;
            carHeight = 20;

            solidBrush = new SolidBrush(Color.Black);

            // Create the temporary buffer to use to draw on.
            Bitmap tempBuffer = new Bitmap("crossroad.png");

            using (Graphics g = Graphics.FromImage(tempBuffer)) //backgroundImage
            {
                // Draw the traffic light colours which are turned on:]

                // Draw for light one.
                if (lightOneRed)
                {
                    solidBrush = new SolidBrush(Color.Red);

                    g.FillEllipse(solidBrush, lightOneRedX, lightOneRedY, lightRadius, lightRadius);
                }
                else
                {
                    solidBrush = new SolidBrush(Color.Black);

                    g.FillEllipse(solidBrush, lightOneRedX, lightOneRedY, lightRadius, lightRadius);
                }


                if (lightOneAmber)
                {
                    solidBrush = new SolidBrush(Color.Yellow);

                    g.FillEllipse(solidBrush, lightOneAmberX, lightOneAmberY, lightRadius, lightRadius);
                }
                else
                {
                    solidBrush = new SolidBrush(Color.Black);

                    g.FillEllipse(solidBrush, lightOneAmberX, lightOneAmberY, lightRadius, lightRadius);
                }


                if (lightOneGreen)
                {
                    solidBrush = new SolidBrush(Color.Green);

                    g.FillEllipse(solidBrush, lightOneGreenX, lightOneGreenY, lightRadius, lightRadius);
                }
                else
                {
                    solidBrush = new SolidBrush(Color.Black);

                    g.FillEllipse(solidBrush, lightOneGreenX, lightOneGreenY, lightRadius, lightRadius);
                }


                // Draw for light two.
                if (lightTwoRed)
                {
                    solidBrush = new SolidBrush(Color.Red);

                    g.FillEllipse(solidBrush, lightTwoRedX, lightTwoRedY, lightRadius, lightRadius);
                }
                else
                {
                    solidBrush = new SolidBrush(Color.Black);

                    g.FillEllipse(solidBrush, lightTwoRedX, lightTwoRedY, lightRadius, lightRadius);
                }


                if (lightTwoAmber)
                {
                    solidBrush = new SolidBrush(Color.Yellow);

                    g.FillEllipse(solidBrush, lightTwoAmberX, lightTwoAmberY, lightRadius, lightRadius);
                }
                else
                {
                    solidBrush = new SolidBrush(Color.Black);

                    g.FillEllipse(solidBrush, lightTwoAmberX, lightTwoAmberY, lightRadius, lightRadius);
                }


                if (lightTwoGreen)
                {
                    solidBrush = new SolidBrush(Color.Green);

                    g.FillEllipse(solidBrush, lightTwoGreenX, lightTwoGreenY, lightRadius, lightRadius);
                }
                else
                {
                    solidBrush = new SolidBrush(Color.Black);

                    g.FillEllipse(solidBrush, lightTwoGreenX, lightTwoGreenY, lightRadius, lightRadius);
                }


                // Draw for light three.
                if (lightThreeRed)
                {
                    solidBrush = new SolidBrush(Color.Red);

                    g.FillEllipse(solidBrush, lightThreeRedX, lightThreeRedY, lightRadius, lightRadius);
                }
                else
                {
                    solidBrush = new SolidBrush(Color.Black);

                    g.FillEllipse(solidBrush, lightThreeRedX, lightThreeRedY, lightRadius, lightRadius);
                }


                if (lightThreeAmber)
                {
                    solidBrush = new SolidBrush(Color.Yellow);

                    g.FillEllipse(solidBrush, lightThreeAmberX, lightThreeAmberY, lightRadius, lightRadius);
                }
                else
                {
                    solidBrush = new SolidBrush(Color.Black);

                    g.FillEllipse(solidBrush, lightThreeAmberX, lightThreeAmberY, lightRadius, lightRadius);
                }


                if (lightThreeGreen)
                {
                    solidBrush = new SolidBrush(Color.Green);

                    g.FillEllipse(solidBrush, lightThreeGreenX, lightThreeGreenY, lightRadius, lightRadius);
                }
                else
                {
                    solidBrush = new SolidBrush(Color.Black);

                    g.FillEllipse(solidBrush, lightThreeGreenX, lightThreeGreenY, lightRadius, lightRadius);
                }


                // Draw for light four.
                if (lightFourRed)
                {
                    solidBrush = new SolidBrush(Color.Red);

                    g.FillEllipse(solidBrush, lightFourRedX, lightFourRedY, lightRadius, lightRadius);
                }
                else
                {
                    solidBrush = new SolidBrush(Color.Black);

                    g.FillEllipse(solidBrush, lightFourRedX, lightFourRedY, lightRadius, lightRadius);
                }


                if (lightFourAmber)
                {
                    solidBrush = new SolidBrush(Color.Yellow);

                    g.FillEllipse(solidBrush, lightFourAmberX, lightFourAmberY, lightRadius, lightRadius);
                }
                else
                {
                    solidBrush = new SolidBrush(Color.Black);

                    g.FillEllipse(solidBrush, lightFourAmberX, lightFourAmberY, lightRadius, lightRadius);
                }


                if (lightFourGreen)
                {
                    solidBrush = new SolidBrush(Color.Green);

                    g.FillEllipse(solidBrush, lightFourGreenX, lightFourGreenY, lightRadius, lightRadius);
                }
                else
                {
                    solidBrush = new SolidBrush(Color.Black);

                    g.FillEllipse(solidBrush, lightFourGreenX, lightFourGreenY, lightRadius, lightRadius);
                }


                //solidBrush = new SolidBrush(Color.Blue);
                // Draw cars for light one.
                if (lightOneCars.Count > 0)
                {
                    for (int i = 0; i < lightOneCars.Count; i++)
                    {
                        carData = lightOneCars.ElementAt(i);

                        carLight = carData[0];
                        carXPosition = carData[1];
                        carYPosition = carData[2];

                        // Draw a square to represent the car.
                        //g.FillRectangle(solidBrush, carXPosition, carYPosition, carSize, carSize);

                        // Draw the appropriate image of the car depending on its light number.
                        g.DrawImage(lightOneCarImage, carXPosition, carYPosition, carWidth, carHeight);
                    }
                }


                if (lightTwoCars.Count > 0)
                {
                    // Draw cars for light two.
                    for (int i = 0; i < lightTwoCars.Count; i++)
                    {
                        carData = lightTwoCars.ElementAt(i);

                        carLight = carData[0];
                        carXPosition = carData[1];
                        carYPosition = carData[2];

                        // Draw a square to represent the car.
                        //g.FillRectangle(solidBrush, carXPosition, carYPosition, carSize, carSize);

                        // Draw the appropriate image of the car depending on its light number.
                        g.DrawImage(lightTwoCarImage, carXPosition, carYPosition, carWidth, carHeight);
                    }
                }


                // Draw cars for light three.
                if (lightThreeCars.Count > 0)
                {
                    for (int i = 0; i < lightThreeCars.Count; i++)
                    {
                        carData = lightThreeCars.ElementAt(i);

                        carLight = carData[0];
                        carXPosition = carData[1];
                        carYPosition = carData[2];

                        // Draw a square to represent the car.
                        //g.FillRectangle(solidBrush, carXPosition, carYPosition, carSize, carSize);

                        // Draw the appropriate image of the car depending on its light number.
                        g.DrawImage(lightThreeCarImage, carXPosition, carYPosition, carWidth, carHeight);
                    }
                }


                if (lightFourCars.Count > 0)
                {
                    // Draw cars for light four.
                    for (int i = 0; i < lightFourCars.Count; i++)
                    {
                        carData = lightFourCars.ElementAt(i);

                        carLight = carData[0];
                        carXPosition = carData[1];
                        carYPosition = carData[2];

                        // Draw a square to represent the car.
                        //g.FillRectangle(solidBrush, carXPosition, carYPosition, carSize, carSize);

                        // Draw the appropriate image of the car depending on its light number.
                        g.DrawImage(lightFourCarImage, carXPosition, carYPosition, carWidth, carHeight);
                    }
                }
            }

            // Draw the original background image back to the screen (used to double buffer).
            using (Graphics g = drawingPanel.CreateGraphics())
            {
                g.DrawImage(backgroundImage, 0, 0, drawingPanel.Width, drawingPanel.Height);
            }

            // Save the background image buffer and dispose of the brush.
            backgroundImage = tempBuffer;
            solidBrush.Dispose();
        }


        /// <summary>
        /// Timer to control the car objects on the crossroad; updates locations of cars.
        /// </summary>
        private void crossroadTimer_Tick(object sender, EventArgs e)
        {
            int[] carData;
            int[] nextCarData;

            //if (lightOneCars.Count > 0 || lightTwoCars.Count > 0 || lightThreeCars.Count > 0 || lightFourCars.Count > 0)
            //{
            // Remove cars which have already finished their journey.
            for (int i = 0; i < lightOneCars.Count; i++)
            {
                carData = lightOneCars.ElementAt(i);

                if (carData[0] == 1 && carData[1] >= lightOneDestination)
                {
                    lightOneCars.RemoveAt(i);
                }
            }


            // Remove cars which have already finished their journey.
            for (int i = 0; i < lightTwoCars.Count; i++)
            {
                carData = lightTwoCars.ElementAt(i);

                if (carData[0] == 2 && carData[2] >= lightTwoDestination)
                {
                    lightTwoCars.RemoveAt(i);
                }
            }


            // Remove cars which have already finished their journey.
            for (int i = 0; i < lightThreeCars.Count; i++)
            {
                carData = lightThreeCars.ElementAt(i);

                if (carData[0] == 3 && carData[1] <= lightThreeDestination)
                {
                    lightThreeCars.RemoveAt(i);
                }
            }


            // Remove cars which have already finished their journey.
            for (int i = 0; i < lightFourCars.Count; i++)
            {
                carData = lightFourCars.ElementAt(i);

                if (carData[0] == 4 && carData[2] <= lightFourDestination)
                {
                    lightFourCars.RemoveAt(i);
                }
            }



            // Loop through each of the active cars in the crossroad.
            for (int i = 0; i < lightOneCars.Count; i++)
            {
                carData = lightOneCars.ElementAt(i);

                // If the car is at traffic light 1 only move the x co-ordinate.
                if (carData[0] == 1)
                {
                    if (carData[1] < carData[3])
                    {
                        // Increment the x co-ordinate of the car.
                        carData[1] += 5;
                    }
                    else
                    {
                        // See if there is a green light on the road at the moment.
                        if (lightOneGreen == true)
                        {
                            // If there is a green, set the destination to be the other end of the road.
                            carData[3] = lightOneDestination;
                        }
                    }
                }
            }


            for (int i = 0; i < lightTwoCars.Count; i++)
            {
                carData = lightTwoCars.ElementAt(i);

                // If the car is at traffic light 2 only move the y co-ordinate.
                if (carData[0] == 2)
                {
                    if (carData[2] < carData[3])
                    {
                        // Increment the y co-ordinate of the car.
                        carData[2] += 5;
                    }
                    else
                    {
                        // See if there is a green light on the road at the moment.
                        if (lightTwoGreen == true)
                        {
                            // If there is a green, set the destination to be the other end of the road.
                            carData[3] = lightTwoDestination;
                        }
                    }
                }
            }


            for (int i = 0; i < lightThreeCars.Count; i++)
            {
                carData = lightThreeCars.ElementAt(i);

                // If the car is at traffic light 1 only move the x co-ordinate.
                if (carData[0] == 3)
                {
                    if (carData[1] > carData[3])
                    {
                        // Increment the x co-ordinate of the car.
                        carData[1] -= 5;
                    }
                    else
                    {
                        // See if there is a green light on the road at the moment.
                        if (lightThreeGreen == true)
                        {
                            // If there is a green, set the destination to be the other end of the road.
                            carData[3] = lightThreeDestination;
                        }
                    }
                }
            }


            for (int i = 0; i < lightFourCars.Count; i++)
            {
                carData = lightFourCars.ElementAt(i);

                // If the car is at traffic light 4 only move the y co-ordinate.
                if (carData[0] == 4)
                {
                    if (carData[2] > carData[3])
                    {
                        // Increment the y co-ordinate of the car.
                        carData[2] -= 5;
                    }
                    else
                    {
                        // See if there is a green light on the road at the moment.
                        if (lightFourGreen == true)
                        {
                            // If there is a green, set the destination to be the other end of the road.
                            carData[3] = lightFourDestination;
                        }
                    }
                }
            }




            // Give each car a distance between the other one.
            // If the car moves closer than the gap then make it slow down 
            // (reset its distance to the max gap allowed).
            for (int i = 0; i < lightOneCars.Count - 1; i++)
            {
                carData = lightOneCars.ElementAt(i);
                nextCarData = lightOneCars.ElementAt(i + 1);

                // If the current distance between the two cars 
                // is smaller than the gap allowed between cars...
                if (carData[1] - nextCarData[1] < carGap)
                {
                    // ...set the next car's x co-ordinate to the co-ordinate 
                    // minus the space needed to stay in the gap allowed between cars.
                    nextCarData[1] -= (carGap - (carData[1] - nextCarData[1]));
                }
            }


            for (int i = 0; i < lightTwoCars.Count - 1; i++)
            {
                carData = lightTwoCars.ElementAt(i);
                nextCarData = lightTwoCars.ElementAt(i + 1);

                // If the current distance between the two cars 
                // is smaller than the gap allowed between cars...
                if (carData[2] - nextCarData[2] < carGap)
                {
                    // ...set the next car's x co-ordinate to the co-ordinate 
                    // minus the space needed to stay in the gap allowed between cars.
                    nextCarData[2] -= (carGap - (carData[2] - nextCarData[2]));
                }
            }


            // Adjust the distance between cars which are too close - light 3.
            for (int i = 0; i < lightThreeCars.Count - 1; i++)
            {
                carData = lightThreeCars.ElementAt(i);
                nextCarData = lightThreeCars.ElementAt(i + 1);

                // If the current distance between the two cars 
                // is smaller than the gap allowed between cars...
                if (nextCarData[1] - carData[1] < carGap)
                {
                    // ...set the next car's x co-ordinate to the co-ordinate 
                    // minus the space needed to stay in the gap allowed between cars.
                    nextCarData[1] += (carGap - (nextCarData[1] - carData[1]));
                }
            }


            for (int i = 0; i < lightFourCars.Count - 1; i++)
            {
                carData = lightFourCars.ElementAt(i);
                nextCarData = lightFourCars.ElementAt(i + 1);

                // If the current distance between the two cars 
                // is smaller than the gap allowed between cars...
                if (nextCarData[2] - carData[2] < carGap)
                {
                    // ...set the next car's x co-ordinate to the co-ordinate 
                    // minus the space needed to stay in the gap allowed between cars.
                    nextCarData[2] += (carGap - (nextCarData[2] - carData[2]));
                }
            }


            // Draw all of the crossroad objects.
            DrawCrossroadObjects();

            // Call the garbage collector after drawing the objects.
            // NOTE: Using bitmaps keeps increasing the process memory, 
            //       this is the current solution used to control this.
            GC.Collect();
        }


        /// <summary>
        /// Runs every 5 seconds checking for 10+ cars waiting at each 
        /// traffic light on the four sides of the road.
        /// </summary>
        private void trafficTimer_Tick(object sender, EventArgs e)
        {
            // Only do if clients are connected.
            if (crossroadData.Count > 0)
            {
                if (!trafficTimerInProgress && lightSequenceCount == 0)
                {
                    listBoxOutput.Items.Add("Checking for traffic light status.");

                    trafficTimerInProgress = true;

                    // Loop through the traffic lights and detect if any of them
                    // currently have roads where the number of cars is greater than or equal to 10.
                    foreach (KeyValuePair<string, int[]> data in crossroadData)
                    {
                        // Check if there are 10 or more cars waiting at the road related to that particular traffic light.
                        if (data.Value[1] >= 10 )
                        {
                            listBoxOutput.Items.Add("Queue of 10+ cars at client: " + data.Key);

                            // Send the main message to the relevant client to start the light sequence on the client.
                            SendLightSequence(data.Key);
                            lightSequenceCount += 1;
                            lightSequenceClientIDs[0] = data.Key;

                            // Find the traffic light opposite to the main light.
                            int oppositeLightNum = crossroadData[data.Key][2];
                            foreach(KeyValuePair<string, int[]> oppositeLightData in crossroadData)
                            {
                                // If we find the opposite light number, then send the sequence to 
                                // open that light as well.
                                if (oppositeLightData.Value[0] == oppositeLightNum)
                                {
                                    SendLightSequence(oppositeLightData.Key);
                                    lightSequenceCount += 1;
                                    lightSequenceClientIDs[1] = oppositeLightData.Key;
                                }
                            }
                        }
                    }

                    // Enable the traffic timer again.
                    trafficTimerInProgress = false;
                }
            }   
        }


        /// <summary>
        /// Draw the current background image onto the panel to draw again later.
        /// </summary>
        private void drawingPanel_Paint(object sender, PaintEventArgs e)
        {
            // Draw the currently saved background Image.
            e.Graphics.DrawImage(backgroundImage, Point.Empty);
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
