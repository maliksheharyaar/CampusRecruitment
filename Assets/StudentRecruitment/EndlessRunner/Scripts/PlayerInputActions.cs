using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace StudentRecruitment.EndlessRunner
{
    // Simplified wrapper for input actions
    public class PlayerInputActions : IDisposable
    {
        // Input action asset
        private InputActionAsset _asset;
        
        // Action maps
        private InputActionMap _playerMap;
        
        // Actions
        private InputAction _jumpAction;
        private InputAction _moveLeftAction;
        private InputAction _moveRightAction;
        private InputAction _pauseAction;
        
        public PlayerInputActions()
        {
            // Create the asset
            _asset = ScriptableObject.CreateInstance<InputActionAsset>();
            _asset.name = "PlayerInputActions";
            
            // Create player action map
            _playerMap = new InputActionMap("Player");
            _asset.AddActionMap(_playerMap);
            
            // Create actions
            _jumpAction = _playerMap.AddAction("Jump", InputActionType.Button);
            _jumpAction.AddBinding("<Keyboard>/space");
            _jumpAction.AddBinding("<Keyboard>/w");
            _jumpAction.AddBinding("<Keyboard>/upArrow");
            
            _moveLeftAction = _playerMap.AddAction("MoveLeft", InputActionType.Button);
            _moveLeftAction.AddBinding("<Keyboard>/a");
            _moveLeftAction.AddBinding("<Keyboard>/leftArrow");
            
            _moveRightAction = _playerMap.AddAction("MoveRight", InputActionType.Button);
            _moveRightAction.AddBinding("<Keyboard>/d");
            _moveRightAction.AddBinding("<Keyboard>/rightArrow");
            
            _pauseAction = _playerMap.AddAction("Pause", InputActionType.Button);
            _pauseAction.AddBinding("<Keyboard>/escape");
            _pauseAction.AddBinding("<Keyboard>/p");
        }
        
        // Public action accessors
        public InputAction JumpAction => _jumpAction;
        public InputAction MoveLeftAction => _moveLeftAction;
        public InputAction MoveRightAction => _moveRightAction;
        public InputAction PauseAction => _pauseAction;
        
        // Enable/disable input
        public void Enable() => _playerMap.Enable();
        
        public void Disable() => _playerMap.Disable();
        
        public void Dispose()
        {
            UnityEngine.Object.Destroy(_asset);
        }
        
        // Helper method to setup callbacks with the runner controller
        public IDisposable BindPlayerControls(RunnerController controller)
        {
            _jumpAction.performed += ctx => controller.OnJumpInput();
            _moveLeftAction.performed += ctx => controller.OnMoveLeftInput();
            _moveRightAction.performed += ctx => controller.OnMoveRightInput();
            _pauseAction.performed += ctx => controller.OnPauseInput();
            
            return new ActionDisposable(() => {
                _jumpAction.performed -= ctx => controller.OnJumpInput();
                _moveLeftAction.performed -= ctx => controller.OnMoveLeftInput();
                _moveRightAction.performed -= ctx => controller.OnMoveRightInput();
                _pauseAction.performed -= ctx => controller.OnPauseInput();
                return null;
            });
        }
    }
    
    // Simple disposable helper for actions
    public class ActionDisposable : IDisposable
    {
        private Func<IDisposable> _createDisposable;
        private IDisposable _disposable;
        
        public ActionDisposable(Func<IDisposable> createDisposable)
        {
            _createDisposable = createDisposable;
            _disposable = _createDisposable?.Invoke();
        }
        
        public void Dispose()
        {
            _disposable?.Dispose();
            _disposable = null;
        }
    }
} 