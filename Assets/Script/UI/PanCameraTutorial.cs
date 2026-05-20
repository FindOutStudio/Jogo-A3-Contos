using System.Collections;
using UnityEngine;
using Cinemachine; 

public class PanCameraTutorial : MonoBehaviour
{
    [Header("Configuração da Câmera Principal")]
    [Tooltip("Arraste a sua Cinemachine Virtual Camera que já segue o Player")]
    public CinemachineVirtualCamera cameraPrincipal;
    
    [Tooltip("Arraste o objeto do Raio/Coletável que você quer mostrar")]
    public Transform alvoParaMostrar;

    [Header("Configuração do Tempo")]
    [Tooltip("Quantos segundos a câmera fica olhando pro coletável antes de voltar?")]
    public float tempoOlhando = 2.5f;

    private bool jaMostrou = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !jaMostrou)
        {
            StartCoroutine(MostrarOItem());
        }
    }

    private IEnumerator MostrarOItem()
    {
        jaMostrou = true;

        // 1. Salva quem a câmera estava seguindo (que é o seu Player)
        Transform alvoOriginal = cameraPrincipal.Follow;

        // 2. Muda o alvo da câmera para o coletável
        if (cameraPrincipal != null && alvoParaMostrar != null)
        {
            cameraPrincipal.Follow = alvoParaMostrar;
        }

        // 3. Espera o tempo pra ele admirar o coletável 
        // (Mudamos para WaitForSeconds normal, já que o jogo não vai mais pausar)
        yield return new WaitForSeconds(tempoOlhando);

        // 4. Devolve o alvo original (Player) para a câmera
        if (cameraPrincipal != null)
        {
            cameraPrincipal.Follow = alvoOriginal;
        }
    }
}