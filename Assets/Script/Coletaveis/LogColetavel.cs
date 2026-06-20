using UnityEngine;
using System.Collections.Generic;

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
        if (PlayerPrefs.GetInt("LogColetado_" + logID, 0) == 1)
        {
            SpriteRenderer meuSprite = GetComponent<SpriteRenderer>();
            if (meuSprite != null)
            {
                Color corAtual = meuSprite.color;
                corAtual.a = opacidadeFantasma;
                meuSprite.color = corAtual;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Coletar(collision.gameObject);
        }
    }

    private void Coletar(GameObject jogador)
    {
        if (SoundManager.instance != null) SoundManager.instance.TocarLog();
        
        PlayerPrefs.SetInt("LogColetado_" + logID, 1);
        PlayerPrefs.Save();

        // Envia o texto montado para o UI Manager
        if (LogUIManager.instance != null)
        {
            string textoPronto = MontarConversa();
            LogUIManager.instance.MostrarLog(logID, textoPronto);
        }

        // === CORREÇÃO DO LANÇAMENTO ===
        // Comunica com o teu PlayerController para cortar o elástico e esconder a linha
        if (jogador != null)
        {
            // Força a execução das funções de reset de arrasto no jogador
            jogador.SendMessage("CancelarArrasto", SendMessageOptions.DontRequireReceiver);
            jogador.SendMessage("ResetarLancamento", SendMessageOptions.DontRequireReceiver);
            
            // Segurança extra: Esconde a linha visual do rastro caso o rato/touch fique preso
            TrailRenderer rastro = jogador.GetComponentInChildren<TrailRenderer>();
            if (rastro != null) rastro.emitting = false;
            
            LineRenderer linha = jogador.GetComponentInChildren<LineRenderer>();
            if (linha != null) linha.enabled = false;
        }

        Destroy(gameObject);
    }

    private string MontarConversa()
    {
        string textoFinal = "";

        for (int i = 0; i < batePapo.Count; i++)
        {
            string corHex = ColorUtility.ToHtmlStringRGB(batePapo[i].corDaFala);
            textoFinal += $"<color=#{corHex}>{batePapo[i].fala}</color>";

            if (i < batePapo.Count - 1)
            {
                textoFinal += "\n\n";
            }
        }

        return textoFinal;
    }
}