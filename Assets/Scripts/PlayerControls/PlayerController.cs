using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
using Unity.Collections;
using Unity.BossRoom.Infrastructure;
using DG.Tweening;

public class PlayerController : NetworkBehaviour
{
    [SerializeField]
    private PlayerPhysics _physics;
    [SerializeField]
    private TenisBallCannon _cannon;
    [SerializeField]
    private PlayerInputController _inputController;
    [SerializeField]
    private CursorLock _cursorLock;
    [SerializeField]
    private Transform _grabPoint;
    [SerializeField]
    private Transform _grabPointFps;

    [Space, SerializeField]
    private PlayerAnimator _playerAnimator;
    [SerializeField]
    private GameObject _playerModel;
    [SerializeField]
    private GameObject _cannonModel;
    [SerializeField]
    private List<MeshRenderer> _meshesToDisable = new();

    [Space, SerializeField]
    private float _speed;
    [SerializeField]
    private float _fallSpeedPersSec;
    [SerializeField]
    private float _respawnTime;
    [SerializeField]
    private float _airSpeed;
    [SerializeField]
    private float _jumpHeight;

    [Space, SerializeField]
    private Camera _camera;
    [SerializeField]
    private AudioListener _listener;
    [SerializeField]
    private Canvas _worldCanvas;
    [SerializeField]
    private TextMeshProUGUI _myNicknameText;
    [SerializeField]
    private float _cursorSens;

    private Vector3 movement;
    private float _clampedRotationOnXAxis;

    private float _timeWithoutMove;
    private float _timeInAir;
    private bool _fireLock;

    private DeathCamera _deathCam;

    private NetworkVariable<FixedString64Bytes> _playerNickname = new NetworkVariable<FixedString64Bytes>("not_loaded");

