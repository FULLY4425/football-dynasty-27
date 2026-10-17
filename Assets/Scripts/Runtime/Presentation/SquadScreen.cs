using System;
using System.Linq;
using UnityEngine;
namespace FootballDynasty {
 // IMGUI kept in a separate component; no licensed card artwork is embedded.
 public sealed class SquadScreen:MonoBehaviour {
  FootballSimulator game;int selected=-1,benchPage;bool formations;string club;
  GUIStyle heading,text,small,bold,button,center;readonly Color ink=new Color(.025f,.055f,.11f),panel=new Color(.055f,.12f,.20f),accent=new Color(.16f,.88f,.72f);
  public void Initialize(FootballSimulator owner){game=owner;}
  void Styles(){if(heading!=null)return;heading=Style(27,FontStyle.Bold);text=Style(17);small=Style(12);bold=Style(16,FontStyle.Bold);center=Style(17,FontStyle.Bold);center.alignment=TextAnchor.MiddleCenter;button=new GUIStyle(GUI.skin.button){fontSize=17,wordWrap=true};}
  static GUIStyle Style(int size,FontStyle weight=FontStyle.Normal){var s=new GUIStyle(GUI.skin.label){fontSize=size,fontStyle=weight,wordWrap=true};s.normal.textColor=Color.white;return s;}
  static void Fill(Rect r,Color color){var old=GUI.color;GUI.color=color;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
  void Label(Rect r,string value,GUIStyle style){GUI.Label(r,value,style);}
  bool Button(Rect r,string value)=>GUI.Button(r,value,button);
  Texture2D pitchTexture;
  void Pitch(Rect area){
   if(!pitchTexture){
    const int width=256,height=384;var pixels=new Color[width*height];
    for(int y=0;y<height;y++)for(int x=0;x<width;x++){
     var color=(y/38)%2==0?new Color(.06f,.29f,.19f):new Color(.05f,.25f,.17f);
     bool border=((x>=6&&x<=249)&&(Math.Abs(y-6)<2||Math.Abs(y-377)<2))||((y>=6&&y<=377)&&(Math.Abs(x-6)<2||Math.Abs(x-249)<2));
     bool half=Math.Abs(y-192)<1&&x>=6&&x<=249;
     float radius=(float)Math.Sqrt((x-128)*(x-128)+(y-192)*(y-192));bool circle=Math.Abs(radius-39)<1.2f;
     bool box=x>=66&&x<=190&&(Math.Abs(y-65)<2||Math.Abs(y-318)<2)||((Math.Abs(x-66)<2||Math.Abs(x-190)<2)&&(y>=6&&y<=65||y>=318&&y<=377));
     pixels[y*width+x]=border||half||circle||box?new Color(.63f,.80f,.72f):color;
    }
    pitchTexture=new Texture2D(width,height,TextureFormat.RGB24,false);pitchTexture.SetPixels(pixels);pitchTexture.Apply(false,true);
   }
   GUI.DrawTexture(area,pitchTexture,ScaleMode.StretchToFill);
  }
  void OnDestroy(){if(pitchTexture)Destroy(pitchTexture);}
  void Card(Rect r,Player p,bool active,bool dim,Action click){
   Fill(r,active?accent:p.legend?new Color(.85f,.64f,.25f):new Color(.32f,.53f,.64f));Fill(new Rect(r.x+3,r.y+3,r.width-6,r.height-6),p.legend?new Color(.26f,.20f,.12f):panel);
   var color=GUI.color;if(dim)GUI.color=new Color(1,1,1,.4f);
   Label(new Rect(r.x+7,r.y+3,34,24),p.rating.ToString(),bold);Label(new Rect(r.xMax-43,r.y+6,38,20),p.position,small);
   // Original initials badge until licensed portraits are supplied.
   string initials=string.Concat(p.name.Split(' ').Where(n=>n.Length>0).Take(2).Select(n=>n.Substring(0,1)));
   Fill(new Rect(r.center.x-19,r.y+25,38,30),new Color(.16f,.32f,.40f));Label(new Rect(r.center.x-19,r.y+25,38,30),initials,center);
   Label(new Rect(r.x+4,r.y+56,r.width-8,32),p.name,small);GUI.color=color;
   if(GUI.Button(r,GUIContent.none,GUIStyle.none))click();
  }
  void OnGUI(){
   if(game==null||game.Career==null||game.Screen!="squad"||game.Replay.IsPlaying)return;Styles();if(club!=game.ClubId){club=game.ClubId;selected=-1;benchPage=0;}
   // Portrait gives the bench its own lower panel, landscape places it on the right.
   bool portrait=UnityEngine.Screen.width<UnityEngine.Screen.height;float w=portrait?800:1440,h=portrait?1280:900;var old=GUI.matrix;float scale=Mathf.Min(UnityEngine.Screen.width/w,UnityEngine.Screen.height/h);Fill(new Rect(0,0,UnityEngine.Screen.width,UnityEngine.Screen.height),ink);GUI.matrix=Matrix4x4.TRS(new Vector3((UnityEngine.Screen.width-w*scale)/2,(UnityEngine.Screen.height-h*scale)/2,0),Quaternion.identity,Vector3.one*scale);
   bool wasEnabled=GUI.enabled;GUI.enabled=wasEnabled&&!formations;Fill(new Rect(0,0,w,h),ink);if(Button(new Rect(24,18,140,42),"← Carrera")){game.Screen="career";formations=false;}
   Label(new Rect(184,18,w-210,38),"ONCE INICIAL · "+game.Career.Clubs[club].name,heading);
   var team=game.Career.Team(club);if(Button(new Rect(24,72,230,44),"FORMACIÓN  "+team.formation))formations=true;
   Label(new Rect(275,75,w-300,40),"Elige un titular y después una carta para intercambiar.",text);
   Rect pitch=portrait?new Rect(24,132,752,650):new Rect(24,132,1000,652);
   Rect bench=portrait?new Rect(24,798,752,340):new Rect(1044,132,372,652);Pitch(pitch);Fill(bench,panel);
   Label(new Rect(bench.x+16,bench.y+12,bench.width-32,28),"SUPLENTES Y RESERVAS",bold);
   var slots=FormationLayout.Slots(team.formation);float cw=portrait?100:124,ch=100;
   for(int i=0;i<11;i++){int index=i;var p=game.Career.Players[team.lineup[i]];var slot=slots[i];Card(new Rect(pitch.x+slot.X*pitch.width-cw/2,pitch.y+slot.Y*(pitch.height-28)-ch/2,cw,ch),p,selected==i,false,()=>Select(index));}
   var reserves=game.Career.Squad(club).Where(p=>!team.lineup.Contains(p.id)).ToArray();int pageSize=portrait?10:8;int pages=Math.Max(1,(reserves.Length+pageSize-1)/pageSize);benchPage=Math.Min(benchPage,pages-1);
   float bw=portrait?128:145,bh=114;int columns=portrait?5:2;
   for(int i=0;i<pageSize&&benchPage*pageSize+i<reserves.Length;i++){var p=reserves[benchPage*pageSize+i];bool compatible=selected<0||p.position==game.Career.Players[team.lineup[selected]].position;float x=bench.x+16+(i%columns)*(bench.width-32)/columns;float y=bench.y+52+(i/columns)*(portrait?120:130);Card(new Rect(x,y,bw,bh),p,false,!compatible,()=>Replace(p));}
   if(Button(new Rect(bench.x+16,bench.yMax-46,65,32),"←"))benchPage=Math.Max(0,benchPage-1);Label(new Rect(bench.x+100,bench.yMax-46,bench.width-200,32),(benchPage+1)+" / "+pages,center);if(Button(new Rect(bench.xMax-81,bench.yMax-46,65,32),"→"))benchPage=Math.Min(pages-1,benchPage+1);
   float footer=portrait?1150:796;
   if(selected>=0){var player=game.Career.Players[team.lineup[selected]];Label(new Rect(24,footer,w-48,30),player.name+"   ·   "+player.stats.RIT+" RIT   "+player.stats.TIR+" TIR   "+player.stats.PAS+" PAS   "+player.stats.REG+" REG   "+player.stats.DEF+" DEF   "+player.stats.FIS+" FIS",text);
    if(Button(new Rect(24,footer+36,460,40),"DYNASTY IQ · "+game.Career.GetRole(player.id))){var roles=((TacticalRole[])Enum.GetValues(typeof(TacticalRole))).Where(r=>player.position=="POR"?r==TacticalRole.Portero:r!=TacticalRole.Portero).ToArray();game.Career.SetRole(club,player.id,roles[(Array.IndexOf(roles,game.Career.GetRole(player.id))+1)%roles.Length]);game.Save();}
   }else Label(new Rect(24,footer,w-48,35),"Selecciona una carta para ver sus estadísticas y su rol Dynasty IQ.",text);
   Label(new Rect(510,footer+38,w-540,40),game.Status,small);GUI.enabled=wasEnabled;if(formations)FormationPicker(w,h);GUI.matrix=old;
  }
  void Select(int index){if(selected>=0&&selected!=index){var team=game.Career.Team(club);if(game.Career.SetSlot(club,selected,team.lineup[index])){game.Save();selected=-1;return;}game.Status="El intercambio requiere jugadores de la misma posición.";}selected=index;}
  void Replace(Player p){if(selected<0){game.Status="Selecciona primero un titular.";return;}if(!game.Career.SetSlot(club,selected,p.id)){game.Status="Ese puesto requiere un jugador de la misma posición.";return;}game.Status=p.name+" entra en el once.";game.Save();selected=-1;}
  void FormationPicker(float w,float h){
   Fill(new Rect(0,0,w,h),new Color(0,0,0,.82f));var box=new Rect(24,130,w-48,h-260);Fill(box,panel);Label(new Rect(48,150,w-160,45),"ELIGE TU FORMACIÓN",heading);if(Button(new Rect(w-100,148,52,42),"X"))formations=false;
   var names=Career.Formations.Keys.ToArray();int columns=w<1000?3:5;float tileW=(w-120)/columns,tileH=w<1000?163:180;
   for(int i=0;i<names.Length;i++){string name=names[i];var r=new Rect(48+(i%columns)*(tileW+8),210+(i/columns)*(tileH+10),tileW,tileH);Fill(r,game.Career.Team(club).formation==name?accent:ink);var mini=new Rect(r.x+7,r.y+7,r.width-14,r.height-42);Pitch(mini);foreach(var s in FormationLayout.Slots(name))Fill(new Rect(mini.x+s.X*mini.width-4,mini.y+s.Y*mini.height-4,8,8),s.Position=="POR"?Color.cyan:s.Position=="DEF"?Color.green:s.Position=="MED"?Color.yellow:Color.red);Label(new Rect(r.x,r.yMax-34,r.width,30),name,center);
    if(GUI.Button(r,GUIContent.none,GUIStyle.none)){if(game.Career.SetFormation(club,name)){game.Save();selected=-1;formations=false;game.Status="Formación "+name+" guardada.";}else game.Status="Tu plantilla no tiene suficientes jugadores para esa formación.";}
   }
   Label(new Rect(48,h-100,w-96,35),"Se elige automáticamente un once válido. Después puedes personalizarlo.",small);
  }
 }
}
