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
    
    [Header("Bate-Papo (Adicione as linhas no +)")]
    public List<LinhaDeDialogo> batePapo;

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

            // Pula de linha para a resposta da outra pessoa ficar embaixo (exceto na última frase)
            if (i < batePapo.Count - 1)
            {
                textoFinal += "\n\n"; // Se quiser que fique mais juntinho, deixe apenas um "\n"
            }
        }

        return textoFinal;
    }
}