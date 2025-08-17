namespace DNA.CastleMinerZ
{
	public partial class OptionsForm : global::System.Windows.Forms.Form
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
			global::System.ComponentModel.ComponentResourceManager componentResourceManager = new global::System.ComponentModel.ComponentResourceManager(typeof(global::DNA.CastleMinerZ.OptionsForm));
			this.okButton = new global::System.Windows.Forms.Button();
			this.cancelButton = new global::System.Windows.Forms.Button();
			this.fullScreenCheckBox = new global::System.Windows.Forms.CheckBox();
			this.facebookCheckBox = new global::System.Windows.Forms.CheckBox();
			base.SuspendLayout();
			componentResourceManager.ApplyResources(this.okButton, "okButton");
			this.okButton.DialogResult = global::System.Windows.Forms.DialogResult.OK;
			this.okButton.Name = "okButton";
			this.okButton.UseVisualStyleBackColor = true;
			componentResourceManager.ApplyResources(this.cancelButton, "cancelButton");
			this.cancelButton.DialogResult = global::System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.UseVisualStyleBackColor = true;
			componentResourceManager.ApplyResources(this.fullScreenCheckBox, "fullScreenCheckBox");
			this.fullScreenCheckBox.Name = "fullScreenCheckBox";
			this.fullScreenCheckBox.UseVisualStyleBackColor = true;
			componentResourceManager.ApplyResources(this.facebookCheckBox, "facebookCheckBox");
			this.facebookCheckBox.Checked = true;
			this.facebookCheckBox.CheckState = global::System.Windows.Forms.CheckState.Checked;
			this.facebookCheckBox.Name = "facebookCheckBox";
			this.facebookCheckBox.UseVisualStyleBackColor = true;
			base.AcceptButton = this.okButton;
			componentResourceManager.ApplyResources(this, "$this");
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Font;
			base.CancelButton = this.cancelButton;
			base.Controls.Add(this.facebookCheckBox);
			base.Controls.Add(this.fullScreenCheckBox);
			base.Controls.Add(this.cancelButton);
			base.Controls.Add(this.okButton);
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "OptionsForm";
			base.ShowInTaskbar = false;
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		private global::System.ComponentModel.IContainer components;

		private global::System.Windows.Forms.Button okButton;

		private global::System.Windows.Forms.Button cancelButton;

		private global::System.Windows.Forms.CheckBox fullScreenCheckBox;

		private global::System.Windows.Forms.CheckBox facebookCheckBox;
	}
}
