using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PB.EntityFramework;
using PB.Model;
using PB.Model.Models;
using PB.Server.Repository;
using PB.Shared.Enum;
using PB.Shared.Models;
using PB.Shared.Models.Common;
using PB.Shared.Models.CRM.Customer;
using PB.Shared.Models.Inventory.Invoices;
using PB.Shared.Models.Inventory.Supplier;
using PB.Shared.Tables;
using PB.Shared.Tables.Inventory.Suppliers;
using System.Data;

namespace PB.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SupplierController : BaseController
    {
        private readonly IDbContext _dbContext;
        private readonly IDbConnection _cn;
        private readonly IMapper _mapper;
        private readonly ISupplierRepository _supplierRepository;
        private readonly IEntityRepository _entity;
        private readonly ICommonRepository _common;
        private readonly IAccountRepository _accounts;
        public SupplierController(IDbContext dbContext, IDbConnection cn, IMapper mapper, ISupplierRepository supplierRepository, IEntityRepository entity, ICommonRepository common, IAccountRepository accounts)
        {
            _cn = cn;
            _dbContext = dbContext;
            _mapper = mapper;
            _supplierRepository = supplierRepository;
            _entity = entity;
            _common = common;
            _accounts = accounts;
        }

        #region Supplier

        [HttpPost("save-supplier")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> SaveSupplier(SupplierModel supplierModel)
        {
            var phone = await _dbContext.GetByQueryAsync<string>(@$"Select Phone 
                                                                            from Entity
                                                                            where Phone=@Phone and EntityID<>@EntityID and ClientID={CurrentClientID} and IsDeleted=0 and EntityTypeID={(int)EntityType.Supplier}", supplierModel);

            if (!string.IsNullOrEmpty(phone))
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "InvalidSubmission",
                    ResponseMessage = "PhoneNumberExist"
                });
            }

            var email = await _dbContext.GetByQueryAsync<string>(@$"Select EmailAddress 
                                                                            from Entity
                                                                            where EmailAddress=@EmailAddress and EntityID<>@EntityID and EntityTypeID in({(int)EntityType.Client},{(int)EntityType.Branch},{(int)EntityType.User},{(int)EntityType.Customer},{(int)EntityType.Supplier}) and ClientID={CurrentClientID} and IsDeleted=0", supplierModel);

            if (!string.IsNullOrEmpty(email))
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "InvalidSubmission",
                    ResponseMessage = "EmailExist"
                });
            }

            var taxNumber = await _dbContext.GetByQueryAsync<string>(@$"Select TaxNumber
                                                                        From Supplier
                                                                        Where TaxNumber=@TaxNumber And SupplierID<>@SupplierID And ClientID={CurrentClientID} And IsDeleted=0", supplierModel);

            if (!string.IsNullOrEmpty(taxNumber))   
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "InvalidSubmission",
                    ResponseMessage = "TaxNumberExist"
                });
            }

            //Entity
            var supplierEntity = _mapper.Map<EntityCustom>(supplierModel);
            supplierEntity.EntityTypeID = (int)EntityType.Supplier;
            supplierEntity.ClientID = CurrentClientID;
            supplierModel.EntityID= supplierEntity.EntityID = await _dbContext.SaveAsync(supplierEntity);

            //EntityInstituteInfo
            var supplierEntityInstituteInfo = _mapper.Map<EntityInstituteInfo>(supplierModel);
            supplierEntityInstituteInfo.EntityInstituteInfoID = await _dbContext.SaveAsync(supplierEntityInstituteInfo);

            //Supplier
            var supplier = _mapper.Map<Supplier>(supplierModel);
            supplier.ClientID = CurrentClientID;
            supplier.SupplierID = await _dbContext.SaveAsync(supplier);

            return Ok(new SupplierSaveSuccessModel()
            {
                SupplierID = supplier.SupplierID,
                SupplierEntityID= supplierModel.EntityID,
                ResponseTitle = "SaveSuccess",
                ResponseMessage = "SupplierSaveSuccess"
            });
        }

        [HttpGet("get-supplier/{supplierEntityID}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetSupplier(int supplierEntityID)
        {
            var supplier = await _dbContext.GetByQueryAsync<SupplierModel>(@$"Select E.EntityID,E.EntityTypeID,E.Phone,E.Phone2,E.EmailAddress,E.MediaID,EI.EntityInstituteInfoID,
                                                                            EI.Name,S.SupplierID,S.Status,S.Remarks,S.TaxNumber,E.CountryID,CT.CountryName,ISDCode
                                                                            From Supplier S
                                                                            Left Join Entity E ON E.EntityID=S.EntityID
                                                                            Left Join EntityInstituteInfo EI ON EI.EntityID=S.EntityID
                                                                            Left Join Country CT on CT.CountryID=E.CountryID and CT.IsDeleted=0
                                                                            Where S.EntityID={supplierEntityID} and S.ClientID={CurrentClientID} and S.IsDeleted=0", null);
            if (supplier is not null)
            {
                supplier.Addresses = await _supplierRepository.GetListOfSupplierAddress(supplierEntityID);
                supplier.ContactPersons = await _supplierRepository.GetSupplierContactPersons(supplierEntityID, null);
            }
            return Ok(supplier ?? new());
        }

        [HttpPost("get-supplier-paged-list")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetSupplierPagedList(PagedListPostModelWithFilter pagedListPostModel) 
        {
            PagedListQueryModel query = pagedListPostModel;
            query.Select = $@"Select S.EntityID,E.Name,E.Phone,E.EmailAddress,S.Status
                                            From Supplier S
                                            Join viEntity E ON E.EntityID=S.EntityID";
            query.WhereCondition = $"S.IsDeleted=0 and S.ClientID={CurrentClientID}";
            query.OrderByFieldName = pagedListPostModel.OrderByFieldName;
            query.WhereCondition += _common.GeneratePagedListFilterWhereCondition(pagedListPostModel, "E.Name");
            var result = await _dbContext.GetPagedList<SupplierListModel>(query, null);
            return Ok(result);
        }

        [HttpGet("get-supplier-menu-list")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetSupplierMenuList() 
        {
            var result = await _dbContext.GetListByQueryAsync<ViewPageMenuModel>($@"Select S.EntityID as ID,E.Name as MenuName
                                                                            From Supplier S
                                                                            Join viEntity E ON E.EntityID=S.EntityID
                                                                            Where S.IsDeleted=0 and S.ClientID={CurrentClientID}
                                                                            Order By 1 Desc", null);
            return Ok(result);
        }

        [HttpGet("delete-supplier/{supplierEntityID}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteCustomer(int supplierEntityID)
        {

            int invoiceCount = await _dbContext.GetByQueryAsync<int>($@"Select Count(*) From Invoice Where BranchID={CurrentBranchID} and SupplierEntityID={supplierEntityID} and IsDeleted=0", null);

            if (invoiceCount > 0)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ErrorCode = 0,
                    ResponseTitle = "Invalid operation",
                    ResponseMessage = "Invoie entry exist with the supplier you are trying to remove"
                });
            }

            _cn.Open();
            using (var tran = _cn.BeginTransaction())
            {
                string supplierName = await _dbContext.GetByQueryAsync<string>($"Select Name From viEntity Where EntityID={supplierEntityID}", null, tran);
                int logSummaryID = await _dbContext.InsertDeleteLogSummary(supplierEntityID, "Supplier : " + supplierName + " Deleted by User " + CurrentUserID, tran);

                List<int> supplierContactPersons = await _dbContext.GetListByQueryAsync<int>(@$"
                                                Select ContactPersonID 
                                                From SupplierContactPerson
                                                Where SupplierEntityID={supplierEntityID} and IsDeleted=0", null, tran);

                if (supplierContactPersons.Count > 0)
                {
                    for (int i = 0; i < supplierContactPersons.Count; i++)
                    {
                        await _dbContext.ExecuteAsync($@"
                                                Update SupplierContactPerson 
                                                Set IsDeleted=1 
                                                Where ContactPersonID={supplierContactPersons[i]}", null, tran);
                    }
                }

                List<int> customerAddressList = await _dbContext.GetListByQueryAsync<int>($@"
                                                Select AddressID
                                                From EntityAddress
                                                Where EntityID={supplierEntityID} and IsDeleted=0", null, tran);

                if (customerAddressList.Count > 0)
                {
                    for (int i = 0; i < customerAddressList.Count; i++)
                    {
                        await _dbContext.ExecuteAsync($@"Update EntityAddress
                                            Set IsDeleted=1
                                            Where AddressID={customerAddressList[i]} and IsDeleted=0", null, tran);
                    }
                }

                await _dbContext.ExecuteAsync($@"Update Supplier
                                            Set IsDeleted=1
                                            Where EntityID={supplierEntityID}", null, tran);

                await _dbContext.ExecuteAsync($@"Update WhatsappContact Set EntityID=null
                                            Where EntityID={supplierEntityID}", null, tran);

                await _dbContext.DeleteAsync<Entity>(supplierEntityID, tran, logSummaryID: logSummaryID);
                await _dbContext.DeleteAsync<EntityPersonalInfo>(supplierEntityID, tran);
                tran.Commit();
                return Ok(true);
            }
        }

        [HttpGet("get-supplier-details/{supplierEntityID}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetCustomerData(int supplierEntityID)
        {
            CustomerOrSupplierDataModel supplierData = new();
            supplierData.TaxNumber = await _dbContext.GetByQueryAsync<string>($@"SELECT TaxNumber FROM Supplier WHERE EntityID={supplierEntityID}", null);
            var customerAddresses = await _entity.GetListOfEntityAddress(supplierEntityID);
            if (customerAddresses != null)
            {
                List<AddressView> supplierAddressViews = new();
                foreach (var customerAddress in customerAddresses)
                {
                    AddressView customerAddressView = _entity.GetEntityAddressView(customerAddress);
                    supplierAddressViews.Add(customerAddressView);
                }
                supplierData.CustomerAddresses = supplierAddressViews;
            }
            var supplierMailRecepients = await _entity.GetEntityContactPersons(supplierEntityID);
            if (supplierMailRecepients != null)
            {
                List<SupplierMailRecipientsModel> supplierMailRecipientsModels = new();
                foreach (var supplierMailRecepient in (List<SupplierContactPersonModel>)supplierMailRecepients) 
                {
                    SupplierMailRecipientsModel supplierMailRecipient = new()
                    {
                        ContactPersonEntityID = supplierMailRecepient.EntityID.Value,
                        EmailAddress = supplierMailRecepient.Email
                    };
                    supplierMailRecipientsModels.Add(supplierMailRecipient);
                }
            }
            supplierData.ISDCode = await _dbContext.GetByQueryAsync<string?>(@$"Select ISDCode 
                                                                From Entity E
                                                                Join Country C ON C.CountryID=E.CountryID And C.IsDeleted=0
															    Where E.EntityID={supplierEntityID} and E.IsDeleted=0", null);
            supplierData.CountryID = await _dbContext.GetByQueryAsync<int?>($"Select CountryID From Entity Where EntityID={supplierEntityID}", null);
            if (supplierData.CountryID is not null)
                supplierData.CurrencyID = await _dbContext.GetByQueryAsync<int?>($"Select CurrencyID From Country Where CountryID={supplierData.CountryID.Value}", null);

            int ledgerID = await _accounts.GetEntityLedgerID(supplierEntityID, CurrentClientID);
            supplierData.Advances = await _dbContext.GetListByQueryAsync<BillToBillAgainstReferenceModel>($@"Select B.BillID,B.ReferenceNo,B.Date,vB.Debit As BillAmount
                                                                    From AccBilltoBill B
                                                                    Join viBillToBillBalance vB ON B.BillID=vB.BillID
                                                                    Where B.LedgerID={ledgerID} And B.ReferenceTypeID={(int)ReferenceTypes.Advance}", null);
            return Ok(supplierData);
        }

        #endregion

        #region Supplier Contact 


        [HttpPost("save-supplier-contact-person")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> SaveContactPerson(SupplierContactPersonModel contactPersonModel)
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

            try
            {
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

                var person = new SupplierContactPerson()
                {
                    ContactPersonID = contactPersonModel.ContactPersonID,
                    EntityID = entity.EntityID,
                    Department = contactPersonModel.Department,
                    Designation = contactPersonModel.Designation,
                    SupplierEntityID = contactPersonModel.SupplierEntityID,
                };
                person.ContactPersonID = await _dbContext.SaveAsync(person);

                SupplierContactPersonAddResultModel returnObject = new()
                {
                    ContactPersonID = person.ContactPersonID,
                    EntityID = entity.EntityID,
                    EntityPersonalInfoID = entityPersonal.EntityPersonalInfoID,
                    Name = contactPersonModel.Name + " <" + contactPersonModel.Email + ">",
                    EmailAddress = contactPersonModel.Email,
                    ResponseTitle = "Succes",
                    ResponseMessage = "Contact Person added successfully"
                };

                return Ok(returnObject);
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

        [HttpGet("get-supplier-contact-person/{contactPersonID}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetContactPerson(int contactPersonID)
        {
            var result = await _dbContext.GetByQueryAsync<SupplierContactPersonModel>($@"
                                        Select
                                        E.EmailAddress As Email,E.Phone As Phone,EIP.EntityPersonalInfoID,EIP.FirstName As Name,CCP.*
                                        From SupplierContactPerson CCP
                                        Left Join EntityPersonalInfo EIP ON CCP.EntityID=EIP.EntityID and EIP.IsDeleted=0
                                        Left Join Entity E ON E.EntityID=CCP.EntityID and E.IsDeleted=0
                                        Where CCP.ContactPersonID={contactPersonID} and CCP.IsDeleted=0
            ", null);
            return Ok(result);
        }

        [HttpGet("delete-supplier-contact-person/{contactPersonID}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteContactPerson(int contactPersonID)
        {
            try
            {
                int entityID = await _dbContext.GetFieldsAsync<SupplierContactPerson, int>("EntityID", $"ContactPersonID={contactPersonID} and IsDeleted=0", null);

                await _dbContext.ExecuteAsync($@"
									Update SupplierContactPerson Set IsDeleted=1 Where ContactPersonID={contactPersonID}
				");

                await _dbContext.ExecuteAsync($@"
									Update Entity Set IsDeleted=1 Where EntityID={entityID}
				");

                await _dbContext.ExecuteAsync($@"
									Update EntityPersonalInfo Set IsDeleted=1 Where EntityID={entityID}
				");

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
