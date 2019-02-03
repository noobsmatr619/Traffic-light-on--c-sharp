using System;
using System.Drawing;
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
// accepts the incoming  connection.                                      //    
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

        // This one is needed so that we can post messages back to the form's
        // thread and don't violate C#'s threading rule that says you can    
        // only touch the UI components from the form's thread.              
        SynchronizationContext uiContext;

        // The client ID of the traffic light.
        // A random 4 digit token used to communicate before a client id is set
        // by the server for the client.
        private int clientID = -1;

        // The number of that have arrived at the traffic light.
        private int numCars = 0;

        // Random object to create a random number.
        private Random rand;


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
            // Initialise the random object to use.
            rand = new Random();

            // Find out IPV4 number of traffic light client.
            IPHostEntry localHostInfo = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress ipAddress in localHostInfo.AddressList)
            {
                //listBoxOutput.Items.Add(address.ToString());

                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    clientIPLabel.Text = ipAddress.ToString();
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


            listBoxOutput.Items.Add("Info: Enter the Traffic Server IP and press \"Connect to Proxy\" to start this traffic light.");
        }


        /// <summary>
        ///  The OnClick for the "connect"command button.  Create a new client
        ///  socket. Much of this code is exception processing.   
        /// </summary>
        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (serverIPTextBox.Text.Length != 0)
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
                    serverIPTextBox.Enabled = false;
                    buttonCarArrived.Enabled = true;

                    // We have now accepted a connection:                   
                    //
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

                    Console.WriteLine("DEBUG: Created new connection class.");

                    // Send a start message to the server to make it aware of the new traffic light.
                    SendStartMessage();
                }
            }
            else
            {
                MessageBox.Show("Please enter the server IP Address before connecting to Proxy.", "Traffic Client - Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        /// <summary>
        /// Send an initial message to the traffic light server alerting
        /// it of the client's presence. Generates our random client ID.
        /// </summary>
        private void SendStartMessage()
        {
            // Generate a random 4 digit number to recognise the client by
            // when deciding on a client id.
            clientID = rand.Next(1000, 9999);
            sendString("Start", "", serverIPTextBox.Text);
        }


        /// <summary>
        /// Button click for the car arrived button.  All it does is send the
        /// string "Car" to the server. 
        /// </summary>
        private void buttonCarArrived_Click(object sender, EventArgs e)
        {
            string carOptions;

            // Increment the number of cars at the traffic light.
            numCars++;
            lightCarsNumLabel.Text = numCars.ToString();

            // Send a message to the server notifying that a car has arrived,
            // along with the total number of cars at the traffic light now.
            carOptions = numCars.ToString();
            sendString("Car", carOptions, serverIPTextBox.Text);
        }


        /// <summary>
        /// Change the lights on our client and send the changes to the server as well.
        /// </summary>
        private async void SendLightSequence()
        {
            string serverIP = serverIPTextBox.Text;

            // Prevent any more cars from being added?
            buttonCarArrived.Enabled = false;

            // Turn on the red light for 2 seconds.
            if (!labelRed.Visible)
            {
                sendString("LightDone", "Red On", serverIP);
                labelRed.Visible = true;

                await Task.Delay(2000);
            }

            // Turn off red light.
            sendString("LightDone", "Red Off", serverIP);
            labelRed.Visible = false;

            await Task.Delay(500);

            // Turn on the amber label for 2 seconds.
            sendString("LightDone", "Amber On", serverIP);
            labelAmber.Visible = true;

            await Task.Delay(2000);

            // Turn off amber light.
            sendString("LightDone", "Amber Off", serverIP);
            labelAmber.Visible = false;

            await Task.Delay(500);

            // Turn on the green light for 10 seconds.
            sendString("LightDone", "Green On", serverIP);
            labelGreen.Visible = true;

            // A 15 second gap in which for cars to pass at the green light.
            await Task.Delay(15000);

            // Turn off the green light.
            sendString("LightDone", "Green Off", serverIP);
            labelGreen.Visible = false;

            await Task.Delay(2000);

            // Turn on the amber label for 2 seconds.
            sendString("LightDone", "Amber On", serverIP);
            labelAmber.Visible = true;

            await Task.Delay(2000);

            // Turn off amber light.
            sendString("LightDone", "Amber Off", serverIP);
            labelAmber.Visible = false;

            // Turn off red light.
            sendString("LightDone", "Red Off", serverIP);
            labelRed.Visible = false;

            await Task.Delay(2000);

            // Turn the red light back on.
            sendString("LightDone", "Red On", serverIP);
            labelRed.Visible = true;

            // Set the number of cars at the light back to zero.
            numCars = 0;
            lightCarsNumLabel.Text = numCars.ToString();

            // Send the total number of cars back to the server.
            sendString("Car", numCars.ToString(), serverIP);

            // Send back to the server to state that the light sequence has been completed.
            sendString("LightSequenceDone", "", serverIP);

            // Enable the car arrived button to allow more cars to come in.
            buttonCarArrived.Enabled = true;
        }


        /// <summary>
        /// Message was posted back to us. This is to get over the C# threading
        /// rules whereby we can only touch the UI components from the thread  
        /// that created them, which is the form's main thread.              
        /// </summary>
        /// <param name="received"></param>
        public void MessageReceived(object received)
        {
            string receivedClientID;
            string message = (string)received;
            string[] messageParts = message.Split(' ');

            // NOTE: The event is the only item we read from the array 
            //       before we switch-case, prevents reading any looped when running client 
            //       & server on the same computer.

            // Get the event that the server has given us.
            string serverEvent = messageParts[1];

            // Process the appropriate event the server sends.
            switch (serverEvent)
            {
                // Server sends us the client id we should use
                // for all further messages.
                case "Accept":

                    // Get the client ID this message is meant for.
                    receivedClientID = messageParts[messageParts.Length - 1];

                    if (clientID == Convert.ToInt32(receivedClientID))
                    {
                        // Update the client ID information on the server form.
                        clientIDNumLabel.Text = clientID.ToString();

                        listBoxOutput.Items.Add("Server Event: Accept - The server accepted the following client ID: " + clientID);
                    }
                        
                    break;


                case "Reject":

                    // Get the client ID this message is meant for.
                    receivedClientID = messageParts[messageParts.Length - 1];

                    if (clientID == Convert.ToInt32(receivedClientID))
                    {
                        // Server sends a reject message if there are already 4 clients working on the same IP.
                        listBoxOutput.Items.Add("Server Event: Reject - The server rejected communication from the client. The server may already be full.");
                    }

                    break;


                // Server sends a light change message indicating the colour of the light to change and whether it should be on/off.
                case "Light":

                    // Get the client ID this message is meant for.
                    receivedClientID = messageParts[messageParts.Length - 1];

                    if (clientID == Convert.ToInt32(receivedClientID))
                    {
                        listBoxOutput.Items.Add("Server Event: Light - Change " + messageParts[2] + " to " + messageParts[3]);

                        // Call the change lights function to alter the lights.
                        ChangeLights(messageParts[2], messageParts[3]);

                        // Send back to the server stating that we have completed the light change.
                        string lightDoneOptions = messageParts[2] + " " + messageParts[3];
                        sendString("LightDone", lightDoneOptions, messageParts[0]);
                    }

                    break;


                case "LightSequence":

                    // Get the client ID this message is meant for.
                    receivedClientID = messageParts[messageParts.Length - 1];

                    if (clientID == Convert.ToInt32(receivedClientID))
                    {
                        listBoxOutput.Items.Add("Server Event: LightSequence - Starting light sequence.");

                        SendLightSequence();
                    }

                    break;


                case "CarUpdate":

                    // Get the client ID this message is meant for.
                    receivedClientID = messageParts[messageParts.Length - 1];

                    if (clientID == Convert.ToInt32(receivedClientID))
                    {
                        listBoxOutput.Items.Add("Server Event: Car Update - Received a total car update from server. Total cars at light: " + messageParts[2]);

                        lightCarsNumLabel.Text = messageParts[2];
                    }

                    break;
            }
        }


        /// <summary>
        /// Change the status of the lights.     
        /// </summary>
        /// <param name="command"></param>
        private void ChangeLights(string lightColour, string lightState)
        {
            // Turn on/off the appropriate light given the contents
            // of the light colour and its state.
            if (lightColour == "Red")
            {
                if (lightState == "On")
                {
                    labelRed.Visible = true;
                }
                else if (lightState == "Off")
                {
                    labelRed.Visible = false;
                }
            }
            else if (lightColour == "Amber")
            {
                if (lightState == "On")
                {
                    labelAmber.Visible = true;
                }
                else if (lightState == "Off")
                {
                    labelAmber.Visible = false;
                }
            }
            else if (lightColour == "Green")
            {
                if (lightState == "On")
                {
                    labelGreen.Visible = true;

                }
                else if (lightState == "Off")
                {
                    labelGreen.Visible = false;
                }
            }

            listBoxOutput.Items.Add("Changed " + lightColour + " to " + lightState + ".");
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
        /// <param name="lightEvent"></param>
        /// <param name="lightEventOptions"></param>
        /// <param name="sendToIP"></param>
        private void sendString(string lightEvent, string lightEventOptions, string sendToIP)
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

                // Construct the final message to send.
                string finalMessage = lightEvent;

                // Add the event options to the final message if it is present.
                if (lightEventOptions.Length > 0)
                {
                    finalMessage += " " + lightEventOptions;
                }

                // If the client id has been set then use it, 
                // otherwise do not include it in the message.
                if (clientID != -1)
                {
                    finalMessage += " " + clientID.ToString();
                }

                // Start assembling message
                int bufferIndex = 4;

                // Turn the string into an array of characters.
                int length = finalMessage.Length;
                char[] chars = finalMessage.ToCharArray();

            
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
                //listBoxOutput.Items.Add(string.Join(", ", packet));
                
                // Write the packet into our outgoing binary stream.
                outStream.Write(packet, 0, bufferSize);
                listBoxOutput.Items.Add("Debug: Sent " + finalMessage);
            }
            catch (Exception err)
            {
                listBoxOutput.Items.Add("An error occurred: " + err.Message);
            }
        }


        /// <summary>
        /// Form closing; if the connection thread was ever created, then kill it off.       
        /// </summary>                                                    
        private void FormTrafficLight_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (threadConnection != null)
            {
                threadConnection.StopThread();
            }
        }
    }   // End of classy class.
}   // End of namespace
