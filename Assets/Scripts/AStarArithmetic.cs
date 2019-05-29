using System;
using System.Collections.Generic;

namespace AStar
{
    public enum DirectionRelativeParent
    {
        //上
        UP,
        //下
        DOWN,
        //左
        LEFT,
        //右
        RIGHT,
        //左上
        LEFT_UP,
        //左下
        LEFT_DOWN,
        //右上
        RIGHT_UP,
        //右下
        RIGHT_DOWN,
        //开始
        START,
        //结束
        END,
        //未知
        UNKNOWN
    }

    public struct Point
    {
        public int x;
        public int y;
        public int z;
        public Point(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
            this.z = 0;
        }

        public static Point operator +(Point p1, Point p2)
        {
            return new Point(p1.x + p2.x, p1.y + p2.y, p1.z + p2.z);
        }

        public static Point operator -(Point p1, Point p2)
        {
            return new Point(p1.x - p2.x, p1.y - p2.y, p1.z - p2.z);
        }
    }

    public class NodeInfo
    {
        //当前点到目标点的估算值
        public int HValue;
        //当前点到起点的实际值
        public int GValue;
        public int FValue
        {
            get
            {
                return HValue + GValue;
            }
        }
        //当前点的位置
        public int gridX;
        public int gridY;
        //父节点
        public NodeInfo parentNode;
        //相对于父节点的方向
        public DirectionRelativeParent direction;
        //用坐标表示该位置
        public string toString
        {
            get
            {
                return gridX + "|" + gridY;
            }
        }
    }
    //开放表
    class OpenNodeList : IComparer<NodeInfo>
    {
        private List<NodeInfo> openList;
        private Dictionary<string, NodeInfo> openDict;

        public OpenNodeList()
        {
            openList = new List<NodeInfo>();
            openDict = new Dictionary<string, NodeInfo>();
        }

        public void AddNode(NodeInfo node)
        {
            bool isNeedSort = false;
            if (IsAlreadyInOpenList(node))
            {
                var nodeInOpenList = GetNodeInfoByGridString(node.toString);
                //如果已经在openlist中节点的GValue比较大，则更新G 和 parent
                if (nodeInOpenList.GValue > node.GValue)
                {
                    nodeInOpenList.GValue = node.GValue;
                    nodeInOpenList.parentNode = node.parentNode;
                    isNeedSort = true;
                }
            }
            else
            {
                openList.Add(node);
                openDict.Add(node.toString, node);
                isNeedSort = true;
            }

            //不在此处进行sort
            //if (isNeedSort)
            //{
            //    openList.Sort(this);
            //}
        }

        public int Compare(NodeInfo x, NodeInfo y)
        {
            return x.FValue - y.FValue;
        }

        public void SortOpenList()
        {
            openList.Sort(this);
        }

        public void RemoveNode(NodeInfo node)
        {
            if (openList.Contains(node))
            {
                openList.Remove(node);
                openDict.Remove(node.toString);
            }
        }

        public bool IsAlreadyInOpenList(int gridX, int gridY)
        {
            return IsAlreadyInOpenList(gridX + "|" + gridY);
        }

        public bool IsAlreadyInOpenList(string posStr)
        {
            return openDict.ContainsKey(posStr);
        }

        public bool IsAlreadyInOpenList(NodeInfo node)
        {
            return IsAlreadyInOpenList(node.toString);
        }

        public NodeInfo GetNodeInfoByGridString(string posStr)
        {
            NodeInfo node;
            if (openDict.TryGetValue(posStr, out node))
            {
                return node;
            }
            return null;
        }

        public NodeInfo GetMinFValueNode()
        {
            //即openlist第一个值
            return openList[0];
        }

        public void Clear()
        {
            openList.Clear();
            openDict.Clear();
        }
    }

    //封闭表
    class CloseNodeList
    {
        public List<NodeInfo> closeList;
        private Dictionary<string, NodeInfo> closeDict;

        public CloseNodeList()
        {
            closeList = new List<NodeInfo>();
            closeDict = new Dictionary<string, NodeInfo>();
        }

        public void AddNode(NodeInfo node)
        {
            if (IsAlreadyInCloseList(node))
            {
                return;
            }
            closeList.Add(node);
            closeDict.Add(node.toString, node);
        }

        public bool IsAlreadyInCloseList(NodeInfo node)
        {
            return closeDict.ContainsKey(node.toString);
        }

        public bool IsAlreadyInCloseList(int gridX, int gridY)
        {
            return closeDict.ContainsKey(gridX + "|" + gridY);
        }

        public NodeInfo GetCloseNodeByGrid(int gridX, int gridY)
        {
            return closeDict[gridX + "|" + gridY];
        }

        public void Clear()
        {
            closeList.Clear();
            closeDict.Clear();
        }
    }

    class NodeManager
    {
        private int startGridX;
        private int startGridY;
        private int endGridX;
        private int endGridY;

        public NodeInfo startNode;
        public NodeInfo endNode;

