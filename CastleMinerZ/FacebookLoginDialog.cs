using System;
using System.ComponentModel;
using System.Drawing;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Facebook;
using Microsoft.CSharp.RuntimeBinder;

namespace DNA.CastleMinerZ
{
	public partial class FacebookLoginDialog : Form
	{
		public FacebookOAuthResult FacebookOAuthResult { get; private set; }

		public FacebookLoginDialog(string appId, string extendedPermissions)
			: this(new FacebookClient(), appId, extendedPermissions)
		{
		}

		public FacebookLoginDialog(FacebookClient fb, string appId, string extendedPermissions)
		{
			if (fb == null)
			{
				throw new ArgumentNullException("fb");
			}
			if (string.IsNullOrWhiteSpace(appId))
			{
				throw new ArgumentNullException("appId");
			}
			this._fb = fb;
			this._loginUrl = this.GenerateLoginUrl(appId, extendedPermissions);
			this.InitializeComponent();
		}

		private Uri GenerateLoginUrl(string appId, string extendedPermissions)
		{
			object obj = new ExpandoObject();
			if (FacebookLoginDialog.<GenerateLoginUrl>o__SiteContainer0.<>p__Site1 == null)
			{
				FacebookLoginDialog.<GenerateLoginUrl>o__SiteContainer0.<>p__Site1 = CallSite<Func<CallSite, object, string, object>>.Create(Binder.SetMember(CSharpBinderFlags.None, "client_id", typeof(FacebookLoginDialog), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null)
				}));
			}
			FacebookLoginDialog.<GenerateLoginUrl>o__SiteContainer0.<>p__Site1.Target(FacebookLoginDialog.<GenerateLoginUrl>o__SiteContainer0.<>p__Site1, obj, appId);
			if (FacebookLoginDialog.<GenerateLoginUrl>o__SiteContainer0.<>p__Site2 == null)
			{
				FacebookLoginDialog.<GenerateLoginUrl>o__SiteContainer0.<>p__Site2 = CallSite<Func<CallSite, object, string, object>>.Create(Binder.SetMember(CSharpBinderFlags.None, "redirect_uri", typeof(FacebookLoginDialog), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
				}));
			}
			FacebookLoginDialog.<GenerateLoginUrl>o__SiteContainer0.<>p__Site2.Target(FacebookLoginDialog.<GenerateLoginUrl>o__SiteContainer0.<>p__Site2, obj, "https://www.facebook.com/connect/login_success.html");
			if (FacebookLoginDialog.<GenerateLoginUrl>o__SiteContainer0.<>p__Site3 == null)
			{
				FacebookLoginDialog.<GenerateLoginUrl>o__SiteContainer0.<>p__Site3 = CallSite<Func<CallSite, object, string, object>>.Create(Binder.SetMember(CSharpBinderFlags.None, "response_type", typeof(FacebookLoginDialog), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
				}));
			}
			FacebookLoginDialog.<GenerateLoginUrl>o__SiteContainer0.<>p__Site3.Target(FacebookLoginDialog.<GenerateLoginUrl>o__SiteContainer0.<>p__Site3, obj, "token");
			if (FacebookLoginDialog.<GenerateLoginUrl>o__SiteContainer0.<>p__Site4 == null)
			{
				FacebookLoginDialog.<GenerateLoginUrl>o__SiteContainer0.<>p__Site4 = CallSite<Func<CallSite, object, string, object>>.Create(Binder.SetMember(CSharpBinderFlags.None, "display", typeof(FacebookLoginDialog), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.Constant, null)
				}));
			}
			FacebookLoginDialog.<GenerateLoginUrl>o__SiteContainer0.<>p__Site4.Target(FacebookLoginDialog.<GenerateLoginUrl>o__SiteContainer0.<>p__Site4, obj, "popup");
			if (!string.IsNullOrWhiteSpace(extendedPermissions))
			{
				if (FacebookLoginDialog.<GenerateLoginUrl>o__SiteContainer0.<>p__Site5 == null)
				{
					FacebookLoginDialog.<GenerateLoginUrl>o__SiteContainer0.<>p__Site5 = CallSite<Func<CallSite, object, string, object>>.Create(Binder.SetMember(CSharpBinderFlags.None, "scope", typeof(FacebookLoginDialog), new CSharpArgumentInfo[]
					{
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
						CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null)
					}));
				}
				FacebookLoginDialog.<GenerateLoginUrl>o__SiteContainer0.<>p__Site5.Target(FacebookLoginDialog.<GenerateLoginUrl>o__SiteContainer0.<>p__Site5, obj, extendedPermissions);
			}
			if (FacebookLoginDialog.<GenerateLoginUrl>o__SiteContainer0.<>p__Site6 == null)
			{
				FacebookLoginDialog.<GenerateLoginUrl>o__SiteContainer0.<>p__Site6 = CallSite<Func<CallSite, object, Uri>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(Uri), typeof(FacebookLoginDialog)));
			}
			Func<CallSite, object, Uri> target = FacebookLoginDialog.<GenerateLoginUrl>o__SiteContainer0.<>p__Site6.Target;
			CallSite <>p__Site = FacebookLoginDialog.<GenerateLoginUrl>o__SiteContainer0.<>p__Site6;
			if (FacebookLoginDialog.<GenerateLoginUrl>o__SiteContainer0.<>p__Site7 == null)
			{
				FacebookLoginDialog.<GenerateLoginUrl>o__SiteContainer0.<>p__Site7 = CallSite<Func<CallSite, FacebookClient, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "GetLoginUrl", null, typeof(FacebookLoginDialog), new CSharpArgumentInfo[]
				{
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
					CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
				}));
			}
			return target(<>p__Site, FacebookLoginDialog.<GenerateLoginUrl>o__SiteContainer0.<>p__Site7.Target(FacebookLoginDialog.<GenerateLoginUrl>o__SiteContainer0.<>p__Site7, this._fb, obj));
		}

		private void FacebookLoginDialog_Load(object sender, EventArgs e)
		{
			this.webBrowser.Navigate(this._loginUrl.AbsoluteUri);
		}

		private void webBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
		{
			FacebookOAuthResult facebookOAuthResult;
			if (this._fb.TryParseOAuthCallbackUrl(e.Url, out facebookOAuthResult))
			{
				this.FacebookOAuthResult = facebookOAuthResult;
				base.DialogResult = (this.FacebookOAuthResult.IsSuccess ? DialogResult.OK : DialogResult.No);
				return;
			}
			this.FacebookOAuthResult = null;
		}

		private readonly Uri _loginUrl;

		protected readonly FacebookClient _fb;

		[CompilerGenerated]
		private static class <GenerateLoginUrl>o__SiteContainer0
		{
			public static CallSite<Func<CallSite, object, string, object>> <>p__Site1;

			public static CallSite<Func<CallSite, object, string, object>> <>p__Site2;

			public static CallSite<Func<CallSite, object, string, object>> <>p__Site3;

			public static CallSite<Func<CallSite, object, string, object>> <>p__Site4;

			public static CallSite<Func<CallSite, object, string, object>> <>p__Site5;

			public static CallSite<Func<CallSite, object, Uri>> <>p__Site6;

			public static CallSite<Func<CallSite, FacebookClient, object, object>> <>p__Site7;
		}
	}
}
