﻿namespace TrafficLightServer
{
    partial class FormServer
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
            this.components = new System.ComponentModel.Container();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.listBoxOutput = new System.Windows.Forms.ListBox();
            this.labelStatus = new System.Windows.Forms.Label();
            this.lightOneIPTextBox = new System.Windows.Forms.TextBox();
            this.lightOneIPLabel = new System.Windows.Forms.Label();
            this.comboBoxLightColour = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBoxLightState = new System.Windows.Forms.ComboBox();
            this.sendCommandButton = new System.Windows.Forms.Button();
            this.drawingPanel = new System.Windows.Forms.Panel();
            this.lightOneCarsLabel = new System.Windows.Forms.Label();
            this.lightOneNumLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lightTwoIPTextBox = new System.Windows.Forms.TextBox();
            this.lightTwoNumLabel = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lightFourNumLabel = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lightFourIPTextBox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.lightThreeNumLabel = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.lightThreeIPTextBox = new System.Windows.Forms.TextBox();
            this.trafficTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new System.Drawing.Point(14, 37);
            this.buttonConnect.Margin = new System.Windows.Forms.Padding(2);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(214, 31);
            this.buttonConnect.TabIndex = 0;
            this.buttonConnect.Text = "Connect to Proxy";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // listBoxOutput
            // 
            this.listBoxOutput.FormattingEnabled = true;
            this.listBoxOutput.HorizontalScrollbar = true;
            this.listBoxOutput.Location = new System.Drawing.Point(14, 378);
            this.listBoxOutput.Margin = new System.Windows.Forms.Padding(2);
            this.listBoxOutput.Name = "listBoxOutput";
            this.listBoxOutput.Size = new System.Drawing.Size(380, 225);
            this.listBoxOutput.TabIndex = 1;
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStatus.Location = new System.Drawing.Point(11, 9);
            this.labelStatus.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(158, 18);
            this.labelStatus.TabIndex = 2;
            this.labelStatus.Text = "Status: Not Connected";
            // 
            // lightOneIPTextBox
            // 
            this.lightOneIPTextBox.Enabled = false;
            this.lightOneIPTextBox.Location = new System.Drawing.Point(135, 90);
            this.lightOneIPTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.lightOneIPTextBox.Name = "lightOneIPTextBox";
            this.lightOneIPTextBox.Size = new System.Drawing.Size(93, 20);
            this.lightOneIPTextBox.TabIndex = 3;
            // 
            // lightOneIPLabel
            // 
            this.lightOneIPLabel.AutoSize = true;
            this.lightOneIPLabel.Location = new System.Drawing.Point(16, 93);
            this.lightOneIPLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lightOneIPLabel.Name = "lightOneIPLabel";
            this.lightOneIPLabel.Size = new System.Drawing.Size(101, 13);
            this.lightOneIPLabel.TabIndex = 4;
            this.lightOneIPLabel.Text = "Light 1 - IP address:";
            // 
            // comboBoxLightColour
            // 
            this.comboBoxLightColour.Enabled = false;
            this.comboBoxLightColour.FormattingEnabled = true;
            this.comboBoxLightColour.Items.AddRange(new object[] {
            "Red",
            "Amber",
            "Green"});
            this.comboBoxLightColour.Location = new System.Drawing.Point(353, 87);
            this.comboBoxLightColour.Margin = new System.Windows.Forms.Padding(2);
            this.comboBoxLightColour.Name = "comboBoxLightColour";
            this.comboBoxLightColour.Size = new System.Drawing.Size(51, 21);
            this.comboBoxLightColour.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(258, 90);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(92, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Set colour of light:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(284, 122);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(66, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Light Status:";
            // 
            // comboBoxLightState
            // 
            this.comboBoxLightState.Enabled = false;
            this.comboBoxLightState.FormattingEnabled = true;
            this.comboBoxLightState.Items.AddRange(new object[] {
            "On",
            "Off"});
            this.comboBoxLightState.Location = new System.Drawing.Point(352, 119);
            this.comboBoxLightState.Name = "comboBoxLightState";
            this.comboBoxLightState.Size = new System.Drawing.Size(52, 21);
            this.comboBoxLightState.TabIndex = 9;
            // 
            // sendCommandButton
            // 
            this.sendCommandButton.Enabled = false;
            this.sendCommandButton.Location = new System.Drawing.Point(249, 170);
            this.sendCommandButton.Name = "sendCommandButton";
            this.sendCommandButton.Size = new System.Drawing.Size(155, 23);
            this.sendCommandButton.TabIndex = 10;
            this.sendCommandButton.Text = "Send Command";
            this.sendCommandButton.UseVisualStyleBackColor = true;
            this.sendCommandButton.Click += new System.EventHandler(this.sendCommandButton_Click);
            // 
            // drawingPanel
            // 
            this.drawingPanel.Location = new System.Drawing.Point(421, 37);
            this.drawingPanel.Name = "drawingPanel";
            this.drawingPanel.Size = new System.Drawing.Size(763, 573);
            this.drawingPanel.TabIndex = 11;
            // 
            // lightOneCarsLabel
            // 
            this.lightOneCarsLabel.AutoSize = true;
            this.lightOneCarsLabel.Location = new System.Drawing.Point(16, 127);
            this.lightOneCarsLabel.Name = "lightOneCarsLabel";
            this.lightOneCarsLabel.Size = new System.Drawing.Size(77, 13);
            this.lightOneCarsLabel.TabIndex = 12;
            this.lightOneCarsLabel.Text = "Cars at light 1: ";
            // 
            // lightOneNumLabel
            // 
            this.lightOneNumLabel.AutoSize = true;
            this.lightOneNumLabel.Location = new System.Drawing.Point(132, 127);
            this.lightOneNumLabel.Name = "lightOneNumLabel";
            this.lightOneNumLabel.Size = new System.Drawing.Size(13, 13);
            this.lightOneNumLabel.TabIndex = 13;
            this.lightOneNumLabel.Text = "0";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 157);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 13);
            this.label1.TabIndex = 14;
            this.label1.Text = "Light 2 - IP address:";
            // 
            // lightTwoIPTextBox
            // 
            this.lightTwoIPTextBox.Enabled = false;
            this.lightTwoIPTextBox.Location = new System.Drawing.Point(135, 154);
            this.lightTwoIPTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.lightTwoIPTextBox.Name = "lightTwoIPTextBox";
            this.lightTwoIPTextBox.Size = new System.Drawing.Size(93, 20);
            this.lightTwoIPTextBox.TabIndex = 15;
            // 
            // lightTwoNumLabel
            // 
            this.lightTwoNumLabel.AutoSize = true;
            this.lightTwoNumLabel.Location = new System.Drawing.Point(132, 194);
            this.lightTwoNumLabel.Name = "lightTwoNumLabel";
            this.lightTwoNumLabel.Size = new System.Drawing.Size(13, 13);
            this.lightTwoNumLabel.TabIndex = 17;
            this.lightTwoNumLabel.Text = "0";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(16, 194);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(77, 13);
            this.label5.TabIndex = 16;
            this.label5.Text = "Cars at light 2: ";
            // 
            // lightFourNumLabel
            // 
            this.lightFourNumLabel.AutoSize = true;
            this.lightFourNumLabel.Location = new System.Drawing.Point(132, 329);
            this.lightFourNumLabel.Name = "lightFourNumLabel";
            this.lightFourNumLabel.Size = new System.Drawing.Size(13, 13);
            this.lightFourNumLabel.TabIndex = 25;
            this.lightFourNumLabel.Text = "0";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(16, 329);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(77, 13);
            this.label7.TabIndex = 24;
            this.label7.Text = "Cars at light 4: ";
            // 
            // lightFourIPTextBox
            // 
            this.lightFourIPTextBox.Enabled = false;
            this.lightFourIPTextBox.Location = new System.Drawing.Point(135, 289);
            this.lightFourIPTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.lightFourIPTextBox.Name = "lightFourIPTextBox";
            this.lightFourIPTextBox.Size = new System.Drawing.Size(93, 20);
            this.lightFourIPTextBox.TabIndex = 23;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(16, 292);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(101, 13);
            this.label8.TabIndex = 22;
            this.label8.Text = "Light 4 - IP address:";
            // 
            // lightThreeNumLabel
            // 
            this.lightThreeNumLabel.AutoSize = true;
            this.lightThreeNumLabel.Location = new System.Drawing.Point(132, 262);
            this.lightThreeNumLabel.Name = "lightThreeNumLabel";
            this.lightThreeNumLabel.Size = new System.Drawing.Size(13, 13);
            this.lightThreeNumLabel.TabIndex = 21;
            this.lightThreeNumLabel.Text = "0";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(16, 262);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(77, 13);
            this.label10.TabIndex = 20;
            this.label10.Text = "Cars at light 3: ";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(16, 228);
            this.label11.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(101, 13);
            this.label11.TabIndex = 19;
            this.label11.Text = "Light 3 - IP address:";
            // 
            // lightThreeIPTextBox
            // 
            this.lightThreeIPTextBox.Enabled = false;
            this.lightThreeIPTextBox.Location = new System.Drawing.Point(135, 225);
            this.lightThreeIPTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.lightThreeIPTextBox.Name = "lightThreeIPTextBox";
            this.lightThreeIPTextBox.Size = new System.Drawing.Size(93, 20);
            this.lightThreeIPTextBox.TabIndex = 18;
            // 
            // trafficTimer
            // 
            this.trafficTimer.Interval = 5000;
            this.trafficTimer.Tick += new System.EventHandler(this.trafficTimer_Tick);
            // 
            // FormServer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1196, 622);
            this.Controls.Add(this.lightFourNumLabel);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.lightFourIPTextBox);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.lightThreeNumLabel);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.lightThreeIPTextBox);
            this.Controls.Add(this.lightTwoNumLabel);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.lightTwoIPTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lightOneNumLabel);
            this.Controls.Add(this.lightOneCarsLabel);
            this.Controls.Add(this.drawingPanel);
            this.Controls.Add(this.sendCommandButton);
            this.Controls.Add(this.comboBoxLightState);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBoxLightColour);
            this.Controls.Add(this.lightOneIPLabel);
            this.Controls.Add(this.lightOneIPTextBox);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.listBoxOutput);
            this.Controls.Add(this.buttonConnect);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "FormServer";
            this.Text = "Server (sort of)";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormServer_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.ListBox listBoxOutput;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.TextBox lightOneIPTextBox;
        private System.Windows.Forms.Label lightOneIPLabel;
        private System.Windows.Forms.ComboBox comboBoxLightColour;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBoxLightState;
        private System.Windows.Forms.Button sendCommandButton;
        private System.Windows.Forms.Panel drawingPanel;
        private System.Windows.Forms.Label lightOneCarsLabel;
        private System.Windows.Forms.Label lightOneNumLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox lightTwoIPTextBox;
        private System.Windows.Forms.Label lightTwoNumLabel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lightFourNumLabel;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox lightFourIPTextBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label lightThreeNumLabel;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox lightThreeIPTextBox;
        private System.Windows.Forms.Timer trafficTimer;
    }
}

