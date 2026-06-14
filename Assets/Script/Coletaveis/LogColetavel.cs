using UnityEngine;
using System.Collections.Generic;

// O bloquinho que vai aparecer no Inspector: só a Fala e a Cor
[System.Serializable]
public class LinhaDeDialogo
{
    [TextArea(2, 4)]
    public string fala;
    public Color corDaFala = Color.white;
}

[RequireComponent(typeof(Collider2D))]
public class LogColetavel : MonoBehaviour
{
    [Header("Dados do Log")]
    public int logID;
    
    [Header("Efeito Fantasma (Estilo Celeste)")]
    [Tooltip("Transparência do log caso já tenha sido coletado antes (0 = invisível, 1 = normal)")]
    [Range(0f, 1f)] public float opacidadeFantasma = 0.4f;
    
    [Header("Bate-Papo (Adicione as linhas no +)")]
    public List<LinhaDeDialogo> batePapo;

    private void Start()
    {
        // O Segredo: Assim que a fase carregar, ele verifica no save se já foi pego
        if (PlayerPrefs.GetInt("LogColetado_" + logID, 0) == 1)
        {
            // Se já pegou, pega o componente de imagem e deixa ele meio "apagado"
            SpriteRenderer meuSprite = GetComponent<SpriteRenderer>();
            if (meuSprite != null)
            {
                Color corAtual = meuSprite.color;
                corAtual.a = opacidadeFantasma; // Aplica a opacidade (ex: 40%)
                meuSprite.color = corAtual;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Coletar();
        }
    }

    private void Coletar()
    {
        if (SoundManager.instance != null) SoundManager.instance.TocarLog();
        
        PlayerPrefs.SetInt("LogColetado_" + logID, 1);
        PlayerPrefs.Save();

        // Envia o textão montado para o UI Manager
        if (LogUIManager.instance != null)
        {
            string textoPronto = MontarConversa();
            LogUIManager.instance.MostrarLog(logID, textoPronto);
        }

        Destroy(gameObject);
    }

    // A mágica acontece aqui: ele pega a lista e transforma num textão só com as cores embutidas
    private string MontarConversa()
    {
        string textoFinal = "";

        for (int i = 0; i < batePapo.Count; i++)
        {
            // O "RGB" no final garante que a Unity não deixe seu texto invisível por causa do Alpha!
            string corHex = ColorUtility.ToHtmlStringRGB(batePapo[i].corDaFala);

            // Coloca a cor em volta da frase atual
            textoFinal += $"<color=#{corHex}>{batePapo[i].fala}</color>";

            // Pula de linha para a resposta da outra pessoa ficar embaixo (exceto a última)
            if (i < batePapo.Count - 1)
            {
                textoFinal += "\n\n";
            }
        }

        return textoFinal;
    }
}