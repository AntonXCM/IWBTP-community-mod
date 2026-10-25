#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class SetSpriteMaskInteraction
{
    private const string MenuPath = "Assets/Tools/Sprites/Set VisibleOutsideMask";

    [MenuItem(MenuPath, false, 2000)]
    private static void Apply()
    {
        int prefabsChanged = 0;
        int spritesChanged = 0;

        foreach (var obj in Selection.GetFiltered<GameObject>(SelectionMode.Assets))
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path) || !path.EndsWith(".prefab"))
                continue;

            // Открываем содержимое префаба для редактирования
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                int changed = 0;
                // true = включая неактивные объекты
                foreach (var sr in root.GetComponentsInChildren<SpriteRenderer>(true))
                {
                    if (sr.maskInteraction == SpriteMaskInteraction.VisibleOutsideMask)
                        continue;

                    sr.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
                    changed++;
                }

                if (changed > 0)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    prefabsChanged++;
                    spritesChanged += changed;
                    Debug.Log($"[SetSpriteMaskInteraction] {path}: изменено спрайтов — {changed}", obj);
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[SetSpriteMaskInteraction] Готово. Префабов изменено: {prefabsChanged}, спрайтов изменено: {spritesChanged}");
    }

    [MenuItem(MenuPath, true)]
    private static bool Validate()
    {
        foreach (var obj in Selection.GetFiltered<GameObject>(SelectionMode.Assets))
        {
            Debug.Log(AssetDatabase.GetAssetPath(obj));
            if (AssetDatabase.GetAssetPath(obj).EndsWith(".prefab"))
                return true;
        }
        return false;
    }
}
#endif