using UnityEngine;
using Unity.Cinemachine;
using Unity.Cinemachine.TargetTracking;

namespace FootballDynasty
{
    [DefaultExecutionOrder(-100)]
    public sealed class BroadcastCamera : MonoBehaviour
    {
        private FootballSimulator game;
        private Transform focus;
        private CinemachineCamera broadcast;
        private CinemachineBrain brain;
        public void Initialize(FootballSimulator owner)
        {
            game = owner;
            brain = game.MatchCamera.GetComponent<CinemachineBrain>();
            if (!brain)
                brain = game.MatchCamera.gameObject.AddComponent<CinemachineBrain>();
            focus = new GameObject("Broadcast focus").transform;
            focus.SetParent(transform, false);
            var rig = new GameObject("Cinemachine · Broadcast");
            rig.transform.SetParent(transform, false);
            broadcast = rig.AddComponent<CinemachineCamera>();
            broadcast.Follow = focus;
            broadcast.LookAt = focus;
            var follow = rig.AddComponent<CinemachineFollow>();
            follow.FollowOffset = new Vector3(0, 24, 36);
            follow.TrackerSettings.BindingMode = BindingMode.WorldSpace;
            follow.TrackerSettings.PositionDamping = new Vector3(.35f, .2f, .35f);
            rig.AddComponent<CinemachineHardLookAt>();
            var lens = broadcast.Lens;
            lens.FieldOfView = 50;
            lens.NearClipPlane = .1f;
            lens.FarClipPlane = 350;
            broadcast.Lens = lens;
        }

        public void SuspendForReplay(bool suspended)
        {
            if (brain)
                brain.enabled = !suspended;
        }

        void Update()
        {
            if (!game || !game.Ball || game.Replay.IsPlaying)
                return;
            var target = game.Ball.transform.position;
            if (game.Controlled)
                target = Vector3.Lerp(target, game.Controlled.transform.position, .18f);
            target.y = .3f;
            target.x = Mathf.Clamp(target.x, -36, 36);
            target.z = Mathf.Clamp(target.z * .5f, -12, 12);
            focus.position = target;
            var lens = broadcast.Lens;
            lens.FieldOfView = Mathf.Lerp(lens.FieldOfView, game.Running ? 50 : 45, 1 - Mathf.Exp(-Time.unscaledDeltaTime * 3));
            broadcast.Lens = lens;
        }
    }
}
