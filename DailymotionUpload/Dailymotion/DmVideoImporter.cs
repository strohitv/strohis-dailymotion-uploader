using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrohisUploadLib.Dailymotion
{
	public static class DmVideoImporter
	{
		public static ObservableCollection<UploadElement> ImportCSV(string path)
		{
			ObservableCollection<UploadElement> uploadElementsFromCsv = new ObservableCollection<UploadElement>();

			StreamReader csvReader = new StreamReader(path);
			string[] lines = csvReader.ReadToEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

			foreach (string singleLine in lines)
			{
				string[] singleFields = singleLine.Split(new char[] { ';' });

				UploadElement elementToAdd = new UploadElement()
				{
					Path = singleFields[0],
					Title = singleFields[1],
					IsOfficial = Convert.ToBoolean((int)Enum.Parse(typeof(YesNoValue), singleFields[2])),
					Tags = singleFields[3],
					Description = singleFields[4],
					VideoLanguage = (Language)(Array.IndexOf(DmUploadConstants.Language, singleFields[5])),
					VideoChannel = (Channel)(Array.IndexOf(DmUploadConstants.Channels, singleFields[6])),
					RecordDate = singleFields[7],
					Location = singleFields[8],
					IsCommentingAllowed = Convert.ToBoolean((int)Enum.Parse(typeof(YesNoValue), singleFields[9])),
					IsPrivate = Convert.ToBoolean((int)Enum.Parse(typeof(YesNoValue), singleFields[10])),
					IsNewVideostar = Convert.ToBoolean((int)Enum.Parse(typeof(YesNoValue), singleFields[11])),
					GroupId = singleFields[12],
					PlayListId = singleFields[13],
				};

				uploadElementsFromCsv.Add(elementToAdd);
			}

			return uploadElementsFromCsv;
		}

		public static UploadElement ImportSingleVideo(string path)
		{
			UploadElement elementToAdd = new UploadElement()
			{
				Path = path,
				Title = new FileInfo(path).Name,
				IsOfficial = false,
				Tags = string.Empty,
				Description = string.Empty,
				VideoLanguage = (Language)(Array.IndexOf(DmUploadConstants.Language, "DE")),
				VideoChannel = (Channel)(Array.IndexOf(DmUploadConstants.Channels, "videogames")),
				RecordDate = string.Format("{0}-{1}-{2}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day),
				Location = string.Empty,
				IsCommentingAllowed = true,
				IsPrivate = false,
				IsNewVideostar = false,
				GroupId = string.Empty,
				PlayListId = string.Empty,
			};

			return elementToAdd;
		}
	}
}
