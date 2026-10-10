using UnityEngine;

// NUEVO SCRIPT: oculta el objeto cuando el juego corre en el navegador (WebGL).
// Se usa en los botones SALIR, porque en una página web no se puede cerrar el juego.
public class OcultarEnWeb : MonoBehaviour
{
    private void Awake ()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
            gameObject.SetActive (false);
    }
}
