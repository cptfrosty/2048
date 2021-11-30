using UnityEngine; //Пространство имён в котором содержится MonoBehaviour

//Нода (квадратное поле под ячейкой)
public class Node : MonoBehaviour
{
    //Позиция ноды
    public Vector2 Pos => transform.position;
    //Ячейка, которая находится в ноде
    //Если в ноде не указан объект, значит в нём нет ячейки
    public Block OccupiedBlock;
}
