using System.ComponentModel.DataAnnotations;

namespace TransportRental.Web.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Введите логин")]
    [Display(Name = "Логин")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите пароль")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 4, ErrorMessage = "Пароль должен быть не менее 4 символов")]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите ФИО")]
    [Display(Name = "ФИО")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Выберите роль")]
    [Display(Name = "Роль")]
    public int RoleId { get; set; }
}
