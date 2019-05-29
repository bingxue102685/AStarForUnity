using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ClickHanlder : MonoBehaviour
{
    public Tilemap tilemap;
    public Vector3 worldPos;
    public Vector3 mousePos;
    public Camera maincamera;

    private Tile wayTile;
    private Tile normalTile;
    private Tile startTile;
    private Tile endTile;

    private Vector3Int startPos;
    private Vector3Int endPos;
    private List<string> blockList;
    // Start is called before the first frame update
    void Start()
    {

        startPos = new Vector3Int(0, 0, 0);
        endPos = new Vector3Int(6, -1, 0);

        wayTile = tilemap.GetTile(new Vector3Int(10, -10, 0)) as Tile;
        normalTile = tilemap.GetTile(new Vector3Int(9, -10, 0)) as Tile;
        startTile = tilemap.GetTile(new Vector3Int(8, -10, 0)) as Tile;
        endTile = tilemap.GetTile(new Vector3Int(7, -10, 0)) as Tile;

        blockList = new List<string>();
        blockList.Add(CovertVector3IntToString(new Vector3Int(4, 3, 0)));
        blockList.Add(CovertVector3IntToString(new Vector3Int(4, 2, 0)));
        blockList.Add(CovertVector3IntToString(new Vector3Int(4, 1, 0)));
        blockList.Add(CovertVector3IntToString(new Vector3Int(4, 0, 0)));
        blockList.Add(CovertVector3IntToString(new Vector3Int(4, -1, 0)));
        blockList.Add(CovertVector3IntToString(new Vector3Int(4, -2, 0)));
        blockList.Add(CovertVector3IntToString(new Vector3Int(4, -3, 0)));
        blockList.Add(CovertVector3IntToString(new Vector3Int(4, -4, 0)));

        blockList.Add(CovertVector3IntToString(new Vector3Int(3, 3, 0)));
        blockList.Add(CovertVector3IntToString(new Vector3Int(2, 3, 0)));
        blockList.Add(CovertVector3IntToString(new Vector3Int(1, 3, 0)));
        blockList.Add(CovertVector3IntToString(new Vector3Int(3, -4, 0)));
        blockList.Add(CovertVector3IntToString(new Vector3Int(2, -4, 0)));
        blockList.Add(CovertVector3IntToString(new Vector3Int(1, -4, 0)));

        blockList.Add(CovertVector3IntToString(new Vector3Int(1, 2, 0)));
        blockList.Add(CovertVector3IntToString(new Vector3Int(1, 1, 0)));
        blockList.Add(CovertVector3IntToString(new Vector3Int(1, 0, 0)));
        blockList.Add(CovertVector3IntToString(new Vector3Int(1, -1, 0)));
        blockList.Add(CovertVector3IntToString(new Vector3Int(1, -2, 0)));
        blockList.Add(CovertVector3IntToString(new Vector3Int(1, -3, 0)));
    }

    string CovertVector3IntToString(Vector3Int vector3Int)
    {
        return vector3Int.x+"|"+vector3Int.y;
    }

    // Update is called once per frame
    void Update()
    {
        //左键修改起始位置
        if (Input.GetMouseButtonDown(0))
        {
            RecoverMap();
            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, maincamera.nearClipPlane);
            Vector3 wordPosition = maincamera.ScreenToWorldPoint(mousePosition);
            var tempStartPos = tilemap.WorldToCell(new Vector3(wordPosition.x, wordPosition.y, 0));
            tilemap.SetTile(tempStartPos, startTile);
            tilemap.SetTile(startPos, normalTile);
            startPos = tempStartPos;
            AStarFindWay();
        }
        //右键修改结束位置
        if (Input.GetMouseButtonDown(1))
        {
            
            RecoverMap();
            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, maincamera.nearClipPlane);
            Vector3 wordPosition = maincamera.ScreenToWorldPoint(mousePosition);
            var tempEndPos = tilemap.WorldToCell(new Vector3(wordPosition.x, wordPosition.y, 0));
            tilemap.SetTile(tempEndPos, endTile);
            tilemap.SetTile(endPos, normalTile);
            endPos = tempEndPos;
            AStarFindWay();
        }
    }

    private void AStarFindWay()
    {
        AStar.AStarArithmetic.instance.Init(startPos.x, startPos.y, endPos.x, endPos.y, blockList, true);
        AStar.AStarArithmetic.instance.StartFindWay();
        var aStarWay = AStar.AStarArithmetic.instance.GetAvailableWay();

        for (int i = 1; i < aStarWay.Count - 1; i++)
        {
            tilemap.SetTile(new Vector3Int(aStarWay[i].x, aStarWay[i].y, 0), wayTile);
        }
    }

    private void RecoverMap()
    {
        var aStarWay = AStar.AStarArithmetic.instance.GetAvailableWay();

        for (int i = 1; i < aStarWay.Count - 1; i++)
        {
            tilemap.SetTile(new Vector3Int(aStarWay[i].x, aStarWay[i].y, 0), normalTile);
        }
    }
}
