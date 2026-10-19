     /////// 
  ////     ////.    =================================*
 ///          ///   Скрипты сделаны AntonXCM для проекта
//     /===,    //  The Sailor & his Garden и исключительно
/*    (         "/  предоставлены Kitket427 для использования
//     \==="    //  в любых проектах в качестве подарка.
 "//          ///   
  ////      ///*    Могу дать еще, мне не жалко ;)
     *//////"

using UnityEngine;

public partial class Localization {
	public static string GetText((string language, string fullKey) tableKey) {
		if (LocalizedTexts.TryGetValue(tableKey, out string result))
			return result;
		
		Debug.LogError($"ERROR {tableKey.fullKey} {nameof(GetText)}");
		return $"ERROR {tableKey.fullKey}";
	}
	
	public static string GetText(string language, string fileName, string key) => GetText((language, $"{fileName}_{key}"));

	public static string[] GetTextArray(string language, string fileName) {
		if(LocalizedTextArrays.TryGetValue((language, fileName), out var result)) return result;
		Debug.LogError($"ERROR {fileName} {nameof(GetTextArray)}");
		return new string[1] {$"ERROR {fileName}"};
	}

	public static string GetText(string fileName, string key) => GetText(Language, fileName, key);
	public static string GetText(string fullKey) => GetText((Language, fullKey));
	public static string[] GetTextArray(string fileName) => GetTextArray(Language, fileName);
	public static string GetTextFormatted(string fileName, string key, params object[] formatArgs) 
		=> GetTextFormatted((Language, $"{fileName}_{key}"), formatArgs);
	public static string GetTextFormatted(string language, string fileName, string key, params object[] formatArgs)
		=> GetTextFormatted((language, $"{fileName}_{key}"), formatArgs: formatArgs);
	public static string GetTextFormatted(string fullKey, params object[] formatArgs) 
		=> GetTextFormatted((Language, fullKey), formatArgs: formatArgs);

	public static string GetTextFormatted((string language, string fullKey) tableKey, params object[] formatArgs) {
		if (LocalizedTexts.TryGetValue(tableKey, out string result))
			return string.Format(result, args: formatArgs);
		
		Debug.LogError($"ERROR {tableKey.fullKey} {nameof(GetText)}");
		return $"ERROR {tableKey.fullKey}";
	}
}