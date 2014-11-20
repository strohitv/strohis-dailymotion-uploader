using System;
using System.IO;
using System.Xml.Linq;

namespace StrohisUploadLib.Dailymotion
{
	internal static class SettingsProvider
	{
		public static string Key
		{
			get
			{
				return GetXmlElementValue("key");
			}
		}

		public static string Secret
		{
			get
			{
				return GetXmlElementValue("secret");
			}
		}

		//public static string Username
		//{
		//	get
		//	{
		//		return GetXmlElementValue("username");
		//	}
		//}

		//public static string Password
		//{
		//	get
		//	{
		//		return GetXmlElementValue("password");
		//	}
		//}

		//public static string CallbackUrl
		//{
		//	get
		//	{
		//		return GetXmlElementValue("callbackurl");
		//	}
		//}

		private static string GetXmlElementValue(string elementName)
		{
			//if (!File.Exists("secrets.xml"))
			//{
			//	throw new Exception("Rename secrets.xml.sample to secrets.xml and enter your secrets in the file.");
			//}
			string xmlString = @"<root>
  <key>d7b976fb4ac33d3b8d7a</key>
  <secret>e4d8327ee265b6f2589a8632e82aaa4540d694de</secret>
  <callbackurl></callbackurl>
</root>";

			return XDocument.Parse(xmlString).Element("root").Element(elementName).Value;
		}
	}
}
