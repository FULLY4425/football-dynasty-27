using FootballDynasty;using System.Text.Json;

var options=new JsonSerializerOptions{IncludeFields=true};

var databasePath=Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"../../../../../Assets/Resources/database-2026.json"));

Database Fresh()=>JsonSerializer.Deserialize<Database>(File.ReadAllText(databasePath),options);

int passed=0;void Check(bool value,string message){if(!value)throw new Exception(message);passed++;Console.WriteLine("PASS "+message);}

var runPose=PlayerMotion.Sample(.2f,8,7,PlayerAction.None,2,false,false,0,true);
Check(Math.Abs(runPose.leftLeg+runPose.rightLeg)<.001f&&Math.Abs(runPose.leftArm+runPose.rightArm)<.001f,"running alternates arms and legs");
Check(PlayerMotion.Sample(.2f,8,8,PlayerAction.None,2,false,false,0,true).leftLeg!=runPose.leftLeg,"individual animation phase varies");
foreach(var action in new[]{PlayerAction.Pass,PlayerAction.Shoot,PlayerAction.Receive,PlayerAction.Save,PlayerAction.ThrowIn}){var pose=PlayerMotion.Sample(0,0,1,action,.3f,false,false,0,true);Check(pose.rightLeg!=0||pose.spread!=0||pose.rightArm!=0,"action pose "+action);}
for(int seed=0;seed<3;seed++){var pose=PlayerMotion.Sample(0,0,seed,PlayerAction.None,2,false,true,.2f,true);Check(pose.spread>0||pose.leftArm<0,"celebration style "+seed);}
Check(PlayerMotion.Sample(0,0,1,PlayerAction.None,2,false,true,.2f,false).lean==12,"conceding team does not celebrate");
Check(PlayerMotion.Sample(0,0,1,PlayerAction.None,2,true,false,0,true).lean==65,"slide pose overrides locomotion");
Check(PlayerMotion.Sample(0,0,1,PlayerAction.Shoot,1,false,false,0,true).rightLeg==0,"kick pose ends after action");
Check(PlayerMotion.StableSeed("player-1")==PlayerMotion.StableSeed("player-1")&&PlayerMotion.StableSeed("player-1")!=PlayerMotion.StableSeed("player-2"),"player animation seed deterministic");
var animatedFrame=new ReplayFrame{time=0,ball=new ReplayPose(),players=new[]{new ReplayPose{leftArm=-160,spread=100,bob=.2f,lean=10,twist=15}}};var poseCopy=ReplayMath.Copy(animatedFrame);animatedFrame.players[0].leftArm=0;
Check(poseCopy.players[0].leftArm==-160&&poseCopy.players[0].spread==100&&poseCopy.players[0].bob==.2f,"replay deep copies full animation pose");
foreach(int side in new[]{-1,1}){
Check(GoalLineReviewRules.ShouldReview(side*52.2f,.3f,0,side*52.45f,.3f,0,.1f,false),"review ball stopped on line "+side);
Check(GoalLineReviewRules.ShouldReview(side*52.55f,.3f,0,side*52.2f,.3f,0,8,false),"review rebound from line "+side);
Check(GoalLineReviewRules.Clearance(side,side*52.55f)<0&&GoalLineReviewRules.Clearance(side,side*52.62f)>0,"whole ball beyond line required "+side);
Check(!GoalLineReviewRules.ShouldReview(side*52.4f,3,0,side*52.2f,3,0,8,false),"no review over crossbar "+side);
Check(!GoalLineReviewRules.ShouldReview(side*52.4f,.3f,6,side*52.2f,.3f,6,8,false),"no review wide shot "+side);
Check(!GoalLineReviewRules.ShouldReview(side*20,.3f,0,side*21,.3f,0,8,false),"no review normal play "+side);
Check(!GoalLineReviewRules.ShouldReview(side*52.2f,.3f,0,side*52.5f,.3f,0,20,false),"wait for outcome of incoming shot "+side);
}
Check(VarDecisions.Goal(true,true)=="GOL CONFIRMADO"&&VarDecisions.Goal(true,false)=="GOL ANULADO"&&VarDecisions.Goal(false,true)=="NO GOL CONFIRMADO","VAR goal decisions follow evidence");
Check(VarDecisions.Contact(true,true)=="PENAL CONFIRMADO"&&VarDecisions.Contact(true,false)=="FALTA CONFIRMADA"&&VarDecisions.Contact(false,true).StartsWith("SIN FALTA"),"VAR contact decisions follow first contact and area");
var varEvent=new VarIncident(VarIncidentKind.Penalty,"PENAL CONFIRMADO","Contacto anterior al balón",10);Check(varEvent.Available(10)&&varEvent.Available(22)&&!varEvent.Available(22.01f)&&!varEvent.Available(9),"VAR incident expires and cannot be reviewed before occurring");
foreach(int keeperTeam in new[]{0,1}){float x=keeperTeam==0?-48:48;
Check(KeeperCatchRules.CanCatch(keeperTeam,new PitchPoint(x,0),new PitchPoint(x+.2f,0),1,1,12,80,false),"keeper catches own area "+keeperTeam);
Check(!KeeperCatchRules.CanCatch(keeperTeam,new PitchPoint(0,0),new PitchPoint(0,0),1,1,12,80,false),"keeper cannot handle outside area "+keeperTeam);
Check(!KeeperCatchRules.CanCatch(keeperTeam,new PitchPoint(x,0),new PitchPoint(x,0),1,1,12,80,true),"keeper rejects direct keeperTeammate backpass "+keeperTeam);
Check(!KeeperCatchRules.CanCatch(keeperTeam,new PitchPoint(x,0),new PitchPoint(x,0),1,1,40,80,false)&&!KeeperCatchRules.CanCatch(keeperTeam,new PitchPoint(x,0),new PitchPoint(x,0),3,1,5,80,false),"keeper rejects unreachable or excessive speed "+keeperTeam);
}
var naturalPose=PlayerMotion.Sample(.9f,8,7,PlayerAction.None,2,false,false,0,true);Check(naturalPose.leftKnee>0||naturalPose.rightKnee>0,"running bends knee in recovery phase");Check(naturalPose.leftElbow<0&&naturalPose.rightElbow<0&&naturalPose.twist!=0,"running bends elbows and rotates torso");
var c=new Career(Fresh());Check(c.Data.clubs.Length==136&&c.Data.players.Length==4006,"136 clubs and 4006 players");

