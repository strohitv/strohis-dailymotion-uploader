using StrohisUploadLib.Dailymotion;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StrohisUploader.Controls
{
	/// <summary>
	/// Interaktionslogik für UploadListItem.xaml
	/// </summary>
	public partial class UploadListItem : UserControl, INotifyPropertyChanged
	{
		public UploadListItem()
		{
			InitializeComponent();
		}

		public UploadElement Element
		{
			get
			{
				return (UploadElement)this.GetValue(ElementProperty);
			}
			set
			{
				this.SetValue(ElementProperty, value);
				OnPropertyChanged("VideoTitle");
			}
		}

		public static DependencyProperty ElementProperty = DependencyProperty.Register("Element", typeof(UploadElement), typeof(UploadListItem), new PropertyMetadata(null));

		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(name));
			}
		}
	}
}
