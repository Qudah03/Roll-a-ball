using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private int count;
    private int totalPickUps; // Dynamic count
    
    private float movementX;
    private float movementY;

    public float speed = 0;
    public TextMeshProUGUI countText;
    public GameObject winTextObject;
    public float maxVelocity = 15f; // Adjust this in the Inspector

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        count = 0;

        // DYNAMIC FIX: Count how many pickups actually exist in this specific scene
        totalPickUps = GameObject.FindGameObjectsWithTag("PickUp").Length;

        SetCountText();
        winTextObject.SetActive(false);
    }

    void OnMove(InputValue movementValue)
    {
        Vector2 movementVector = movementValue.Get<Vector2>();
        movementX = movementVector.x;
        movementY = movementVector.y;
    }

    void FixedUpdate()
    {
        Vector3 movement = new Vector3(movementX, 0.0f, movementY); // Keep your inverted fix here if you have it
        rb.AddForce(movement * speed);

        // Clamp the velocity so it doesn't infinitely accelerate
        if (rb.linearVelocity.magnitude > maxVelocity)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxVelocity;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 1. Handle Pickups
        if (other.gameObject.CompareTag("PickUp"))
        {
            other.gameObject.SetActive(false);
            count++;
            SetCountText();
        }

        // 2. Handle Maze Exit (For Level 2)
        if (other.gameObject.CompareTag("Exit"))
        {
            // 1. Show UI
            winTextObject.SetActive(true);
            var textComp = winTextObject.GetComponent<TMPro.TextMeshProUGUI>();
            if (textComp != null) textComp.text = "YOU ESCAPED!";
        
            // 2. Shut down the Enemy completely
            // Stop the AI and Player
            GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");
            if (enemy != null)
            {
                // Stop physical movement
                var agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null) agent.isStopped = true;

                // Disable the script so it stops trying to calculate paths
                var enemyBrain = enemy.GetComponent<EnemyMovement>();
                if (enemyBrain != null) enemyBrain.enabled = false;
            }
            
            // 3. Stop the player
            rb.linearVelocity = Vector3.zero;
            this.enabled = false;
        }
    }

    void SetCountText()
    {
        if (countText != null) countText.text = "Count: " + count.ToString();

        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // LEVEL 1 WIN CONDITION: Exactly 14 boxes
        if (currentScene == "MiniGame")
        {
            if (count >= 14)
            {
                ExecuteWin();
            }
        }
        // LEVEL 2 WIN CONDITION: Dynamic boxes (if any)
        else 
        {
            if (totalPickUps > 0 && count >= totalPickUps)
            {
                ExecuteWin();
            }
        }
    }

    public void ExecuteWin()
    {
        // 1. Show the Text
        if (winTextObject != null)
        {
            winTextObject.SetActive(true);
            var textComp = winTextObject.GetComponent<TMPro.TextMeshProUGUI>();
            if (textComp != null) textComp.text = "YOU WIN!";
        }

        // 2. Kill the Enemy
        GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");
        if (enemy != null) Destroy(enemy);

        // 3. Move to Level 2 or Stop
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MiniGame")
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Level2");
        }
        else
        {
            // If we are already in Level 2, just freeze the player
            rb.linearVelocity = Vector3.zero;
            this.enabled = false; 
        }
    }

    private void WinGame()
    {
        winTextObject.SetActive(true);
        winTextObject.GetComponent<TextMeshProUGUI>().text = "You Win!";
        
        // Stop the enemy if it exists
        GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");
        if (enemy != null)
        {
          Destroy(enemy);  
        }
        // Optional: Stay here or load a 'Credits' scene
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Instead of destroying the player immediately (which can break the camera), 
            // you might want to just stop movement and show lose text
            
            rb.linearVelocity = Vector3.zero; 
            rb.isKinematic = true; // Stop physics from moving the ball
            
            winTextObject.SetActive(true);
            winTextObject.GetComponent<TextMeshProUGUI>().text = "You Lose!";
            
            this.enabled = false; // Disable this script so player can't move
            
            // Optional: Reload the scene after 2 seconds
            Invoke("ReloadLevel", 2f);
        }
    }
    
    void ReloadLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}