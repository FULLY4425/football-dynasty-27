using UnityEngine;
namespace FootballDynasty {
 public static class FootballAppearance {
  public static Material BallMaterial(FootballSimulator game){
   var material=game.Material(Color.white);var texture=new Texture2D(256,128,TextureFormat.RGBA32,false);texture.wrapMode=TextureWrapMode.Repeat;texture.filterMode=FilterMode.Bilinear;
   float phi=(1+Mathf.Sqrt(5))/2;var centers=new[]{new Vector3(0,1,phi),new Vector3(0,-1,phi),new Vector3(0,1,-phi),new Vector3(0,-1,-phi),new Vector3(1,phi,0),new Vector3(-1,phi,0),new Vector3(1,-phi,0),new Vector3(-1,-phi,0),new Vector3(phi,0,1),new Vector3(phi,0,-1),new Vector3(-phi,0,1),new Vector3(-phi,0,-1)};
   for(int i=0;i<centers.Length;i++)centers[i].Normalize();
   for(int y=0;y<128;y++)for(int x=0;x<256;x++){
    float longitude=(x+.5f)/256*Mathf.PI*2,latitude=((y+.5f)/128-.5f)*Mathf.PI;var n=new Vector3(Mathf.Cos(latitude)*Mathf.Cos(longitude),Mathf.Sin(latitude),Mathf.Cos(latitude)*Mathf.Sin(longitude));
    var closest=centers[0];float dot=-1;foreach(var center in centers){float d=Vector3.Dot(n,center);if(d>dot){dot=d;closest=center;}}
    var tangent=Vector3.Cross(closest,Vector3.up).normalized;var bitangent=Vector3.Cross(closest,tangent);float a=Mathf.Atan2(Vector3.Dot(n,bitangent),Vector3.Dot(n,tangent));float sector=Mathf.Repeat(a+Mathf.PI/5,Mathf.PI*2/5)-Mathf.PI/5;float edge=.25f*Mathf.Cos(Mathf.PI/5)/Mathf.Cos(sector);float radius=Mathf.Acos(Mathf.Clamp(dot,-1,1));
    texture.SetPixel(x,y,radius<edge?new Color(.025f,.035f,.045f):radius<edge+.014f?new Color(.5f,.52f,.54f):new Color(.94f,.94f,.91f));
   }
   texture.Apply();if(material.HasProperty("_BaseMap"))material.SetTexture("_BaseMap",texture);if(material.HasProperty("_MainTex"))material.SetTexture("_MainTex",texture);material.SetFloat("_Smoothness",.32f);return material;
  }
 }
}
