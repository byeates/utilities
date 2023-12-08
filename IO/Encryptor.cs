using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Code.Common.IO
{
	/// <summary>
	/// Handles basic salt/iv encryption/decryption
	/// </summary>
	public class Encryptor
	{
		private string salt;
		private string iv;
		
		/// <summary>
		/// Basic decrypting of requests
		/// </summary>
		private ICryptoTransform encryptor, decryptor;
		
		/// <summary>
		/// Utf8 encoder
		/// </summary>
		private UTF8Encoding encoder;
		
		/// <summary>
		/// Set salt/iv values
		/// </summary>
		/// <param name="s">salt</param>
		/// <param name="i">iv</param>
		public void Init(string s, string i)
		{
			salt = s;
			iv = i;
			
			RijndaelManaged rm = new RijndaelManaged();
			byte[] key = Encoding.UTF8.GetBytes(salt);
			byte[] vector = Encoding.UTF8.GetBytes(iv);
			encryptor = rm.CreateEncryptor(key, vector);
			decryptor = rm.CreateDecryptor(key, vector);
			encoder = new UTF8Encoding();
		}
		
		/// <summary>
		/// Encrypt a string
		/// </summary>
		/// <param name="unencrypted"></param>
		/// <returns></returns>
		public string Encrypt(string unencrypted)
		{
			return Convert.ToBase64String(Encrypt(encoder.GetBytes(unencrypted)));
		}

		/// <summary>
		/// Decrypt string
		/// </summary>
		/// <param name="encrypted"></param>
		/// <returns></returns>
		public string Decrypt(string encrypted)
		{
			return encoder.GetString(Decrypt(Convert.FromBase64String(encrypted)));
		}
		
		/// <summary>
		/// Internal encrypt to <see cref="Transform"/>
		/// </summary>
		/// <param name="buffer"></param>
		/// <returns></returns>
		private byte[] Encrypt(byte[] buffer)
		{
			return Transform(buffer, encryptor);
		}

		/// <summary>
		/// Internal decrypt to <see cref="Transform"/>
		/// </summary>
		/// <param name="buffer"></param>
		/// <returns></returns>
		private byte[] Decrypt(byte[] buffer)
		{
			return Transform(buffer, decryptor);
		}

		protected byte[] Transform(byte[] buffer, ICryptoTransform transform)
		{
			MemoryStream stream = new MemoryStream();
			using (CryptoStream cs = new CryptoStream(stream, transform, CryptoStreamMode.Write))
			{
				cs.Write(buffer, 0, buffer.Length);
			}
			return stream.ToArray();
		}
	}
}