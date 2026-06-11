using UnityEngine;
using System.Collections.Generic;

public class LevelEditorManager : MonoBehaviour
{
    public enum TipoFerramenta { Bloco, Serra, Laser, Raio, Borracha, Espinho }

    [Header("=== Estado Atual ===")]
    public TipoFerramenta ferramentaAtual = TipoFerramenta.Bloco;

    [Header("=== Prefabs dos Itens ===")]
    public GameObject prefabBloco;
    public GameObject prefabSerra;
    public GameObject prefabLaser;
    public GameObject prefabEspinho;
    public GameObject prefabRaio;

    [Header("=== Configurações de Câmera e Zoom ===")]
    public float zoomMinimoGameplay = 5f;
    public float velocidadeZoom = 10f;
    public float velocidadeArrasto = 1.2f;

    [Header("=== Configurações da Grade (Grid) ===")]
    public float tamanhoGrade = 1f; 
    public float limiteEsquerdo = -10f;
    public float limiteDireito = 10f;
    public float limiteInferior = -5f;
    public float limiteSuperior = 5f;
    public float compensacaoBorda = 0.5f;

    [Header("Visual Geral")]
    public GameObject fantasmaVisual;      
    private SpriteRenderer fantasmaRenderer;

    private Camera cam;
    private Vector2 posicaoSnapMouse; 
    private Vector3 ultimaPosicaoMouseJanela;
    private List<GameObject> objetosConstruidos = new List<GameObject>();

    private bool arrastandoDoisPontos = false;
    private Vector2 pontoInicialDrag;
    private LineRenderer linhaDeVisualizacao; 

    private void Awake()
    {
        linhaDeVisualizacao = gameObject.AddComponent<LineRenderer>();
        linhaDeVisualizacao.startWidth = 0.1f;
        linhaDeVisualizacao.endWidth = 0.1f;
        
        Shader defaultShader = Shader.Find("Sprites/Default");
        if (defaultShader != null) linhaDeVisualizacao.material = new Material(defaultShader);
        
        linhaDeVisualizacao.startColor = Color.red;
        linhaDeVisualizacao.endColor = Color.red;
        linhaDeVisualizacao.positionCount = 2;
        linhaDeVisualizacao.sortingOrder = 50; 
        linhaDeVisualizacao.enabled = false;
    }

    private void Start()
    {
        cam = Camera.main;
        if (cam != null)
        {
            cam.orthographicSize = zoomMinimoGameplay;
            float centroX = (limiteEsquerdo + limiteDireito) / 2f;
            float centroY = (limiteInferior + limiteSuperior) / 2f;
            cam.transform.position = new Vector3(centroX, centroY, -10f);
        }

        if (fantasmaVisual != null) fantasmaRenderer = fantasmaVisual.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return; 

        if (!Input.GetMouseButton(2) && !MouseEstaSobreUI())
        {
            AtualizarPosicaoMouse();
            MoverFantasma();
            ProcessarMecanicasDeConstrucao();
        }
        else
        {
            if (fantasmaVisual != null) fantasmaVisual.transform.position = new Vector3(9999f, 9999f, 0f);
            arrastandoDoisPontos = false;
            linhaDeVisualizacao.enabled = false;
        }

        ProcessarArrastoCamera();
        ProcessarZoomCamera();
        TravarCameraNosLimites();
    }

    private bool MouseEstaSobreUI()
    {
        if (UnityEngine.EventSystems.EventSystem.current == null) return false;
        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }

    private void ProcessarMecanicasDeConstrucao()
    {
        if (ferramentaAtual == TipoFerramenta.Borracha)
        {
            if (Input.GetMouseButton(0)) 
            {
                Collider2D colisorNoLocal = Physics2D.OverlapPoint(posicaoSnapMouse);
                if (colisorNoLocal != null && objetosConstruidos.Contains(colisorNoLocal.gameObject))
                {
                    objetosConstruidos.Remove(colisorNoLocal.gameObject);
                    Destroy(colisorNoLocal.gameObject);
                   
                }
            }
        }
        else if (ferramentaAtual == TipoFerramenta.Laser || ferramentaAtual == TipoFerramenta.Serra)
        {
            if (Input.GetMouseButtonDown(0))
            {
                arrastandoDoisPontos = true;
                pontoInicialDrag = posicaoSnapMouse;
                linhaDeVisualizacao.enabled = true;
            }

            if (arrastandoDoisPontos)
            {
                linhaDeVisualizacao.SetPosition(0, pontoInicialDrag);
                linhaDeVisualizacao.SetPosition(1, posicaoSnapMouse);
            }

            if (Input.GetMouseButtonUp(0) && arrastandoDoisPontos)
            {
                arrastandoDoisPontos = false;
                linhaDeVisualizacao.enabled = false;

                if (pontoInicialDrag != posicaoSnapMouse)
                {
                    ConstruirObjetoDeDoisPontos(pontoInicialDrag, posicaoSnapMouse);
                }
            }
        }
        else
        {
            if (Input.GetMouseButton(0)) 
            {
                Collider2D colisorNoLocal = Physics2D.OverlapPoint(posicaoSnapMouse);
                if (colisorNoLocal == null) 
                {
                    GameObject prefabAlvo = ObterPrefabDaFerramentaAtual();
                    if (prefabAlvo != null)
                    {
                        GameObject novoObjeto = Instantiate(prefabAlvo, posicaoSnapMouse, Quaternion.identity);
                        objetosConstruidos.Add(novoObjeto);
                    
                    }
                }
            }
        }
    }

