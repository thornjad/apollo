using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;

namespace UnityStandardAssets.Characters.FirstPerson
{
  [RequireComponent(typeof (CharacterController))]
  [RequireComponent(typeof (AudioSource))]
  public class FirstPersonController : MonoBehaviour
  {
    [SerializeField] private bool IsWalking;
    [SerializeField] private float WalkSpeed;
    [SerializeField] private float RunSpeed;
    [SerializeField] [Range(0f, 1f)] private float RunStepModifier;
    [SerializeField] private float JumpSpeed;
    [SerializeField] private float StickToGroundForce;
    [SerializeField] private float GravityMultiplier;
    [SerializeField] private MouseLook MouseLook;
    [SerializeField] private bool UseFovKick;
    [SerializeField] private FOVKick FovKick = new FOVKick();
    [SerializeField] private float StepInterval;
    [SerializeField] private AudioClip[] FootstepSounds;
    [SerializeField] private AudioClip JumpSound;
    [SerializeField] private AudioClip LandSound;

    private Camera Camera;
    private bool Jump;
    private float YRotation;
    private Vector2 Input;
    private Vector3 MoveDirection = Vector3.zero;
    private CharacterController CharacterController;
    private CollisionFlags CollisionFlags;
    private bool WasPreviouslyGrounded;
    private Vector3 OriginalCameraPosition;
    private float StepCycle;
    private float NextStep;
    private bool IsJumping;
    private AudioSource AudioSource;
  
    private void Start()
    {
      CharacterController = GetComponent<CharacterController>();
      Camera = Camera.main;
      OriginalCameraPosition = Camera.transform.localPosition;
      FovKick.Setup(Camera);
      StepCycle = 0f;
      NextStep = StepCycle/2f;
      IsJumping = false;
      AudioSource = GetComponent<AudioSource>();
			    MouseLook.Init(transform , Camera.transform);
    }

    private void Update()
    {
      RotateView();
      // the jump state needs to read here to make sure it is not missed
      if (!Jump)
      {
        Jump = CrossPlatformInputManager.GetButtonDown("Jump");
      }

      if (!WasPreviouslyGrounded && CharacterController.isGrounded)
      {
        PlayLandingSound();
        MoveDirection.y = 0f;
        IsJumping = false;
      }
      if (!CharacterController.isGrounded && !IsJumping && WasPreviouslyGrounded)
      {
        MoveDirection.y = 0f;
      }

      WasPreviouslyGrounded = CharacterController.isGrounded;
    }


    private void PlayLandingSound()
    {
      AudioSource.clip = LandSound;
      AudioSource.Play();
      NextStep = StepCycle + .5f;
    }


    private void FixedUpdate()
    {
      float speed;
      GetInput(out speed);
      // always move along the camera forward as it is the direction that it being aimed at
      Vector3 desiredMove = transform.forward*Input.y + transform.right*Input.x;

      // get a normal for the surface that is being touched to move along it
      RaycastHit hitInfo;
      Physics.SphereCast(transform.position, CharacterController.radius, Vector3.down, out hitInfo,
                 CharacterController.height/2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
      desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

      MoveDirection.x = desiredMove.x*speed;
      MoveDirection.z = desiredMove.z*speed;


      if (CharacterController.isGrounded)
      {
        MoveDirection.y = -StickToGroundForce;

        if (Jump)
        {
          MoveDirection.y = JumpSpeed;
          PlayJumpSound();
          Jump = false;
          IsJumping = true;
        }
      }
      else
      {
        MoveDirection += Physics.gravity*GravityMultiplier*Time.fixedDeltaTime;
      }
      CollisionFlags = CharacterController.Move(MoveDirection*Time.fixedDeltaTime);

      ProgressStepCycle(speed);
      UpdateCameraPosition(speed);

      MouseLook.UpdateCursorLock();
    }


    private void PlayJumpSound()
    {
      AudioSource.clip = JumpSound;
      AudioSource.Play();
    }


    private void ProgressStepCycle(float speed)
    {
      if (CharacterController.velocity.sqrMagnitude > 0 && (Input.x != 0 || Input.y != 0))
      {
        StepCycle += (CharacterController.velocity.magnitude + (speed*(IsWalking ? 1f : RunStepModifier)))*
               Time.fixedDeltaTime;
      }

      if (!(StepCycle > NextStep))
      {
        return;
      }

      NextStep = StepCycle + StepInterval;

      PlayFootStepAudio();
    }


    private void PlayFootStepAudio()
    {
      if (CharacterController.isGrounded) {
        AudioSource.clip = GetRandomFootstepSound();
        AudioSource.PlayOneShot(AudioSource.clip);
      }
    }

    private AudioClip GetRandomFootstepSound() {
      int n = Random.Range(1, FootstepSounds.Length);
      AudioClip Sound = FootstepSounds[n];
      CycleFootstepSoundToFront(n);
      return Sound;
    }

    private void CycleFootstepSoundToFront(int ClipIndex) {
      AudioClip ClipToMove = FootstepSounds[ClipIndex];
      FootstepSounds[ClipIndex] = FootstepSounds[0];
      FootstepSounds[0] = ClipToMove;
    }

    private void UpdateCameraPosition(float speed)
    {
      Vector3 newCameraPosition;
      if (CharacterController.velocity.magnitude > 0 && CharacterController.isGrounded)
      {
        newCameraPosition = Camera.transform.localPosition;
        newCameraPosition.y = Camera.transform.localPosition.y;
      }
      else
      {
        newCameraPosition = Camera.transform.localPosition;
        newCameraPosition.y = OriginalCameraPosition.y;
      }
      Camera.transform.localPosition = newCameraPosition;
    }


    private void GetInput(out float speed)
    {
      // Read input
      float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
      float vertical = CrossPlatformInputManager.GetAxis("Vertical");

      bool waswalking = IsWalking;

#if !MOBILE_INPUT
    // On standalone builds, walk/run speed is modified by a key press.
    // keep track of whether or not the character is walking or running
    IsWalking = !UnityEngine.Input.GetKey(KeyCode.LeftShift);
#endif
      // set the desired speed to be walking or running
      speed = IsWalking ? WalkSpeed : RunSpeed;
      Input = new Vector2(horizontal, vertical);

      // normalize input if it exceeds 1 in combined length:
      if (Input.sqrMagnitude > 1)
      {
        Input.Normalize();
      }

      // handle speed change to give an fov kick
      // only if the player is going to a run, is running and the fovkick is to be used
      if (IsWalking != waswalking && UseFovKick && CharacterController.velocity.sqrMagnitude > 0)
      {
        StopAllCoroutines();
        StartCoroutine(!IsWalking ? FovKick.FOVKickUp() : FovKick.FOVKickDown());
      }
    }


    private void RotateView()
    {
      MouseLook.LookRotation (transform, Camera.transform);
    }


    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
      Rigidbody body = hit.collider.attachedRigidbody;
      //dont move the rigidbody if the character is on top of it
      if (CollisionFlags == CollisionFlags.Below)
      {
        return;
      }

      if (body == null || body.isKinematic)
      {
        return;
      }
      body.AddForceAtPosition(CharacterController.velocity*0.1f, hit.point, ForceMode.Impulse);
    }
  }
}
