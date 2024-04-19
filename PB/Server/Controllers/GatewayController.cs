using ccavutil;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using PB.Client.Pages.SuperAdmin;
using PB.DatabaseFramework;
using PB.EntityFramework;
using PB.Model;
using PB.Server.Repository;
using PB.Shared;
using PB.Shared.Enum;
using PB.Shared.Models;
using PB.Shared.Models.SuperAdmin.PaymentGateway;
using PB.Shared.Tables;
using PB.Shared.Tables.Accounts.JournalMaster;
using System.Collections.Specialized;
using System.Data;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;

namespace PB.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GatewayController : BaseController
    {
        private readonly IDbContext _dbContext;
        private readonly IPaymentGatewayRepository _payment;
        private readonly IConfiguration _config;

        public GatewayController(IDbContext dbContext, IPaymentGatewayRepository payment, IConfiguration config)
        {
            _dbContext=dbContext;
            _payment=payment;
            _config=config;
        }

        #region SuperAdmin

        [HttpGet("get-payment-gateway/{id}")]
        public async Task<IActionResult> GetPaymentGateway(int id)
        {
            var res = await _dbContext.GetAsync<PaymentGateway>(id);
            return Ok(res);
        }

        [HttpPost("get-all-gateways")]
        public async Task<IActionResult> GetAllPaymentGateway(PagedListPostModelWithFilter model)
        {
            PagedListQueryModel query = model;
            query.Select=@"Select *
                            From PaymentGateway";
            query.WhereCondition="IsDeleted=0";
            query.SearchLikeColumnNames=new() {"GatewayName"};
            var res=await _dbContext.GetPagedList<PaymentGateway>(query, null);
            return Ok(res);
        }

        [HttpPost("save-payment-gateway")]
        public async Task<IActionResult> SavePaymentGateway(PaymentGateway model)
        {
            await _dbContext.SaveAsync(model);
            return Ok(model);
        }

        [HttpGet("delete-payment-gateway")]
        public async Task<IActionResult> DeletePaymentGateway(int id)
        {
            var res = await _dbContext.GetAsync<PaymentGatewayClientCredential>($"GatewayID={id} and ISNULL(IsDeleted,0)=0", null);
            if (res!=null)
                return BadRequest(new Error() { ResponseMessage="Sorry the payment gateway is currently being used",ResponseTitle="Cannot Delete"});
            await _dbContext.DeleteAsync<PaymentGateway>(id);
            return Ok(new Success());
        }

        #endregion

        #region Client Payment Gateway Settings

        [HttpGet("get-all-gateway-list")]
        public async Task<IActionResult> GetAllGatewayIdnValuePair()
        {
            var res = await _dbContext.GetListAsync<PaymentGateway>(orderby:"GatewayName asc");
            return Ok(res);
        }

        [HttpGet("get-client-gateway-credentials")]
        public async Task<IActionResult> GetClientGatewaySettings()
        {
            var res = await _dbContext.GetAsync<PaymentGatewayClientCredential>($"ClientID={CurrentClientID} and IsDeleted=0", null);
            return Ok(res??new());
        }

        [HttpPost("save-gateway-credentials")]
        public async Task<IActionResult> SaveGatewayCredentials(PaymentGatewayClientCredential gatewayCredential)
        {
            gatewayCredential.ClientID=CurrentClientID;
            await _dbContext.SaveAsync(gatewayCredential);
            return Ok(new Success());
        }

        #endregion

        #region Payment Gateway 

        [HttpGet("payment-initiation/{journalMasterId}")]
        public async Task<IActionResult> PaymentInitiation(int journalMasterId)
        {
            // Discuss about Language ,Purpose of payment,Currency in PaymentEncryption
            var res = await _dbContext.GetByQueryAsync<PaymentInitiationModel>($@"Select MerchantID,AccessCode,WorkingKey,Salt,GatewayType,Case when IsLive=1 then PaymentInitiationLiveLink else PaymentInitiationTestLink end as PaymentInitiationLink,Case when IsLive=1 then PaymentVerificationLiveLink else PaymentVerificationTestLink end as PaymentVerificationLink,Cast(JournalNo as varchar) as JournalNo,FirstName,LastName,EmailAddress,Phone
                                                                                                    From AccJournalMaster A
                                                                                                    Join Branch B on B.BranchID=A.BranchID and B.IsDeleted=0
                                                                                                    Join PaymentGatewayClientCredential PC on PC.ClientID=B.ClientID and PC.IsDeleted=0
                                                                                                    Join PaymentGateway PG on PG.GatewayID=PC.GatewayID and PG.IsDeleted=0
                                                                                                    Join viEntity V on V.EntityID=A.EntityID
                                                                                                    where A.JournalMasterID={journalMasterId}", null);
            res.Amount=await _payment.GetAmountByJournalMasterID(journalMasterId);
            res.OrderID=string.Concat(_config["PaymentOrderIDSalt"], journalMasterId.ToString());
            res.ProductInfo=$"Purchase No:{res.JournalNo}";
            res.ResponseAPIURL=string.Concat(_config["ServerURL"], "/api/Gateway/payment-completion");
            res.Hash=await _payment.PaymentEncryption(null, res);
            return Ok(res);
        }

        [HttpPost("payment-completion")]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> PaymentCompletion([FromForm] IFormCollection formvalue)
        {
            int JournalMasterID=0;
            string SuccessfulStatus = "";
            string orderStatus = "";
            string amount = "";
            string paymentGatewayID = "";
            string bankRefNo = "";
            string paymentMode = "";
            string failureMessage = "";
            string serverURL = string.Concat(_config["ServerURL"]);
            string formValueOrderNo = "";

            if (formvalue.ContainsKey("mihpayid"))
            {
                SuccessfulStatus="success";

                #region Fetching values from IFormCollection

                paymentGatewayID = formvalue["mihpayid"];
                string hash = formvalue["hash"];
                orderStatus = formvalue["status"];
                paymentMode = formvalue["mode"];
                string email = formvalue["email"];
                string productInfo = formvalue["productinfo"];
                string firstName = formvalue["firstname"];
                amount = formvalue["amount"];
                bankRefNo = formvalue["bank_ref_num"];
                formValueOrderNo = formvalue["txnid"];
                string key = formvalue["key"];
                failureMessage = formvalue["error_Message"];

                #endregion

                JournalMasterID =Convert.ToInt32(formValueOrderNo.Replace(_config["PaymentOrderIDSalt"],""));//Get JournalMasterID From TXNID
                string salt = await _dbContext.GetByQueryAsync<string>($@"Select Salt
	                                                                        From AccJournalMaster A
	                                                                        Join Branch B on B.BranchID=A.BranchID and B.IsDeleted=0
	                                                                        Join PaymentGatewayClientCredential PC on PC.ClientID=B.ClientID and PC.IsDeleted=0
	                                                                        where A.JournalMasterID={JournalMasterID}", null);
                string hashstring = $"{salt}|{orderStatus}|||||||||||{email}|{firstName}|{productInfo}|{amount}|{formValueOrderNo}|{key}";//hash string creation
                string hashResult = await _payment.PaymentEncryption(JournalMasterID,new PaymentInitiationModel {GatewayType=(int)PaymentGatewayTypes.PayU},hashstring);
                if (hashResult != hash) //hashValue from Payu checking with hashvalue created above(from our side) to avoid tampering
                {
                    return Redirect(serverURL+$"payment-redirect/{JournalMasterID}");
                }
            }
            else if(formvalue.ContainsKey("encResp"))
            {
                SuccessfulStatus="Success";
                #region Fetching values from IFormCollection

                formValueOrderNo = formvalue["orderNo"];
                string encResp = formvalue["encResp"];
                string crossSellUrl = formvalue["crossSellUrl"];

                #endregion
                JournalMasterID =Convert.ToInt32(formValueOrderNo.Replace(_config["PaymentOrderIDSalt"], ""));//Get JournalMasterID From TXNID
                #region Decryption
                string decryptedResponse = await _payment.PaymentEncryption(JournalMasterID,new PaymentInitiationModel {OrderID=formValueOrderNo, GatewayType=(int)PaymentGatewayTypes.CCAvenue }, encResp,true);
                #endregion

                #region Fetching values from decryptedResponse

                // Split the transaction string by the '&' delimiter to get the key-value pairs
                string[] transactionPairs = decryptedResponse.Split('&');

                // Declare a dictionary to store the key-value pairs
                Dictionary<string, string> transactionDict = new();

                // Iterate over the transaction pairs and extract the key-value pairs
                foreach (string pair in transactionPairs)
                {
                    string[] keyValue = pair.Split('=');
                    string key = keyValue[0];
                    string value = keyValue[1];
                    transactionDict.Add(key, value);
                }

                // Access the values from the dictionary
                string decryptedOrderid = transactionDict["order_id"];
                paymentGatewayID = transactionDict["tracking_id"];
                bankRefNo = transactionDict["bank_ref_no"];
                orderStatus = transactionDict["order_status"];//transaction status
                failureMessage = transactionDict["failure_message"];
                paymentMode = transactionDict["payment_mode"];
                string card_name = transactionDict["card_name"];
                string status_code = transactionDict["status_code"];
                string status_message = transactionDict["status_message"];
                string currency = transactionDict["currency"];
                amount = transactionDict["amount"];
                string billing_name = transactionDict["billing_name"];
                string billing_tel = transactionDict["billing_tel"];
                string billing_email = transactionDict["billing_email"];
                string trans_date = transactionDict["trans_date"];
                string bin_country = transactionDict["bin_country"];
                #endregion

                if (formValueOrderNo != decryptedOrderid)
                {
                    return Redirect(serverURL+$"payment-redirect/{JournalMasterID}");
                }

            }
            int IsSuccess = 0;
            if (orderStatus == SuccessfulStatus)
            {
                var amountFromDatabase = await _payment.GetAmountByJournalMasterID(JournalMasterID,null);
                if (amountFromDatabase == amount) //Comparison between amount from payu api response and amount from database to avoid tampering
                {
                    #region Duplicate entry validation
                    var tracking = await _dbContext.GetAsync<AccJournalMaster>($"PaymentGatewayID='{paymentGatewayID}'", null);//Check tracking id uniqueness
                    var Reference = await _dbContext.GetAsync<AccJournalMaster>($"ReferenceNo=@ReferenceNo", new { ReferenceNo = bankRefNo });//Check bank reference number
                    if (tracking!=null || Reference!=null)
                    {
                        await _dbContext.ExecuteAsync($"Update AccJournalMaster Set IsSuccess={IsSuccess},PaymentMode=@PaymentMode,Remarks=@Remarks,PaymentGatewayID=@PaymentGatewayID,ReferenceNo=@ReferenceNo where JournalMasterID={JournalMasterID}", new { PaymentMode = paymentMode , Remarks = failureMessage , PaymentGatewayID = paymentGatewayID , ReferenceNo = bankRefNo });
                        return Redirect(serverURL+$"payment-redirect/{JournalMasterID}");
                    }
                    var AlreadyUsedJournalMaster = await _dbContext.GetAsync<AccJournalMaster>($"JournalMasterID=@JournalMasterID and (PaymentGatewayID=@PaymentGatewayID or ReferenceNo=@ReferenceNo)", new { JournalMasterID = JournalMasterID, PaymentGatewayID = paymentGatewayID , ReferenceNo = bankRefNo });//CHECK HERE FOR DUPLICATE ENTRY OF JOURNAL MASTER (CHECK WHETHER ALREADY USED) 
                    if (AlreadyUsedJournalMaster!=null)
                    {
                        return Redirect(serverURL+$"payment-redirect/{JournalMasterID}");
                    }
                    #endregion
                    var res = new TransactionDetailsViewModel();
                    var clientCredentials = await _payment.GetClientCredentialsByJournalMasterID(JournalMasterID, null);
                    clientCredentials.OrderID=formValueOrderNo;
                    if (clientCredentials.GatewayType==(int)PaymentGatewayTypes.PayU)
                    {
                        res = await _payment.VerifyPayUPayment(clientCredentials, null);
                        if (res.status == "success")
                            IsSuccess = 1;
                    }
                    else if (clientCredentials.GatewayType==(int)PaymentGatewayTypes.CCAvenue)
                    {
                        res = await _payment.CCAvenuePaymentStatus(clientCredentials, null);
                        if (res.status == "Shipped")
                            IsSuccess = 1;
                    }
                    paymentMode = res.mode;
                    failureMessage = res.error_Message;
                    paymentGatewayID = res.mihpayid;
                    bankRefNo = res.bank_ref_num;
                }
                else
                {
                    failureMessage="Database amount and amount from response didn't match";
                }
            }
            await _dbContext.ExecuteAsync($"Update AccJournalMaster Set IsSuccess={IsSuccess},PaymentMode=@PaymentMode,Remarks=@Remarks,PaymentGatewayID=@PaymentGatewayID,ReferenceNo=@ReferenceNo where JournalMasterID={JournalMasterID}", new { PaymentMode = paymentMode, Remarks = failureMessage , PaymentGatewayID = paymentGatewayID , ReferenceNo = bankRefNo });
            return Redirect(serverURL + $"payment-redirect/{JournalMasterID}");
        }
        
        #region Verify Payment 

        [HttpGet("check-payment-status/{journalMasterID}")]
        public async Task<IActionResult> GetPayUPaymentStatus(int journalMasterID)
        {
            var clientCredentials = await _payment.GetClientCredentialsByJournalMasterID(journalMasterID, null);
            clientCredentials.OrderID=string.Concat(_config["PaymentOrderIDSalt"], journalMasterID.ToString());
            var res = new TransactionDetailsViewModel();
            string validSuccessMessage = "";
            switch (clientCredentials.GatewayType)
            {
                case (int)PaymentGatewayTypes.PayU:
                    res = await _payment.VerifyPayUPayment(clientCredentials, null);
                    validSuccessMessage ="success";
                    break;
                case (int)PaymentGatewayTypes.CCAvenue:
                    res = await _payment.CCAvenuePaymentStatus(clientCredentials,null);
                    validSuccessMessage="Shipped";
                    break;
            }
            if (!string.IsNullOrEmpty(res.JSONConversionErrorTitle))
                return BadRequest(new Error(res.JSONConversionErrorMessage, res.JSONConversionErrorTitle));
            else
            {
                bool IsSuccess = false;
                if (res.status == validSuccessMessage)
                    IsSuccess = true;
                await _dbContext.ExecuteAsync($"Update AccJournalMaster Set IsSuccess={Convert.ToInt32(IsSuccess)},PaymentMode=@PaymentMode,Remarks=@Remarks,PaymentGatewayID=@PaymentGatewayID,ReferenceNo=@ReferenceNo where JournalMasterID={journalMasterID}", new { PaymentMode  = res.mode, Remarks  = res.error_Message , PaymentGatewayID  = res.mihpayid , ReferenceNo  = res.bank_ref_num });
                return Ok(IsSuccess);
            }

        }

        #endregion

        #endregion

    }
}
