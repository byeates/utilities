using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using JetBrains.Annotations;
using UnityEngine;

namespace Code.Common.IO
{
	/// <summary>
	/// Simple static raw file reader/writer
	/// </summary>
	public class FileIO
	{
		/// <summary>
		/// Returns string contents of file at path
		/// </summary>
		/// <param name="pathToFile"></param>
		/// <returns></returns>
		public static string ReadFile(string pathToFile)
		{
			StreamReader reader = new StreamReader(pathToFile);
			string output = reader.ReadToEnd();
			reader.Close();

			return output;
		}
		
		/// <summary>
		/// Writes a string to the file
		/// </summary>
		/// <param name="pathToFile"></param>
		/// <param name="input"></param>
		/// <param name="newLine">whether to use a new line</param>
		/// <param name="returnOutput">set to true to return new raw output</param>
		[CanBeNull]
		public static string WriteString(string pathToFile, string input, bool newLine = false, bool returnOutput = false)
		{
			StreamWriter writer = new StreamWriter(pathToFile, true);

			if (newLine)
			{
				writer.WriteLine(input);
			}
			else
			{
				writer.Write(input);
			}
			
			writer.Close();

			if (returnOutput)
			{
				StreamReader reader = new StreamReader(pathToFile);
				string output = reader.ReadToEnd();
				reader.Close();
				return output;
			}

			return null;
		}
		
		/// <summary>
		/// Writes a string to the file, overwrites the entire file if used
		/// </summary>
		/// <param name="pathToFile"></param>
		/// <param name="input"></param>
		/// <param name="newLine">whether to use a new line</param>
		/// <param name="returnOutput">set to true to return new raw output</param>
		[CanBeNull]
		public static string WriteAllString(string pathToFile, string input, bool newLine = false, bool returnOutput = false)
		{
			StreamWriter writer = new StreamWriter(pathToFile);

			if (newLine)
			{
				writer.WriteLine(input);
			}
			else
			{
				writer.Write(input);
			}
			
			writer.Close();

			if (returnOutput)
			{
				StreamReader reader = new StreamReader(pathToFile);
				string output = reader.ReadToEnd();
				reader.Close();
				return output;
			}

			return null;
		}

		/// <summary>
		/// Attempts to write the object serialized as binary to the designated file. Does not throw
		/// an error if an exception occurs.
		/// </summary>
		/// <param name="pathToFile"></param>
		/// <param name="input"></param>
		/// <param name="append"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static bool SafeWriteBinaryToFile<T>(string pathToFile, T input, bool append = false)
		{
			using Stream stream = File.Open(pathToFile, append ? FileMode.Append : FileMode.Create);
			
			try
			{
				var binaryFormatter = new BinaryFormatter();
				binaryFormatter.Serialize(stream, input);
				return true;
			}
			catch (Exception e)
			{
				return false;
			}
		}

		/// <summary>
		/// Reads serialized object from file
		/// </summary>
		/// <param name="pathToFile"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T ReadBinaryFromFile<T>(string pathToFile)
		{
			using Stream stream = File.Open(pathToFile, FileMode.Open);
			var binaryFormatter = new BinaryFormatter();
			return (T)binaryFormatter.Deserialize(stream);
		}

		/// <summary>
		/// Attempts delete at the file location
		/// </summary>
		/// <param name="pathToFile"></param>
		public static void DeleteFile(string pathToFile)
		{
			try
			{
				File.Delete(pathToFile);
			}
			catch (Exception e)
			{
				Logger.Error<FileIO>($"Unable to delete file: {e.Message}");
			}
		}
	}
}