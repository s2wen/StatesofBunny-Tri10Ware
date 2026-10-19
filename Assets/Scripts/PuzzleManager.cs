using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [SerializeField] private Exit exit;

    private BlockTarget[] targets;
    private GameObject[] blocks;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        targets = FindObjectsByType<BlockTarget>();
        blocks = GameObject.FindGameObjectsWithTag("Blocks");
        Push.BlockSettled += Check;
        Check();
    }

    void OnDestroy() => Push.BlockSettled -= Check;

    private void Check(){
        
        foreach(var t in targets){
            if(!t.IsCovered(blocks)){
                exit.SetOpen(false);
                return;
            } 
        }

        exit.SetOpen(true);
        Debug.Log("exit open");
    }
}
