using System;
using System.IO;
using DNA.CastleMinerZ.GraphicsProfileSupport;
using DNA.Drawing;
using DNA.IO;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ
{
	public class GlobalSettings
	{
		public GlobalSettings()
		{
			if (GraphicsProfileManager.Instance.Profile == GraphicsProfile.Reach)
			{
				this.TextureQualityLevel = 2;
			}
		}

		public static string GetAppDataDirectory(string subdir)
		{
			string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			string text = Path.Combine(folderPath, "CastleMinerZ");
			if (!string.IsNullOrEmpty(subdir))
			{
				text = Path.Combine(text, subdir);
			}
			return text;
		}

		public static string GetAppDataDirectory()
		{
			return GlobalSettings.GetAppDataDirectory(null);
		}

		public void Save()
		{
			HTFDocument htfdocument = new HTFDocument();
			string appDataDirectory = GlobalSettings.GetAppDataDirectory();
			if (!Directory.Exists(appDataDirectory))
			{
				Directory.CreateDirectory(appDataDirectory);
			}
			string text = Path.Combine(appDataDirectory, "user.settings");
			htfdocument.Children.Add(new HTFElement("FullScreen", this.FullScreen.ToString()));
			htfdocument.Children.Add(new HTFElement("AskForFacebook", this.AskForFacebook.ToString()));
			htfdocument.Children.Add(new HTFElement("ScreenWidth", this.ScreenSize.Width.ToString()));
			htfdocument.Children.Add(new HTFElement("ScreenHeight", this.ScreenSize.Height.ToString()));
			htfdocument.Children.Add(new HTFElement("TextureQuality", this.TextureQualityLevel.ToString()));
			htfdocument.Save(text);
		}

		public void Load()
		{
			try
			{
				HTFDocument htfdocument = new HTFDocument();
				string appDataDirectory = GlobalSettings.GetAppDataDirectory();
				string text = Path.Combine(appDataDirectory, "user.settings");
				htfdocument.Load(text);
				for (int i = 0; i < htfdocument.Children.Count; i++)
				{
					string id;
					if ((id = htfdocument.Children[i].ID) != null)
					{
						if (!(id == "FullScreen"))
						{
							if (!(id == "AskForFacebook"))
							{
								if (!(id == "ScreenWidth"))
								{
									if (!(id == "ScreenHeight"))
									{
										if (id == "TextureQuality")
										{
											this.TextureQualityLevel = int.Parse(htfdocument.Children[i].Value);
										}
									}
									else
									{
										this.ScreenSize.Height = int.Parse(htfdocument.Children[i].Value);
									}
								}
								else
								{
									this.ScreenSize.Width = int.Parse(htfdocument.Children[i].Value);
								}
							}
							else
							{
								this.AskForFacebook = bool.Parse(htfdocument.Children[i].Value);
							}
						}
						else
						{
							this.FullScreen = bool.Parse(htfdocument.Children[i].Value);
						}
					}
				}
			}
			catch
			{
			}
		}

		public bool FullScreen;

		public bool AskForFacebook = true;

		public Size ScreenSize = new Size(1280, 720);

		public int TextureQualityLevel = 1;
	}
}
