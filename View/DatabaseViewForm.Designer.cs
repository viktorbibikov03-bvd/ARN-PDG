namespace View
{
    partial class DatabaseViewForm
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

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // DatabaseViewForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 800);
            this.Name = "DatabaseViewForm";
            this.Text = "Просмотр данных из базы данных";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DatabaseViewForm_FormClosing);
            this.ResumeLayout(false);
        }
    }
}