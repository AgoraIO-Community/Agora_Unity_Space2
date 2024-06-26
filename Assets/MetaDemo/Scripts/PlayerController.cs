using UnityEngine;

namespace Agora.Spaces.Controller
{
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Rigidbody))]

    /*
    *   PlayerController is based on Mirror.Examples.AdditiveLevels.PlayerController
    * with addition to use JoyStick control.
    */
    public class PlayerController : MonoBehaviour
    {
        public enum GroundState : byte { Jumping, Falling, Grounded }

        [Header("Avatar Components")]
        public CharacterController characterController;

        [Header("Movement")]
        [Range(1, 20)]
        public float moveSpeedMultiplier = 8f;

        [Header("Turning")]
        [Range(1f, 200f)]
        public float maxTurnSpeed = 100f;
        [Range(.5f, 5f)]
        public float turnDelta = 3f;

        [Header("Jumping")]
        [Range(0.1f, 1f)]
        public float initialJumpSpeed = 0.2f;
        [Range(1f, 10f)]
        public float maxJumpSpeed = 5f;
        [Range(0.1f, 1f)]
        public float jumpDelta = 0.2f;

        [Header("Diagnostics - Do Not Modify")]
        public GroundState groundState = GroundState.Grounded;

        [Range(-1f, 1f)]
        public float horizontal;
        [Range(-1f, 1f)]
        public float vertical;

        [Range(-200f, 200f)]
        public float turnSpeed;

        [Range(-10f, 10f)]
        public float jumpSpeed;

        [Range(-1.5f, 1.5f)]
        public float animVelocity;

        [Range(-1.5f, 1.5f)]
        public float animRotation;

        public Vector3Int velocity;
        public Vector3 direction;

        VariableJoystick _variableJoystick;
        VariableJoystick _variableJoystick2;
        Rigidbody _rigidbody;

        void Awake()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();

            // Override CharacterController default values
            characterController.skinWidth = 0.02f;
            characterController.minMoveDistance = 0f;

            GetComponent<Rigidbody>().isKinematic = true;
        }

        //public override void OnStartAuthority()
        void Start()
        {
            characterController.enabled = true;
            SetupJoyStick();
        }

        void OnDisable()
        {
            characterController.enabled = false;
        }

        void SetupJoyStick()
        {
            _rigidbody = GetComponent<Rigidbody>();
            GameObject go = GameObject.Find("Joystick");
            if (go != null)
            {
                _variableJoystick = go.GetComponent<VariableJoystick>();
            }
            go = GameObject.Find("Joystick2");
            _variableJoystick2 = go?.GetComponent<VariableJoystick>();
        }

        void Update()
        {
            if (!characterController.enabled)
                return;

            HandleTurning();
            HandleJumping();
            HandleMove();

            // Reset ground state
            if (characterController.isGrounded)
                groundState = GroundState.Grounded;
            else if (groundState != GroundState.Jumping)
                groundState = GroundState.Falling;

            // Diagnostic velocity...FloorToInt for display purposes
            velocity = Vector3Int.FloorToInt(characterController.velocity);
        }

        bool _joyJumpOn = false;
        void FixedUpdate()
        {
            if (_variableJoystick != null)
            {
                Move(_variableJoystick.Horizontal, _variableJoystick.Vertical);
            }

            if (_variableJoystick2 != null)
            {
                //Debug.Log($"h={_variableJoystick2.Horizontal} v={_variableJoystick2.Vertical}");
                if (_variableJoystick2.Horizontal < 0.035f && _variableJoystick2.Vertical > 0.8f)
                {
                    _joyJumpOn = true;
                }
                else
                {
                    _joyJumpOn = false;
                    if (_variableJoystick2.Horizontal != 0 && _variableJoystick2.Vertical != 0)
                    {
                        turnSpeed = 1;

                        // rotate on y axis when moving joystick left or right.
                        transform.rotation = transform.rotation * Quaternion.AngleAxis(_variableJoystick2.Horizontal * turnSpeed, Vector3.up);
                    }
                }
            }
        }

        // TODO: Turning works while airborne...feature?
        void HandleTurning()
        {
            // Q and E cancel each other out, reducing the turn to zero.
            if (Input.GetKey(KeyCode.Q))
                turnSpeed = Mathf.MoveTowards(turnSpeed, -maxTurnSpeed, turnDelta);
            if (Input.GetKey(KeyCode.E))
                turnSpeed = Mathf.MoveTowards(turnSpeed, maxTurnSpeed, turnDelta);

            // If both pressed, reduce turning speed toward zero.
            if (Input.GetKey(KeyCode.Q) && Input.GetKey(KeyCode.E))
                turnSpeed = Mathf.MoveTowards(turnSpeed, 0, turnDelta);

            // If neither pressed, reduce turning speed toward zero.
            if (!Input.GetKey(KeyCode.Q) && !Input.GetKey(KeyCode.E))
                turnSpeed = Mathf.MoveTowards(turnSpeed, 0, turnDelta);

            transform.Rotate(0f, turnSpeed * Time.deltaTime, 0f);
        }

        void HandleJumping()
        {
            // Handle variable force jumping.
            // Jump starts with initial power on takeoff, and jumps higher / longer
            // as player holds spacebar. Jump power is increased by a diminishing amout
            // every frame until it reaches maxJumpSpeed, or player releases the spacebar,
            // and then changes to the falling state until it gets grounded.
            if (groundState != GroundState.Falling && (Input.GetKey(KeyCode.Space) || _joyJumpOn))
            {
                if (groundState != GroundState.Jumping)
                {
                    // Start jump at initial power.
                    groundState = GroundState.Jumping;
                    jumpSpeed = initialJumpSpeed;
                }
                else
                    // Jumping has already started...increase power toward maxJumpSpeed over time.
                    jumpSpeed = Mathf.MoveTowards(jumpSpeed, maxJumpSpeed, jumpDelta);

                // If power has reached maxJumpSpeed, change to falling until grounded.
                // This prevents over-applying jump power while already in the air.
                if (jumpSpeed == maxJumpSpeed)
                    groundState = GroundState.Falling;
            }
            else if (groundState != GroundState.Grounded)
            {
                // handles running off a cliff and/or player released Spacebar.
                groundState = GroundState.Falling;
                jumpSpeed = Mathf.Min(jumpSpeed, maxJumpSpeed);
                jumpSpeed += Physics.gravity.y * Time.deltaTime;
            }
            else
                jumpSpeed = Physics.gravity.y * Time.deltaTime;
        }

        // TODO: Directional input works while airborne...feature?
        void HandleMove()
        {
            // Capture inputs
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
            Move(horizontal, vertical);
        }

        void Move(float horizontal, float vertical)
        {
            // Create initial direction vector without jumpSpeed (y-axis).
            direction = new Vector3(horizontal, 0f, vertical);

            // Clamp so diagonal strafing isn't a speed advantage.
            direction = Vector3.ClampMagnitude(direction, 1f);

            // Transforms direction from local space to world space.
            direction = transform.TransformDirection(direction);

            // Multiply for desired ground speed.
            direction *= moveSpeedMultiplier;

            // Add jumpSpeed to direction as last step.
            direction.y = jumpSpeed;

            // Finally move the character.
            characterController.Move(direction * Time.deltaTime);
        }
    }
}

