using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(PlayerInput)), RequireComponent(typeof(Collider))]
public class PlayerController : MonoBehaviour {
    // Input System Setup:
    // Add this script to the player object with a PlayerInput component attached.
    // Create the following actions in the PlayerInput component:
    // - Move (Vector2)
    // - Look (Vector2)
    // - Jump (Button)
    // - Interact (Button)
    // - InteractSecondary (Button)
    // - Crouch (Button) - With "Press" Interaction AND Trigger Interaction of "Press And Release"
    // - Sprint (Button) - With "Press" Interaction AND Trigger Interaction of "Press And Release"
    // - Walk (Button) - With "Press" Interaction AND Trigger Interaction of "Press And Release"

    /// <summary>
    /// Defines the different move states of the player.
    /// <para>
    /// Note: All move states only indicate locomotive movement. Looking around does not affect the move state.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <list type="table">
    /// <listheader>
    /// <term>Move State</term>
    /// <description>Description</description>
    /// </listheader>
    /// <item>
    /// <term>Sprinting</term>
    /// <description>Player is sprinting</description>
    /// </item>
    /// <item>
    /// <term>Running</term>
    /// <description>Player is running</description>
    /// </item>
    /// <item>
    /// <term>Walking</term>
    /// <description>Player is walking</description>
    /// </item>
    /// <item>
    /// <term>Crouching</term>
    /// <description>Player is crouching</description>
    /// </item>
    /// <item>
    /// <term>Crawling</term>
    /// <description>Player is crawling</description>
    /// </item>
    /// <item>
    /// <term>Idle</term>
    /// <description>Player is not moving</description>
    /// </item>
    /// </list>
    /// </remarks>
    /// <seealso cref="INPUT_MODE"/>
    public enum MOVE_STATE {
        SPRINTING,
        RUNNING,
        WALKING,
        CROUCHING,
        CRAWLING,
        IDLE
    }

    /// <summary>
    /// Defines the different input modes for actions.
    /// </summary>
    /// <remarks>
    /// <list type="table">
    /// <listheader>
    /// <term>Input Mode</term>
    /// <description>Description</description>
    /// </listheader>
    /// <item>
    /// <term>Toggle</term>
    /// <description>Press once to toggle on, press again to toggle off</description>
    /// </item>
    /// <item>
    /// <term>Hold</term>
    /// <description>Press and hold to keep on, release to turn off</description>
    /// </item>
    /// </list>
    /// </remarks>
    /// <seealso cref="MOVE_STATE"/>
    public enum INPUT_MODE {
        TOGGLE,
        HOLD
    }

    private Rigidbody _rb;

    [Header("Player Settings")] public float moveSpeed = 5f; // Base speed for player movement.
    [Range(0, 1)] public float groundFrictionMultiplier = 1f / 3f;
    public float jumpForce = 5f; // Force applied when player jumps.
    public bool jumpUpIsRelative = true; // Indicates if the player should jump relative to the player's rotation.

    public float maxAirSpeed = 25f; // Maximum speed for player movement while in the air.
    [Range(0, 1)] public float airStrafeForce = 0.5f; // Amount of control the player has in the air (0 = no control, 1 = full control)
    public bool airStrafeStopOnOppositeDirection = true; // Indicates if the player should stop when moving in the opposite direction of their current motion.

    [Header("States & Events")] public MOVE_STATE moveState = MOVE_STATE.RUNNING; // Default movement state.

    // Interaction delegates.
    public Func<bool> PreInteract; // Checks if primary interaction is allowed.
    public Action PostInteract; // Primary interaction method.
    public Func<bool> PreInteractSecondary; // Checks if secondary interaction is allowed.
    public Action PostInteractSecondary; // Secondary interaction method.

    [Header("Input Modes")]
    // Input modes for actions.
    public INPUT_MODE crouchMode = INPUT_MODE.HOLD;
    public INPUT_MODE sprintMode = INPUT_MODE.HOLD;
    public INPUT_MODE walkMode = INPUT_MODE.HOLD;

    [Header("Camera Settings")] public Transform cameraTransform; // The main camera's transform.

    [Header("Debug")] public bool _debug; // Indicates if debug mode is enabled.

    private Vector2 _moveInput;

    private bool _isJumping; // Indicates if the player is currently attempting to jump.
    
