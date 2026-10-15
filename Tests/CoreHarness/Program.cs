using FootballDynasty;using System.Text.Json;
var options=new JsonSerializerOptions{IncludeFields=true};
var databasePath=Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"../../../../../Assets/Resources/database-2026.json"));
Database Fresh()=>JsonSerializer.Deserialize<Database>(File.ReadAllText(databasePath),options);
int passed=0;void Check(bool value,string message){if(!value)throw new Exception(message);passed++;Console.WriteLine("PASS "+message);}
var c=new Career(Fresh());Check(c.Data.clubs.Length==136&&c.Data.players.Length==4006,"136 clubs and 4006 players");
foreach(var l in c.Data.leagues){var schedule=Career.Schedule(l.clubs);Check(schedule.Count==(l.clubs.Length-1)*2,"full schedule "+l.name);Check(schedule.SelectMany(d=>d).Select(p=>p[0]+"/"+p[1]).Distinct().Count()==l.clubs.Length*(l.clubs.Length-1),"unique fixtures "+l.name);foreach(var day in schedule)if(day.SelectMany(p=>p).Distinct().Count()!=l.clubs.Length)throw new Exception("repeated club");}
foreach(var f in Career.Formations.Keys)Check(c.SetFormation("madrid",f)&&c.Valid("madrid",c.Team("madrid").lineup,f),"lineup "+f);
c.SetFormation("madrid","4-3-3");var ids=c.Team("madrid").lineup.ToArray();ids[1]=ids[2];Check(!c.Valid("madrid",ids,"4-3-3"),"duplicate lineup rejected");
var messi=c.Players["player-45843"];double seller=c.Clubs[messi.club].budget,buyer=c.Clubs["madrid"].budget,fee=messi.value,bonus=messi.wage*4;
Check(c.Buy(messi.id,"madrid","club-20232").Ok,"Messi bought from Inter Miami");Check(c.Clubs["club-20232"].budget==seller+fee&&c.Clubs["madrid"].budget==buyer-fee-bonus,"seller funds and buyer debit");Check(!c.Buy(messi.id,"madrid","club-20232").Ok,"old owner rejected");
var team=c.Team("madrid");int slot=Array.FindIndex(team.lineup,id=>c.Players[id].position=="DEL");Check(c.SetSlot("madrid",slot,messi.id),"purchased player joins XI");
c.Clubs["madrid"].budget=0;Check(!c.Buy("player-22774","madrid","club-817").Ok&&c.Players["player-22774"].club=="club-817","insufficient funds do not mutate ownership");c.Clubs["madrid"].budget=200000000;Check(c.Buy("player-22774","madrid","club-817").Ok,"Ronaldo bought from Al Nassr");var legend=c.Data.players.First(p=>p.name=="Zinedine Zidane");c.Clubs["madrid"].budget=200000000;Check(c.Buy(legend.id,"madrid",legend.club).Ok,"historical owner sells legend");
var europe=Career.EuropeanSchedule(c.Data.champions);foreach(var id in c.Data.champions)if(europe.Sum(d=>d.Count(p=>p[0]==id))!=4||europe.Sum(d=>d.Count(p=>p[1]==id))!=4)throw new Exception("home-away imbalance");Check(europe.SelectMany(d=>d).Select(p=>string.Join("/",p.OrderBy(s=>s))).Distinct().Count()==144,"Champions: unique opponents and four home/four away");
for(int i=0;i<40;i++)c.Advance();Check(c.Tournaments.All(t=>!string.IsNullOrEmpty(t.champion)),"eight tournament champions");Check(c.Tournaments.Single(t=>t.id=="champions").games.Count==189,"189 Champions fixtures");foreach(var l in c.Data.leagues){var rows=c.Table(l.clubs,c.Results.Where(m=>m.competition==l.id));Check(rows.All(r=>r.Played==(l.clubs.Length-1)*2)&&rows.Sum(r=>r.GF)==rows.Sum(r=>r.GA),"full coherent table "+l.name);}
var save=JsonSerializer.Serialize(c.Snapshot(),options);var loaded=new Career(Fresh());loaded.Restore(JsonSerializer.Deserialize<CareerSave>(save,options));Check(loaded.Round==40&&loaded.Results.Count==c.Results.Count&&loaded.Players[messi.id].club=="madrid","save roundtrip");
var played=new Career(Fresh());var fixture=played.NextLeague("spain").First(p=>p.Contains("madrid"));played.Advance(new MatchResult{home=fixture[0],away=fixture[1],hg=3,ag=1});Check(played.Results.Single(m=>m.home==fixture[0]&&m.away==fixture[1]).hg==3&&played.Round==1,"played score advances whole career");
string Serialize(Career career)=>JsonSerializer.Serialize(career.Snapshot(),options);
CareerSave CopySave(Career career)=>JsonSerializer.Deserialize<CareerSave>(Serialize(career),options);
void RejectWithoutMutation(Career career,Action<CareerSave> corrupt,string label){var before=Serialize(career);var invalid=CopySave(career);corrupt(invalid);bool rejected=false;try{career.Restore(invalid);}catch(ArgumentException){rejected=true;}Check(rejected&&Serialize(career)==before,label);}
RejectWithoutMutation(loaded,s=>s.teams[0].lineup[0]="missing-player","invalid lineup restore is atomic");
RejectWithoutMutation(loaded,s=>s.owners[0]=s.owners[1],"duplicate ownership rejected atomically");
RejectWithoutMutation(loaded,s=>s.budgets[0].budget=double.PositiveInfinity,"infinite budget rejected atomically");
RejectWithoutMutation(loaded,s=>s.results[0].home="missing-club","unknown fixture rejected atomically");
RejectWithoutMutation(loaded,s=>s.tournaments[0].alive=new[]{"missing-club"},"invalid tournament rejected atomically");
var seedBefore=Serialize(played);bool early=false;try{played.StartNextSeason();}catch(InvalidOperationException){early=true;}Check(early&&Serialize(played)==seedBefore,"early next season rejected without mutation");
var preservedOwner=loaded.Players[messi.id].club;double preservedFunds=loaded.Clubs["madrid"].budget;var preservedXI=loaded.Team("madrid").lineup.ToArray();loaded.StartNextSeason();
Check(loaded.Season==2027&&loaded.Round==0&&loaded.Results.Count==0&&loaded.Tournaments.All(t=>t.games.Count==0&&string.IsNullOrEmpty(t.champion)),"new season resets fixtures and tournaments");
Check(loaded.Players[messi.id].club==preservedOwner&&loaded.Clubs["madrid"].budget==preservedFunds&&loaded.Team("madrid").lineup.SequenceEqual(preservedXI),"new season preserves transfers funds and lineup");
var resumed=new Career(Fresh());resumed.Restore(CopySave(loaded));loaded.Advance();resumed.Advance();Check(Serialize(loaded)==Serialize(resumed),"save resumes deterministic simulation in next season");
for(int i=1;i<40;i++)loaded.Advance();Check(loaded.SeasonFinished&&loaded.Tournaments.All(t=>!string.IsNullOrEmpty(t.champion))&&loaded.Tournaments.Single(t=>t.id=="champions").games.Count==189,"second season completes all tournaments");

