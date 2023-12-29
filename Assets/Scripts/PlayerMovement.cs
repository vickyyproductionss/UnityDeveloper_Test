using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public float turnSpeed = 20f;
    public float moveSpeed = 20f;

    Animator m_Animator;
    Rigidbody m_Rigidbody;
    Vector3 m_Movement;

    public GameObject Holo;
    Vector3 newGravity;

    bool IsInAir;
    bool IsTransforming;
    [SerializeField] int totalPointCubes;
    [SerializeField] GameObject winWindow;
    [SerializeField] GameObject lostWindow;
    [SerializeField] TMP_Text timerText;
    [SerializeField] TMP_Text pointCubesText;

    [Header("Timer")]
    private float timerDuration = 120f; // Two minutes in seconds
    private float currentTime = 0f;
    private bool timerRunning = false;

    void Start()
    {
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();
        StartTimer();
    }
    /// <summary>
    /// Handles scorig UI
    /// </summary>
    /// <param name="collected"></param>
    void HandleUI(int collected)
    {
        totalPointCubes -= collected;
        pointCubesText.text = $"Cubes Left: {totalPointCubes}";
        if(totalPointCubes == 0)
        {
            winWindow.SetActive(true);
        }
    }

    /// <summary>
    /// Starts the timer
    /// </summary>
    void StartTimer()
    {
        timerRunning = true;
    }

    /// <summary>
    /// Method to do something when timer ends
    /// </summary>
    void TimerComplete()
    {
        timerRunning = false;
        lostWindow.SetActive(true);
    }

    void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        if(Input.GetKeyDown(KeyCode.Space))
        {
            m_Animator.SetBool("Jump", true);
        }
        m_Movement = transform.forward * vertical + transform.right * horizontal;
        m_Movement.Normalize();

        bool hasHorizontalInput = !Mathf.Approximately(horizontal, 0f);
        bool hasVerticalInput = !Mathf.Approximately(vertical, 0f);
        bool isWalking = hasHorizontalInput || hasVerticalInput;
        m_Animator.SetBool("IsWalking", isWalking);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            IsInAir = true;
            Holo.SetActive(true);
            Holo.transform.position = transform.position + transform.forward + transform.up*2;//Sets position after Gravity Manipulation
            Vector3 targetUp = -transform.right;
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, targetUp) * transform.rotation;//Finds rotation after Gravity Manipulation
            Holo.transform.rotation = targetRotation;
            ChangeGravity(transform.right);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            IsInAir = true;
            Holo.SetActive(true);
            Holo.transform.position = transform.position + transform.forward + transform.up * 2;
            Vector3 targetUp = transform.right;
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, targetUp) * transform.rotation;
            Holo.transform.rotation = targetRotation;
            ChangeGravity(-transform.right);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            IsInAir = true;
            Holo.SetActive(true);
            Holo.transform.position = transform.position + transform.forward + transform.up * 2;
            Vector3 targetUp = -transform.forward;
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, targetUp) * transform.rotation;
            Holo.transform.rotation = targetRotation;
            ChangeGravity(transform.forward);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            IsInAir = true;
            Holo.SetActive(true);
            Holo.transform.position = transform.position + transform.forward + transform.up * 2;
            Vector3 targetUp = transform.forward;
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, targetUp) * transform.rotation;
            Holo.transform.rotation = targetRotation;
            ChangeGravity(-transform.forward);
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            IsTransforming = true;
            transform.rotation = Holo.transform.rotation;
            transform.position = Holo.transform.position;
            Physics.gravity = newGravity;
            IsTransforming = false;
            Holo.SetActive(false);
        }
        if (timerRunning)
        {
            currentTime += Time.deltaTime;

            float remainingSeconds = timerDuration - currentTime;
            int minutes = Mathf.FloorToInt(remainingSeconds / 60);
            int seconds = Mathf.FloorToInt(remainingSeconds % 60);

            timerText.text = $"Time Remaining: { minutes.ToString("00")} :  {seconds.ToString("00")}";

            if (currentTime >= timerDuration)
            {
                TimerComplete();
            }
        }
    }

    /// <summary>
    /// Changes gravity in the particular direction
    /// </summary>
    /// <param name="direction"></param>
    void ChangeGravity(Vector3 direction)
    {
        newGravity = direction.normalized * Physics.gravity.magnitude;
    }

    /// <summary>
    /// As update runs after update so used to make Jump False
    /// </summary>
    private void LateUpdate()
    {
        m_Animator.SetBool("Jump", false);
    }

    /// <summary>
    /// Moves the player to designated position
    /// </summary>
    void OnAnimatorMove()
    {
        if(!IsInAir && !IsTransforming)
        {
            m_Rigidbody.MovePosition(m_Rigidbody.transform.position + m_Movement * Time.deltaTime*moveSpeed);
        }
    }

    #region Collision Handling
    private void OnCollisionEnter(Collision collision)
    {
        IsInAir = false;
        falling = false;
        StopAllCoroutines();
        if(collision.gameObject.tag == "PointCube")
        {
            Destroy(collision.gameObject);
            HandleUI(1);
        }
        m_Animator.SetBool("Falling", false);
    }
    private void OnCollisionStay(Collision collision)
    {
        StopAllCoroutines();
        if(falling)
        {
            falling = false;
        }
        if(IsInAir)
        {
            IsInAir = false;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        falling = true;
        m_Animator.SetBool("Falling", true);
        Debug.Log("trueeee");
        StopAllCoroutines();
        freefall = StartCoroutine(GameOverOnFreeFall());
    }
    Coroutine freefall;
    bool falling;
    IEnumerator GameOverOnFreeFall()
    {
        yield return new WaitForSeconds(5);
        if(falling)
        {
            lostWindow.SetActive(true);
        }
    }
    #endregion

    /// <summary>
    /// Method to play again
    /// </summary>
    public void PlayAgain()
    {
        Physics.gravity = new Vector3(0, -9.8f, 0);
        SceneManager.LoadScene(0);
    }
}