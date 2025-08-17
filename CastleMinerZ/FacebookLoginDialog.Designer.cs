namespace DNA.CastleMinerZ
{
	public partial class FacebookLoginDialog : global::System.Windows.Forms.Form
	{
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			global::System.ComponentModel.ComponentResourceManager componentResourceManager = new global::System.ComponentModel.ComponentResourceManager(typeof(global::DNA.CastleMinerZ.FacebookLoginDialog));
			this.webBrowser = new global::System.Windows.Forms.WebBrowser();
			base.SuspendLayout();
			componentResourceManager.ApplyResources(this.webBrowser, "webBrowser");
			this.webBrowser.MinimumSize = new global::System.Drawing.Size(20, 20);
			this.webBrowser.Name = "webBrowser";
			this.webBrowser.ScrollBarsEnabled = false;
			this.webBrowser.Navigated += new global::System.Windows.Forms.WebBrowserNavigatedEventHandler(this.webBrowser_Navigated);
			componentResourceManager.ApplyResources(this, "$this");
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.Controls.Add(this.webBrowser);
			base.FormBorderStyle = global::System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "FacebookLoginDialog";
			base.ShowInTaskbar = false;
			base.Load += new global::System.EventHandler(this.FacebookLoginDialog_Load);
			base.ResumeLayout(false);
		}

		private global::System.ComponentModel.IContainer components;

		private global::System.Windows.Forms.WebBrowser webBrowser;
	}
}
