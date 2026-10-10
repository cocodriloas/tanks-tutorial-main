// NUEVO SCRIPT: guarda lo que se eligió en el menú principal
// (quién es humano, quién es la computadora y quién no juega).
// Al ser "static", los datos se conservan al cambiar de escena.

public enum TipoJugador
{
    Humano = 0,
    CPU = 1,
    Apagado = 2
}

public static class ConfiguracionPartida
{
    // Valores por defecto: Jugador 1 y 2 humanos, Jugador 3 la computadora.
    // Se usan si abres directamente la escena del mapa sin pasar por el menú.
    public static TipoJugador[] Jugadores = { TipoJugador.Humano, TipoJugador.Humano, TipoJugador.CPU };

    public static TipoJugador ObtenerTipo(int indice)
    {
        if (indice < 0 || indice >= Jugadores.Length)
            return TipoJugador.Humano;

        return Jugadores[indice];
    }
}
