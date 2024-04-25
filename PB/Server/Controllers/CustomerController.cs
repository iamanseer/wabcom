using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PB.Client.Pages.Settings;
using PB.Shared.Models;
using PB.DatabaseFramework;
using PB.Model;
using PB.Shared;
using PB.Shared.Enum;
using PB.Shared.Models.Common;
using PB.Shared.Tables;
using System.ComponentModel.DataAnnotations;
using System.Data;
using PB.Model.Models;
using PB.EntityFramework;
using PB.Shared.Models.CRM.Customer;
using PB.Shared.Tables.Common;
using Microsoft.IdentityModel.Tokens;
using System.Net.Mail;
using PB.Server.Repository;
using PB.Shared.Models.Inventory.Supplier;
using PB.Shared.Models.Inventory.Invoices;

namespace PB.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CustomerController : BaseController
    {
        private readonly IDbContext _dbContext;
        private readonly IDbConnection _cn;
        private readonly IMapper _mapper;
        private readonly IEntityRepository _entity;
        private readonly IAccountRepository _accounts;
        public CustomerController(IDbContext dbContext, IDbConnection cn, IMapper mapper, ICustomerRepository customer, IEntityRepository entity, IAccountRepository accounts)
        {
            _cn = cn;
            _dbContext = dbContext;
            _mapper = mapper;
            _entity = entity;
            _accounts = accounts;
        }

        #region Customer

        [HttpPost("save-customer")]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
        public async Task<IActionResult> SaveCustomer(CustomerModelNew customerModel)
        {
            //var phone = await _dbContext.GetByQueryAsync<string>(@$"Select Phone 
            //                                                                from Entity
            //                                                                where Phone=@Phone and EntityID<>{Convert.ToInt32(customerModel.EntityID)} and ClientID={CurrentClientID} and IsDeleted=0", customerModel);

            //if (!string.IsNullOrEmpty(phone))
            //{
            //    return BadRequest(new BaseErrorResponse()
            //    {
            //        ErrorCode = 0,
            //        ResponseTitle = "Invalidsubmission",
            //        ResponseMessage = "PhoneNumberExist"
            //    });
            //}

            var email = await _dbContext.GetByQueryAsync<string>(@$"Select EmailAddress 
                                                                            from Entity
                                                                            where EmailAddress=@EmailAddress and EntityID<>{Convert.ToInt32(customerModel.EntityID)} and EntityTypeID in({(int)EntityType.Client},{(int)EntityType.Branch},{(int)EntityType.User},{(int)EntityType.Customer}) and ClientID={CurrentClientID} and IsDeleted=0", customerModel);

            if (!string.IsNullOrEmpty(email))
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Invalidsubmission",
                    ResponseMessage = "EmailExist"
                });
            }

            var taxNumber = await _dbContext.GetByQueryAsync<string>(@$"Select TaxNumber
                                                                        From Customer
                                                                        Where TaxNumber=@TaxNumber And CustomerID<>@CustomerID And ClientID={CurrentClientID} And IsDeleted=0", customerModel);

            if (!string.IsNullOrEmpty(taxNumber))
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "InvalidSubmission",
                    ResponseMessage = "TaxNumberExist"
                });
            }

            _cn.Open();
            using (var tran = _cn.BeginTransaction())
            {

                int logSummaryID = await _dbContext.InsertAddEditLogSummary(Convert.ToInt32(customerModel.EntityID), "Customer :" + customerModel.Name + " Added/Edited by User " + CurrentUserID, tran);
                var customerEntity = new EntityCustom()
                {
                    EntityID = Convert.ToInt32(customerModel.EntityID),
                    EntityTypeID = (int)EntityType.Customer,
                    Phone = customerModel.Phone,
                    EmailAddress = customerModel.EmailAddress,
                    ClientID = CurrentClientID,
                    CountryID = customerModel.CountryID
                };
                customerEntity.EntityID = await _dbContext.SaveAsync(customerEntity, tran, logSummaryID: logSummaryID);
                customerModel.EntityID = customerEntity.EntityID;

                if (customerModel.Type == (int)CustomerTypes.Business)
                {
                    EntityInstituteInfo entityInstituteInfo = new()
                    {
                        EntityInstituteInfoID = customerModel.EntityInstituteInfoID,
                        EntityID = customerEntity.EntityID,
                        Name = customerModel.CompanyName
                    };
                    entityInstituteInfo.EntityInstituteInfoID = await _dbContext.SaveAsync(entityInstituteInfo, tran);
                }
                    var entityPersonal = new EntityPersonalInfo()
                    {
                        EntityPersonalInfoID = customerModel.EntityPersonalInfoID,
                        EntityID = customerEntity.EntityID,
                        FirstName = customerModel.Name,
                    };
                    await _dbContext.SaveAsync(entityPersonal, tran);
                

                Customer customer = new()
                {
                    CustomerID = customerModel.CustomerID,
                    EntityID = customerEntity.EntityID,
                    Status = customerModel.Status,
                    Type = customerModel.Type,
                    Remarks = customerModel.Remarks,
                    ClientID = CurrentClientID,
                    TaxNumber = customerModel.TaxNumber
                };

                customerModel.CustomerID = await _dbContext.SaveAsync(customer, tran, logSummaryID: logSummaryID);
                CustomerAddResultModel returnObject = new() { EntityID = customerEntity.EntityID, Name = customerModel.Name };
                tran.Commit();
                return Ok(returnObject);
            }
        }

        [HttpPost("save-customer-new")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
        public async Task<IActionResult> SaveCustomerNew(CustomerModelNew customerModel)
        {
            //var phone = await _dbContext.GetByQueryAsync<string>(@$"Select Phone 
            //                                                                from Entity
            //                                                                where Phone=@Phone and EntityID<>{Convert.ToInt32(customerModel.EntityID)} and ClientID={CurrentClientID} and IsDeleted=0", customerModel);

            //if (!string.IsNullOrEmpty(phone))
            //{
            //    return BadRequest(new BaseErrorResponse()
            //    {
            //        ErrorCode = 0,
            //        ResponseTitle = "Invalidsubmission",
            //        ResponseMessage = "PhoneNumberExist"
            //    });
            //}

            var email = await _dbContext.GetByQueryAsync<string>(@$"Select EmailAddress 
                                                                            from Entity
                                                                            where EmailAddress=@EmailAddress and EntityID<>{Convert.ToInt32(customerModel.EntityID)} and EntityTypeID in({(int)EntityType.Client},{(int)EntityType.Branch},{(int)EntityType.User},{(int)EntityType.Customer}) and ClientID={CurrentClientID} and IsDeleted=0", customerModel);

            if (!string.IsNullOrEmpty(email))
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Invalidsubmission",
                    ResponseMessage = "EmailExist"
                });
            }

            var taxNumber = await _dbContext.GetByQueryAsync<string>(@$"Select TaxNumber
                                                                        From Customer
                                                                        Where TaxNumber=@TaxNumber And CustomerID<>@CustomerID And ClientID={CurrentClientID} And IsDeleted=0", customerModel);

            if (!string.IsNullOrEmpty(taxNumber))
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "InvalidSubmission",
                    ResponseMessage = "TaxNumberExist"
                });
            }

            _cn.Open();
            using (var tran = _cn.BeginTransaction())
            {

                int logSummaryID = await _dbContext.InsertAddEditLogSummary(Convert.ToInt32(customerModel.EntityID), "Customer :" + customerModel.Name + " Added/Edited by User " + CurrentUserID, tran);
                var customerEntity = new EntityCustom()
                {
                    EntityID = Convert.ToInt32(customerModel.EntityID),
                    EntityTypeID = (int)EntityType.Customer,
                    Phone = customerModel.Phone,
                    EmailAddress = customerModel.EmailAddress,
                    ClientID = CurrentClientID,
                    CountryID = customerModel.CountryID
                };
                customerEntity.EntityID = await _dbContext.SaveAsync(customerEntity, tran, logSummaryID: logSummaryID);
                customerModel.EntityID = customerEntity.EntityID;

                if (customerModel.Type == (int)CustomerTypes.Business)
                {
                    EntityInstituteInfo entityInstituteInfo = new()
                    {
                        EntityInstituteInfoID = customerModel.EntityInstituteInfoID,
                        EntityID = customerEntity.EntityID,
                        Name = customerModel.CompanyName
                    };
                    entityInstituteInfo.EntityInstituteInfoID = await _dbContext.SaveAsync(entityInstituteInfo, tran);
                }
                var entityPersonal = new EntityPersonalInfo()
                {
                    EntityPersonalInfoID = customerModel.EntityPersonalInfoID,
                    EntityID = customerEntity.EntityID,
                    FirstName = customerModel.Name,
                };
                await _dbContext.SaveAsync(entityPersonal, tran);


                Customer customer = new()
                {
                    CustomerID = customerModel.CustomerID,
                    EntityID = customerEntity.EntityID,
                    Status = customerModel.Status,
                    Type = customerModel.Type,
                    Remarks = customerModel.Remarks,
                    ClientID = CurrentClientID,
                    TaxNumber = customerModel.TaxNumber
                };

                customerModel.CustomerID = await _dbContext.SaveAsync(customer, tran, logSummaryID: logSummaryID);
                CustomerAddResultModel returnObject = new() { EntityID = customerEntity.EntityID, Name = customerModel.Name };
                tran.Commit();
                return Ok(returnObject);
            }
        }

        [HttpGet("get-customer/{customerEntityID}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
        public async Task<IActionResult> GetCustomer(int customerEntityID)
        {
            var customer = await _dbContext.GetByQueryAsync<CustomerModel>($@"
                                                    Select 
                                                    Case
                                                        When Type={(int)CustomerTypes.Business} Then EI.Name 
                                                        Else EP.FirstName 
                                                        End as Name,
                                                    EP.EntityPersonalInfoID,E.EntityTypeID,E.EmailAddress,E.Phone,E.MediaID,E.EntityID,C.EntityID,
                                                    C.Status,C.Type,C.Remarks,C.CustomerID,EntityInstituteInfoID,WC.ContactID,E.CountryID,CT.CountryName,CT.ISDCode,C.TaxNumber
                                                    From Customer C
                                                    Left Join EntityPersonalInfo EP ON EP.EntityID=C.EntityID and EP.IsDeleted=0
                                                    Left Join Entity E ON E.EntityID=C.EntityID and E.IsDeleted=0 
                                                    Left Join Country CT on CT.CountryID=E.CountryID and CT.IsDeleted=0
													Left Join EntityInstituteInfo EI on EI.EntityID=E.EntityID and EI.IsDeleted=0
                                                    Left Join WhatsappContact WC ON C.EntityID=WC.EntityID AND WC.IsDeleted=0
                                                    Where C.EntityID={customerEntityID} and C.ClientID={CurrentClientID} and C.IsDeleted=0", null);
            customer.Addresses = await _entity.GetListOfEntityAddress(customerEntityID, null);
            var contactPerson = await _entity.GetEntityContactPersons(customerEntityID, null);
            if (contactPerson is not null)
                customer.ContactPersons = (List<CustomerContactPersonModel>)contactPerson;
            return Ok(customer ?? new());
        }

        [HttpGet("get-customer-new/{customerEntityID}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
        public async Task<IActionResult> GetCustomerNew(int customerEntityID)
        {
            var customer = await _dbContext.GetByQueryAsync<CustomerModelNew>($@"
                                                    Select 
                                                    EP.EntityPersonalInfoID,E.EntityTypeID,E.EmailAddress,E.Phone,E.MediaID,E.EntityID,C.EntityID,
                                                    C.Status,C.Type,C.Remarks,C.CustomerID,EntityInstituteInfoID,WC.ContactID,E.CountryID,CT.CountryName,
                                                    CT.ISDCode,C.TaxNumber,EI.Name as CompanyName,EP.FirstName as Name
                                                    From Customer C
                                                    Left Join EntityPersonalInfo EP ON EP.EntityID=C.EntityID and EP.IsDeleted=0
                                                    Left Join Entity E ON E.EntityID=C.EntityID and E.IsDeleted=0 
                                                    Left Join Country CT on CT.CountryID=E.CountryID and CT.IsDeleted=0
													Left Join EntityInstituteInfo EI on EI.EntityID=E.EntityID and EI.IsDeleted=0
                                                    Left Join WhatsappContact WC ON C.EntityID=WC.EntityID AND WC.IsDeleted=0
                                                    Where C.EntityID={customerEntityID} and C.ClientID={CurrentClientID} and C.IsDeleted=0", null);
            var addresses = await _entity.GetListOfEntityAddress(customerEntityID, null);
            foreach (var address in addresses)
            {
                customer
                    .Addresses
                    .Add(_entity.GetEntityAddressView(address));
            }
            var contactPerson = await _entity.GetEntityContactPersons(customerEntityID, null);
            if (contactPerson is not null)
                customer.ContactPersons = (List<CustomerContactPersonModel>)contactPerson;
            return Ok(customer ?? new());
        }

        [HttpPost("get-customers-paged-list")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
        public async Task<IActionResult> GetCustomerPagedList(PagedListPostModelWithFilter pagedListPostModel)
        {
            PagedListQueryModel query = pagedListPostModel;
            query.Select = $@"Select C.EntityID,E.Name,E.Phone,E.EmailAddress,C.Status,C.Type
                                            From Customer C
                                            Join viEntity E ON E.EntityID=C.EntityID";

            query.WhereCondition = $"C.IsDeleted=0 and C.ClientID={CurrentClientID}";
            query.OrderByFieldName = pagedListPostModel.OrderByFieldName;
            query.WhereCondition += !string.IsNullOrEmpty(pagedListPostModel.SearchString) ? $" and E.Name like '%{pagedListPostModel.SearchString}%'" : "";
            var result = await _dbContext.GetPagedList<CustomerListModel>(query, null);
            return Ok(result);
        }

        [HttpGet("get-customer-menu-list")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
        public async Task<IActionResult> GetCustomerMenuList()
        {
            var result = await _dbContext.GetListByQueryAsync<ViewPageMenuModel>($@"
                                                                            Select C.EntityID as ID,E.Name as MenuName
                                                                            From Customer C
                                                                            Join viEntity E ON E.EntityID=C.EntityID
                                                                            Where C.IsDeleted=0 and C.ClientID={CurrentClientID}
                                                                            Order By 1 Desc
            ", null);
            return Ok(result);
        }

        [HttpGet("delete-customer/{customerEntityID}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
        public async Task<IActionResult> DeleteCustomer(int customerEntityID)
        {
            int enquiryCount = await _dbContext.GetByQueryAsync<int>($@"Select Count(*) From Enquiry Where BranchID={CurrentBranchID} and CustomerEntityID={customerEntityID} and IsDeleted=0", null);

            if (enquiryCount > 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Invalid operation",
                    ResponseMessage = "Enquiry entry exist with the customer you are trying to remove"
                });
            }

            int quotationCount = await _dbContext.GetByQueryAsync<int>($@"Select Count(*) From Quotation Where BranchID={CurrentBranchID} and CustomerEntityID={customerEntityID} and IsDeleted=0", null);

            if (quotationCount > 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Invalid operation",
                    ResponseMessage = "Quotation entry exist with the customer you are trying to remove"
                });
            }

            _cn.Open();
            using (var tran = _cn.BeginTransaction())
            {
                string customerName = await _dbContext.GetByQueryAsync<string>($"Select Name From viEntity Where EntityID={customerEntityID}", null, tran);
                int logSummaryID = await _dbContext.InsertDeleteLogSummary(customerEntityID, "Customer : " + customerName + " Deleted by User " + CurrentUserID, tran);

                List<int> customerContactPersons = await _dbContext.GetListByQueryAsync<int>(@$"
                                                Select ContactPersonID 
                                                From CustomerContactPerson
                                                Where CustomerEntityID={customerEntityID} and IsDeleted=0", null, tran);

                if (customerContactPersons.Count > 0)
                {
                    for (int i = 0; i < customerContactPersons.Count; i++)
                    {
                        await _dbContext.ExecuteAsync($@"
                                                Update CustomerContactPerson 
                                                Set IsDeleted=1 
                                                Where ContactPersonID={customerContactPersons[i]}
                        ", null, tran);
                    }
                }

                List<int> customerAddressList = await _dbContext.GetListByQueryAsync<int>($@"
                                                Select AddressID
                                                From EntityAddress
                                                Where EntityID={customerEntityID} and IsDeleted=0
                ", null, tran);

                if (customerAddressList.Count > 0)
                {
                    for (int i = 0; i < customerAddressList.Count; i++)
                    {
                        await _dbContext.ExecuteAsync($@"Update EntityAddress
                                            Set IsDeleted=1
                                            Where AddressID={customerAddressList[i]} and IsDeleted=0
                        ", null, tran);
                    }
                }

                await _dbContext.ExecuteAsync($@"Update Customer
                                            Set IsDeleted=1
                                            Where EntityID={customerEntityID}
                        ", null, tran);

                await _dbContext.ExecuteAsync($@"Update WhatsappContact Set EntityID=null
                                            Where EntityID={customerEntityID}
                ", null, tran);

                await _dbContext.DeleteAsync<Entity>(customerEntityID, tran, logSummaryID: logSummaryID);
                await _dbContext.DeleteAsync<EntityPersonalInfo>(customerEntityID, tran);
                tran.Commit();
                return Ok(true);
            }
        }

        [HttpGet("get-customer-details/{customerEntityID}")]
        public async Task<IActionResult> GetCustomerData(int customerEntityID)
        {
            CustomerOrSupplierDataModel customerData = new();
            customerData.TaxNumber = await _dbContext.GetByQueryAsync<string>($@"SELECT TaxNumber FROM Customer WHERE EntityID={customerEntityID}", null);
            var customerAddresses = await _entity.GetListOfEntityAddress(customerEntityID);
            if (customerAddresses != null)
            {
                List<AddressView> customerAddressViews = new();
                foreach (var customerAddress in customerAddresses)
                {
                    AddressView customerAddressView = _entity.GetEntityAddressView(customerAddress);
                    customerAddressViews.Add(customerAddressView);
                }
                customerData.CustomerAddresses = customerAddressViews;
            }
            var contactPersons = await _entity.GetEntityContactPersons(customerEntityID);
            if (contactPersons is not null)
            {
                List<MailRecipentsModel> MailRecipientsModels = new();
                foreach (var customerContactPerson in (List<CustomerContactPersonModel>)contactPersons)
                {
                    MailRecipentsModel customerMailRecipient = new()
                    {
                        EntityID = customerContactPerson.EntityID.Value,
                        EmailAddress = customerContactPerson.Email
                    };
                    customerData.MailReceipients.Add(customerMailRecipient);
                }
            }
            customerData.ISDCode = await _dbContext.GetByQueryAsync<string?>(@$"Select ISDCode 
                                                                From Entity E
                                                                Join Country C ON C.CountryID=E.CountryID And C.IsDeleted=0
															    Where E.EntityID={customerEntityID} and E.IsDeleted=0", null);
            customerData.CountryID = await _dbContext.GetByQueryAsync<int?>($"Select CountryID From Entity Where EntityID={customerEntityID}", null);
            if (customerData.CountryID is not null)
                customerData.CurrencyID = await _dbContext.GetByQueryAsync<int?>($"Select CurrencyID From Country Where CountryID={customerData.CountryID.Value}", null);
            int ledgerID = await _accounts.GetEntityLedgerID(customerEntityID, CurrentClientID);
            customerData.Advances = await _dbContext.GetListByQueryAsync<BillToBillAgainstReferenceModel>($@"Select B.BillID,B.ReferenceNo,B.Date,vB.Credit As BillAmount
                                                                    From AccBilltoBill B
                                                                    Join viBillToBillBalance vB ON B.BillID=vB.BillID
                                                                    Where B.LedgerID={ledgerID} And B.ReferenceTypeID={(int)ReferenceTypes.Advance}", null);

            return Ok(customerData);
        }

        [HttpPost("update-customer-email")]
        public async Task<IActionResult> UpdateCustomerEmail(CustomerMailUpdateModel model)
        {
            var email = await _dbContext.GetByQueryAsync<string>(@$"Select EmailAddress 
                                                                            from Entity
                                                                            where EmailAddress=@EmailAddress and EntityID<>@CustomerEntityID and EntityTypeID in({(int)EntityType.Branch},{(int)EntityType.User},{(int)EntityType.Customer}) and IsDeleted=0
            ", model);

            if (!string.IsNullOrEmpty(email))
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Invalid submission",
                    ResponseMessage = "The Email already exist,try different email address"
                });
            }

            await _dbContext.ExecuteAsync($"UPDATE Entity SET EmailAddress='{model.EmailAddress}' Where EntityID={model.CustomerEntityID}");
            return Ok(new Success());
        }

        #endregion

        #region Customer Contact Person

        [HttpPost("save-contact-person")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> SaveContactPerson(CustomerContactPersonModel contactPersonModel)
        {
            var phone = await _dbContext.GetByQueryAsync<string>(@$"
                                        Select Phone 
                                        from Entity
                                        where Phone=@Phone and EntityID<>{Convert.ToInt32(contactPersonModel.EntityID)} and ClientID={CurrentClientID} and IsDeleted=0", contactPersonModel);

            if (phone != null)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Duplication not allowed",
                    ResponseMessage = "Phone number already exist,try different phone numner"
                });
            }

            if (!string.IsNullOrEmpty(contactPersonModel.Email))
            {
                var email = await _dbContext.GetByQueryAsync<string>(@$"
                                        Select EmailAddress 
                                        from Entity
                                        where EmailAddress=@Email and EntityID<>{Convert.ToInt32(contactPersonModel.EntityID)} and ClientID={CurrentClientID} and IsDeleted=0", contactPersonModel);

                if (email != null)
                {
                    return BadRequest(new BaseErrorResponse()
                    {
                        ErrorCode = 0,
                        ResponseTitle = "Duplication",
                        ResponseMessage = "Email already exist, use different email and try again.."
                    });
                }
            }

            var entity = new Entity()
            {
                EntityID = Convert.ToInt32(contactPersonModel.EntityID),
                EntityTypeID = (int)EntityType.ContactPerson,
                Phone = contactPersonModel.Phone,
                EmailAddress = contactPersonModel.Email,
                ClientID = CurrentClientID
            };
            entity.EntityID = await _dbContext.SaveAsync(entity);

            var entityPersonal = new EntityPersonalInfo()
            {
                EntityPersonalInfoID = contactPersonModel.EntityPersonalInfoID,
                EntityID = entity.EntityID,
                FirstName = contactPersonModel.Name,
            };
            await _dbContext.SaveAsync(entityPersonal);

            var person = new CustomerContactPerson()
            {
                ContactPersonID = contactPersonModel.ContactPersonID,
                EntityID = entity.EntityID,
                Department = contactPersonModel.Department,
                Designation = contactPersonModel.Designation,
                CustomerEntityID = contactPersonModel.CustomerEntityID,
            };
            person.ContactPersonID = await _dbContext.SaveAsync(person);

            ContactPersonAddResultModel returnObject = new()
            {
                ContactPersonID = person.ContactPersonID,
                EntityID = entity.EntityID,
                EntityPersonalInfoID = entityPersonal.EntityPersonalInfoID,
                Name = contactPersonModel.Name,
                EmailAddress = contactPersonModel.Email,
                ResponseTitle = "Succes",
                ResponseMessage = "Contact Person added successfully"
            };
            return Ok(returnObject);
        }

        [HttpGet("delete-contact-person/{contactPersonID}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteContactPerson(int contactPersonID)
        {
            int entityID = await _dbContext.GetFieldsAsync<CustomerContactPerson, int>("EntityID", $"ContactPersonID={contactPersonID} and IsDeleted=0", null);
            await _dbContext.ExecuteAsync($@"Update CustomerContactPerson Set IsDeleted=1 Where ContactPersonID={contactPersonID}");
            await _dbContext.ExecuteAsync($@"Update Entity Set IsDeleted=1 Where EntityID={entityID}");
            await _dbContext.ExecuteAsync($@"Update EntityPersonalInfo Set IsDeleted=1 Where EntityID={entityID}");
            return Ok(true);
        }

        [HttpGet("get-customer-contact-person/{contactPersonID}")]
       // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetContactPerson(int contactPersonID)
        {
            var result = await _dbContext.GetByQueryAsync<CustomerContactPersonModel>($@"Select
                                        E.EmailAddress As Email,E.Phone As Phone,EIP.EntityPersonalInfoID,EIP.FirstName As Name,CCP.*
                                        From CustomerContactPerson CCP
                                        Left Join EntityPersonalInfo EIP ON CCP.EntityID=EIP.EntityID and EIP.IsDeleted=0
                                        Left Join Entity E ON E.EntityID=CCP.EntityID and E.IsDeleted=0
                                        Where CCP.ContactPersonID={contactPersonID} and CCP.IsDeleted=0", null);
            return Ok(result);
        }

        #endregion

        #region Customer Address

        [HttpPost("save-customer-address")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> SaveAddress(AddressModel addressmodel)
        {
            var address = _mapper.Map<EntityAddressCustom>(addressmodel);

            addressmodel.AddressID = address.AddressID = await _dbContext.SaveAsync(address);

            var addressView = _entity.GetEntityAddressView(addressmodel);
            AddressAddResultModel returnObject = new()
            {
                AddressID = address.AddressID,
                CompleteAddress = addressView.CompleteAddress,
            };
            return Ok(returnObject);
        }

        [HttpGet("get-customer-address/{addressID}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetAddress(int addressID)
        {
            var res = await _dbContext.GetByQueryAsync<AddressModel>($@"Select EA.*,C.CountryName,C.ISDCode
                                                            From EntityAddress EA
                                                            LEFT JOIN Country C ON C.CountryID = EA.CountryID
                                                            Where EA.IsDeleted=0 and EA.AddressID={addressID}", null);
            return Ok(res);
        }

        [HttpGet("get-customer-address-view/{addressID}")]
        public async Task<IActionResult> GetCustomerAddressView(int addressID)
        {
            AddressView customerAddressView = await _entity.GetAddressView(addressID);
            return Ok(customerAddressView);
        }

        [HttpGet("delete-address/{addressID}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteAddress(int addressID)
        {
            try
            {
                await _dbContext.ExecuteAsync($@"Update EntityAddress Set IsDeleted=1 Where AddressID={addressID}");
                return Ok(true);
            }
            catch (Exception err)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Something went wrong",
                    ResponseMessage = err.Message
                });
            }
        }

        #endregion
    }
}
