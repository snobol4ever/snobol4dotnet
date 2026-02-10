namespace Snobol4W
{
    partial class InputForm
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
            textBox1 = new TextBox();
            buttonOK = new Button();
            buttonEOF = new Button();
            SuspendLayout();
            // 
            // textBox1
            // 
            textBox1.AcceptsTab = true;
            textBox1.Location = new Point(17, 20);
            textBox1.Margin = new Padding(4, 5, 4, 5);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(793, 31);
            textBox1.TabIndex = 0;
            // 
            // buttonOK
            // 
            buttonOK.Location = new Point(846, 20);
            buttonOK.Margin = new Padding(4, 5, 4, 5);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new Size(107, 38);
            buttonOK.TabIndex = 1;
            buttonOK.Text = "OK";
            buttonOK.UseVisualStyleBackColor = true;
            // 
            // buttonEOF
            // 
            buttonEOF.Location = new Point(961, 20);
            buttonEOF.Margin = new Padding(4, 5, 4, 5);
            buttonEOF.Name = "buttonEOF";
            buttonEOF.Size = new Size(107, 38);
            buttonEOF.TabIndex = 2;
            buttonEOF.Text = "EOF";
            buttonEOF.UseVisualStyleBackColor = true;
            // 
            // InputForm
            // 
            AcceptButton = buttonOK;
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1100, 85);
            ControlBox = false;
            Controls.Add(buttonEOF);
            Controls.Add(buttonOK);
            Controls.Add(textBox1);
            Margin = new Padding(4, 5, 4, 5);
            Name = "InputForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Waiting for input ...";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBox1;
        private Button buttonOK;
        private Button buttonEOF;
    }
}