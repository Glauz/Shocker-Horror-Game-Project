using UnityEngine;
using System.Collections;
using Enums;

public class FirstPersonController : MonoBehaviour
{

    [Header("Control Settings")]
    public float speed = 5f;
    public GameObject handIcon; //Should be in different class
    private float defaultSpeed;
    public float mouseSensitivityX = 100f, mouseSensitivityY = 100f, rotateSensitivity = 50f;

    private Camera playerCamera;
    private CharacterController cc;

    private State state = State.normal;
    public enum State { normal, grab, rotate }

    private MoveState moveState = MoveState.normal;
    public enum MoveState { normal, walk, sprint, falling}

    //Camera
    private float rotationX;

    //Raycast
    [Header("Interaction Settings")]
    private Ray ray;
    private RaycastHit hit;
    public float grabLength = 2.4f;
    public float carryLength = 1.1f;
    private bool isColliding;
    //private LayerMask prevLayer;

    //Carried GameObject
    [Header("Carry Object Settings")]
    private Grabbable grabbableScript;
    private Rigidbody carriedObject;
    public float throwStrength = 100f;
    public float magnetForce = 850f;            //When it stick to center of screen
    public float maxCarryDistance = 2f;         //If object is blocking path for object to get to player
    private bool rotateInPosition;              //Used to get the item to go into a custom position initially
    private float objectRotationSpeed = 560f;   //Initial rotation into place
    private Transform previousParent;

    //yVelocity
    [Header("Y-Axis Velocity Settings")]
    public float fallSpeed = 0.24f;
    public float jumpPower = 0.11f;
    private float prevStep;     //Used when jumping so player can't step onto objects while in the air.
    private Vector3 gravity;
    public float yVelocity;

    //Headbob
    [Header("Headbob Settings")]
    private float headbobInitial = 1f;
    public float headbobSpeed = 0.46f, headbobYOffset = 0.12f;
    public float headbobSpeedSprint = 0.76f, headbobYOffsetSprint = 0.42f;
    private bool headbobBottomCycle;

    //FOV
    [Header("FOV Settings")]
    private float fovInitial = 60f;
    public float fovSprint = 75f;
    public float fovSpeed = 6.6f;

    [Header("DEBUG")]
    public GameObject debugThrowable;
    private Animator anim;
    private Vector3 origCameraPos;

    // Use this for initialization
    void Start()
    {
        playerCamera = Camera.main;
        cc = GetComponent<CharacterController>();
        origCameraPos = playerCamera.transform.position;

        //If character controller doesn't exist
        if (cc == null)
            gameObject.AddComponent<CharacterController>();

        anim = GetComponent<Animator>();

        LockCursor();

        defaultSpeed = speed;        

    }

    // Update is called once per frame
    void Update()
    {
        //print(state);
        //playerCamera.transform.position = origCameraPos;

        //Player
        Move();
        Sprint();
        FieldOfViewSprint();
        //Headbob();
        Jump();
        Interact();
        InventoryThrow();
        Lean();
        Crouch();
        //Flashlight();

    }

    void LateUpdate()
    {
        //Camera
        RotatePlayer(); //if    in item Interact mode then rotate item instead
        RotateCamera();
    }

    void FixedUpdate()
    {
        //Object
        CarryObject();
        RotateObject();
        Throw();

        Fall();
        Jump();

    }

    private void RotatePlayer()
    {
        if (state == State.rotate) return;

        //If mouse is moved, camera moves
        transform.Rotate(new Vector3(0, Input.GetAxis("Mouse X"), 0) * Time.deltaTime * mouseSensitivityX);
    }

    private void RotateCamera()
    {
        if (state == State.rotate) return;

        rotationX -= Input.GetAxis("Mouse Y") * mouseSensitivityY * Time.deltaTime;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);
        //print(rotationX);

