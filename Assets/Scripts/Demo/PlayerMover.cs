using UnityEngine;
using UnityEngine.InputSystem;

namespace LearningScenarioBuilder.Demo
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMover : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Действие движения (InputSystem_Actions → Player/Move).")]
        private InputActionReference _moveAction;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Скорость движения в единицах мира в секунду.")]
        private float _speed = 5f;

        private Rigidbody2D _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            _moveAction.action.Enable();
        }

        private void OnDisable()
        {
            _moveAction.action.Disable();
        }

        private void FixedUpdate()
        {
            _rigidbody.linearVelocity = _moveAction.action.ReadValue<Vector2>() * _speed;
        }
    }
}