    private Collider GetGround => Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1.1f) ? hit.collider : null; // Gets the ground object the player is touching.
    private bool IsGrounded => GetGround != null; // Indicates if the player is touching the ground.

    /// <summary>
    /// Indicates if the cursor is locked.
    /// <para>
    /// Note: This is a wrapper for Cursor.lockState.
    /// </para>
    /// </summary>
    public static bool LockCursor {
        get => Cursor.lockState == CursorLockMode.Locked;
        set => Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
    }

    public GameObject tempPrefab; // TODO: Remove

    private void Start() {
        _rb = GetComponent<Rigidbody>();

        LockCursor = true;

        PostInteract += () => {
            _rb.linearVelocity = cameraTransform.forward * 10f;
        
            GameObject temp = Instantiate(tempPrefab, transform.position + Vector3.down + cameraTransform.forward, cameraTransform.rotation);
            Rigidbody tempRb = temp.GetComponent<Rigidbody>();
            tempRb.mass = _rb.mass;
            tempRb.linearVelocity = _rb.linearVelocity;
            Destroy(temp, 15f);
        };
    }

    private void FixedUpdate() {
        HandleMovement();
        HandleJump();
    }

    /// <summary>
    /// Applies a jump force to the player and resets the jump flag.
    /// </summary>
    private void HandleJump() {
        if (!_isJumping) return;

        _rb.AddForce((jumpUpIsRelative ? transform.up : Vector3.up) * (_rb.mass * jumpForce), ForceMode.Impulse);
        _isJumping = false;
    }

    /// <summary>
    /// Returns the speed multiplier for the current move state.
    /// </summary>
    /// <returns>Speed multiplier for the current move state.</returns>
    /// <remarks>
    /// The speed multiplier is used to adjust the player's movement speed based on the current move state.
    /// </remarks>
    /// <seealso cref="MOVE_STATE"/>
    private float GetSpeedMultiplier() {
        return moveState switch {
            MOVE_STATE.SPRINTING => 2f,
            MOVE_STATE.WALKING => 0.5f,
            MOVE_STATE.CROUCHING => 0.33f,
            MOVE_STATE.CRAWLING => 0.2f,
            _ => 1f
        };
    }

    private void HandleMovement() {
        Vector3 moveDirection = cameraTransform.forward * _moveInput.y + cameraTransform.right * _moveInput.x;
        moveDirection.y = 0;
        float speedMultiplier = GetSpeedMultiplier();

        Collider ground = GetGround;
        PhysicsMaterial groundMaterial = ground?.material;
        float frictionCoefficient = (groundMaterial?.dynamicFriction ?? 1f) * groundFrictionMultiplier;

        Vector3 targetVelocity = moveDirection.normalized * (moveSpeed * speedMultiplier);
        Vector3 currentHorizontalVelocity = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);

        if (ground != null) {
            Vector3 force = (targetVelocity - currentHorizontalVelocity) * frictionCoefficient;
            _rb.AddForce(force, ForceMode.VelocityChange);
        }
        else if (airStrafeForce > 0) {
            ApplyAirStrafing(targetVelocity);
        }
    }

    /// <summary>
    /// Applies air strafing mechanics to the Rigidbody based on the given direction vector.
    /// </summary>
    /// <param name="vector3">The target direction vector for air strafing.</param>
    /// <seealso cref="airStrafeForce"/>
    /// <seealso cref="airStrafeStopOnOppositeDirection"/>
    /// <seealso cref="maxAirSpeed"/>
    /// <seealso cref="HandleMovement"/>
    void ApplyAirStrafing(Vector3 vector3) {
        // Project the Rigidbody's velocity onto the target direction vector.
        Vector3 projVel = Vector3.Project(_rb.linearVelocity, vector3);

        // Determine if the target direction is opposite to the projected velocity.
        bool isAway = Vector3.Dot(vector3, projVel) <= 0f;

        // Apply force if moving away from velocity or if velocity is below the maximum air speed.
        if (projVel.magnitude < maxAirSpeed || isAway) {
            // Calculate the ideal movement force, normalized and scaled by the air strafing force.
            Vector3 strafeForce = vector3.normalized * airStrafeForce;

            // Cap the force to prevent exceeding maxAirSpeed.
            strafeForce = Vector3.ClampMagnitude(strafeForce, isAway ? maxAirSpeed + projVel.magnitude : maxAirSpeed - projVel.magnitude);

            // Apply the calculated force to the Rigidbody.
            _rb.AddForce(strafeForce, ForceMode.VelocityChange);
        }

        // Additional control to stop air strafing when moving in the opposite direction.
        if (airStrafeStopOnOppositeDirection) {
            // Extract the horizontal components of the Rigidbody's velocity and the target direction.
            Vector3 currentHorizontalVelocity = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
            Vector3 strafeDirection = new Vector3(vector3.x, 0, vector3.z);

            // Check if the player is attempting to move in the opposite direction.
            if (Vector3.Dot(currentHorizontalVelocity.normalized, strafeDirection.normalized) < -0.1f) {
                // Quickly reduce forward momentum if moving in the opposite direction.
                Vector3 perpendicularComponent = currentHorizontalVelocity - Vector3.Project(currentHorizontalVelocity, strafeDirection);
                _rb.linearVelocity = new Vector3(perpendicularComponent.x, _rb.linearVelocity.y, perpendicularComponent.z);
            }
        }
    }

    private void OnMove(InputValue value) => _moveInput = value.Get<Vector2>();
    private void OnJump(InputValue value) => _isJumping = value.isPressed && IsGrounded;

    private void OnInteract(InputValue value) => HandleInteraction(value, PreInteract, PostInteract);
    private void OnInteractSecondary(InputValue value) => HandleInteraction(value, PreInteractSecondary, PostInteractSecondary);

    private void OnCrouch(InputValue value) => HandleModeAction(value, crouchMode, MOVE_STATE.CROUCHING);
    private void OnSprint(InputValue value) => HandleModeAction(value, sprintMode, MOVE_STATE.SPRINTING);
    private void OnWalk(InputValue value) => HandleModeAction(value, walkMode, MOVE_STATE.WALKING);

    /// <summary>
    /// Handles an interaction action.
    /// </summary>
    /// <param name="value">The input value.</param>
    /// <param name="preCheck">If the preCheck returns false, the interaction will not be performed.</param>
    /// <param name="action">The action to perform.</param>
    ///
    private void HandleInteraction(InputValue value, Func<bool> preCheck, Action action) {
        if (!value.isPressed) return;
        if (preCheck == null || preCheck.GetInvocationList().All(func => ((Func<bool>)func)()))
            action?.Invoke();
    }

    /// <summary>
    /// Handles a mode action.
    /// </summary>
    /// <param name="value">The input value.</param>
    /// <param name="mode">The input mode.</param>
    /// <param name="desiredState">The desired move state.</param>
    /// <seealso cref="INPUT_MODE"/>
    /// <seealso cref="MOVE_STATE"/>
    /// <remarks>
    /// This method handles the different <see cref="INPUT_MODE"/> for actions.
    /// </remarks>
    private void HandleModeAction(InputValue value, INPUT_MODE mode, MOVE_STATE desiredState) {
        moveState = mode switch {
            INPUT_MODE.TOGGLE when value.isPressed => moveState == desiredState ? MOVE_STATE.RUNNING : desiredState,
            INPUT_MODE.HOLD => value.isPressed ? desiredState : MOVE_STATE.RUNNING,
            _ => moveState
        };
    }

    /// <summary>
    /// Draws debug information to the screen.
    /// </summary>
    private void OnGUI() {
        if (!_debug) return;
        
        float scale = Screen.width / 1200f;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));

        int yPos = 10;
        GUI.Label(new Rect(10, yPos, 500, 20), $"Move State: {moveState}");
        GUI.Label(new Rect(10, yPos += 20, 500, 20), $"Is Grounded: {IsGrounded}");
        GUI.Label(new Rect(10, yPos += 20, 500, 20), $"Lock Cursor: {LockCursor}");
        GUI.Label(new Rect(10, yPos += 20, 500, 20), $"Move Input: {_moveInput}");
        GUI.Label(new Rect(10, yPos += 20, 500, 20), $"World Velocity: {_rb.linearVelocity}");
        GUI.Label(new Rect(10, yPos += 20, 500, 20), $"Local Velocity: {transform.InverseTransformDirection(_rb.linearVelocity)}");
        GUI.Label(new Rect(10, yPos += 20, 500, 20), $"Speed: {(_rb.linearVelocity.magnitude > 0.25f ? _rb.linearVelocity.magnitude : 0f):0.0} m/s");

        Vector3 localVelocity = transform.InverseTransformDirection(_rb.linearVelocity);
        float verticalSpeed = localVelocity.z;
        float horizontalSpeed = Mathf.Sqrt(Mathf.Pow(localVelocity.x, 2) + Mathf.Pow(localVelocity.y, 2));

        GUI.Label(new Rect(10, yPos += 20, 500, 20), $"Vertical Speed: {(verticalSpeed > 0.25f ? verticalSpeed : 0f):0.0} m/s");
        GUI.Label(new Rect(10, yPos += 20, 500, 20), $"Horizontal Speed: {(horizontalSpeed > 0.25f ? horizontalSpeed : 0f):0.0} m/s");
    }
}