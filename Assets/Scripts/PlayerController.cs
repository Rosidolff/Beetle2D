using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float torqueAmount = 30f;
    [SerializeField] private float flapForce = 30f;
    [SerializeField] private ScoreManager scoreManager;

    [SerializeField] private ParticleSystem dustEffect;
    [SerializeField] private ParticleSystem flapEffect;
    [SerializeField] private ParticleSystem windEffect;

    [SerializeField] private GameObject charactersFly;
    [SerializeField] private GameObject charactersGround;

    //******
    [SerializeField] private float centerOfMassHeight = 0.1f; // Unbalance center of mass to make the player have to keep the balance

    // the sprites starts to fliker when the ball bounces on the bumps of the floor, so we wait a bit before showing the fly sprites
    [SerializeField] private float flyDelay = 0.2f; // Time in the air before showing the fly sprites. 

    // SurfaceEffector2D surfaceEffector2D;
    Rigidbody2D rb;

    float baseGravity; // Store the initial gravity scale of the Rigidbody2D
    // float baseSpeed;    
    InputAction moveAction;
    Vector2 moveInput;
    bool upWasPressed; // Store if the up key was pressed in the previous frame
    float timeInAir; // Store the time the ball has been in the air
    bool isGrounded; // Store if the ball is touching the floor
    int score; // Store the total score of the player

    float wingsSpeed; // Vertical speed with the wings power-up (0 when it is not active) replace the flapForce and gravityScale
    int scoreMultiplier = 1; // Multiply the points (2 with the DoublePoints power-up)
    float windTorque; // Torque applied by the wind power-down the resistence of the wind (0 when it is not active)


    private bool canControlPlayer = true; // Flag to control player input

    public bool CanControlPlayer { get => canControlPlayer; set => canControlPlayer = value; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Activate the selected character model
        int selectedCharacter = PlayerPrefs.GetInt("SelectedCharacter", 0);
        charactersFly.transform.GetChild(selectedCharacter).gameObject.SetActive(true);
        charactersGround.transform.GetChild(selectedCharacter).gameObject.SetActive(true);

        moveAction = InputSystem.actions.FindAction("Move");
        rb = GetComponent<Rigidbody2D>();
        // surfaceEffector2D = FindAnyObjectByType<SurfaceEffector2D>();
        // baseSpeed = surfaceEffector2D.speed; // Store the initial speed of the SurfaceEffector2D
        baseGravity = rb.gravityScale; // Store the initial gravity scale of the Rigidbody2D


        //******* Place the center of mass just above the center of the ball, so the player has to keep the balance (modo dark souls)
        Vector2 ballCenter = GetComponent<CircleCollider2D>().offset;
        rb.centerOfMass = ballCenter + new Vector2(0, centerOfMassHeight);

        // The player starts in the air until the ball touches the floor
        SetCharacterSprites(false);
    }


    // Update is called once per frame
    void Update()
    {
        if (!canControlPlayer) return; // If player input is disabled, exit the method

        moveInput = moveAction.ReadValue<Vector2>();
        PlayerFly();
        CheckGround();

    }

    // FixedUpdate is called at a fixed rate, in sync with the physics
    // Torque is applied here so it doesn't depend on the frame rate 
    //Update it is not enough it crash imediately forward when the player press forward, so we use FixedUpdate to apply the torque 
    // more fluidly.
    void FixedUpdate()
    {
        if (!canControlPlayer) return;
        PlayerTorque();
    }

    /// <summary>
    /// Shows the ground or fly sprites. The fly sprites are shown only after flyDelay seconds
    /// in the air, so they don't flicker when the ball bounces on the bumps of the floor.
    /// </summary>
    void CheckGround()
    {
        if (isGrounded)
        {
            timeInAir = 0;
        }
        else
        {
            timeInAir += Time.deltaTime;
        }

        bool onGround = timeInAir < flyDelay;
        if (onGround != charactersGround.activeSelf) // Only when the state changes
        {
            SetCharacterSprites(onGround);
        }
    }

    void SetCharacterSprites(bool onGround)
    {
        charactersGround.SetActive(onGround);
        charactersFly.SetActive(!onGround);

        if (onGround)
        {
            dustEffect.Play();
        }
        else
        {
            dustEffect.Stop();
        }
    }

    /// <summary>
    /// Applies torque to the player based on the horizontal input from the Move action.
    /// </summary>
    void PlayerTorque()
    {
        if (moveInput.x < 0)
        {
            rb.AddTorque(torqueAmount);
        }
        else if (moveInput.x > 0)
        {
            rb.AddTorque(-torqueAmount);
        }

        rb.AddTorque(windTorque); // Wind power-down torque (0 when it is not active)

    }

    void PlayerFly()
    {
        //Flap when the up arrow key is pressed (only in the frame it is pressed, like Flappy Bird)
        //With the wings power-up the vertical speed is controlled with the up and down arrow keys
        bool upPressed = moveInput.y > 0;

        if (wingsSpeed > 0)
        {
            // If the player has the wings power-up, wich means no gravity so the velocity in vertical direction is controlled by the up and down arrow keys, and the player can move up and down freely
            if (moveInput.y != 0 || !isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, moveInput.y * wingsSpeed);
            }
        }
        // If the player doesn't have the wings power-up, allow flapping only when the up arrow key is pressed
        else if (upPressed && !upWasPressed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, flapForce); // Same vertical speed in every flap
            flapEffect.Play();
        }

        //with this avoid long presses of the up arrow key, so the player can only flap once per press.
        upWasPressed = upPressed; // Update the previous state for the next frame
    }

    // Detects when the player collides with the floor and plays the snow effect
    void OnCollisionEnter2D(Collision2D collision)
    {
        int layerIndex = LayerMask.NameToLayer("Floor");
        if (collision.gameObject.layer == layerIndex)
        {
            isGrounded = true;
        }
    }

    // Detects when the player exits the collision with the floor and stops the snow effect
    void OnCollisionExit2D(Collision2D collision)
    {
        int layerIndex = LayerMask.NameToLayer("Floor");
        if (collision.gameObject.layer == layerIndex)
        {
            isGrounded = false;
        }
    }

    /// <summary>
    /// Adds the points of a light point to the score and shows it
    /// </summary>
    public void AddPoints(int points)
    {
        score += points * scoreMultiplier;
        scoreManager.AddScore(score);
    }

    public void ApplyPowerUp(PowerUpScriptableObject powerUpData)
    {
        if (powerUpData.PowerUpType == "Wings")
        {
            wingsSpeed = powerUpData.PowerUpValue;
            rb.gravityScale = 0; // No gravity while the power-up is active
        }
        else if (powerUpData.PowerUpType == "DoublePoints")
        {
            scoreMultiplier = (int)powerUpData.PowerUpValue; // Multiply the points by the power-up value
        }
        else if (powerUpData.PowerUpType == "Wind")
        {
            windTorque = powerUpData.PowerUpValue; // Start applying the wind torque
            windEffect.Play();
        }
    }

    public void DeactivatePowerUp(PowerUpScriptableObject powerUpData)
    {
        if (powerUpData.PowerUpType == "Wings")
        {
            wingsSpeed = 0; // Remove the wings
            rb.gravityScale = baseGravity; // Return to normal gravity
        }
        else if (powerUpData.PowerUpType == "DoublePoints")
        {
            scoreMultiplier = 1; // Return to normal points
        }
        else if (powerUpData.PowerUpType == "Wind")
        {
            windTorque = 0; // Stop the wind torque
            windEffect.Stop();
        }
    }
}