namespace ProjectGaia.Server.Models
{
    /// <summary>
    /// Representa os diferentes tipos de conta reconhecidos pelo sistema.
    /// </summary>
    public enum AccountType
    {
        /// <summary>
        /// Conta de utilizador comum com permissões padrão.
        /// </summary>
        User,

        /// <summary>
        /// Conta de administrador com permissões elevadas.
        /// </summary>
        Admin
    }
}
