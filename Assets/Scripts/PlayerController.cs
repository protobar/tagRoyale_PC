using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections;
using TMPro;
using Photon.Voice.Unity;
using Photon.Voice.PUN;


public class PlayerController : MonoBehaviourPun
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float gravity = 20f;
    public Vector3 respawnPosition = new Vector3(0, 10, 0);
    public AudioClip footstepSound;
    public float footstepInterval = 0.5f;

    public float boostSpeed = 10f;
    public float boostDuration = 2f;
    public float boostCooldown = 5f;

    public float mouseSensitivity = 100f;

    public GameObject respawnEffectPrefab;
    public AudioClip respawnSound;

    private float currentMoveSpeed;
    private bool isBoosting;
    private float boostTimer;
    private float boostCooldownTimer;

    private Rigidbody rb;
    private Animator anim;
    private bool isGrounded;
    private bool isTagged;
    private AudioSource audioSource;
    private float footstepTimer;

    public GameObject tagSphere;
    public Slider boostSlider;
    public TMP_Text playerNameText;

    private float touchbackCountdown;
    [SerializeField] private float touchbackDuration;

    private CursorManager cursorManager;
    private Collider triggerCollider;

    // Joystick variables
    public Joystick joystick;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        rb.useGravity = false;

        cursorManager = FindObjectOfType<CursorManager>();

        currentMoveSpeed = moveSpeed;

        // Initialize trigger collider
        triggerCollider = GetComponent<SphereCollider>();
        triggerCollider.isTrigger = true;

        // Ignore collisions with other players
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        foreach (PlayerController player in players)
        {
            if (player != this)
            {
                Physics.IgnoreCollision(GetComponent<Collider>(), player.GetComponent<Collider>());
            }
        }

        if (!photonView.IsMine)
        {
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
            boostSlider = FindObjectOfType<Slider>();
            playerNameText = GameObject.Find("Text_Player1").GetComponent<TMP_Text>();

            if (boostSlider != null)
            {
                boostSlider.maxValue = boostCooldown;
                boostSlider.value = boostCooldown;
            }

            if (playerNameText != null)
            {
                playerNameText.text = PhotonNetwork.NickName;
            }

            if (cursorManager != null)
            {
                cursorManager.LockCursor();
            }
        }

        // Assign joystick reference
        if (joystick == null)
        {
            joystick = GameObject.Find("Fixed Joystick").GetComponent<Joystick>();
        }
    }

    void Update()
    {
        if (!photonView.IsMine || gameObject == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && !isBoosting && boostCooldownTimer <= 0)
        {
            StartCoroutine(Boost());
        }

        // Use joystick for movement if on mobile
        float moveHorizontal = joystick.Horizontal;
        float moveVertical = joystick.Vertical;

        // Use keyboard for movement if not on mobile
        if (Mathf.Approximately(moveHorizontal, 0) && Mathf.Approximately(moveVertical, 0))
        {
            moveHorizontal = Input.GetAxis("Horizontal");
            moveVertical = Input.GetAxis("Vertical");
        }

        // Adjust movement for isometric view
        Vector3 movement = new Vector3(moveHorizontal, 0, moveVertical).normalized;
        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 desiredMoveDirection = (forward * moveVertical + right * moveHorizontal).normalized;

        rb.velocity = new Vector3(desiredMoveDirection.x * currentMoveSpeed, rb.velocity.y, desiredMoveDirection.z * currentMoveSpeed);

        // Rotate player to face the direction of movement
        if (desiredMoveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredMoveDirection), Time.deltaTime * 10f);
        }

        /*float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);*/

        rb.velocity += Vector3.down * gravity * Time.deltaTime;

        if (transform.position.y < -10f)
        {
            RespawnPlayer();
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
            anim.SetTrigger("isJumping");
            isGrounded = false;

            photonView.RPC("Jump", RpcTarget.All);
        }

        if (desiredMoveDirection.magnitude > 0)
        {
            anim.SetBool("isRunning", true);
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

        if (boostSlider != null)
        {
            boostSlider.value = boostCooldown - boostCooldownTimer;
        }

        if (touchbackCountdown > 0)
        {
            touchbackCountdown -= Time.deltaTime;
        }

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

    public void OnJumpButtonPressed()
    {
        if (isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
            anim.SetTrigger("isJumping");
            isGrounded = false;

            photonView.RPC("Jump", RpcTarget.All);
        }
    }

    public void OnBoostButtonPressed()
    {
        if (!isBoosting && boostCooldownTimer <= 0)
        {
            StartCoroutine(Boost());
        }
    }

    void OnTriggerEnter(Collider other)
    {
        var otherPlayer = other.GetComponent<PlayerController>();

        if (otherPlayer != null)
        {
            if (isTagged && touchbackCountdown <= 0f)
            {
                photonView.RPC("OnUnTagged", RpcTarget.AllBuffered);
                otherPlayer.photonView.RPC("OnTagged", RpcTarget.AllBuffered);
            }
        }

        if (other.CompareTag("Bound"))
        {
            RespawnPlayer();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    [PunRPC]
    public void OnTagged()
    {
        isTagged = true;
        touchbackCountdown = touchbackDuration;
        tagSphere.SetActive(true);
    }

    [PunRPC]
    public void OnUnTagged()
    {
        isTagged = false;
        tagSphere.SetActive(false);
    }

    public bool IsTagged()
    {
        return isTagged;
    }

    void RespawnPlayer()
    {
        transform.position = respawnPosition;
        rb.velocity = new Vector3(0, -jumpForce, 0);
        if (respawnEffectPrefab != null)
        {
            photonView.RPC("InstantiateRespawnEffect", RpcTarget.All, respawnPosition);
        }
        if (respawnSound != null)
        {
            photonView.RPC("PlayRespawnSound", RpcTarget.All);
        }
    }

    [PunRPC]
    void InstantiateRespawnEffect(Vector3 position)
    {
        Instantiate(respawnEffectPrefab, position, Quaternion.identity);
    }

    [PunRPC]
    void PlayRespawnSound()
    {
        Debug.Log("Playing respawn sound");
        audioSource.PlayOneShot(respawnSound);
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
