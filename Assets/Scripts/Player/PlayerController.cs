using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private PlayerInput input;
    private Vector2 movement;
    private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    public event Action OnEncountered;


    private void Awake()
    {
        input = new PlayerInput();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

    }
    private void OnEnable()
    {
        input.Enable();
    }
    public void HandleUpdate()
    {
        PlayerInput();
        Move();
    }
    private void PlayerInput()
    {
        movement = input.Movement.Move.ReadValue<Vector2>();
    }
    private void Move()
    {
        if (movement != Vector2.zero)
        {
            Vector2 moveDirection = movement.normalized * (moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(rb.position + moveDirection);
            animator.SetBool("isMoving", true);
            animator.SetFloat("moveX", movement.x);
            animator.SetFloat("moveY", movement.y);
        }
        else
        {
            animator.SetBool("isMoving", false);
        }

        //animator.SetFloat("Speed", runSpeed);

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            OnEncountered();
        }
    }

}
