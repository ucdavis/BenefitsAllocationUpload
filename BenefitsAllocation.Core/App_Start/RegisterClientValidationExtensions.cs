using DataAnnotationsExtensions.ClientValidation;

[assembly: WebActivator.PreApplicationStartMethod(typeof(BenefitsAllocation.Core.App_Start.RegisterClientValidationExtensions), "Start")]


namespace BenefitsAllocation.Core.App_Start
{
    class RegisterClientValidationExtensions
    {
        public static void Start()
        {
            DataAnnotationsModelValidatorProviderExtensions.RegisterValidationExtensions();
        }
    }
}
