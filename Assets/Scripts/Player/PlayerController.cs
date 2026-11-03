using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    private Animator anim;

    private Rigidbody2D rb;
    private float movement;
    [SerializeField] private float moveSpeed;
    
    [SerializeField] private float gravity = 75;
    [SerializeField] private float gravityAngle = -Mathf.PI * .5f;
    Vector2 gravityVector, gravityOrthogonal;
    public float GravityAngle { 
        get => gravityAngle;
        set { gravityVector = new Vector2(Mathf.Cos(value), Mathf.Sin(value)); gravityOrthogonal = new(-gravityVector.y, gravityVector.x); gravityAngle = value;}
        }
    
    [SerializeField] private float animSpeed;
    [SerializeField] private LayerMask layer;
    [SerializeField] private Transform posJump;
    [SerializeField] private Vector2 hitboxJump;
    [SerializeField] private bool isGround;
    [SerializeField] private float jumpForce;
    [SerializeField] private bool jumpMode;
    public int extraJump;
    private AudioSource[] audioSource;
    private static KeyCode[] keys;
    //[SerializeField] private GameObject jumpEffect;
    private Collider2D[] collidersNonalloc = new Collider2D[8];
    private void FixedUpdate()
    {
        isGround = false;
        for(int i = Physics2D.OverlapBoxNonAlloc(posJump.position, hitboxJump, gravityAngle, collidersNonalloc, layer); i > 0;)
            isGround |= !collidersNonalloc[--i].isTrigger;
        if (!IsMovementOverridden) rb.velocity = WithComponentAlong(rb.velocity, gravityOrthogonal, movement * moveSpeed);

        if (isGround && (movement == 0) && (Vector2.Dot(rb.velocity, gravityVector) >= 0))
            rb.Sleep();
        else
        {
            rb.velocity += gravity * Time.fixedDeltaTime * gravityVector;
            rb.WakeUp();
        }
        if (!IsMovementOverridden && Vector2.Dot(rb.velocity, gravityVector) > 177) rb.velocity = WithComponentAlong(rb.velocity, gravityVector, 177);
    }

    private void Awake()
    {
        GravityAngle = GravityAngle;
        transform.position = new Vector2(PlayerPrefs.GetFloat("SaveX"), PlayerPrefs.GetFloat("SaveY"));
        transform.rotation = Quaternion.Euler(0, PlayerPrefs.GetFloat("SaveZ"), 0);
        keys = new KeyCode[3];
        for (int k = 0; k < 3; k++) keys[k] = (KeyCode)PlayerPrefs.GetInt("Key" + k);
        parentGroundLastPos = transform.position;
    }
    private void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponentsInChildren<AudioSource>();
        extraJump = 1;
    }

    public static int Movement
    {
        get
        {
            float axis = Input.GetAxis("Horizontal");
            int i = 0;
            if (Input.GetKey(keys[0]) || axis > 0.9f)
                i++;
            if (Input.GetKey(keys[1]) || axis < -0.9f)
                i--;
            return i;
        }
    }
    public static bool AttemptsToJump => Input.GetKeyDown(keys[2]) ||
        Input.GetKeyDown(KeyCode.JoystickButton0) && PlayerPrefs.GetInt("Gamepad") == 1 ||
        Input.GetKeyDown(KeyCode.JoystickButton1) && PlayerPrefs.GetInt("Gamepad") == 2;
    public static bool HoldsJump => Input.GetKey(keys[2]) ||
        Input.GetKey(KeyCode.JoystickButton0) && PlayerPrefs.GetInt("Gamepad") == 1 ||
        Input.GetKey(KeyCode.JoystickButton1) && PlayerPrefs.GetInt("Gamepad") == 2;
    public static bool CancelsJump => Input.GetKeyUp(keys[2]) ||
        Input.GetKeyUp(KeyCode.JoystickButton0) && PlayerPrefs.GetInt("Gamepad") == 1 ||
        Input.GetKeyUp(KeyCode.JoystickButton1) && PlayerPrefs.GetInt("Gamepad") == 2;

    private void Update()
    {
        if (Time.timeScale == 1)
        {
            if (jumpMode)
            {
                extraJump = 1;
            }
            movement = (gravityVector.y + gravityVector.x) <= 0 ? Movement : -Movement;

            Quaternion z = Quaternion.AngleAxis(gravityAngle * Mathf.Rad2Deg + 90f, Vector3.forward);
            if (movement == 1)
            {
                transform.rotation = z * Quaternion.AngleAxis(0, Vector3.up);
            }
            else if (movement == -1)
            {
                transform.rotation = z * Quaternion.AngleAxis(180f, Vector3.up);
            }

            if (movement == 0 || Mathf.Abs(Vector2.Dot(rb.velocity, gravityOrthogonal)) <= 0.1f)
            {
                anim.SetBool("Walking", false);
                anim.speed = 1;
            }
            else
            {
                anim.SetBool("Walking", true);
                anim.speed = moveSpeed / animSpeed;
            }
            if (AttemptsToJump && (extraJump > 0 || IsMovementOverridden))   // OVERRIDE
            {
                bool fromOverride = ReleaseOverride();                        // OVERRIDE
                audioSource[isGround ? 0 : 1].Play();
                rb.velocity = JumpVelocity(rb.velocity, jumpForce, fromOverride);   // OVERRIDE
                if (!fromOverride) extraJump--;                               // OVERRIDE
            }
            if (!IsMovementOverridden && CancelsJump && Vector2.Dot(rb.velocity, gravityVector) < 0)   // OVERRIDE
            {
                rb.velocity = new Vector2(rb.velocity.x, 0);
            }
            anim.SetBool("Ground", isGround);
            if (isGround)
            {
                extraJump = 1;
                if (parentGround)
                {
                    Vector3 parentGroundPos = parentGround.position;
                    if (!IsMovementOverridden) transform.position += parentGroundLastPos - parentGroundPos;   // OVERRIDE
                    parentGroundLastPos = parentGroundPos;
                }
            }
        }
    }
    private Transform parentGround;
    private Vector3 parentGroundLastPos;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        parentGround = collision.transform;
        parentGroundLastPos = parentGround.position;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform == parentGround)
            parentGround = null;
    }
    public void GetExtraJump() => extraJump++;
    private void OnDisable()
    {
        extraJump = 1;
        ReleaseOverride();   // OVERRIDE
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = Matrix4x4.Translate(posJump.transform.position);
        Gizmos.matrix *= Matrix4x4.Rotate(Quaternion.Euler(0,0,gravityAngle * Mathf.Rad2Deg + 90));
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(hitboxJump.x,hitboxJump.y,0));
    }
}