var actors=Enumerable.Range(0,22).Select(i=>new ReplayActor{id="actor-"+i,name="Player "+i,team=i<11?0:1,position=i%11==0?"POR":"DEL",color="#ffffff"}).ToArray();
ReplayFrame Frame(float time)=>new ReplayFrame{time=time,ball=new ReplayPose{x=time,y=.11f},players=Enumerable.Range(0,22).Select(i=>new ReplayPose{x=i,y=.9f,z=time}).ToArray()};
var buffer=new ReplayBuffer();for(int i=0;i<=1000;i++)buffer.Add(Frame(i*.02f));var clip=buffer.Export("Last play","Home","Away",actors);
Check(clip.Duration<=10.001f&&clip.Duration>9.97f&&buffer.Count<=502,"rolling replay buffer retains ten real seconds");
ReplayMath.Sample(clip,3.25f,out var left,out var right,out var blend);Check(Math.Abs(left.time+(right.time-left.time)*blend-(clip.frames[0].time+3.25f))<.0001f,"replay interpolation samples correct time");
ReplayMath.Sample(clip,999,out left,out right,out blend);Check(blend==1&&right.time==clip.frames.Last().time,"replay seek clamps to end");
ReplayMath.Sample(clip,-5,out left,out right,out blend);Check(blend==0&&left.time==clip.frames[0].time,"replay seek clamps to start");
buffer.Clear();actors[0].name="Changed";Check(clip.frames.Length>400&&clip.actors[0].name=="Player 0","goal clip survives buffer reset and roster changes");
var savedClip=JsonSerializer.Serialize(clip,options);var replayPath=Path.Combine(Path.GetTempPath(),Guid.NewGuid()+".fdreplay.json");try{File.WriteAllText(replayPath,savedClip);var reopened=JsonSerializer.Deserialize<ReplayClip>(File.ReadAllText(replayPath),options);ReplayMath.Validate(reopened);Check(reopened.frames.Length==clip.frames.Length&&reopened.actors[0].name==clip.actors[0].name&&reopened.frames.Last().ball.x==clip.frames.Last().ball.x,"saved replay file reopens with actors and exact poses");}finally{File.Delete(replayPath);}
void RejectReplay(Action<ReplayClip> corrupt,string label){var copy=JsonSerializer.Deserialize<ReplayClip>(savedClip,options);corrupt(copy);bool rejected=false;try{ReplayMath.Validate(copy);}catch(ArgumentException){rejected=true;}Check(rejected,label);}
RejectReplay(r=>r.frames[1].time=r.frames[0].time,"replay rejects unordered frames");
RejectReplay(r=>r.frames[0].ball.x=float.NaN,"replay rejects nonfinite positions");
RejectReplay(r=>r.frames[0].players=new ReplayPose[1],"replay rejects missing actors");
RejectReplay(r=>r.frames.Last().time+=20,"replay rejects excessive duration");
RejectReplay(r=>r.actors[0].id=r.actors[1].id,"replay rejects duplicate actor IDs");
Check(GoalRules.CrossedGoal(52,.5f,0,54,.5f,0)==1&&GoalRules.CrossedGoal(-52,.5f,0,-54,.5f,0)==-1,"full ball crossing detects both goals");
Check(GoalRules.CrossedGoal(52,.5f,0,52.59f,.5f,0)==0,"partial line crossing is not goal");
Check(GoalRules.CrossedGoal(52,3,0,54,3,0)==0&&GoalRules.CrossedGoal(52,1,4,54,1,4)==0,"shots above bar and outside posts rejected");
Check(GoalRules.CrossedGoal(52,1,0,60,1,20)==1,"fast shot uses crossing point rather than end position");
Check(GoalRules.CrossedGoal(54,1,0,52,1,0)==0,"returning ball cannot award repeated goal");
ControlMath.Analog(.05f,.05f,.12f,out var ax,out var ay);Check(ax==0&&ay==0,"joystick dead zone prevents drift");
ControlMath.Analog(1,1,.12f,out ax,out ay);Check(Math.Abs(ax*ax+ay*ay-1)<.00001f&&Math.Abs(ax-ay)<.00001f,"joystick diagonal is limited to unit speed");
ControlMath.Analog(.56f,0,.12f,out ax,out ay);Check(Math.Abs(ax-.5f)<.00001f&&ay==0,"joystick retains analog half-speed");
ControlMath.Analog(float.NaN,1,.12f,out ax,out ay);Check(ax==0&&ay==0,"joystick rejects nonfinite input");
ControlMath.Analog(-1,0,.12f,out ax,out ay);Check(ax==-1&&ay==0,"joystick preserves negative direction");

