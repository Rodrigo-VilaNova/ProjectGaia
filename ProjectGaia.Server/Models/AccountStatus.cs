namespace ProjectGaia.Server.Models
{
    /// <summary>
    /// Representa os diferentes estados de uma conta.
    /// </summary>
    public enum AccountStatus
    {
        /// <summary>
        /// Conta ativa, com acesso permitido.
        /// </summary>
        Active,

        /// <summary>
        /// Conta bloqueada, sem acesso permitido.
        /// </summary>
        Blocked
    }
}
