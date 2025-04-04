namespace ProjectGaia.Server.Models
{
    /// <summary>
    /// Representa os diferentes tipos de eventos que podem ocorrer no sistema.
    /// </summary>
    public enum EventType
    {
        /// <summary>
        /// Evento relacionado a um pagamento a realizar.
        /// </summary>
        Payment,

        /// <summary>
        /// Evento relacionado a um ajuste no preço.
        /// </summary>
        Price,

        /// <summary>
        /// Evento de natureza diversa, que não se encaixa nas categorias anteriores.
        /// </summary>
        Miscellaneous
    }
}
