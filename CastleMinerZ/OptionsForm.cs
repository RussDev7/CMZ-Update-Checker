using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace DNA.CastleMinerZ
{
	public partial class OptionsForm : Form
	{
		public bool FullScreenMode
		{
			get
			{
				return this.fullScreenCheckBox.Checked;
			}
			set
			{
				this.fullScreenCheckBox.Checked = value;
			}
		}

		public bool AskForFacebook
		{
			get
			{
				return this.facebookCheckBox.Checked;
			}
			set
			{
				this.facebookCheckBox.Checked = value;
			}
		}

		public OptionsForm()
		{
			this.InitializeComponent();
		}
	}
}
