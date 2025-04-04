using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectGaia.Server.Models
{
    /// <summary>
    /// Representa uma fatura.
    /// </summary>
    public class Invoice
    {
        /// <summary>
        /// Identificador único da fatura.
        /// </summary>
        [Key]
        public int ID { get; set; }

        /// <summary>
        /// Valor total da fatura. Representa o valor pago.
        /// </summary>
        [Required]
        public decimal Price { get; set; }

        /// <summary>
        /// Quantidade de eletricidade consumida.
        /// </summary>
        [Required]
        public decimal Consumption { get; set; }

        /// <summary>
        /// Data de emissão da fatura.
        /// </summary>
        [Required]
        public DateTime EmissionDate { get; set; }

        /// <summary>
        /// Data do carregamento inicial da fatura para o sistema.
        /// </summary>
        [Required]
        public DateTime UploadDate { get; set; }

        //Navigation properties
        /// <summary>
        /// Identificador da conta associada à sessão. Esse campo é uma chave estrangeira que faz referência a uma conta de utilizador.
        /// </summary>
        [Required]
        [ForeignKey("Account")]
        public int AccountID { get; set; }

        /// <summary>
        /// A conta associada à sessão. Representa a navegação para a entidade de conta do utilizador.
        /// </summary>
        public Account? Account { get; set; }
    }
}
