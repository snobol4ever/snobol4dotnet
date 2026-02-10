namespace Snobol4W
{
    partial class Splash
    {
                                private System.ComponentModel.IContainer components = null;

                                        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

                                        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            timer1 = new System.Windows.Forms.Timer(components);
            //progressBar1 = new ProgressBar();
            label1 = new Label();
            SuspendLayout();
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Interval = 500;
            timer1.Tick += timer1_Tick;
            // 
            // progressBar1
            // 
            //progressBar1.Location = new Point(16, 168);
            //progressBar1.Margin = new Padding(1, 1, 1, 1);
            //progressBar1.MarqueeAnimationSpeed = 0;
            //progressBar1.Name = "progressBar1";
            //progressBar1.Size = new Size(529, 6);
            //progressBar1.Style = ProgressBarStyle.Marquee;
            //progressBar1.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI Semibold", 9.900001F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.YellowGreen;
            label1.Location = new Point(472, 130);
            label1.Margin = new Padding(2, 2, 2, 2);
            label1.Name = "label1";
            label1.Size = new Size(47, 28);
            label1.TabIndex = 1;
            label1.Text = ".net";
            label1.Click += label1_Click;
            // 
            // Splash
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ButtonHighlight;
            BackgroundImage = Properties.Resources.Gimpel;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(574, 178);
            Controls.Add(label1);
            //Controls.Add(progressBar1);
            ForeColor = Color.DarkBlue;
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(4, 5, 4, 5);
            Name = "Splash";
            Padding = new Padding(3, 3, 3, 3);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Splash";
            Load += Splash_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Timer timer1;
        //private ProgressBar progressBar1;
        private Label label1;
    }
}