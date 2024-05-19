using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPun
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float gravity = 20f; // Custom gravity value

    private Rigidbody rb;
    private Animator anim;
    private bool isGrounded;
    private bool isTagged;

    public GameObject tagSphere;

    private float touchbackCountdown;
    [SerializeField] private float touchbackDuration;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        rb.useGravity = false; // Disable built-in gravity

        if (!photonView.IsMine)
        {
            enabled = false; // Disable script if this is not the local player
        }
    }

    void Update()
    {
        if (!photonView.IsMine || gameObject == null)
        {
            return;
        }

        // Horizontal movement
        float moveHorizontal = Input.GetAxis("Horizontal");
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, 0.0f);
        rb.velocity = new Vector3(movement.x * moveSpeed, rb.velocity.y, 0.0f);

        // Apply gravity manually
        rb.velocity += Vector3.down * gravity * Time.deltaTime;

        // Jumping
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, 0.0f);
            anim.SetTrigger("isJumping");
            isGrounded = false;

            // Broadcast jump event to all clients
            photonView.RPC("Jump", RpcTarget.All);
        }

        // Rotate player based on movement direction
        if (moveHorizontal < 0) // Moving left
        {
            anim.SetBool("isRunning", true);
            transform.rotation = Quaternion.Euler(0, -90, 0); // Rotate left
        }
        else if (moveHorizontal > 0) // Moving right
        {
            anim.SetBool("isRunning", true);
            transform.rotation = Quaternion.Euler(0, -270, 0); // Rotate right
        }
        else
        {
            anim.SetBool("isRunning", false);
        }

        // Touch back timers
        if (touchbackCountdown > 0)
        {
            touchbackCountdown -= Time.deltaTime;
        }
    }

    [PunRPC]
    void Jump()
    {
        anim.SetTrigger("isJumping");
    }

    void OnCollisionEnter(Collision collision)
    {
        var otherPlayer = collision.collider.GetComponent<PlayerController>();

        if (otherPlayer != null)
        {
            if (isTagged && touchbackCountdown <= 0f)
            {
                // Untag ourself
                photonView.RPC("OnUnTagged", RpcTarget.AllBuffered);

                // Tag the collided player
                otherPlayer.photonView.RPC("OnTagged", RpcTarget.AllBuffered);
            }
        }

        // Check if player is grounded
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        // Check if player is not grounded
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    [PunRPC]
    public void OnTagged()
    {
        // Flag as tagged
        isTagged = true;

        // Start the touchback countdown
        touchbackCountdown = touchbackDuration;

        // Turn on the sphere tag Game object
        tagSphere.SetActive(true);
    }

    [PunRPC]
    public void OnUnTagged()
    {
        // Flag as untagged
        isTagged = false;

        // Turn off the sphere tag Game object
        tagSphere.SetActive(false);
    }

    public bool IsTagged()
    {
        return isTagged;
    }
}
