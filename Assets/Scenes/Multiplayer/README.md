## Multiplayer Prototype Phase

Decided to build out a multiplayer version of the game using Mirror Networking. To keep things clean, I created a static test arena separate from the procedural maze generator to figure out the network logic first.

**What I Did:**

## Phase 1: Core Multiplayer Foundation
* Set up a basic Server/Host and Client architecture using Mirror's `NetworkManager`.
* Wrote a custom `NetworkPlayerController` from scratch to replace the single-player spaghetti code.
* Synced physics and movement across multiple screens using `NetworkTransform` and `NetworkRigidbody`.
* Fixed the "Empty Build" and missing `NetworkIdentity` bugs to get two players successfully existing in the same scene.

**What I Learned (The Hard Way):**
* **The Golden Rule:** `if (!isLocalPlayer) return;` is the most important line of code in multiplayer. Without it, pressing 'W' moves every player on the screen.
* **Component Hijacking:** You have to manually disable the Camera and Input components for the "clones" that spawn on your screen, otherwise they take over your local view.
* **NetworkBehaviour:** Multiplayer scripts can't just be standard `MonoBehaviour`. They need to inherit from `NetworkBehaviour` to talk to the server, and they *must* be attached to an object with a `NetworkIdentity`.

## Phase 2: Asymmetrical Spawning & Authority

The default Mirror setup only spawns identical prefabs. To fulfill the "Player vs Enemy" requirement, I had to hijack the server's spawning sequence so the Host plays as the Ball and the Client plays as the Cube.

**What I Did:**
* Created a `CustomNetworkManager` script overriding Mirror's `OnServerAddPlayer` method.
* Set up logic to spawn `Player_Ball` for `numPlayers == 0` (Host) and `Enemy_Cube` for the joining Client.
* Fixed the jump desync: The client originally couldn't jump because of a fight between Client Authority (NetworkTransform) and Server Authority (the `[Command]` jump script). Moved jump physics strictly to the local client to fix it.
* Bypassed Unity's Inspector safeguards to swap the default NetworkManager out without breaking the HUD.

**Current Known Issues Being Debugged:**
* **Network Physics Jitter:** The `Player_Ball` experiences micro-bouncing/stuttering while moving. I am currently testing Rigidbody Interpolation, Continuous Collision Detection, and Dead Physics materials to sync the 50hz FixedUpdate physics loop with the network tick rate.