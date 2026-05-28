using UnityEngine;

public class SomObstaculo3D : MonoBehaviour
{
    public enum TipoObstaculo { Serra, Laser }
    public TipoObstaculo meuTipo;
    
    private AudioSource meuAudio;

    private void Awake()
    {
        meuAudio = gameObject.AddComponent<AudioSource>();
        meuAudio.spatialBlend = 1f; // 100% 3D
        meuAudio.rolloffMode = AudioRolloffMode.Linear;
        meuAudio.minDistance = 2f;
        meuAudio.maxDistance = 15f; 
        meuAudio.loop = true;
    }

    private void OnEnable()
    {
        if (SoundManager.instance == null || meuAudio == null) return;

        // Puxa o som com o volume global certinho
        if (meuTipo == TipoObstaculo.Serra && SoundManager.instance.obstaculoSerra != null)
        {
            meuAudio.clip = SoundManager.instance.obstaculoSerra;
            meuAudio.volume = SoundManager.instance.volumeSerra * SoundManager.instance.volumeGlobalSFX;
        }
        else if (meuTipo == TipoObstaculo.Laser && SoundManager.instance.obstaculoLaser != null)
        {
            meuAudio.clip = SoundManager.instance.obstaculoLaser;
            meuAudio.volume = SoundManager.instance.volumeLaser * SoundManager.instance.volumeGlobalSFX;
        }

        if (meuAudio.clip != null)
        {
            meuAudio.pitch = Random.Range(0.95f, 1.05f); 
            meuAudio.Play();
        }
    }

    private void OnDisable()
    {
        if (meuAudio != null && meuAudio.isPlaying)
        {
            meuAudio.Stop(); 
        }
    }
}