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
			string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			string dataPath = Path.Combine(appData, "CastleMinerZ");
			if (!string.IsNullOrEmpty(subdir))
			{
				dataPath = Path.Combine(dataPath, subdir);
			}
			return dataPath;
		}

		public static string GetAppDataDirectory()
		{
			return GlobalSettings.GetAppDataDirectory(null);
		}

		public void Save()
		{
			HTFDocument settingsDoc = new HTFDocument();
			string dataPath = GlobalSettings.GetAppDataDirectory();
			if (!Directory.Exists(dataPath))
			{
				Directory.CreateDirectory(dataPath);
			}
			string fullSavePath = Path.Combine(dataPath, "user.settings");
			settingsDoc.Children.Add(new HTFElement("FullScreen", this.FullScreen.ToString()));
			settingsDoc.Children.Add(new HTFElement("AskForFacebook", this.AskForFacebook.ToString()));
			settingsDoc.Children.Add(new HTFElement("ScreenWidth", this.ScreenSize.Width.ToString()));
			settingsDoc.Children.Add(new HTFElement("ScreenHeight", this.ScreenSize.Height.ToString()));
			settingsDoc.Children.Add(new HTFElement("TextureQuality", this.TextureQualityLevel.ToString()));
			settingsDoc.Save(fullSavePath);
		}

		public void Load()
		{
			try
			{
				HTFDocument settingsDoc = new HTFDocument();
				string dataPath = GlobalSettings.GetAppDataDirectory();
				string fullSavePath = Path.Combine(dataPath, "user.settings");
				settingsDoc.Load(fullSavePath);
				for (int i = 0; i < settingsDoc.Children.Count; i++)
				{
					string id;
					if ((id = settingsDoc.Children[i].ID) != null)
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
											this.TextureQualityLevel = int.Parse(settingsDoc.Children[i].Value);
										}
									}
									else
									{
										this.ScreenSize.Height = int.Parse(settingsDoc.Children[i].Value);
									}
								}
								else
								{
									this.ScreenSize.Width = int.Parse(settingsDoc.Children[i].Value);
								}
							}
							else
							{
								this.AskForFacebook = bool.Parse(settingsDoc.Children[i].Value);
							}
						}
						else
						{
							this.FullScreen = bool.Parse(settingsDoc.Children[i].Value);
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
