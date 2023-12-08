using System;
using UnityEngine;
using System.Collections;

public class PrefsWrapper
{
	/*================================================================================
	ACCESSOR OVERRIDES	
	=================================================================================*/
	public static int GetInt(string key, int defaultValue = 0)
	{
		return PlayerPrefs.GetInt(key, defaultValue);
	}

	public static bool GetBool(string key, bool defaultValue = false)
	{
		int defaultInt = Convert.ToInt32(defaultValue);
		return PlayerPrefs.GetInt(key, defaultInt) != 0;
	}

	public static float GetFloat(string key, float defaultValue = 0f)
	{
		return PlayerPrefs.GetFloat(key, defaultValue);
	}

	public static string GetString(string key, string defaultValue = "")
	{
		return PlayerPrefs.GetString(key, defaultValue);
	}

	/*================================================================================
	MUTATOR OVERRIDES
	=================================================================================*/
	public static void SetInt(string key, int newValue)
	{
		PlayerPrefs.SetInt(key,newValue);
	}

	public static void SetBool(string key, bool newValue)
	{
		PlayerPrefs.SetInt(key, Convert.ToInt32(newValue));
	}

	public static void SetFloat(string key, float newValue)
	{
		PlayerPrefs.SetFloat(key, newValue);
	}

	public static void SetString(string key, string newValue)
	{
		PlayerPrefs.SetString(key, newValue);
	}
	
	/*================================================================================
	SAVING	
	=================================================================================*/
	public static void Save()
	{
		PlayerPrefs.Save();
	}
	
	/*================================================================================
	LOOKUP	
	=================================================================================*/
	public static bool HasKey(string key)
	{
		return PlayerPrefs.HasKey(key);
	}
	
	/*================================================================================
	CLEARING	
	=================================================================================*/
	public static void Delete(string key)
	{
		PlayerPrefs.DeleteKey(key);
	}

	public static void DeleteAll()
	{
		PlayerPrefs.DeleteAll();
	}
}