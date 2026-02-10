namespace Snobol4W
{
    partial class MainForm
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
            textConsole = new TextBox();
            btnSelectFiles = new Button();
            textSelectedFiles = new TextBox();
            btnRun = new Button();
            tabControl1 = new TabControl();
            tabOutput = new TabPage();
            textConsoleRTF = new RichTextBox();
            tabError = new TabPage();
            textErrorsRTF = new RichTextBox();
            textCommands = new TextBox();
            label1 = new Label();
            buttonClear = new Button();
            tabControl1.SuspendLayout();
            tabOutput.SuspendLayout();
            tabError.SuspendLayout();
            SuspendLayout();
            // 
            // textConsole
            // 
            textConsole.Dock = DockStyle.Fill;
            textConsole.Font = new Font("Lucida Sans Typewriter", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textConsole.Location = new Point(3, 4);
            textConsole.Margin = new Padding(3, 4, 3, 4);
            textConsole.Multiline = true;
            textConsole.Name = "textConsole";
            textConsole.ScrollBars = ScrollBars.Both;
            textConsole.Size = new Size(994, 102);
            textConsole.TabIndex = 0;
            // 
            // btnSelectFiles
            // 
            btnSelectFiles.Location = new Point(933, 58);
            btnSelectFiles.Margin = new Padding(3, 4, 3, 4);
            btnSelectFiles.Name = "btnSelectFiles";
            btnSelectFiles.Size = new Size(86, 30);
            btnSelectFiles.TabIndex = 1;
            btnSelectFiles.Text = "Select Files";
            btnSelectFiles.UseVisualStyleBackColor = true;
            btnSelectFiles.Click += btnSelectFiles_Click;
            // 
            // textSelectedFiles
            // 
            textSelectedFiles.Font = new Font("Lucida Sans Typewriter", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textSelectedFiles.Location = new Point(14, 18);
            textSelectedFiles.Margin = new Padding(3, 4, 3, 4);
            textSelectedFiles.Multiline = true;
            textSelectedFiles.Name = "textSelectedFiles";
            textSelectedFiles.ScrollBars = ScrollBars.Both;
            textSelectedFiles.Size = new Size(915, 110);
            textSelectedFiles.TabIndex = 2;
            textSelectedFiles.Text = "\\\\wsl.localhost\\Ubuntu\\home\\jcooper\\x64-main\\bin\\test.spt";
            // 
            // btnRun
            // 
            btnRun.Location = new Point(933, 96);
            btnRun.Margin = new Padding(3, 4, 3, 4);
            btnRun.Name = "btnRun";
            btnRun.Size = new Size(86, 30);
            btnRun.TabIndex = 1;
            btnRun.Text = "Run";
            btnRun.UseVisualStyleBackColor = true;
            btnRun.Click += btnRun_Click;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabOutput);
            tabControl1.Controls.Add(tabError);
            tabControl1.Font = new Font("Cascadia Code", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            tabControl1.Location = new Point(14, 187);
            tabControl1.Margin = new Padding(3, 4, 3, 4);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1008, 143);
            tabControl1.TabIndex = 4;
            tabControl1.MouseDown += tabControl1_MouseDown;
            // 
            // tabOutput
            // 
            tabOutput.Controls.Add(textConsoleRTF);
            tabOutput.Controls.Add(textConsole);
            tabOutput.Location = new Point(4, 29);
            tabOutput.Margin = new Padding(3, 4, 3, 4);
            tabOutput.Name = "tabOutput";
            tabOutput.Padding = new Padding(3, 4, 3, 4);
            tabOutput.Size = new Size(1000, 110);
            tabOutput.TabIndex = 0;
            tabOutput.Text = "Output";
            tabOutput.UseVisualStyleBackColor = true;
            // 
            // textConsoleRTF
            // 
            textConsoleRTF.Dock = DockStyle.Fill;
            textConsoleRTF.Font = new Font("Lucida Sans Typewriter", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textConsoleRTF.HideSelection = false;
            textConsoleRTF.Location = new Point(3, 4);
            textConsoleRTF.Margin = new Padding(2);
            textConsoleRTF.Name = "textConsoleRTF";
            textConsoleRTF.Size = new Size(994, 102);
            textConsoleRTF.TabIndex = 1;
            textConsoleRTF.Text = "";
            textConsoleRTF.WordWrap = false;
            // 
            // tabError
            // 
            tabError.Controls.Add(textErrorsRTF);
            tabError.Location = new Point(4, 29);
            tabError.Margin = new Padding(3, 4, 3, 4);
            tabError.Name = "tabError";
            tabError.Padding = new Padding(3, 4, 3, 4);
            tabError.Size = new Size(1000, 110);
            tabError.TabIndex = 1;
            tabError.Text = "Listing";
            tabError.UseVisualStyleBackColor = true;
            // 
            // textErrorsRTF
            // 
            textErrorsRTF.Dock = DockStyle.Fill;
            textErrorsRTF.Font = new Font("Lucida Sans Typewriter", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textErrorsRTF.HideSelection = false;
            textErrorsRTF.Location = new Point(3, 4);
            textErrorsRTF.Margin = new Padding(2);
            textErrorsRTF.Name = "textErrorsRTF";
            textErrorsRTF.Size = new Size(994, 102);
            textErrorsRTF.TabIndex = 7;
            textErrorsRTF.Text = "";
            textErrorsRTF.WordWrap = false;
            // 
            // textCommands
            // 
            textCommands.Font = new Font("Lucida Sans Typewriter", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textCommands.Location = new Point(95, 146);
            textCommands.Margin = new Padding(3, 4, 3, 4);
            textCommands.Name = "textCommands";
            textCommands.Size = new Size(834, 25);
            textCommands.TabIndex = 5;
            textCommands.Text = "-c -x -v";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(22, 150);
            label1.Name = "label1";
            label1.Size = new Size(71, 20);
            label1.TabIndex = 6;
            label1.Text = "Comands";
            label1.TextAlign = ContentAlignment.TopCenter;
            // 
            // buttonClear
            // 
            buttonClear.Location = new Point(933, 19);
            buttonClear.Margin = new Padding(3, 4, 3, 4);
            buttonClear.Name = "buttonClear";
            buttonClear.Size = new Size(86, 30);
            buttonClear.TabIndex = 1;
            buttonClear.Text = "Clear Files";
            buttonClear.UseVisualStyleBackColor = true;
            buttonClear.Click += btnClearFiles_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1037, 690);
            Controls.Add(label1);
            Controls.Add(textCommands);
            Controls.Add(tabControl1);
            Controls.Add(textSelectedFiles);
            Controls.Add(btnRun);
            Controls.Add(buttonClear);
            Controls.Add(btnSelectFiles);
            Margin = new Padding(3, 4, 3, 4);
            Name = "MainForm";
            Text = "Form1";
            Load += MainForm_Load;
            Layout += MainForm_Layout;
            tabControl1.ResumeLayout(false);
            tabOutput.ResumeLayout(false);
            tabOutput.PerformLayout();
            tabError.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textConsole;
        private Button btnSelectFiles;
        private TextBox textSelectedFiles;
        private Button btnRun;
        private TabControl tabControl1;
        private TabPage tabOutput;
        private TabPage tabError;
        private TextBox textCommands;
        private Label label1;
        private RichTextBox textConsoleRTF;
        private RichTextBox textErrorsRTF;
        private Button buttonClear;
    }
}
