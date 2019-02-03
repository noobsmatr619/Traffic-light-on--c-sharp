namespace TrafficLight
{
    partial class FormTrafficLight
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.serverIPLabel = new System.Windows.Forms.Label();
            this.serverIPTextBox = new System.Windows.Forms.TextBox();
            this.labelStatus = new System.Windows.Forms.Label();
            this.listBoxOutput = new System.Windows.Forms.ListBox();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.labelRed = new System.Windows.Forms.Label();
            this.labelGreen = new System.Windows.Forms.Label();
            this.labelAmber = new System.Windows.Forms.Label();
            this.buttonCarArrived = new System.Windows.Forms.Button();
            this.carArrivedLabel = new System.Windows.Forms.Label();
            this.lightCarsNumLabel = new System.Windows.Forms.Label();
            this.traffic = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.clientIDLabel = new System.Windows.Forms.Label();
            this.clientIDNumLabel = new System.Windows.Forms.Label();
            this.clientIPHeadLabel = new System.Windows.Forms.Label();
            this.clientIPLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // serverIPLabel
            // 
            this.serverIPLabel.AutoSize = true;
            this.serverIPLabel.Location = new System.Drawing.Point(12, 81);
            this.serverIPLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.serverIPLabel.Name = "serverIPLabel";
            this.serverIPLabel.Size = new System.Drawing.Size(137, 13);
            this.serverIPLabel.TabIndex = 11;
            this.serverIPLabel.Text = "IP number of Traffic Server:";
            // 
            // serverIPTextBox
            // 
            this.serverIPTextBox.Location = new System.Drawing.Point(159, 78);
            this.serverIPTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.serverIPTextBox.Name = "serverIPTextBox";
            this.serverIPTextBox.Size = new System.Drawing.Size(129, 20);
            this.serverIPTextBox.TabIndex = 10;
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStatus.Location = new System.Drawing.Point(11, 19);
            this.labelStatus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(158, 18);
            this.labelStatus.TabIndex = 9;
            this.labelStatus.Text = "Status: Not Connected";
            // 
            // listBoxOutput
            // 
            this.listBoxOutput.FormattingEnabled = true;
            this.listBoxOutput.HorizontalScrollbar = true;
            this.listBoxOutput.Location = new System.Drawing.Point(324, 77);
            this.listBoxOutput.Margin = new System.Windows.Forms.Padding(2);
            this.listBoxOutput.Name = "listBoxOutput";
            this.listBoxOutput.Size = new System.Drawing.Size(491, 342);
            this.listBoxOutput.TabIndex = 8;
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new System.Drawing.Point(91, 112);
            this.buttonConnect.Margin = new System.Windows.Forms.Padding(2);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(126, 31);
            this.buttonConnect.TabIndex = 7;
            this.buttonConnect.Text = "Connect to Proxy";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // labelRed
            // 
            this.labelRed.AutoSize = true;
            this.labelRed.BackColor = System.Drawing.Color.Red;
            this.labelRed.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelRed.Location = new System.Drawing.Point(47, 240);
            this.labelRed.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelRed.Name = "labelRed";
            this.labelRed.Size = new System.Drawing.Size(39, 37);
            this.labelRed.TabIndex = 12;
            this.labelRed.Text = "R";
            this.labelRed.Visible = false;
            // 
            // labelGreen
            // 
            this.labelGreen.AutoSize = true;
            this.labelGreen.BackColor = System.Drawing.Color.Lime;
            this.labelGreen.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelGreen.Location = new System.Drawing.Point(47, 360);
            this.labelGreen.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelGreen.Name = "labelGreen";
            this.labelGreen.Size = new System.Drawing.Size(42, 37);
            this.labelGreen.TabIndex = 13;
            this.labelGreen.Text = "G";
            this.labelGreen.Visible = false;
            // 
            // labelAmber
            // 
            this.labelAmber.AutoSize = true;
            this.labelAmber.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.labelAmber.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAmber.Location = new System.Drawing.Point(47, 301);
            this.labelAmber.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelAmber.Name = "labelAmber";
            this.labelAmber.Size = new System.Drawing.Size(39, 37);
            this.labelAmber.TabIndex = 14;
            this.labelAmber.Text = "A";
            this.labelAmber.Visible = false;
            // 
            // buttonCarArrived
            // 
            this.buttonCarArrived.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.buttonCarArrived.Enabled = false;
            this.buttonCarArrived.Location = new System.Drawing.Point(162, 357);
            this.buttonCarArrived.Margin = new System.Windows.Forms.Padding(2);
            this.buttonCarArrived.Name = "buttonCarArrived";
            this.buttonCarArrived.Size = new System.Drawing.Size(105, 40);
            this.buttonCarArrived.TabIndex = 15;
            this.buttonCarArrived.Text = "Car Arrived";
            this.buttonCarArrived.UseVisualStyleBackColor = true;
            this.buttonCarArrived.Click += new System.EventHandler(this.buttonCarArrived_Click);
            // 
            // carArrivedLabel
            // 
            this.carArrivedLabel.AutoSize = true;
            this.carArrivedLabel.Location = new System.Drawing.Point(126, 249);
            this.carArrivedLabel.Name = "carArrivedLabel";
            this.carArrivedLabel.Size = new System.Drawing.Size(116, 13);
            this.carArrivedLabel.TabIndex = 16;
            this.carArrivedLabel.Text = "Number of cars at light:";
            // 
            // lightCarsNumLabel
            // 
            this.lightCarsNumLabel.AutoSize = true;
            this.lightCarsNumLabel.Location = new System.Drawing.Point(267, 249);
            this.lightCarsNumLabel.Name = "lightCarsNumLabel";
            this.lightCarsNumLabel.Size = new System.Drawing.Size(13, 13);
            this.lightCarsNumLabel.TabIndex = 18;
            this.lightCarsNumLabel.Text = "0";
            // 
            // traffic
            // 
            this.traffic.BackColor = System.Drawing.Color.Black;
            this.traffic.Location = new System.Drawing.Point(27, 219);
            this.traffic.Name = "traffic";
            this.traffic.Size = new System.Drawing.Size(80, 200);
            this.traffic.TabIndex = 19;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(138, 296);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(150, 42);
            this.label2.TabIndex = 21;
            this.label2.Text = "Press \"Car Arrived\" to indicate a car arriving at the traffic light.";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 49);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(563, 13);
            this.label3.TabIndex = 22;
            this.label3.Text = "NOTE: Enter the IP address of the Traffic Server and press connect. Disconnect fr" +
    "om proxy to change the IP address.";
            // 
            // clientIDLabel
            // 
            this.clientIDLabel.AutoSize = true;
            this.clientIDLabel.Location = new System.Drawing.Point(126, 219);
            this.clientIDLabel.Name = "clientIDLabel";
            this.clientIDLabel.Size = new System.Drawing.Size(109, 13);
            this.clientIDLabel.TabIndex = 23;
            this.clientIDLabel.Text = "Traffic Light Client ID:";
            // 
            // clientIDNumLabel
            // 
            this.clientIDNumLabel.AutoSize = true;
            this.clientIDNumLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.clientIDNumLabel.Location = new System.Drawing.Point(270, 219);
            this.clientIDNumLabel.Name = "clientIDNumLabel";
            this.clientIDNumLabel.Size = new System.Drawing.Size(37, 13);
            this.clientIDNumLabel.TabIndex = 24;
            this.clientIDNumLabel.Text = "None";
            // 
            // clientIPHeadLabel
            // 
            this.clientIPHeadLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.clientIPHeadLabel.Location = new System.Drawing.Point(39, 170);
            this.clientIPHeadLabel.Name = "clientIPHeadLabel";
            this.clientIPHeadLabel.Size = new System.Drawing.Size(59, 13);
            this.clientIPHeadLabel.TabIndex = 25;
            this.clientIPHeadLabel.Text = "Client IP:";
            // 
            // clientIPLabel
            // 
            this.clientIPLabel.AutoSize = true;
            this.clientIPLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.clientIPLabel.Location = new System.Drawing.Point(126, 170);
            this.clientIPLabel.Name = "clientIPLabel";
            this.clientIPLabel.Size = new System.Drawing.Size(0, 13);
            this.clientIPLabel.TabIndex = 26;
            // 
            // FormTrafficLight
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(823, 426);
            this.Controls.Add(this.clientIPLabel);
            this.Controls.Add(this.clientIPHeadLabel);
            this.Controls.Add(this.clientIDNumLabel);
            this.Controls.Add(this.clientIDLabel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lightCarsNumLabel);
            this.Controls.Add(this.carArrivedLabel);
            this.Controls.Add(this.buttonCarArrived);
            this.Controls.Add(this.labelAmber);
            this.Controls.Add(this.labelGreen);
            this.Controls.Add(this.labelRed);
            this.Controls.Add(this.serverIPLabel);
            this.Controls.Add(this.serverIPTextBox);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.listBoxOutput);
            this.Controls.Add(this.buttonConnect);
            this.Controls.Add(this.traffic);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "FormTrafficLight";
            this.Text = "Traffic Light - Client";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormTrafficLight_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label serverIPLabel;
        private System.Windows.Forms.TextBox serverIPTextBox;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.ListBox listBoxOutput;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.Label labelRed;
        private System.Windows.Forms.Label labelGreen;
        private System.Windows.Forms.Label labelAmber;
        private System.Windows.Forms.Button buttonCarArrived;
        private System.Windows.Forms.Label carArrivedLabel;
        private System.Windows.Forms.Label lightCarsNumLabel;
        private System.Windows.Forms.Label traffic;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label clientIDLabel;
        private System.Windows.Forms.Label clientIDNumLabel;
        private System.Windows.Forms.Label clientIPHeadLabel;
        private System.Windows.Forms.Label clientIPLabel;
    }
}

