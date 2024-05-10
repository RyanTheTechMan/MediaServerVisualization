using System;
using UnityEngine;
using Unity.Cinemachine;

public class CameraController : MonoBehaviour {
    /// <summary>
    /// Defines the different camera modes.
    /// </summary>
    /// <remarks>
    /// All camera modes are relative to the player and do not affect the player's movement. <br/>
    /// Note that when turning using the look input, the player will always turn the same amount, regardless of the camera mode.
    /// <list type="table">
    /// <listheader>
    /// <term>Camera Mode</term>
    /// <description>Description</description>
    /// </listheader>
    /// <item>
    /// <term>First Person</term>
    /// <description>Camera is positioned slightly behind the player's head</description>
    /// </item>
    /// <item>
    /// <term>Third Person</term>
    /// <description>Camera is positioned behind the player</description>
    /// </item>
    /// <item>
    /// <term>Orbit</term>
    /// <description>Camera is positioned behind the player and is able to be rotated around the player</description>
    /// </item>
    /// </list>
    /// </remarks>
    public enum CAMERA_MODE { FIRST_PERSON, THIRD_PERSON, ORBIT }

    /// <summary>
    /// Defines the different camera rotation modes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Follow Target: Camera follows the target's rotation<br/>
    /// If in <see cref="CAMERA_MODE.FIRST_PERSON"/> or <see cref="CAMERA_MODE.THIRD_PERSON"/> this will act as normal.<br/>
    /// If in <see cref="CAMERA_MODE.FIXED"/> this will cause the camera to look at the player.
    /// </para>
    /// <para>
    /// Follow Mouse: Camera follows the mouse's position<br/>
    /// When in <see cref="CAMERA_MODE.FIRST_PERSON"/> the player will no longer be able to look fully around, however they can look 'slightly' around.<br/>
    /// When in <see cref="CAMERA_MODE.THIRD_PERSON"/> the player will be able to look fully around, the player rotation will be controlled by the player's movement input. The camera will no longer be behind the player.<br/>
    /// When in <see cref="CAMERA_MODE.FIXED"/> does nothing. TODO: Should this do nothing or should it do something?
    /// </para>
    /// </remarks>
    /// <seealso cref="CAMERA_MODE"/>
    public enum CAMERA_ROTATION_MODE { FOLLOW_TARGET, FOLLOW_MOUSE }

    [Header("General Settings")]
    // public GameObject viewModel;
    public GameObject cameraTarget; // Normally an empty game object that is a child of the player.
    public CAMERA_MODE cameraMode = CAMERA_MODE.THIRD_PERSON;
    public CAMERA_ROTATION_MODE rotationMode = CAMERA_ROTATION_MODE.FOLLOW_TARGET;

    [Header("Follow Mouse Settings")]
    public float raycastDistance = 100f;

    public new Camera camera;
    public CinemachineCamera vcam1stPerson;
    public CinemachineCamera vcam3rdPerson;
    public CinemachineCamera vcamOrbit;

    private CinemachineCamera selectedCamera;

    private void Start() {
    }

    private void Update() {
        SwitchCameraMode();
    }

    private void SwitchCameraMode() {
        switch (cameraMode) {
            case CAMERA_MODE.FIRST_PERSON:
                selectedCamera = vcam1stPerson;
                vcam1stPerson.Priority = 1;
                vcam3rdPerson.Priority = 0;
                vcamOrbit.Priority = 0;
                break;
            case CAMERA_MODE.THIRD_PERSON:
                selectedCamera = vcam3rdPerson;
                vcam1stPerson.Priority = 0;
                vcam3rdPerson.Priority = 1;
                vcamOrbit.Priority = 0;
                break;
            case CAMERA_MODE.ORBIT:
                selectedCamera = vcamOrbit;
                vcam1stPerson.Priority = 0;
                vcam3rdPerson.Priority = 0;
                vcamOrbit.Priority = 1;
                break;
            default:
                Debug.LogWarning("Camera mode not found");
                break;
        }

        switch (rotationMode) {
            case CAMERA_ROTATION_MODE.FOLLOW_TARGET:
                selectedCamera.Follow = cameraTarget.transform;
                selectedCamera.LookAt = cameraTarget.transform;
                break;
            case CAMERA_ROTATION_MODE.FOLLOW_MOUSE:
                Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, raycastDistance);
                selectedCamera.Follow = hit.transform;
                selectedCamera.LookAt = hit.transform;
                break;
            default:
                Debug.LogWarning("Camera rotation mode not found");
                break;
        }
    }
}
