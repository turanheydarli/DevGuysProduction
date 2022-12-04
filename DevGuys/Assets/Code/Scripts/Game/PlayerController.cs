using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using TMPro;
using UnityEngine;

namespace Code.Scripts.Game
{
    public class PlayerController : MonoBehaviourPunCallbacks
    {
        private Vector3 _direction;
        private bool _gameEnded;
        private Rigidbody _rigidbody;
        private Animator _animator;
        private Collider[] _colliders;
        private float _nextCollectTime;
        public PhotonView _photonView;

        public Vector3 onPath;

        [SerializeField] PlayerController[] _allPlayers;
        [SerializeField] public List<float> _leaderboard;

        [SerializeField] float turnSmoothVelocity;
        [SerializeField] float smoothTurnTime = 0.01f;
        [SerializeField] private float movementSpeed = 10;

        [SerializeField] public TMP_Text playerName;

        void Start()
        {
            _photonView = GetComponent<PhotonView>();
            _animator = GetComponent<Animator>();
            _rigidbody = GetComponent<Rigidbody>();
            
            _allPlayers = FindObjectsOfType<PlayerController>();

            playerName.text = _photonView.Owner.NickName;
           
        }

        private void Update()
        {
            onPath = transform.position;
            _leaderboard = _allPlayers.Select(m => m.onPath.z).ToList();
            _leaderboard.Sort();

            int position = _leaderboard.IndexOf(onPath.z) + 1;
            //Debug.Log(position);

            string suffix = position == 1 ? "st" : position == 2 ? "nd" : position == 3 ? "rd" : "th";
            GameUIManager.Instance.SetPositionInfo(position + suffix);
        }

        void FixedUpdate()
        {
            if (_photonView.IsMine && !_gameEnded)
            {
                Move();
            }
        }


        private void Move()
        {
            _direction = new Vector3(InputManager.Horizontal, 0, InputManager.Vertical);

            _animator.SetFloat($"Running", _direction.magnitude);

            if (_direction.magnitude > 0.01f)
            {
                float targetAngle = Mathf.Atan2(_direction.x, _direction.z) * Mathf.Rad2Deg;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity,
                    smoothTurnTime);

                transform.rotation = Quaternion.Euler(0, angle, 0);

                _rigidbody.MovePosition(_rigidbody.position + _direction * (movementSpeed * Time.deltaTime));
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Finish"))
            {
                _direction = new Vector3(0, 0, 0);
                _animator.SetFloat($"Running", _direction.magnitude);
                _gameEnded = true;

            }
        }
    }
}