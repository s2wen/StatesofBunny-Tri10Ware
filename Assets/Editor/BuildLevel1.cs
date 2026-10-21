using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Rendering.Universal;

public static class BuildLevel1
{
    static Sprite square;
    static Material material;
    static Tilemap floor;
    static GameObject Visual(string name, int x, int y, Color color, int order = 0)
    {
        var go = new GameObject(name);
        go.transform.position = new Vector3(x, y, 0);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = square; sr.sharedMaterial = material; sr.color = color; sr.sortingOrder = order;
        return go;
    }
    static void Reference(Object component, string property, Object value)
    {
        var so = new SerializedObject(component); so.FindProperty(property).objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo();
    }
    static GameObject Prefab(string path, string name, int x, int y)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));
        go.name = name; go.transform.position = new Vector3(x,y,0); return go;
    }
    public static void Capture()
    {
        foreach (string level in new[]{"1","2","2.5","3"})
        {
        EditorSceneManager.OpenScene($"Assets/Scenes/Level{level}.unity");
        var camera = Camera.main;
        var target = new RenderTexture(1000,1000,24);
        var request = new UniversalRenderPipeline.SingleCameraRequest { destination = target };
        UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(camera,request);
        RenderTexture.active=target;
        var image=new Texture2D(1000,1000,TextureFormat.RGB24,false);
        image.ReadPixels(new Rect(0,0,1000,1000),0,0);image.Apply();
        System.IO.File.WriteAllBytes($"/tmp/Level{level}-preview.png",image.EncodeToPNG());
        RenderTexture.active=null;Object.DestroyImmediate(image);target.Release();Object.DestroyImmediate(target);
        Debug.Log($"LEVEL{level}_PREVIEW_CAPTURED");
        }
    }
    [MenuItem("Tools/Levels/Rebuild Starter and Gag Levels")]
    public static void Build()
    {
        // Rebuild the starter and gag levels; never overwrite the user's Level3.
        EditorSceneManager.OpenScene("Assets/Scenes/PlayerMovement.unity");
        var original = Object.FindAnyObjectByType<PlayerController>();
        square = original.GetComponent<SpriteRenderer>().sprite;
        material = original.GetComponent<SpriteRenderer>().sharedMaterial;
        BuildStarter(1);
        BuildStarter(2);
        BuildGag();
        EditorBuildSettings.scenes = new[] {
            new EditorBuildSettingsScene("Assets/Scenes/Main Menu.unity",true),
            new EditorBuildSettingsScene("Assets/Scenes/Tutorial.unity",true),
            new EditorBuildSettingsScene("Assets/Scenes/Level1.unity",true),
            new EditorBuildSettingsScene("Assets/Scenes/Level2.unity",true),
            new EditorBuildSettingsScene("Assets/Scenes/Level2.5.unity",true),
            new EditorBuildSettingsScene("Assets/Scenes/Level3.unity",true),
            new EditorBuildSettingsScene("Assets/Scenes/Ending.unity",true)
        };
        AssetDatabase.SaveAssets();
        Debug.Log("PROGRESSION_BUILD_OK");
    }
    // Clone Level2 so the gag keeps its existing art and components.
    public static void BuildGag()
    {
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/Level2.unity");
        var player=Object.FindAnyObjectByType<PlayerController>();
        var map=player.FloorTilemap;
        map.ClearAllTiles();
        var tile=AssetDatabase.LoadAssetAtPath<Tile>("Assets/Art/Level1Floor.asset");
        void Cell(int x,int row)=>map.SetTile(new Vector3Int(x,-row,0),tile);
        void Room(int x0,int x1,int r0,int r1) { for(int x=x0;x<=x1;x++)for(int r=r0;r<=r1;r++)Cell(x,r); }
        Room(1,5,1,5);Room(8,12,1,5);Room(8,12,11,13);Room(1,5,10,12);
        Cell(5,6);Cell(9,6);Room(4,9,7,7);Cell(7,5);Cell(7,6);Room(4,7,8,8);Cell(9,8);
        Cell(4,8);Cell(4,9);Room(10,10,6,8);
        foreach(var cooler in GameObject.FindGameObjectsWithTag("Cooler"))Object.DestroyImmediate(cooler);
        Prefab("Assets/Prefabs/Cooler.prefab","Cooler",1,-12);
        foreach(var position in new[]{new Vector2Int(3,-11),new Vector2Int(4,-11)}) {
            var bridge=Prefab("Assets/Prefabs/BridgeBlock.prefab","Bridge Block",position.x,position.y);
            Reference(bridge.GetComponent<Push>(),"tilemap",map);
            var sprite=bridge.GetComponent<SpriteRenderer>();sprite.color=new Color(1f,.85f,.15f);sprite.sortingOrder=1;
        }
        foreach(int row in new[]{8,9}) {
            var hole=Prefab("Assets/Prefabs/Hole.prefab","Hole",4,-row);
            hole.GetComponent<SpriteRenderer>().color=Color.white;
        }
        foreach(var target in Object.FindObjectsByType<BlockTarget>()) {
            var settings=new SerializedObject(target);settings.FindProperty("movable").boolValue=true;settings.ApplyModifiedPropertiesWithoutUndo();
            target.name="Movable Objective Target";
        }
        var exit=Object.FindAnyObjectByType<Exit>();
        var exitSettings=new SerializedObject(exit);exitSettings.FindProperty("nextScene").stringValue="Level3";exitSettings.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/Level2.5.unity");
        scene=EditorSceneManager.OpenScene("Assets/Scenes/Level2.unity");
        exitSettings=new SerializedObject(Object.FindAnyObjectByType<Exit>());exitSettings.FindProperty("nextScene").stringValue="Level2.5";exitSettings.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.SaveScene(scene);
        EditorBuildSettings.scenes=new[] {
            new EditorBuildSettingsScene("Assets/Scenes/Main Menu.unity",true),
            new EditorBuildSettingsScene("Assets/Scenes/Tutorial.unity",true),
            new EditorBuildSettingsScene("Assets/Scenes/Level1.unity",true),
            new EditorBuildSettingsScene("Assets/Scenes/Level2.unity",true),
            new EditorBuildSettingsScene("Assets/Scenes/Level2.5.unity",true),
            new EditorBuildSettingsScene("Assets/Scenes/Level3.unity",true),
            new EditorBuildSettingsScene("Assets/Scenes/Ending.unity",true)
        };
        AssetDatabase.SaveAssets();
        Debug.Log("LEVEL2_5_BUILD_PASSED");
    }
    static void BuildStarter(int level)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var grid = new GameObject("Grid", typeof(Grid));
        var ground = new GameObject("Floor", typeof(Tilemap), typeof(TilemapRenderer), typeof(LevelBoard));
        ground.transform.SetParent(grid.transform);
        floor = ground.GetComponent<Tilemap>(); floor.tileAnchor = Vector3.zero;
        ground.GetComponent<LevelBoard>().floor = floor;
        ground.GetComponent<TilemapRenderer>().sortingOrder = -2;
        ground.GetComponent<TilemapRenderer>().sharedMaterial = material;
        var tile = AssetDatabase.LoadAssetAtPath<Tile>("Assets/Art/Level1Floor.asset");
        // Coordinates use rows downwards, matching the approved drawings.
        void Floor(int x,int row) => floor.SetTile(new Vector3Int(x,-row,0),tile);
        void Room(int x0,int x1,int r0,int r1) {
            for(int x=x0;x<=x1;x++) for(int r=r0;r<=r1;r++) Floor(x,r);
        }
        GameObject Place(string asset,string name,int x,int row) => Prefab("Assets/Prefabs/"+asset+".prefab",name,x,-row);
        Room(1,5,1,5); Room(8,12,1,5);
        int goalRow = level==1 ? 9 : 12;
        Room(8,12,goalRow-1,goalRow+1);
        for(int x=7;x<=13;x++) for(int r=goalRow-2;r<=goalRow+2;r++)
            if(x==7 || x==13 || r==goalRow-2 || r==goalRow+2)
                Place("SolidWall","Goal Room Wall",x,r);
        if(level==2) {
            Floor(5,6); Floor(9,6);
            for(int x=5;x<=9;x++) Floor(x,7);
            Floor(4,7); Floor(9,8); Floor(7,5); Floor(7,6); Floor(7,8);
            Room(3,5,7,9); Floor(3,10); Floor(8,8);
            Place("Heater","Heater",3,2);
            Place("Cooler","Cooler",3,10);
        }
        var light = new GameObject("Global Light 2D").AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Global; light.intensity = 1;
        var cameraGO = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)); cameraGO.tag="MainCamera";
        cameraGO.transform.position = new Vector3(7,level==1 ? -6 : -7.5f,-10);
        var camera = cameraGO.GetComponent<Camera>(); camera.orthographic=true; camera.orthographicSize=level==1 ? 7 : 8;
        camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=new Color(.12f,.10f,.18f);
        var player = Prefab("Assets/Prefabs/Player.prefab","Player",2,level==1 ? -5 : -4);
        var controller = player.GetComponent<PlayerController>(); Reference(controller,"tilemap",floor);
        var goal = Visual("Goal",level==1 ? 11 : 10,-goalRow,Color.gray);
        var exit = goal.AddComponent<Exit>(); Reference(exit,"sprite",goal.GetComponent<SpriteRenderer>());
        var settings=new SerializedObject(exit); settings.FindProperty("nextScene").stringValue=level==1 ? "Level2" : "Level3"; settings.ApplyModifiedPropertiesWithoutUndo();
        Reference(controller,"exit",exit);
        var loader=Prefab("Assets/Prefabs/LevelLoader.prefab","LevelLoader",0,0).GetComponent<LevelLoader>();
        Reference(exit,"levelLoader",loader);
        var manager=new GameObject("PuzzleManager").AddComponent<PuzzleManager>(); Reference(manager,"exit",exit);
        manager.gameObject.AddComponent<LevelInstructions>(); // Existing R-to-restart behavior, no extra UI/object.
        var targets=level==1 ? new[]{new Vector2Int(2,2),new Vector2Int(11,3)} : new[]{new Vector2Int(11,2),new Vector2Int(11,4)};
        var crates=level==1 ? new[]{new Vector2Int(2,4),new Vector2Int(9,3)} : new[]{new Vector2Int(3,3),new Vector2Int(3,4)};
        foreach(var p in targets) Place("target","Objective Target",p.x,p.y).GetComponent<SpriteRenderer>().sortingOrder=-1;
        foreach(var p in crates) {
            var crate=Place("ObjectiveBlock","Objective Block",p.x,p.y);
            Reference(crate.GetComponent<Push>(),"tilemap",floor);
        }
        int drainX=level==1 ? 9 : 12;
        Place("Drain","Drain - puzzle rooms",drainX,5);
        Place("Drain","Drain - goal room",drainX,goalRow);
        EditorSceneManager.SaveScene(scene,$"Assets/Scenes/Level{level}.unity");
    }
}