var coachCareer=new Career(Fresh());
CoachProfile Profile()=>new CoachProfile{firstName="  Alex  ",lastName="  Rivera  ",nickname="DT",gender="female",country="México",birthYear=1985,birthMonth=2,birthDay=3,avatar=2,outfit=2,glasses=true,tie=true,shirtColor=1,trousersColor=2,shoes=1};
var coach=Profile();Check(coach.Validate(2026)==null,"coach accepts complete identity and appearance");
coachCareer.AssignCoach(coach,"club-227");coach.firstName="Mutated";Check(coachCareer.Coach.DisplayName=="Alex Rivera"&&coachCareer.ManagedClub=="club-227","career stores trimmed independent coach and chosen club");
var coachSnapshot=CopySave(coachCareer);coachSnapshot.coach.lastName="Other";Check(coachCareer.Coach.lastName=="Rivera","snapshot coach is independent");
var coachReloaded=new Career(Fresh());coachReloaded.Restore(CopySave(coachCareer));Check(coachReloaded.Coach.DisplayName=="Alex Rivera"&&coachReloaded.Coach.avatar==2&&coachReloaded.Coach.glasses&&coachReloaded.Coach.tie&&coachReloaded.ManagedClub=="club-227","coach identity outfit accessories and club survive JSON save/load");
var badCoach=Profile();badCoach.birthMonth=2;badCoach.birthDay=30;Check(badCoach.Validate(2026)!=null,"coach rejects impossible birthday");
badCoach=Profile();badCoach.birthYear=2008;badCoach.birthMonth=7;badCoach.birthDay=2;Check(badCoach.Validate(2026)!=null,"coach must be adult at season start");badCoach.birthDay=1;Check(badCoach.Validate(2026)==null,"coach turning eighteen at season start accepted");
badCoach=Profile();badCoach.firstName=" ";Check(badCoach.Validate(2026)!=null,"coach requires a name");badCoach=Profile();badCoach.firstName="Alex\nInjected";Check(badCoach.Validate(2026)!=null,"coach rejects control characters");
badCoach=Profile();badCoach.outfit=20;Check(badCoach.Validate(2026)!=null,"coach rejects unknown outfit");badCoach=Profile();badCoach.gender="unknown";Check(badCoach.Validate(2026)!=null,"coach rejects unknown gender option");
RejectWithoutMutation(coachCareer,s=>s.managedClub="club-20232","coach restore rejects external club without changing career");RejectWithoutMutation(coachCareer,s=>s.coach.birthDay=35,"invalid coach restore is atomic");
var legacySave=CopySave(coachCareer);legacySave.coach=null;legacySave.managedClub="missing-club";var legacyCareer=new Career(Fresh());legacyCareer.Restore(legacySave);Check(legacyCareer.Coach==null&&legacyCareer.ManagedClub==null&&legacyCareer.Results.Count==coachCareer.Results.Count,"old saves enter onboarding without losing career progress");
var originalCoach=Serialize(coachCareer);bool externalRejected=false;try{coachCareer.AssignCoach(Profile(),"club-20232");}catch(ArgumentException){externalRejected=true;}Check(externalRejected&&Serialize(coachCareer)==originalCoach,"only clubs in the six playable leagues may be selected");
Check(coachCareer.Data.leagues.SelectMany(l=>l.clubs).Count()==114&&coachCareer.Data.leagues.All(l=>l.clubs.All(id=>coachCareer.Squad(id).Length>=11&&!string.IsNullOrWhiteSpace(ClubIdentity.Story(coachCareer.Clubs[id],l)))),"all 114 playable clubs have squad and identity text");
Check(ClubIdentity.Histories.Count==7&&ClubIdentity.Histories.All(h=>coachCareer.Clubs.ContainsKey(h.Key)&&h.Value.Founded>1800&&h.Value.Source.StartsWith("https://")),"seven sourced club histories use valid club IDs");
loaded.AssignCoach(Profile(),"club-227");loaded.StartNextSeason();Check(loaded.Coach.DisplayName=="Alex Rivera"&&loaded.ManagedClub=="club-227","coach and club persist into following season");
Console.WriteLine($"SUCCESS {passed} checks. Domain only; not Unity graphics or PhysX.");
