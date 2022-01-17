using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Pixel_Snapping_bone))]
[CanEditMultipleObjects]
public class Pixel_Snapping_GUI :Editor
{
    public override void OnInspectorGUI() {
        var snap = target as Pixel_Snapping_bone;
        snap.bones = (Pixel_Snapping_bone.bone)EditorGUILayout.EnumFlagsField("Snap_bone",snap.bones);
     }

    
}
