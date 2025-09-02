using System.Collections;
using UnityEngine;

public class TrickController : MonoBehaviour
{
    public enum TrickState
    {
        Normal, 
        InTrick
    }

    
    [Header("Trick Buttons")]
    public KeyCode kickflipButton;
    public KeyCode heelflipButton;
    public KeyCode treflipButton;
    public KeyCode laserflipButton;

    [Header("Car State")]
    [SerializeField] private Animator carAnimator;
    [SerializeField] private BoxCollider carHitbox;
    private TrickState currentState;
    private bool isGrounded;
    private float groundedDistance = 0.7f;

    private Rigidbody carRB;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        carRB = GetComponent<Rigidbody>();
        carHitbox.enabled = false;
        currentState = TrickState.Normal;
    }

    // Update is called once per frame
    void Update()
    {
        CheckGrounded();
        CheckInput();
    }

    private void CheckGrounded()
    {
        isGrounded = false;

        if (currentState != TrickState.InTrick && Physics.Raycast(transform.position + (Vector3.up * 0.5f), Vector3.down, groundedDistance))
        {
            isGrounded = true;
            carHitbox.enabled = false;
        }
    }

    private void CheckInput()
    {
        if (isGrounded && currentState != TrickState.InTrick)
        {
            if (Input.GetKeyDown(kickflipButton))
            {
                StartCoroutine(DoTrick("CarKickflip"));
            }

            else if (Input.GetKeyDown(heelflipButton))
            {
                StartCoroutine(DoTrick("CarHeelflip"));
            }

            else if (Input.GetKeyDown(treflipButton))
            {
                StartCoroutine(DoTrick("CarTreflip"));
            }

            else if (Input.GetKeyDown(laserflipButton))
            {
                StartCoroutine(DoTrick("CarLaserflip"));
            }
        }
    }

    private IEnumerator DoTrick(string trickName)
    {
        currentState = TrickState.InTrick;
        
        // Pop car up
        carRB.AddForce(transform.up * 15000, ForceMode.Impulse);

        // Add trick specific forces
        if (trickName == "CarKickflip")
            carRB.AddForce(-transform.right * 7500, ForceMode.Impulse);
        if (trickName == "CarHeelflip")
            carRB.AddForce(transform.right * 7500, ForceMode.Impulse);

        // Perform trick
        carAnimator.Play(trickName);

        yield return new WaitForSeconds(0.1f);

        carHitbox.enabled = true;

        yield return new WaitForSeconds(1.0f); //Time adds up to 1.1 which is animation length

        currentState = TrickState.Normal;
    }
}