    private void ConstruirObjetoDeDoisPontos(Vector2 inicio, Vector2 fim)
    {
        GameObject prefabAlvo = ObterPrefabDaFerramentaAtual();
        if (prefabAlvo == null) return;

        Vector2 centro = (inicio + fim) / 2f;
        GameObject novoObjeto = Instantiate(prefabAlvo, centro, Quaternion.identity);

        // ======= A BUSCA INTELIGENTE QUE NÃO QUEBRA NUNCA =======
        Transform baseA = EncontrarBaseSegura(novoObjeto.transform, "basea");
        Transform baseB = EncontrarBaseSegura(novoObjeto.transform, "baseb");

        if (baseA != null && baseB != null)
        {
            baseA.position = inicio;
            baseB.position = fim;

            Vector2 direcaoParaB = fim - inicio;
            float anguloA = Mathf.Atan2(direcaoParaB.y, direcaoParaB.x) * Mathf.Rad2Deg;
            baseA.rotation = Quaternion.Euler(0, 0, anguloA);

            Vector2 direcaoParaA = inicio - fim;
            float anguloB = Mathf.Atan2(direcaoParaA.y, direcaoParaA.x) * Mathf.Rad2Deg;
            baseB.rotation = Quaternion.Euler(0, 0, anguloB);
        }
        else
        {
            Debug.LogWarning("Chefe, o prefab não tem bases detectáveis! Cheque a hierarquia.");
        }

        objetosConstruidos.Add(novoObjeto);
       
    }

    // A MÁGICA: Varre o prefab ignorando maiúsculas, minúsculas e espaços!
    private Transform EncontrarBaseSegura(Transform pai, string nomeDesejado)
    {
        nomeDesejado = nomeDesejado.ToLower().Replace(" ", "");
        
        foreach (Transform filho in pai.GetComponentsInChildren<Transform>(true))
        {
            if (filho == pai) continue;
            string nomeFilho = filho.name.ToLower().Replace(" ", "");
            if (nomeFilho == nomeDesejado) return filho;
        }

        foreach (Transform filho in pai.GetComponentsInChildren<Transform>(true))
        {
            if (filho == pai) continue;
            string nomeFilho = filho.name.ToLower().Replace(" ", "");
            if (nomeFilho.Contains(nomeDesejado)) return filho;
        }

        return null;
    }

    private GameObject ObterPrefabDaFerramentaAtual()
    {
        switch (ferramentaAtual)
        {
            case TipoFerramenta.Bloco: return prefabBloco;
            case TipoFerramenta.Serra: return prefabSerra;
            case TipoFerramenta.Laser: return prefabLaser;
            case TipoFerramenta.Espinho: return prefabEspinho;
            case TipoFerramenta.Raio:  return prefabRaio;
            default: return null;
        }
    }

    public void SelecionarFerramenta(int idFerramenta)
    {
        ferramentaAtual = (TipoFerramenta)idFerramenta;
        
        if (fantasmaRenderer != null)
        {
            if (ferramentaAtual == TipoFerramenta.Borracha)
                fantasmaRenderer.color = new Color(1f, 0f, 0f, 0.5f); 
            else if (ferramentaAtual == TipoFerramenta.Laser || ferramentaAtual == TipoFerramenta.Serra)
                fantasmaRenderer.color = new Color(0f, 0.5f, 1f, 0.5f); 
            else
                fantasmaRenderer.color = new Color(0.2f, 1f, 0.2f, 0.5f); 
        }
    }

    public void BotaoRefazerUm() 
    {
        if (objetosConstruidos.Count > 0)
        {
            int ultimoIndice = objetosConstruidos.Count - 1;
            GameObject objetoParaDeletar = objetosConstruidos[ultimoIndice];
            
            objetosConstruidos.RemoveAt(ultimoIndice);
            Destroy(objetoParaDeletar);
        }
    }

    public void BotaoRefazerTudo() 
    {
        foreach (GameObject obj in objetosConstruidos)
        {
            if (obj != null) Destroy(obj);
        }
        objetosConstruidos.Clear();
    }

