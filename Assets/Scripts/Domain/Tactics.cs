using System;

namespace FootballDynasty
{
    public enum TacticalRole
    {
        CazadorDeAreas,
        ExtremoInvertido,
        CreadorDeJuego,
        CentralDeCobertura,
        MediocentroDefensivo,
        Portero
    }

    public enum GamePhase
    {
        Ofensivo,
        Defensivo,
        BalonDisputado
    }

    public enum TacticalAction
    {
        MantenerForma,
        DiagonalInterior,
        ApoyoCorto,
        AtacarArea,
        Cubrir,
        Presionar,
        Recuperar,
        Conducir,
        GuardarPorteria
    }

    public struct PitchPoint
    {
        public float X, Z;
        public PitchPoint(float x, float z)
        {
            X = x;
            Z = z;
        }

        public static float DistanceSquared(PitchPoint a, PitchPoint b)
        {
            float x = a.X - b.X, z = a.Z - b.Z;
            return x * x + z * z;
        }
    }

    public struct TacticalPlayer
    {
        public int Team;
        public TacticalRole Role;
        public PitchPoint Position, Anchor;
    }

    public struct TacticalDecision
    {
        public PitchPoint Target;
        public TacticalAction Action;
        public bool Sprint;
    }

    // One shared possession detector. A loose ball is not automatically owned by its nearest player.
    public sealed class PossessionTracker
    {
        public int Team { get; private set; } = -1;
        public int Carrier { get; private set; } = -1;

        private float grace;
        public void Reset()
        {
            Team = Carrier = -1;
            grace = 0;
        }

        public GamePhase PhaseFor(int team) => Team < 0 ? GamePhase.BalonDisputado : Team == team ? GamePhase.Ofensivo : GamePhase.Defensivo;
        public void Update(TacticalPlayer[] players, PitchPoint ball, float ballHeight, float ballSpeed, float dt)
        {
            int candidate = -1;
            float best = 1.4f * 1.4f;
            if (ballHeight < 1.6f)
                for (int i = 0; i < players.Length; i++)
                {
                    float distance = PitchPoint.DistanceSquared(players[i].Position, ball);
                    if (distance < best && (ballSpeed < 12 || distance < .65f * .65f))
                    {
                        best = distance;
                        candidate = i;
                    }
                }

            if (candidate >= 0)
            {
                Carrier = candidate;
                Team = players[candidate].Team;
                grace = .4f;
                return;
            }

            Carrier = -1;
            grace -= Math.Max(0, dt);
            if (grace <= 0)
                Team = -1;
        }
    }

