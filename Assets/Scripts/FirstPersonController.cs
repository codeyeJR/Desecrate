using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Photon.Pun;
using Photon.Realtime;

public class FirstPersonController : MonoBehaviour
{
    // Assesing whether the Player is currently allowed to do something and if the key is pressed for it
    public bool canMove { get; private set; } = true;
    private bool isSprinting => canSprint && Input.GetKey(sprintKey);
    private bool shouldJump => Input.GetKeyDown(jumpKey) && characterController.isGrounded;
    private bool shouldCrouch => Input.GetKeyDown(crouchKey) && !duringCrouchAnimation && characterController.isGrounded;

    // Options Variables
    [Header("Functional Options")]
    [SerializeField] private bool canSprint = true;
    [SerializeField] private bool canJump = true;
    [SerializeField] public bool mouseLocked = true;
    [SerializeField] private bool canCrouch = true;
    [SerializeField] private bool canUseHeadbob = true;
    [SerializeField] private bool willSlideOnSlope = true;
    [SerializeField] private bool canZoom = true;
    [SerializeField] private bool canInteract = true;
    [SerializeField] private bool useFootsteps = true;


    // Controls Variables
    [Header("Controls")]
    [SerializeField]  private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
    [SerializeField] private KeyCode zoomKey = KeyCode.Mouse1;
    [SerializeField] private KeyCode interactKey = KeyCode.Mouse0;

    // Movement Variables
    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float sprintSpeed = 6.0f;
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float slopeSpeed = 8f;
    
    // Look Variables
    [Header("Look Parameters")]
    [SerializeField, Range(1, 10)] private float lookSpeedX = 2.0f;
    [SerializeField, Range(1, 10)] private float lookSpeedY = 2.0f;
    [SerializeField, Range(1, 180)] private float upperLookLimit = 80.0f;
    [SerializeField, Range(1, 180)] private float lowerLookLimit = 80.0f;

    // Health Variables
    [Header("Health Parameters")]
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float timeBeforeRegenStarts = 3;
    [SerializeField] private float healthValueIncrement = 1;
    [SerializeField] private float healthTimeIncrement = 0.1f;
    private float currentHealth;
    private Coroutine regeneratingHealth;
    public static Action<float> OnTakeDamage;
    public static Action<float> OnDamage;
    public static Action<float> OnHeal;