foreach(var l in c.Data.leagues){var schedule=Career.Schedule(l.clubs);Check(schedule.Count==(l.clubs.Length-1)*2,"full schedule "+l.name);Check(schedule.SelectMany(d=>d).Select(p=>p[0]+"/"+p[1]).Distinct().Count()==l.clubs.Length*(l.clubs.Length-1),"unique fixtures "+l.name);foreach(var day in schedule)if(day.SelectMany(p=>p).Distinct().Count()!=l.clubs.Length)throw new Exception("repeated club");}

foreach(var f in Career.Formations.Keys)Check(c.SetFormation("madrid",f)&&c.Valid("madrid",c.Team("madrid").lineup,f),"lineup "+f);

// Formation rows must preserve all eleven slots without visual collisions.
foreach(var f in Career.Formations.Keys){var layout=FormationLayout.Slots(f);Check(layout.Length==11&&layout[0].Position=="POR"&&layout.Select(q=>q.Position).SequenceEqual(Career.Formations[f]),"layout roles "+f);Check(layout.All(q=>q.X>0&&q.X<1&&q.Y>0&&q.Y<1)&&layout.Select(q=>(q.X,q.Y)).Distinct().Count()==11,"unique bounded slots "+f);}
var diamond=FormationLayout.Slots("4-3-1-2");Check(diamond.Count(q=>q.Position=="MED")==4&&diamond.Where(q=>q.Position=="MED").Select(q=>q.Y).Distinct().Count()==2,"diamond separates three midfielders and playmaker");
c.SetFormation("madrid","4-3-1-2");var layoutSave=c.Snapshot();var layoutRestored=new Career(Fresh());layoutRestored.Restore(layoutSave);Check(layoutRestored.Team("madrid").formation=="4-3-1-2"&&layoutRestored.Team("madrid").lineup.SequenceEqual(c.Team("madrid").lineup),"diamond formation and lineup survive save restore");
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


