using UnityEngine;

namespace FootballDynasty
{
    public sealed class TeamTactics : MonoBehaviour
    {
        private FootballSimulator game;
        private TacticalPlayer[] snapshot;
        public readonly PossessionTracker Possession = new PossessionTracker();
        public void Initialize(FootballSimulator owner)
        {
            game = owner;
        }

        public void ResetPossession() => Possession.Reset();
        // Called once, before all agents move. Every decision sees the same world snapshot.
        public void Tick(float dt)
        {
            if (!game.Ball)
                return;
            int count = game.Agents.Count;
            if (snapshot == null || snapshot.Length != count)
                snapshot = new TacticalPlayer[count];
            for (int i = 0; i < count; i++)
            {
                var a = game.Agents[i];
                var p = a.Body.position;
                var anchor = a.FormationAnchor;
                snapshot[i] = new TacticalPlayer
                {
                    Team = a.Team,
                    Role = a.Role,
                    Position = new PitchPoint(p.x, p.z),
                    Anchor = new PitchPoint(anchor.x, anchor.z)
                };
            }

            var ball = game.Ball.position;
            var point = new PitchPoint(ball.x, ball.z);
            Possession.Update(snapshot, point, ball.y, game.Ball.linearVelocity.magnitude, dt);
            for (int i = 0; i < count; i++)
                game.Agents[i].Decision = TacticalBrain.Evaluate(i, snapshot, point, Possession);
        }
    }
}
