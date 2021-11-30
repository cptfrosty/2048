//Префаб - это готоый игровой объект, 
//который был собран из компонентов и других игровых объектов

//Библиотека ассета, который был скачан с AssetStore для плавной анимации перемещения ячеек
using DG.Tweening;
using System.Collections.Generic; //Для структуры данных List
using System.Linq; //Для запросов к листу в формате Linq (похожий на SQL)
using UnityEngine; //Пространство имён в котором содержится MonoBehaviour и т.п
using UnityEngine.SceneManagement; //Для управления сценами

//Игровой менеджер, в котором происходит основная логика игры
public class GameManager : MonoBehaviour
{
    [SerializeField] private int _width = 4; //Размер поля в ширину
    [SerializeField] private int _height = 4; //Размер поля в высоту
    [SerializeField] private Node _nodePrefab; //Префаб ноды
    [SerializeField] private Block _blockPrefab; //Префаб ячейки
    //Префаб доски (взависимости от размера поля, она растягивается)
    [SerializeField] private SpriteRenderer _boardPrefab;
    //Типы блоков: 2, 4, 8, 16 ... 2048 так же тут указывается не только значение но и цвет
    [SerializeField] private List<BlockType> _types;
    [SerializeField] private float _travelTime = 0.2f; //Время перемещения от ячейки
    [SerializeField] private int _winCondition = 2048; //Максимальное значение для победы

    //Ссылка на игровой объект _winScreen - экран победы _loseScreen - экран поражения
    [SerializeField] private GameObject _winScreen, _loseScreen;

    private List<Node> _nodes; //Все ноды, которые присутствуют на сцене (для контролирования всех нодов на сцене)
    private List<Block> _blocks; //Все ячейки, которые присутствуют на сцене (для контролирования всех ячеек на сцене)
    private GameState _state; //Состояние игры
    private int _round; //Раунд игры

    //Получить блок со значением value. 
    // _types.First(t => t.Value == value) - это запрос Linq он означает
    // из всех блоков _types найти элемент соответствующий значению value
    // и вернуть самый первый найденый элемент.
    // Примечание: По сути этот метод используется только один раз, для поиска
    // наибольшего значения, а именно 2048. Если 2048 было найдено, 
    //то тогда вернётся результат
    private BlockType GetBlockTypeValue(int value) => _types.First(t => t.Value == value);

    //Метод старт от базового класса MonoBehaviour
    //Он срабатывает когда игра только запускается и скрипт на объекте был включен
    private void Start()
    {
        //Вызвать метод ChangeState - изменение состояния игры
        //и установить состояние игры - генерация уровня
        ChangeState(GameState.GenerateLevel);
    }

