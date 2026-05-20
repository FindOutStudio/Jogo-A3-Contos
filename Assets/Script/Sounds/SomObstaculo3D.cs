using UnityEngine;

public class SomObstaculo3D : MonoBehaviour
{
    public enum TipoObstaculo { Serra, Laser }
    public TipoObstaculo meuTipo;
    
    private AudioSource meuAudio;

    private void Awake()
    {
        // Cria a "caixa de som" no objeto assim que a fase carrega
        meuAudio = gameObject.AddComponent<AudioSource>();
        meuAudio.spatialBlend = 1f; // 100% 3D
        meuAudio.rolloffMode = AudioRolloffMode.Linear;
        meuAudio.minDistance = 2f;
        meuAudio.maxDistance = 15f; 
        meuAudio.loop = true;
    }

    // OnEnable roda toda vez que o objeto APARECE / LIGA na tela
    private void OnEnable()
    {
        if (SoundManager.instance == null || meuAudio == null) return;

        // Puxa o som e o volume certinhos lá do SoundManager
        if (meuTipo == TipoObstaculo.Serra && SoundManager.instance.obstaculoSerra != null)
        {
            meuAudio.clip = SoundManager.instance.obstaculoSerra;
            meuAudio.volume = SoundManager.instance.volumeSerra;
        }
        else if (meuTipo == TipoObstaculo.Laser && SoundManager.instance.obstaculoLaser != null)
        {
            meuAudio.clip = SoundManager.instance.obstaculoLaser;
            meuAudio.volume = SoundManager.instance.volumeLaser;
        }

        // Se achou o áudio, dá o play!
        if (meuAudio.clip != null)
        {
            meuAudio.pitch = Random.Range(0.95f, 1.05f); // Evita som robótico se tiver vários
            meuAudio.Play();
        }
    }

    // OnDisable roda toda vez que o objeto SOME / DESLIGA na tela
    private void OnDisable()
    {
        if (meuAudio != null && meuAudio.isPlaying)
        {
            meuAudio.Stop(); // Corta o som do laser instantaneamente!
        }
    }
}