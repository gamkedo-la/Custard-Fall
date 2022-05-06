using FishNet;
using FishNet.Object;
using FishNet.Object.Prediction;
using UnityEngine;

/*
* 
* See TransformPrediction.cs for more detailed notes.
* 
*/

namespace FishNet.Example.Prediction.CharacterControllers
{
    public class CharacterControllerPrediction : NetworkBehaviour
    {
        #region Types.

        public struct MoveData
        {
            public float Horizontal;
            public float Vertical;
        }

        public struct ReconcileData
        {
            public Vector3 Position;
            public Quaternion Rotation;

            public ReconcileData(Vector3 position, Quaternion rotation)
            {
                Position = position;
                Rotation = rotation;
            }
        }

        #endregion

        #region Serialized.

        [SerializeField] private float _moveRate = 5f;

        #endregion

        #region Private.

        private CharacterController _characterController;
        private MoveData _clientMoveData;

        #endregion

        private void Awake()
        {
            InstanceFinder.TimeManager.OnTick += TimeManager_OnTick;
            InstanceFinder.TimeManager.OnUpdate += TimeManager_OnUpdate;
            _characterController = GetComponent<CharacterController>();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            _characterController.enabled = IsServer || IsOwner;
        }

        private void OnDestroy()
        {
            if (InstanceFinder.TimeManager != null)
            {
                InstanceFinder.TimeManager.OnTick -= TimeManager_OnTick;
                InstanceFinder.TimeManager.OnUpdate -= TimeManager_OnUpdate;
            }
        }

        private void TimeManager_OnTick()
        {
            if (IsOwner)
            {
                Reconciliation(default, false);
                CheckInput(out var md);
                Move(md, false);
            }

            if (IsServer)
            {
                Move(default, true);
                var rd = new ReconcileData(transform.position, transform.rotation);
                Reconciliation(rd, true);
            }
        }


        private void TimeManager_OnUpdate()
        {
            if (IsOwner)
                MoveWithData(_clientMoveData, Time.deltaTime);
        }

        private void CheckInput(out MoveData md)
        {
            md = default;

            var horizontal = Input.GetAxisRaw("Horizontal");
            var vertical = Input.GetAxisRaw("Vertical");

            if (horizontal == 0f && vertical == 0f)
                return;

            md = new MoveData()
            {
                Horizontal = horizontal,
                Vertical = vertical
            };
        }

        [Replicate]
        private void Move(MoveData md, bool asServer, bool replaying = false)
        {
            if (asServer || replaying)
                MoveWithData(md, (float) TimeManager.TickDelta);
            else if (!asServer)
                _clientMoveData = md;
        }

        private void MoveWithData(MoveData md, float delta)
        {
            var move = new Vector3(md.Horizontal, Physics.gravity.y, md.Vertical);
            _characterController.Move(move * _moveRate * delta);
        }

        [Reconcile]
        private void Reconciliation(ReconcileData rd, bool asServer)
        {
            transform.position = rd.Position;
            transform.rotation = rd.Rotation;
        }
    }
}