    // Jump Variables
    [Header("Jumping Parameters")]
    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] private float gravity = 30f;

    // Crouch Variables
    [Header("Crouch Parameters")]
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float standHeight = 2f;
    [SerializeField] private float timeToCrouch = 0.25f;
    [SerializeField] private Vector3 crouchingCenter = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 standingCenter = new Vector3(0, 0, 0);
    private bool isCrouching;
    private bool duringCrouchAnimation;

    // Headbob Variables
    [Header("Headbob Parameters")]
    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float sprintBobSpeed = 18f;
    [SerializeField] private float sprintBobAmount = 0.1f;
    [SerializeField] private float crouchBobSpeed = 8f;
    [SerializeField] private float crouchBobAmount = 0.025f;
    private float defaultYPos = 0;
    private float timer;

    // Zoom Variables
    [Header("Zoom Parameters")]
    [SerializeField] private float timeToZoom = 0.3f;
    [SerializeField] private float zoomFOV = 30f;
    private float defaultFOV;
    private Coroutine zoomRoutine;

    // Footstep Variables
    [Header("Footstep Parameters")]
    [SerializeField] private float baseStepSpeed = 0.5f;
    [SerializeField] private float crouchStepMultiplier = 1.5f;
    [SerializeField] private float sprintStepMultiplier = 0.6f;
    [SerializeField] private AudioSource footstepAudioSource = default;
    [SerializeField] private AudioClip[] metalClips = default;
    [SerializeField] private AudioClip[] woodClips = default;
    [SerializeField] private AudioClip[] grassClips = default;
    private float footstepTimer = 0;
    private float getCurrentOffset => isCrouching ? baseStepSpeed * crouchStepMultiplier : isSprinting ? baseStepSpeed * sprintStepMultiplier : baseStepSpeed;

    // Slope Sliding Variables
    private Vector3 hitPointNormal;
    private bool isSliding 
    {
        get
        {
            // If currently on slope
            if(characterController.isGrounded && Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHit, 2f))
            {
                hitPointNormal = slopeHit.normal;
                return Vector3.Angle(hitPointNormal, Vector3.up) > characterController.slopeLimit;
                // This will return true when it deems the slope is steep enough to slide down
            }
            else
            {
                return false;
            }
        }
    }

    // Interaction Variables
    [Header("Interaction")]
    [SerializeField] private Vector3 interactionRayPoint = default;
    [SerializeField] private float interactionDistance = default;
    [SerializeField] private LayerMask interactionLayer = default;
    private Interactable currentInteractable;

    // Character Controller Variables
    private Camera playerCamera;
    private CharacterController characterController;
    private Vector3 moveDirection;
    private Vector2 currentInput;

    private float rotationX = 0;
    public static FirstPersonController Instance;

    // Networking
    PhotonView PV;
    
    private void OnEnable()
    {
        OnTakeDamage += ApplyDamage;
    }

    private void OnDisable()
    {
        OnTakeDamage -= ApplyDamage;
    }

    void Awake()
    {
        // Sets all of the default parameters when the Player is initialised
        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
        defaultYPos = playerCamera.transform.localPosition.y;
        defaultFOV = playerCamera.fieldOfView;
        currentHealth = maxHealth;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        PV = GetComponent<PhotonView>();
        Instance = this;
    }

    void Start()
    {
        if(!PV.IsMine)
        {
            // Destroys the Player Camer if not the Player that is being controlled
            Destroy(GetComponentInChildren<Camera>().gameObject);
        }
    }

    void Update()
    {
        if (canMove && PV.IsMine)
        {
            // Mouse locked added for use of generators
            HandleMovementInput();
            if(mouseLocked)
            {
                HandleMouseLook();
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            if(!mouseLocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            // Handling all of the movement toggles as long as they're allowd to activate
            if(canJump)
                HandleJump();

            if(canCrouch)
                HandleCrouch();
            
            if(canUseHeadbob)
                HandleHeadBob();

            if(canZoom)
                HandleZoom();

            if(useFootsteps)
                HandleFootsteps();
                
            if(canInteract)
            {
                HandleInteractionCheck();
                HandleInteractionInput();
            }
            // Instantiation for Proton, means that the camera will be activated if you are the Player that is being controlled
            if(!PV.IsMine)
                return;
            
            ApplyFinalMovement();
        }
    }

    private void HandleMovementInput()
    {
        // Finds the inputs from the Player for movement in the X and Y axis
        currentInput = new Vector2((isCrouching ? crouchSpeed : isSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Vertical"), (isCrouching ? crouchSpeed : isSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Horizontal"));

        float moveDirectionY = moveDirection.y;
        moveDirection = (transform.TransformDirection(Vector3.forward) * currentInput.x) + (transform.TransformDirection(Vector3.right) * currentInput.y);
        moveDirection.y = moveDirectionY;
    }

    private void HandleJump()
    {
        // Jumps when the player presses the jump key and when the Player is grounded
        if(shouldJump)
            moveDirection.y = jumpForce;
    }

    private void HandleMouseLook()
    {
        // Finds out where the mouse is moving and applies it ot the camera within the look limits
        rotationX -= Input.GetAxis("Mouse Y") * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);        
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        // Rotates the entire player model when the mouse is moving from left to right
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeedX, 0);
    }

    private void HandleCrouch()
    {
        if(shouldCrouch)
            StartCoroutine(CrouchStand());
    }

    private void HandleHeadBob()
    {
        if(!characterController.isGrounded) return;

        // Makes the head bob
        if(Mathf.Abs(moveDirection.x) > 0.1f || Mathf.Abs(moveDirection.z) > 0.1f)
        {
            timer += Time.deltaTime * (isCrouching ? crouchBobSpeed : isSprinting ? sprintBobSpeed : walkBobSpeed);
            playerCamera.transform.localPosition = new Vector3(
                playerCamera.transform.localPosition.x,
                defaultYPos + Mathf.Sin(timer) * (isCrouching ? crouchBobAmount : isSprinting ? sprintBobAmount : walkBobAmount),
                playerCamera.transform.localPosition.z
            );
        }
    }

    private void HandleZoom()
    {
        // Initiates zoom when the zoom key is pressed
        if(Input.GetKeyDown(zoomKey))
        {
            if(zoomRoutine != null)
            {
                StopCoroutine(zoomRoutine);
                zoomRoutine = null;
            }

            zoomRoutine = StartCoroutine(ToggleZoom(true));
        }

        // deistantiates the zoom when the key is let go of
        if(Input.GetKeyUp(zoomKey))
        {
            if(zoomRoutine != null)
            {
                StopCoroutine(zoomRoutine);
                zoomRoutine = null;
            }

            zoomRoutine = StartCoroutine(ToggleZoom(false));
        }
    } 

    private void HandleFootsteps()
    {
        if(!characterController.isGrounded) return;
        if(currentInput == Vector2.zero) return;

        // Counts down the time untill next footstep, Time.deltatTime is time between each fram
        footstepTimer -= Time.deltaTime;

        if(footstepTimer <= 0)
        {
            if(Physics.Raycast(playerCamera.transform.position, Vector3.down, out RaycastHit hit, 3))
            {
                // Playing footstep sounds from the library
                switch(hit.collider.tag)
                {
                    case "footsteps/wood" :
                        footstepAudioSource.PlayOneShot(woodClips[Random.Range(0, woodClips.Length - 1)]);
                        break;
                    case "footsteps/metal" :
                        footstepAudioSource.PlayOneShot(metalClips[Random.Range(0, metalClips.Length - 1)]);
                        break;
                    case "footsteps/grass" :
                        footstepAudioSource.PlayOneShot(grassClips[Random.Range(0, grassClips.Length - 1)]);
                        break;
                    default :
                        footstepAudioSource.PlayOneShot(grassClips[Random.Range(0, grassClips.Length - 1)]);
                        break;
                }
            }
        
            footstepTimer = getCurrentOffset;
        }
    }

    private void ApplyDamage(float dmg)
    {
        // Applys the damage the Player took
        currentHealth -= dmg;
        OnDamage?.Invoke(currentHealth);
        print("dmg" + dmg);

        // Kills the player when health is 0
        if(currentHealth <= 0)
            KillPlayer();
        else if(regeneratingHealth != null)
            StopCoroutine(regeneratingHealth);

        regeneratingHealth = StartCoroutine(RegenerateHealth());
    }

    private void KillPlayer()
    {
        currentHealth = 0;

        // Enacting the players death
        if(regeneratingHealth != null)
            StopCoroutine(regeneratingHealth);

        // Placeholder for deinstatiating the Player
        print("dead");
    }

    private void HandleInteractionCheck()
    {
        // Checking if the Player is looking at an ibject they can interact with
        if(Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out RaycastHit hit, interactionDistance))
        {
            if(hit.collider.gameObject.layer == 8 && (currentInteractable == null || hit.collider.gameObject.GetInstanceID() != currentInteractable.GetInstanceID()))
            {
                // Gets the information on the object the Player is looking at
                hit.collider.TryGetComponent<Interactable>(out currentInteractable);

                // Calls the function from Objectives.cs
                if(currentInteractable)
                    currentInteractable.onFocus();
            }
        }
        // Called when looking away
        else if(currentInteractable)
        {
            currentInteractable.onLoseFocus();
            currentInteractable = null;
        }
    }
    private void HandleInteractionInput()
    {
        // Interacting with the Object
        if(Input.GetKeyDown(interactKey) && currentInteractable != null && Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out RaycastHit hit, interactionDistance, interactionLayer))
        {
            currentInteractable.onInteract();
        }
    }

    private void ApplyFinalMovement()
    {
        // Falling if not grounded
        if(!characterController.isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;

        // Checking if the Player is going to slide down the slope
        if(willSlideOnSlope && isSliding)
            moveDirection += new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) * slopeSpeed;
        
        // Applying the final movements for the player
        characterController.Move(moveDirection * Time.deltaTime);
    }

    private IEnumerator CrouchStand()
    {
        // Breaking the crouch animation if already crouching
        if(isCrouching && Physics.Raycast(playerCamera.transform.position, Vector3.up, 1f))
            yield break;

        duringCrouchAnimation = true;

        // Data on where the Player currently is in the crouch animation
        float timeElapsed =0;
        float targetHeight = isCrouching ? standHeight : crouchHeight;
        float currentHeight = characterController.height;
        Vector3 targetCenter = isCrouching ? standingCenter : crouchingCenter;
        Vector3 currentCenter = characterController.center;

        // Finish crouching when the animation is finished or if it has not completed the animation
        while(timeElapsed < timeToCrouch)
        {
            characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed/timeToCrouch);
            characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed/timeToCrouch);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        characterController.height = targetHeight;
        characterController.center = targetCenter;

        isCrouching = !isCrouching;

        duringCrouchAnimation = false;
    }

    private IEnumerator ToggleZoom(bool isEnter)
    {
        float targetFOV = isEnter ? zoomFOV : defaultFOV;
        float startingFOV = playerCamera.fieldOfView;
        float timeElapsed = 0;

        // Runs the animation for zooming
        while(timeElapsed < timeToZoom)
        {
            playerCamera.fieldOfView = Mathf.Lerp(startingFOV, targetFOV, timeElapsed/timeToZoom);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        playerCamera.fieldOfView = targetFOV;
        zoomRoutine = null;
    }

    private IEnumerator RegenerateHealth()
    {
        yield return new WaitForSeconds(timeBeforeRegenStarts);
        WaitForSeconds timeToWait = new WaitForSeconds(healthTimeIncrement);

        // Regenerates the Players health
        while(currentHealth < maxHealth)
        {
            currentHealth += healthValueIncrement;

            if(currentHealth > maxHealth)
                currentHealth = maxHealth;

            OnHeal?.Invoke(currentHealth);
            yield return timeToWait;
        }

        regeneratingHealth = null;
    }
}