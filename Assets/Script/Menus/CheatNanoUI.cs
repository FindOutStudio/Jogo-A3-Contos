using UnityEngine;
using UnityEngine.EventSystems;

public class CheatNanoUI : MonoBehaviour, IPointerClickHandler
{
    [Header("Conexões com os Menus")]
    public LevelSelector seletorDeFases;
    public MenuDeLogs menuDeLogs;

    [Header("Configuração do Cheat")]
    public float tempoMaximoEntreCliques = 0.5f;
    
    private int contagemCliques = 0;
    private float tempoUltimoClique = 0f;

    // ======= ATALHO DE TECLADO PARA TESTAR O CÓDIGO =======
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            ExecutarLogicaDoCheat();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Time.unscaledTime - tempoUltimoClique > tempoMaximoEntreCliques)
        {
            contagemCliques = 0; 
        }

        contagemCliques++;
        tempoUltimoClique = Time.unscaledTime;

        if (contagemCliques >= 3)
        {
            contagemCliques = 0; 
            ExecutarLogicaDoCheat();
        }
    }

    private void ExecutarLogicaDoCheat()
    {
        bool painelFasesLigado = seletorDeFases != null && seletorDeFases.telaSelecaoLevel != null && seletorDeFases.telaSelecaoLevel.activeSelf;
        bool painelLogsLigado = menuDeLogs != null && menuDeLogs.telaGradeLogs != null && menuDeLogs.telaGradeLogs.activeSelf;

        if (!painelFasesLigado && !painelLogsLigado) return;

        if (SoundManager.instance != null) SoundManager.instance.TocarSFX(SoundManager.instance.uiSelecao);

        // ======= CHEAT DAS FASES =======
        if (painelFasesLigado && seletorDeFases != null)
        {
            Debug.Log("🎮 CHEAT: Acionado no Painel de Fases!");
            
            bool temFaseTrancada = false;
            
            for (int i = 1; i < seletorDeFases.fases.Length; i++)
            {
                if (PlayerPrefs.GetInt("FaseLiberada_" + i, 0) == 0) temFaseTrancada = true;
            }

            int valorParaSalvar = temFaseTrancada ? 1 : 0;

            for (int i = 1; i < seletorDeFases.fases.Length; i++)
            {
                PlayerPrefs.SetInt("FaseLiberada_" + i, valorParaSalvar);
            }
            seletorDeFases.GerarBotoesDeFase(); 
        }

        // ======= CHEAT DOS LOGS =======
        if (painelLogsLigado && menuDeLogs != null && menuDeLogs.todosOsLogs.Length > 0)
        {
            Debug.Log("📜 CHEAT: Acionado no Painel de Logs!");
            
            bool estaTravado = PlayerPrefs.GetInt("LogColetado_" + menuDeLogs.todosOsLogs[0].logID, 0) == 0;
            int valorParaSalvar = estaTravado ? 1 : 0;

            foreach (var log in menuDeLogs.todosOsLogs)
            {
                PlayerPrefs.SetInt("LogColetado_" + log.logID, valorParaSalvar);
            }
            menuDeLogs.GerarBotoesDeLog(); 
        }

        PlayerPrefs.Save();
    }
}