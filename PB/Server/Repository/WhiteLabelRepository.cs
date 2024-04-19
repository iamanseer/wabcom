using Hangfire;
using Hangfire.Common;
using PB.DatabaseFramework;
using PB.EntityFramework;
using PB.Model;
using PB.Shared.Enum;
using PB.Shared.Helpers;
using PB.Shared.Models;
using PB.Shared.Models.Common;
using PB.Shared.Tables;
using System.Data;

namespace PB.Server.Repository
{
    public interface IWhiteLabelRepository
    {
        Task<RegistraionAddResultModel?> CreateSuperAdminClient(RegistrationModel model, IDbTransaction? tran = null);
    }
    public class WhiteLabelRepository : IWhiteLabelRepository
    {
        private readonly IDbContext _dbContext;
        private readonly IUserRepository _user;
        private readonly IAccountRepository _accounts;
        private readonly IBackgroundJobClient _job;

        public WhiteLabelRepository(IDbContext dbContext, IUserRepository user, IAccountRepository accounts, IBackgroundJobClient job)
        {
            this._dbContext = dbContext;
            this._user = user;
            this._accounts = accounts;
            this._job = job;
        }

        #region Creating super admin Client

        public async Task<RegistraionAddResultModel?> CreateSuperAdminClient(RegistrationModel model, IDbTransaction? tran = null)
        {
            try
            {
                model.ClientID = PDV.ProgbizClientID;
                model.PackageID = 1;

                #region Client

                var clientEntity = new EntityCustom
                {
                    Phone = model.MobileNo,
                    EntityTypeID = (int)EntityType.Client,
                    EmailAddress = model.Email,
                    CountryID = model.CountryID,

                };
                clientEntity.EntityID = await _dbContext.SaveAsync(clientEntity, tran);

                var client = new ClientCustom
                {
                    ClientID = model.ClientID,
                    EntityID = clientEntity.EntityID,
                    PackageID = model.PackageID,

                };
                await _dbContext.IdentityInsertAsync(client, tran);

                Model.Branch branch = new()
                {
                    ClientID = model.ClientID,
                    EntityID = clientEntity.EntityID,
                };
                branch.BranchID = await _dbContext.SaveAsync(branch, tran);

                EntityInstituteInfo instituteInfo = new()
                {
                    EntityID = clientEntity.EntityID,
                    Name = model.CompanyName,
                    ContactPerson = model.ClientName,
                };
                await _dbContext.SaveAsync(instituteInfo, tran);

                #endregion

                #region User

                UserCustom newuser = new()
                {
                    EntityID = clientEntity.EntityID,
                    LoginStatus = true,
                    UserName = model.Email,
                    UserTypeID = (int)UserTypes.Client,
                    Password = model.Password,
                    ClientID = model.ClientID,
                    EmailConfirmed = false,
                };
                newuser.UserID = await _user.SaveUser(newuser, true, tran, PDV.BrandName, model.Email);

                #endregion

                var packageFeatures = await _dbContext.GetByQueryAsync<string>(@$"Select F.FeatureName
                                                                                From MembershipPackage P
                                                                                Left Join MembershipPackageFeature PF ON P.PackageID=PF.PackageID And PF.IsDeleted=0
                                                                                Left join MembershipFeature F ON F.FeatureID=PF.FeatureID And F.IsDeleted=0
                                                                                Where P.PackageID={model.PackageID.Value} And P.IsDeleted=0", null);

                //if (packageFeatures.Contains("Accounts"))
                //{
                //    _job.Enqueue(() => _accounts.InsertAccountsRelatedDefaultEntriesForClient(model.ClientID, branch.BranchID));
                //}

                return new RegistraionAddResultModel() { ClientID = model.ClientID, UserID = newuser.UserID };
            }
            catch(Exception e) 
            {
                return null;
            }
        }

        #endregion
    }
}
