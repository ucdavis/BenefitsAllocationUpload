
using BenefitsAllocationUpload.Services;
//using BenefitsAllocation.Core.Repositories;
using BenefitsAllocationUpload.Controllers;
using Castle.Windsor;
using UCDArch.Core.CommonValidator;
using UCDArch.Core.DataAnnotationsValidator.CommonValidatorAdapter;
using UCDArch.Core.PersistanceSupport;
using UCDArch.Data.NHibernate;
using Castle.MicroKernel.Registration;

namespace BenefitsAllocationUpload
{
    internal static class ComponentRegistrar
    {
        public static void AddComponentsTo(IWindsorContainer container)
        {
            AddGenericRepositoriesTo(container);

            container.Register(Component.For<IValidator>().ImplementedBy<Validator>().Named("validator"));
            container.Register(Component.For<IDbContext>().ImplementedBy<DbContext>().Named("dbContext"));
        }

        private static void AddGenericRepositoriesTo(IWindsorContainer container)
        {
            container.Register(Component.For(typeof(IRepositoryWithTypedId<,>)).ImplementedBy(typeof(RepositoryWithTypedId<,>)).Named("repositoryWithTypedId"));
            container.Register(Component.For(typeof(IRepository<>)).ImplementedBy(typeof(Repository<>)).Named("repositoryType"));
            container.Register(Component.For<IRepository>().ImplementedBy<Repository>().Named("repository"));

            //container.Register(Component.For(typeof(IRepositoryFactory)).ImplementedBy<RepositoryFactory>().Named("repositoryFactory"));
            //container.Register(Component.For(typeof(IDbService)).ImplementedBy<DbService>().Named("dbService"));

            container.Register(Component.For(typeof(IDataExtractionService)).ImplementedBy<DataExtractionService>().Named("dataExtractionService"));
            container.Register(Component.For(typeof(ISftpService)).ImplementedBy<SftpService>().Named("sftpService"));
        }
    }
}