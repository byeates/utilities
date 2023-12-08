using System;
using System.IO;
using Code.Common.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Code.Common.Storage
{
	/// <summary>
	/// Instantiate an instance of persistent data to use, parse, update, and save a json file
	/// </summary>
	public abstract class PersistentData<T>
	{
		/// <summary>
		/// Path of the loaded file, set when <see cref="LoadFile"/> is called
		/// </summary>
		public string filePath { get; protected set; }
		
		/// <summary>
		/// Loaded data converted to type T
		/// </summary>
		public T data { get; protected set; }
		
		/// <summary>
		/// String representation of the unserialized data
		/// </summary>
		public string rawData { get; protected set; }

		/// <summary>
		/// Subclasses to handle loaded files
		/// </summary>
		public abstract void HandledLoadedFile();

		/// <summary>
		/// Default case/method called for when no file is found, and no backup is detected
		/// </summary>
		public abstract void HandleFileNotFound();

		/// <summary>
		/// Key to use to additionally save to player prefs
		/// </summary>
		public string prefBackupKey;

		//====================
		// PRIVATE
		//====================
		/// <summary>
		/// When set to true will enable encryption
		/// </summary>
		private bool enableEncryption = false;

		/// <summary>
		/// Encryption salt
		/// </summary>
		private string encryptionSalt;
		
		/// <summary>
		/// Encryption IV
		/// </summary>
		private string encryptionIV;

		/// <summary>
		/// <see cref="Encryptor"/>
		/// </summary>
		private Encryptor encryptor;

		/// <summary>
		/// Enable or disable using encryption when saving the data
		/// </summary>
		/// <param name="enable"></param>
		/// <param name="salt"></param>
		/// <param name="iv"></param>
		public void SetEncryption(bool enable, string salt, string iv)
		{
			enableEncryption = enable;
			encryptionSalt = salt;
			encryptionIV = iv;

			if (enableEncryption)
			{
				encryptor = new Encryptor();
				encryptor.Init(salt, iv);
			}
		}

		/// <summary>
		/// Loads the json file at the given file name/path (references into persistent data)
		/// </summary>
		/// <param name="path">Partial path from persistent data, does not require leading '/'</param>
		/// <param name="prefKeyBackup">pref key to load data from if the file load fails</param>
		/// <param name="isEncrypted">set to true if encrpytion was used. make sure <see cref="SetEncryption"/> was called
		/// before attempting to use it</param>
		/// <returns>true if the file was loaded</returns>
		public virtual bool LoadFile(string path, string prefKeyBackup = null, bool isEncrypted = false)
		{
			filePath = path;

			try
			{
				string file = Application.persistentDataPath + "/" + path;
				
				if (File.Exists(file))
				{
					rawData = FileIO.ReadFile(file);
				}
				if (PrefsWrapper.HasKey(prefKeyBackup))
				{
					rawData = PrefsWrapper.GetString(prefKeyBackup);
				}

				if (!string.IsNullOrEmpty(rawData))
				{
					if (isEncrypted)
					{
						rawData = encryptor.Decrypt(rawData);
					}
					
					HandledLoadedFile();
					return true;
				}
				HandleFileNotFound();
			}
			catch (Exception e)
			{
				Debug.LogError($"Unable to load file {filePath} from persistent data = {e.Message}");
			}
			
			return false;
		}

		/// <summary>
		/// Saves the json data back to the file
		/// </summary>
		/// <param name="path"></param>
		/// <returns>true if the file was saved</returns>
		public virtual bool SaveFile()
		{
			if (string.IsNullOrEmpty(filePath))
			{
				Debug.LogError("Must load file before saving");
			}

			try
			{
				string output = data.ToString();

				if (enableEncryption && !string.IsNullOrEmpty(encryptionSalt) && !string.IsNullOrEmpty(encryptionIV))
				{
					output = encryptor.Encrypt(output);
				}
				
				if (!string.IsNullOrEmpty(prefBackupKey))
				{
					PrefsWrapper.SetString(prefBackupKey, output);
				}
				FileIO.WriteAllString(Application.persistentDataPath + "/" + filePath, output);
				return true;
			}
			catch (Exception e)
			{
				Debug.LogError($"Unable to write file {filePath} to persistent data");
			}

			return false;
		}

		/// <summary>
		/// Attempts to delete file
		/// </summary>
		public virtual void DeleteFile()
		{
			if (string.IsNullOrEmpty(filePath))
			{
				Debug.LogError("Unable to delete, no file path set");
			}
			
			try
			{
				if (!string.IsNullOrEmpty(prefBackupKey))
				{
					PrefsWrapper.Delete(prefBackupKey);
				}
				FileIO.DeleteFile(Application.persistentDataPath + "/" + filePath);
			}
			catch (Exception e)
			{
				Debug.LogError($"Unable to write file {filePath} to persistent data");
			}
		}
	}
}