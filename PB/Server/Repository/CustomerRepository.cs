using PB.EntityFramework;
using PB.Model.Models;
using PB.Shared.Models.CRM.Customer;
using System.Data;

namespace PB.Server.Repository
{
    public interface ICustomerRepository 
    {
        Task<AddressModel> GetCustomerAddress(int addressID, IDbTransaction? tran = null);
        Task<List<AddressModel>> GetListOfCustomerAddress(int customerEntityID, IDbTransaction? tran = null);
        Task<AddressView> GetAddressView(int addressID, IDbTransaction? tran = null);
        AddressView GetCustomerAddressViewModel(AddressModel addressModel);
        Task<List<CustomerContactPersonModel>> GetCustomerCotactPersons(int customerEnttityID, IDbTransaction? tran = null);

    }

    public class CustomerRepository : ICustomerRepository
    {
        private readonly IDbContext _dbContext;
        public CustomerRepository(IDbContext dbContext) 
        {
            _dbContext = dbContext;
        }

        public async Task<AddressModel> GetCustomerAddress(int addressID, IDbTransaction? tran = null)
        {
            return await _dbContext.GetByQueryAsync<AddressModel>(@$"Select EA.*,C.CountryName 
                                                                From EntityAddress EA
                                                                Left Join Country C ON EA.CountryID=C.CountryID and C.IsDeleted=0  
                                                                Where EA.AddressID={addressID} and EA.IsDeleted=0", null);
        }
        public async Task<List<AddressModel>> GetListOfCustomerAddress(int customerEntityID, IDbTransaction? tran = null)
        {
            List<int> addressIDs = await _dbContext.GetListFieldsAsync<EntityAddressCustom, int>("AddressID", $"EntityID={customerEntityID}", null);
            List<AddressModel> customerAddressList = new();
            if (addressIDs.Count > 0)
            {
                foreach (var addressID in addressIDs)
                {
                    customerAddressList.Add(await GetCustomerAddress(addressID));
                }
            }
            return customerAddressList;
        }
        public async Task<AddressView> GetAddressView(int addressID, IDbTransaction? tran = null)
        {
            AddressModel customerAddress = await GetCustomerAddress(addressID);
            AddressView customerAddressView = GetCustomerAddressViewModel(customerAddress);
            return customerAddressView;
        }
        public AddressView GetCustomerAddressViewModel(AddressModel addressModel)
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
        public async Task<List<CustomerContactPersonModel>> GetCustomerCotactPersons(int customerEnttityID, IDbTransaction? tran = null)
        {
            return await _dbContext.GetListByQueryAsync<CustomerContactPersonModel>($@"
                                                               SELECT ROW_NUMBER() OVER(ORDER BY (SELECT 1)) AS RowIndex,E.EmailAddress As Email,E.Phone As Phone,EIP.FirstName As Name,EIP.EntityPersonalInfoID,CCP.*
                                                                From CustomerContactPerson CCP
                                                                Left Join EntityPersonalInfo EIP ON CCP.EntityID=EIP.EntityID and EIP.IsDeleted=0
                                                                Left Join Entity E ON E.EntityID=CCP.EntityID and E.IsDeleted=0
                                                                Where CCP.CustomerEntityID={customerEnttityID} and CCP.IsDeleted=0", null, tran);
        }

       
    }
}
