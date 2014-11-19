using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace StrohisUploadLib.Dailymotion
{
	internal static class XmlManager
	{
		internal static void WriteDecryptedAccountXml(ObservableCollection<Account> accounts, string password, string path = "accounts.xml")
		{
			XmlSerializer serializer = new XmlSerializer(typeof(XmlAccounts));
			var accountsToSerialize = new XmlAccounts() { Account = accounts };

			StringWriter writer = new StringWriter();
			serializer.Serialize(writer, accountsToSerialize);
			var SerializedString = writer.ToString();

			if (!string.IsNullOrWhiteSpace(password))
			{
				string encryptedXml = EncryptXml(SerializedString, password);
				if (!string.IsNullOrWhiteSpace(encryptedXml) && !string.IsNullOrWhiteSpace(path))
				{
					StreamWriter encryptedWriter = new StreamWriter(path);
					encryptedWriter.Write(encryptedXml);
					encryptedWriter.Flush();
					encryptedWriter.Close();
				}
			}
		}

		internal static ObservableCollection<Account> ReadEncryptedAccountXml(string password, string path = "accounts.xml")
		{
			if (!string.IsNullOrWhiteSpace(password) && !string.IsNullOrWhiteSpace(path) && File.Exists(path))
			{
				StreamReader reader = new StreamReader(path);
				string encryptedXml = reader.ReadToEnd();

				string decryptedXml = DecryptXml(encryptedXml, password);
				if (!string.IsNullOrWhiteSpace(decryptedXml))
				{
					StringReader stringReader = new StringReader(decryptedXml);

					try
					{
						XmlSerializer serializer = new XmlSerializer(typeof(XmlAccounts));

						XmlAccounts accounts = (XmlAccounts)serializer.Deserialize(stringReader);
						return accounts.Account;
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.Message);
					}
				}
			}
			return null;
		}

		private static string EncryptXml(string xml, string password)
		{
			RijndaelManaged key = null;
			string returnString = null;

			try
			{
				// Create a new Rijndael key.
				key = new RijndaelManaged();

				int position = 0;
				while (password.Length % 32 != 0)
				{
					password += password[position];
					position++;
				}

				byte[] passwordbytes = System.Text.Encoding.UTF8.GetBytes(password);
				key.Key = passwordbytes;
				// Load an XML document.
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.PreserveWhitespace = true;
				xmlDoc.LoadXml(xml);

				// Encrypt the "creditcard" element.
				Encrypt(xmlDoc, "XmlAccounts", key);
				returnString = xmlDoc.InnerXml;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
			finally
			{
				// Clear the key.
				if (key != null)
				{
					key.Clear();
				}
			}
			return returnString;
		}

		private static string DecryptXml(string xml, string password)
		{
			RijndaelManaged key = null;
			string returnString = null;

			try
			{
				// Create a new Rijndael key.
				key = new RijndaelManaged();

				int position = 0;
				while (password.Length % 32 != 0)
				{
					password += password[position];
					position++;
				}

				byte[] passwordbytes = System.Text.Encoding.UTF8.GetBytes(password);
				key.Key = passwordbytes;
				// Load an XML document.
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.PreserveWhitespace = true;
				xmlDoc.LoadXml(xml);

				Decrypt(xmlDoc, key);
				returnString = xmlDoc.InnerXml;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
			finally
			{
				// Clear the key.
				if (key != null)
				{
					key.Clear();
				}
			}
			return returnString;
		}

		public static void Encrypt(XmlDocument Doc, string ElementName, SymmetricAlgorithm Key)
		{
			// Check the arguments.  
			if (Doc == null)
				throw new ArgumentNullException("Doc");
			if (ElementName == null)
				throw new ArgumentNullException("ElementToEncrypt");
			if (Key == null)
				throw new ArgumentNullException("Alg");

			////////////////////////////////////////////////
			// Find the specified element in the XmlDocument
			// object and create a new XmlElemnt object.
			////////////////////////////////////////////////
			XmlElement elementToEncrypt = Doc.GetElementsByTagName(ElementName)[0] as XmlElement;
			// Throw an XmlException if the element was not found.
			if (elementToEncrypt == null)
			{
				throw new XmlException("The specified element was not found");

			}

			//////////////////////////////////////////////////
			// Create a new instance of the EncryptedXml class 
			// and use it to encrypt the XmlElement with the 
			// symmetric key.
			//////////////////////////////////////////////////

			EncryptedXml eXml = new EncryptedXml();

			byte[] encryptedElement = eXml.EncryptData(elementToEncrypt, Key, false);
			////////////////////////////////////////////////
			// Construct an EncryptedData object and populate
			// it with the desired encryption information.
			////////////////////////////////////////////////

			EncryptedData edElement = new EncryptedData();
			edElement.Type = EncryptedXml.XmlEncElementUrl;

			// Create an EncryptionMethod element so that the 
			// receiver knows which algorithm to use for decryption.
			// Determine what kind of algorithm is being used and
			// supply the appropriate URL to the EncryptionMethod element.

			string encryptionMethod = null;

			if (Key is TripleDES)
			{
				encryptionMethod = EncryptedXml.XmlEncTripleDESUrl;
			}
			else if (Key is DES)
			{
				encryptionMethod = EncryptedXml.XmlEncDESUrl;
			}
			if (Key is Rijndael)
			{
				switch (Key.KeySize)
				{
					case 128:
						encryptionMethod = EncryptedXml.XmlEncAES128Url;
						break;
					case 192:
						encryptionMethod = EncryptedXml.XmlEncAES192Url;
						break;
					case 256:
						encryptionMethod = EncryptedXml.XmlEncAES256Url;
						break;
				}
			}
			else
			{
				// Throw an exception if the transform is not in the previous categories
				throw new CryptographicException("The specified algorithm is not supported for XML Encryption.");
			}

			edElement.EncryptionMethod = new EncryptionMethod(encryptionMethod);

			// Add the encrypted element data to the 
			// EncryptedData object.
			edElement.CipherData.CipherValue = encryptedElement;

			////////////////////////////////////////////////////
			// Replace the element from the original XmlDocument
			// object with the EncryptedData element.
			////////////////////////////////////////////////////
			EncryptedXml.ReplaceElement(elementToEncrypt, edElement, false);
		}

		public static void Decrypt(XmlDocument Doc, SymmetricAlgorithm Alg)
		{
			// Check the arguments.  
			if (Doc == null)
				throw new ArgumentNullException("Doc");
			if (Alg == null)
				throw new ArgumentNullException("Alg");

			// Find the EncryptedData element in the XmlDocument.
			XmlElement encryptedElement = Doc.GetElementsByTagName("EncryptedData")[0] as XmlElement;

			// If the EncryptedData element was not found, throw an exception.
			if (encryptedElement == null)
			{
				throw new XmlException("The EncryptedData element was not found.");
			}


			// Create an EncryptedData object and populate it.
			EncryptedData edElement = new EncryptedData();
			edElement.LoadXml(encryptedElement);

			// Create a new EncryptedXml object.
			EncryptedXml exml = new EncryptedXml();


			// Decrypt the element using the symmetric key.
			byte[] rgbOutput = exml.DecryptData(edElement, Alg);

			// Replace the encryptedData element with the plaintext XML element.
			exml.ReplaceData(encryptedElement, rgbOutput);

		}
	}

	[XmlRoot]
	public class XmlAccounts
	{
		[XmlElement, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
		public ObservableCollection<Account> Account { get; set; }
	}
}
