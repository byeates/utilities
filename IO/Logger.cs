using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Common.IO
{
	/// <summary>
	/// A filtered log that classes can all use. Use the <see cref="FilterLogsFor"/> to add types to the
	/// filtered log list. This will allow the console output to only display logs from those types
	/// </summary>
	public class Logger : IDestructible
	{
		/// <summary>
		/// Filters logs coming in from only these types
		/// </summary>
		private static List<Type> filteredLogs = new List<Type>();
		
		/// <summary>
		/// Destroy called from destructibles, clears any filters etc.
		/// </summary>
		public static void Destroy()
		{
			RemoveAllFilters();
		}
		
		/*================================================================================
		FILTERING		
		=================================================================================*/
		/// <summary>
		/// Add T to filtered log list, only logs coming through from that type will be output
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public static void FilterLogsFor<T>()
		{
			filteredLogs.Add(typeof(T));
		}
		
		/// <summary>
		/// Remove any filters for T
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public static void RemoveFilter<T>()
		{
			filteredLogs.Remove(typeof(T));
		}

		/// <summary>
		/// Removes all the filters
		/// </summary>
		public static void RemoveAllFilters()
		{
			filteredLogs?.Clear();
		}

		/*================================================================================
		LOGGING		
		=================================================================================*/
		/// <summary>
		/// Normal debug log, T must be a class
		/// </summary>
		/// <param name="message"></param>
		/// <typeparam name="T"></typeparam>
		public static void Log<T>(string message)
		{
			if (IsFiltered<T>()) { return; }
			
			Debug.Log(FormPrefix<T>() + message);
		}
		
		/// <summary>
		/// Warning debug log, T must be a class
		/// </summary>
		/// <param name="message"></param>
		/// <typeparam name="T"></typeparam>
		public static void Warning<T>(string message)
		{
			if (IsFiltered<T>()) { return; }
			
			Debug.LogWarning(FormPrefix<T>() + message);
		}
		
		/// <summary>
		/// Error debug log, T must be a class
		/// </summary>
		/// <param name="message"></param>
		/// <typeparam name="T"></typeparam>
		public static void Error<T>(string message)
		{
			if (IsFiltered<T>()) { return; }
			
			Debug.LogError(FormPrefix<T>() + message);
		}

		/// <summary>
		/// Log exception
		/// </summary>
		/// <param name="message"></param>
		/// <typeparam name="T"></typeparam>
		public static void Exception<T>(string message)
		{
			if (IsFiltered<T>()) { return; }
			
			Debug.LogException(new Exception(FormPrefix<T>() + message));
		}

		/*================================================================================
		ANCILLARY		
		=================================================================================*/
		private static bool IsFiltered<T>() => filteredLogs != null && filteredLogs.Contains(typeof(T));
		
		private static string FormPrefix<T>()
		{
			return $"{typeof(T).FullName} => ";
		}
	}
}