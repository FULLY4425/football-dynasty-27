using UnityEngine;

namespace FootballDynasty
{
    public sealed class TeamTactics : MonoBehaviour
    {
        private FootballSimulator game;
        private TacticalPlayer[] snapshot;private float decisionTimer;
        public readonly PossessionTracker Possession = new PossessionTracker();
        public void Initialize(FootballSimulator owner)
        {
            game = owner;
        }

        public void ResetPossession(){Possession.Reset();decisionTimer=0;}
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
            Possession.Update(snapshot, point, ball.y, game.Ball.linearVelocity.magnitude, dt);if(Possession.Carrier>=0)game.NotifyReception(game.Agents[Possession.Carrier],true);
            decisionTimer-=dt;if(decisionTimer>0)return;decisionTimer=.08f;
            for (int i = 0; i < count; i++) {
                var a=game.Agents[i];var level=a.Data.club==game.ClubId?MatchDifficulty.Medio:game.Career.Difficulty;
                if(Time.fixedTime-a.LastDecisionTime<DifficultySettings.Reaction(level))continue;
                a.LastDecisionTime=Time.fixedTime;
                a.Decision = CoordinatedTactics.Evaluate(i, snapshot, point, Possession,level,new PitchPoint(game.Ball.linearVelocity.x,game.Ball.linearVelocity.z));
                if(a==game.IntendedReceiver&&Time.fixedTime<game.ReceptionDeadline)a.Decision=new TacticalDecision{Target=new PitchPoint(Mathf.Clamp(ball.x+game.Ball.linearVelocity.x*.18f,-50,50),Mathf.Clamp(ball.z+game.Ball.linearVelocity.z*.18f,-32,32)),Action=TacticalAction.ApoyoCorto,Sprint=true};
            }
        }
    }
}
