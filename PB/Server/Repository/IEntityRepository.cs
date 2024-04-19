using PB.EntityFramework;
using PB.Model.Models;
using PB.Shared.Enum;
using PB.Shared.Models.CRM.Customer;
using PB.Shared.Models.Inventory.Supplier;
using PB.Shared.Tables;
using System.Data;

namespace PB.Server.Repository
{
    public interface IEntityRepository
    {
        Task<AddressModel> GetEntityAddress(int addressID, IDbTransaction? tran = null);
        Task<List<AddressModel>> GetListOfEntityAddress(int entityID, IDbTransaction? tran = null);
        Task<AddressView> GetAddressView(int addressID, IDbTransaction? tran = null); 
        AddressView GetEntityAddressView(AddressModel addressModel);
        Task<object?> GetEntityContactPersons(int entityID, IDbTransaction? tran = null);
    }

    public class EntityRepository : IEntityRepository
    {
        private readonly IDbContext _dbContext;
        public EntityRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<AddressModel> GetEntityAddress(int addressID, IDbTransaction? tran = null)
        {
            return await _dbContext.GetByQueryAsync<AddressModel>(@$"Select EA.*,C.CountryName 
                                                                From EntityAddress EA
                                                                Left Join Country C ON EA.CountryID=C.CountryID and C.IsDeleted=0  
                                                                Where EA.AddressID={addressID} and EA.IsDeleted=0", null);
        }
        public async Task<List<AddressModel>> GetListOfEntityAddress(int entityID, IDbTransaction? tran = null) 
        {
            List<int> addressIDs = await _dbContext.GetListFieldsAsync<EntityAddressCustom, int>("AddressID", $"EntityID={entityID}", null);
            List<AddressModel> customerAddressList = new();
            if (addressIDs.Count > 0)
            {
                foreach (var addressID in addressIDs)
                {
                    customerAddressList.Add(await GetEntityAddress(addressID));
                }
            }
            return customerAddressList;
        }
        public async Task<AddressView> GetAddressView(int addressID, IDbTransaction? tran = null)
        {
            AddressModel customerAddress = await GetEntityAddress(addressID);
            AddressView customerAddressView = GetEntityAddressView(customerAddress);
            return customerAddressView;
        }
        public AddressView GetEntityAddressView(AddressModel addressModel)
        {
            AddressView customerAddressView = new AddressView() { AddressID = addressModel.AddressID };
            customerAddressView.CompleteAddress = addressModel.AddressLine1 + ',';
            if (!string.IsNullOrEmpty(addressModel.AddressLine2))
                customerAddressView.CompleteAddress += addressModel.AddressLine2 + ',';
            if (!string.IsNullOrEmpty(addressModel.AddressLine3))
                customerAddressView.CompleteAddress += addressModel.AddressLine3 + ',';
            if (!string.IsNullOrEmpty(addressModel.City))
                customerAddressView.CompleteAddress += addressModel.City + ',';
            if (!string.IsNullOrEmpty(addressModel.State))
                customerAddressView.CompleteAddress += addressModel.State + ',';
            if (!string.IsNullOrEmpty(addressModel.CountryName))
                customerAddressView.CompleteAddress += addressModel.CountryName + ',';
            if (!string.IsNullOrEmpty(addressModel.Pincode))
                customerAddressView.CompleteAddress += addressModel.Pincode + ',';
            customerAddressView.CompleteAddress = customerAddressView.CompleteAddress.TrimEnd(',');
            return customerAddressView;
        }
        public async Task<object?> GetEntityContactPersons(int entityID, IDbTransaction? tran = null)
        {
            int entityTypeID = await _dbContext.GetFieldsAsync<EntityCustom, int>("EntityTypeID", $"EntityID={entityID}", tran);
            if(entityTypeID == (int)EntityType.Customer)
            {
                return await _dbContext.GetListByQueryAsync<CustomerContactPersonModel>($@"
                                                               SELECT ROW_NUMBER() OVER(ORDER BY (SELECT 1)) AS RowIndex,vE.EmailAddress As Email,vE.Phone As Phone,vE.FirstName As Name,vE.EntityPersonalInfoID,CCP.*
                                                                From CustomerContactPerson CCP
																Join viEntity vE ON vE.EntityID=CCP.EntityID
                                                                Where CCP.CustomerEntityID={entityID} and CCP.IsDeleted=0", null, tran);
            }
            if(entityTypeID == (int)EntityType.Supplier)
            {
                return await _dbContext.GetListByQueryAsync<SupplierContactPersonModel>($@"
																SELECT ROW_NUMBER() OVER(ORDER BY (SELECT 1)) AS RowIndex,vE.EmailAddress As Email,vE.Phone As Phone,vE.FirstName As Name,vE.EntityPersonalInfoID,SCP.*
                                                                From SupplierContactPerson SCP
																Join viEntity vE ON vE.EntityID=SCP.EntityID
                                                                Where SCP.SupplierEntityID={entityID} and SCP.IsDeleted=0", null, tran);
            }
            return null;
        }
    }
}
