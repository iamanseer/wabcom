using PB.EntityFramework;
using PB.Model.Models;
using PB.Shared.Models.CRM.Customer;
using PB.Shared.Models.Inventory.Supplier;
using System.Data;

namespace PB.Server.Repository
{ 
    public interface ISupplierRepository
    { 
        Task<AddressModel> GetSupplierAddress(int addressID, IDbTransaction? tran = null);
        Task<List<AddressModel>> GetListOfSupplierAddress(int supplierEntityID, IDbTransaction? tran = null);
        Task<AddressView> GetAddressView(int addressID, IDbTransaction? tran = null);
        AddressView GetSupplierAddressViewModel(AddressModel addressModel); 
        Task<List<SupplierContactPersonModel>> GetSupplierContactPersons(int supplierEntityID, IDbTransaction? tran = null); 
    }

    public class SupplierRepository : ISupplierRepository 
    {
        private readonly IDbContext _dbContext;
        public SupplierRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<AddressModel> GetSupplierAddress(int addressID, IDbTransaction? tran = null) 
        {
            return await _dbContext.GetByQueryAsync<AddressModel>(@$"Select EA.*,C.CountryName 
                                                                From EntityAddress EA
                                                                Left Join Country C ON EA.CountryID=C.CountryID and C.IsDeleted=0  
                                                                Where EA.AddressID={addressID} and EA.IsDeleted=0", null);
        }
        public async Task<List<AddressModel>> GetListOfSupplierAddress(int supplierEntityID, IDbTransaction? tran = null) 
        {
            List<int> addressIDs = await _dbContext.GetListFieldsAsync<EntityAddressCustom, int>("AddressID", $"EntityID={supplierEntityID}", null);
            List<AddressModel> customerAddressList = new();
            if (addressIDs.Count > 0)
            {
                foreach (var addressID in addressIDs)
                {
                    customerAddressList.Add(await GetSupplierAddress(addressID));
                }
            }
            return customerAddressList;
        }
        public async Task<AddressView> GetAddressView(int addressID, IDbTransaction? tran = null)
        {
            AddressModel customerAddress = await GetSupplierAddress(addressID);
            AddressView customerAddressView = GetSupplierAddressViewModel(customerAddress);
            return customerAddressView;
        }
        public AddressView GetSupplierAddressViewModel(AddressModel addressModel) 
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
        public async Task<List<SupplierContactPersonModel>> GetSupplierContactPersons(int supplierEntityID, IDbTransaction? tran = null)
        {
            return await _dbContext.GetListByQueryAsync<SupplierContactPersonModel>($@"
                                                               SELECT ROW_NUMBER() OVER(ORDER BY (SELECT 1)) AS RowIndex,E.EmailAddress As Email,E.Phone As Phone,EIP.FirstName As Name,EIP.EntityPersonalInfoID,SSP.*
                                                                From SupplierContactPerson SSP
                                                                Left Join EntityPersonalInfo EIP ON SSP.EntityID=EIP.EntityID and EIP.IsDeleted=0
                                                                Left Join Entity E ON E.EntityID=SSP.EntityID and E.IsDeleted=0
                                                                Where SSP.SupplierEntityID={supplierEntityID} and SSP.IsDeleted=0", null, tran);
        }

    } 
}
