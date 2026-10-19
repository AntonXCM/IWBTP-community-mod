     /////// 
  ////     ////.    =================================
 ///          ///   Скрипты сделаны AntonXCM для проекта
//     /===,    //  The Sailor & his Garden и исключительно
/*    (         "/  предоставлены Kitket427 для использования
//     \==="    //  в любых проектах в качестве подарка.
 "//          ///   
  ////      ///*    Могу дать еще, мне не жалко ;)
     *//////"

using System;
using System.IO;
using UnityEngine;

public static partial class Localization {
	public static void LoadFromStreamingAssets() {
		string root = Path.Combine(Application.streamingAssetsPath, "Language");
		if (!Directory.Exists(root))
			throw new DirectoryNotFoundException(root);

		string[] folders = Directory.GetDirectories(root);
		if (folders.Length == 0)
			throw new DirectoryNotFoundException($"No languages found in {root}");

		foreach (string folder in folders)
			ParseLanguage(folder);
	}

	public static void ParseLanguage(string textFolderPath) {
		string languageName = Path.GetFileName(textFolderPath);
		foreach (string filePath in Directory.GetFiles(textFolderPath)) {
            Debug.Log(filePath);
			string extension = Path.GetExtension(filePath);

			if (extension.Equals(".csv", StringComparison.OrdinalIgnoreCase)) {
				string csvName = Path.GetFileNameWithoutExtension(filePath);

				foreach (string line in File.ReadAllLines(filePath)) {
                    Debug.Log(line);
					if (string.IsNullOrWhiteSpace(line))
						continue;

					if (line.TrimStart().StartsWith("#", StringComparison.Ordinal))
						continue;
                    Debug.Log(line);

					int separatorIndex = line.IndexOf(';');
					if (separatorIndex < 0)
						continue;

					string key = line[..separatorIndex].Trim();
                    Debug.Log(key);
					if (key.Length == 0)
						continue;

					LocalizedTexts[(languageName, $"{csvName}_{key}")] =
						ProcessRawText(line[(separatorIndex + 1)..]);
				}
			}
			else if (extension.Equals(".txt", StringComparison.OrdinalIgnoreCase)) {
				string txtName = Path.GetFileNameWithoutExtension(filePath);
				LocalizedTextArrays[(languageName, txtName)] = File.ReadAllLines(filePath);
			}
		}
	}

	public static string ProcessRawText(string asset) =>
		asset.Replace("\\n", "\n").Replace(",.", ";");
}