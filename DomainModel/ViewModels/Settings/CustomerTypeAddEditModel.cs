using System.ComponentModel.DataAnnotations;

namespace DomainModel.ViewModels.Settings
{
    public class CustomerTypeAddEditModel
    {
        public int CustomerTypeID { get; set; }

        [Required(ErrorMessage = "عنوان نوع مشتری الزامی است.")]
        [StringLength(50, ErrorMessage = "عنوان حداکثر ۵۰ کاراکتر است.")]
        [Display(Name = "عنوان")]
        public string Title { get; set; } = string.Empty;
    }
}