        public CloseNodeList closeNodeList;
        public OpenNodeList openNodeList;
        //阻挡块
        public List<string> blockList;
        public List<Point> resultList;

        private bool isCalcDiagonal;

        private NodeInfo currentNode;

        private static NodeManager nodeManager;

        public static NodeManager instance
        {
            get
            {
                if (nodeManager == null)
                {
                    nodeManager = new NodeManager();
                }
                return nodeManager;
            }
        }

        private NodeManager()
        {
            resultList = new List<Point>();
        }


        public NodeInfo CreateNode(int gridX, int gridY, NodeInfo parentNode = null)
        {
            int parentGridX = 0;
            int parentGridY = 0;
            int parentGValue = 0;
            if (parentNode != null)
            {
                parentGridX = parentNode.gridX;
                parentGridY = parentNode.gridY;
                parentGValue = parentNode.GValue;
            }

            NodeInfo newNode = new NodeInfo();
            newNode.gridX = gridX;
            newNode.gridY = gridY;
            newNode.direction = GetDirectionRelativeParent(gridX, gridY, parentGridX, parentGridY);
            newNode.parentNode = parentNode;
            newNode.GValue = CalculateGValue(newNode.direction, parentGValue);
            newNode.HValue = CalculateHValue(gridX, gridY);
            return newNode;
        }

        public void Init(int startGridX, int startGridY, int endGridX, int endGridY, List<string> block, bool isCalcDiagonal)
        {
            this.startGridX = startGridX;
            this.startGridY = startGridY;
            this.endGridX = endGridX;
            this.endGridY = endGridY;
            this.blockList = block;
            this.isCalcDiagonal = isCalcDiagonal;

            closeNodeList = new CloseNodeList();
            openNodeList = new OpenNodeList();
            resultList.Clear();

            //创建startNode
            startNode = CreateNode(startGridX, startGridY);
            //把第一个node 放入开放表
            openNodeList.AddNode(startNode);
        }

        public int CalculateHValue(NodeInfo node)
        {
            //采用Manhattan
            return CalculateHValue(node.gridX, node.gridY);
        }

        public int CalculateHValue(int gridX, int gridY)
        {
            //到达结束点，H直接等0，方便结束
            if (gridX == endGridX && gridY == endGridY)
            {
                return 0;
            }
            return (Math.Abs(endGridX - gridX) + Math.Abs(endGridY - gridY)) * 10;
        }

        public int CalculateGValue(NodeInfo node)
        {
            return CalculateGValue(node.direction, node.parentNode.GValue);
        }

        public int CalculateGValue(DirectionRelativeParent direction, int parentGValue)
        {
            if (direction == DirectionRelativeParent.END)
            {
                return 0;
            }
            return GetDirectionRelativeValue(direction) + parentGValue;
        }

        public int GetDirectionRelativeValue(DirectionRelativeParent direction)
        {
            //斜对角√2 * 10 直线 1 * 10  
            switch (direction)
            {
                case DirectionRelativeParent.UP:
                case DirectionRelativeParent.DOWN:
                case DirectionRelativeParent.LEFT:
                case DirectionRelativeParent.RIGHT:
                    return 10;
                case DirectionRelativeParent.LEFT_UP:
                case DirectionRelativeParent.LEFT_DOWN:
                case DirectionRelativeParent.RIGHT_UP:
                case DirectionRelativeParent.RIGHT_DOWN:
                    return 14;
                case DirectionRelativeParent.START:
                case DirectionRelativeParent.END:
                    return 0;
                default:
                    return 0;
            }
        }

        public DirectionRelativeParent GetDirectionRelativeParent(int gridX, int gridY, int parentGridX, int parentGridY)
        {
            int diffX = gridX - parentGridX;
            int diffY = gridY - parentGridY;

            if (gridX == startGridX && gridY == startGridY) return DirectionRelativeParent.START;

            if (gridX == endGridX && gridY == endGridY) return DirectionRelativeParent.END;

            if (diffX == 0 && diffY > 0) return DirectionRelativeParent.UP;

            if (diffX == 0 && diffY < 0) return DirectionRelativeParent.DOWN;

            if (diffX > 0 && diffY > 0) return DirectionRelativeParent.RIGHT_UP;

            if (diffX > 0 && diffY < 0) return DirectionRelativeParent.RIGHT_DOWN;

            if (diffX > 0 && diffY == 0) return DirectionRelativeParent.RIGHT;

            if (diffX < 0 && diffY > 0) return DirectionRelativeParent.LEFT_UP;

            if (diffX < 0 && diffY < 0) return DirectionRelativeParent.LEFT_DOWN;

            if (diffX < 0 && diffY == 0) return DirectionRelativeParent.LEFT;

            return DirectionRelativeParent.UNKNOWN;
        }

        public bool IsBlock(int gridX, int gridY)
        {
            return blockList.Contains(gridX + "|" + gridY);
        }

