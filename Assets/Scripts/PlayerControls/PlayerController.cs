using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
using Unity.Collections;
using Unity.BossRoom.Infrastructure;
using UnityEngine.Scripting;
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
    private List<MeshRenderer> _meshesToDisable = new();

    [Space, SerializeField]
    private PlayerAnimator _playerAnimator;
    [SerializeField]
    private GameObject _playerModel;
    [SerializeField]
    private SkinnedMeshRenderer _playerMeshRenderer;
    [SerializeField]
    private Transform _playerSpine;

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
    [SerializeField]
    private float _jumpTimeOffset;
    [SerializeField]
    private float _footstepsTimeSpan;

    private float y;
    public float x { get { return y; } private set { y = value; } }

    [Space, SerializeField]
    private Camera _camera;
    [SerializeField]
    private AudioListener _listener;
    [SerializeField]
    private Canvas _worldCanvas;
    [SerializeField]
    private Image _playerNicknamePanel;
    [SerializeField]
    private TextMeshProUGUI _myNicknameText;
    [SerializeField]
    private float _cursorSens;

    [Space, SerializeField]
    private Material _winMaterial;
    [SerializeField]
    private GameObject _winParticles;

    private GameTimer _timer;

    private Vector3 movement;
    private float _clampedRotationOnXAxis;

    private float _timeWithoutMove;
    private float _timeInAir;
    private float _timeBetweenSteps;

    private bool _jumpPerformed;
    private bool _fireLock;
    private bool _inputLock;

    private NetworkVariable<bool> _allowFootsteps = new(false);
    private NetworkVariable<FixedString64Bytes> _playerNickname = new NetworkVariable<FixedString64Bytes>("not_loaded");
    private NetworkVariable<Vector3> _playerColor = new NetworkVariable<Vector3>(new Vector3(1, 1, 1));
    private NetworkVariable<float> _playerSpineRotation = new NetworkVariable<float>(0);

    public ulong myId { get; private set; }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        GameplayManager.instance.InitializeUI();
        NetworkManager.Singleton.OnClientConnectedCallback += WriteNickname;
        WriteNickname(1);

        if (!IsOwner) return;

        transform.position = new Vector3(0, 20, 0);

        _worldCanvas.gameObject.SetActive(false);

        _timer = FindObjectOfType<GameTimer>();

        myId = NetworkManager.LocalClientId;

        _playerModel.SetActive(false);
        _grabPoint.gameObject.SetActive(false);

        foreach(var mesh in _meshesToDisable)
        {
            mesh.enabled = true;
        }

        _physics.OnDeathEvent += CallRespawn;
        _physics.OnWinEvent += OnWinActions;

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

    [ServerRpc(RequireOwnership = false)]
    private void WriteNicknameServerRpc(FixedString64Bytes nickname, Vector3 color)
    {
        _playerNickname.Value = nickname;
        _playerColor.Value = color;
    }

    private void WriteNickname(ulong id)
    {
        if(IsOwner) WriteNicknameServerRpc(SessionData.instance.LocalPlayerNickname, SessionData.instance.LocalPlayerColor);
        _myNicknameText.text = _playerNickname.Value.ToString();
        _playerNicknamePanel.color = new Color(_playerColor.Value.x, _playerColor.Value.y, _playerColor.Value.z, 0.8f);
    }

    private void AuthenticateCamera()
    {
        _camera.enabled = IsOwner;
        _listener.enabled = IsOwner;
    }

    private void LateUpdate()
    {
        if (_myNicknameText.text == "not_loaded") WriteNickname(1);
        if (_allowFootsteps.Value)
        {
            _timeBetweenSteps += Time.deltaTime;
            if (_timeBetweenSteps > _footstepsTimeSpan)
            {
                _timeBetweenSteps = 0;
                Footsteps();
            }
        }

        SessionData.instance.RotateTowardsLocalPlayer(_worldCanvas.transform);

        LeanBodyWithCamera(_playerSpineRotation.Value);

        if (!IsOwner) return;

        if (_physics.IsGrounded() && _inputController.MoveVectorValue.magnitude > 0) AllowFootstepsServerRpc(true);
        else AllowFootstepsServerRpc(false);

        Look();
    }

    [ServerRpc]
    private void AllowFootstepsServerRpc(bool allow)
    {
        _allowFootsteps.Value = allow;
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
        if (_inputLock) moveVector = Vector3.zero;
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
            if(_timeInAir > _jumpTimeOffset) _jumpPerformed = false;
            if (_timeInAir > 0.6f) movement.y -= _fallSpeedPersSec * Time.deltaTime;
            movementSpeed = _airSpeed;
        }

        _physics.MoveRigidbody(movement * movementSpeed * Time.fixedDeltaTime);
    }

    private void Footsteps()
    {
         SFX[] sfxArray = new[]{ SFX.STEPS_1, SFX.STEPS_2 };
         SoundManager.instance.PlayRandomSFX(sfxArray, transform);
    }

    private void Jump()
    {
        if (_inputLock) return;

        if (_physics.IsGrounded()) _jumpPerformed = false;

        if (_jumpPerformed) return;

        if (!_physics.IsGrounded() && _timeInAir > _jumpTimeOffset) return;

        _jumpPerformed = true;
        _playerAnimator.SetTrigger(Animation.JUMP);
        _physics.SetVelocity(transform.up * _jumpHeight);
    }

    private void Fire()
    {
        if (!IsOwner) return;

        if (_inputLock) return;

        if (!_cursorLock.CursorLocked) return;

        if (_fireLock) return;

        if (_cannon) _cannon.Shot();
    }
    
    private void Look()
    {
        if (_inputLock) return;

        if (!_cursorLock.CursorLocked) return;

        var lookVector = _inputController.LookVectorValue;

        float CursorX = lookVector.x * _cursorSens * Time.deltaTime;
        float CursorY = lookVector.y * _cursorSens * Time.deltaTime;

        _clampedRotationOnXAxis -= CursorY;
        _clampedRotationOnXAxis = Mathf.Clamp(_clampedRotationOnXAxis, -90f, 60f);

        _camera.transform.localRotation = Quaternion.Euler(_clampedRotationOnXAxis, 0f, 0f);
        SetBodyLeanRotationServerRpc(_clampedRotationOnXAxis);
        transform.Rotate(Vector3.up * CursorX);
    }

    [ServerRpc]
    public void SetBodyLeanRotationServerRpc(float rot)
    {
        _playerSpineRotation.Value = rot;
    }

    private void LeanBodyWithCamera(float rot)
    {
        _playerSpine.transform.localRotation = Quaternion.Euler(-rot, 0f, 0f);
    }

    public void OnShotLoaded()
    {
        StartCoroutine(ZoomCameraForShot(_cannon.LoadTime, 65f));
        _footstepsTimeSpan *= 5f;
        LoadShotSoundServerRpc();
        _fireLock = true;
        _playerAnimator.SetTrigger(Animation.SHOOT);
        _speed *= 0.2f;
        //_cursorSens *= 0.01f;
    }

    [ServerRpc(RequireOwnership = false)]
    private void LoadShotSoundServerRpc()
    {
         LoadShotSoundClientRpc();
    }

    [ClientRpc]
    private void LoadShotSoundClientRpc()
    {
        SoundManager.instance.PlaySFX(SFX.LOADING_SHOT, transform);
    }

    public void OnShotPerformed()
    {
        StartCoroutine(ZoomCameraForShot(_cannon.LoadTime/4f, 75f));
        _fireLock = false;
        _physics.SetVelocity(-_camera.transform.forward * 10f);
        _footstepsTimeSpan *= 0.2f;
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
        GameplayManager.instance.DeathCamera.OnDeath(_camera);
        
        _inputLock = true;
        StartCoroutine(RespawnManager.instance.StartCounting((int)_respawnTime));
        yield return new WaitForSeconds(_respawnTime);
        ResetPhysics();
        _physics.SetKinematic(true);
        yield return new WaitForEndOfFrame();
        transform.position = RespawnManager.instance.GetRespawnPoint();
        yield return new WaitForEndOfFrame();
        _inputLock = false;
        _timer.ResetTimer();
        System.GC.Collect();
        GameplayManager.instance.DeathCamera.OnRespawn(_camera);
        _physics.SetKinematic(false);
        ResetPhysics();
    }

    private void ResetPhysics()
    {
        movement = Vector3.zero;
        _jumpPerformed = false;
        _timeInAir = 0;
        _physics.StopRigidbody();
    }

    public void OnWinActions()
    {
        _inputLock = true;

        foreach (var mesh in _meshesToDisable)
        {
            mesh.enabled = false;
        }

        ResetPhysics();

        OnGameFinishedServerRpc(_playerNickname.Value);

        StartCoroutine(WinActions());
    }

    private IEnumerator WinActions()
    {
        _playerAnimator.SetBool(Animation.CHARGE, true);

        yield return new WaitForSeconds(2);

        _playerModel.SetActive(true);

        OnWinCelebrationServerRpc();     

        yield return new WaitForSeconds(2);

        AferWinActionsServerRpc();

        _playerAnimator.SetBool(Animation.CHARGE, false);
    }

    [ClientRpc]
    public void OnGameFinishedClientRpc(FixedString64Bytes winnerNickname)
    {
        SoundManager.instance.ClearMusic();
        OnGameFinished(winnerNickname);
    }

    private void OnGameFinished(FixedString64Bytes winnerNickname)
    {
        //_playerAnimator.SetTrigger(Animation.CHARGE);
        _cursorLock.ForceCursorVisible();
        _grabPoint.gameObject.SetActive(false);
        GameplayManager.instance.TriggerWin(winnerNickname.ToString(), _timer.Minutes+":"+_timer.Seconds);
    }

    [ClientRpc]
    public void AferWinActionsClientRpc()
    {
        AferWinActions();
    }

    private void AferWinActions()
    {
        //_playerAnimator.SetTrigger(Animation.VICTORY);
        _timer.StopTimer();
        _winParticles.SetActive(true);
        _playerMeshRenderer.material = _winMaterial;
    }

    [ClientRpc]
    public void OnWinCelebrationClientRpc()
    {
        OnWinCelebration();
    }

    private void OnWinCelebration()
    {
        GameplayManager.instance.WinCamera.OnWin();
        SoundManager.instance.PlayMusic(Music.WIN_MUSIC_2);
        _camera.enabled = false;
    }

    [ServerRpc]
    public void OnGameFinishedServerRpc(FixedString64Bytes winnerNickname)
    {
        OnGameFinishedClientRpc(winnerNickname);
    }

    [ServerRpc]
    public void AferWinActionsServerRpc()
    {
        AferWinActionsClientRpc();
    }

    [ServerRpc]
    public void OnWinCelebrationServerRpc()
    {
        OnWinCelebrationClientRpc();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (!IsOwner) return;

        _cursorLock.ForceCursorVisible();
        SceneLoader.instance.ChangeScene(1);

        _inputController.OnFirePerformed -= Fire;
        _inputController.OnJumpPerformed -= Jump;

        _physics.OnDeathEvent -= CallRespawn;
        _physics.OnWinEvent -= OnWinActions;

        NetworkManager.Singleton.OnClientConnectedCallback -= WriteNickname;
        _cannon.OnShotPerformedEvent -= OnShotPerformed;
        _cannon.OnShotLoadedEvent -= OnShotLoaded;

        DOTween.KillAll();
        DestoryPlayerServerRpc();
    }

    [ServerRpc]
    public void DestoryPlayerServerRpc()
    {
        Destroy(this);
    }
}
