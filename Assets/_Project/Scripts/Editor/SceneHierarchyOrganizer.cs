using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Editor tool to organize the scene hierarchy into logical groups.
/// Run via menu: Tools > Organize Scene Hierarchy
/// </summary>
public class SceneHierarchyOrganizer : EditorWindow
{
    [MenuItem("Tools/Organize Scene Hierarchy")]
    public static void OrganizeHierarchy()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        Undo.SetCurrentGroupName("Organize Scene Hierarchy");
        int undoGroup = Undo.GetCurrentGroup();

        // Define hierarchy structure
        var sections = new Dictionary<string, List<string>>
        {
            ["--- MANAGEMENT ---"] = new List<string>
            {
                "XROptimizationManager",
                "EventSystem",
                "GameManager",
                "AudioManager"
            },
            ["--- ENVIRONMENT ---"] = new List<string>
            {
                "Directional Light",
                "Global Volume",
                "Plane",
                "Floor",
                "Terrain",
                "Lighting"
            },
            ["--- XR ---"] = new List<string>
            {
                "XR Origin",
                "XR Interaction Manager",
                "XR Origin (XR Rig)"
            },
            ["--- DEBUG ---"] = new List<string>
            {
                "[Graphy]",
                "DebugCanvas"
            }
        };

        // Create section containers and organize objects
        foreach (var section in sections)
        {
            GameObject container = CreateSectionContainer(section.Key);

            foreach (string objectName in section.Value)
            {
                MoveObjectsToContainer(objectName, container.transform);
            }

            // Remove empty containers
            if (container.transform.childCount == 0)
            {
                Undo.DestroyObjectImmediate(container);
            }
        }

        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log("Scene hierarchy organized successfully!");
    }

    private static GameObject CreateSectionContainer(string name)
    {
        // Check if container already exists
        GameObject existing = GameObject.Find(name);
        if (existing != null)
        {
            return existing;
        }

        GameObject container = new GameObject(name);
        container.tag = "EditorOnly";
        container.transform.SetAsLastSibling();
        Undo.RegisterCreatedObjectUndo(container, "Create Section Container");

        return container;
    }

    private static void MoveObjectsToContainer(string searchName, Transform container)
    {
        // Find all root objects matching the name (partial match)
        GameObject[] allObjects = UnityEngine.SceneManagement.SceneManager
            .GetActiveScene()
            .GetRootGameObjects();

        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains(searchName) && obj.transform.parent == null)
            {
                // Skip section containers
                if (obj.name.StartsWith("---"))
                {
                    continue;
                }

                Undo.SetTransformParent(obj.transform, container, "Move to Section");
            }
        }
    }

    [MenuItem("Tools/Flatten Scene Hierarchy")]
    public static void FlattenHierarchy()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        Undo.SetCurrentGroupName("Flatten Scene Hierarchy");
        int undoGroup = Undo.GetCurrentGroup();

        // Find all section containers and unparent their children
        GameObject[] allObjects = UnityEngine.SceneManagement.SceneManager
            .GetActiveScene()
            .GetRootGameObjects();

        foreach (GameObject obj in allObjects)
        {
            if (obj.name.StartsWith("---") && obj.CompareTag("EditorOnly"))
            {
                // Move children to root
                while (obj.transform.childCount > 0)
                {
                    Transform child = obj.transform.GetChild(0);
                    Undo.SetTransformParent(child, null, "Unparent Object");
                }

                // Delete the container
                Undo.DestroyObjectImmediate(obj);
            }
        }

        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log("Scene hierarchy flattened successfully!");
    }
}