        public Point GetOffsetValueByDirection(DirectionRelativeParent direction)
        {
            switch (direction)
            {
                case DirectionRelativeParent.UP:
                    return new Point(0, 1);
                case DirectionRelativeParent.DOWN:
                    return new Point(0, -1);
                case DirectionRelativeParent.LEFT:
                    return new Point(-1, 0);
                case DirectionRelativeParent.RIGHT:
                    return new Point(1, 0);
                case DirectionRelativeParent.LEFT_UP:
                    return new Point(-1, 1);
                case DirectionRelativeParent.LEFT_DOWN:
                    return new Point(-1, -1);
                case DirectionRelativeParent.RIGHT_UP:
                    return new Point(1, 1);
                case DirectionRelativeParent.RIGHT_DOWN:
                    return new Point(1, -1);
                case DirectionRelativeParent.START:
                case DirectionRelativeParent.END:
                case DirectionRelativeParent.UNKNOWN:
                default:
                    return new Point(0, 0);
            }
        }

        public void GenerateRoundNode(NodeInfo parentNode)
        {
            //直线
            for (DirectionRelativeParent i = DirectionRelativeParent.UP; i <= DirectionRelativeParent.RIGHT; i++)
            {
                CreateNodeByDirection(i, parentNode);
            }

            //对角线
            if (isCalcDiagonal)
            {
                for (DirectionRelativeParent i = DirectionRelativeParent.LEFT_UP; i <= DirectionRelativeParent.RIGHT_DOWN; i++)
                {
                    CreateNodeByDirection(i, parentNode);
                }
            }
            //排序
            openNodeList.SortOpenList();
        }

        public void CreateNodeByDirection(DirectionRelativeParent direction, NodeInfo parentNode)
        {
            Point offset = GetOffsetValueByDirection(direction);
            int girdX = parentNode.gridX + offset.x;
            int gridY = parentNode.gridY + offset.y;

            //如果是障碍物，则不创建
            if (IsBlock(girdX, gridY)) return;

            //创建并加入Open列表
            openNodeList.AddNode(CreateNode(girdX, gridY, parentNode));
        }

        public void FindWay()
        {
            currentNode = startNode;

            while (currentNode.direction != DirectionRelativeParent.END)
            {
                //生成周围节点
                GenerateRoundNode(currentNode);
                //把当前节点从openlist删除
                openNodeList.RemoveNode(currentNode);
                //把当前节点加入closelis
                closeNodeList.AddNode(currentNode);
                //选取F值最小的节点
                currentNode = openNodeList.GetMinFValueNode();

                //printClose(closeNodeList.closeList);
            };

            //把结束的点 放入封闭表
            closeNodeList.AddNode(currentNode);
            //找出可用路径
            GenerateAvailableWay();
            //寻路结束
            Clear();
        }

        private void GenerateAvailableWay()
        {
            resultList.Clear();
            //找出可用路径
            NodeInfo tempNode = closeNodeList.GetCloseNodeByGrid(endGridX, endGridY);
            while (tempNode.direction != DirectionRelativeParent.START)
            {
                resultList.Add(new Point(tempNode.gridX, tempNode.gridY));
                tempNode = tempNode.parentNode;
            }
            resultList.Add(new Point(tempNode.gridX, tempNode.gridY));
        }

        public List<Point> GetAvailableWay()
        {
            return resultList;
        }

        public void Clear()
        {
            closeNodeList.Clear();
            openNodeList.Clear();
            closeNodeList = null;
            openNodeList = null;
        }
    }

    //A星算法
    public class AStarArithmetic
    {
        private static AStarArithmetic aStar;
        public static AStarArithmetic instance
        {
            get
            {
                if (aStar == null)
                {
                    aStar = new AStarArithmetic();
                }
                return aStar;
            }
        }

        private AStarArithmetic()
        {
            //Do Nothing
        }

        /// <summary>
        /// 初始化寻路参数
        /// </summary>
        /// <param name="startGridX"> 起点X </param>
        /// <param name="startGridY"> 起点Y </param>
        /// <param name="endGridX"> 终点X </param>
        /// <param name="endGridY"> 终点Y </param>
        /// <param name="block"> 障碍物位置列表（x|y） </param>
        /// <param name="isCalcDiagonal"> 是否计算对角线 </param>
        public void Init(int startGridX, int startGridY, int endGridX, int endGridY, List<string> block, bool isCalcDiagonal = true)
        {
            NodeManager.instance.Init(startGridX, startGridY, endGridX, endGridY, block, isCalcDiagonal);
        }
        /// <summary>
        /// 开始寻路
        /// </summary>
        public void StartFindWay()
        {
            NodeManager.instance.FindWay();
        }
        /// <summary>
        /// 获取可用路径
        /// </summary>
        /// <returns></returns>
        public List<Point> GetAvailableWay()
        {
            return NodeManager.instance.GetAvailableWay();
        }

        public List<NodeInfo> GetClose()
        {
            return NodeManager.instance.closeNodeList.closeList;
        }
    }
}