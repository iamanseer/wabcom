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
using PB.Shared.Models.Inventory.Items;
using PB.Shared.Tables.Inventory.Items;
using PB.Shared.Models.Inventory.Item;

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
        public CustomerController(IDbContext dbContext, IDbConnection cn, IMapper mapper, IEntityRepository entity, IAccountRepository accounts)
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
                    TaxNumber = customerModel.TaxNumber,
                    JoinedOn=customerModel.JoinedOn,
                    CategoryID=customerModel.CategoryID,
                    BusinessTypeID= customerModel.BusinessTypeID,
                    AccountantEmailID= customerModel.AccountantEmailID,
                    AccountantContactNo= customerModel.AccountantContactNo,
                    TallyEmailID= customerModel.TallyEmailID,
                    CustomerPriority= customerModel.CustomerPriority,
                    OwnedBy= customerModel.OwnedBy,

                };

                customerModel.CustomerID = await _dbContext.SaveAsync(customer, tran, logSummaryID: logSummaryID);

                CustomerSerialNumbers tallySerialNo = new()
                {
                    ID= customerModel.TallyNos.ID,
                    TallySerialNo= customerModel.TallyNos.TallySerialNo,
                    TallySerialNo2= customerModel.TallyNos.TallySerialNo2,
                    TallySerialNo3= customerModel.TallyNos.TallySerialNo3,
                    CustomerID= customerModel.CustomerID
                };
                customerModel.CustomerID = await _dbContext.SaveAsync(tallySerialNo, tran);

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
                string? customerName = string.IsNullOrEmpty(customerModel.Name)? customerModel.CompanyName : customerModel.Name;
                customerModel.CustomerID = await _dbContext.SaveAsync(customer, tran, logSummaryID: logSummaryID);
                CustomerAddResultModel returnObject = new() { EntityID = customerEntity.EntityID, Name = customerName };
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
                                                        End as Name,EI.Name as CompanyName,EP.FirstName as Name,
                                                    EP.EntityPersonalInfoID,E.EntityTypeID,E.EmailAddress,E.Phone,E.MediaID,E.EntityID,C.EntityID,
                                                    C.Status,C.Type,C.Remarks,C.CustomerID,EntityInstituteInfoID,WC.ContactID,E.CountryID,CT.CountryName,CT.ISDCode,C.TaxNumber,
                                                    JoinedOn,C.CategoryID,CategoryName,C.BusinessTypeID,BusinessTypeName,TallyEmailID,CustomerPriority,OwnedBy,BusinessGivenInNos,
                                                    BusinessGivenInNos,LastBusinessType,AMCStatus,AMCExpiry,SubscriptionExpiry,AccountantEmailID,AccountantContactNo,V.Name as StaffName
                                                    From Customer C
                                                    Left Join EntityPersonalInfo EP ON EP.EntityID=C.EntityID and EP.IsDeleted=0
                                                    Left Join Entity E ON E.EntityID=C.EntityID and E.IsDeleted=0 
                                                    Left Join Country CT on CT.CountryID=E.CountryID and CT.IsDeleted=0
                                                    Left Join EntityInstituteInfo EI on EI.EntityID=E.EntityID and EI.IsDeleted=0
                                                    Left Join WhatsappContact WC ON C.EntityID=WC.EntityID AND WC.IsDeleted=0
                                                    Left Join CustomerCategory CC on CC.CategoryID=C.CategoryID and CC.IsDeleted=0
                                                    Left Join BusinessType BT on BT.BusinessTypeID=C.BusinessTypeID and BT.IsDeleted=0
                                                    Left Join viEntity V on V.EntityID=C.OwnedBy 
                                                    Where C.EntityID={customerEntityID} and C.ClientID={CurrentClientID} and C.IsDeleted=0", null);
            var numbers = await _dbContext.GetByQueryAsync<CustomerSerialNoModel>($@" Select * From CustomerSerialNumbers
                                                                                            Where IsDeleted=0 and CustomerID={customer.CustomerID}", null);
            if (numbers is not null)
                customer.TallyNos = numbers;
            customer.CustomerServices = await _entity.GetCustomerServiceList(customerEntityID, null);
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
                                                    CT.ISDCode,C.TaxNumber,EI.Name as CompanyName,EP.FirstName as Name,
                                                    JoinedOn,C.CategoryID,CategoryName,C.BusinessTypeID,BusinessTypeName,TallyEmailID,CustomerPriority,OwnedBy,BusinessGivenInNos,
                                                    BusinessGivenInNos,LastBusinessType,AMCStatus,AMCExpiry,SubscriptionExpiry,AccountantEmailID,AccountantContactNo,V.Name as StaffName
                                                    From Customer C
                                                    Left Join EntityPersonalInfo EP ON EP.EntityID=C.EntityID and EP.IsDeleted=0
                                                    Left Join Entity E ON E.EntityID=C.EntityID and E.IsDeleted=0 
                                                    Left Join Country CT on CT.CountryID=E.CountryID and CT.IsDeleted=0
													Left Join EntityInstituteInfo EI on EI.EntityID=E.EntityID and EI.IsDeleted=0
                                                    Left Join WhatsappContact WC ON C.EntityID=WC.EntityID AND WC.IsDeleted=0
                                                    Left Join CustomerCategory CC on CC.CategoryID=C.CategoryID and CC.IsDeleted=0
                                                    Left Join BusinessType BT on BT.BusinessTypeID=C.BusinessTypeID and BT.IsDeleted=0
                                                    Left Join viEntity V on V.EntityID=C.OwnedBy 
                                                    Where C.EntityID={customerEntityID} and C.ClientID={CurrentClientID} and C.IsDeleted=0", null);
            var numbers = await _dbContext.GetByQueryAsync<CustomerSerialNoModel>($@" Select * From CustomerSerialNumbers
                                                                                            Where IsDeleted=0 and CustomerID={customer.CustomerID}", null);
            if (numbers is not null)
                customer.TallyNos = numbers;

            customer.CustomerServices = await _entity.GetCustomerServiceList(customerEntityID,null);
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

        #region Customer Category


        [HttpPost("save-customer-category")]
        public async Task<IActionResult> SaveCustomerCategory(CustomerCategoryModel model)
        {
            var customerCategoryCount = await _dbContext.GetByQueryAsync<int>(@$"Select Count(*) 
                                                                            from CustomerCategory
                                                                            where LOWER(CategoryName)=LOWER(@CategoryName) and CategoryID<>@CategoryID and ClientID={CurrentClientID} and IsDeleted=0", model);
            if (customerCategoryCount != 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "InvalidSubmission",
                    ResponseMessage = "Customer category is already exist"
                });
            }

            CustomerCategory category = _mapper.Map<CustomerCategory>(model);
            category.ClientID = CurrentClientID;
            category.CategoryID = await _dbContext.SaveAsync(category);
            CustomerCategorySuccessModel customerCategorySuccessModel = new()
            {
                CategoryID = category.CategoryID,
                CategoryName = model.CategoryName
            };
            return Ok(customerCategorySuccessModel);
        }

        [HttpGet("delete-customer-category/{categoryID}")]
        public async Task<IActionResult> DeleteCustomerCategory(int categoryID)
        {
            int customerCount = await _dbContext.GetByQueryAsync<int>($@"Select Count(C.CategoryID)
                                                                        From Customer C
                                                                        Left Join CustomerCategory CC on CC.CategoryID=C.CategoryID and CC.IsDeleted=0
                                                                        Where C.IsDeleted=0 and C.CategoryID={categoryID}", null);

            if (customerCount > 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "InvalidSubmission",
                    ResponseMessage = "Customer category in use"
                });
            }

            await _dbContext.DeleteAsync<CustomerCategory>(categoryID, null);
            // await _dbContext.ExecuteAsync($"Update CustomerCategory Set IsDeleted=1 Where CategoryID={categoryID}", null);
            return Ok(true);
        }

        [HttpGet("get-customer-category/{categoryID}")]
        public async Task<IActionResult> GetCustomerCategory(int categoryID)
        {
            return Ok(await _dbContext.GetByQueryAsync<CustomerCategoryModel>($"Select * From CustomerCategory Where CategoryID={categoryID} and IsDeleted=0", null));
        }

        [HttpPost("get-customer-category-paged-list")]
        public async Task<IActionResult> GetCustomerCategoryPagedList(PagedListPostModel pagedListPostModel)
        {
            PagedListQueryModel queryModel = new();
            queryModel.Select = @"Select CategoryID, CategoryName
                                            From CustomerCategory ";
            queryModel.WhereCondition = $"ClientID={CurrentClientID} and IsDeleted=0";
            queryModel.OrderByFieldName = pagedListPostModel.OrderByFieldName;
            queryModel.SearchLikeColumnNames = new() { "CategoryName" };
            queryModel.SearchString = pagedListPostModel.SearchString;
            var itemCategoryList = await _dbContext.GetPagedList<CustomerCategoryModel>(queryModel, null);
            return Ok(itemCategoryList ?? new());
        }


        #endregion

        #region Customer Subscription


        [HttpPost("save-customer-subscription")]
        public async Task<IActionResult> SaveCustomerSubscription(CustomerSubscriptionModel model)
        {
            var customerCategoryCount = await _dbContext.GetByQueryAsync<int>(@$"Select Count(*) 
                                                                            from CustomerSubscription
                                                                            where LOWER(SubscriptionName)=LOWER(@SubscriptionName) and SubscriptionID<>@SubscriptionID and ClientID={CurrentClientID} and IsDeleted=0", model);
            if (customerCategoryCount != 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "InvalidSubmission",
                    ResponseMessage = "Customer subscription is already exist"
                });
            }

            CustomerSubscription subscription = _mapper.Map<CustomerSubscription>(model);
            subscription.ClientID = CurrentClientID;
            subscription.SubscriptionID = await _dbContext.SaveAsync(subscription);
            CustomerSubscriptionSuccessModel customerCategorySuccessModel = new()
            {
                SubscriptionID = subscription.SubscriptionID,
                SubscriptionName = model.SubscriptionName
            };
            return Ok(customerCategorySuccessModel);
        }

        [HttpGet("delete-customer-subscription/{subscriptionID}")]
        public async Task<IActionResult> DeleteCustomerSubscription(int subscriptionID)
        {
            int customerCount = await _dbContext.GetByQueryAsync<int>($@"Select Count(C.SubscriptionID)
                                                                        From Customer C
                                                                        Left Join CustomerSubscription CC on CC.SubscriptionID=C.SubscriptionID and CC.IsDeleted=0
                                                                        Where C.IsDeleted=0 and C.SubscriptionID={subscriptionID}", null);

            if (customerCount > 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "InvalidSubmission",
                    ResponseMessage = "Customer subscription in use"
                });
            }

           // await _dbContext.DeleteAsync<CustomerSubscription>(subscriptionID, null);
            await _dbContext.ExecuteAsync($"Update CustomerSubscription Set IsDeleted=1 Where SubscriptionID={subscriptionID}", null);
            return Ok(true);
        }

        [HttpGet("get-customer-subscription/{subscriptionID}")]
        public async Task<IActionResult> GetCustomerSubscription(int subscriptionID)
        {
            return Ok(await _dbContext.GetByQueryAsync<CustomerSubscriptionModel>($"Select * From CustomerSubscription Where SubscriptionID={subscriptionID} and IsDeleted=0", null));
        }

        [HttpPost("get-customer-subscription-paged-list")]
        public async Task<IActionResult> GetCustomerSubscriptionPagedList(PagedListPostModel pagedListPostModel)
        {
            PagedListQueryModel queryModel = new();
            queryModel.Select = @"Select SubscriptionID, SubscriptionName
                                            From CustomerSubscription ";
            queryModel.WhereCondition = $"ClientID={CurrentClientID} and IsDeleted=0";
            queryModel.OrderByFieldName = pagedListPostModel.OrderByFieldName;
            queryModel.SearchLikeColumnNames = new() { "SubscriptionName" };
            queryModel.SearchString = pagedListPostModel.SearchString;
            var subscriptionList = await _dbContext.GetPagedList<CustomerSubscriptionModel>(queryModel, null);
            return Ok(subscriptionList ?? new());
        }


        #endregion

        #region Customer Service

        [HttpGet("get-customer-service-item-details/{id}")]
        public async Task<IActionResult>GetServiceItem(int id)
        {
            var res = await _dbContext.GetByQueryAsync<ItemListModel>($@"Select  ItemID,ItemName,IsNull(IsSubscription,0) as IsSubscription From Item Where ItemID={id} and IsDeleted=0", null);
            return Ok(res??new());
        }

        [HttpGet("get-customer-service/{customerEntityID}")]
        public async Task<IActionResult> GetServiceCustomer(int customerEntityID)
        {
            var res = await _dbContext.GetListByQueryAsync<CustomerServiceModel>($@"Select Row_Number() Over(Order By S.ServiceID) As RowIndex,S.*,ItemName as ServiceName,IsSubscription
                                                                        From CustomerServices S
                                                                        Left Join Item I on I.ItemID=S.ItemID and I.IsDeleted=0
                                                                        Where S.CustomerEntityID={customerEntityID} and S.IsDeleted=0", null);
            return Ok(res ?? new());
        }

        [HttpGet("get-customer-service-details/{serviceID}")]
        public async Task<IActionResult> GetService(int serviceID)
        {
            var res = await _dbContext.GetByQueryAsync<CustomerServiceModel>($@"Select S.*,ItemName as ServiceName,IsSubscription
                                                                        From CustomerServices S
                                                                        Left Join Item I on I.ItemID=S.ItemID and I.IsDeleted=0
                                                                        Where S.ServiceID={serviceID} and S.IsDeleted=0", null);
            return Ok(res??new());
        }

        [HttpPost("save-customer-service")]
        public async Task<IActionResult>SaveService(CustomerServiceModel model)
        {

            //var sameServiceData = await _dbContext.GetByQueryAsync<int>($@"Select Count(*) From CustomerServices 
            //                                                            Where ItemID=@ItemID and Date=@Date and Quantity=@Quantity and 
            //                                                            Amount=@Amount and ExpiryDate=@ExpiryDate and 
            //                                                            CustomerEntityID=@CustomerEntityID and ServiceID<>@ServiceID", model);
            //if(sameServiceData!=0)
            //{
            //    return BadRequest(new BaseErrorResponse()
            //    {
            //        ErrorCode = 0,
            //        ResponseTitle = "Invalid submission",
            //        ResponseMessage = "This entry is alreaddy exist in same customer"
            //    });
            //}
            CustomerServices service = new()
            {
                ServiceID = model.ServiceID,
                ItemID = model.ItemID,
                CustomerEntityID = model.CustomerEntityID,
                Date = model.Date,
                Quantity = model.Quantity,
                Amount=model.Amount,
                ExpiryDate=model.ExpiryDate
            };
            service.ServiceID=await _dbContext.SaveAsync(service);
            CustomerServiceSuccessModel result = new() {ServiceID= service.ServiceID ,ServiceName=model.ServiceName};
            return Ok(result??new());
        }


        [HttpGet("delete-customer-service/{serviceID}")]
        public async Task<IActionResult> DeleteCustomerService(int serviceID)
        {
            await _dbContext.ExecuteAsync($"Update CustomerServices SET IsDeleted=1 Where ServiceID={serviceID}");
            return Ok(true);
        }
        #endregion
    }
}
