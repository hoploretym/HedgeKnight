using UnityEngine;
using System.IO;
using System.Text;
using UnityEngine.SceneManagement;

public class SceneHierarchyExporter : MonoBehaviour
{
    void Start()
    {
        ExportHierarchy();
    }

    void ExportHierarchy()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== Scene Hierarchy ===");

        foreach (GameObject obj in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            ExportObject(obj.transform, 0, sb);
        }

        string path = Application.dataPath + "/SceneHierarchy.txt";
        File.WriteAllText(path, sb.ToString());
        Debug.Log("Иерархия сцены сохранена в: " + path);
    }

    void ExportObject(Transform obj, int level, StringBuilder sb)
    {
        sb.AppendLine(new string('-', level * 2) + obj.name + " [" + obj.gameObject.activeSelf + "]");

        foreach (Component comp in obj.GetComponents<Component>())
        {
            sb.AppendLine(new string(' ', (level + 1) * 2) + "- " + comp.GetType().Name);
        }

        foreach (Transform child in obj)
        {
            ExportObject(child, level + 1, sb);
        }
    }
}