    // Deterministic state machine: no Unity types, physics writes, random numbers or per-tick allocations.
    public static class TacticalBrain
    {
        static float Clamp(float x, float low, float high) => Math.Max(low, Math.Min(high, x));
        public static TacticalDecision Evaluate(int index, TacticalPlayer[] players, PitchPoint ball, PossessionTracker possession)
        {
            var self = players[index];
            float dir = self.Team == 0 ? 1 : -1;
            var decision = new TacticalDecision
            {
                Target = self.Anchor,
                Action = TacticalAction.MantenerForma
            };
            if (self.Role == TacticalRole.Portero)
            {
                decision.Target = new PitchPoint(-dir * 49, Clamp(ball.Z * .45f, -3.2f, 3.2f));
                decision.Action = TacticalAction.GuardarPorteria;
                return decision;
            }

            if (possession.Carrier == index)
            {
                decision.Target = new PitchPoint(dir * 49, self.Position.Z * .7f);
                decision.Action = TacticalAction.Conducir;
                return decision;
            }

            var phase = possession.PhaseFor(self.Team);
            // Exactly one outfield player per side recovers/presses; all others keep their roles.
            int nearest = -1;
            float best = float.MaxValue;
            for (int i = 0; i < players.Length; i++)
                if (players[i].Team == self.Team && players[i].Role != TacticalRole.Portero)
                {
                    float d = PitchPoint.DistanceSquared(players[i].Position, ball);
                    if (d < best)
                    {
                        best = d;
                        nearest = i;
                    }
                }

            if (phase != GamePhase.Ofensivo && nearest == index)
            {
                decision.Target = ball;
                decision.Action = phase == GamePhase.Defensivo ? TacticalAction.Presionar : TacticalAction.Recuperar;
                decision.Sprint = true;
                return decision;
            }

            if (phase == GamePhase.Ofensivo)
            {
                switch (self.Role)
                {
                    case TacticalRole.ExtremoInvertido:
                        if (Math.Abs(ball.Z) > 10 && Math.Abs(self.Anchor.Z) > 4 && ball.Z * self.Anchor.Z < 0)
                        {
                            decision.Target = new PitchPoint(dir * 43, Math.Sign(self.Anchor.Z) * 4);
                            decision.Action = TacticalAction.DiagonalInterior;
                            decision.Sprint = true;
                        }
                        else
                            decision.Target = new PitchPoint(dir * Clamp(ball.X * dir + 8, 8, 43), self.Anchor.Z);
                        break;
                    case TacticalRole.CreadorDeJuego:
                        var carrier = possession.Carrier >= 0 ? players[possession.Carrier].Position : ball;
                        float side = self.Anchor.Z >= 0 ? 1 : -1;
                        float supportWidth = Clamp(4 + Math.Abs(self.Anchor.Z) * .4f, 4, 8);
                        var first = new PitchPoint(carrier.X - dir * 7, carrier.Z + side * supportWidth);
                        var second = new PitchPoint(carrier.X - dir * 7, carrier.Z - side * supportWidth);
                        decision.Target = LaneRisk(carrier, first, self.Team, players) <= LaneRisk(carrier, second, self.Team, players) ? first : second;
                        decision.Action = TacticalAction.ApoyoCorto;
                        break;
                    case TacticalRole.CazadorDeAreas:
                        decision.Target = new PitchPoint(dir * (ball.X * dir > 8 ? 44 : Clamp(ball.X * dir + 15, 10, 36)), Clamp(self.Anchor.Z * .3f, -7, 7));
                        decision.Action = TacticalAction.AtacarArea;
                        break;
                    default:
                        decision.Target = new PitchPoint(dir * Clamp(ball.X * dir - 18, -32, 10), Clamp(self.Anchor.Z + ball.Z * .15f, -27, 27));
                        decision.Action = TacticalAction.Cubrir;
                        break;
                }
            }
            else if (self.Role == TacticalRole.CentralDeCobertura)
            {
                decision.Target = new PitchPoint(dir * Clamp(-30 + ball.X * dir * .15f, -44, -18), Clamp(self.Anchor.Z * .6f + ball.Z * .25f, -25, 25));
                decision.Action = TacticalAction.Cubrir;
            }
            else if (self.Role == TacticalRole.MediocentroDefensivo)
            {
                decision.Target = new PitchPoint(dir * Clamp(ball.X * dir - 10, -32, 8), Clamp(ball.Z * .55f, -24, 24));
                decision.Action = TacticalAction.Cubrir;
            }
            else
                decision.Target = new PitchPoint(self.Anchor.X - dir * 8, Clamp(self.Anchor.Z + ball.Z * .15f, -29, 29));
            decision.Target = new PitchPoint(Clamp(decision.Target.X, -50, 50), Clamp(decision.Target.Z, -32, 32));
            return decision;
        }

        static float LaneRisk(PitchPoint start, PitchPoint end, int team, TacticalPlayer[] players)
        {
            float risk = 0, dx = end.X - start.X, dz = end.Z - start.Z, length = dx * dx + dz * dz;
            foreach (var p in players)
                if (p.Team != team)
                {
                    float t = Clamp(((p.Position.X - start.X) * dx + (p.Position.Z - start.Z) * dz) / Math.Max(.01f, length), 0, 1);
                    var closest = new PitchPoint(start.X + dx * t, start.Z + dz * t);
                    if (PitchPoint.DistanceSquared(p.Position, closest) < 4)
                        risk++;
                }

            return risk;
        }
    }
}
