namespace FireShip
{
    partial class ScreenView
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
            this.SuspendLayout();
            // 
            // ScreenView
            // 
            this.AccessibleName = "screenViewForm";
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.KeyPreview = true;
            this.Name = "ScreenView";
            this.Text = "FireShip";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ScreenView_FormClosed);
            this.Load += new System.EventHandler(this.ScreenView_Load);
            this.Shown += new System.EventHandler(this.ScreenView_Shown);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.ScreenView_Paint);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AnyKeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.AnyKeyUp);
            this.ResumeLayout(false);

        }

        #endregion
    }
}