        playerCamera.transform.eulerAngles = new Vector3(rotationX,
                                                                playerCamera.transform.eulerAngles.y,
                                                                playerCamera.transform.eulerAngles.z);
    }

    private void LockCursor()
    {
        //Should put in GameManager
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Move()
    {
        if (state == State.rotate) return;

        float moveSpeed = speed * Time.deltaTime;

        //With Character Controller
        Vector3 destination = Vector3.zero;
        destination += transform.forward * Input.GetAxis("Vertical") * moveSpeed;
        destination += transform.right * Input.GetAxis("Horizontal") * moveSpeed;
        //destination += transform.up * yVelocity;    //Fall
        cc.Move(destination);

        //Without Character Controller
        //        transform.position += transform.forward * Input.GetAxis("Vertical") * moveSpeed;
        //        transform.position += transform.right * Input.GetAxis("Horizontal") * moveSpeed;
    }

    public void Lean()
    {
        anim.SetFloat("Lean", Input.GetAxis("Lean"));
    }

    public void Crouch()
    {
        anim.SetFloat("Crouch", Input.GetAxis("Crouch"));
    }

    private void Sprint()
    {
        if (Input.GetButton("Fire3"))
        {
            speed = defaultSpeed + (speed * 0.5f);
            moveState = MoveState.sprint;
        }
        else if (speed != defaultSpeed)
        {
            moveState = MoveState.normal;
            speed = defaultSpeed;
        }

        //Should change fov when sprinting
    }

    private void FieldOfViewSprint()
    {
        if (Input.GetButton("Fire3"))
            playerCamera.fieldOfView = Mathf.MoveTowards(playerCamera.fieldOfView, fovSprint, fovSprint * Time.deltaTime);
        else if (playerCamera.fieldOfView != fovInitial)
            playerCamera.fieldOfView = Mathf.MoveTowards(playerCamera.fieldOfView, fovInitial, fovSprint * Time.deltaTime);
    }

    private void Headbob()
    {
        float headbobSpeed = this.headbobSpeed, headbobYOffset = this.headbobYOffset;

        if (moveState == MoveState.sprint)
        {
            headbobSpeed = headbobSpeedSprint;
            headbobYOffset = headbobYOffsetSprint;
        }
        ///**** FIX ABOVE SLOPPY!

        //If False (camera goes up)
        if (!headbobBottomCycle)
        {
            //playerCamera.transform.position = Vector3.MoveTowards(playerCamera.transform.position,
            //                                                      playerCamera.transform.position + (playerCamera.transform.up * headbobInitial),
            //                                                      Time.deltaTime * headbobSpeed);

            playerCamera.transform.Translate(playerCamera.transform.up * headbobSpeed * Time.deltaTime, Space.World);

            if (playerCamera.transform.localPosition.y >= headbobInitial)
                headbobBottomCycle = true;
        }

        //If moving (In between so camera goes back up) | Don't work when falling
        if (Input.GetAxisRaw("Horizontal") + Input.GetAxisRaw("Vertical") == 0 || yVelocity >= 0.02f) { headbobBottomCycle = true; return; }

        //If True (camera go down)
        if (headbobBottomCycle)
        {
            //playerCamera.transform.position = Vector3.MoveTowards(playerCamera.transform.position,
            //                                                      playerCamera.transform.position - (playerCamera.transform.up * (headbobInitial - headbobYOffset)),
            //                                                      Time.deltaTime * headbobSpeed);

            playerCamera.transform.Translate(-playerCamera.transform.up * headbobSpeed * Time.deltaTime, Space.World);

            if (playerCamera.transform.localPosition.y <= headbobInitial - headbobYOffset)
                headbobBottomCycle = false;
        }
    }

    private void Interact()
    {
        if (state == State.rotate) return;

        ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        Debug.DrawRay(ray.origin, ray.direction * grabLength, Color.red);

        if (state != State.grab)
            GameManager.Instance.SetCursor(cursor.normal);

        //This is to refresh lock cursor
        if (Input.GetButtonDown("Fire1")) LockCursor();

        if (Physics.Raycast(ray, out hit, grabLength))
        {

            if (state != State.grab)
            {
                grabbableScript = hit.transform.GetComponent<Grabbable>();
                if (grabbableScript != null)
                    GameManager.Instance.SetCursor(cursor.interact);
            }


            //Initial Interaction
            if (Input.GetButtonDown("Fire1") && state != State.grab && grabbableScript != null && grabbableScript.enable)
            {
                //Only work if in normal state (not carrying an object already)
                if (state == State.normal)
                {
                    if (grabbableScript.parent != null)
                    {
                        grabbableScript.transform.SetParent(null);
                        grabbableScript.parent = null;
                    }

                    carriedObject = grabbableScript.GetComponent<Rigidbody>();
                    state = State.grab;
                    carriedObject.isKinematic = false;
                    GameManager.Instance.SetCursor(cursor.none);

                    if (grabbableScript.initialRotation == Grabbable.RotateType.zero)
                        carriedObject.transform.rotation = Quaternion.identity;

                    //prevLayer = carriedObject.gameObject.layer;
                    //carriedObject.gameObject.layer = 2;     //Set to ignore raycast so character controller doesn't use carried object as platform

                }

            }

        }

        if (Input.GetButtonUp("Fire1") && state == State.grab)
            DropObject();

    }

    private void Jump()
    {
        if (Input.GetButtonDown("Jump") && cc.isGrounded)
            yVelocity = jumpPower;
    }

    private void Fall()
    {
        if (!cc.isGrounded || yVelocity > 0f)
        {
            yVelocity -= fallSpeed * Time.deltaTime;
            cc.Move(Vector3.zero + transform.up * yVelocity);

            if (cc.stepOffset != 0f)
            {
                prevStep = cc.stepOffset;
                cc.stepOffset = 0f;
            }
        }

        else if (yVelocity != 0f)
        {
            //print("IS GROUNDED!");
            //if yVelocity != Vector3.zero)
            //    yVelocity = Vector3.zero;
            yVelocity = 0f;
            cc.stepOffset = prevStep;
        }
    }

    private void Throw()
    {
        if (state == State.grab && Input.GetButton("Fire2"))
        {
            //Give forward force to throw object
            carriedObject.AddForce((carriedObject.transform.position - transform.position) * throwStrength);

            DropObject();

        }
    }

    //Can do additional Commands when in carryMode
    private void CarryObject()
    {
        if (state == State.grab || state == State.rotate)
        {
            //carriedObject.transform.position = Vector3.Lerp(carriedObject.position, ray.origin + ray.direction * rayLength, 15f * Time.deltaTime);
            Vector3 moveTo = ray.origin + ray.direction * carryLength;
            //carriedObject.MovePosition(moveTo);

            //Get rotation into initial position if custom rotation is set
            if (grabbableScript.initialRotation == Grabbable.RotateType.custom && !rotateInPosition)
            {
                carriedObject.transform.rotation = Quaternion.RotateTowards(carriedObject.transform.rotation,   //Initial Rotation
                        Quaternion.Euler(playerCamera.transform.eulerAngles + grabbableScript.customRotation),  //Rotate towards custom 
                                                   objectRotationSpeed * Time.deltaTime);               //Speed

                //                if (carriedObject.transform.rotation == Quaternion.Euler(playerCamera.transform.rotation * grabbableScript.customRotation))
                //                    rotateInPosition = true;  
            }

            //Just only rotate object as player turns
            else
            {
                //Rotate relative to world space
                carriedObject.transform.Rotate(0, Input.GetAxis("Mouse X") * Time.deltaTime * mouseSensitivityX, 0, Space.World);
            }

            //How the item is being held
            carriedObject.AddForce((moveTo - carriedObject.transform.position) * magnetForce);
            carriedObject.velocity = (moveTo - carriedObject.transform.position);

            if (Vector3.Distance(moveTo, carriedObject.transform.position) > maxCarryDistance)
                DropObject();
        }
    }

    private void DropObject()
    {
        state = State.normal;
        rotateInPosition = false;
        //carriedObject.gameObject.layer = prevLayer;
        carriedObject = null;

        //Any changes here add in throw (since throw needs to be reordered for throwing)
    }

    private void RotateObject()
    {
        if (state == State.normal) return;

        if (Input.GetKey(KeyCode.R))
        {
            //print("Rotate Mode!");
            state = State.rotate;
            rotateInPosition = true;

            //Rotate relative to world space
            carriedObject.transform.Rotate(new Vector3(Input.GetAxis("Mouse Y") * Time.deltaTime * rotateSensitivity,
             -Input.GetAxis("Mouse X") * Time.deltaTime * rotateSensitivity, 0), Space.World);
        }

        else if (state == State.rotate && state != State.normal && !Input.GetKey(KeyCode.R))
            state = State.grab;

        else if (!Input.GetButton("Fire1"))
            DropObject();
    }

    private void InventoryThrow()
    {
        //Throw Debug throwable object
        //Object Pool this later!



    }
}
