using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