    public void BotaoPlay()
    {
        int raiosNaTela = 0;
        foreach (GameObject obj in objetosConstruidos)
        {
            if (obj != null && obj.name.Contains("Raio")) raiosNaTela++;
        }

        if (raiosNaTela != 3)
        {
            Debug.LogWarning($"Bloqueado! A fase precisa ter exatamente 3 raios. Você colocou {raiosNaTela}.");
            if (SoundManager.instance != null) SoundManager.instance.TocarErro();
            return;
        }

        Debug.Log("Fase validada com sucesso! Iniciando modo teste...");
    }

    private void AtualizarPosicaoMouse()
    {
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        float snapX = Mathf.Round(mousePos.x / tamanhoGrade) * tamanhoGrade;
        float snapY = Mathf.Round(mousePos.y / tamanhoGrade) * tamanhoGrade;

        float limiteConstrucaoEsq = limiteEsquerdo + compensacaoBorda;
        float limiteConstrucaoDir = limiteDireito - compensacaoBorda;
        float limiteConstrucaoInf = limiteInferior + compensacaoBorda;
        float limiteConstrucaoSup = limiteSuperior - compensacaoBorda; 

        snapX = Mathf.Clamp(snapX, limiteConstrucaoEsq, limiteConstrucaoDir);
        snapY = Mathf.Clamp(snapY, limiteConstrucaoInf, limiteConstrucaoSup);

        posicaoSnapMouse = new Vector2(snapX, snapY);
    }

    private void MoverFantasma()
    {
        if (fantasmaVisual != null) fantasmaVisual.transform.position = posicaoSnapMouse;
    }

    private void ProcessarArrastoCamera()
    {
        if (cam == null) return;
        if (Input.GetMouseButtonDown(2)) ultimaPosicaoMouseJanela = Input.mousePosition;

        if (Input.GetMouseButton(2))
        {
            Vector3 deltaMouse = Input.mousePosition - ultimaPosicaoMouseJanela;
            float dimensaoMundo = (cam.orthographicSize * 2f) / Screen.height;
            Vector3 movimento = new Vector3(-deltaMouse.x * dimensaoMundo, -deltaMouse.y * dimensaoMundo, 0f);
            cam.transform.position += movimento * velocidadeArrasto;
            ultimaPosicaoMouseJanela = Input.mousePosition;
        }
    }

    private void ProcessarZoomCamera()
    {
        if (cam == null) return;
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            if (scrollInput != 0f)
            {
                float alvoZoom = cam.orthographicSize - (scrollInput * velocidadeZoom);
                float maxTamanhoVertical = (limiteSuperior - limiteInferior) / 2f;
                float maxTamanhoHorizontal = (limiteDireito - limiteEsquerdo) / (2f * cam.aspect);
                float zoomMaximoPermitido = Mathf.Min(maxTamanhoVertical, maxTamanhoHorizontal);
                cam.orthographicSize = Mathf.Clamp(alvoZoom, zoomMinimoGameplay, zoomMaximoPermitido);
            }
        }
    }

    private void TravarCameraNosLimites()
    {
        if (cam == null) return;
        float metadeAlturaCam = cam.orthographicSize;
        float metadeLarguraCam = cam.orthographicSize * cam.aspect;
        float clampX = Mathf.Clamp(cam.transform.position.x, limiteEsquerdo + metadeLarguraCam, limiteDireito - metadeLarguraCam);
        float clampY = Mathf.Clamp(cam.transform.position.y, limiteInferior + metadeAlturaCam, limiteSuperior - metadeAlturaCam);
        cam.transform.position = new Vector3(clampX, clampY, -10f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector2 topoEsq = new Vector2(limiteEsquerdo, limiteSuperior); Vector2 topoDir = new Vector2(limiteDireito, limiteSuperior);
        Vector2 baixoEsq = new Vector2(limiteEsquerdo, limiteInferior); Vector2 baixoDir = new Vector2(limiteDireito, limiteInferior);
        Gizmos.DrawLine(topoEsq, topoDir); Gizmos.DrawLine(baixoEsq, baixoDir); Gizmos.DrawLine(topoEsq, baixoEsq); Gizmos.DrawLine(topoDir, baixoDir);    

        Gizmos.color = Color.yellow;
        float inEsq = limiteEsquerdo + compensacaoBorda; float inDir = limiteDireito - compensacaoBorda;
        float inInf = limiteInferior + compensacaoBorda; float inSup = limiteSuperior - compensacaoBorda;
        Vector2 inTopoEsq = new Vector2(inEsq, inSup); Vector2 inTopoDir = new Vector2(inDir, inSup);
        Vector2 inBaixoEsq = new Vector2(inEsq, inInf); Vector2 inBaixoDir = new Vector2(inDir, inInf);
        Gizmos.DrawLine(inTopoEsq, inTopoDir); Gizmos.DrawLine(inBaixoEsq, inBaixoDir); Gizmos.DrawLine(inTopoEsq, inBaixoEsq); Gizmos.DrawLine(inTopoDir, inBaixoDir); 
    }
}