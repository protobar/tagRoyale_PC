using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections;
using TMPro; // Add TextMeshPro namespace

public class PlayerController : MonoBehaviourPun
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float gravity = 20f; // Custom gravity value
    public Vector3 respawnPosition = new Vector3(0, 10, 0); // Respawn position at y=10
    public AudioClip footstepSound; // Footstep sound effect
    public float footstepInterval = 0.5f; // Interval between footsteps

    public float boostSpeed = 10f; // Speed during boost
    public float boostDuration = 2f; // Duration of the boost
    public float boostCooldown = 5f; // Time required to regenerate boost

    private float currentMoveSpeed;
    private bool isBoosting;
    private float boostTimer;
    private float boostCooldownTimer;

    private Rigidbody rb;
    private Animator anim;
    private bool isGrounded;
    private bool isTagged;
    private AudioSource audioSource; // Audio source for footstep sounds
    private float footstepTimer; // Timer to control footstep sounds

    public GameObject tagSphere;
    public Slider boostSlider; // UI Slider for boost status
    public TMP_Text playerNameText; // TMP text for displaying player name

    private float touchbackCountdown;
    [SerializeField] private float touchbackDuration;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        rb.useGravity = false; // Disable built-in gravity

        currentMoveSpeed = moveSpeed;

        if (!photonView.IsMine)
        {
            // Disable script and hide boost slider if this is not the local player
            enabled = false;
            if (boostSlider != null)
            {
                boostSlider.gameObject.SetActive(false);
            }
            if (playerNameText != null)
            {
                playerNameText.gameObject.SetActive(false);
            }
        }
        else
        {
            // Find the boost slider and player name text in the scene
            boostSlider = FindObjectOfType<Slider>();
            playerNameText = GameObject.Find("Text_Player1").GetComponent<TMP_Text>(); // Adjust if necessary

            if (boostSlider != null)
            {
                // Initialize boost slider
                boostSlider.maxValue = boostCooldown;
                boostSlider.value = boostCooldown;
            }

            if (playerNameText != null)
            {
                // Initialize player name text
                playerNameText.text = PhotonNetwork.NickName;
            }
        }
    }

    void Update()
    {
        if (!photonView.IsMine || gameObject == null)
        {
            return;
        }

        // Handle boost
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isBoosting && boostCooldownTimer <= 0)
        {
            StartCoroutine(Boost());
        }

        // Horizontal and vertical movement
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        rb.velocity = new Vector3(movement.x * currentMoveSpeed, rb.velocity.y, movement.z * currentMoveSpeed);

        // Apply gravity manually
        rb.velocity += Vector3.down * gravity * Time.deltaTime;

        // Check if the player falls below y=-10
        if (transform.position.y < -10f)
        {
            RespawnPlayer();
        }

        // Jumping
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
            anim.SetTrigger("isJumping");
            isGrounded = false;

            // Broadcast jump event to all clients
            photonView.RPC("Jump", RpcTarget.All);
        }

        // Rotate player based on movement direction
        if (movement.magnitude > 0)
        {
            anim.SetBool("isRunning", true);
            transform.rotation = Quaternion.LookRotation(movement); // Rotate to face movement direction

            // Play footstep sounds
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                PlayFootstepSound();
                footstepTimer = footstepInterval;
            }
        }
        else
        {
            anim.SetBool("isRunning", false);
        }

        // Update boost slider value
        if (boostSlider != null)
        {
            boostSlider.value = boostCooldown - boostCooldownTimer;
        }

        // Touch back timers
        if (touchbackCountdown > 0)
        {
            touchbackCountdown -= Time.deltaTime;
        }

        // Handle boost cooldown
        if (boostCooldownTimer > 0)
        {
            boostCooldownTimer -= Time.deltaTime;
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

    void RespawnPlayer()
    {
        // Reset position to respawnPosition and fall from a higher position
        transform.position = respawnPosition;
        rb.velocity = new Vector3(0, -jumpForce, 0); // Fall from a higher position
    }

    private IEnumerator Boost()
    {
        isBoosting = true;
        currentMoveSpeed = boostSpeed;
        boostTimer = boostDuration;

        while (boostTimer > 0)
        {
            boostTimer -= Time.deltaTime;
            yield return null;
        }

        currentMoveSpeed = moveSpeed;
        isBoosting = false;
        boostCooldownTimer = boostCooldown;
    }

    void PlayFootstepSound()
    {
        if (footstepSound != null && isGrounded && !audioSource.isPlaying)
        {
            audioSource.PlayOneShot(footstepSound);
        }
    }
}
