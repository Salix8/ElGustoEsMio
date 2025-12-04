#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(IngredienteCondicional))]
public class IngredienteCondicionalEditor : Editor
{
    public override void OnInspectorGUI()
    {
        IngredienteCondicional script = (IngredienteCondicional)target;

        // Buscar MinigameProgressManager
        MinigameProgressManager manager = FindObjectOfType<MinigameProgressManager>();

        EditorGUILayout.LabelField("Condiciones de Visibilidad", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (manager != null && manager.nombresMinijuegos != null && manager.nombresMinijuegos.Count > 0)
        {
            // Crear dropdown con los minijuegos disponibles
            List<string> opciones = new List<string> { "(Selecciona un minijuego)" };
            opciones.AddRange(manager.nombresMinijuegos);

            int indiceActual = 0;
            if (!string.IsNullOrEmpty(script.minijuegoRequerido))
            {
                indiceActual = opciones.IndexOf(script.minijuegoRequerido);
                if (indiceActual == -1) indiceActual = 0;
            }

            EditorGUI.BeginChangeCheck();
            int nuevoIndice = EditorGUILayout.Popup("Minijuego Requerido", indiceActual, opciones.ToArray());
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(script, "Cambiar Minijuego Requerido");
                if (nuevoIndice > 0)
                {
                    script.minijuegoRequerido = opciones[nuevoIndice];
                }
                else
                {
                    script.minijuegoRequerido = "";
                }
                EditorUtility.SetDirty(script);
            }

            // Mostrar el nombre actual como texto también
            if (!string.IsNullOrEmpty(script.minijuegoRequerido))
            {
                EditorGUILayout.HelpBox($"Minijuego seleccionado: {script.minijuegoRequerido}", MessageType.Info);
            }
        }
        else
        {
            // Fallback: campo de texto manual
            EditorGUILayout.HelpBox("MinigameProgressManager no encontrado o sin minijuegos configurados.", MessageType.Warning);
            script.minijuegoRequerido = EditorGUILayout.TextField("Minijuego Requerido (manual)", script.minijuegoRequerido);
        }

        EditorGUILayout.Space();

        // Resto de campos
        script.mostrarAntesDeCompletar = EditorGUILayout.Toggle(
            new GUIContent("Mostrar Antes De Completar", "Si está marcado, estos ingredientes se muestran ANTES de completar el minijuego"),
            script.mostrarAntesDeCompletar
        );

        script.mostrarDespuesDeCompletar = EditorGUILayout.Toggle(
            new GUIContent("Mostrar Después De Completar", "Si está marcado, estos ingredientes se muestran DESPUÉS de completar el minijuego"),
            script.mostrarDespuesDeCompletar
        );

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Referencias", EditorStyles.boldLabel);
        
        SerializedProperty ingredientesProp = serializedObject.FindProperty("ingredientes");
        EditorGUILayout.PropertyField(ingredientesProp, new GUIContent("Ingredientes", "Lista de GameObjects que serán mostrados/ocultados según las condiciones"), true);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Ayuda", EditorStyles.boldLabel);
        
        SerializedProperty ayudaProp = serializedObject.FindProperty("minijuegosDisponibles");
        EditorGUILayout.PropertyField(ayudaProp, new GUIContent("Minijuegos Disponibles"), true);

        // Botones de utilidad
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Utilidades", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Actualizar Lista"))
        {
            script.SendMessage("ActualizarListaMinijuegos", SendMessageOptions.DontRequireReceiver);
        }
        if (GUILayout.Button("Mostrar Todos"))
        {
            script.SendMessage("MostrarTodos");
        }
        if (GUILayout.Button("Ocultar Todos"))
        {
            script.SendMessage("OcultarTodos");
        }
        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(script);
        }
    }
}
#endif
