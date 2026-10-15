using System;
using System.Collections.Generic;
namespace FootballDynasty {
 [Serializable] public class Attributes {public int RIT,TIR,PAS,REG,DEF,FIS,REF,MAN;}
 [Serializable] public class Player { public string id,name,club,position,source;public int age,rating,potential,fitness,goals,contract;public double value,wage;public bool legend,youth;public Attributes stats; }
 [Serializable] public class Club {public string id,name,shortName,shortCode,color,altColor,league,country;public double budget;public bool historic,external;}
 [Serializable] public class League {public string id,name,country;public string[] clubs;}
 [Serializable] public class Database {public string season,provider;public Club[] clubs;public Player[] players;public League[] leagues;public string[] champions;}
 [Serializable] public class SavedTeam {public string club,formation;public string[] lineup;}
 [Serializable] public class SavedOwner {public string player,club;}
 [Serializable] public class SavedBudget {public string club;public double budget;}
 [Serializable] public class MatchResult {public string home,away,competition,winner;public int hg,ag,round;public bool penalties;}
 [Serializable] public class SavedRole {public string player;public TacticalRole role;}
 [Serializable] public class CareerSave {public SavedRole[] roles;public CoachProfile coach;public string managedClub;public int version=1,season=2026,round;public SavedTeam[] teams;public SavedOwner[] owners;public SavedBudget[] budgets;public MatchResult[] results;public Tournament[] tournaments;public int rng;}
 [Serializable] public class Tournament {public string id,name,stage,champion;public string[] participants,alive,direct;public int phase,leg;public List<MatchResult> games=new List<MatchResult>();public List<Tie> ties=new List<Tie>();}
 [Serializable] public class Tie {public string a,b;public int ga,gb;}
 public readonly struct TransferResult {public readonly bool Ok;public readonly string Message;public TransferResult(bool ok,string message){Ok=ok;Message=message;}}
}
