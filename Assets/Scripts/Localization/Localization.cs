     /////// 
  ////     ////.    =================================*
 ///          ///   Скрипты сделаны AntonXCM для проекта
//     /===,    //  The Sailor & his Garden и исключительно
/*    (         "/  предоставлены Kitket427 для использования
//     \==="    //  в любых проектах в качестве подарка.
 "//          ///   
  ////      ///*    Могу дать еще, мне не жалко ;)
     *//////"

using System;
using System.Collections.Generic;
using UnityEngine;

public static partial class Localization {
	public static string Language = "en";

	public static Dictionary<(string, string), string> LocalizedTexts = new();
	public static Dictionary<(string, string), string[]> LocalizedTextArrays = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
	public static void Init() {
        Debug.Log("INIT LOCALIZATION");
        LocalizedTexts.Clear();
        LocalizedTextArrays.Clear();
		if(PlayerPrefs.HasKey("Language")) {
			SetLanguage(PlayerPrefs.GetInt("Language"));	
		}
        LoadFromStreamingAssets();
	}

	public static void SetLanguage(int id) => SetLanguage(id switch {
			1 => "ru",
			_ => "en",
		});

	public static void SetLanguage(string id) {
		Language = id; 
		OnChangeLanguage?.Invoke();
		PlayerPrefs.SetInt("Language", id switch { "ru" => 1, _ => 0 });
	}
	public static event Action OnChangeLanguage;
}

[Serializable]
public class LocalizedString {
	public string Key, File;

	public LocalizedString(string file, string key) {
		Key = key;
		File = file;
	}

	public override string ToString() => Localization.GetText(File, Key);
}