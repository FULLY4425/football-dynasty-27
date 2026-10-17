using System;
using System.Collections.Generic;
using System.Linq;
namespace FootballDynasty {
 // Normalized positions: goalkeeper at the bottom, attack at the top.
 // The same slots drive presentation and the initial match anchors.
 public readonly struct FormationSlot {
  public readonly string Position;public readonly float X,Y;
  public FormationSlot(string position,float x,float y){Position=position;X=x;Y=y;}
 }
 public static class FormationLayout {
  public static FormationSlot[] Slots(string formation){
   var rows=formation.Split('-').Select(int.Parse).ToArray();
   if(rows.Length<3||rows.Any(n=>n<1||n>5)||rows.Sum()!=10)throw new ArgumentException("Formación inválida.");
   var result=new List<FormationSlot>{new FormationSlot("POR",.5f,.91f)};
   for(int row=0;row<rows.Length;row++)for(int i=0;i<rows[row];i++)
    result.Add(new FormationSlot(row==0?"DEF":row==rows.Length-1?"DEL":"MED",.1f+.8f*(i+.5f)/rows[row],.72f-row*(.62f/(rows.Length-1))));
   return result.ToArray();
  }
  public static string[] Positions(string formation)=>Slots(formation).Select(s=>s.Position).ToArray();
 }
}
