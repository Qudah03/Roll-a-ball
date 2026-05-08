## Multiplayer Prototype Phase

Decided to build out a multiplayer version of the game using Mirror Networking. To keep things clean, I created a static test arena separate from the procedural maze generator to figure out the network logic first.

**What I Did:**
* Set up a basic Server/Host and Client architecture using Mirror's `NetworkManager`.
* Wrote a custom `NetworkPlayerController` from scratch to replace the single-player spaghetti code.
* Synced physics and movement across multiple screens using `NetworkTransform` and `NetworkRigidbody`.
* Fixed the "Empty Build" and missing `NetworkIdentity` bugs to get two players successfully existing in the same scene.

**What I Learned (The Hard Way):**
* **The Golden Rule:** `if (!isLocalPlayer) return;` is the most important line of code in multiplayer. Without it, pressing 'W' moves every player on the screen.
* **Component Hijacking:** You have to manually disable the Camera and Input components for the "clones" that spawn on your screen, otherwise they take over your local view.
* **NetworkBehaviour:** Multiplayer scripts can't just be standard `MonoBehaviour`. They need to inherit from `NetworkBehaviour` to talk to the server, and they *must* be attached to an object with a `NetworkIdentity`.