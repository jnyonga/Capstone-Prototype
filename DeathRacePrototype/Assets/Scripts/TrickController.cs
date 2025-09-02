using System.Collections;
using UnityEngine;

public class TrickController : MonoBehaviour
{
    public enum TrickState
    {
        Normal, 
        InTrick
    }

    private TrickState currentState;
    public KeyCode kickflipButton;
    [SerializeField] private Animator carAnimator;

    private bool isGrounded;
    private float groundedDistance = 0.7f;

    private Rigidbody carRB;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        carRB = GetComponent<Rigidbody>();
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
        }
    }

    private void CheckInput()
    {
        if (Input.GetKeyDown(kickflipButton) && isGrounded && currentState != TrickState.InTrick)
        {
            StartCoroutine(DoTrick());
        }
    }

    private IEnumerator DoTrick()
    {
        currentState = TrickState.InTrick;
        Debug.Log("In trick");
        // Pop car up
        carRB.AddForce(Vector3.up * 15000, ForceMode.Impulse);

        yield return new WaitForSeconds(0.2f);

        carAnimator.Play("CarKickflip");

        yield return new WaitForSeconds(1.1f);

        currentState = TrickState.Normal;
    }

    private void UpdateState()
    {

    }
}
