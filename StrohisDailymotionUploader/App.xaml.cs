using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml;

namespace StrohisUploader
{
	/// <summary>
	/// Interaktionslogik für "App.xaml"
	/// </summary>
	public partial class App : Application
	{
		private void DispatcherUnhandledExceptionEventHandler(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			// Process unhandled exception

			//Console.WriteLine(e);
			//MessageBox.Show("Scheiße ey, alles ging schief!");


			int i = 0;
			while (File.Exists(string.Format("Fehler_{0}.fail", i)))
			{
				i++;
			}

			XmlWriter writer = XmlWriter.Create(string.Format("Fehler_{0}.fail", i));

			new ExceptionXElement(e.Exception, true).WriteTo(writer);
			writer.Flush();
			writer.Close();
			Console.WriteLine();

			MessageBox.Show(string.Format("Es gab einen unerwarteten Fehler! Die Fehlermeldung wurde in der Datei 'Fehler_{0}.fail' gespeichert, die sich im selben Ordner wie die .exe des Uploaders befindet. Bitte leite sie an @Strohi weiter, damit er den unerwarteten Fehler in Zukunft abfangen / verhindern kann.{1}{1}Die Fehlermeldung lautet wie folgt:{1}{2}", i, Environment.NewLine, e.Exception.Message), "Unerwarteter Fehler!", MessageBoxButton.OK, MessageBoxImage.Error);

			e.Handled = true;
		}
	}
}