TacticalPlayer TP(int team,TacticalRole role,float x,float z,float? az=null)=>new TacticalPlayer{Team=team,Role=role,Position=new PitchPoint(x,z),Anchor=new PitchPoint(x,az??z)};
var actorsT=new[]{TP(0,TacticalRole.CazadorDeAreas,20,22),TP(0,TacticalRole.ExtremoInvertido,10,-18),TP(0,TacticalRole.CreadorDeJuego,0,0),TP(1,TacticalRole.CentralDeCobertura,38,0),TP(1,TacticalRole.MediocentroDefensivo,32,10)};
var possession=new PossessionTracker();var tacticalBall=new PitchPoint(20,22);possession.Update(actorsT,tacticalBall,.15f,2,.02f);
Check(possession.Carrier==0&&possession.PhaseFor(0)==GamePhase.Ofensivo&&possession.PhaseFor(1)==GamePhase.Defensivo,"possession gives opposite phases to both teams");
var wing=TacticalBrain.Evaluate(1,actorsT,tacticalBall,possession);Check(wing.Action==TacticalAction.DiagonalInterior&&wing.Target.X>actorsT[1].Position.X&&Math.Abs(wing.Target.Z)<Math.Abs(actorsT[1].Position.Z),"opposite-flank inverted winger attacks diagonally into box");
var creator=TacticalBrain.Evaluate(2,actorsT,tacticalBall,possession);Check(creator.Action==TacticalAction.ApoyoCorto&&PitchPoint.DistanceSquared(creator.Target,actorsT[0].Position)>25&&PitchPoint.DistanceSquared(creator.Target,actorsT[0].Position)<144,"playmaker offers short pass without standing on carrier");
actorsT[1].Anchor=new PitchPoint(10,18);Check(TacticalBrain.Evaluate(1,actorsT,tacticalBall,possession).Action!=TacticalAction.DiagonalInterior,"same-side winger does not trigger opposite-side diagonal");
var beforeBall=new PitchPoint(tacticalBall.X,tacticalBall.Z);TacticalBrain.Evaluate(1,actorsT,tacticalBall,possession);Check(tacticalBall.X==beforeBall.X&&tacticalBall.Z==beforeBall.Z,"tactical decision does not mutate world snapshot");
var defensive=TacticalBrain.Evaluate(3,actorsT,tacticalBall,possession);Check(defensive.Action==TacticalAction.Cubrir&&defensive.Target.X>0,"cover defender protects own goal while teammate presses");
var mirrored=actorsT.Select(p=>new TacticalPlayer{Team=1-p.Team,Role=p.Role,Position=new PitchPoint(-p.Position.X,p.Position.Z),Anchor=new PitchPoint(-p.Anchor.X,p.Anchor.Z)}).ToArray();mirrored[1].Anchor=new PitchPoint(-10,-18);var mirroredPossession=new PossessionTracker();mirroredPossession.Update(mirrored,new PitchPoint(-20,22),.15f,2,.02f);var mirroredWing=TacticalBrain.Evaluate(1,mirrored,new PitchPoint(-20,22),mirroredPossession);Check(mirroredWing.Action==TacticalAction.DiagonalInterior&&mirroredWing.Target.X==-wing.Target.X&&mirroredWing.Target.Z==wing.Target.Z,"away-team tactics mirror attack direction correctly");
possession.Update(actorsT,new PitchPoint(-40,-30),3,25,.1f);Check(possession.Carrier==-1&&possession.PhaseFor(0)==GamePhase.Ofensivo,"short loose-ball grace avoids immediate possession flicker");possession.Update(actorsT,new PitchPoint(-40,-30),3,25,.5f);Check(possession.Team==-1&&possession.PhaseFor(0)==GamePhase.BalonDisputado,"long aerial ball is not assigned to nearest player");
Check(Enumerable.Range(0,actorsT.Length).Count(i=>actorsT[i].Team==0&&TacticalBrain.Evaluate(i,actorsT,new PitchPoint(-40,-30),possession).Action==TacticalAction.Recuperar)==1,"only one outfield player per team chases a loose ball");
possession.Reset();Check(possession.Team==-1&&possession.Carrier==-1,"kickoff resets possession state");
var tacticalCareer=new Career(Fresh());var rolePlayer=tacticalCareer.Team("madrid").lineup.First(id=>tacticalCareer.Players[id].position=="MED");Check(tacticalCareer.SetRole("madrid",rolePlayer,TacticalRole.CreadorDeJuego),"assigned tactical role accepted for owned player");Check(!tacticalCareer.SetRole("barcelona",rolePlayer,TacticalRole.CazadorDeAreas)&&!tacticalCareer.SetRole("madrid",rolePlayer,TacticalRole.Portero),"role edits reject other clubs and outfield goalkeeper role");var tacticalReloaded=new Career(Fresh());tacticalReloaded.Restore(CopySave(tacticalCareer));Check(tacticalReloaded.GetRole(rolePlayer)==TacticalRole.CreadorDeJuego,"assigned roles survive JSON save/load");RejectWithoutMutation(tacticalCareer,r=>r.roles[0].role=(TacticalRole)999,"invalid saved role rejected atomically");
var oldTacticalSave=CopySave(tacticalCareer);oldTacticalSave.roles=null;tacticalReloaded.Restore(oldTacticalSave);Check(tacticalReloaded.Roles.Count==0&&Enum.IsDefined(typeof(TacticalRole),tacticalReloaded.GetRole(rolePlayer)),"old saves receive formation-based default roles");
// Integrate the actual decision kernel in a 10-second kinematic scenario, independently of Unity physics.
var moving=new[]{TP(0,TacticalRole.CazadorDeAreas,20,22),TP(0,TacticalRole.ExtremoInvertido,10,-18),TP(0,TacticalRole.CreadorDeJuego,-10,0),TP(1,TacticalRole.CentralDeCobertura,40,5)};var movingPossession=new PossessionTracker();
for(int tick=0;tick<500;tick++){movingPossession.Update(moving,tacticalBall,.15f,0,.02f);for(int i=1;i<3;i++){var d=TacticalBrain.Evaluate(i,moving,tacticalBall,movingPossession);float dx=d.Target.X-moving[i].Position.X,dz=d.Target.Z-moving[i].Position.Z,len=(float)Math.Sqrt(dx*dx+dz*dz);float stepSize=Math.Min(len,.1f);if(len>.001f)moving[i].Position=new PitchPoint(moving[i].Position.X+dx/len*stepSize,moving[i].Position.Z+dz/len*stepSize);}}
Check(moving[1].Position.X>40&&Math.Abs(moving[1].Position.Z)<5&&PitchPoint.DistanceSquared(moving[2].Position,moving[0].Position)<144,"500-step simulation moves winger into box and playmaker into short support");
if(args.Length==2&&args[0]=="--export-tactics"){
 var demo=new[]{TP(0,TacticalRole.CazadorDeAreas,20,22),TP(0,TacticalRole.ExtremoInvertido,10,-18),TP(0,TacticalRole.CreadorDeJuego,-10,0),TP(1,TacticalRole.CentralDeCobertura,40,5)};
 var detector=new PossessionTracker();var trace=new List<object>();
 for(int tick=0;tick<=500;tick++){
  detector.Update(demo,tacticalBall,.15f,0,.02f);
  if(tick%10==0)trace.Add(new{time=tick*.02f,ball=new{x=tacticalBall.X,z=tacticalBall.Z},players=demo.Select((p,i)=>new{x=p.Position.X,z=p.Position.Z,role=p.Role.ToString(),action=TacticalBrain.Evaluate(i,demo,tacticalBall,detector).Action.ToString()}).ToArray()});
  for(int i=1;i<3;i++){var d=TacticalBrain.Evaluate(i,demo,tacticalBall,detector);float dx=d.Target.X-demo[i].Position.X,dz=d.Target.Z-demo[i].Position.Z,len=(float)Math.Sqrt(dx*dx+dz*dz);if(len>.001f){float distance=Math.Min(.1f,len);demo[i].Position=new PitchPoint(demo[i].Position.X+dx/len*distance,demo[i].Position.Z+dz/len*distance);}}
 }
 File.WriteAllText(args[1],JsonSerializer.Serialize(trace));Console.WriteLine("Exported actual tactical decisions: "+args[1]);
}
var previewCareer = new Career(Fresh());
var beforePreview = JsonSerializer.Serialize(previewCareer.Snapshot());
foreach (var league in previewCareer.Data.leagues) foreach (var club in league.clubs) {
 var preview = MatchPreview.Create(previewCareer,club);
 Check(preview != null && preview.Fixture.competition == league.id && preview.Fixture.round == 0 && preview.Fixture.home != preview.Fixture.away && preview.Opponent != club, "preview real fixture " + club);
}
Check(beforePreview == JsonSerializer.Serialize(previewCareer.Snapshot()), "preview does not mutate career, budget or random seed");
Check(MatchPreview.Create(previewCareer,"nonexistent") == null, "preview rejects unavailable club");
var firstPreview = MatchPreview.Create(previewCareer,"madrid");
previewCareer.Advance(new MatchResult{home=firstPreview.Fixture.home,away=firstPreview.Fixture.away,hg=2,ag=1});
Check(MatchPreview.Create(previewCareer,"madrid").Fixture.round == 1, "next preview follows calendar advancement");
Check(firstPreview.Form(previewCareer,firstPreview.Fixture.home) == "V" && firstPreview.Form(previewCareer,firstPreview.Fixture.away) == "D", "preview recent form uses home and away scores");
for(int round=1;round<40;round++)previewCareer.Advance();
Check(MatchPreview.Create(previewCareer,"madrid") == null, "no stale preview after end of season");


