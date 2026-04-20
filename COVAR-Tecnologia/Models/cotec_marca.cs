using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COVAR_Tecnologia.Models
{
    public class cotec_marca
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required,StringLength(50)]
        public string Nombre { get; set; }
        public List<cotec_producto> Productos { get; set; }
    }
}
