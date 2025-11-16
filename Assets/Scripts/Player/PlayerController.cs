using UnityEngine;

public class PlayerController : MonoBehaviour
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
    private int extraJump;
    private AudioSource[] audioSource;
    private KeyCode[] keys;
    //[SerializeField] private GameObject jumpEffect;

    private void Awake()
    {
        transform.position = new Vector2(PlayerPrefs.GetFloat("SaveX"), PlayerPrefs.GetFloat("SaveY"));
        transform.rotation = Quaternion.Euler(0, PlayerPrefs.GetFloat("SaveZ"), 0);
        keys = new KeyCode[3];
        for(int k = 0; k<3; k++) keys[k] = (KeyCode)PlayerPrefs.GetInt("Key" + k);
        parentGroundLastPos = transform.position;
    }
    private void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponentsInChildren<AudioSource>();
        extraJump = 1;
    }
    private void FixedUpdate()
    {
        rb.velocity = new Vector2(movement * moveSpeed, rb.velocity.y);
    }
    private void Update()
    {
        if (Time.timeScale == 1)
        {
            if (jumpMode)
            {
                extraJump = 1;
            }
            if (Input.GetKey(keys[0]) && Input.GetKey(keys[1]) == false || Input.GetAxis("Horizontal") > 0.9f)
            {
                movement = 1;
                if(rb.gravityScale > 0) transform.rotation = Quaternion.Euler(0, 0, 0);
                else transform.rotation = Quaternion.Euler(180, 0, 0);
            }
            else if (Input.GetKey(keys[1]) && Input.GetKey(keys[0]) == false || Input.GetAxis("Horizontal") < -0.9f)
            {
                movement = -1;
                if (rb.gravityScale > 0) transform.rotation = Quaternion.Euler(0, 180, 0);
                else transform.rotation = Quaternion.Euler(180, 180, 0);
            }
            else
                movement = 0;
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
            if ((Input.GetKeyDown(keys[2]) || 
                Input.GetKeyDown(KeyCode.JoystickButton0) && PlayerPrefs.GetInt("Gamepad") == 1 ||
                Input.GetKeyDown(KeyCode.JoystickButton1) && PlayerPrefs.GetInt("Gamepad") == 2) && extraJump > 0)
            {
                audioSource[isGround ? 0 : 1].Play();
                rb.velocity = new Vector2(rb.velocity.x, jumpForce * 10 * rb.gravityScale / 25);
                extraJump--;
            }
            if ((Input.GetKeyUp(keys[2]) || 
                Input.GetKeyUp(KeyCode.JoystickButton0) && PlayerPrefs.GetInt("Gamepad") == 1 ||
                Input.GetKeyUp(KeyCode.JoystickButton1) && PlayerPrefs.GetInt("Gamepad") == 2) && (rb.velocity.y > 0 && rb.gravityScale > 0 || rb.velocity.y < 0 && rb.gravityScale < 0))
            {
                rb.velocity = new Vector2(rb.velocity.x, 0);
            }
            isGround = Physics2D.OverlapBox(posJump.position, hitboxJump, 0, layer);
            anim.SetBool("Ground", isGround);
            if (isGround)
            {
                extraJump = 1;
                if (parentGround)
                {   
                    Vector3 parentGroundPos = parentGround.position;
                    transform.position += parentGroundLastPos - parentGroundPos;
                    parentGroundLastPos = parentGroundPos;
                }
            }
            if (rb.velocity.y < -177 && rb.gravityScale > 0) rb.velocity = new Vector2(rb.velocity.x, -177);
            if (rb.velocity.y > 177 && rb.gravityScale < 0) rb.velocity = new Vector2(rb.velocity.x, 177);
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
        if(collision.transform == parentGround)
            parentGround = null;
    }
    public void GetExtraJump() => extraJump ++;
    private void OnDisable()
    {
        extraJump = 1;
    }
}