// Match rules: geometry, delayed intervention and both attacking directions.
var defenders=new[]{new TacticalPlayer{Team=1,Position=new PitchPoint(50,0)},new TacticalPlayer{Team=1,Position=new PitchPoint(40,0)},new TacticalPlayer{Team=0,Position=new PitchPoint(43,5)},new TacticalPlayer{Team=0,Position=new PitchPoint(30,0)}};
Check(RefereeRules.OffsidePosition(0,new PitchPoint(43,5),new PitchPoint(30,0),defenders),"offside beyond ball and second-last opponent");
Check(!RefereeRules.OffsidePosition(0,new PitchPoint(40,5),new PitchPoint(30,0),defenders),"level with second-last opponent is onside");
Check(!RefereeRules.OffsidePosition(0,new PitchPoint(43,5),new PitchPoint(45,0),defenders),"receiver behind ball is onside");
Check(!RefereeRules.OffsidePosition(0,new PitchPoint(-1,5),new PitchPoint(-10,0),defenders),"own half never offside");
var reverseDefenders=defenders.Select(p=>new TacticalPlayer{Team=1-p.Team,Position=new PitchPoint(-p.Position.X,p.Position.Z)}).ToArray();
Check(RefereeRules.OffsidePosition(1,new PitchPoint(-43,5),new PitchPoint(-30,0),reverseDefenders),"away offside mirrors attack direction");
var offsideTracker=new OffsideTracker();offsideTracker.Capture(3,defenders,new PitchPoint(30,0));defenders[2].Position=new PitchPoint(20,5);
Check(offsideTracker.Touch(2,0,true),"offside snapshot survives attacker returning onside before touch");
offsideTracker.Capture(3,reverseDefenders,new PitchPoint(-30,0));
Check(!offsideTracker.Touch(0,0,false)&&offsideTracker.Touch(2,1,false),"defensive deflection does not reset offside snapshot");
offsideTracker.Capture(3,reverseDefenders,new PitchPoint(-30,0));offsideTracker.Touch(0,0,true);
Check(!offsideTracker.Touch(2,1,true),"opponent deliberate play resets offside");
Check(RefereeRules.Foul(0,new PitchPoint(-40,0))==RestartKind.Penalty,"home defender foul in own penalty area");
Check(RefereeRules.Foul(1,new PitchPoint(40,0))==RestartKind.Penalty,"away defender foul in own penalty area");
Check(RefereeRules.Foul(0,new PitchPoint(40,0))==RestartKind.FreeKick,"foul in opponent penalty area is not penalty");
Check(RefereeRules.Foul(1,new PitchPoint(35.99f,0))==RestartKind.FreeKick,"contact outside penalty area is free kick");
Check(RefereeRules.Foul(1,new PitchPoint(36,20.16f))==RestartKind.Penalty,"penalty area boundary belongs to area");
Check(RefereeRules.Foul(1,new PitchPoint(40,20.2f))==RestartKind.FreeKick,"wide contact outside penalty area");
float cleanBall=RefereeRules.ContactFraction(new PitchPoint(0,0),new PitchPoint(4,0),new PitchPoint(2,0),.4f);
float lateBody=RefereeRules.ContactFraction(new PitchPoint(0,0),new PitchPoint(4,0),new PitchPoint(3,0),.65f);
Check(cleanBall>=0&&cleanBall<lateBody,"swept slide detects ball before body");
Check(RefereeRules.ContactFraction(new PitchPoint(0,0),new PitchPoint(4,0),new PitchPoint(1,0),.65f)<cleanBall,"swept slide detects body before ball");
Check(RefereeRules.ContactFraction(new PitchPoint(0,0),new PitchPoint(4,0),new PitchPoint(2,3),.65f)<0,"missed slide has no invented contact");
Check(RefereeRules.ContactFraction(new PitchPoint(0,0),new PitchPoint(0,0),new PitchPoint(0,0),.65f)==0,"already overlapping capsule is a contact");
Check(DifficultySettings.Reaction(MatchDifficulty.Facil)>DifficultySettings.Reaction(MatchDifficulty.Medio)&&DifficultySettings.Reaction(MatchDifficulty.Medio)>DifficultySettings.Reaction(MatchDifficulty.Dificil),"hard AI reacts more quickly");
Check(DifficultySettings.AimError(MatchDifficulty.Facil)>DifficultySettings.AimError(MatchDifficulty.Medio)&&DifficultySettings.AimError(MatchDifficulty.Medio)>DifficultySettings.AimError(MatchDifficulty.Dificil),"hard AI improves precision without changing player ratings");
var rulesCareer=new Career(Fresh());rulesCareer.Difficulty=MatchDifficulty.Dificil;var rulesReload=new Career(Fresh());rulesReload.Restore(CopySave(rulesCareer));
Check(rulesReload.Difficulty==MatchDifficulty.Dificil,"difficulty survives career save");
RejectWithoutMutation(rulesCareer,r=>r.difficulty=(MatchDifficulty)99,"invalid difficulty rejected atomically");
Check(CompetitionMenu.ForClub(rulesCareer,"madrid").Any(t=>t.id=="cup-spain"),"Copa del Rey shown for eligible Spanish club");
Check(CompetitionMenu.ForClub(rulesCareer,"arsenal").Any(t=>t.id=="cup-england"),"FA Cup shown for eligible English club");
var europaTournament=rulesCareer.Tournaments.Single(t=>t.id=="europa");
Check(europaTournament.participants.Length==36&&!europaTournament.participants.Intersect(rulesCareer.Data.champions).Any(),"Europa seeded separately from Champions");
Check(CompetitionMenu.ForClub(rulesCareer,europaTournament.participants[0]).Any(t=>t.id=="europa"),"Europa shown only for a participant");
Check(CompetitionMenu.ForClub(rulesCareer,rulesCareer.Data.champions[0]).Any(t=>t.id=="champions")&&!CompetitionMenu.ForClub(rulesCareer,rulesCareer.Data.champions[0]).Any(t=>t.id=="europa"),"Champions club menu does not invent Europa qualification");
var legacyCups=CopySave(rulesCareer);legacyCups.tournaments=legacyCups.tournaments.Where(t=>t.id!="europa").ToArray();rulesReload.Restore(legacyCups);
Check(rulesReload.Tournaments.Any(t=>t.id=="europa"),"legacy saves migrate Europa without losing existing tournaments");
var block=new TacticalPlayer[22];for(int i=0;i<22;i++){int blockSlot=i%11,blockTeam=i/11;float dir=blockTeam==0?1:-1;var role=blockSlot==0?TacticalRole.Portero:blockSlot<5?TacticalRole.CentralDeCobertura:blockSlot<8?TacticalRole.CreadorDeJuego:TacticalRole.ExtremoInvertido;block[i]=new TacticalPlayer{Team=blockTeam,Role=role,Anchor=new PitchPoint(dir*(blockSlot<5?-30:blockSlot<8?-10:18),(blockSlot%4-1.5f)*10),Position=new PitchPoint(dir*(blockSlot<5?-30:blockSlot<8?-10:18),(blockSlot%4-1.5f)*10)};}
foreach(var level in new[]{MatchDifficulty.Facil,MatchDifficulty.Medio,MatchDifficulty.Dificil}){
 var blockPossession=new PossessionTracker();blockPossession.Update(block,new PitchPoint(0,0),3,20,.6f);
 var choices=Enumerable.Range(1,10).Select(i=>CoordinatedTactics.Evaluate(i,block,new PitchPoint(0,0),blockPossession,level)).ToArray();
 var held=choices.Where(d=>d.Action!=TacticalAction.Recuperar).ToArray();
 Check(held.Max(d=>d.Target.X)-held.Min(d=>d.Target.X)<=37,"coordinated block limits distance between defensive and attacking lines "+level);
 Check(choices.Count(d=>d.Action==TacticalAction.Recuperar)==1,"one recovering player rather than whole team chasing "+level);
 Check(choices.All(d=>Math.Abs(d.Target.X)<=50&&Math.Abs(d.Target.Z)<=29),"coordinated targets remain on pitch "+level);
}


