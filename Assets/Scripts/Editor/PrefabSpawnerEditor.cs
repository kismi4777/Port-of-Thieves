using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PrefabSpawner))]
public class PrefabSpawnerEditor : Editor
{
    private PrefabSpawner spawner;
    
    void OnEnable()
    {
        spawner = (PrefabSpawner)target;
    }
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Управление спавн поинтами", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Создать спавн поинт", GUILayout.Height(30)))
        {
            spawner.CreateSpawnPoint();
        }
        
        if (GUILayout.Button("Удалить все", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Подтверждение", 
                "Удалить все спавн поинты?", "Да", "Отмена"))
            {
                spawner.ClearAllSpawnPoints();
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Количество спавн поинтов: " + spawner.GetSpawnPointsCount());
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Тестирование", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Спавн префаба"))
        {
            spawner.SpawnPrefab();
        }
        
        if (GUILayout.Button("Очистить объекты"))
        {
            spawner.ClearAllSpawnedObjects();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Заспавнено объектов: " + spawner.GetSpawnedObjectsCount());
        
        if (GUI.changed)
        {
            EditorUtility.SetDirty(spawner);
        }
    }
    
    void OnSceneGUI()
    {
        if (spawner.spawnPoints != null)
        {
            for (int i = 0; i < spawner.spawnPoints.Length; i++)
            {
                if (spawner.spawnPoints[i] != null)
                {
                    Transform spawnPoint = spawner.spawnPoints[i];
                    Vector3 position = spawnPoint.position;
                    
                    // Рисуем ручки для перемещения
                    Handles.color = Color.green;
                    Vector3 newPosition = Handles.PositionHandle(position, Quaternion.identity);
                    
                    if (newPosition != position)
                    {
                        Undo.RecordObject(spawnPoint, "Move Spawn Point");
                        spawnPoint.position = newPosition;
                    }
                    
                    // Рисуем номер спавн поинта
                    Handles.Label(position + Vector3.up * 0.8f, 
                                 "SP " + i.ToString(), 
                                 EditorStyles.boldLabel);
                }
            }
        }
    }
}
