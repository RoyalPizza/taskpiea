using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using Taskpiea.Core.Accounts;

namespace Taskpiea.WPFClient;

internal class UserValidationResult : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var user = (value as BindingGroup).Items[0] as User;
        var userRepository = AppDataCache.shared.RepositoryManager.Get<IUserRepository>();
        var result = userRepository.ValidateCreateAsync(AppDataCache.ProjectName, user).GetAwaiter().GetResult();
        if (result.ResultCode == Core.Results.ResultCode.Failure)
            return new ValidationResult(false, result.ErrorMessage);

        return ValidationResult.ValidResult;
    }
}
