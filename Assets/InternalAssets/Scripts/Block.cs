//MonoBehaviour - это базовый класс Unity от которого 
//наследуются все скрипты. По этому эти скрипты можно поместить 
//на игровые объекты

//Библиотека для работы с продвинутым текстом TextMeshPro
using TMPro;
//Пространство имён в котором содержится MonoBehaviour
//Vector2, SpriteRenderer
using UnityEngine;

/* Класс блок назначается на игровые объекты "Ячейка".
 * Класс блок содержит в себе информацию:
 * об значении ячейки; о ноде где расположена ячейка;
 * ссылку на блок с которым должно произойти слияние;
 * процесс слияния (Merging) это нужно, чтобы случайно
 * три ячейки не слились в одну. Если 3 ячейки со значением 2
 * сольются в одну, то получится ячейка с номером 4, а значение
 * 2 потеряется.
 */
public class Block : MonoBehaviour
{
    public int Value; //Значение ячейки
    public Node Node; //Нода на которой находится ячейка
    public Block MergingBlock; //Блок с которым должно произойти слияние
    public bool Merging; //Происходит слияние или нет

    //Позиция игрового объекта 
    //на котором находится экземпляр класса Block
    public Vector2 Pos => transform.position;

    //[SerializeField] для того, чтобы приватное поле отображалось 
    //в игровом инспекторе. Через инспектор можно увидеть эти поля.
    //SpriteRenderer отвечает за отображение спрайта (картинки)
    [SerializeField] private SpriteRenderer _renderer; //Ссылка на компонент SpriteRenderer
    //Текст в котором отображается значение ячейки
    [SerializeField] private TextMeshPro _text;

    //Задать начальные значения для ячейки
    public void Init(BlockType type)
    {
        //значение
        Value = type.Value;
        //цвет
        _renderer.color = type.color;
        //значение в тип данных string
        _text.text = type.Value.ToString();
    }

    /// <summary>
    /// Установить новую ноду для ячейки
    /// </summary>
    /// <param name="node">Новая нода для ячейки</param>
    public void SetBlock(Node node)
    {
        //Если есть значение ноды, то очистить указанный нод от ячейки
        //Нужно для того, когда ячейка перемещается в другую ноду
        if (Node != null) Node.OccupiedBlock = null;
        //Для ячейки назначить новую ноду
        Node = node;
        //В Node теперь новая ячейка и в этой новой ячейке 
        //указывается ЭТА ячейка (на которой установлен блок)
        Node.OccupiedBlock = this;
    }

    /// <summary>
    /// Соединение двух блоков
    /// </summary>
    /// <param name="blockToMergeWith">блок для соединения</param>
    public void MergeBlock(Block blockToMergeWith)
    {
        //Назначается блок для соединения
        MergingBlock = blockToMergeWith;
        //Нода на которой стояла ЭТА ячейка становится не занятой
        Node.OccupiedBlock = null;

        //Указать, то что сейчас происходит слияние двух блоков
        blockToMergeWith.Merging = true;
    }

    /// <summary>
    /// Узнать можно ли соединить эти две ячейки
    /// </summary>
    /// <param name="value">Значение с которой проверяется слияние</param>
    /// <returns>Возможность слияния</returns>
    public bool CanMerge(int value) => value == Value && !Merging && MergingBlock == null;
}
