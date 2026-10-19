using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

// Runs the solver's complete route against the real movement coroutines in Play Mode.
[InitializeOnLoad]
public static class VerifyLevel1
{
    [Serializable] class Step { public int dx,dy,state; }
    [Serializable] class Route { public Step[] actions; }
    static Route route;
    static int index;
    static bool pending, initialized;
    static Vector3 expected;
    static double deadline, finishedAt;
    static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
    static VerifyLevel1() { EditorApplication.update += Tick; }
    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Level1.unity");
        if (UnityEngine.Object.FindObjectsByType<BlockTarget>().Length!=3) throw new Exception("Expected 3 objectives");
        if (UnityEngine.Object.FindObjectsByType<Hole>().Length!=2) throw new Exception("Expected 2 holes");
        if (UnityEngine.Object.FindObjectsByType<Push>().Count(p=>p.FillsHoles)!=2) throw new Exception("Expected 2 bridge blocks");
        var floor=UnityEngine.Object.FindAnyObjectByType<Tilemap>();
        foreach(var wall in UnityEngine.Object.FindObjectsByType<PermeableWall>())
            if(floor.HasTile(floor.WorldToCell(wall.transform.position))) throw new Exception("Wall was painted as floor");
        SessionState.SetBool("VerifyLevel1.Active",true);
        EditorApplication.EnterPlaymode();
    }
    static void Assert(bool test,string message) { if(!test) throw new Exception(message); }
    static void Tick()
    {
        if(!SessionState.GetBool("VerifyLevel1.Active",false) || !EditorApplication.isPlaying || EditorApplication.isCompiling) return;
        try
        {
            if(SceneManager.GetActiveScene().name=="Level2")
            {
                Debug.Log("LEVEL1_PLAYTEST_PASSED: complete solution, both bridges locked, three targets covered, actual Level2 transition.");
                SessionState.SetBool("VerifyLevel1.Active",false); EditorApplication.Exit(0); return;
            }
            var player=UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            if(!player || player.GetComponent<PlayerAttributes>()==null)return;
            // Wait until Start has initialized the controller.
            if(typeof(PlayerController).GetField("playerState",Flags).GetValue(player)==null)return;
            var attr=player.GetComponent<PlayerAttributes>();
            if(!initialized)
            {
                route=JsonUtility.FromJson<Route>(File.ReadAllText(Path.Combine(Application.dataPath,"../Tools/level1-solution.json")));
                var exit=UnityEngine.Object.FindAnyObjectByType<Exit>();Assert(!exit.IsOpen,"Exit initially unlocked");
                var canMove=typeof(PlayerController).GetMethod("canMove",Flags);
                player.transform.position=new Vector3(5,7,0);attr.setState(States.Solid);
                Assert(!(bool)canMove.Invoke(player,new object[]{Vector2.left}),"Solid can enter unfilled hole");
                attr.setState(States.Gas);Assert((bool)canMove.Invoke(player,new object[]{Vector2.left}),"Gas cannot cross hole");
                player.transform.position=new Vector3(7,5,0);attr.setState(States.Solid);
                Assert(!(bool)canMove.Invoke(player,new object[]{Vector2.down}),"Solid can cross wall");
                attr.setState(States.Gas);Assert((bool)canMove.Invoke(player,new object[]{Vector2.down}),"Gas cannot cross wall");
                player.transform.position=new Vector3(7,2,0);attr.setState(States.Solid);
                exit.ReachExit();Assert(SceneManager.GetActiveScene().name=="Level1","Locked exit transitioned");
                var regular=UnityEngine.Object.FindObjectsByType<Push>().First(p=>!p.FillsHoles);
                var originalPosition=regular.transform.position;regular.transform.position=new Vector3(3,7,0);
                Assert(!regular.Move(Vector2.right),"Regular objective block filled a hole");regular.transform.position=originalPosition;
                var trigger=typeof(PlayerController).GetMethod("OnTriggerEnter2D",Flags);
                var cooler=GameObject.FindGameObjectWithTag("Cooler").GetComponent<Collider2D>();
                var heater=GameObject.FindGameObjectWithTag("Heater").GetComponent<Collider2D>();
                attr.setState(States.Gas);trigger.Invoke(player,new object[]{cooler});Assert(attr.getState()==States.Liquid,"Cooler failed");
                trigger.Invoke(player,new object[]{cooler});Assert(attr.getState()==States.Solid,"Cooler failed to freeze");
                trigger.Invoke(player,new object[]{heater});Assert(attr.getState()==States.Liquid,"Heater failed");
                trigger.Invoke(player,new object[]{heater});Assert(attr.getState()==States.Gas,"Heater failed to vaporize");
                var drain=UnityEngine.Object.FindAnyObjectByType<Drain>();var destination=drain.moveNext();
                attr.setState(States.Liquid);trigger.Invoke(player,new object[]{drain.GetComponent<Collider2D>()});
                Assert(Vector2.Distance(player.transform.position,destination.getPos())<.01f,"Drain failed to teleport liquid");
                trigger.Invoke(player,new object[]{destination.GetComponent<Collider2D>()});
                Assert(Vector2.Distance(player.transform.position,destination.getPos())<.01f,"Drain bounced player back");
                typeof(PlayerController).GetMethod("OnTriggerExit2D",Flags).Invoke(player,new object[]{destination.GetComponent<Collider2D>()});
                player.transform.position=new Vector3(0,8,0);attr.setState(States.Gas);
                Assert((bool)canMove.Invoke(player,new object[]{Vector2.left}),"Gas cannot cross empty space like PlayerMovement");
                player.transform.position=new Vector3(7,7,0);attr.setState(States.Solid);
                initialized=true;deadline=EditorApplication.timeSinceStartup+120;
            }
            if(EditorApplication.timeSinceStartup>deadline)throw new Exception("Playtest timed out at action "+index);
            bool moving=(bool)typeof(PlayerController).GetField("isMoving",Flags).GetValue(player);
            if(moving)return;
            if(pending)
            {
                Assert(Vector3.Distance(player.transform.position,expected)<.01f,"Move failed at action "+(index-1)+" expected "+expected+" actual "+player.transform.position);
                pending=false;
            }
            if(index==route.actions.Length)
            {
                if(finishedAt==0) finishedAt=EditorApplication.timeSinceStartup;
                if(EditorApplication.timeSinceStartup-finishedAt>5) throw new Exception("Route ended without Level2 transition");
                return;
            }
            if(index==route.actions.Length-1)
            {
                Assert(UnityEngine.Object.FindAnyObjectByType<Exit>().IsOpen,"Completed objectives did not unlock exit");
                var bridges=UnityEngine.Object.FindObjectsByType<Push>().Where(p=>p.FillsHoles).ToArray();
                Assert(bridges.All(p=>p.IsBridge && !p.Move(Vector2.up)),"Bridge was not permanently locked");
                Assert(bridges.All(p=>p.GetComponent<Rigidbody2D>().bodyType==RigidbodyType2D.Static),"Bridge body not static");
            }
            var step=route.actions[index++]; attr.setState((States)step.state);
            player.GetComponent<SpriteRenderer>().color=attr.getColor();
            expected=player.transform.position+new Vector3(step.dx,step.dy,0);pending=true;
            typeof(PlayerController).GetMethod("Move",Flags).Invoke(player,new object[]{new Vector2(step.dx,step.dy)});
        }
        catch(Exception e)
        {
            Debug.LogError("LEVEL1_PLAYTEST_FAILED: "+e);
            SessionState.SetBool("VerifyLevel1.Active",false);EditorApplication.Exit(1);
        }
    }
}
