using ccavutil;
using Newtonsoft.Json;
using NPOI.OpenXmlFormats.Wordprocessing;
using PB.DatabaseFramework;
using PB.EntityFramework;
using PB.Shared.Enum;
using PB.Shared.Models.SuperAdmin.PaymentGateway;
using System.Collections.Specialized;
using System.Data;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace PB.Server.Repository
{
    public interface IPaymentGatewayRepository
    {
        Task<string> PaymentEncryption(int? journalMasterId, PaymentInitiationModel model, string hashGenerationString = null, bool isDecryption = false, IDbTransaction tran=null);
        Task<string> GetAmountByJournalMasterID(int JournalMasterID,IDbTransaction tran=null);
        Task<TransactionDetailsViewModel> VerifyPayUPayment(PaymentGatewayClientCredentialsModel model, IDbTransaction? tran = null);
        Task<TransactionDetailsViewModel> CCAvenuePaymentStatus(PaymentGatewayClientCredentialsModel model, IDbTransaction? tran = null);
        Task<PaymentGatewayClientCredentialsModel> GetClientCredentialsByJournalMasterID(int journalMasterID,IDbTransaction tran=null);
    }

    public class PaymentGatewayRepository: IPaymentGatewayRepository
    {
        private readonly IDbContext _dbContext;
        private readonly IConfiguration _config;

        public PaymentGatewayRepository(IDbContext dbContext, IConfiguration config)
        {
            this._dbContext =dbContext;
            this._config =config;
        }

        public async Task<string> PaymentEncryption(int? journalMasterId,PaymentInitiationModel model,string hashGenerationString=null,bool isDecryption=false,IDbTransaction tran = null)
        {
            switch (model.GatewayType)
            {
                case (int)PaymentGatewayTypes.PayU:
                    if(string.IsNullOrEmpty(hashGenerationString))
                        hashGenerationString = $"{model.WorkingKey}|{model.OrderID}|{model.Amount}|{model.ProductInfo}|{model.FirstName}|{model.EmailAddress}|||||||||||{model.Salt}";
                    StringBuilder sb = new();
                    using (SHA512 sha512 = SHA512.Create())
                    {
                        byte[] hashBytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(hashGenerationString));
                        foreach (byte b in hashBytes)
                        {
                            sb.Append($"{b:X2}");
                        }
                    }
                    model.Hash= sb.ToString().ToLower();
                    break;
                case (int)PaymentGatewayTypes.CCAvenue:
                    if (string.IsNullOrEmpty(hashGenerationString))
                        hashGenerationString = "merchant_id=" + HttpUtility.UrlEncode(model.MerchantID) +
                                "&order_id=" + HttpUtility.UrlEncode(model.OrderID) +
                                "&currency=" + HttpUtility.UrlEncode("INR") +
                                "&amount=" + HttpUtility.UrlEncode(model.Amount) +
                                "&redirect_url=" + HttpUtility.UrlEncode(model.ResponseAPIURL) +
                                "&cancel_url=" + HttpUtility.UrlEncode(model.ResponseAPIURL) +
                                "&language=" + HttpUtility.UrlEncode("EN");
                    else
                    {

                        model.WorkingKey = await _dbContext.GetByQueryAsync<string>($@"Select WorkingKey
	                                                                        From AccJournalMaster A
	                                                                        Join Branch B on B.BranchID=A.BranchID and B.IsDeleted=0
	                                                                        Join PaymentGatewayClientCredential PC on PC.ClientID=B.ClientID and PC.IsDeleted=0
	                                                                        where A.JournalMasterID={journalMasterId}", null, transaction:tran);
                    }

                    AesCryptUtil aesUtil = new(model.WorkingKey);
                    if(isDecryption)
                        model.Hash = aesUtil.decrypt(hashGenerationString);
                    else
                        model.Hash = aesUtil.encrypt(hashGenerationString);
                    break;
            }
            return model.Hash;
        }

        public async Task<string> GetAmountByJournalMasterID(int JournalMasterID, IDbTransaction tran = null)
        {
            return(await _dbContext.GetByQueryAsync<string>($@"Select Cast(IsNull(Sum(Debit),0) as varchar) 
                                                                         From AccJournalEntry
                                                                         Where JournalMasterID={JournalMasterID}", null));
        }

        public async Task<PaymentGatewayClientCredentialsModel> GetClientCredentialsByJournalMasterID(int journalMasterID, IDbTransaction tran = null)
        {
            return(await _dbContext.GetByQueryAsync<PaymentGatewayClientCredentialsModel>($@"Select JournalMasterID,MerchantID,AccessCode,WorkingKey,Salt,GatewayType,Case when IsLive=1 then PaymentInitiationLiveLink else PaymentInitiationTestLink end as PaymentInitiationLink,Case when IsLive=1 then PaymentVerificationLiveLink else PaymentVerificationTestLink end as PaymentVerificationLink
                                                                                                    From AccJournalMaster A
                                                                                                    Join Branch B on B.BranchID=A.BranchID and B.IsDeleted=0
                                                                                                    Join PaymentGatewayClientCredential PC on PC.ClientID=B.ClientID and PC.IsDeleted=0
                                                                                                    Join PaymentGateway P on P.GatewayID=PC.GatewayID and P.IsDeleted=0
                                                                                                    where A.JournalMasterID={journalMasterID}", null));
        }

        #region PayU Verify Payment

        public async Task<TransactionDetailsViewModel> VerifyPayUPayment(PaymentGatewayClientCredentialsModel model, IDbTransaction? tran = null)
        {
            TransactionDetailsViewModel paymentResult = new();
            string hashstring = $"{model.WorkingKey}|verify_payment|{model.OrderID}|{model.Salt}";
            string HashValue=await PaymentEncryption(null,new PaymentInitiationModel { GatewayType=(int)PaymentGatewayTypes.PayU}, hashstring);
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), model.PaymentVerificationLink))
                {
                    request.Headers.TryAddWithoutValidation("accept", "application/json");

                    request.Content = new StringContent($"key={model.WorkingKey}&command=verify_payment&var1={model.OrderID}&hash={HashValue}");
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

                    using (var response = await httpClient.SendAsync(request))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();

                        if (!string.IsNullOrWhiteSpace(apiResponse))
                        {
                            apiResponse = apiResponse.Trim();
                        }
                        if ((apiResponse.StartsWith("{") && apiResponse.EndsWith("}")) || //For object
                            (apiResponse.StartsWith("[") && apiResponse.EndsWith("]"))) //For array
                        {
                            try
                            {
                                var JSONObj = JsonConvert.DeserializeObject<dynamic>(apiResponse);
                                var transactionDetails = JSONObj.transaction_details;
                                foreach (Newtonsoft.Json.Linq.JProperty item in transactionDetails)
                                {

                                    if (item.Name == model.OrderID)
                                    {
                                        paymentResult = JsonConvert.DeserializeObject<TransactionDetailsViewModel>(item.Value.ToString());
                                    }
                                }
                            }
                            catch (JsonReaderException jex)
                            {
                                //Exception in parsing json
                                Console.WriteLine(jex.Message);
                                paymentResult.JSONConversionErrorTitle = "Sorry, Try again later";
                                paymentResult.JSONConversionErrorMessage = "Couldn't fetch details from payment gateway provider";
                            }
                            catch (Exception ex) //some other exception
                            {
                                Console.WriteLine(ex.ToString());
                                paymentResult.JSONConversionErrorTitle = "Sorry, Try again later";
                                paymentResult.JSONConversionErrorMessage = "Couldn't not complete the request";
                            }
                        }
                        else
                        {
                            paymentResult.JSONConversionErrorTitle = "Sorry, Try again later";
                            paymentResult.JSONConversionErrorMessage = "Couldn't fetch details from payment gateway provider";
                        }
                    }
                }
            }
            return paymentResult;
        }

        #endregion

        #region ccAvenue PaymentStatus

        public async Task<TransactionDetailsViewModel> CCAvenuePaymentStatus(PaymentGatewayClientCredentialsModel model, IDbTransaction? tran = null)
        {
            TransactionDetailsViewModel paymentResult = new();
            string orderStatusQueryJson = JsonConvert.SerializeObject(new Dictionary<string, string>
            {
                { "order_no", model.OrderID }
            });
            string encJson = await PaymentEncryption(null,new PaymentInitiationModel { GatewayType=(int)PaymentGatewayTypes.CCAvenue }, orderStatusQueryJson);

            // make query for the status of the order to ccAvenues change the command param as per your need
            string authQueryUrlParam = "enc_request=" + encJson + "&access_code=" + model.AccessCode + "&command=orderStatusTracker&request_type=JSON&response_type=JSON&version=1.1";

            // Url Connection
            string message = await postPaymentRequestToGateway(model.PaymentVerificationLink, authQueryUrlParam);
            NameValueCollection param = getResponseMap(message);
            string status = "";
            string encResJson = "";
            if (param != null && param.Count == 2)
            {
                for (int i = 0; i < param.Count; i++)
                {
                    if ("status".Equals(param.Keys[i]))
                    {
                        status = param[i];
                    }
                    if ("enc_response".Equals(param.Keys[i]))
                    {
                        encResJson = param[i];
                        //Response.Write(encResXML);
                    }
                }
                if (!"".Equals(status) && status.Equals("0"))
                {
                    string ResJson = await PaymentEncryption(null,new PaymentInitiationModel {GatewayType=(int)PaymentGatewayTypes.CCAvenue }, encResJson,true);
                    var responseObj = JsonConvert.DeserializeObject<dynamic>(ResJson);
                    paymentResult.mihpayid = responseObj.reference_no.Value;
                    paymentResult.bank_ref_num = responseObj.order_bank_ref_no;
                    paymentResult.mode=responseObj.order_option_type;
                    paymentResult.status=responseObj.order_status;
                    if (paymentResult.status!="Shipped")
                    {
                        paymentResult.status=responseObj.error_desc;
                    }
                }
                else if (!"".Equals(status) && status.Equals("1"))
                {
                    Console.WriteLine("failure response from ccAvenues: " + encResJson);
                }

            }
            return paymentResult;
        }

        private async Task<string> postPaymentRequestToGateway(String queryUrl, String urlParam)
        {

            String message = "";
            try
            {
                var httpClient = new HttpClient();
                var content = new StringContent(urlParam, Encoding.UTF8, "application/x-www-form-urlencoded");
                var response = await httpClient.PostAsync(queryUrl, content);
                message = await response.Content.ReadAsStringAsync();
            }
            catch (Exception exception)
            {
                Console.Write("Exception occured while connection." + exception);
            }
            return message;

        }

        private NameValueCollection getResponseMap(string message)
        {
            NameValueCollection Params = new NameValueCollection();
            if (message != null || !"".Equals(message))
            {
                string[] segments = message.Split('&');
                foreach (string seg in segments)
                {
                    string[] parts = seg.Split('=');
                    if (parts.Length > 0)
                    {
                        string Key = parts[0].Trim();
                        string Value = parts[1].Trim();
                        Params.Add(Key, Value);
                    }
                }
            }
            return Params;
        }

        #endregion
    }
}
