using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

public class TransferComponents : MonoBehaviour {

    [MenuItem("LifeSpark/TransferComponents")]
    public static void Transfer() {
        GameObject src = Selection.gameObjects[0];
        GameObject tar = Selection.gameObjects[1];

        foreach (var comp in src.GetComponents<Component>()) {
            Component new_component = tar.AddComponent(comp.GetType());
            foreach (FieldInfo f in comp.GetType().GetFields()) {
                f.SetValue(new_component, f.GetValue(comp));
            }
        }
    }

}
