using System;
using System.IO;
using System.Net;
using System.Text;
using DNA.CastleMinerZ.Globalization;

namespace DNA.CastleMinerZ
{
	public class PostToWall
	{
		public string ErrorMessage { get; private set; }

		public string PostID { get; private set; }

		private string AppendQueryString(string query, string data)
		{
			return "&" + query + "=" + data;
		}

		public void Post()
		{
			if (string.IsNullOrEmpty(this.Message))
			{
				return;
			}
			string url = "https://graph.facebook.com/me/feed?access_token=" + this.AccessToken;
			string parameters = string.Concat(new string[]
			{
				"?name=name",
				this.AppendQueryString("link", this.Link),
				this.AppendQueryString("caption", this.Caption),
				this.AppendQueryString("description", this.Description),
				this.AppendQueryString("source", this.ImageURL),
				this.AppendQueryString("actions", string.Concat(new string[] { "{\"name\": \"", this.ActionName, "\", \"link\": \"", this.ActionURL, "\"}" })),
				this.AppendQueryString("message", this.Message)
			});
			WebRequest webRequest = WebRequest.Create(url);
			webRequest.ContentType = "application/x-www-form-urlencoded";
			webRequest.Method = "POST";
			byte[] bytes = Encoding.ASCII.GetBytes(parameters);
			webRequest.ContentLength = (long)bytes.Length;
			Stream os = webRequest.GetRequestStream();
			os.Write(bytes, 0, bytes.Length);
			os.Close();
			try
			{
				WebResponse webResponse = webRequest.GetResponse();
				StreamReader sr = null;
				try
				{
					sr = new StreamReader(webResponse.GetResponseStream());
					this.PostID = sr.ReadToEnd();
				}
				finally
				{
					if (sr != null)
					{
						sr.Close();
					}
				}
			}
			catch (WebException ex)
			{
				StreamReader errorStream = null;
				try
				{
					errorStream = new StreamReader(ex.Response.GetResponseStream());
					this.ErrorMessage = errorStream.ReadToEnd();
				}
				finally
				{
					if (errorStream != null)
					{
						errorStream.Close();
					}
				}
			}
		}

		public string Message = "";

		public string AccessToken = "";

		public string Link = "";

		public string Caption = "";

		public string Description = "";

		public string ImageURL = "http://digitaldnagames.com/Images/DNABanner.png";

		public string ActionName = Strings.Create_Account;

		public string ActionURL = "http://digitaldnagames.com/Account/FacebookRegister.aspx";
	}
}
