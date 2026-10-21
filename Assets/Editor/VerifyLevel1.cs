using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Verifies the complete four-level progression using real movement and trigger callbacks.
// The filename is retained for compatibility with existing command-line validation.
[InitializeOnLoad]
public static class VerifyLevel1
{
    [Serializable] class Step { public int dx,dy,state,afterState; public bool teleport; public int ex,ey; }
    [Serializable] class Route { public Step[] actions; public int pushes,objectiveMoves; public bool completeRoute; }
    [Serializable] class Routes { public Route[] levels; }
    static List<Step> actions;
    static int level,index;
    static readonly string[] SceneNames={"Level1","Level2","Level2.5","Level3"};
    static int stage=-1;
    static int targetProbe;
    static bool probePending;
    static BlockTarget probeMarker;
    static Vector3 probePosition,probeStart;
    static bool pending,finishing;
    static bool completeRoute;
    static int objectiveMoves,expectedState;
    static Vector3 expected;
    static double deadline,readyAt,finishedAt;
    static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
    static readonly Vector2Int[] Directions={Vector2Int.right,Vector2Int.left,Vector2Int.up,Vector2Int.down};
    static VerifyLevel1() { EditorApplication.update += Tick; }
    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Level1.unity");
        SessionState.SetBool("VerifyLevel1.Active",true);
        EditorApplication.EnterPlaymode();
    }
    static void Assert(bool test,string message) { if(!test) throw new Exception(message); }
    static Vector2Int Pos(Component c)=>Vector2Int.RoundToInt(c.transform.position);
    static void State(PlayerController player,States state)
    {
        // Exercise the same handler as the E key, rather than changing gameplay state directly.
        for(int i=0;i<3 && player.GetComponent<PlayerAttributes>().getState()!=state;i++)
            typeof(PlayerController).GetMethod("ChangeState",Flags).Invoke(player,null);
    }
    static List<Vector2Int> Path(Vector2Int from,Vector2Int to)
    {
        var occupied=UnityEngine.Object.FindObjectsByType<Push>().Where(p=>!p.IsBridge).Select(Pos).ToHashSet();
        var q=new Queue<Vector2Int>(); q.Enqueue(from);
        var prev=new Dictionary<Vector2Int,Vector2Int>();prev[from]=from;
        while(q.Count>0 && !prev.ContainsKey(to)) {
            var p=q.Dequeue();
            foreach(var d in Directions) {
                var n=p+d;
                if(n.x < -3 || n.x > 18 || n.y < -18 || n.y > 18 || SolidWall.IsAt(n) || occupied.Contains(n) || prev.ContainsKey(n)) continue;
                prev[n]=p;q.Enqueue(n);
            }
        }
        Assert(prev.ContainsKey(to),"No path to "+to);
        var result=new List<Vector2Int>();
        for(var p=to;p!=from;p=prev[p]) result.Add(p);
        result.Reverse();return result;
    }
    static void AppendWalk(ref Vector2Int p,Vector2Int to)
    {
        foreach(var n in Path(p,to)) { actions.Add(new Step{dx=n.x-p.x,dy=n.y-p.y,state=2});p=n; }
    }
    static void FinishRoute(PlayerController player,Exit exit)
    {
        Assert(exit.IsOpen,"All crate moves finished but goal is locked");
        var blocks=GameObject.FindGameObjectsWithTag("Blocks");
        Assert(UnityEngine.Object.FindObjectsByType<BlockTarget>().All(t=>t.IsCovered(blocks)),"Target not covered");
        var bridges=UnityEngine.Object.FindObjectsByType<Push>().Where(p=>p.FillsHoles).ToArray();
        Assert(bridges.All(p=>p.IsBridge && !p.Move(Vector2.up)),"Bridge did not lock permanently");
        // Enter the drain whose actual runtime successor is inside the sealed goal room.
        var drains=UnityEngine.Object.FindObjectsByType<Drain>();
        var goal=Pos(exit);
        var destination=drains.OrderBy(d=>(Pos(d)-goal).sqrMagnitude).First();
        var source=drains.First(d=>d.moveNext()==destination);
        var current=Pos(player);
        var occupied=UnityEngine.Object.FindObjectsByType<Push>().Where(p=>!p.IsBridge).Select(Pos).ToHashSet();
        var stand=Directions.Select(d=>Pos(source)+d).First(p=>player.FloorTilemap.HasTile(new Vector3Int(p.x,p.y,0)) && !occupied.Contains(p) && !SolidWall.IsAt(p));
        AppendWalk(ref current,stand);
        var delta=Pos(source)-current;
        actions.Add(new Step{dx=delta.x,dy=delta.y,state=1,teleport=true,ex=Pos(destination).x,ey=Pos(destination).y});
        current=Pos(destination);AppendWalk(ref current,goal);
        Debug.Log($"LEVEL{level}_OBJECTIVES_PASSED; testing drain and goal now.");
    }
    static void Inspect(PlayerController player,Exit exit)
    {
        Assert(!exit.IsOpen,"Goal initially open");
        Assert(UnityEngine.Object.FindObjectsByType<BlockTarget>().Length==(level==3?3:2),"Wrong target count");
        Assert(UnityEngine.Object.FindObjectsByType<Push>().Count(p=>p.FillsHoles)==((level==3 || level==25)?2:0),"Unexpected bridge blocks");
        Assert(UnityEngine.Object.FindObjectsByType<Drain>().Length==(level==3?3:2),"Unexpected drain count / scene leak");
        var start=player.transform.position;
        player.transform.position=exit.transform.position;State(player,States.Gas);exit.ReachExit();
        Assert(!exit.IsCompleted && SceneManager.GetActiveScene().name==SceneNames[stage],"Gas bypassed locked goal");
        var canMove=typeof(PlayerController).GetMethod("canMove",Flags);
        foreach(var wall in UnityEngine.Object.FindObjectsByType<SolidWall>()) {
            player.transform.position=wall.transform.position+Vector3.left;
            foreach(States state in Enum.GetValues(typeof(States))) {
                State(player,state);
                Assert(!(bool)canMove.Invoke(player,new object[]{Vector2.right}),"Form passed solid wall");
            }
        }
        if(level<3 || level==25) {
            var crate=UnityEngine.Object.FindObjectsByType<Push>().First(p=>!p.FillsHoles);
            var original=crate.transform.position;
            player.transform.position=original+Vector3.left;
            foreach(var state in new[]{States.Liquid,States.Gas}) {
                State(player,state);
                Assert(!(bool)canMove.Invoke(player,new object[]{Vector2.right}),"Non-solid form moved a crate");
            }
            crate.transform.position=new Vector3(5,-3,0);
            Assert(!(bool)typeof(Push).GetMethod("canMove",Flags).Invoke(crate,new object[]{Vector2.right}),"Crate can bypass route through empty space");
            crate.transform.position=original;
        }
        player.transform.position=start;State(player,States.Solid);
    }
    static void Tick()
    {
        if(!SessionState.GetBool("VerifyLevel1.Active",false) || !EditorApplication.isPlaying || EditorApplication.isCompiling)return;
        try {
            if(stage==3 && SceneManager.GetActiveScene().name=="Ending") {
                Debug.Log("PROGRESSION_PLAYTEST_PASSED: all four levels solved and transitioned to Ending.");
                SessionState.SetBool("VerifyLevel1.Active",false);EditorApplication.Exit(0);return;
            }
            var player=UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            if(!player || typeof(PlayerController).GetField("playerState",Flags).GetValue(player)==null)return;
            var exit=UnityEngine.Object.FindAnyObjectByType<Exit>();
            string scene=SceneManager.GetActiveScene().name;
            if(stage<0 || scene!=SceneNames[stage]) {
                Assert(stage+1<SceneNames.Length && scene==SceneNames[stage+1],"Unexpected scene transition: "+scene);
                if(level>0)Debug.Log($"LEVEL{level}_TRANSITION_PASSED -> {scene}");
                stage++;level=stage==2 ? 25 : stage==3 ? 3 : stage+1;
                var routes=JsonUtility.FromJson<Routes>(File.ReadAllText(System.IO.Path.Combine(Application.dataPath,"../Tools/level1-solution.json")));
                var route=routes.levels[stage];
                actions=route.actions.ToList();completeRoute=route.completeRoute;objectiveMoves=route.objectiveMoves;
                index=0;pending=false;finishing=completeRoute;finishedAt=0;
                Inspect(player,exit);
                Assert(UnityEngine.Object.FindObjectsByType<BlockTarget>().Count(t=>t.Movable)==(level==25?2:0),"Movable targets leaked to another scene");
                if(level==25) {
                    probeMarker=UnityEngine.Object.FindObjectsByType<BlockTarget>().OrderBy(t=>t.transform.position.y).Last();
                    probePosition=probeMarker.transform.position;probeStart=player.transform.position;targetProbe=0;probePending=false;
                    probeMarker.transform.position=new Vector3(5,-3,0);
                    Assert(!probeMarker.Move(Vector2.right,player.FloorTilemap,GameObject.FindGameObjectsWithTag("Blocks")),"Target crossed empty space");
                    probeMarker.transform.position=probePosition;
                }
                deadline=EditorApplication.timeSinceStartup+180;
                readyAt=EditorApplication.timeSinceStartup+.2;
            }
            if(EditorApplication.timeSinceStartup>deadline)throw new Exception($"Timeout Level{level} move {index}");
            if(Time.time<player.movementLockedUntil || EditorApplication.timeSinceStartup<readyAt || (bool)typeof(PlayerController).GetField("isMoving",Flags).GetValue(player))return;
            if(level==25 && targetProbe<3) {
                if(probePending) {
                    Assert(Vector3.Distance(probeMarker.transform.position,probePosition+Vector3.right)<.01f,"Target cannot be pushed by "+(States)targetProbe);
                    Assert(Vector3.Distance(player.transform.position,probePosition)<.01f,"Player did not follow movable target");
                    probeMarker.transform.position=probePosition;player.transform.position=probeStart;
                    probePending=false;targetProbe++;
                    if(targetProbe==3) {
                        State(player,States.Solid);readyAt=EditorApplication.timeSinceStartup+.2;
                        Debug.Log("MOVABLE_TARGET_ALL_THREE_FORMS_PASSED");return;
                    }
                }
                player.transform.position=probePosition+Vector3.left;State(player,(States)targetProbe);
                typeof(PlayerController).GetMethod("Move",Flags).Invoke(player,new object[]{Vector2.right});
                probePending=true;readyAt=EditorApplication.timeSinceStartup+.22;return;
            }
            if(pending) {
                Assert(Vector3.Distance(player.transform.position,expected)<.01f,$"Level{level} move {index-1}: expected {expected}, got {player.transform.position}");
                if(completeRoute) Assert((int)player.GetComponent<PlayerAttributes>().getState()==expectedState,$"Temperature state mismatch at move {index-1}");
                pending=false;
                if(completeRoute && index==objectiveMoves) {
                    Assert(exit.IsOpen,"Temperature-only solution did not unlock goal");
                    Assert(UnityEngine.Object.FindObjectsByType<BlockTarget>().All(t=>t.IsCovered(GameObject.FindGameObjectsWithTag("Blocks"))),"Target not covered");
                    if(level==25) {
                        var bridges=UnityEngine.Object.FindObjectsByType<Push>().Where(p=>p.FillsHoles).ToArray();
                        Assert(bridges.Length==2 && bridges.All(p=>p.IsBridge && !p.Move(Vector2.up)),"Gag bridges failed to lock");
                        Assert(bridges.All(p=>p.GetComponent<Rigidbody2D>().bodyType==RigidbodyType2D.Static),"Gag bridge body not static");
                    }
                    Debug.Log(SceneNames[stage]+"_TEMPERATURE_ONLY_OBJECTIVES_PASSED; no E key used in solution.");
                }
            }
            if(index==actions.Count) {
                if(!finishing) { finishing=true;FinishRoute(player,exit); }
                else {
                    if(level==3 && exit.IsCompleted) {
                        Debug.Log("PROGRESSION_PLAYTEST_PASSED: all four levels solved; real Level1 -> Level2 -> Level2.5 -> Level3 transitions; drains, wall blocking and locked goals verified; final level completes without looping.");
                        SessionState.SetBool("VerifyLevel1.Active",false);EditorApplication.Exit(0);return;
                    }
                    if(finishedAt==0)finishedAt=EditorApplication.timeSinceStartup;
                    Assert(EditorApplication.timeSinceStartup-finishedAt<15,"Goal did not complete/transition");return;
                }
            }
            var step=actions[index++];
            if(step.state>=0) State(player,(States)step.state);
            else Assert(completeRoute,"Unexpected temperature-only action");
            expectedState=step.afterState;
            expected=step.teleport ? new Vector3(step.ex,step.ey,0) : player.transform.position+new Vector3(step.dx,step.dy,0);
            pending=true;readyAt=EditorApplication.timeSinceStartup+.22;
            typeof(PlayerController).GetMethod("Move",Flags).Invoke(player,new object[]{new Vector2(step.dx,step.dy)});
        } catch(Exception e) {
            Debug.LogError("PROGRESSION_PLAYTEST_FAILED: "+e);
            SessionState.SetBool("VerifyLevel1.Active",false);EditorApplication.Exit(1);
        }
    }
}
