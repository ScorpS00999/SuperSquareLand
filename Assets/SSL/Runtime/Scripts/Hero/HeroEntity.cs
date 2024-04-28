using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class HeroEntity : MonoBehaviour
{
    #region Header

    [Header("Physics")]
    [SerializeField] private Rigidbody2D _rigidbody;

    [Header("Horizontal Movements")]
    [SerializeField] private HeroHorizontalMovementSettings _movementsSettings;
    private float _horizontalSpeed = 0f;
    private float _moveDirX = 0f;

    [Header("Dash Movements")]
    [SerializeField] private HeroDashSettings _dashSettings;
    [SerializeField] private float _dashTimer = 0f;
    public bool _isDashing = false;

    [Header("Orientation")]
    [SerializeField] private Transform _orientVisualRoot;
    private float _orientX = 1f;

    [Header("Debug")]
    [SerializeField] private bool _guiDebug = false;

    [Header("Vertical Movements")]
    private float _verticalSpeed = 0f;

    [Header("Fall")]
    [SerializeField] private HeroFallSettings _fallSettings;

    [Header("Ground")]
    [SerializeField] private GroundDetector _groundDetector;
    public bool IsTouchingGround { get; private set; } = false;

    [Header("Wall")]
    [SerializeField] private WallDetector _wallDetector;
    public bool IsTouchingWall { get; private set; } = false;

    [Header("Jump")]
    [SerializeField] private List<HeroJumpSettings> _jumpSettings;
    [SerializeField] private HeroFallSettings _jumpFallSettings;

    public int jumpCount = 0;
    public int jumpLenght;
    public bool CanJump = true;

    enum JumpState
    {
        NotJumping,
        JumpImpulsion,
        Falling
    }
    private JumpState _jumpState = JumpState.NotJumping;
    private float _jumpTimer = 0f;


    [Header("Horizontal Movements")]
    [FormerlySerializedAs("_movementsSettings")]
    [SerializeField] private HeroHorizontalMovementSettings _groundHorizontalMovementsSettings;
    [SerializeField] private HeroHorizontalMovementSettings _airHorizontalMovementsSettings;


    //Camera Follow
    private CameraFollowable _cameraFollowable;


    #endregion


    #region Awake, Update, FixedUpdate

    private void Awake()
    {
        _cameraFollowable = GetComponent<CameraFollowable>();
        _cameraFollowable.FollowPositionX = _rigidbody.position.x;
        _cameraFollowable.FollowPositionY = _rigidbody.position.y;

        jumpLenght = _jumpSettings.Count;
    }

    private void Update()
    {
        _UpdateOrientVisual();
    }

    private void FixedUpdate()
    {
        _ApplyGroundDetection();
        _ApplyWallDetection();

        _UpdateCameraFollowPosition();

        HeroHorizontalMovementSettings horizontalMovementsSettings = _GetCurrentHorizontalMovementsSettings();

        if (_AreOrientAndMovementOpposite())
        {
            _TurnBack(horizontalMovementsSettings);
        } else
        {
            _UpdateHorizontalSpeed(horizontalMovementsSettings);
            _ChangeOrientFromHorizontalMovement();
        }
        if (_isDashing)
        {
            DashMovement();
        }

        if (IsJumping)
        {
            _UpdateJump();
        } else
        {
            if (!IsTouchingGround)
            {
                _ApplyFallGravity(_fallSettings);
            }
            else
            {
                _ResetVerticalSpeed();
            }
        }


        _ApplyHorizontalSpeed();
        _ApplyVerticalSpeed();
    }

    #endregion


    #region Controle camera - suivre un object

    private void _UpdateCameraFollowPosition()
    {
        _cameraFollowable.FollowPositionX = _rigidbody.position.x;
        if (IsTouchingGround && !IsJumping)
        {
            _cameraFollowable.FollowPositionY = _rigidbody.position.y;
        }
    }

    #endregion


    #region Déplacements horizontaux

    public void SetMoveDirX(float dirX)
    {
        _moveDirX = dirX;
    }

    private void _ApplyHorizontalSpeed()
    {
        Vector2 velocity = _rigidbody.velocity;
        velocity.x = _horizontalSpeed * _orientX;
        _rigidbody.velocity = velocity;
    }

    #endregion


    #region Orientation personnage
    private void _ChangeOrientFromHorizontalMovement()
    {
        if (_moveDirX == 0f) return;
        _orientX = Mathf.Sign(_moveDirX);
    }

    private void _UpdateOrientVisual()
    {
        Vector3 newScale = _orientVisualRoot.localScale;
        newScale.x = _orientX;
        _orientVisualRoot.localScale = newScale;
    }

    #endregion


    #region Accélération / Décélération

    private void _Accelerate(HeroHorizontalMovementSettings settings)
    {
        _horizontalSpeed += settings.acceleration * Time.deltaTime;
        if (_horizontalSpeed > settings.speedMax)
        {
            _horizontalSpeed = settings.speedMax;
        }
    }

    private void _Decelerate(HeroHorizontalMovementSettings settings)
    {
        _horizontalSpeed -= settings.deceleration * Time.deltaTime;
        if (_horizontalSpeed < 0f)
        {
            _horizontalSpeed = 0f;
        }
    }

    private void _UpdateHorizontalSpeed(HeroHorizontalMovementSettings settings)
    {
        if (_moveDirX != 0f)
        {
            _Accelerate(settings);
        }
        else
        {
            _Decelerate(settings);
        }
    }

    #endregion


    #region Demi tour

    private void _TurnBack(HeroHorizontalMovementSettings settings)
    {
        _horizontalSpeed -= settings.turnBackFriction * Time.fixedDeltaTime;
        if (_horizontalSpeed < 0f)
        {
            _horizontalSpeed = 0f;
            _ChangeOrientFromHorizontalMovement();
        }
    }

    private bool _AreOrientAndMovementOpposite()
    {
        return _moveDirX * _orientX < 0f;
    }

    #endregion


    #region Chute, gravité et saut


    private void _ApplyVerticalSpeed()
    {
        Vector2 velocity = _rigidbody.velocity;
        velocity.y = _verticalSpeed;
        _rigidbody.velocity = velocity;
    }

    private void _ApplyFallGravity(HeroFallSettings settings)
    {
        _verticalSpeed -= settings.fallGravity * Time.fixedDeltaTime;
        if (_verticalSpeed < -settings.fallSpeedMax)
        {
            _verticalSpeed = -settings.fallSpeedMax;
        }
    }


    //Partie Saut
    public void JumpStart()
    {
        _jumpState = JumpState.JumpImpulsion;
        _jumpTimer = 0f;
    }
    public bool IsJumping => _jumpState != JumpState.NotJumping;

    private void _UpdateJumpStateImpulsion()
    {
        _jumpTimer += Time.fixedDeltaTime;
        if (_jumpTimer < _jumpSettings[jumpCount].jumpMaxDuration)
        {
            _verticalSpeed = _jumpSettings[jumpCount].jumpSpeed;
        }
        else
        {
            _jumpState = JumpState.Falling;
        }
    }

    private void _UpdateJumpStateFalling()
    {
        if (!IsTouchingGround)
        {
            _ApplyFallGravity(_jumpFallSettings);
        }
        else
        {
            _ResetVerticalSpeed();
            _jumpState = JumpState.NotJumping;
            jumpCount = 0;
            CanJump = true;
        }
    }

    private void _UpdateJump()
    {
        switch (_jumpState)
        {
            case JumpState.JumpImpulsion:
                _UpdateJumpStateImpulsion();
                break;

            case JumpState.Falling:
                _UpdateJumpStateFalling();
                break;
        }
    }

    #endregion


    #region Détection sol et mur

    private void _ApplyGroundDetection()
    {
        IsTouchingGround = _groundDetector.DetectGroundNearBy();
    }
    private void _ResetVerticalSpeed()
    {
        _verticalSpeed = 0f;
    }

    private void _ApplyWallDetection()
    {
        IsTouchingWall = _wallDetector.DetectWallNearBy();
    }

    #endregion


    #region Hauteur saut dynamique 

    public void StopJumpImpulsion()
    {
        _jumpState = JumpState.Falling;
    }

    public bool IsJumpImpulsing => _jumpState == JumpState.JumpImpulsion;

    public bool IsJumpMinDurationReached => _jumpTimer >= _jumpSettings[jumpCount].jumpMinDuration;

    #endregion


    #region Air control

    private HeroHorizontalMovementSettings _GetCurrentHorizontalMovementsSettings()
    {
        if (IsTouchingGround)
        {
            return _groundHorizontalMovementsSettings;
        }
        else
        {
            return _airHorizontalMovementsSettings;
        }
    }
    #endregion


    #region Dash

    public void dashStart()
    {
        _isDashing = true;
        _dashTimer = 0f;

    }

    public void DashMovement()
    {
        _dashTimer += Time.deltaTime;

        if (_dashTimer < _dashSettings.duration && !IsTouchingWall)
        {
            _horizontalSpeed = _dashSettings.speed;
            _verticalSpeed = 0f;
        }
        else
        {
            _horizontalSpeed = 0f;
            _isDashing = false;
        }
    }

    #endregion


    #region GUI

    private void OnGUI()
    {
        if (!_guiDebug) return;

        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label(gameObject.name);
        GUILayout.Label($"MoveDirX = {_moveDirX}");
        GUILayout.Label($"OrientX = {_orientX}");
        if (IsTouchingGround)
        {
            GUILayout.Label("OnGround");
        }
        else
        {
            GUILayout.Label("InAir");
        }
        GUILayout.Label($"JumpState = {_jumpState}");
        GUILayout.Label($"Horizontal Speed = {_horizontalSpeed}");
        GUILayout.Label($"Vertical Speed = {_verticalSpeed}");

        if (IsTouchingWall)
        {
            GUILayout.Label("mur");
        }
        else
        {
            GUILayout.Label("pas mur");
        }
        GUILayout.EndVertical();
    }

    #endregion
}