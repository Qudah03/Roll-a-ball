// NetworkPlayerController.cs

using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkPlayerController : NetworkBehaviour
    {
        [Header("Components")]
        [SerializeField] private Rigidbody rbplayer;
        [SerializeField] private Camera playerCamera;
        
        [Header("Input Setup")]
        [SerializeField] private InputAction playerControls;
        [SerializeField] private InputAction jumpAction;

        [Header("Settings")]
        public float speed = 20f;
        public float jumpForce = 7f;
        
        private Vector2 _moveInput;
        private bool _isGrounded;


        // Mirror calls this only for the player you own. 
        // Not void start()
        // OnStartLocalPlayer() is a built-in Mirror method that only executes on the local player's computer.
        // It will never, ever run for the "enemy" player on your screen.
        // Use override because you are overriding Mirror's method.
        public override void OnStartLocalPlayer()
        {
            playerControls.Enable();
            jumpAction.Enable();
            
            // Enable the camera only for the local player
            if (playerCamera != null)
            {
                playerCamera.gameObject.SetActive(true);
                playerCamera.transform.SetParent(null); // Detach so it follows smoothly or stays put
            }
        }
        
        private void Start()
        {
            if (!isLocalPlayer) 
            {
                playerControls?.Disable();
                jumpAction?.Disable();

                if (playerCamera != null) 
                {
                    playerCamera.gameObject.SetActive(false); 
                }
            }
        }
        
        private void Update()
                {
                    // Safety check: only the owner of this ball can process input
                    if (!isLocalPlayer) return;
                    
                    _moveInput = playerControls.ReadValue<Vector2>().normalized;
                    // Debug.Log("Local Player Input: " + _moveInput);

                    // // Jump trigger
                    // if (jumpAction.triggered && _isGrounded)
                    // {
                    //     // tell the server you want to jumo
                    //     CmdJump();
                    // }

                    if (jumpAction.triggered && _isGrounded)
                        {
                            // Check if the jump button was pressed this frame
                            // AND make sure the player is currently touching the ground.
                            // This prevents infinite jumping in the air.

                            // Apply an instant upward physics force to the Rigidbody.
                            // Vector3.up = (0, 1, 0), meaning straight upward.
                            // jumpForce controls how strong the jump is.
                            // ForceMode.Impulse applies the force immediately like a real jump.

                            // Since we are using Mirror with NetworkTransform,
                            // the new player position will automatically sync
                            // to the server and all connected clients.
                            rbplayer.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                        }
                }
        
        void FixedUpdate()
        {
            if (!isLocalPlayer) return;

            // Apply movement physics locally
            Vector3 movement = new Vector3(_moveInput.x, 0, _moveInput.y) * speed;
            
            // if it is a bit laggy (delay between command and the UI move to Update())
            rbplayer.AddForce(movement); 

            // Ground check: check if ball is touching the floor
            _isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.6f);
        }
        

        // [Command] makes this code run on the SERVER, even though the client called it
        [Command]
        // void CmdJump()
        // {
        //     // The server applies the force to the ball
        //     // Since the NetworkTransform syncs position, everyone will see the jump
        //     rbplayer.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        // }
        
        private void OnDisable()
        {
            playerControls.Disable();
            jumpAction.Disable();
        }
    }