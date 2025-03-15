using Unity.Netcode.Components;
using UnityEngine;

#if UNITY_EDITOR

using Unity.Netcode.Editor;
using UnityEditor;

/// <summary>
/// custom editor for the PlayerCubeController component
/// </summary>
[CustomEditor(typeof(PlayerCubeController), true)]
public class PlayerCubeControllerEditor : NetworkTransformEditor
{
    private SerializedProperty m_Speed;
    private SerializedProperty m_ApplyVerticalInputToZAxis;

    public override void OnEnable()
    {
        // Cache the serialized properties
        m_Speed = serializedObject.FindProperty(nameof(PlayerCubeController.Speed));
        m_ApplyVerticalInputToZAxis = serializedObject.FindProperty(nameof(PlayerCubeController.ApplyVerticalInputToZAxis));

        base.OnEnable();
    }

    private void DisplayPlayerCubeControllerProperties()
    {
        // Display the properties for the PlayerCubeController component
        EditorGUILayout.PropertyField(m_Speed);
        EditorGUILayout.PropertyField(m_ApplyVerticalInputToZAxis);
    }

    public override void OnInspectorGUI()
    {
        // Cast the target to a PlayerCubeController
        var playerCubeController = target as PlayerCubeController;

        // Define the method to set the expanded state
        void SetExpanded(bool expanded)
        {
            playerCubeController.PlayerCubeControllerPropertiesVisible = expanded;
        }

        // Draw the foldout group for the PlayerCubeController component
        DrawFoldOutGroup<PlayerCubeController>(playerCubeController.GetType(), DisplayPlayerCubeControllerProperties, playerCubeController.PlayerCubeControllerPropertiesVisible, SetExpanded);

        base.OnInspectorGUI();
    }
}

#endif

/// <summary>
/// This class is a simple example of a player controller that moves a cube around the scene.
/// </summary>
public class PlayerCubeController : NetworkTransform
{
#if UNITY_EDITOR

    // These bool properties ensure that any expanded or collapsed property views within the inspector view
    // will be saved and restored the next time the asset/prefab is viewed.
    public bool PlayerCubeControllerPropertiesVisible;

#endif

    public float Speed = 10;
    public bool ApplyVerticalInputToZAxis;
    private Vector3 m_Motion;

    private void Update()
    {
        // If not spawned or we don't have authority, then don't update
        if (!IsSpawned || !HasAuthority)
        {
            return;
        }

        // Handle acquiring and applying player input
        m_Motion = Vector3.zero;
        m_Motion.x = Input.GetAxis("Horizontal");

        // Determine whether the vertical input is applied to the Y or Z axis
        if (!ApplyVerticalInputToZAxis)
        {
            m_Motion.y = Input.GetAxis("Vertical");
        }
        else
        {
            m_Motion.z = Input.GetAxis("Vertical");
        }

        // If there is any player input magnitude, then apply that amount of
        // motion to the transform
        if (m_Motion.sqrMagnitude > 0 * 0)
        {
            transform.position += Speed * Time.deltaTime * m_Motion;
        }
    }
}