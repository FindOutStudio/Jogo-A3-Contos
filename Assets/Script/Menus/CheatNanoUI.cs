using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CheatNanoUI : MonoBehaviour, IPointerClickHandler
{
    [Header("Conexões com os Menus")]
    public LevelSelector seletorDeFases;
    public MenuDeLogs menuDeLogs;

    [Header("Configuração do Cheat")]
    public float tempoMaximoEntreCliques = 0.5f;
    
    private int contagemCliques = 0;
    private float tempoUltimoClique = 0f;
    
    private Image imagemNano; 

    private void Awake()
    {
        imagemNano = GetComponent<Image>();
    }

    private void Update()
    {
        bool painelFasesLigado = seletorDeFases != null && seletorDeFases.telaSelecaoLevel != null && seletorDeFases.telaSelecaoLevel.activeSelf;
        bool painelLogsLigado = menuDeLogs != null && menuDeLogs.telaGradeLogs != null && menuDeLogs.telaGradeLogs.activeSelf;

        if (imagemNano != null)
        {
            imagemNano.raycastTarget = painelFasesLigado || painelLogsLigado;
        }

        if (Time.unscaledTime - tempoUltimoClique > tempoMaximoEntreCliques)
        {
            contagemCliques = 0;
        }

        // ==========================================
        // ATALHO DIRETO DO TECLADO (Sem precisar clicar no Nano)
        // ==========================================
        if (painelFasesLigado || painelLogsLigado)
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                ExecutarCheat(true); // Destranca tudo
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                ExecutarCheat(false); // Tranca (Reseta) tudo
            }
        }
    }

    // ==========================================
    // SISTEMA DOS 3 CLIQUES NA IMAGEM
    // ==========================================
    public void OnPointerClick(PointerEventData eventData)
    {
        contagemCliques++;
        tempoUltimoClique = Time.unscaledTime; 

        if (contagemCliques >= 3)
        {
            contagemCliques = 0;
            
            // Por padrão, os 3 cliques Destrancam as coisas. 
            // Mas se você estiver segurando a tecla R enquanto clica 3x, ele Tranca.
            bool querDestrancar = !Input.GetKey(KeyCode.R);
            ExecutarCheat(querDestrancar);
        }
    }

    private void ExecutarCheat(bool liberar)
    {
        bool painelFasesLigado = seletorDeFases != null && seletorDeFases.telaSelecaoLevel != null && seletorDeFases.telaSelecaoLevel.activeSelf;
        bool painelLogsLigado = menuDeLogs != null && menuDeLogs.telaGradeLogs != null && menuDeLogs.telaGradeLogs.activeSelf;

        if (!painelFasesLigado && !painelLogsLigado) return;

        if (SoundManager.instance != null && SoundManager.instance.uiSelecao != null) 
            SoundManager.instance.TocarSFX(SoundManager.instance.uiSelecao);

        int valorParaSalvar = liberar ? 1 : 0;

        // ======= CHEAT DAS FASES =======
        if (painelFasesLigado && seletorDeFases != null)
        {
            Debug.Log(liberar ? "🎮 CHEAT: Fases Desbloqueadas!" : "🗑️ RESET: Fases Trancadas!");
            
            for (int i = 1; i < seletorDeFases.fases.Length; i++)
            {
                PlayerPrefs.SetInt("FaseLiberada_" + i, valorParaSalvar);
            }
            seletorDeFases.GerarBotoesDeFase(); 
        }

        // ======= CHEAT DOS LOGS =======
        if (painelLogsLigado && menuDeLogs != null && menuDeLogs.todosOsLogs.Length > 0)
        {
            Debug.Log(liberar ? "📜 CHEAT: Logs Desbloqueados!" : "🗑️ RESET: Logs Trancados!");
            
            for (int i = 0; i < menuDeLogs.todosOsLogs.Length; i++)
            {
                PlayerPrefs.SetInt("LogColetado_" + menuDeLogs.todosOsLogs[i].logID, valorParaSalvar);
            }

            // Garante que o Easter Egg do Log 6 e 7 fique engatilhado quando você destranca tudo!
            if (liberar)
            {
                PlayerPrefs.SetInt("MemeSixSevenVisto", 0);
            }
            else 
            {
                PlayerPrefs.SetInt("MemeSixSevenVisto", 1); 
            }

            PlayerPrefs.Save();
            menuDeLogs.GerarBotoesDeLog(); 
        }
    }
}