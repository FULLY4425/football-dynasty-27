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
        private CinemachineBrain brain;private CinemachineFollow follow;
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
            follow = rig.AddComponent<CinemachineFollow>();
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
            if(game.IsCelebrating&&game.CelebrationLeader)target=game.CelebrationLeader.Body.position;
            follow.FollowOffset=Vector3.Lerp(follow.FollowOffset,game.IsCelebrating?new Vector3(0,4,8):new Vector3(0,24,36),1-Mathf.Exp(-Time.deltaTime*3));
            target.y = game.IsCelebrating?1:.3f;
            target.x = Mathf.Clamp(target.x, game.IsCelebrating?-50:-36, game.IsCelebrating?50:36);
            target.z = game.IsCelebrating?Mathf.Clamp(target.z,-31,31):Mathf.Clamp(target.z * .5f, -12, 12);
            focus.position = target;
            var lens = broadcast.Lens;
            lens.FieldOfView = Mathf.Lerp(lens.FieldOfView, game.IsCelebrating?45:game.Running ? 50 : 45, 1 - Mathf.Exp(-Time.unscaledDeltaTime * 3));
            broadcast.Lens = lens;
        }
    }
}
