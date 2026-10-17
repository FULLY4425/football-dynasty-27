using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FootballDynasty.Editor
{
    // Editor-only scenario exercising the actual scene, Rigidbody agents and shared tactical adapter.
    // Run during a match. It discards the current match without advancing/saving the career.
    [DefaultExecutionOrder(-500)]
    public sealed class TacticalPlayCheck : MonoBehaviour
    {
        private FootballSimulator game;
        private FootballAgent carrier, wing, creator;
        private readonly Dictionary<FootballAgent, TacticalRole> roles = new Dictionary<FootballAgent, TacticalRole>();
        private float elapsed;
        private bool failed, active;
        [MenuItem("Football Dynasty/Comprobar tácticas durante partido (10 s)")]
        static void Run()
        {
            var match = FindFirstObjectByType<FootballSimulator>();
            if (!Application.isPlaying || !match || !match.Running || match.Replay.IsPlaying || Time.timeScale == 0)
                throw new InvalidOperationException("Inicia un partido en Play, sin pausa ni repetición, antes de ejecutar esta prueba.");
            if (match.GetComponent<TacticalPlayCheck>())
                throw new InvalidOperationException("La prueba ya está en curso.");
            var test = match.gameObject.AddComponent<TacticalPlayCheck>();
            test.Begin(match);
        }

        void Begin(FootballSimulator match)
        {
            game = match;
            foreach (var a in game.Agents)
            {
                roles[a] = a.Role;
                if (a.Team == 0 && a.Data.position != "POR")
                {
                    if (!carrier)
                        carrier = a;
                    else if (!wing)
                        wing = a;
                    else if (!creator)
                        creator = a;
                }
            }

            foreach (var a in game.Agents)
                a.Role = TacticalRole.Portero;
            carrier.Role = TacticalRole.CazadorDeAreas;
            wing.Role = TacticalRole.ExtremoInvertido;
            creator.Role = TacticalRole.CreadorDeJuego;
            // Their authored formation anchors determine flank. Pick a player with negative flank for the winger.
            if (wing.FormationAnchor.z >= -4)
            {
                foreach (var a in game.Agents)
                    if (a.Team == 0 && a != carrier && a != creator && a.FormationAnchor.z < -4)
                    {
                        wing.Role = TacticalRole.Portero;
                        wing = a;
                        wing.Role = TacticalRole.ExtremoInvertido;
                        break;
                    }
            }

            wing.Body.position = new Vector3(10, .95f, -18);
            creator.Body.position = new Vector3(-10, .95f, 0);
            game.Controlled = carrier;
            active = true;
            Debug.Log("Prueba táctica iniciada: diagonal interior y apoyo corto. No guardará el partido.");
        }

        // Before the match snapshot: keep a stationary carrier on the opposite wing.
        void FixedUpdate()
        {
            if (!active)
                return;
            carrier.Body.position = new Vector3(20, .95f, 22);
            carrier.Body.linearVelocity = Vector3.zero;
            game.Ball.position = new Vector3(20, .15f, 22);
            game.Ball.linearVelocity = game.Ball.angularVelocity = Vector3.zero;
            elapsed += Time.fixedDeltaTime;
            if (elapsed > .2f && (wing.Decision.Action != TacticalAction.DiagonalInterior || creator.Decision.Action != TacticalAction.ApoyoCorto))
                failed = true;
            if (elapsed < 10)
                return;
            // The coordinated block no longer sends a winger to x=43 regardless of the ball.
            var wingTarget = new Vector3(wing.Decision.Target.X, wing.Body.position.y, wing.Decision.Target.Z);
            bool ok = !failed && wing.Body.position.x > 30 && Mathf.Abs(wing.Body.position.z) < 6 && Vector3.Distance(wing.Body.position, wingTarget) < 2 && Vector3.Distance(creator.Body.position, carrier.Body.position) < 12;
            game.Status = ok ? "Prueba táctica completada: diagonal y apoyo correctos." : "La prueba táctica falló: revisa la consola.";
            if (ok)
                Debug.Log(game.Status);
            else
                Debug.LogError(game.Status);
            game.Running = false;
            game.Finished = false;
            game.Screen = "career";
            Restore();
            Destroy(this);
        }

        void Restore()
        {
            active = false;
            foreach (var pair in roles)
                if (pair.Key)
                    pair.Key.Role = pair.Value;
            roles.Clear();
        }

        void OnDestroy() => Restore();
    }
}
