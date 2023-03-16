using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    private Animator anima;
    private float moveInput;
    private bool isGrounded;
    private float jumpTimeCounter;
    private bool isJumping;

    private enum State { idle, running, jumping, falling, hurt};
    private State state = State.idle;
    private Collider2D coll;

    //Menginisialisasi Menyerang
    public Transform attackPoint;
    public float attackRange = 1f;
    public LayerMask attackMask;
    public int damageBoss = 25;

    //Menginisialisasi Saat Lari
    public float speed;
    //Menginisialisasi Saat Lompat
    public float jumpForce;
    public float jumpTime;
    public Transform feetPos;
    public float checkRadius;
    public LayerMask whatIsGround;
    public AudioSource footstep;
    public AudioSource koin;

    //Menginsisialisasi Terkena Demage
    public float hurtforce = 10f;

    //Menginisialisasi Environtment coin
    public int coin = 0;
    public Text coinText;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        anima = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();
        footstep = GetComponent<AudioSource>();
        koin = GetComponent<AudioSource>();
    }

    void FixedUpdate()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(moveInput * speed, rb.velocity.y);
    }

    // Update is called once per frame
    void Update()
    {
        if(state != State.hurt)
        {
            Movement();
        }
        AnimationState();
        anima.SetInteger("state", (int)state); //mengatur animasi awal
    //---------------------------------------attack jump----------------------------
        Collider2D colInfo = Physics2D.OverlapCircle(attackPoint.position, attackRange, attackMask);
        if (state == State.falling)
        {
            if (colInfo != null)
            {
                colInfo.GetComponent<BossHealth>().TakeDamageBoss(damageBoss);
            }
        }
    }
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
    //---------------------------------------attack jump----------------------------

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Collectable")
        {
            koin.Play();
            Destroy(collision.gameObject);
            coin += 1;
            coinText.text = coin.ToString();
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Boss")
        {
            if (state == State.falling)
            {
                Jump();
            }
        }

        if (other.gameObject.tag == "Enemy")
        {
            Enemy greenenemy = other.gameObject.GetComponent<Enemy>();
            if (state == State.falling)
            {
                greenenemy.JumpedOn();
                Jump();
            }
            else
            {
                state = State.hurt;
                if(other.gameObject.transform.position.x > transform.position.x)
                {
                    rb.velocity = new Vector2(-hurtforce, rb.velocity.y);
                }
                else 
                {
                    rb.velocity = new Vector2(hurtforce, rb.velocity.y);
                }
            }
        }
    }

    private void Movement()
    {
        isGrounded = Physics2D.OverlapCircle(feetPos.position, checkRadius, whatIsGround);
        //Jalan arah kiri
        if (moveInput < 0)
        {
            //merubah tampilan karakter mengarah ke kiri
            transform.localScale = new Vector2(-1, 1);
            //mengaktifkan animator running menjadi true dan animation lari karakter akan aktif
            anim.SetBool("running", true);
        }
        //Jalan arah kanan
        else if (moveInput > 0)
        {
            //merubah tampilan karakter mengarah ke kiri
            transform.localScale = new Vector2(1, 1);
            anim.SetBool("running", true);
        }
        else
        {
            anim.SetBool("running", false);
        }

        //Mencek kondisi juka ground true dan lompat isJumping akan di set true, 
        //jumptimecounter akan mengambil variabel dari jumptime yang sudah ditentukan
        //rb.velocity akan di set dari Vector2.up (shortscript dari vector2(0.1)) jadi Vector2.up * jumpForce yang sudah ditentukan
        if (isGrounded == true && Input.GetButtonDown("Jump"))
        {
            isJumping = true;
            jumpTimeCounter = jumpTime;
            Jump();
        }
        if (Input.GetButton("Jump"))
        {
            if (jumpTimeCounter > 0)
            {
                //kurang paham tapi kalau nebak mungkin jika kondisi timecounter lebih dari 0 timecounter akan di kurangi menjadi 0
                rb.velocity = Vector2.up * jumpForce;
                jumpTimeCounter -= Time.deltaTime;
            }
        }
        if (Input.GetButtonUp("Jump"))
        {
            isJumping = false;
        }
    }

    private void Jump()
    {
        rb.velocity = Vector2.up * jumpForce;
        state = State.jumping;
    }
    private void AnimationState()
    {
        if (state == State.jumping)
        {
            if (rb.velocity.y < .1f)
            {
                state = State.falling;
            }
        }
        else if (state == State.falling)
        {
            if (Input.GetButtonDown("Jump"))
            {
                state = State.jumping;
            }
            if (isGrounded == true)
            {
                state = State.idle;
            }
        }
        else if (state == State.hurt)
        {
            if(Mathf.Abs(rb.velocity.x) < .1f)
            {
                state = State.idle;
            }
        }


        else if(Mathf.Abs(rb.velocity.x) > 2f)
        {
            //moving
            state = State.running;
        }
        else
        {
            state = State.idle;
        }
    }

    private void Footstep()
    {
        footstep.Play();
    }
}
