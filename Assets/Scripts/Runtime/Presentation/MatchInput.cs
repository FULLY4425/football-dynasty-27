using UnityEngine;

namespace FootballDynasty
{
    public struct MatchInputFrame
    {
        public Vector3 Movement;
        public bool Sprint, Shoot, Pass, Switch, Pause, Slide;
    }

    public sealed class MatchInput : MonoBehaviour
    {
        [SerializeField]
        private MobileMatchUI mobile;
        public void Configure(MobileMatchUI ui) => mobile = ui;
        public MatchInputFrame Read(Camera camera)
        {
            var result = new MatchInputFrame();
            Vector2 move;
            if (mobile && mobile.IsVisible)
            {
                move = mobile.Joystick.Value;
                result.Shoot = mobile.Shoot.Consume();
                result.Pass = mobile.Pass.Consume();
                result.Switch = mobile.Switch.Consume();result.Slide=mobile.Slide.Consume();
                result.Sprint = move.sqrMagnitude > .85f * .85f;
            }
            else
            {
                move = new Vector2((Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1 : 0) - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1 : 0), (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1 : 0) - (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1 : 0));
                move = Vector2.ClampMagnitude(move, 1);
                result.Sprint = Input.GetKey(KeyCode.LeftShift);
                result.Shoot = Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.Space);
                result.Pass = Input.GetKeyDown(KeyCode.J);
                result.Switch = Input.GetKeyDown(KeyCode.L);
                result.Pause = Input.GetKeyDown(KeyCode.P);result.Slide=Input.GetKeyDown(KeyCode.Q);
            }

            // Both devices use camera-relative movement on the pitch plane.
            var forward = Vector3.ProjectOnPlane(camera.transform.forward, Vector3.up).normalized;
            var right = Vector3.Cross(Vector3.up, forward);
            result.Movement = (right * move.x + forward * move.y);
            return result;
        }
    }
}
