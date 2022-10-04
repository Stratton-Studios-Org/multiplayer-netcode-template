using System;
using Unity.Netcode;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.EnhancedTouch;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;

public class PlayerController : NetworkBehaviour {
    [SerializeField] private float _speed = 3;
    private Rigidbody _rb;
    [SerializeField]private CharacterController m_CharacterController;
    
    public float m_speed = 3.0f;
    [SerializeField]
    private float m_RotSpeed = 5;


  public float m_movementX = 0;
   public float m_movementY = 0;

    /// <summary>
    /// other inputs
    /// </summary>
    
    [SerializeField]
    private Vector2 JoystickSize = new Vector2(300, 300);
    [SerializeField]
    private FloatingJoystick Joystick;

    private Finger MovementFinger;
    private Vector2 MovementAmount;
    NetworkManager _networkManager;
    [SerializeField] private RPCManager rpcManager;
    
    GameManager _gameManager;
    
    [SerializeField] public Animator _animator;

    bool init;
    private void Awake() {
        _rb = GetComponent<Rigidbody>();
        _networkManager = GameObject.FindObjectOfType<NetworkManager>();
    }


    void AddMyselfToRPCmanagerList()
    {
        if(init){return;}
        
        init = true;
      var f =   GetComponent<RPCManager>().CheckIfInList(_networkManager.LocalClientId);
        
    }
   

        void   UpdateServer()
        {
            Vector3 scaledMovement;
            
            if (IsOwner)
            {
                scaledMovement = m_speed * Time.fixedUnscaledDeltaTime * new Vector3(
                    MovementAmount.x,
                    0,
                    MovementAmount.y);
                //for anim handling
                AddMyselfToRPCmanagerList();

            }
            else
            {
                scaledMovement = m_speed * Time.fixedUnscaledDeltaTime * new Vector3(m_movementX, 0, m_movementY);
            }
            _animator.SetFloat("Movespeed", scaledMovement.magnitude);

            m_CharacterController.transform.LookAt(m_CharacterController.transform.position + scaledMovement, Vector3.up);
             m_CharacterController.SimpleMove(scaledMovement);
             
             
            
        
         }
        
        
        private void UpdateClient()
        {
            if(!IsOwner){return;}
         //   Debug.Log("UpdateClient");
            if(MovementAmount.x == 0  && MovementAmount.y == 0)
            {
                rpcManager.AnimationMovementResetServerRpc();
              // dont update input if we dont need to
                return;
            }
            Vector3 scaledMovement;
            scaledMovement = m_speed * Time.fixedUnscaledDeltaTime * new Vector3(
                MovementAmount.x,
                0,
                MovementAmount.y);
            _animator.SetFloat("Movespeed", scaledMovement.magnitude);
            rpcManager.PlayerInputServerRpc(MovementAmount.x, MovementAmount.y);
            
           // throw new NotImplementedException();
        }

        
        
    private void Update()
    {
        if (_gameManager == null)
        {
            _gameManager = GameObject.FindObjectOfType<GameManager>();
        }
        
            if(!_gameManager.HasGameStarted()){return;}
        
                if(IsServer)
                {
//                    Debug.Log(_networkManager.IsServer);
                    UpdateServer();
                }
                else
                {
                    UpdateClient();
                }
    }

  

    public override void OnNetworkSpawn() {
        if (!IsOwner) Destroy(this);
        
        EventManager.OnGameEnd += OnGameEnd;

        if (IsOwner && !IsServer)
        {
            if(_gameManager == null)
            {
                _gameManager = GameObject.FindObjectOfType<GameManager>();
            }
            
            _gameManager.OnPlayerSpawned(_networkManager.LocalClient.ClientId);
        }else if(IsServer)
        {
            if(_gameManager == null)
            {
                _gameManager = GameObject.FindObjectOfType<GameManager>();
            }
           
            _gameManager.OnPlayerSpawned(GetComponent<NetworkObject>().NetworkObjectId);
        }
        
    }

    void OnGameEnd()
    {
        
    }
    
    
    
    private void OnGUI()
    {
        GUIStyle labelStyle = new GUIStyle()
        {
            fontSize = 24,
            normal = new GUIStyleState()
            {
                textColor = Color.white
            }
        };
        if (MovementFinger != null)
        {
            GUI.Label(new Rect(10, 35, 500, 20), $"Finger Start Position: {MovementFinger.currentTouch.startScreenPosition}", labelStyle);
            GUI.Label(new Rect(10, 65, 500, 20), $"Finger Current Position: {MovementFinger.currentTouch.screenPosition}", labelStyle);
        }
        else
        {
            GUI.Label(new Rect(10, 35, 500, 20), "No Current Movement Touch", labelStyle);
        }

        GUI.Label(new Rect(10, 10, 500, 20), $"Screen Size ({Screen.width}, {Screen.height})", labelStyle);
    }
    
    
      private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        ETouch.Touch.onFingerDown += HandleFingerDown;
        ETouch.Touch.onFingerUp += HandleLoseFinger;
        ETouch.Touch.onFingerMove += HandleFingerMove;
    }

    private void OnDisable()
    {
        ETouch.Touch.onFingerDown -= HandleFingerDown;
        ETouch.Touch.onFingerUp -= HandleLoseFinger;
        ETouch.Touch.onFingerMove -= HandleFingerMove;
        EnhancedTouchSupport.Disable();
    }
    
    
    

    #region Input
    
    private void HandleFingerMove(Finger MovedFinger)
    {
        if (MovedFinger == MovementFinger)
        {
            Vector2 knobPosition;
            float maxMovement = JoystickSize.x / 2f;
            ETouch.Touch currentTouch = MovedFinger.currentTouch;

            if (Vector2.Distance(
                    currentTouch.screenPosition,
                    Joystick.RectTransform.anchoredPosition
                ) > maxMovement)
            {
                knobPosition = (
                    currentTouch.screenPosition - Joystick.RectTransform.anchoredPosition
                    ).normalized
                    * maxMovement;
            }
            else
            {
                knobPosition = currentTouch.screenPosition - Joystick.RectTransform.anchoredPosition;
            }

            Joystick.Knob.anchoredPosition = knobPosition;
            MovementAmount = knobPosition / maxMovement;
        }
    }

    private void HandleLoseFinger(Finger LostFinger)
    {
        if (LostFinger == MovementFinger)
        {
            MovementFinger = null;
            Joystick.Knob.anchoredPosition = Vector2.zero;
            Joystick.gameObject.SetActive(false);
            MovementAmount = Vector2.zero;
        }
    }

    private void HandleFingerDown(Finger TouchedFinger)
    {
        // if (MovementFinger == null && TouchedFinger.screenPosition.x <= Screen.width / 2f)
        // {
        if (MovementFinger == null){
            MovementFinger = TouchedFinger;
            MovementAmount = Vector2.zero;
            Joystick.gameObject.SetActive(true);
            Joystick.RectTransform.sizeDelta = JoystickSize;
            Joystick.RectTransform.anchoredPosition = ClampStartPosition(TouchedFinger.screenPosition);
        }
    }

    private Vector2 ClampStartPosition(Vector2 StartPosition)
    {
        if (StartPosition.x < JoystickSize.x / 2)
        {
            StartPosition.x = JoystickSize.x / 2;
        }

        if (StartPosition.y < JoystickSize.y / 2)
        {
            StartPosition.y = JoystickSize.y / 2;
        }
        else if (StartPosition.y > Screen.height - JoystickSize.y / 2)
        {
            StartPosition.y = Screen.height - JoystickSize.y / 2;
        }

        return StartPosition;
    }
    
    
    #endregion


    
}