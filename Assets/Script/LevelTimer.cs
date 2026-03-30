using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelTimer : MonoBehaviour
{
    public float tempoInicial = 60f;
    public TextMeshProUGUI textoTempo;
    private float tempoAtual;

    void Start()
    {
        tempoAtual = tempoInicial;
    }

    void Update()
    {
        if (tempoAtual > 0)
        {
            tempoAtual -= Time.deltaTime;
            textoTempo.text = Mathf.CeilToInt(tempoAtual).ToString();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}