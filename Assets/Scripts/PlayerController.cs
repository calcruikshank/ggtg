using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
// using UnityEditor.Animations;

public class PlayerController : NetworkBehaviour
{
    Rigidbody rb;
    Vector3 lastLookedPosition;
    float  horizontalMovement;
    float  verticalMovement;
    Vector2 inputFace;

    public State state;

    float currentSpeed;
    public Gun gun;
    //float acceleration;
    //float currentSpeed;

    [SerializeField] private Animator anim;

    [SerializeField] Transform pelvis;
    [SerializeField] Transform pelvisLegs;
    [SerializeField] Transform rootRoot;

    [SerializeField] Transform dieBody;
    [SerializeField] Animator capeAnim;
    [SerializeField] List<Vector3> faceRotations;
    [SerializeField] List<Color> faceColors;

    int currentFaceIndex = 0;
    Vector3 targetFaceRot;
    Color targetColor;

    public float entryTime;
    public float currentPercentage;
    public Image circleImage;
    float acceleration = 10f;

    bool pressedFireWhileDispelling = false;

    public GameObject dispellMesh;

    public float currentMana;

    Transform firstPersonCameraParent;

    public enum State
    {
        Normal,
        Firing,
        Rolling,
        Dispelling
    }


    private void Start()
    {
        if (IsOwner)
        {
            rb = this.GetComponentInChildren<Rigidbody>();
            Cursor.lockState = CursorLockMode.Locked;
            firstPersonCameraParent = Camera.main.transform.parent;
            firstPersonCameraParent.transform.position = this.transform.position; 
            firstPersonCameraParent.transform.parent = pelvis.transform; 
            gun = GetComponentInChildren<Gun>();
            gun.Init(this);
        }
    }

    void Update()
    {
        if (!IsOwner) return;
        HandleInput();
        switch (state)
        {
            case State.Normal:
                FaceLookDirection();
                HandleMovement();
                HandleFireInput();
                HandleDispelInput();
                HandleRollInput();
                HandleJumpInput();
                break;
            case State.Firing:
                FaceLookDirection();
                HandleMovement();
                HandleDispelInput();
                HandleRollInput();
                FireAnimation(anim.GetCurrentAnimatorStateInfo(1));
                HandleJumpInput();
                break;
            case State.Dispelling:
                HandleDispelAnimation(anim.GetCurrentAnimatorStateInfo(1));
                FaceLookDirection();
                HandleMovement();
                HandleRollInput();
                HandleJumpInput();
                break;
            case State.Rolling:
                FaceLookDirection();
                HandleRollAnimation(anim.GetCurrentAnimatorStateInfo(0));
                HandleJumpInput();
                break;
        }
    }

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask whatIsGround;
    public bool IsGrounded()
    {
        return Physics.CheckSphere(groundCheck.position, groundDistance, whatIsGround);
    }
    private void HandleJumpInput()
    {
        if (spacePressed && IsGrounded())
        {
            spacePressed = false;
            Jump();
        }
    }