var partialExit=BallBoundary.Evaluate(new PitchPoint(0,33),new PitchPoint(0,34.10f),0);
Check(partialExit.Kind==RestartKind.None,"ball must entirely cross touchline");
foreach(int lastTouch in new[]{0,1})foreach(float touchSide in new[]{-1f,1f}){
 var throwExit=BallBoundary.Evaluate(new PitchPoint(5,touchSide*33),new PitchPoint(9,touchSide*35),lastTouch);
 Check(throwExit.Kind==RestartKind.ThrowIn&&throwExit.Team==1-lastTouch&&throwExit.Spot.Z==touchSide*34&&throwExit.Spot.X>5&&throwExit.Spot.X<9,"throw-in assigned to opponent at interpolated crossing "+lastTouch+"/"+touchSide);
}
Check(BallBoundary.Evaluate(new PitchPoint(51,20),new PitchPoint(54,20),0).Kind==RestartKind.GoalKick,"attacker last touch over opponent goal line: goal kick");
Check(BallBoundary.Evaluate(new PitchPoint(51,20),new PitchPoint(54,20),1).Kind==RestartKind.CornerKick,"defender last touch over own goal line: corner");
var leftCorner=BallBoundary.Evaluate(new PitchPoint(-51,-20),new PitchPoint(-54,-20),0);
Check(leftCorner.Kind==RestartKind.CornerKick&&leftCorner.Team==1&&leftCorner.Spot.X==-52.5f&&leftCorner.Spot.Z==-34,"left corner mirrors team and restart spot");
Check(BallBoundary.Evaluate(new PitchPoint(50,33),new PitchPoint(60,45),0).Kind==RestartKind.ThrowIn,"diagonal exit uses first crossed line");
Check(BallBoundary.Evaluate(new PitchPoint(50,30),new PitchPoint(60,35),1).Kind==RestartKind.CornerKick,"goal line crossed before touchline selects corner");
Check(BallBoundary.Evaluate(new PitchPoint(0,33),new PitchPoint(0,35),-1).Kind==RestartKind.None,"unknown last touch does not invent restart ownership");
Check(ControlSelection.CanReceive(0,0,true,20,5,4),"deliberate reception switches to own receiver");
Check(!ControlSelection.CanReceive(0,1,true,0,5,4),"opponent reception never transfers human control");
Check(!ControlSelection.CanReceive(0,0,false,25,5,4),"fast incidental deflection does not steal control");
Check(!ControlSelection.CanReceive(0,0,true,0,5,6),"manual change has brief priority over automatic selection");
Check(ControlSelection.CanReceive(0,0,false,5,5,4),"controlled slow ball contact selects own receiver");
var passOpponents=new[]{new TacticalPlayer{Team=1,Position=new PitchPoint(5,0)}};
Check(Passing.Score(new PitchPoint(0,0),new PitchPoint(10,0),0,passOpponents)>Passing.Score(new PitchPoint(0,0),new PitchPoint(10,8),0,passOpponents),"passing AI prefers clear lane over intercepted straight pass");
Check(Passing.Score(new PitchPoint(0,0),new PitchPoint(10,0),0,Array.Empty<TacticalPlayer>())<Passing.Score(new PitchPoint(0,0),new PitchPoint(-10,0),0,Array.Empty<TacticalPlayer>()),"safe forward progress preferred over equally distant back pass");
var predictivePlayers=new[]{new TacticalPlayer{Team=0,Role=TacticalRole.CentralDeCobertura,Position=new PitchPoint(-5,0),Anchor=new PitchPoint(-20,0)},new TacticalPlayer{Team=0,Role=TacticalRole.CreadorDeJuego,Position=new PitchPoint(-20,15),Anchor=new PitchPoint(-10,15)},new TacticalPlayer{Team=1,Role=TacticalRole.CazadorDeAreas,Position=new PitchPoint(0,0),Anchor=new PitchPoint(0,0)}};
var pressurePossession=new PossessionTracker();pressurePossession.Update(predictivePlayers,new PitchPoint(0,0),.15f,1,.02f);
var predictiveEasy=CoordinatedTactics.Evaluate(0,predictivePlayers,new PitchPoint(0,0),pressurePossession,MatchDifficulty.Facil,new PitchPoint(10,0));
var predictiveHard=CoordinatedTactics.Evaluate(0,predictivePlayers,new PitchPoint(0,0),pressurePossession,MatchDifficulty.Dificil,new PitchPoint(10,0));
Check(predictiveHard.Target.X>predictiveEasy.Target.X,"hard pressure anticipates travelling ball further ahead");

foreach(float cornerX in new[]{-52.5f,52.5f})foreach(float cornerZ in new[]{-34f,34f}){
 var legalRestart=RestartPlacement.KeepAway(new PitchPoint(cornerX,cornerZ),new PitchPoint(cornerX,cornerZ),9.3f);
 Check(Math.Abs(legalRestart.X)<=51.5f&&Math.Abs(legalRestart.Z)<=33&&PitchPoint.DistanceSquared(legalRestart,new PitchPoint(cornerX,cornerZ))>=86.48f,"restart distance remains legal after clamping near corner "+cornerX+"/"+cornerZ);
}
Console.WriteLine($"SUCCESS {passed} checks. Domain only; not Unity graphics or PhysX.");

