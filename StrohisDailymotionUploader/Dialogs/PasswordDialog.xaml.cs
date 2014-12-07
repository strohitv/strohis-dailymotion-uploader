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
	/// Interaktionslogik für PasswordDlg.xaml
	/// </summary>
	public partial class PasswordDialog : Window, INotifyPropertyChanged
	{
		public PasswordDialog()
		{
			InitializeComponent();

			pwbxAccountXmlPassword.Focus();
		}

		public bool EnteredPassword { get; set; }
		public string Password { get; set; }

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

		private void btnOkClick(object sender, RoutedEventArgs e)
		{
			if (!string.IsNullOrEmpty(pwbxAccountXmlPassword.Password))
			{
				this.Password = pwbxAccountXmlPassword.Password;
				this.EnteredPassword = true;
				this.Close();
			}
			else
			{
				var result = MessageBox.Show("Du hast kein Passwort eingegeben. Wenn du jetzt bestätigst, dann werden die Accounts nicht geladen. Fortfahren?", "Fehler: Kein Passwort angegeben", MessageBoxButton.YesNo, MessageBoxImage.Warning);
				if (result == MessageBoxResult.Yes)
				{
					this.EnteredPassword = false;
					this.Close();
				}
			}
		}

		private void btnCancelClick(object sender, RoutedEventArgs e)
		{
			this.EnteredPassword = false;
			this.Close();
		}
	}
}
