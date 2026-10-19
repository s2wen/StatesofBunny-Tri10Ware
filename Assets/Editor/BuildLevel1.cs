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
        EditorSceneManager.OpenScene("Assets/Scenes/Level1.unity");
        var camera = Camera.main;
        var target = new RenderTexture(1000,1000,24);
        var request = new UniversalRenderPipeline.SingleCameraRequest { destination = target };
        UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(camera,request);
        RenderTexture.active=target;
        var image=new Texture2D(1000,1000,TextureFormat.RGB24,false);
        image.ReadPixels(new Rect(0,0,1000,1000),0,0);image.Apply();
        System.IO.File.WriteAllBytes("/tmp/Level1-preview.png",image.EncodeToPNG());
        RenderTexture.active=null;Object.DestroyImmediate(image);target.Release();Object.DestroyImmediate(target);
        Debug.Log("LEVEL1_PREVIEW_CAPTURED");
    }
    [MenuItem("Tools/Levels/Build Level1 From Plan")]
    public static void Build()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/PlayerMovement.unity");
        var original = Object.FindAnyObjectByType<PlayerController>();
        square = original.GetComponent<SpriteRenderer>().sprite;
        material = original.GetComponent<SpriteRenderer>().sharedMaterial;
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var grid = new GameObject("Grid", typeof(Grid));
        var ground = new GameObject("Floor", typeof(Tilemap), typeof(TilemapRenderer), typeof(LevelBoard));
        ground.transform.SetParent(grid.transform);
        floor = ground.GetComponent<Tilemap>(); floor.tileAnchor = Vector3.zero;
        ground.GetComponent<LevelBoard>().floor = floor;
        ground.GetComponent<TilemapRenderer>().sortingOrder = -2;
        ground.GetComponent<TilemapRenderer>().sharedMaterial = material;
        const string tilePath = "Assets/Art/Level1Floor.asset";
        var tile = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
        if (!tile) { tile = ScriptableObject.CreateInstance<Tile>(); AssetDatabase.CreateAsset(tile, tilePath); }
        var originalTile = AssetDatabase.LoadAssetAtPath<Tile>("Assets/Art/ground.asset");
        tile.sprite = originalTile.sprite; tile.color = originalTile.color; tile.colliderType = Tile.ColliderType.None;
        tile.transform = originalTile.transform; EditorUtility.SetDirty(tile);
        for (int x=0;x<15;x++) for (int y=0;y<15;y++)
            if ((x>=5 && x<=9) || (y>=5 && y<=9)) floor.SetTile(new Vector3Int(x,y,0),tile);

        var light = new GameObject("Global Light 2D").AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Global; light.intensity = 1;
        var cameraGO = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)); cameraGO.tag="MainCamera";
        cameraGO.transform.position = new Vector3(7,7,-10);
        var camera = cameraGO.GetComponent<Camera>(); camera.orthographic=true; camera.orthographicSize=8.6f;
        camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=new Color(.12f,.10f,.18f);

        var player = Visual("Player",7,7,Color.red,2); player.tag="Player";
        player.AddComponent<PlayerAttributes>();
        var controller = player.AddComponent<PlayerController>(); Reference(controller,"tilemap",floor);
        player.AddComponent<BoxCollider2D>().size = new Vector2(.65f,.65f);
        var body=player.AddComponent<Rigidbody2D>(); body.gravityScale=0; body.constraints=RigidbodyConstraints2D.FreezeRotation;

        var goal = Visual("Goal - opens when all three targets are covered",7,2,Color.gray);
        var exit = goal.AddComponent<Exit>(); Reference(exit,"sprite",goal.GetComponent<SpriteRenderer>());
        var exitSettings=new SerializedObject(exit); exitSettings.FindProperty("nextScene").stringValue="Level2"; exitSettings.ApplyModifiedPropertiesWithoutUndo();
        Reference(controller,"exit",exit);
        var manager=new GameObject("PuzzleManager").AddComponent<PuzzleManager>(); Reference(manager,"exit",exit);
        foreach (var p in new[]{new Vector2Int(5,9),new Vector2Int(9,9),new Vector2Int(9,5)})
            Prefab("Assets/Prefabs/target.prefab","Objective Target",p.x,p.y).GetComponent<SpriteRenderer>().sortingOrder=-1;

        var regular=Visual("ObjectiveBlock",0,0,new Color(.48f,.29f,.10f),1);regular.tag="Blocks";
        regular.AddComponent<Push>();
        PrefabUtility.SaveAsPrefabAsset(regular,"Assets/Prefabs/ObjectiveBlock.prefab"); Object.DestroyImmediate(regular);
        foreach (var p in new[]{new Vector2Int(1,7),new Vector2Int(13,7),new Vector2Int(8,13)})
        {
            var crate=Prefab("Assets/Prefabs/ObjectiveBlock.prefab","Objective Block",p.x,p.y);Reference(crate.GetComponent<Push>(),"tilemap",floor);
        }
        foreach(var p in new[]{new Vector2Int(1,8),new Vector2Int(6,13)})
        {
            var bridge=Prefab("Assets/Prefabs/BridgeBlock.prefab","Bridge Block",p.x,p.y);
            Reference(bridge.GetComponent<Push>(),"tilemap",floor);
            bridge.GetComponent<SpriteRenderer>().sortingOrder=1;
        }
        foreach(var p in new[]{new Vector2Int(4,7),new Vector2Int(10,7)})
            Prefab("Assets/Prefabs/Hole.prefab","Hole",p.x,p.y);

        var wall=new GameObject("PermeableWall");wall.tag="Vents";wall.AddComponent<PermeableWall>();
        PrefabUtility.SaveAsPrefabAsset(wall,"Assets/Prefabs/PermeableWall.prefab");Object.DestroyImmediate(wall);
        for(int y=5;y<=9;y++) if(y!=7) foreach(int x in new[]{4,10}) Prefab("Assets/Prefabs/PermeableWall.prefab","Permeable Wall",x,y);
        foreach(int x in new[]{5,6,8,9}) Prefab("Assets/Prefabs/PermeableWall.prefab","Permeable Wall",x,10);
        for(int x=5;x<=9;x++) Prefab("Assets/Prefabs/PermeableWall.prefab","Permeable Wall",x,4);
        foreach(var p in new[]{new Vector2Int(0,9),new Vector2Int(14,9)}) Prefab("Assets/Prefabs/Cooler.prefab","Cooler",p.x,p.y);
        Prefab("Assets/Prefabs/Heater.prefab","Heater",5,0);
        foreach(var p in new[]{new Vector2Int(0,5),new Vector2Int(14,5),new Vector2Int(9,0)}) Prefab("Assets/Prefabs/Drain.prefab","Drain",p.x,p.y);
        foreach (var w in Object.FindObjectsByType<PermeableWall>())
            floor.SetTile(floor.WorldToCell(w.transform.position), null);
        for(int x=4;x<=10;x++) for(int y=-1;y<=4;y++)
            if(x==4 || x==10 || y==-1 || y==4)
                Prefab("Assets/Prefabs/SolidWall.prefab",$"Goal Room Wall ({x}, {y})",x,y);
        new GameObject("Level Controls",typeof(LevelInstructions));
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/Level1.unity");
        var scenes=EditorBuildSettings.scenes.ToList();
        scenes.RemoveAll(s=>s.path=="Assets/Scenes/Level1.unity");
        scenes.Insert(0,new EditorBuildSettingsScene("Assets/Scenes/Level1.unity",true));
        EditorBuildSettings.scenes=scenes.ToArray();AssetDatabase.SaveAssets();
        Debug.Log("LEVEL1_BUILD_OK");
    }
}
