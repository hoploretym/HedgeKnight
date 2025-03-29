using UnityEditor;
using UnityEngine;

public class FindMissingScripts : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/🧹 Найти Missing Scripts в сцене")]
    static void FindAllMissingScripts()
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        int missingCount = 0;

        foreach (GameObject go in allObjects)
        {
            Component[] components = go.GetComponents<Component>();

            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    Debug.LogWarning($"Missing script in: {go.name}", go);
                    missingCount++;
                }
            }
        }

        Debug.Log($"🔍 Готово. Найдено {missingCount} объектов с отсутствующими скриптами.");
    }
#endif
}
