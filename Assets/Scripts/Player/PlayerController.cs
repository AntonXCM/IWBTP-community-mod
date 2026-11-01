using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    private Animator anim;

    private Rigidbody2D rb;
    private float movement;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float animSpeed;
    [SerializeField] private LayerMask layer;
    [SerializeField] private Transform posJump;
    [SerializeField] private Vector2 hitboxJump;
    private bool isGround;
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
        for(int i = Physics2D.OverlapBoxNonAlloc(posJump.position, hitboxJump, 0, collidersNonalloc, layer); i > 0;)
            isGround |= !collidersNonalloc[--i].isTrigger;
        if (IsMovementOverridden) return;   // OVERRIDE
        rb.velocity = new Vector2(movement * moveSpeed, rb.velocity.y);

        if (isGround && (movement == 0) && (rb.velocity.y <= 0))
            rb.Sleep();
        else
            rb.WakeUp();
    }

    private void Awake()
    {
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
            movement = Movement;
            if (movement == 1)
            {
                if (rb.gravityScale > 0) transform.rotation = Quaternion.Euler(0, 0, 0);
                else transform.rotation = Quaternion.Euler(180, 0, 0);
            }
            else if (movement == -1)
            {
                if (rb.gravityScale > 0) transform.rotation = Quaternion.Euler(0, 180, 0);
                else transform.rotation = Quaternion.Euler(180, 180, 0);
            }
            if (movement == 0 || rb.velocity.x >= -0.1f && rb.velocity.x <= 0.1f)
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
                float jumpVy = jumpForce * 10 * rb.gravityScale / 25;
                rb.velocity = new Vector2(rb.velocity.x, fromOverride ? KeepMomentum(rb.velocity.y, jumpVy) : jumpVy);   // OVERRIDE
                if (!fromOverride) extraJump--;                               // OVERRIDE
            }
            if (!IsMovementOverridden && CancelsJump && (rb.velocity.y > 0 && rb.gravityScale > 0 || rb.velocity.y < 0 && rb.gravityScale < 0))   // OVERRIDE
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
            if (!IsMovementOverridden && rb.velocity.y < -177 && rb.gravityScale > 0) rb.velocity = new Vector2(rb.velocity.x, -177);   // OVERRIDE
            if (!IsMovementOverridden && rb.velocity.y > 177 && rb.gravityScale < 0) rb.velocity = new Vector2(rb.velocity.x, 177);     // OVERRIDE
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
}