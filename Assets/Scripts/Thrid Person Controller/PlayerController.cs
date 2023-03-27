using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f; //recheck
    [SerializeField] float rotationSpeed = 500f;
    [SerializeField] float boostSpeed;

    [Header("Ground Check Settings")]
    [SerializeField] float groundCheckRadius = 0.2f;
    [SerializeField] Vector3 groundCheckOffset;
    [SerializeField] LayerMask groundLayer;

    bool isGrounded;
    bool hasControl = true;
    float moveAmount;

    public bool IsMouseController = false;

    float ySpeed;
    Quaternion targetRotation;

    CameraController cameraController;
    Animator animator;
    CharacterController characterController;
    NavMeshAgent agent;


    public Vector3 screenPosition;
    public Vector3 worldPosition;
    Plane plane = new Plane(Vector3.down, 0);

    RaycastHit hitInfo = new RaycastHit();


    private void Awake()
    {
        cameraController =  Camera.main.GetComponent<CameraController>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        //var velocity1;
        if (Input.GetKeyDown(KeyCode.X))
        {
            Debug.Log("Toggled to Mouse mode");
            cameraController.GetComponent<CameraController>().enabled = false;
            IsMouseController = true;
            agent.enabled = true;
            characterController.enabled = false;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Toggled to Keyboard mode");
            cameraController.GetComponent<CameraController>().enabled = true;
            IsMouseController = false;
            agent.enabled = false;
            characterController.enabled = true;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            Application.Quit();
        }
        if (IsMouseController)
        {
            MouseMode();
        }
        else
            KeyboardMode();
    }

    private void KeyboardMode()
    {
        //Debug.Log("arrow");

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        moveAmount = Mathf.Clamp01(Mathf.Abs(h) + Mathf.Abs(v));

        var moveInput = (new Vector3(h, 0, v)).normalized;

        var moveDir = cameraController.PlanarRotation * moveInput;
        //var moveDir =  moveInput;

        if (!hasControl)
            return;

        GroundCheck();
        if (isGrounded)
        {
            ySpeed = -0.5f;
        }
        else
        {
            ySpeed += Physics.gravity.y * Time.deltaTime;
        }

        var velocity = moveDir * moveSpeed;
        velocity.y = ySpeed;

        characterController.Move(velocity * Time.deltaTime);

        if (moveAmount > 0)
        {
            targetRotation = Quaternion.LookRotation(moveDir);
            if (Input.GetKey(KeyCode.RightShift))
            {
                //Debug.Log("Dash Activated");
                boostSpeed = 2f;
                //moveSpeed = 50f;
            }
            else
                boostSpeed = moveAmount;
        }
        else
            boostSpeed = moveAmount;

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation,
            rotationSpeed * Time.deltaTime);

        //Debug.Log("moveAmount: " + boostSpeed);
        animator.SetFloat("moveAmount", boostSpeed, 0.2f, Time.deltaTime);
    }

    public void MouseMode()
    {
        //Mathf.Clamp01(agent.velocity.magnitude);
        animator.SetFloat("moveAmount", Mathf.Clamp01(agent.velocity.magnitude));
        //Debug.Log("point funciton");
        //animator.SetFloat("moveAmount", agent.velocity.magnitude);
        if (Input.GetMouseButton(0))
        {
            //Debug.Log("Mouse Clicked");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //Debug.DrawRay(ray.origin, ray.direction, Color.red);
            if (Physics.Raycast(ray.origin, ray.direction, out hitInfo))
            {
                //Debug.Log("Executing");
                agent.destination = hitInfo.point;
            }
        }
    }

    //IEnumerator MovePlayer(Vector3 offset)
    //{
    //    characterController.Move(offset * Time.deltaTime);
    //    yield return null;
    //}

    void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius, groundLayer);
    }

    public void SetControl(bool hasControl)
    {
        this.hasControl = hasControl;
        characterController.enabled = hasControl;

        if (!hasControl)
        {
            animator.SetFloat("moveAmount", 0f);
            targetRotation = transform.rotation;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius);
    }

    public float RotationSpeed => rotationSpeed;
    public float MoveAmount => moveAmount;
}
