using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    private InputSystem_Actions controls;
    private Rigidbody2D rb;

    [SerializeField] private bool useMouse = true;

    [Header("Movement")]
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float braking = 10f;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float reverseSpeed = 5f;
    [SerializeField] private float drag = 2f;

    [Header("Steering")]
    [SerializeField] private float turnSpeed = 180f;
    [SerializeField] private float steeringSmoothTime = 0.15f;

    private float currentSteering;
    private float steeringVelocity;

    [Header("Lance")]
    [SerializeField] private GameObject lance;
    [SerializeField] private float lanceMaxAngle = 45f;

    // How responsive the lance is while standing still
    [SerializeField] private float minLanceSmoothTime = 0.08f;

    // How heavy the lance feels at full gallop
    [SerializeField] private float maxLanceSmoothTime = 0.35f;

    [Header("Animation")]
    [SerializeField] private Animator headTurnAnimator;
    [SerializeField] private float headTurnSmoothTime = 0.2f;

    private float currentHeadTurn;
    private float headTurnVelocity;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector2 pointerPos;

    private float currentSpeed;
    private float lanceAngularVelocity;

    private bool lanceLocked = false;

    private float keyboardTargetAngle;

    private void Awake()
    {
        controls = new InputSystem_Actions();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        controls.Enable();

        controls.Player.Move.performed += OnMove;
        controls.Player.Move.canceled += OnMove;

        controls.Player.Look.performed += OnLook;
        controls.Player.Look.canceled += OnLook;

        controls.Player.PointerPos.performed += OnPointerPos;
        controls.Player.PointerPos.canceled += OnPointerPos;
    }

    private void OnDisable()
    {
        controls.Player.Move.performed -= OnMove;
        controls.Player.Move.canceled -= OnMove;

        controls.Player.Look.performed -= OnLook;
        controls.Player.Look.canceled -= OnLook;

        controls.Player.PointerPos.performed -= OnPointerPos;
        controls.Player.PointerPos.canceled -= OnPointerPos;

        controls.Disable();
    }

    private void Start()
    {
        controls.Player.SecondaryAction.performed += ctx => LanceLock(true);
        controls.Player.SecondaryAction.canceled += ctx => LanceLock(false);
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    private void OnLook(InputAction.CallbackContext ctx)
    {
        lookInput = ctx.ReadValue<Vector2>();
    }

    private void OnPointerPos(InputAction.CallbackContext ctx)
    { 
        pointerPos = ctx.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void LanceLock(bool lockState)
    { 
        lanceLocked = lockState;
    }

    private void HandleMovement()
    {
        float throttle = moveInput.y;

        if (currentSpeed > 0f)
            currentSpeed = rb.linearVelocity.magnitude;
        else if (currentSpeed < 0f)
            currentSpeed = -rb.linearVelocity.magnitude;

        if (throttle > 0f)
        {

            currentSpeed += acceleration * throttle * Time.fixedDeltaTime;
        }
        else if (throttle < 0f)
        {
            currentSpeed += braking * throttle * Time.fixedDeltaTime;
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                0f,
                drag * Time.fixedDeltaTime);
        }
        // Clamp speed (with reverse)
        currentSpeed = Mathf.Clamp(currentSpeed, -reverseSpeed, maxSpeed);

        // Steering (more effective at low speed, but 0 if stationary)
        float targetSteering = moveInput.x;

        // Steering has inertia
        currentSteering = Mathf.SmoothDamp(
             currentSteering,
            moveInput.x,
            ref steeringVelocity,
            steeringSmoothTime);

        // Turning becomes weaker at high speed
        float steeringStrength = Mathf.Lerp(1f, 0.3f, Mathf.Abs(currentSpeed) / maxSpeed);
        // Can't steer while standing still
        steeringStrength *= currentSpeed / maxSpeed;

        float rotation = currentSteering
         * turnSpeed
         * steeringStrength
         * Time.fixedDeltaTime;

        rb.MoveRotation(rb.rotation - rotation);

        // Move forward
        rb.linearVelocity = transform.up * currentSpeed;

        // Animate head turn
        float headTurn = Mathf.Lerp(currentSteering, moveInput.x, 0.3f);
        // Head looks slightly into the turn
        float targetHeadTurn = currentSteering;

        // Smooth the head separately
        currentHeadTurn = Mathf.SmoothDamp(
            currentHeadTurn,
            targetHeadTurn,
            ref headTurnVelocity,
            headTurnSmoothTime);

        // Convert [-1, 1] to [0, 1]
        headTurnAnimator.SetFloat(
            "TurnPercentage",
            (currentHeadTurn + 1f) * 0.5f);
    }

    private void Update()
    {
        HandleLance();
    }

    private void HandleLance()
    {
        float targetAngle;

        if (useMouse)
        {
            // Mouse aiming
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(pointerPos);
            mouseWorld.z = 0f;

            Vector2 localMouse = transform.InverseTransformPoint(mouseWorld);

            targetAngle = Vector2.SignedAngle(Vector2.up, localMouse);
            targetAngle = Mathf.Clamp(targetAngle, -lanceMaxAngle, lanceMaxAngle);
        }
        else
        {
            // Keyboard aiming (Left/Right arrows)
            if (Mathf.Abs(lookInput.x) > 0.01f)
            {
                keyboardTargetAngle += -lookInput.x * 90f * Time.deltaTime;
                keyboardTargetAngle = Mathf.Clamp(
                    keyboardTargetAngle,
                    -lanceMaxAngle,
                    lanceMaxAngle);
            }

            targetAngle = keyboardTargetAngle;
        }

        // Lance gets heavier as the horse speeds up
        float speedPercent = currentSpeed / maxSpeed;
        float smoothTime = Mathf.Lerp(
            minLanceSmoothTime,
            maxLanceSmoothTime,
            speedPercent);

        float currentAngle = lance.transform.localEulerAngles.z;
        if (currentAngle > 180f)
            currentAngle -= 360f;

        float newAngle = Mathf.SmoothDampAngle(
            currentAngle,
            targetAngle,
            ref lanceAngularVelocity,
            smoothTime);

        if (lanceLocked)
        {
            newAngle = currentAngle; // Lock the lance in place
        }

        lance.transform.localRotation = Quaternion.Euler(0f, 0f, newAngle);
    }
}