    public ulong myId { get; private set; }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            WriteNicknameServerRpc(SessionData.instance.LocalPlayerNickname);           
        }

        NetworkManager.Singleton.OnClientConnectedCallback += WriteNickname;
        WriteNickname(1);

        if (!IsOwner) return;

        _worldCanvas.gameObject.SetActive(false);

        base.OnNetworkSpawn();

        foreach (var mesh in _meshesToDisable)
        {
            mesh.enabled = true;
        }

        myId = NetworkManager.LocalClientId;

        _deathCam = FindObjectOfType<DeathCamera>();

        _playerModel.SetActive(false);
        _grabPoint.gameObject.SetActive(false);

        _physics.OnDeathEvent += CallRespawn;

        _inputController.InitializeInput();
        _inputController.OnJumpPerformed += Jump;
        _inputController.OnFirePerformed += Fire;

        _cursorLock.Initialize();

        SessionData.instance.AssignLocalPlayerTransform(transform);

        AuthenticateCamera();

        _cannon.Initialize();
        _cannon.OnShotPerformedEvent += OnShotPerformed;
        _cannon.OnShotLoadedEvent += OnShotLoaded;
    }

    [ServerRpc]
    private void WriteNicknameServerRpc(FixedString64Bytes nickname)
    {
        _playerNickname.Value = nickname;
    }

    private void WriteNickname(ulong id)
    {
        _myNicknameText.text = _playerNickname.Value.ToString();
    }

    private void AuthenticateCamera()
    {
        _camera.enabled = IsOwner;
        _listener.enabled = IsOwner;
    }

    private void LateUpdate()
    {
        SessionData.instance.RotateTowardsLocalPlayer(_worldCanvas.transform);

        if (!IsOwner) return;

        Look();
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        Move();
        _physics.OnSlope();
    }

    private void Move()
    {
        float movementSpeed;
        var moveVector = _inputController.MoveVectorValue;

        if(moveVector.magnitude == 0)
        {

            _timeWithoutMove += Time.deltaTime;
            if (_timeWithoutMove > 0.5f)
            {
                _playerAnimator.SetBool(Animation.IDLE, true);
                _playerAnimator.SetBool(Animation.RUN, false);
            }
        }
        else if(_physics.IsGrounded())
        {
            _timeWithoutMove = 0;
            _playerAnimator.SetBool(Animation.IDLE, false);
            _playerAnimator.SetBool(Animation.RUN, true);
        }

        if (_physics.IsGrounded())
        {
            movementSpeed = _speed;
            movement = (moveVector.z * transform.forward) + (moveVector.x * transform.right);
            _timeInAir = 0;
        }
        else
        {
            _timeInAir += Time.deltaTime;
            if (_timeInAir > 0.6f) movement.y -= _fallSpeedPersSec * Time.deltaTime;
            movementSpeed = _airSpeed;
        }

        _physics.MoveRigidbody(movement * movementSpeed * Time.fixedDeltaTime);
    }

    private void Jump()
    {
        if (!_physics.IsGrounded()) return;

        _playerAnimator.SetTrigger(Animation.JUMP);
        _physics.SetVelocity(transform.up * _jumpHeight);
    }

    private void Fire()
    {
        if (!_cursorLock.CursorLocked) return;

        if (_fireLock) return;

        if (!IsOwner) return;

        if (_cannon) _cannon.Shot();
    }
    
    private void Look()
    {
        if (!_cursorLock.CursorLocked) return;

        var lookVector = _inputController.LookVectorValue;

        float CursorX = lookVector.x * _cursorSens * Time.deltaTime;
        float CursorY = lookVector.y * _cursorSens * Time.deltaTime;

        _clampedRotationOnXAxis -= CursorY;
        _clampedRotationOnXAxis = Mathf.Clamp(_clampedRotationOnXAxis, -90f, 60f);

        _camera.transform.localRotation = Quaternion.Euler(_clampedRotationOnXAxis, 0f, 0f);
        _playerAnimator.LeanBodyWithCamera(_clampedRotationOnXAxis);
        transform.Rotate(Vector3.up * CursorX);
    }

    public void OnShotLoaded()
    {
        StartCoroutine(ZoomCameraForShot(_cannon.LoadTime, 65f));
        _fireLock = true;
        _speed *= 0.2f;
        //_cursorSens *= 0.01f;
    }

    public void OnShotPerformed()
    {
        StartCoroutine(ZoomCameraForShot(_cannon.LoadTime/4f, 75f));
        _fireLock = false;
        _physics.SetVelocity(-_camera.transform.forward * 10f);
        _speed *= 5f;
        //_cursorSens *= 100f;
    }

    private IEnumerator ZoomCameraForShot(float totalTime, float fov)
    {
        var startFov = _camera.fieldOfView;
        var finalFov = fov;
        var timeElasped = 0f;
        while(timeElasped <= totalTime)
        {            
            _camera.fieldOfView = Mathf.Lerp(startFov, finalFov, timeElasped / totalTime);
            timeElasped += Time.deltaTime;
            yield return null;
        }
    }

    private void CallRespawn()
    {
        StartCoroutine(WaitForRespawn());
    }

    private IEnumerator WaitForRespawn()
    {
        _deathCam.OnDeath(_camera);
        StartCoroutine(RespawnManager.instance.StartCounting((int)_respawnTime));
        yield return new WaitForSeconds(_respawnTime);
        ResetAfterRespawn();
        _physics.SetKinematic(true);
        yield return new WaitForEndOfFrame();
        transform.position = RespawnManager.instance.GetRespawnPoint();
        yield return new WaitForEndOfFrame();

        _deathCam.OnRespawn(_camera);
        _physics.SetKinematic(false);
        ResetAfterRespawn();
    }

    private void ResetAfterRespawn()
    {
        movement = Vector3.zero;
        _timeInAir = 0;
        _physics.StopRigidbody();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        _cursorLock.ForceCursorVisible();
        SceneLoader.instance.ChangeScene(1);

        _inputController.OnFirePerformed -= Fire;
        _inputController.OnJumpPerformed -= Jump;

        _physics.OnDeathEvent -= CallRespawn;
        NetworkManager.Singleton.OnClientConnectedCallback -= WriteNickname;
        _cannon.OnShotPerformedEvent -= OnShotPerformed;
        _cannon.OnShotLoadedEvent -= OnShotLoaded;
    }
}
