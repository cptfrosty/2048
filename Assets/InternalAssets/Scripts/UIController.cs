using UnityEngine; //Пространство имён в котором содержится MonoBehaviour
using UnityEngine.SceneManagement; //Библиотека для управления сценами

//Класс для управление UI событиями с кнопок
public class UIController : MonoBehaviour
{
    //Загрузка сцены
    //nameScene - название сцены для загрузки
    public void LoadScene(string nameScene)
    {
        //Обращение к пространству имён UnityEngine.SceneManagement 
        //для загрузки сцены
        SceneManager.LoadScene(nameScene);
    }

    //Перезапустить игру
    public void RestartGame()
    {
        //SceneManager.GetActiveScene().name - получение текущей сцены по имени (названию)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
