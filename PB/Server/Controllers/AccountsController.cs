using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto;
using PB.Client.Pages.Accounts;
using PB.Client.Pages.SuperAdmin;
using PB.DatabaseFramework;
using PB.EntityFramework;
using PB.Model;
using PB.Model.Models;
using PB.Server.Repository;
using PB.Shared;
using PB.Shared.Enum;
using PB.Shared.Enum.Accounts;
using PB.Shared.Enum.WhatsApp;
using PB.Shared.Models;
using PB.Shared.Models.Accounts;
using PB.Shared.Models.Accounts.AccountGroups;
using PB.Shared.Models.Accounts.Ledgers;
using PB.Shared.Models.Accounts.VoucherEntry;
using PB.Shared.Models.Accounts.VoucherTypes;
using PB.Shared.Models.Common;
using PB.Shared.Models.Court;
using PB.Shared.Models.Inventory.Invoice;
using PB.Shared.Models.Inventory.Invoices;
using PB.Shared.Tables;
using PB.Shared.Tables.Accounts.AccountGroups;
using PB.Shared.Tables.Accounts.JournalMaster;
using PB.Shared.Tables.Accounts.Ledgers;
using PB.Shared.Tables.Accounts.VoucherTypes;
using System.Data;

namespace PB.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AccountsController : BaseController
    {
        private readonly IDbContext _dbContext;
        private readonly IDbConnection _cn;
        private readonly IMapper _mapper;
        private readonly IAccountRepository _accounts;
        private readonly ICommonRepository _common;
        private readonly IInventoryRepository _inventory;

        public AccountsController(IDbContext dbContext, IDbConnection cn, IMapper mapper, IAccountRepository accounts, ICommonRepository common, IInventoryRepository inventory)
        {
            _dbContext = dbContext;
            _cn = cn;
            _mapper = mapper;
            _accounts = accounts;
            _common = common;
            _inventory = inventory;
        }

        #region Account Groups

        [HttpPost("save-account-group")]
        public async Task<IActionResult> SaveAccountGroup(AccountGroupModel accountGroupModel)
        {
            var accountGroupName = await _dbContext.GetByQueryAsync<string>(@$"
                                        Select AccountGroupName 
                                        From AccAccountGroup
                                        Where AccountGroupID<>@AccountGroupID AND ClientID={CurrentClientID} AND (IsNull(BranchID,0)=0 OR BranchID={CurrentBranchID}) AND LOWER(TRIM(AccountGroupName))=LOWER(TRIM(@AccountGroupName)) AND IsDeleted=0
            ", new { AccountGroupID = accountGroupModel.AccountGroupID, AccountGroupName = accountGroupModel.AccountGroupName });

            if (!string.IsNullOrEmpty(accountGroupName))
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ResponseTitle = "InvalidSubmission",
                    ResponseMessage = "AccountGroupNameExist"
                });
            }

            var accountGroupCode = await _dbContext.GetByQueryAsync<string>(@$"
                                        Select AccountGroupCode 
                                        From AccAccountGroup
                                        Where AccountGroupID<>@AccountGroupID AND ClientID={CurrentClientID} AND (IsNull(BranchID,0)=0 OR BranchID={CurrentBranchID}) AND LOWER(TRIM(AccountGroupCode))=LOWER(TRIM(@AccountGroupCode)) AND IsDeleted=0
            ", new { AccountGroupID = accountGroupModel.AccountGroupID, AccountGroupCode = accountGroupModel.AccountGroupCode });

            if (!string.IsNullOrEmpty(accountGroupCode))
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ResponseTitle = "InvalidSubmission",
                    ResponseMessage = "AccountGroupCodeExist"
                });
            }

            accountGroupModel.ClientID = CurrentClientID;
            accountGroupModel.BranchID = CurrentBranchID;
            int accountGroupID = await _accounts.InsertAccAccountGroup(accountGroupModel, CurrentUserID);
            return Ok(new AccountGroupAddResultModel()
            {
                AccountGroupID = accountGroupID,
                AccountGroupName = accountGroupModel.AccountGroupName,
                ResponseMessage = "AccountGroupSaved",
                ResponseTitle = "SaveSuccess"
            });
        }

        [HttpGet("get-account-group/{accountGroupID}")]
        public async Task<IActionResult> GetAccountGroup(int accountGroupID)
        {
            var res = await _accounts.GetAccAccountGroup(accountGroupID, CurrentClientID, CurrentBranchID);
            return Ok(res);
        }

        [HttpGet("delete-account-group/{accountGroupID}")]
        public async Task<IActionResult> DeleteAccountGroup(int accountGroupID)
        {
            string accountGroupName = await _dbContext.GetFieldsAsync<AccAccountGroup, string>("AccountGroupName", $"AccountGroupID={accountGroupID}", null);
            var logSummaryID = await _dbContext.InsertDeleteLogSummary(accountGroupID, "Account Group : " + accountGroupName + "(AccountGroupID : " + accountGroupID + ") deleted by User " + CurrentUserID);
            await _dbContext.DeleteAsync<AccAccountGroup>(accountGroupID, null, logSummaryID);
            return Ok(true);
        }

        [HttpPost("get-account-groups-paged-list")]
        public async Task<IActionResult> GetAccountGroups(PagedListPostModelWithFilter model)
        {
            PagedListQueryModel query = model;
            query.Select = $@"Select A.AccountGroupID,A.AccountGroupName,A.AccountGroupCode,
                                Case 
	                                When A.IsSuperParent=1 Then GT.GroupTypeName
	                                Else P.AccountGroupName
	                                End As ParentOrGroupTypeName
                                From AccAccountGroup A
                                Left Join AccAccountGroup P on P.AccountGroupID=A.ParentID and P.IsDeleted=0
                                Left Join AccAccountGroupType GT ON A.GroupTypeID=GT.GroupTypeID And GT.IsDeleted=0";
            query.WhereCondition = $"A.ClientID={CurrentClientID} AND  A.IsDeleted=0 AND (ISNULL(A.BranchID, 0) = {CurrentBranchID} OR A.BranchID IS NULL)";
            query.OrderByFieldName = model.OrderByFieldName;
            query.WhereCondition += _common.GeneratePagedListFilterWhereCondition(model, "A.AccountGroupName");
            var res = await _dbContext.GetPagedList<AccountGroupListModel>(query, null);
            return Ok(res);
        }

        [HttpGet("get-all-account-groups")]
        public async Task<IActionResult> GetAllAccountGroups()
        {
            var accountGroups = await _dbContext.GetListOfIdValuePairAsync<AccAccountGroup>("AccountGroupName", $"ClientID={CurrentClientID}", null);
            return Ok(accountGroups ?? new());
        }

        [HttpGet("get-account-group-type/{accountGroupID}")]
        public async Task<IActionResult> GetAccountGroupType(int accountGroupID)
        {
            return Ok(await _dbContext.GetFieldsAsync<AccAccountGroup, int>("GroupTypeID", $"AccountGroupID={accountGroupID}", null));
        }

        #endregion

        #region Ledgers

        [HttpPost("save-ledger")]
        public async Task<IActionResult> SaveLedger(AccLedgerModel ledgerModel)
        {

            var ledgerName = await _dbContext.GetByQueryAsync<string>(@$"
                                        Select LedgerName 
                                        From AccLedger
                                        Where LedgerID<>@LedgerID AND ClientID={CurrentClientID} AND (IsNull(BranchID,0)=0 OR BranchID={CurrentBranchID}) AND LOWER(TRIM(LedgerName))=LOWER(TRIM(@LedgerName)) AND IsDeleted=0
            ", new { LedgerID = ledgerModel.LedgerID, LedgerName = ledgerModel.LedgerName });

            if (!string.IsNullOrEmpty(ledgerName))
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ResponseTitle = "InvalidSubmission",
                    ResponseMessage = "LedgerNameExist"
                });
            }

            var ledgerCode = await _dbContext.GetByQueryAsync<string>(@$"
                                        Select LedgerCode 
                                        From AccLedger
                                        Where LedgerID<>@LedgerID AND ClientID={CurrentClientID} AND (IsNull(BranchID,0)=0 OR BranchID={CurrentBranchID}) AND LOWER(TRIM(LedgerCode))=LOWER(TRIM(@LedgerCode)) AND IsDeleted=0",
                                        new { LedgerID = ledgerModel.LedgerID, LedgerCode = ledgerModel.LedgerCode });

            if (!string.IsNullOrEmpty(ledgerCode))
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ResponseTitle = "InvalidSubmission",
                    ResponseMessage = "LedgerCodeExist"
                });
            }

            if (ledgerModel.GroupTypeID == (int)AccountGroupTypes.SundryCreditors || ledgerModel.GroupTypeID == (int)AccountGroupTypes.SundryDebtors)
            {
                var phone = await _dbContext.GetByQueryAsync<string>($@"
                                        Select Phone 
                                        From Entity
                                        Where EntityID<>@EntityID AND ClientID=@ClientID AND Phone=@Phone AND IsDeleted=0", new { EntityID = Convert.ToInt32(ledgerModel.EntityID), ClientID = CurrentClientID, Phone = ledgerModel.Phone });

                if (!string.IsNullOrEmpty(phone))
                {
                    return BadRequest(new BaseErrorResponse()
                    {
                        ResponseTitle = "InvalidSubmission",
                        ResponseMessage = "PhoneExist"
                    });
                }

                if (!string.IsNullOrEmpty(ledgerModel.EmailAddress))
                {
                    var emailAddress = await _dbContext.GetByQueryAsync<string>($@" 
                                        Select EmailAddress 
                                        From Entity
                                        Where EntityID<>@EntityID AND ClientID=@ClientID AND EmailAddress=@EmailAddress AND IsDeleted=0",
                                        new { EntityID = Convert.ToInt32(ledgerModel.EntityID), ClientID = CurrentClientID, EmailAddress = ledgerModel.EmailAddress });

                    if (!string.IsNullOrEmpty(emailAddress))
                    {
                        return BadRequest(new BaseErrorResponse()
                        {
                            ResponseTitle = "InvalidSubmission",
                            ResponseMessage = "EmailExist"
                        });
                    }
                }
            }

            _cn.Open();
            using (var tran = _cn.BeginTransaction())
            {
                try
                {
                    ledgerModel.ClientID = CurrentClientID;
                    ledgerModel.BranchID = CurrentBranchID;
                    ledgerModel.LedgerID = await _accounts.InsertAccLedger(ledgerModel, CurrentUserID, tran);

                    tran.Commit();

                    return Ok(new LedgerAddResultModel()
                    {
                        LedgerID = ledgerModel.LedgerID,
                        LedgerName = ledgerModel.LedgerName,
                        ResponseMessage = "LedgerSaved",
                        ResponseTitle = "SaveSuccess"
                    });
                }
                catch (Exception e)
                {
                    tran.Rollback();
                    return BadRequest(new BaseErrorResponse()
                    {
                        ResponseMessage = e.Message,
                        ResponseTitle = "SomethingWentWrong"
                    });
                }
            }
        }

        [HttpGet("get-ledger/{ledgerID}")]
        public async Task<IActionResult> GetLedger(int ledgerID)
        {
            return Ok(await _accounts.GetAccLedger(ledgerID, CurrentClientID, CurrentBranchID));
        }

        [HttpGet("delete-ledger/{ledgerID}")]
        public async Task<IActionResult> DeleteLedger(int ledgerID)
        {
            var journalEntryID = await _dbContext.GetFieldsAsync<AccJournalEntry, int?>("JournalEntryID", $"LedgerID={ledgerID} AND IsDeleted=0", null);
            if (journalEntryID is not null)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ResponseTitle = "SomethingWentWrong",
                    ResponseMessage = "LedgerIsInUse"
                });
            }

            await _accounts.DeleteAccLedger(ledgerID, CurrentUserID);
            return Ok(true);
        }

        [HttpPost("get-ledgers-paged-list")]
        public async Task<IActionResult> GetLedgersPagedList(PagedListPostModelWithFilter searchModel)
        {
            PagedListQueryModel searchData = searchModel;
            searchData.Select = $@"Select L.LedgerID,L.LedgerName,AT.GroupTypeName,L.LedgerCode,A.AccountGroupName,L.Alias
                                From AccLedger L
                                LEFT JOIN AccAccountGroup A on A.AccountGroupID=L.AccountGroupID
                                LEFT JOIN AccAccountGroupType AT on A.GroupTypeID=AT.GroupTypeID and AT.IsDeleted=0";
            searchData.OrderByFieldName = searchModel.OrderByFieldName;
            searchData.WhereCondition = $"L.IsDeleted=0 and L.ClientID={CurrentClientID} AND (L.BranchID={CurrentBranchID} or ISNULL(L.BranchID,0)=0)";
            searchData.WhereCondition += _common.GeneratePagedListFilterWhereCondition(searchModel, "L.LedgerName");
            var LedgersPagedList = await _dbContext.GetPagedList<LedgerListModel>(searchData, null);
            if (LedgersPagedList is not null)
            {
                foreach (var ledger in LedgersPagedList.Data)
                {
                    ledger.LedgerCode = string.IsNullOrEmpty(ledger.LedgerCode) ? "NIL" : ledger.LedgerCode;
                    ledger.Alias = string.IsNullOrEmpty(ledger.Alias) ? "NIL" : ledger.Alias;
                    ledger.AccountGroupName = string.IsNullOrEmpty(ledger.AccountGroupName) ? "NIL" : ledger.AccountGroupName;
                }
            }
            return Ok(LedgersPagedList ?? new());
        }

        [HttpGet("get-ledger-menu-list")]
        public async Task<IActionResult> GetLedgersMenuList()
        {
            var result = await _dbContext.GetListByQueryAsync<ViewPageMenuModel>($@"Select LedgerID as ID,
                                                                                    CASE 
                                                                                        WHEN LEN(LedgerName) > 17 
                                                                                        THEN CONCAT(SUBSTRING(LedgerName, 1, 17),'...') 
                                                                                        ELSE LedgerName 
                                                                                        END AS MenuName
                                                                                    From AccLedger
                                                                                    Where IsDeleted=0 and ClientID={CurrentClientID}
                                                                                    Order By LedgerID DESC
            ", null);
            return Ok(result);
        }

        #endregion

        #region Voucher Types

        [HttpPost("save-voucher-type")]
        public async Task<IActionResult> SaveVoucherType(VoucherTypeModel model)
        {
            var voucherTypeName = await _dbContext.GetByQueryAsync<string>($@"
                                                            Select VoucherTypeName
                                                            From AccVoucherType
                                                            Where VoucherTypeID<>@VoucherTypeID AND IsDeleted=0 AND ClientID=@ClientID AND LOWER(TRIM(VoucherTypeName))=LOWER(TRIM(@VoucherTypeName))
            ", new { VoucherTypeID = model.VoucherTypeID, ClientID = CurrentClientID, VoucherTypeName = model.VoucherTypeName });

            if (!string.IsNullOrEmpty(voucherTypeName))
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ResponseTitle = "SomethingWentWrong",
                    ResponseMessage = "VoucherTypeNameDuplicate"
                });
            }

            var voucherTypePrefix = await _dbContext.GetByQueryAsync<string>($@"
                                                            Select S.Prefix
                                                            From AccVoucherType V
                                                            Left Join AccVoucherTypeSetting S ON S.VoucherTypeID=V.VoucherTypeID AND S.IsDeleted=0
                                                            Where V.VoucherTypeID<>@VoucherTypeID AND V.IsDeleted=0 AND V.ClientID=@ClientID AND LOWER(TRIM(S.Prefix))=LOWER(TRIM(@Prefix)) AND S.BranchID=@BranchID
            ", new { VoucherTypeID = model.VoucherTypeID, ClientID = CurrentClientID, Prefix = model.Prefix, BranchID = CurrentBranchID });

            if (!string.IsNullOrEmpty(voucherTypePrefix))
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ResponseTitle = "SomethingWentWrong",
                    ResponseMessage = "VoucherTypePrefixDuplicate"
                });
            }

            _cn.Open();
            using (var tran = _cn.BeginTransaction())
            {
                model.ClientID = CurrentClientID;
                model.BranchID = CurrentBranchID;
                model.VoucherTypeID = await _accounts.InsertAccVoucherType(model, tran);
                tran.Commit();
                return Ok(new VoucherTypeSaveResultModel()
                {
                    VoucherTypeID = model.VoucherTypeID,
                    VoucherTypeName = model.VoucherTypeName,
                    ResponseTitle = "Success",
                    ResponseMessage = "VoucherTypeSaved"
                });
            }
        }

        [HttpGet("get-voucher-type/{voucherTypeID}")]
        public async Task<IActionResult> GetVoucherType(int voucherTypeID)
        {
            var voucherType = await _dbContext.GetByQueryAsync<VoucherTypeModel>($@"
                                                        Select * 
                                                        From AccVoucherType V
                                                        Left Join AccVoucherTypeSetting VS ON V.VoucherTypeID=VS.VoucherTypeID AND VS.IsDeleted=0
                                                        Where V.VoucherTypeID={voucherTypeID} AND V.IsDeleted=0 AND V.ClientID={CurrentClientID}
            ", null);

            return Ok(voucherType ?? new());
        }

        [HttpGet("delete-voucher-type/{voucherTypeID}")]
        public async Task<IActionResult> DeleteVoucherType(int voucherTypeID)
        {
            var journalMasterID = await _dbContext.GetByQueryAsync<int?>($@"Select VoucherTypeID
                                                    From AccJournalMaster
                                                    Where BranchID={CurrentBranchID} AND IsDeleted=0 AND VoucherTypeID={voucherTypeID}", null);

            if (journalMasterID is not null)
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ResponseTitle = "SomethingWentWrong",
                    ResponseMessage = "VoucherTypeInUse"
                });
            }

            int logSummaryID = await _dbContext.InsertDeleteLogSummary(voucherTypeID, "Voucher Type deleted by User : " + CurrentUserID);
            int SettingID = await _dbContext.GetByQueryAsync<int>($@"
                                                            Select S.ID
                                                            From AccVoucherType V
                                                            Left Join AccVoucherTypeSetting S ON S.VoucherTypeID=V.VoucherTypeID AND S.IsDeleted=0
                                                            Where V.VoucherTypeID={voucherTypeID} AND V.ClientID={CurrentClientID} AND V.IsDeleted=0", null);

            await _dbContext.DeleteAsync<AccVoucherType>(voucherTypeID, null, logSummaryID);
            await _dbContext.DeleteAsync<AccVoucherTypeSetting>(SettingID);
            return Ok(true);
        }

        [HttpPost("get-voucher-type-paged-list")]
        public async Task<IActionResult> GetVoucherTypes(PagedListPostModelWithFilter model)
        {
            PagedListQueryModel query = model;
            query.Select = $@"Select V.VoucherTypeID,V.VoucherTypeName,N.VoucherTypeNatureName,S.Prefix
                                    From AccVoucherType V
                                    Left Join AccVoucherTypeNature N ON V.VoucherTypeNatureID=N.VoucherTypeNatureID AND N.IsDeleted=0
                                    Left Join AccVoucherTypeSetting S ON S.VoucherTypeID=V.VoucherTypeID AND S.IsDeleted=0";
            query.WhereCondition = $"V.ClientID={CurrentClientID} AND V.IsDeleted=0 AND N.ShowInList=1";
            query.OrderByFieldName = model.OrderByFieldName;
            query.WhereCondition += _common.GeneratePagedListFilterWhereCondition(model, "V.VoucherTypeName");
            var res = await _dbContext.GetPagedList<VoucherTypeListModel>(query, null);
            return Ok(res ?? new());
        }

        #endregion

        #region Vouchers

        [HttpPost("insert-voucher-entry")]
        public async Task<IActionResult> InsertVoucherEntry(VoucherEntryModel model)
        {
            if ((model.JournalEntries.Sum(s => s.Debit) == 0) && (model.JournalEntries.Sum(s => s.Credit) == 0))
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ResponseTitle = "SomethingWentWrong",
                    ResponseMessage = "Cannot insert empty voucher entry, please add entry to the voucher"
                });
            }

            if (model.JournalEntries.Sum(s => s.Debit) != model.JournalEntries.Sum(s => s.Credit))
            {
                return BadRequest(new BaseErrorResponse()
                {
                    ResponseTitle = "Invalid submission",
                    ResponseMessage = "Debit and Credit should be equal, check tha datas you've entered"
                });
            }

            if (model.JournalMasterID == 0)
            {
                var journalMasterID = await _dbContext.GetByQueryAsync<int?>(@$"
                                                            Select JournalMasterID
                                                            From AccJournalMaster
                                                            where JournalMasterID<>{model.JournalMasterID} AND JournalNoPrefix=@JournalNoPrefix AND JournalNo=@JournalNo AND IsDeleted=0 AND BranchID=@BranchID
                ", new { JournalNoPrefix = model.JournalNoPrefix, JournalNo = model.JournalNo, BranchID = CurrentBranchID });

                if (journalMasterID is not null)
                {
                    return BadRequest(new BaseErrorResponse()
                    {
                        ResponseTitle = "SomethingWentWrong",
                        ResponseMessage = "JournalNumberTaken"
                    });
                }

                model.BranchID = CurrentBranchID;
                model.IsSuccess = true;
                model.IsVerified = true;
                model.VerifiedBy = CurrentUserID;
                model.IsOnlinePayment = false;
                model.VerifiedOn = DateTime.UtcNow;
            }

            _cn.Open();
            using (var tran = _cn.BeginTransaction())
            {
                try
                {
                    int logSummaryID = await _dbContext.InsertAddEditLogSummary(model.JournalMasterID, "Voucher Entry (" + model.JournalNoPrefix + '/' + model.JournalNo + ") added/edited by User :" + CurrentUserID, tran);
                    var journalMaster = _mapper.Map<AccJournalMaster>(model);
                    
                    //Jounrnal Master
                    journalMaster.JournalMasterID = await _dbContext.SaveAsync(journalMaster, tran, logSummaryID: logSummaryID);

                    //Journal Entries
                    foreach(var journalEntryModel in model.JournalEntries)
                    {
                        var journalEntry = _mapper.Map<AccJournalEntry>(journalEntryModel);
                        journalEntry.JournalMasterID = journalMaster.JournalMasterID;
                        journalEntry.JournalEntryID = await _dbContext.SaveAsync(journalEntry, tran);
                        if (journalEntryModel.BillToBillReferences is not null && journalEntryModel.BillToBillReferences.Count > 0)
                        {
                            var billToBillReferences = _mapper.Map<List<AccBillToBill>>(journalEntryModel.BillToBillReferences);
                            billToBillReferences.ForEach(billToBillRef => billToBillRef.ReferenceNo = model.JournalNoPrefix + '-' + model.JournalNo);
                            await _dbContext.SaveSubItemListAsync(billToBillReferences, "JournalEntryID", journalEntry.JournalEntryID, tran);
                        }
                    }

                    tran.Commit();
                    return Ok(new VoucherEntrySuccessModel()
                    {
                        ResponseMessage = "SaveSuccess",
                        JournalMasterID = journalMaster.JournalMasterID
                    });
                }
                catch (Exception e)
                {
                    tran.Rollback();
                    return BadRequest(new BaseErrorResponse()
                    {
                        ResponseMessage = e.Message,
                        ResponseTitle = "SomethingWentWrong"
                    });
                }
            }
        }

        [HttpGet("get-voucher-entry/{journalMasterID}")]
        public async Task<IActionResult> GetVoucherEntry(int journalMasterID)
        {
            var result = await _dbContext.GetByQueryAsync<VoucherEntryModel>(@$"
                                                            Select J.*,B.BranchName,V.VoucherTypeName,E.Name AS EntityName,U.Username AS VerifiedByName
                                                            From AccJournalMaster J
                                                            Left Join viBranch B ON B.BranchID=J.BranchID
                                                            Left Join AccVoucherType V ON V.VoucherTypeID=J.VoucherTypeID AND V.IsDeleted=0
                                                            Left Join viEntity E ON E.EntityID=J.EntityID
                                                            Left Join Users U ON U.UserID=J.VerifiedBy AND U.IsDeleted=0
                                                            Where J.IsDeleted=0 AND J.JournalMasterID={journalMasterID}
            ", null);

            result.JournalEntries = await _dbContext.GetListByQueryAsync<AccJournalEntryModel>(@$"
                                                            Select E.*,L.LedgerName,
                                                            CASE
	                                                            WHEN E.Debit>0 THEN {(int)DebitOrCredit.Debit}
	                                                            ELSE {(int)DebitOrCredit.Credit}
	                                                            END AS DrOrCr
                                                            From AccJournalEntry E
                                                            Left Join AccLedger L ON L.LedgerID=E.LedgerID AND L.IsDeleted=0
                                                            Where E.IsDeleted=0 AND E.JournalMasterID={journalMasterID}
            ", null);

            return Ok(result ?? new());
        }

        [HttpGet("verify-voucher-entry/{journalMasterID}")]
        public async Task<IActionResult> VerifyVoucherEntry(int journalMasterID)
        {
            await _dbContext.ExecuteAsync($@"
                                            Update AccJournalMaster
                                            SET 
                                            IsVerified=@IsVerified,VerifiedBy=@VerifiedBy,VerifiedOn=@VerifiedOn
                                            Where JournalMasterID=@JournalMasterID
            ", new { VerifiedBy = CurrentUserID, VerifiedOn = DateTime.UtcNow, IsVerified = true, JournalMasterID = journalMasterID });

            VoucheVerificationResultModel returnObject = new()
            {
                VerifiedBy = CurrentUserID,
                VerifiedOn = DateTime.UtcNow,
                VerifiedByName = await _dbContext.GetFieldsAsync<User, string>("Username", $"UserID={CurrentUserID} AND IsDeleted=0", null)
            };
            return Ok(returnObject);

        }

        [HttpPost("get-vouchers-paged-list")]
        public async Task<IActionResult> GetVouchersPagedList(PagedListPostModelWithFilter searchModel)
        {
            PagedListQueryModel searchData = searchModel;
            searchData.Select = $@"Select J.JournalMasterID,J.Date,Concat(J.JournalNoPrefix,'/',J.JournalNo) AS PrefixNJournalNumber,E.Name AS EntityName,J.IsSuccess,J.IsVerified,J.VerifiedOn,U.UserName AS VerifiedByName
                                    From AccJournalMaster J
                                    LEFT JOIN viEntity E ON E.EntityID=J.EntityID
                                    LEFT JOIN Users U ON U.UserID=J.VerifiedBy";
            searchData.OrderByFieldName = searchModel.OrderByFieldName;
            searchData.WhereCondition = $"J.IsDeleted=0";
            searchData.WhereCondition += _common.GeneratePagedListFilterWhereCondition(searchModel, "E.Name");

            if (CurrentUserTypeID == (int)UserTypes.Staff)
            {
                searchData.WhereCondition += $" AND J.BranchID={CurrentBranchID}";
            }
            else
            {
                var branFilters = searchModel.FilterByIdOptions.Count > 0 ?
                    searchModel.FilterByIdOptions.Where(option => option.FieldName == "J.BranchID").First() : null;
                if (branFilters is not null && branFilters.SelectedEnumValues is not null)
                {
                    searchData.WhereCondition += branFilters.SelectedEnumValues.Count == 0 ?
                        $" AND J.BranchID={CurrentBranchID}" :
                        $" AND J.BranchID IN (" + string.Join(',', branFilters.SelectedEnumValues) + ")";

                }
                else
                {
                    searchData.WhereCondition += $" AND J.BranchID={CurrentBranchID}";
                }
            }

            var res = await _dbContext.GetPagedList<VoucherListModel>(searchData, null);
            return Ok(res ?? new());
        }

        [HttpGet("get-voucher-number/{voucherTypeID}")]
        public async Task<IActionResult> GetVoucherEntriesInitialData(int voucherTypeID)
        {
            var result = await _accounts.GetVoucherNumber(voucherTypeID, CurrentBranchID);
            VoucherNumberDetialsModel voucherNumberDetialsModel = new()
            {
                JournalNo = result.JournalNo,
                JournalNoPrefix = result.JournalNoPrefix,
                VoucherTypeNatureID = await _dbContext.GetFieldsAsync<AccVoucherType, int>("VoucherTypeNatureID", $"VoucherTypeID={voucherTypeID}", null)
            };
            return Ok(voucherNumberDetialsModel);
        }

        [HttpGet("get-voucher-entry-menu-list")]
        public async Task<IActionResult> GetVoucherEntryMenuList()
        {
            var result = await _dbContext.GetListByQueryAsync<VoucherEntryMenuModel>(@$"
                                                            Select J.JournalMasterID as ID,E.Name As MenuName,J.JournalNoPrefix,J.JournalNo
                                                            From AccJournalMaster J
                                                            Join viEntity E ON E.EntityID=J.EntityID
                                                            Where J.IsDeleted=0 AND J.BranchID={CurrentBranchID}
                                                            Order By 1 Desc
            ", null);

            foreach (var menu in result)
            {
                if (!string.IsNullOrEmpty(menu.MenuName))
                {
                    if (menu.MenuName.Length <= 17)
                    {
                        menu.MenuName += " (" + menu.JournalNoPrefix + '/' + menu.JournalNo + ')';
                    }
                    else
                    {
                        menu.MenuName = menu.MenuName.Substring(0, 17) + ".." + " (" + menu.JournalNoPrefix + '/' + menu.JournalNo + ')';
                    }
                }
            }

            var menuList = _mapper.Map<List<ViewPageMenuModel>>(result);
            return Ok(menuList ?? new());
        }

        [HttpGet("get-bill-to-bill-items/{ledgerID}/{voucherTypeID}")]
        public async Task<IActionResult> GetBillToBillItems(int ledgerID, int voucherTypeID)
        {
            List<BillToBillAgainstReferenceModel> againstRefItems = new();
            int voucherTypeNatureID = await _dbContext.GetFieldsAsync<AccVoucherType, int>("VoucherTypeNatureID", $"VoucherTypeID={voucherTypeID}", null);
            if(voucherTypeNatureID == (int)VoucherTypeNatures.Receipt)
            {
                againstRefItems = await _dbContext.GetListByQueryAsync<BillToBillAgainstReferenceModel>($@"Select vB.BillID,B.ReferenceNo,B.Date,vB.Debit as BillAmount,B.Days
                                                                        From viBillToBillBalance vB
                                                                        Left Join AccBillToBill B on B.BillID=vB.BillID and B.IsDeleted=0
                                                                        Where B.LedgerID={ledgerID} and vB.Debit>0", null);
            }
            if (voucherTypeNatureID == (int)VoucherTypeNatures.Payment)
            {
                againstRefItems = await _dbContext.GetListByQueryAsync<BillToBillAgainstReferenceModel>($@"Select vB.BillID,B.ReferenceNo,B.Date,vB.Credit as BillAmount,B.Days
                                                                        From viBillToBillBalance vB
                                                                        Left Join AccBillToBill B on B.BillID=vB.BillID and B.IsDeleted=0
                                                                        Where B.LedgerID={ledgerID} and vB.Credit>0", null);
            }
            return Ok(againstRefItems);
        }

        [HttpGet("get-ledger-group-type/{ledgerID}")]
        public async Task<IActionResult> LedgerIsSupplierOrCustomer(int ledgerID)
        {
            int groupTypeID = await _dbContext.GetByQueryAsync<int>(@$"Select G.GroupTypeID
                                                            From AccLedger L
                                                            Join AccAccountGroup G ON G.AccountGroupID=L.AccountGroupID
                                                            Where L.LedgerID={ledgerID}", null);
            return Ok(groupTypeID);
        }

        #endregion

        #region Tax category

        [HttpPost("generate-tax-categories")]
        public async Task<IActionResult> GenerateTaxCategories(GenerateDefaultTaxCategoryModel model)
        {
            _cn.Open();
            using (var tran = _cn.BeginTransaction())
            {
                try
                {
                    await _accounts.InsertDefaultTaxCategories(model.ClientID.Value, (Countries)model.CountryID, tran);
                    tran.Commit();

                    return Ok(new Success());
                }
                catch (Exception e)
                {
                    tran.Rollback();
                    return BadRequest(new BaseErrorResponse()
                    {
                        ResponseMessage = e.Message,
                    });
                }
            }
        }

        [HttpGet("get-tax-categories")]
        public async Task<IActionResult> GetTaxCategories()
        {
            return Ok(await _dbContext.GetListByQueryAsync<IdnValuePair>($@"Select TaxCategoryID as ID,TaxCategoryName as Value
                From TaxCategory
                Where ClientID={CurrentClientID} and IsDeleted=0", null));
        }


        #endregion

        #region Accounts Report

        [HttpPost("get-general-ledger-report")]
        public async Task<IActionResult> GetGeneralLedgerReport(GeneralLedgerReportSearchModel model)
        {
            var res = await _dbContext.GetListByQueryAsync<GeneralLedgerReportResultModel>($@"
                Select LedgerID,LedgerName,Date,JournalNo,Particulars,Debit,Credit,
                Case when Balance<0 then CONVERT(varchar(20),cast(Balance*-1 as decimal(18,2)))+' CR'
                else CONVERT(varchar(20),cast(Balance as decimal(18,2)))+' DR' end as Balance
                From(

                SELECT LedgerID,LedgerName,Date,JournalNo,Particulars,Debit,Credit,
                SUM(Debit - Credit) OVER(PARTITION BY LedgerID ORDER BY Date,JournalMasterID,JournalEntryID ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) 
                AS Balance
                From(

	                Select 0 as JournalMasterID,0 as JournalEntryID,L.LedgerID,Case When E.EntityID is null then LedgerName else E.Name end as LedgerName,@FromDate Date,'' as JournalNo,'Opening Balance' as Particulars,
	                case when Sum(J.Debit)>Sum(J.Credit) then Sum(J.Debit)-Sum(J.Credit) else 0 end Debit ,
	                case when Sum(J.Credit)>Sum(J.Debit) then Sum(J.Credit)-Sum(J.Debit) else 0 end Credit
	                From AccJournalMaster JM
	                JOIN AccJournalEntry J on JM.JournalMasterID=J.JournalMasterID
	                JOIN AccLedger L on J.LedgerID=L.LedgerID
	                LEFT JOIN viEntity E on E.EntityID=L.EntityID
	                where JM.Date <@FromDate AND JM.IsSuccess = 1 and L.IsDeleted=0 and JM.IsDeleted=0 and JM.BranchID={CurrentBranchID} and (L.LedgerID=@LedgerID or @LedgerID=0) 
	                Group by L.LedgerID,LedgerName,E.EntityID,E.Name

	                UNION

	                Select JM.JournalMasterID,JournalEntryID,L.LedgerID,Case When E.EntityID is null then LedgerName else E.Name end as LedgerName,Date,JournalNoPrefix+CONVERT(varchar, JournalNo) as JournalNo,JM.Particular,Debit,Credit
	                AS Balance
	                from AccJournalEntry J
	                JOIN AccJournalMaster JM on JM.JournalMasterID=J.JournalMasterID
	                JOIN AccLedger L on L.LedgerID=J.LedgerID and L.IsDeleted=0
	                LEFT JOIN viEntity E on E.EntityID=L.EntityID
	                Where JM.Date between @FromDate and @ToDate and JM.IsSuccess=1 AND L.IsDeleted=0 and JM.IsDeleted=0  and JM.BranchID={CurrentBranchID} and (L.LedgerID=@LedgerID or @LedgerID=0)
                )as A
                )As B", model);
            return Ok(res);
        }

        #endregion
    }
}
