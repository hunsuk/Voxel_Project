using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerController : MonoBehaviour
{
    public CharacterController characterController;
    public Transform cameraTransform;
    private Transform playerTransform;
    public float speed = 6.0f;
    public float gravity = -9.81f;
    public float jumpHeight = 2.0f;

    public float flySpeed = 12.0f; // Speed when flying
    private bool isFlying = false; // Whether the player is currently flying

    private Vector3 playerVelocity;
    private bool groundedPlayer;

    void Start()
    {
        SetPlayerCenter();
        playerTransform = transform;
    }
    void Update()
    {
      
        // Toggle fly mode
        if (Input.GetKeyDown(KeyCode.F))
        {
            isFlying = !isFlying;
            characterController.enabled = !isFlying; // Disable the character controller when flying
        }

        if (isFlying)
        {
            Fly();
        }
        else
        {
            PlayerMove();
        }
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Debug.DrawRay(ray.origin, ray.direction * 5f, Color.red);
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Debug.DrawRay(ray.origin, ray.direction * 5f, Color.red);
            if (Physics.Raycast(ray, out hit, 5f))
            {
                Chunk targetChunk = World.Instance.GetChunkAt(hit.transform.position);
                Debug.Log(hit.point);
                Vector3 targetPos = new Vector3(hit.point.x - targetChunk.transform.position.x, hit.point.y - targetChunk.transform.position.y, hit.point.z - targetChunk.transform.position.z);
                targetChunk.HideVoxel((int)Mathf.Abs(targetPos.x), (int)Mathf.Abs(targetPos.y), (int)Mathf.Abs(targetPos.z));
            };
        }
    }
    float Euclidean(Vector3 p1, Vector3 p2)
    {
        return Mathf.Sqrt(Mathf.Pow(p1.x - p2.x, 2) + Mathf.Pow(p1.y - p2.y, 2) + Mathf.Pow(p1.z - p2.z ,2));
    }
    void SetPlayerCenter()
    {
        // Calculate the center position of the world
        int worldCenterIndex = World.Instance.worldSize / 2;
        float worldCenterX = worldCenterIndex * World.Instance.chunkSize;
        float worldCenterZ = worldCenterIndex * World.Instance.chunkSize;

        float noiseValue = GlobalNoise.GetGlobalNoiseValue(worldCenterX, worldCenterZ, World.Instance.noiseArray);

        // Normalize noise value to [0, 1]
        float normalizedNoiseValue = (noiseValue + 1) / 2;

        // Calculate maxHeight
        float maxHeight = normalizedNoiseValue * World.Instance.maxHeight;

        // Adjust the height for the player's position (assuming the player's capsule collider has a height of 2 units)
        maxHeight += 1.5f; // This ensures that the base of the player is at the terrain height

        // Set the player's position to be on top of the terrain
        transform.position = new Vector3(worldCenterX, maxHeight, worldCenterZ);
    }

    void PlayerMove()
    {
        groundedPlayer = characterController.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        move = cameraTransform.forward * move.z + cameraTransform.right * move.x;
        move.y = 0; // We do not want to move up/down by the camera's forward vector

        characterController.Move(move * Time.deltaTime * speed);

        // Changes the height position of the player..
        if (Input.GetButtonDown("Jump") && groundedPlayer)
        {
            Debug.Log("jump");
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravity);
        }

        playerVelocity.y += gravity * Time.deltaTime;
        characterController.Move(playerVelocity * Time.deltaTime);
        cameraTransform.position = transform.position;
        transform.rotation = cameraTransform.rotation;
    }

    void Fly()
    {
        // Get input for flying
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Jump") - Input.GetAxis("Ctrl"); // Space to go up, Crouch (Ctrl) to go down
        float z = Input.GetAxis("Vertical");

        Vector3 flyDirection = cameraTransform.right * x + cameraTransform.up * y + cameraTransform.forward * z;
        transform.position += flyDirection * flySpeed * Time.deltaTime;
        cameraTransform.position = transform.position;
        transform.rotation = cameraTransform.rotation;
    }

    public Vector3 getPlayerPosition()
    {
        return playerTransform.position; // Return the current position of the player.
    }
}
