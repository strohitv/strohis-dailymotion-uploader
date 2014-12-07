using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.HtmlControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace StrohisUploader.Dialogs
{
	/// <summary>
	/// Interaktionslogik für Browser.xaml
	/// </summary>
	public partial class Browser : Window, INotifyPropertyChanged
	{
		public Browser()
		{
			InitializeComponent();
			HideScriptErrors(WbBrowser, true);
		}

		public void HideScriptErrors(WebBrowser wb, bool hide)
		{
			var fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
			if (fiComWebBrowser == null) return;
			var objComWebBrowser = fiComWebBrowser.GetValue(wb);
			if (objComWebBrowser == null)
			{
				wb.Loaded += (o, s) => HideScriptErrors(wb, hide); //In case we are to early
				return;
			}
			objComWebBrowser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { hide });
		}

		private string urlToNavigate;
		public string UrlToNavigate
		{
			get { return urlToNavigate; }
			set
			{
				urlToNavigate = value;
				if (!string.IsNullOrWhiteSpace(urlToNavigate))
				{
					WbBrowser.Navigate(urlToNavigate);
				}
			}
		}

		private string user;
		public string User
		{
			get { return user; }
			set
			{
				user = value;
			}
		}

		#region NotifyProperty

		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(name));
			}
		}

		#endregion NotifyProperty

		private void WbBrowserNavigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
		{
			if (e.Uri != null && e.Uri.AbsoluteUri.ToUpper().StartsWith("http://Strohi.tv/".ToUpper()))
			{
				this.Close();
			}
		}
	}
}
