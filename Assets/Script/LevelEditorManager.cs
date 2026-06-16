using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SpritesDoAutoTile
{
    public Sprite cantoSupEsq;
    public Sprite bordaSupCentro;
    public Sprite cantoSupDir;
    
    public Sprite bordaMeioEsq;
    public Sprite centro;
    public Sprite bordaMeioDir;
    
    public Sprite cantoInfEsq;
    public Sprite bordaInfCentro;
    public Sprite cantoInfDir;
    public Sprite quinaExtSupEsq;
    public Sprite quinaExtSupDir;
    public Sprite quinaExtInfEsq;
    public Sprite quinaExtInfDir;
}

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

    [Header("=== Imagens do Bloco (Auto-Tile) ===")]
    public SpritesDoAutoTile spritesBloco;

    [Header("=== Ajuste de Rotação (Bases) ===")]
    public float rotacaoExtraBaseA = 0f;
    public float rotacaoExtraBaseB = 180f; 

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

    private float tempoUltimoCliqueBloco = 0f;

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
                    colisorNoLocal.gameObject.SetActive(false); 
                    Destroy(colisorNoLocal.gameObject);
                }
            }
            if (Input.GetMouseButtonUp(0)) AtualizarTodosOsBlocos();
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
        else if (ferramentaAtual == TipoFerramenta.Bloco)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (Time.time - tempoUltimoCliqueBloco < 0.3f)
                {
                    PreencherComBlocos(posicaoSnapMouse);
                    return; 
                }
                tempoUltimoCliqueBloco = Time.time;
            }

            if (Input.GetMouseButton(0)) 
            {
                if (!LocalEstaOcupado(posicaoSnapMouse))
                {
                    GameObject novoObjeto = Instantiate(prefabBloco, posicaoSnapMouse, Quaternion.identity);
                    objetosConstruidos.Add(novoObjeto);
                    
                    // Coloca visualmente como borda superior temporária enquanto arrasta
                    SpriteRenderer sr = novoObjeto.GetComponent<SpriteRenderer>();
                    if(sr != null && spritesBloco.bordaSupCentro != null) sr.sprite = spritesBloco.bordaSupCentro;
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                AtualizarTodosOsBlocos();
            }
        }
        else
        {
            if (Input.GetMouseButton(0)) 
            {
                if (!LocalEstaOcupado(posicaoSnapMouse))
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

    private bool LocalEstaOcupado(Vector2 pos)
    {
        foreach (GameObject obj in objetosConstruidos)
        {
            if (obj != null)
            {
                if (Vector2.Distance(obj.transform.position, pos) < 0.1f) return true;
            }
        }
        return false;
    }

    private void PreencherComBlocos(Vector2 startPos)
    {
        HashSet<Vector2> ocupados = new HashSet<Vector2>();
        foreach (GameObject obj in objetosConstruidos)
        {
            if (obj != null)
            {
                float rX = Mathf.Round(obj.transform.position.x / tamanhoGrade) * tamanhoGrade;
                float rY = Mathf.Round(obj.transform.position.y / tamanhoGrade) * tamanhoGrade;
                ocupados.Add(new Vector2(rX, rY));
            }
        }

        Vector2 startGrid = new Vector2(
            Mathf.Round(startPos.x / tamanhoGrade) * tamanhoGrade,
            Mathf.Round(startPos.y / tamanhoGrade) * tamanhoGrade
        );

        if (ocupados.Contains(startGrid)) return; 

        Queue<Vector2> fila = new Queue<Vector2>();
        HashSet<Vector2> visitados = new HashSet<Vector2>();
        List<Vector2> posicoesParaConstruir = new List<Vector2>();

        fila.Enqueue(startGrid);
        visitados.Add(startGrid);

        while (fila.Count > 0)
        {
            Vector2 atual = fila.Dequeue();

            if (atual.x < limiteEsquerdo || atual.x > limiteDireito ||
                atual.y < limiteInferior || atual.y > limiteSuperior)
            {
                continue;
            }

            if (!ocupados.Contains(atual))
            {
                posicoesParaConstruir.Add(atual);

                Vector2 up = atual + Vector2.up * tamanhoGrade;
                Vector2 down = atual + Vector2.down * tamanhoGrade;
                Vector2 left = atual + Vector2.left * tamanhoGrade;
                Vector2 right = atual + Vector2.right * tamanhoGrade;

                if (!visitados.Contains(up)) { fila.Enqueue(up); visitados.Add(up); }
                if (!visitados.Contains(down)) { fila.Enqueue(down); visitados.Add(down); }
                if (!visitados.Contains(left)) { fila.Enqueue(left); visitados.Add(left); }
                if (!visitados.Contains(right)) { fila.Enqueue(right); visitados.Add(right); }
            }
        }

        int maxBlocos = 800; 
        int construidos = 0;

        foreach (Vector2 pos in posicoesParaConstruir)
        {
            if (construidos >= maxBlocos) break;
            
            GameObject novoBloco = Instantiate(prefabBloco, pos, Quaternion.identity);
            objetosConstruidos.Add(novoBloco);
            construidos++;
        }

        AtualizarTodosOsBlocos();
    }

    // ========================================================
    // ======= A MATEMÁTICA DAS 16 FACES PERFEITA =============
    // ========================================================
    private void AtualizarTodosOsBlocos()
{
    HashSet<Vector2> posicoesBlocos = new HashSet<Vector2>();
    foreach (GameObject obj in objetosConstruidos)
    {
        if (obj != null && obj.name.Contains(prefabBloco.name))
        {
            float rX = Mathf.Round(obj.transform.position.x / tamanhoGrade) * tamanhoGrade;
            float rY = Mathf.Round(obj.transform.position.y / tamanhoGrade) * tamanhoGrade;
            posicoesBlocos.Add(new Vector2(rX, rY));
        }
    }

    foreach (GameObject obj in objetosConstruidos)
    {
        if (obj == null || !obj.name.Contains(prefabBloco.name)) continue;
        
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        Vector2 p = new Vector2(Mathf.Round(obj.transform.position.x), Mathf.Round(obj.transform.position.y));

        bool u = posicoesBlocos.Contains(p + Vector2.up);
        bool d = posicoesBlocos.Contains(p + Vector2.down);
        bool l = posicoesBlocos.Contains(p + Vector2.left);
        bool r = posicoesBlocos.Contains(p + Vector2.right);
        
        // Checagem de diagonais para a escadinha
        bool ul = posicoesBlocos.Contains(p + new Vector2(-1, 1));
        bool ur = posicoesBlocos.Contains(p + new Vector2(1, 1));
        bool dl = posicoesBlocos.Contains(p + new Vector2(-1, -1));
        bool dr = posicoesBlocos.Contains(p + new Vector2(1, -1));

        // Lógica de seleção das Quinas (Degraus)
        if (!u && !l && r && d) sr.sprite = spritesBloco.quinaExtSupEsq;
        else if (!u && !r && l && d) sr.sprite = spritesBloco.quinaExtSupDir;
        else if (!d && !l && r && u) sr.sprite = spritesBloco.quinaExtInfEsq;
        else if (!d && !r && l && u) sr.sprite = spritesBloco.quinaExtInfDir;
        // Se não for quina, segue a lógica anterior de bordas
        else
        {
            int soma = (u ? 1 : 0) + (r ? 2 : 0) + (d ? 4 : 0) + (l ? 8 : 0);
            switch (soma)
            {
                case 3: sr.sprite = spritesBloco.cantoInfEsq; break;
                case 6: sr.sprite = spritesBloco.cantoSupEsq; break;
                case 7: sr.sprite = spritesBloco.bordaMeioEsq; break;
                case 9: sr.sprite = spritesBloco.cantoInfDir; break;
                case 11: sr.sprite = spritesBloco.bordaInfCentro; break;
                case 12: sr.sprite = spritesBloco.cantoSupDir; break;
                case 13: sr.sprite = spritesBloco.bordaMeioDir; break;
                case 14: sr.sprite = spritesBloco.bordaSupCentro; break;
                case 15: sr.sprite = spritesBloco.centro; break;
                default: sr.sprite = spritesBloco.bordaSupCentro; break;
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

        Transform baseA = EncontrarBaseSegura(novoObjeto.transform, "basea");
        Transform baseB = EncontrarBaseSegura(novoObjeto.transform, "baseb");

        if (baseA != null && baseB != null)
        {
            Vector2 direcao = fim - inicio;
            float anguloPai = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;
            novoObjeto.transform.rotation = Quaternion.Euler(0, 0, anguloPai);

            baseA.position = inicio;
            baseB.position = fim;

            baseA.up = direcao.normalized;
            baseB.up = -direcao.normalized;

            baseA.Rotate(0, 0, rotacaoExtraBaseA);
            baseB.Rotate(0, 0, rotacaoExtraBaseB);
        }

        objetosConstruidos.Add(novoObjeto);
    }

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
            if (ferramentaAtual == TipoFerramenta.Borracha) fantasmaRenderer.color = new Color(1f, 0f, 0f, 0.5f); 
            else if (ferramentaAtual == TipoFerramenta.Laser || ferramentaAtual == TipoFerramenta.Serra) fantasmaRenderer.color = new Color(0f, 0.5f, 1f, 0.5f); 
            else fantasmaRenderer.color = new Color(0.2f, 1f, 0.2f, 0.5f); 
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
            AtualizarTodosOsBlocos();
        }
    }

    public void BotaoRefazerTudo() 
    {
        foreach (GameObject obj in objetosConstruidos) if (obj != null) Destroy(obj);
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
            return;
        }
        Debug.Log("Fase validada com sucesso! Iniciando modo teste...");
    }

    private void AtualizarPosicaoMouse()
    {
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        float snapX = Mathf.Round(mousePos.x / tamanhoGrade) * tamanhoGrade;
        float snapY = Mathf.Round(mousePos.y / tamanhoGrade) * tamanhoGrade;
        float limitEsq = limiteEsquerdo + compensacaoBorda; float limitDir = limiteDireito - compensacaoBorda;
        float limitInf = limiteInferior + compensacaoBorda; float limitSup = limiteSuperior - compensacaoBorda; 
        snapX = Mathf.Clamp(snapX, limitEsq, limitDir);
        snapY = Mathf.Clamp(snapY, limitInf, limitSup);
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
                float maxVertical = (limiteSuperior - limiteInferior) / 2f;
                float maxHorizontal = (limiteDireito - limiteEsquerdo) / (2f * cam.aspect);
                float zoomMaxPermitido = Mathf.Min(maxVertical, maxHorizontal);
                cam.orthographicSize = Mathf.Clamp(alvoZoom, zoomMinimoGameplay, zoomMaxPermitido);
            }
        }
    }

    private void TravarCameraNosLimites()
    {
        if (cam == null) return;
        float metAlturaCam = cam.orthographicSize;
        float metLarguraCam = cam.orthographicSize * cam.aspect;
        float clampX = Mathf.Clamp(cam.transform.position.x, limiteEsquerdo + metLarguraCam, limiteDireito - metLarguraCam);
        float clampY = Mathf.Clamp(cam.transform.position.y, limiteInferior + metAlturaCam, limiteSuperior - metAlturaCam);
        cam.transform.position = new Vector3(clampX, clampY, -10f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector2 topoEsq = new Vector2(limiteEsquerdo, limiteSuperior); Vector2 topoDir = new Vector2(limiteDireito, limiteSuperior);
        Vector2 baixoEsq = new Vector2(limiteEsquerdo, limiteInferior); Vector2 baixoDir = new Vector2(limiteDireito, limiteInferior);
        Gizmos.DrawLine(topoEsq, topoDir); Gizmos.DrawLine(baixoEsq, baixoDir); Gizmos.DrawLine(topoEsq, baixoEsq); Gizmos.DrawLine(topoDir, baixoDir);    
    }
}