    void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(transform.up * jumpHeight, ForceMode.Impulse);
    }

    float jumpHeight = 800;
    float mouseSensitivity = 2;
    float mouseX;
    float mouseY;
    private void HandleInput()
    {
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        if (Input.GetMouseButtonDown(0))
        {
            OnFireDown();
        }
        if (Input.GetMouseButtonUp(0))
        {
            OnFireUp();
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            OnRollDown();
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            OnRollUp();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpaceDown();
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            SpaceUp();
        }

        horizontalMovement = Input.GetAxisRaw("Horizontal");
        verticalMovement = Input.GetAxisRaw("Vertical");
    }
    bool spacePressed;
    private void SpaceDown()
    {
        spacePressed = true;
    }

    private void SpaceUp()
    {
        spacePressed = false;
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;
        switch (state)
        {
            case State.Normal:
                FixedHandleMovement();
                break;
            case State.Firing:
                FixedHandleMovement();
                break;
            case State.Dispelling:
                FixedHandleMovement();
                break;
            case State.Rolling:
                FixedHandleRoll();
                break;
        }
    }

    private void HandleMovement()
    {
        /*if (movement.magnitude != 0)
        {
            currentSpeed += 165 * Time.deltaTime;
        }
        if (movement.magnitude == 0)
        {
            currentSpeed = 0f;
        }*/
    }
    Vector3 moveDirection;
    private void FixedHandleMovement()
    {
        moveDirection = pelvis.forward * verticalMovement + pelvis.right * horizontalMovement;
        //rb.AddForce(movement.normalized * moveSpeed);
        rb.velocity = new Vector3( moveDirection.x * currentSpeed, rb.velocity.y, moveDirection.z * currentSpeed);
        if (moveDirection.normalized != Vector3.zero)
        {
            pelvisLegs.transform.forward = Vector3.Lerp(pelvisLegs.transform.forward, moveDirection, 20f * Time.deltaTime);
        }
        if (Mathf.Abs(rb.velocity.magnitude) >= .02f)
        {
            anim.SetBool("isMoving", true);
        }
        else
        {
            anim.SetBool("isMoving", false);
        }
        /*if (rb.velocity.magnitude > stats.Speed)
        {
            rb.velocity = rb.velocity.normalized * stats.Speed;
        }
        if (movement.magnitude == 0)
        {
            rb.velocity *= .97f;
        }*/
    }
    float currentRollSpeed;
    private void FixedHandleRoll()
    {
    }

    float yRotation;
    float xRotation;
    void FaceLookDirection()
    {
        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -89, 89);
        pelvis.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
    }

    bool fireDownPressed = false;
    bool fireStillPressed = false;
    public void OnFireDown()
    {
        fireDownPressed = true;
        fireStillPressed = true;
    }

    public void OnFireUp()
    {
        fireDownPressed = false;
        fireStillPressed = false;
    }
    public bool dispelDownPressed = false;
    void OnDispelDown()
    {
        dispelDownPressed = true;
    }
    void OnDispelUp()
    {
        dispelDownPressed = false;
    }

    bool rollDownPressed = false;
    void OnRollDown()
    {
        rollDownPressed = true;
    }
    void OnRollUp()
    {
        rollDownPressed = false;
    }

    public void TakeDamage(float damageSent)
    {
    }
    public void Die()
    {
    }
    void SpawnPlayerBody()
    {
        dieBody.transform.parent = null;
        dieBody.GetComponent<MeshCollider>().enabled = true;
        Rigidbody rb = dieBody.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.None;
        rb.AddForce(Vector3.up * 500f);
        rb.AddTorque(dieBody.transform.right * Random.Range(-500f, 500f));
    }
    /*void OnFaceSelect(InputValue value)
    {
        inputFace = value.Get<Vector2>();
        ChangeFace();
    }*/

    void ChangeFace()
    {
        currentFaceIndex += (int)inputFace.normalized.x;
        if (currentFaceIndex < 0)
        {
            currentFaceIndex = faceRotations.Count - 1;
        }
        if (currentFaceIndex > faceRotations.Count - 1)
        {
            currentFaceIndex = 0;
        }

        capeAnim.Play("CapeAnim");

        targetFaceRot = faceRotations[currentFaceIndex];
        targetColor = faceColors[currentFaceIndex];
    }

    public void PickRandomFace()
    {
        currentFaceIndex = Random.Range(0, faceRotations.Count);
        ChangeFace();
    }

    void HandleFireInput()
    {
        if (fireDownPressed)
        {
            fireDownPressed = false;
            ChangeStateToFire();
        }
    }
    void HandleDispelInput()
    {
        if (dispelDownPressed)
        {
            dispelDownPressed = false;
            StartDispelAnimation();
        }
    }
    void HandleRollInput()
    {
        if (rollDownPressed && !anim.GetCurrentAnimatorStateInfo(0).IsName("Roll"))
        {
            rollDownPressed = false;
            StartRollAnimation();
        }
    }

    void StartDispelAnimation()
    {
        dispelEntryTime = Time.time;
        ChangeStateToDispel();
    }

    float rollEntryTime;
    void StartRollAnimation()
    {
        rollEntryTime = Time.time;
        ChangeStateToRoll();
    }

    public float currentDispelPercentage;

    [SerializeField] Transform dispelPoint;
    GameObject newDispelObject;
    private void HandleDispelAnimation(AnimatorStateInfo stateInfo)
    {
        currentDispelPercentage = (Time.time - dispelEntryTime) / stateInfo.length;
        if (stateInfo.IsName("Dispel") && currentDispelPercentage > .5f && currentDispelPercentage <= .9f && newDispelObject == null)
        {
            newDispelObject = Instantiate(dispellMesh, dispelPoint.position, dispelPoint.rotation);
            //newDispelObject.transform.parent = gun.transform;
            newDispelObject.SetActive(true);
        }
        if (newDispelObject != null)
        {
            newDispelObject.transform.position = dispelPoint.position;
            newDispelObject.transform.rotation = dispelPoint.rotation;
        }
        if (stateInfo.IsName("Dispel") && currentDispelPercentage > 1.2f)
        {
            Destroy(newDispelObject);
        }
        if (!stateInfo.IsName("Dispel") && currentDispelPercentage > .4f)
        {
            ChangeStateToNormal();
        }
    }
    float currentRollPercentage;
    bool startRolling;
    void HandleRollAnimation(AnimatorStateInfo stateInfo)
    {
        float currentRollSpeedMulti = 3f;
        currentRollSpeed -= currentRollSpeedMulti * Time.deltaTime;

        currentRollPercentage = (Time.time - rollEntryTime) / stateInfo.length;

        if (stateInfo.IsName("Roll") && currentRollPercentage > .3f && currentRollPercentage <= .8f)
        {
            startRolling = true;
        }
        if (stateInfo.IsName("Roll") && currentRollPercentage > .8f)
        {
            startRolling = false;
            anim.SetBool("roll", false);
            ChangeStateToNormal();

        }
    }
    private void FireAnimation(AnimatorStateInfo stateInfo)
    {
        currentPercentage = (Time.time - entryTime) / stateInfo.length;

        if (stateInfo.IsName("Arms_Cast 1") && currentPercentage > .4f && !gun.hasFiredForAnim)
        {
            gun.Fire();
        }
        // Debug.Log(stateInfo.length);
        if (!stateInfo.IsName("Arms_Cast 1") && currentPercentage > .4f)
        {


            if (fireStillPressed == true && gun.hasFiredForAnim)
            {
                ChangeStateToFire();
            }

            if (!fireStillPressed == true && gun.hasFiredForAnim)
            {
                ChangeStateToNormal();
            }
        }
    }

    float dispelEntryTime;


    void HandleAnimationSpeeds()
    {
        anim.SetFloat("ArmsDispelSpeed", 1f);
        anim.SetFloat("ArmsAttackSpeed", 1f);
    }

    public MeshRenderer hatRenderer, cloakRenderer;

    public void ChangeColor(Color c)
    {
        Material hatMat = new Material(hatRenderer.material);
        hatMat.color = c;
        hatRenderer.material = hatMat;
        Material cloakMat = new Material(cloakRenderer.material);
        cloakMat.color = c;
        cloakRenderer.material = cloakMat;
        circleImage.color = new Color(c.r, c.g, c.b, 190.0f);
    }


    #region changingOfStates
    void ChangeStateToNormal()
    {
        // Debug.Log(currentDispelPercentage);
        currentSpeed = 10f;
        //wand.GetComponent<Collider>().enabled = false;
        anim.SetBool("dispel", false);
        anim.SetBool("cast", false);
        anim.SetBool("roll", false);
        state = State.Normal;
    }
    void ChangeStateToFire()
    {
        gun.hasFiredForAnim = false;
        entryTime = Time.time;
        anim.SetBool("cast", true);
        anim.SetBool("dispel", false);
        anim.SetBool("roll", false);
        state = State.Firing;
    }
    void ChangeStateToDispel()
    {
        anim.SetBool("dispel", true);
        anim.SetBool("cast", false);
        anim.SetBool("roll", false);
        state = State.Dispelling;
    }
    void ChangeStateToRoll()
    {
        if (newDispelObject != null)
        {
            Destroy(newDispelObject);
        }

        // Debug.Log(currentDispelPercentage);
        currentSpeed = 10f;
        //wand.GetComponent<Collider>().enabled = false;
        anim.SetBool("dispel", false);
        anim.SetBool("cast", false);
        anim.SetBool("roll", true);
        state = State.Rolling;
    }
    #endregion
}
