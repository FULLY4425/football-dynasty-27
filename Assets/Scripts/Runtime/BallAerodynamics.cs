using UnityEngine;
namespace FootballDynasty {
 public sealed class BallAerodynamics:MonoBehaviour {
  private FootballSimulator game;private Rigidbody body;
  public void Initialize(FootballSimulator owner){game=owner;body=GetComponent<Rigidbody>();body.maxAngularVelocity=60;}
  void OnDestroy(){var renderer=GetComponent<Renderer>();if(!renderer)return;var material=renderer.sharedMaterial;if(!material)return;if(material.HasProperty("_BaseMap")&&material.GetTexture("_BaseMap"))Destroy(material.GetTexture("_BaseMap"));Destroy(material);}
  void FixedUpdate(){if(!game||!game.Running||game.Replay.IsPlaying)return;
   var velocity=body.linearVelocity;
   // Mild speed-dependent air drag and spin curvature. Tuning parameters, not CFD.
   body.AddForce(-velocity*velocity.magnitude*.0025f,ForceMode.Acceleration);
   body.AddForce(Vector3.ClampMagnitude(Vector3.Cross(body.angularVelocity,velocity)*.006f,3),ForceMode.Acceleration);
  }
 }
}