    /// <summary>
    /// Используется для изменения состояния игры
    /// </summary>
    /// <param name="newState">Состояние игры, которое нужно установить</param>
    private void ChangeState(GameState newState)
    {
        //Изменить состояние игры
        _state = newState;

        //Выполнить изменение состояния игры
        switch (newState)
        {
            case GameState.GenerateLevel: //Если состояние - генерация уровня
                GenerateGrid(); //Вызвать метод генерации уровня
                break; //Прервать switch и пойти дальше
            case GameState.SpawningBlocks: //Если состояние - спавн (создание) блоков
                //_round++ == 0 ? 2 : 1 - в первом раунде появится 2 ячейки
                // последующие будет появлятся 1 ячейка
                // тут сравнивается, (_round++ == 0) если значение
                // _round равно 0, то в SpawnBlocks передать значение 2, 
                // иначе передать значение 1. После значение _round увеличится на 1.
                SpawnBlocks(_round++ == 0 ? 2 : 1); //Вызвать метод создания блоков 
                break; //Прервать switch и пойти дальше
            case GameState.WaitingInput: //Если состояние - ожидать действий (ввод) игрока
                break; //Прервать switch и пойти дальше
            case GameState.Moving: //Если состояние - перемещение ячеек
                break; //Прервать switch и пойти дальше
            case GameState.Win: //Если состояние ПОБЕДА
                _winScreen.SetActive(true);
                break; //Прервать switch и пойти дальше
            case GameState.Lose: //Если состояние ПОРАЖЕНИЕ
                _loseScreen.SetActive(true);
                break; //Прервать switch и пойти дальше
            default: //Если ниодно условие не подходит в case
                //вызвать ошибку выход за приделы диапазона значений
                throw new System.ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }

    //Метод Update, принадлежит классу MonoBehaviour
    //Срабатывает 30 раз за 1 кадр.
    private void Update()
    {
        //Если состояние игры - ожидание ввода от игрока
        //то (return) - не выполнять код дальше
        if (_state != GameState.WaitingInput) return;

        //Если пользователь нажал кнопку ЛЕВАЯ СТРЕЛКА
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            Shift(Vector2.left); //то передвинуть ячейки влево (Vector2.left - x:-1; y:0)
        if (Input.GetKeyDown(KeyCode.RightArrow)) //ПРАВАЯ СТРЕЛКА
            Shift(Vector2.right); //то передвинуть ячейки влево (Vector2.right - x:1; y:0)
        if (Input.GetKeyDown(KeyCode.UpArrow)) //СТРЕЛКА ВВЕРХ
            Shift(Vector2.up); //то передвинуть ячейки влево (Vector2.up - x:0; y:1)
        if (Input.GetKeyDown(KeyCode.DownArrow)) //СТРЕЛКА ВНИЗ
            Shift(Vector2.down);//то передвинуть ячейки влево (Vector2.down - x:0; y:-1)
    }

    /// <summary>
    /// Генерация сетки
    /// </summary>
    private void GenerateGrid()
    {
        _round = 0; //Раунд равен 0
        _nodes = new List<Node>(); //Создать новый список нодов
        _blocks = new List<Block>(); //Создать новый список блоков

        //Расставление нодов *как работа с двумерным массивом*
        for (int x = 0; x < _width; x++) //По ширине
        {
            for (int y = 0; y < _height; y++) //По высоте
            {
                //var - это неопределённый тип данных. 
                //То что вернётся слевой стороны, такой тип данных будет справой стороны
                //Instantiate - создание игрового объекта на сцене
                //Instantiate(игровой_объект, позиция_объета, поворот_объекта)
                var node = Instantiate(_nodePrefab, new Vector2(x, y), Quaternion.identity);
                _nodes.Add(node); //Добавить ноду в список нодов список.Add(экземпляр_класса_Node).
            }
        }

        //Определить центр, чтобы поместить туда поле (задний фон поля)
        var center = new Vector2((float)_width / 2 - 0.5f, (float)_height / 2 - 0.5f);

        //Создание поля
        var board = Instantiate(_boardPrefab, center, Quaternion.identity);
        board.size = new Vector2(_width, _height); //Выставление размеров полю

        //Установить позицию главной камеры в позициях по X: center.x, по Y: center.y, по Z:-10
        Camera.main.transform.position = new Vector3(center.x, center.y, -10);

        ChangeState(GameState.SpawningBlocks);//Изменить состояние игры на спавн блоков (ячеек)
    }

    /// <summary>
    /// Спавн блоков (ячеек)
    /// </summary>
    /// <param name="amount">Кол-во</param>
    private void SpawnBlocks(int amount)
    {
        //_nodes.Where(n => n.OccupiedBlock == null).OrderBy(b => Random.value).ToList() - ...
        // ... из списка (_nodes) выбрать (Where) игровой объект где не находится ячейка (OccupiedBlock == null)
        // отсортировать (OrderBy) в случайном порядке (Random.value) и конвертировать в тип данных List<> (.ToList())
        var freeNodes = _nodes.Where(n => n.OccupiedBlock == null).OrderBy(b => Random.value).ToList();

        //*foreach - это цикл, похожий на for*
        //freeNodes.Take(amount) из свободных нодов (freeNodes) 
        //взять (Take) элементы в кол-ве amount штук.
        //и поместить ссылку на этот объект в node
        foreach (var node in freeNodes.Take(amount))
        {
            //Вызвать метод спавн(появления) блоков.
            //Передача параметров:
            //node - на которой будет заспавнена ячейка
            //Random.value - генерирует значение от 0 до 1.
            //Random.value > 0.8f ? 4 : 2 - если значение больше чем 0.8
            //то передать значение 4
            //иначе передать значение 2
            SpawnBlock(node, Random.value > 0.8f ? 4 : 2);
        }

        //Если свободная ячейка осталась 1
        //*обычно на ней и спавнится последняя ячейка и кол-во свободных ячеек становится 0*
        if (freeNodes.Count() == 1)
        {
            //то изменить статус игры ПОРАЖЕНИЕ
            ChangeState(GameState.Lose);
            return; //Завершить выполнение метода и не выполнять код дальше после if
        }

        //Если всё же свободных ячеек много, то изменить статус игры:
        // _blocks.Any(b=>b.Value == _winCondition) - LINQ запрос. Если хоть один (Any) 
        // в списке блоков (_blocks) достиг значения (_winCondition), то установить статус игры ПОБЕДА
        // иначе установить статус игры ожидания ввода от игрока
        ChangeState(_blocks.Any(b => b.Value == _winCondition) ? GameState.Win : GameState.WaitingInput);
    }

    /// <summary>
    /// Спавн блока (ячейки)
    /// </summary>
    /// <param name="node">Нода, куда установить ячейку</param>
    /// <param name="value">Значение для ячейки</param>
    private void SpawnBlock(Node node, int value)
    {
        //Заспавнить префаб блока
        var block = Instantiate(_blockPrefab, node.Pos, Quaternion.identity);
        //Вызвать метод инициализации блока, 
        //но перед этим вызвать метод получение информации о блоке GetBlockTypeValue(value)
        block.Init(GetBlockTypeValue(value));
        block.SetBlock(node); //Установить блок в node
        _blocks.Add(block); //Добавить ячейку в список ячеек на сцене
    }

    /// <summary>
    /// Перемещение ячейки
    /// </summary>
    /// <param name="dir">Вектор (направление) перемещения</param>
    void Shift(Vector2 dir)
    {
        //Установить статус перемещения
        ChangeState(GameState.Moving);
        //В листе _blocks (ячеек) отсортировать данные по позиции X, потом по позиции Y, преобразовать в лист
        //и вернуть результат в orderedBlocks
        var orderedBlocks = _blocks.OrderBy(b => b.Pos.x).ThenBy(b => b.Pos.y).ToList();
        //Если направление равняется x:1,y:0 или x:0,y:1, то изменить список на обратную последовательность
        if (dir == Vector2.right || dir == Vector2.up) orderedBlocks.Reverse();

        //Перебор всех ячеек в orderedBlocks
        foreach (var block in orderedBlocks)
        {
            //Получить ноду ячейки
            var next = block.Node;
            do
            {
                //Установить новую ноду для ячейки
                block.SetBlock(next);
                var possibleNode = GetNodeAtPosition(next.Pos + dir);
                //Если возможный блок найден
                if (possibleNode != null)
                {
                    //Если у possibleNode(возможной ноды) есть ячейка и возможно совместить с ячейкой (block) 
                    if (possibleNode.OccupiedBlock != null && possibleNode.OccupiedBlock.CanMerge(block.Value))
                    {
                        //То соединить эти две ячейки
                        block.MergeBlock(possibleNode.OccupiedBlock);
                    }
                    //иначе если possibleNode(возможный нод) не имеет блока...
                    else if (possibleNode.OccupiedBlock == null)
                    {
                        //...то переходим к этой ноде
                        next = possibleNode;
                    }
                }
            } while (next != block.Node); //Повторять цикл пока нода не будет равна не ноде в ячейке
        }

        //Создать анимацию перемещения
        var sequence = DOTween.Sequence();

        //Перебор всех ячеек в orderedBlocks
        foreach (var block in orderedBlocks)
        {
            //Анимация перемещения
            //DOMove(block.Node.Pos, _travelTime) - сама анимация перемещения
            sequence.Insert(0, block.transform.DOMove(block.Node.Pos, _travelTime));
        }

        //По завершению перемещения (OnComplete) выполнить =>
        sequence.OnComplete(() =>
        {
            //Пройтись в списке по ячейкам, которые имеют ячейку для слияния
            foreach (var block in orderedBlocks.Where(b => b.MergingBlock != null))
            {
                //Вызвать метод слияния ячеек
                //block.MergingBlock - ячейка которая будет сливаться с другой
                //block - ячейка которая будет сливаться с block.MergingBlock 
                MergeBlocks(block.MergingBlock, block);
            }
            //Изменить статус игры на спавн блока(ов)
            ChangeState(GameState.SpawningBlocks);
        });
    }

    /// <summary>
    /// Слияние двух ячеек
    /// </summary>
    /// <param name="baseBlock">базовая ячейка</param>
    /// <param name="mergingBlock">ячейка, которая сливается</param>
    private void MergeBlocks(Block baseBlock, Block mergingBlock)
    {
        //Заспавнить блок в ноде базового блока со значением VALUE * 2
        //т.е значение будет увеличено на 2
        SpawnBlock(baseBlock.Node, baseBlock.Value * 2);

        //Удалить базовую ячейку
        RemoveBlock(baseBlock);
        //Удалить ячейку слияния
        RemoveBlock(mergingBlock);
    }

    /// <summary>
    /// Удаление блока
    /// </summary>
    /// <param name="block">блок, который нужно удалить</param>
    private void RemoveBlock(Block block)
    {
        //Удалить ячейку из списка
        _blocks.Remove(block);
        //Удалить ячейку
        Destroy(block.gameObject);
    }

    /// <summary>
    /// Получить ноду, которая находится на этой позиции
    /// </summary>
    /// <param name="pos">проверяемая позиция</param>
    /// <returns></returns>
    private Node GetNodeAtPosition(Vector2 pos)
    {
        //Вернуть первое совпадение по позиции или NULL
        return _nodes.FirstOrDefault(n => n.Pos == pos);
    }

    //Перезагрузить игру
    private void RestartGame()
    {
        //SceneManager.GetActiveScene().name - получение текущей сцены по имени (названию)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

//Атрибут [System.Serializable] тут необходим, чтобы тип данных
//BlockType отображался в инспекторе
[System.Serializable]
public struct BlockType
{
    public int Value; //Значение
    public Color color; //Цвет
}

//Состояния игры
//enum - Тип перечисления
public enum GameState
{
    GenerateLevel,  //Генерация уровня
    SpawningBlocks, //Спавн блоков
    WaitingInput,   //Ожидания ввода
    Moving,         //Перемещение
    Win,            //Победа
    Lose,           //Поражение

}