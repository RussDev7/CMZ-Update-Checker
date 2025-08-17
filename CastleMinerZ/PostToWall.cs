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
			string text = "https://graph.facebook.com/me/feed?access_token=" + this.AccessToken;
			string text2 = string.Concat(new string[]
			{
				"?name=name",
				this.AppendQueryString("link", this.Link),
				this.AppendQueryString("caption", this.Caption),
				this.AppendQueryString("description", this.Description),
				this.AppendQueryString("source", this.ImageURL),
				this.AppendQueryString("actions", string.Concat(new string[] { "{\"name\": \"", this.ActionName, "\", \"link\": \"", this.ActionURL, "\"}" })),
				this.AppendQueryString("message", this.Message)
			});
			WebRequest webRequest = WebRequest.Create(text);
			webRequest.ContentType = "application/x-www-form-urlencoded";
			webRequest.Method = "POST";
			byte[] bytes = Encoding.ASCII.GetBytes(text2);
			webRequest.ContentLength = (long)bytes.Length;
			Stream requestStream = webRequest.GetRequestStream();
			requestStream.Write(bytes, 0, bytes.Length);
			requestStream.Close();
			try
			{
				WebResponse response = webRequest.GetResponse();
				StreamReader streamReader = null;
				try
				{
					streamReader = new StreamReader(response.GetResponseStream());
					this.PostID = streamReader.ReadToEnd();
				}
				finally
				{
					if (streamReader != null)
					{
						streamReader.Close();
					}
				}
			}
			catch (WebException ex)
			{
				StreamReader streamReader2 = null;
				try
				{
					streamReader2 = new StreamReader(ex.Response.GetResponseStream());
					this.ErrorMessage = streamReader2.ReadToEnd();
				}
				finally
				{
					if (streamReader2 != null)
					{
						streamReader2.Close();
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
