using PB.Shared.Enum;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using PB.DatabaseFramework;
using PB.IdentityServer;
using PB.Model;
using PB.Shared.Tables;
using PB.Shared.Tables.Accounts.LedgerTypes;
using PB.Shared.Tables.Accounts.VoucherTypes;
using PB.Shared.Tables.Accounts.AccountGroups;
using PB.EntityFramework;
using PB.Shared.Tables.Inventory.InvoiceType;
using PB.Shared.Tables.CourtClient;
using PB.Shared.Enum.Inventory;
using PB.Shared.Tables.Inventory.Items;
using Microsoft.AspNetCore.Identity;
using PB.Shared.Tables.Tax;
using PB.Shared.Enum.Accounts;

namespace PB.Server.Repository
{
    public interface IDatabaseInitializer
    {
        Task InsertDefaultEntries();
    }

    public class DatabaseInitializer : IDatabaseInitializer
    {
        private readonly IDbContext _dbContext;
        private readonly IDbConnection _cn;
        private readonly IIdentityDatabaseInitializer _identity;
        private readonly IAccountRepository _accounts;
        private int currentVersion;
        private bool isNew = false;

        public DatabaseInitializer(IDbContext dbContext, IDbConnection cn, IIdentityDatabaseInitializer identity, IAccountRepository acc)
        {
            this._dbContext = dbContext;
            this._cn = cn;
            this._identity = identity;
            this._accounts = acc;
        }

        public async Task InsertDefaultEntries()
        {
            _cn.Open();
            using (var tran = _cn.BeginTransaction())
            {
                try
                {
                    currentVersion = await _dbContext.GetByQueryAsync<int>("Select DBVersion From GeneralSetting", null, tran);

                    if (currentVersion == 0)
                    {
                        isNew = true;
                        await Version1(tran);
                        await RegisterFirstClient(tran, "Progbiz", "Progbiz@2030", 101, "9076435672", "info@progbiz.io");
                        currentVersion = 4;
                    }

                    if (currentVersion == 4)
                    {
                        await Version5(tran);
                        currentVersion++;
                    }

                    //if (currentVersion == 5)
                    //{
                    //    await RegisterFirstClient(tran);
                    //    currentVersion = 6;
                    //}

                    await _dbContext.ExecuteAsync($@"Update GeneralSetting Set DBVersion='{currentVersion}'", null, tran);
                    tran.Commit();

                }
                catch (Exception err)
                {
                    Console.WriteLine(err.Message);
                    tran.Rollback();
                }
            }
        }

        public async Task Version1(IDbTransaction tran)
        {
            #region General Settings

            GeneralSetting generalSetting = new()
            {
                DBVersion = 1,
                ID = 1,
            };
            await _dbContext.NonIdentityInsertAsync(generalSetting, tran);

            #endregion

            #region MailSettings

            await _dbContext.IdentityInsertAsync(new MailSettings()
            {
                MailSettingsID = 1,
                SMTPHost = "smtp.gmail.com",
                Port = 587,
                EmailAddress = "dev.progbiz@gmail.com",
                Name = "PnL",
                Password = "iiqsfvwelzfyxrcr",
                EnableSSL = true,
                IsDeleted = false
            }, tran);

            #endregion

            #region User Types

            var userTypes = new List<UserType>
            {
                new () { UserTypeID = (int)DefaultUserTypes.SuperAdmin, PriorityOrder = 0, UserTypeName = "SuperAdmin", DisplayName = "Super Admin" },
                new () { UserTypeID = (int)DefaultUserTypes.Developer, PriorityOrder = 0, UserTypeName = "Developer", DisplayName = "Developer" },
                new () { UserTypeID = (int)DefaultUserTypes.Support, PriorityOrder = 1, UserTypeName = "Support User", DisplayName = "Support User" },
                new () { UserTypeID = (int)DefaultUserTypes.Client, PriorityOrder = 1, UserTypeName = "Client", DisplayName = "Administrator" },
                new () { UserTypeID = (int)UserTypes.Staff, PriorityOrder = 2, UserTypeName = "Staff", DisplayName = "Staff" },
                //new () { UserTypeID = (int)UserTypes.Customer, PriorityOrder = 5, UserTypeName = "Customer", DisplayName = "Customer" }
            };
            await _dbContext.IdentityInsertRangeAsync(userTypes, tran);

            #endregion

            #region Languages

            List<Language> languages = new()
                {
                    new Language { LanguageCode="af",LanguageName="Afrikaans"},
                    new Language { LanguageCode="sq",LanguageName="Albanian"},
                    new Language { LanguageCode="ar",LanguageName="Arabic"},
                    new Language { LanguageCode="az",LanguageName="Azerbaijani"},
                    new Language { LanguageCode="bn",LanguageName="Bengali"},
                    new Language { LanguageCode="bg",LanguageName="Bulgarian"},
                    new Language { LanguageCode="ca",LanguageName="Catalan"},
                    new Language { LanguageCode="zh_CN",LanguageName="Chinese (CHN)"},
                    new Language { LanguageCode="zh_HK",LanguageName="Chinese (HKG)"},
                    new Language { LanguageCode="zh_TW",LanguageName="Chinese (TAI)"},
                    new Language { LanguageCode="hr",LanguageName="Croatian"},
                    new Language { LanguageCode="cs",LanguageName="Czech"},
                    new Language { LanguageCode="da",LanguageName="Danish"},
                    new Language { LanguageCode="nl",LanguageName="Dutch"},
                    new Language { LanguageCode="en",LanguageName="English"},
                    new Language { LanguageCode="en_GB",LanguageName="English (UK)"},
                    new Language { LanguageCode="en_US",LanguageName="English (US)"},
                    new Language { LanguageCode="et",LanguageName="Estonian"},
                    new Language { LanguageCode="fil",LanguageName="Filipino"},
                    new Language { LanguageCode="fi",LanguageName="Finnish"},
                    new Language { LanguageCode="fr",LanguageName="French"},
                    new Language { LanguageCode="de",LanguageName="German"},
                    new Language { LanguageCode="el",LanguageName="Greek"},
                    new Language { LanguageCode="gu",LanguageName="Gujarati"},
                    new Language { LanguageCode="ha",LanguageName="Hausa"},
                    new Language { LanguageCode="he",LanguageName="Hebrew"},
                    new Language { LanguageCode="hi",LanguageName="Hindi"},
                    new Language { LanguageCode="hu",LanguageName="Hungarian"},
                    new Language { LanguageCode="id",LanguageName="Indonesian"},
                    new Language { LanguageCode="ga",LanguageName="Irish"},
                    new Language { LanguageCode="it",LanguageName="Italian"},
                    new Language { LanguageCode="ja",LanguageName="Japanese"},
                    new Language { LanguageCode="kn",LanguageName="Kannada"},
                    new Language { LanguageCode="kk",LanguageName="Kazakh"},
                    new Language { LanguageCode="ko",LanguageName="Korean"},
                    new Language { LanguageCode="lo",LanguageName="Lao"},
                    new Language { LanguageCode="lv",LanguageName="Latvian"},
                    new Language { LanguageCode="lt",LanguageName="Lithuanian"},
                    new Language { LanguageCode="mk",LanguageName="Macedonian"},
                    new Language { LanguageCode="ms",LanguageName="Malay"},
                    new Language { LanguageCode="ml",LanguageName="Malayalam"},
                    new Language { LanguageCode="mr",LanguageName="Marathi"},
                    new Language { LanguageCode="nb",LanguageName="Norwegian"},
                    new Language { LanguageCode="fa",LanguageName="Persian"},
                    new Language { LanguageCode="pl",LanguageName="Polish"},
                    new Language { LanguageCode="pt_BR",LanguageName="Portuguese (BR)"},
                    new Language { LanguageCode="pt_PT",LanguageName="Portuguese (POR)"},
                    new Language { LanguageCode="pa",LanguageName="Punjabi"},
                    new Language { LanguageCode="ro",LanguageName="Romanian"},
                    new Language { LanguageCode="ru",LanguageName="Russian"},
                    new Language { LanguageCode="sr",LanguageName="Serbian"},
                    new Language { LanguageCode="sk",LanguageName="Slovak"},
                    new Language { LanguageCode="sl",LanguageName="Slovenian"},
                    new Language { LanguageCode="es",LanguageName="Spanish"},
                    new Language { LanguageCode="es_AR",LanguageName="Spanish (ARG)"},
                    new Language { LanguageCode="es_ES",LanguageName="Spanish (SPA)"},
                    new Language { LanguageCode="es_MX",LanguageName="Spanish (MEX)"},
                    new Language { LanguageCode="sw",LanguageName="Swahili"},
                    new Language { LanguageCode="sv",LanguageName="Swedish"},
                    new Language { LanguageCode="ta",LanguageName="Tamil"},
                    new Language { LanguageCode="te",LanguageName="Telugu"},
                    new Language { LanguageCode="th",LanguageName="Thai"},
                    new Language { LanguageCode="tr",LanguageName="Turkish"},
                    new Language { LanguageCode="uk",LanguageName="Ukrainian"},
                    new Language { LanguageCode="ur",LanguageName="Urdu"},
                    new Language { LanguageCode="uz",LanguageName="Uzbek"},
                    new Language { LanguageCode="vi",LanguageName="Vietnamese"},
                    new Language { LanguageCode="zu",LanguageName="Zulu"},
                };

            for (int i = 0; i < languages.Count; i++)
            {
                languages[i].LanguageID = i + 1;
            }
            await _dbContext.IdentityInsertRangeAsync(languages, tran);

            #endregion

            #region Lead Through

            List<LeadThrough> leadMedais = new()
                {
                    new()
                    {
                        LeadThroughID = (int)CRM.Model.Enum.LeadThroughs.Facebook,
                        Name = "Facebook"
                    },

                    new()
                    {
                        LeadThroughID = (int)CRM.Model.Enum.LeadThroughs.Instagram,
                        Name = "Instagram"
                    },

                    new()
                    {
                        LeadThroughID = (int)CRM.Model.Enum.LeadThroughs.Whatsapp,
                        Name = "Whatsapp"
                    },

                    new()
                    {
                        LeadThroughID = (int)CRM.Model.Enum.LeadThroughs.Office,
                        Name = "Office"
                    },

                    new()
                    {
                        LeadThroughID = (int)CRM.Model.Enum.LeadThroughs.Website,
                        Name = "Website"
                    },

                    new()
                    {
                        LeadThroughID = (int)CRM.Model.Enum.LeadThroughs.Email,
                        Name = "Email"
                    },

                    new()
                    {
                        LeadThroughID = (int)CRM.Model.Enum.LeadThroughs.Phone,
                        Name = "Phone"
                    },

                    new()
                    {
                        LeadThroughID = (int)CRM.Model.Enum.LeadThroughs.Advertisement,
                        Name = "Advertisement"
                    },

                    new()
                    {
                        LeadThroughID = (int)CRM.Model.Enum.LeadThroughs.Event,
                        Name = "Event"
                    },

                    new()
                    {
                        LeadThroughID = (int)CRM.Model.Enum.LeadThroughs.Other,
                        Name = "Other"
                    },

                    new()
                    {
                        LeadThroughID = (int)CRM.Model.Enum.LeadThroughs.Referral,
                        Name = "Referral"
                    },


                };
            await _dbContext.IdentityInsertRangeAsync(leadMedais, tran);

            #endregion

            #region Tax Preferences

            List<TaxPreference> taxPreferences = new()
                {
                    new()
                    {
                        TaxPreferenceTypeID = (int)TaxPreferences.NonTaxable,
                        TaxPreferenceName = "Non-Taxable"
                    },
                    new()
                    {
                        TaxPreferenceTypeID = (int)TaxPreferences.Taxable,
                        TaxPreferenceName = "Taxable"
                    },
                    new()
                    {
                        TaxPreferenceTypeID = (int)TaxPreferences.OutOfScope,
                        TaxPreferenceName = "Out of Scope"
                    },
                    new()
                    {
                        TaxPreferenceTypeID = (int)TaxPreferences.NonGstSupply,
                        TaxPreferenceName = "Non-GST Supply"
                    },
                };
            await _dbContext.IdentityInsertRangeAsync(taxPreferences, tran);

            #endregion

            #region Role Group

            List<RoleGroup> roleGroups = new()
                 {
                    new()
                    {
                        RoleGroupID = (int)Shared.Enum.RoleGroups.CRM,
                        RoleGroupName = "CRM",
                        DisplayName="CRM"
                    },
                        new()
                    {
                        RoleGroupID = (int)Shared.Enum.RoleGroups.Whatsapp,
                        RoleGroupName = "Whatsapp",
                        DisplayName="Whatsapp"
                    },
                    new()
                    {
                        RoleGroupID = (int)Shared.Enum.RoleGroups.Common,
                        RoleGroupName = "Common",
                        DisplayName="Common"
                    },
                    new()
                    {
                        RoleGroupID = (int)Shared.Enum.RoleGroups.SupportRoles,
                        RoleGroupName = "SupportRoles",
                        DisplayName="Support Roles"
                    },
                    new()
                    {
                        RoleGroupID = (int)Shared.Enum.RoleGroups.CourtManagment,
                        RoleGroupName = "CourtManagment",
                        DisplayName="Court Managment"
                    },
                    new()
                    {
                        RoleGroupID = (int)Shared.Enum.RoleGroups.ClientRoles,
                        RoleGroupName = "ClientRoles",
                        DisplayName="Client Roles"
                    },
                    new()
                    {
                        RoleGroupID = (int)Shared.Enum.RoleGroups.SpinWheel,
                        RoleGroupName = "SpinWheel",
                        DisplayName = "SpinWheel"
                    },
                    new ()
                    {
                        RoleGroupID = (int)Shared.Enum.RoleGroups.AccountsManagement,
                        RoleGroupName = "AccountsManagement",
                        DisplayName = "Accounts Management"
                    },
                    new ()
                    {
                        RoleGroupID = (int)Shared.Enum.RoleGroups.InventoryManagement,
                        RoleGroupName = "InventoryManagement",
                        DisplayName = "Inventory Management"
                    }
                };
            await _dbContext.IdentityInsertRangeAsync(roleGroups, tran);

            #endregion

            #region Roles

            List<Role> roles = new()
                {
                    //CRM Roles
                new Role() { RoleID = (int)Roles.Enquiry, RoleGroupID = (int)Shared.Enum.RoleGroups.CRM, RoleName = Roles.Enquiry.ToString(), DisplayName = "Enquiry" },
                new Role() { RoleID = (int)Roles.Quotation, RoleGroupID = (int)Shared.Enum.RoleGroups.CRM, RoleName = Roles.Quotation.ToString(), DisplayName = "Quotation" },
                new Role() { RoleID = (int)Roles.FollowUp, RoleGroupID = (int)Shared.Enum.RoleGroups.CRM, RoleName = Roles.FollowUp.ToString(), DisplayName = "Follow-up" },
                new Role() { RoleID = (int)Roles.Customer, RoleGroupID = (int)Shared.Enum.RoleGroups.CRM, RoleName = Roles.Customer.ToString(), DisplayName = "Customer" },
                  
                //Whatsapp Roles
                new Role() { RoleID = (int)Roles.BroadCast, RoleGroupID = (int)Shared.Enum.RoleGroups.Whatsapp, RoleName = Roles.BroadCast.ToString(), DisplayName = "BroadCast" },
                new Role() { RoleID = (int)Roles.Chat, RoleGroupID = (int)Shared.Enum.RoleGroups.Whatsapp, RoleName = Roles.Chat.ToString(), DisplayName = "Chat" },
                new Role() { RoleID = (int)Roles.Template, RoleGroupID = (int)Shared.Enum.RoleGroups.Whatsapp, RoleName = Roles.Template.ToString(), DisplayName = "Template" },
                new Role() { RoleID = (int)Roles.ChatbotSetup, RoleGroupID = (int)Shared.Enum.RoleGroups.Whatsapp, RoleName = Roles.ChatbotSetup.ToString(), DisplayName = "Chatbot Setup" },
                new Role() { RoleID = (int)Roles.WhatsappAccount, RoleGroupID = (int)Shared.Enum.RoleGroups.Whatsapp, RoleName = Roles.WhatsappAccount.ToString(), DisplayName = "Whatsapp Account" },
                
                //Common Roles
                new Role() { RoleID = (int)Roles.State, RoleGroupID = (int)Shared.Enum.RoleGroups.Common, RoleName = Roles.State.ToString(), DisplayName = "State" },
                new Role() { RoleID = (int)Roles.City, RoleGroupID = (int)Shared.Enum.RoleGroups.Common, RoleName = Roles.City.ToString(), DisplayName = "City" },
                new Role() { RoleID = (int)Roles.Common, RoleGroupID = (int)Shared.Enum.RoleGroups.Common, RoleName = Roles.Common.ToString(), DisplayName = "Common" },
                
                //Support Roles
                new Role() { RoleID = (int)Roles.ClientManagement, RoleGroupID = (int)Shared.Enum.RoleGroups.SupportRoles, RoleName = Roles.ClientManagement.ToString(), DisplayName = "Clients",IsForSupport=true },
                new Role() { RoleID = (int)Roles.MembershipManagment, RoleGroupID = (int)Shared.Enum.RoleGroups.SupportRoles, RoleName = Roles.MembershipManagment.ToString(), DisplayName = "Membership Managment",IsForSupport=true },
                new Role() { RoleID = (int)Roles.PaymentsManagment, RoleGroupID = (int)Shared.Enum.RoleGroups.SupportRoles, RoleName = Roles.PaymentsManagment.ToString(), DisplayName = "Payments Managment",IsForSupport=true },
                
                //Court Booking Roles
                new Role() { RoleID = (int)Roles.Hall, RoleGroupID = (int)Shared.Enum.RoleGroups.CourtManagment, RoleName = Roles.Hall.ToString(), DisplayName = "Hall"},
                new Role() { RoleID = (int)Roles.HallTiming, RoleGroupID = (int)Shared.Enum.RoleGroups.CourtManagment, RoleName = Roles.HallTiming.ToString(), DisplayName = "HallTiming"},
                new Role() { RoleID = (int)Roles.Court, RoleGroupID = (int)Shared.Enum.RoleGroups.CourtManagment, RoleName = Roles.Court.ToString(), DisplayName = "Court"},
                new Role() { RoleID = (int)Roles.CourtBooking, RoleGroupID = (int)Shared.Enum.RoleGroups.CourtManagment, RoleName = Roles.CourtBooking.ToString(), DisplayName = "CourtBooking"},
                new Role() { RoleID = (int)Roles.CourtPackage, RoleGroupID = (int)Shared.Enum.RoleGroups.CourtManagment, RoleName = Roles.CourtPackage.ToString(), DisplayName = "Court Package" },
                
                //Client Roles
                new Role() { RoleID = (int)Roles.Branch, RoleGroupID = (int)Shared.Enum.RoleGroups.ClientRoles, RoleName = Roles.Branch.ToString(), DisplayName = "Branch" },
                new Role() { RoleID = (int)Roles.Users, RoleGroupID = (int)Shared.Enum.RoleGroups.ClientRoles, RoleName = Roles.Users.ToString(), DisplayName = "Users" },
                new Role() { RoleID = (int)Roles.ClientSetting, RoleGroupID = (int)Shared.Enum.RoleGroups.ClientRoles, RoleName = Roles.ClientSetting.ToString(), DisplayName = "Client Setting" },
                
                //SpinWheel Roles
                new Role() { RoleID = (int)Roles.SpinWheel, RoleGroupID = (int)Shared.Enum.RoleGroups.SpinWheel, RoleName = Roles.SpinWheel.ToString(), DisplayName = "SpinWheel" },
                
                //Accounts
                new(){RoleID=(int)Roles.AccountsGroup,RoleGroupID = (int)RoleGroups.AccountsManagement,RoleName = Roles.AccountsGroup.ToString(),DisplayName = "Account Group",IsForSupport = false},
                new(){RoleID=(int)Roles.Accounts,RoleGroupID = (int)RoleGroups.AccountsManagement,RoleName = Roles.Accounts.ToString(),DisplayName = "Accounts",IsForSupport = false},
                new(){RoleID=(int)Roles.VoucherType,RoleGroupID = (int)RoleGroups.AccountsManagement,RoleName = Roles.VoucherType.ToString(),DisplayName = "Voucher Type",IsForSupport = false},
                new(){RoleID=(int)Roles.VoucherEntry,RoleGroupID = (int)RoleGroups.AccountsManagement,RoleName = Roles.VoucherEntry.ToString(),DisplayName = "Voucher Entry",IsForSupport = false},

                //Inventory
                new(){RoleID=(int)Roles.InvoiceType,RoleGroupID = (int)RoleGroups.InventoryManagement,RoleName = Roles.InvoiceType.ToString(),DisplayName = "Invoice Type",IsForSupport = false},
                new(){RoleID=(int)Roles.Invoice,RoleGroupID = (int)RoleGroups.InventoryManagement,RoleName = Roles.Invoice.ToString(),DisplayName = "Invoice",IsForSupport = false},
                new() { RoleID = (int)Roles.Item, RoleGroupID = (int)RoleGroups.InventoryManagement, RoleName = Roles.Item.ToString(), DisplayName = "Item" },
            
                //AISC Membership
                new Role() { RoleID = (int)Roles.AISCMembership, RoleGroupID = (int)Shared.Enum.RoleGroups.SpinWheel, RoleName = Roles.AISCMembership.ToString(), DisplayName = "AISC Membership" },
            };

            await _dbContext.IdentityInsertRangeAsync(roles, tran);

            #endregion

            #region Default account group

            List<AccAccountGroup> accountGroups = new()
                {
                    new AccAccountGroup()
                    {
                        AccountGroupID=(int)AccountGroups.Asset,
                        AccountGroupName="Asset",
                    },
                    new AccAccountGroup()
                    {
                        AccountGroupID=(int)AccountGroups.Liability,
                        AccountGroupName="Liability",
                    },
                    new AccAccountGroup()
                    {
                        AccountGroupID=(int)AccountGroups.DirectIncome,
                        AccountGroupName="Direct Income",
                    },
                    new AccAccountGroup()
                    {
                        AccountGroupID=(int)AccountGroups.DirectExpense,
                        AccountGroupName="Direct Expense",
                    },
                    new AccAccountGroup()
                    {
                        AccountGroupID=(int)AccountGroups.IndirectIncome,
                        AccountGroupName="Indirect Income",
                    },
                    new AccAccountGroup()
                    {
                        AccountGroupID=(int)AccountGroups.IndirectExpense,
                        AccountGroupName="Indirect Expense",
                    }
                };
            await _dbContext.IdentityInsertRangeAsync(accountGroups, tran);

            #endregion

            #region Default Voucher Type Nature

            List<AccVoucherTypeNature> natureTypes = new()
                {
                    new AccVoucherTypeNature()
                    {
                        VoucherTypeNatureID=(int)VoucherTypeNatures.Receipt,
                        VoucherTypeNatureName="Reciept",
                        ShowInList=true,
                        SlNo=(int)VoucherTypeNatures.Receipt
                    },
                    new AccVoucherTypeNature()
                    {
                        VoucherTypeNatureID=(int)VoucherTypeNatures.Payment,
                        VoucherTypeNatureName="Payment",
                        ShowInList=true,
                        SlNo=(int)VoucherTypeNatures.Payment
                    },
                    new AccVoucherTypeNature()
                    {
                        VoucherTypeNatureID=(int)VoucherTypeNatures.Contra,
                        VoucherTypeNatureName="Contra",
                        ShowInList=true,
                        SlNo=(int)VoucherTypeNatures.Contra
                    },
                    new AccVoucherTypeNature()
                    {
                        VoucherTypeNatureID=(int)VoucherTypeNatures.Journal,
                        VoucherTypeNatureName="Journal",
                        ShowInList=true,
                        SlNo=(int)VoucherTypeNatures.Journal
                    },
                    new AccVoucherTypeNature()
                    {
                        VoucherTypeNatureID=(int)VoucherTypeNatures.Sales,
                        ShowInList=false,
                        VoucherTypeNatureName="Sale",
                        SlNo=0
                    },
                    new AccVoucherTypeNature()
                    {
                        VoucherTypeNatureID=(int)VoucherTypeNatures.SalesReturn,
                        ShowInList=false,
                        VoucherTypeNatureName="Sale Return",
                        SlNo=0
                    },
                    new AccVoucherTypeNature()
                    {
                        VoucherTypeNatureID=(int)VoucherTypeNatures.Purchase,
                        ShowInList=false,
                        VoucherTypeNatureName="Purchase",
                        SlNo=0
                    },
                    new AccVoucherTypeNature()
                    {
                        VoucherTypeNatureID=(int)VoucherTypeNatures.PurchaseReturn,
                        ShowInList=false,
                        VoucherTypeNatureName="Purchase Return",
                        SlNo=0
                    },
                };
            await _dbContext.NonIdentityInsertRangeAsync(natureTypes, tran);

            #endregion

            #region Invoice Type Nature

            List<InvoiceTypeNature> invoiceTypeNatures = new()
                {
                    new()
                    {
                        InvoiceTypeNatureID = (int)InvoiceTypeNatures.Sales,
                        InvoiceTypeNatureName = "Sales",
                        IsDeleted = false

                    },
                    new()
                    {
                        InvoiceTypeNatureID = (int)InvoiceTypeNatures.Sales_Return,
                        InvoiceTypeNatureName = "Sales Return",
                        IsDeleted = false
                    },
                    new()
                    {
                        InvoiceTypeNatureID = (int)InvoiceTypeNatures.Purchase,
                        InvoiceTypeNatureName = "Purchase",
                        IsDeleted = false
                    },
                    new()
                    {
                        InvoiceTypeNatureID = (int)InvoiceTypeNatures.Purchase_Return,
                        InvoiceTypeNatureName = "Purchase Return",
                        IsDeleted = false
                    },
                };

            await _dbContext.NonIdentityInsertRangeAsync(invoiceTypeNatures, tran);

            #endregion

            #region Hour

            List<HourMaster> hours = new()
            {

                new()
                {
                    HourID = 1,
                    HourName="1 AM",
                    HourInMinute =60
                },
                new()
                {
                    HourID = 2,
                    HourName="2 AM",
                    HourInMinute =120
                },new()
                {
                    HourID = 3,
                    HourName="3 AM",
                    HourInMinute =180
                },new()
                {
                    HourID = 4,
                    HourName="4 AM",
                    HourInMinute =240
                },new()
                {
                    HourID = 5,
                    HourName="5 AM",
                    HourInMinute =300
                },new()
                {
                    HourID = 6,
                    HourName="6 AM",
                    HourInMinute =360
                },new()
                {
                    HourID = 7,
                    HourName="7 AM",
                    HourInMinute =420
                },new()
                {
                    HourID = 8,
                    HourName="8 AM",
                    HourInMinute =480
                },new()
                {
                    HourID = 9,
                    HourName="9 AM",
                    HourInMinute =540
                },new()
                {
                    HourID = 10,
                    HourName="10 AM",
                    HourInMinute =600
                },new()
                {
                    HourID = 11,
                    HourName="11 AM",
                    HourInMinute =660
                },new()
                {
                    HourID = 12,
                    HourName="12 PM",
                    HourInMinute =720
                },new()
                {
                    HourID = 13,
                    HourName="01 PM",
                    HourInMinute =780
                },new()
                {
                    HourID = 14,
                    HourName="02 PM",
                    HourInMinute =840
                },new()
                {
                    HourID = 15,
                    HourName="03 PM",
                    HourInMinute =900
                },new()
                {
                    HourID = 16,
                    HourName="04 PM",
                    HourInMinute =960
                },new()
                {
                    HourID = 17,
                    HourName="05 PM",
                    HourInMinute =1020
                },new()
                {
                    HourID = 18,
                    HourName="06 PM",
                    HourInMinute =1080
                },new()
                {
                    HourID = 19,
                    HourName="07 PM",
                    HourInMinute =1140
                },new()
                {
                    HourID = 20,
                    HourName="08 PM",
                    HourInMinute =1200
                },new()
                {
                    HourID = 21,
                    HourName="09 PM",
                    HourInMinute =1260
                },new()
                {
                    HourID = 22,
                    HourName="10 PM",
                    HourInMinute =1320
                },new()
                {
                    HourID = 23,
                    HourName="11 PM",
                    HourInMinute =1380
                },new()
                {
                    HourID = 24,
                    HourName="12 AM",
                    HourInMinute =1440
                }
            };
            await _dbContext.IdentityInsertRangeAsync(hours, tran);

            #endregion

            #region Default User

            string superAdminPassword = "admin@2023";
            string developerPassword = "developer@2023";


            #region Super admin

            var salt1 = Guid.NewGuid().ToString("n").Substring(0, 8);
            var hashPassword1 = AuthenticationRepository.GetHashPassword(superAdminPassword, salt1);

            var entityId1 = await _dbContext.SaveAsync(new Entity()
            {
                EntityTypeID = 1,
                EmailAddress = "admin@mail.com",
                Phone = ""
            }, tran);

            await _dbContext.SaveAsync(new EntityPersonalInfo()
            {
                FirstName = "Super Admin",
                EntityID = entityId1
            }, tran);

            await _dbContext.SaveAsync(
                new User()
                {
                    UserTypeID = (int)DefaultUserTypes.SuperAdmin,
                    EmailConfirmed = true,
                    LoginStatus = true,
                    Password = hashPassword1,
                    PasswordHash = salt1,
                    EntityID = entityId1,
                    UserName = "admin"
                }, tran);
            #endregion

            #region Developer User

            if (!string.IsNullOrEmpty(developerPassword))
            {
                var salt = Guid.NewGuid().ToString("n").Substring(0, 8);
                var hashPassword = AuthenticationRepository.GetHashPassword(developerPassword, salt);

                var entityId = await _dbContext.SaveAsync(new Entity()
                {
                    EntityTypeID = 1,
                    EmailAddress = "progbiz.io@gmail.com",
                    Phone = ""
                }, tran);

                await _dbContext.SaveAsync(new EntityPersonalInfo()
                {
                    FirstName = "Progbiz",
                    EntityID = entityId
                }, tran);

                await _dbContext.SaveAsync(
                    new User()
                    {
                        UserTypeID = (int)DefaultUserTypes.Developer,
                        EmailConfirmed = true,
                        LoginStatus = true,
                        Password = hashPassword,
                        PasswordHash = salt,
                        EntityID = entityId,
                        UserName = "developer"
                    }, tran);
            }

            #endregion

            #endregion
        }

        public async Task Version5(IDbTransaction tran)
        {
            try
            {
                #region Packing Type

                List<ItemPackingType> itemPackingTypes = new()
                {
                    new()
                    {
                        PackingTypeID = (int)PackingTypes.Piece,
                        PackingTypeCode = "PC",
                        PackingTypeName = "Piece"
                    },
                    new()
                    {
                        PackingTypeID = (int)PackingTypes.Box,
                        PackingTypeCode = "Box",
                        PackingTypeName = "Box"
                    },
                    new()
                    {
                        PackingTypeID = (int)PackingTypes.Case,
                        PackingTypeCode = "Case",
                        PackingTypeName = "Case"
                    },
                    new()
                    {
                        PackingTypeID = (int)PackingTypes.KiloGrams,
                        PackingTypeCode = "KG",
                        PackingTypeName = "Kilo Grams"
                    },
                    new()
                    {
                        PackingTypeID = (int)PackingTypes.Meter,
                        PackingTypeCode = "Mtr",
                        PackingTypeName = "Meter"
                    },
                    new()
                    {
                        PackingTypeID = (int)PackingTypes.Carton,
                        PackingTypeCode = "Crt",
                        PackingTypeName = "Carton"
                    },
                    new()
                    {
                        PackingTypeID = (int)PackingTypes.Bundle,
                        PackingTypeCode = "Bndl",
                        PackingTypeName = "Bundle"
                    },
                    new()
                    {
                        PackingTypeID = (int)PackingTypes.Bag,
                        PackingTypeCode = "Bg",
                        PackingTypeName = "Bag"
                    },
                    new()
                    {
                        PackingTypeID = (int)PackingTypes.Litres,
                        PackingTypeCode = "Ltr",
                        PackingTypeName = "Litres"
                    },
                    new()
                    {
                        PackingTypeID = (int)PackingTypes.Loose,
                        PackingTypeCode = "Ls",
                        PackingTypeName = "Loose"
                    },
                    new()
                    {
                        PackingTypeID = (int)PackingTypes.Session,
                        PackingTypeCode = "Ssn",
                        PackingTypeName = "Session"
                    },
                    new()
                    {
                        PackingTypeID = (int)PackingTypes.Task,
                        PackingTypeCode = "Tk",
                        PackingTypeName = "Task"
                    },
                    new()
                    {
                        PackingTypeID = (int)PackingTypes.Project,
                        PackingTypeCode = "Prj",
                        PackingTypeName = "Project"
                    },
                    new()
                    {
                        PackingTypeID = (int)PackingTypes.Appointment,
                        PackingTypeCode = "Appt",
                        PackingTypeName = "Appointment"
                    },
                    new()
                    {
                        PackingTypeID = (int)PackingTypes.Mile,
                        PackingTypeCode = "Ml",
                        PackingTypeName = "Mile"
                    },
                    new()
                    {
                        PackingTypeID = (int)PackingTypes.Class,
                        PackingTypeCode = "Cls",
                        PackingTypeName = "Class"
                    },
                };
                await _dbContext.IdentityInsertRangeAsync(itemPackingTypes, tran);

                #endregion
            }
            catch (Exception e) { }

            #region New Role Group

            RoleGroup Report = new()
            {
                RoleGroupID = (int)Shared.Enum.RoleGroups.Report,
                RoleGroupName = "Reports",
                DisplayName = "Reports"
            };
            await _dbContext.IdentityInsertAsync(Report, tran);

            #endregion

            #region New roles

            List<Role> roles = new()
                {
                    //Accounts Report Roles
                    new Role() { RoleID = (int)Roles.BalanceSheet, RoleGroupID = (int)Shared.Enum.RoleGroups.Report, RoleName = Roles.BalanceSheet.ToString(), DisplayName = "Balancesheet" },
                    new Role() { RoleID = (int)Roles.GeneralLedger, RoleGroupID = (int)Shared.Enum.RoleGroups.Report, RoleName = Roles.GeneralLedger.ToString(), DisplayName = "General Ledger" },
                    new Role() { RoleID = (int)Roles.JournalReport, RoleGroupID = (int)Shared.Enum.RoleGroups.Report, RoleName = Roles.JournalReport.ToString(), DisplayName = "Journal Report" },
                    new Role() { RoleID = (int)Roles.ProfitAndLoss, RoleGroupID = (int)Shared.Enum.RoleGroups.Report, RoleName = Roles.ProfitAndLoss.ToString(), DisplayName = "Profit and Loss" },
                    new Role() { RoleID = (int)Roles.TrialBalance, RoleGroupID = (int)Shared.Enum.RoleGroups.Report, RoleName = Roles.TrialBalance.ToString(), DisplayName = "Trial Balance" },
                    new Role() { RoleID = (int)Roles.RAndD, RoleGroupID = (int)Shared.Enum.RoleGroups.Report, RoleName = Roles.RAndD.ToString(), DisplayName = "Reciept And Disbursement" },
                    
                    //ItemStock Roles
                    new Role() { RoleID = (int)Roles.ItemStock, RoleGroupID = (int)Shared.Enum.RoleGroups.InventoryManagement, RoleName = Roles.ItemStock.ToString(), DisplayName = "Item Stock" },
                    new Role() { RoleID = (int)Roles.StockAdjustment, RoleGroupID = (int)Shared.Enum.RoleGroups.InventoryManagement, RoleName = Roles.StockAdjustment.ToString(), DisplayName = "Stock Adjustment" },
                    new(){RoleID=(int)Roles.Supplier,RoleGroupID = (int)RoleGroups.InventoryManagement,RoleName = Roles.Supplier.ToString(),DisplayName = "Supplier",IsForSupport = false},
                
                    //Client Invoice
				    new Role() { RoleID = (int)Roles.ClientInvoice, RoleGroupID = (int)Shared.Enum.RoleGroups.ClientRoles, RoleName = Roles.ClientInvoice.ToString(), DisplayName = "Client Setting" },

                    //Inventory Report Roles
                    new Role() { RoleID = (int)Roles.SalesByItemReport, RoleGroupID = (int)Shared.Enum.RoleGroups.Report, RoleName = Roles.SalesByItemReport.ToString(), DisplayName = "Sales By Item" },
                    new Role() { RoleID = (int)Roles.SalesByCustomerReport, RoleGroupID = (int)Shared.Enum.RoleGroups.Report, RoleName = Roles.SalesByCustomerReport.ToString(), DisplayName = "Sales By Customer" },
                    new Role() { RoleID = (int)Roles.SalesByStaffReport, RoleGroupID = (int)Shared.Enum.RoleGroups.Report, RoleName = Roles.SalesByStaffReport.ToString(), DisplayName = "Sales By Staff" },
                };
            await _dbContext.IdentityInsertRangeAsync(roles, tran);

            #endregion

            #region Invoice Type Nature Addition

            List<InvoiceTypeNature> invoiceTypeNatures = new()
                {
                    new()
                    {
                        InvoiceTypeNatureID = (int)InvoiceTypeNatures.Stock_Adjustment,
                        InvoiceTypeNatureName = "Stock Adjustment",
                        IsDeleted = false

                    },
                };

            await _dbContext.NonIdentityInsertRangeAsync(invoiceTypeNatures, tran);

            #endregion

            #region Account Group Type

            List<AccAccountGroupType> accountGroupTypes = new()
             {
                 new()
                 {
                     GroupTypeID=(int)AccountGroupTypes.Assets,
                     GroupTypeName=AccountGroupTypes.Assets.ToString(),
                     Nature=(int)AccountGroupNatures.Asset,
                 },
                 new()
                 {
                     GroupTypeID=(int)AccountGroupTypes.Liabilities,
                     GroupTypeName=AccountGroupTypes.Liabilities.ToString(),
                     Nature=(int)AccountGroupNatures.Liability,
                 },
                 new()
                 {
                     GroupTypeID=(int)AccountGroupTypes.DirectExpenses,
                     GroupTypeName="Direct Expenses",
                     Nature=(int)AccountGroupNatures.DirectExpense,
                 },
                 new()
                 {
                     GroupTypeID=(int)AccountGroupTypes.DirectIncome,
                     GroupTypeName="Direct Income",
                     Nature=(int)AccountGroupNatures.DirectIncome,
                 },
                 new()
                 {
                     GroupTypeID=(int)AccountGroupTypes.IndirectExpenses,
                     GroupTypeName="Indirect Expenses",
                     Nature=(int)AccountGroupNatures.IndirectExpense,
                 },
                 new()
                 {
                     GroupTypeID=(int)AccountGroupTypes.IndirectIncome,
                     GroupTypeName="Indirect Income",
                     Nature=(int)AccountGroupNatures.IndirectIncome,
                 },
                 new()
                 {
                     GroupTypeID=(int)AccountGroupTypes.PurchaseAccounts,
                     GroupTypeName="Purchase Accounts",
                     Nature=(int)AccountGroupNatures.IndirectExpense,
                 },
                 new()
                 {
                     GroupTypeID=(int)AccountGroupTypes.SalesAccounts,
                     GroupTypeName="Sales Accounts",
                     Nature=(int)AccountGroupNatures.DirectExpense,
                 },
                 new()
                 {
                     GroupTypeID=(int)AccountGroupTypes.DutiesAndTaxes,
                     GroupTypeName="Duties And Taxes",
                     Nature=(int)AccountGroupNatures.IndirectExpense,
                 },
                 new()
                 {
                     GroupTypeID=(int)AccountGroupTypes.CashInHand,
                     GroupTypeName="Cash In Hand",
                     Nature=(int)AccountGroupNatures.Asset,
                 },
                 new()
                 {
                     GroupTypeID=(int)AccountGroupTypes.BankAccounts,
                     GroupTypeName="Bank Accounts",
                     Nature=(int)AccountGroupNatures.Asset,
                 },
                 new()
                 {
                     GroupTypeID=(int)AccountGroupTypes.CapitalAccounts,
                     GroupTypeName="Capital Accounts",
                     Nature=(int)AccountGroupNatures.IndirectExpense, //Dont know
                 },
                 new()
                 {
                     GroupTypeID=(int)AccountGroupTypes.CurrentAssets,
                     GroupTypeName="Current Assets",
                     Nature=(int)AccountGroupNatures.Asset,
                 },
                 new()
                 {
                     GroupTypeID=(int)AccountGroupTypes.CurrentLiabilities,
                     GroupTypeName="Current Liabilities",
                     Nature=(int)AccountGroupNatures.Liability,
                 },
                 new()
                 {
                     GroupTypeID=(int)AccountGroupTypes.SundryCreditors,
                     GroupTypeName="Sundry Creditors",
                     Nature=(int)AccountGroupNatures.Liability,
                 },
                 new()
                 {
                     GroupTypeID=(int)AccountGroupTypes.SundryDebtors,
                     GroupTypeName="Sundry Debtors",
                     Nature=(int)AccountGroupNatures.Asset,
                 },
                 new()
                 {
                     GroupTypeID=(int)AccountGroupTypes.LoansAndAdvances,
                     GroupTypeName="Loans And Advances",
                     Nature=(int)AccountGroupNatures.Asset,
                 },
                 new()
                 {
                     GroupTypeID=(int)AccountGroupTypes.LoanLiabilities,
                     GroupTypeName="Loan Liabilities",
                     Nature=(int)AccountGroupNatures.Liability,
                 },
                 new()
                 {
                     GroupTypeID=(int)AccountGroupTypes.FixedAssets,
                     GroupTypeName="Fixed Assets",
                     Nature=(int)AccountGroupNatures.Asset,
                 },
                 new()
                 {
                     GroupTypeID=(int)AccountGroupTypes.Investments,
                     GroupTypeName=AccountGroupTypes.Investments.ToString(),
                     Nature=(int)AccountGroupNatures.Asset,
                 },
                 new()
                 {
                     GroupTypeID=(int)AccountGroupTypes.StockInHand,
                     GroupTypeName="Stock In Hand",
                     Nature=(int)AccountGroupNatures.Asset,
                 },
                 new()
                 {
                     GroupTypeID=(int)AccountGroupTypes.GrossProfitOrLoss,
                     GroupTypeName="Gross Profit Or Loss",
                     Nature=(int)AccountGroupNatures.Liability, // Dont know
                 },
                 new()
                 {
                     GroupTypeID=(int)AccountGroupTypes.Depreciation,
                     GroupTypeName=AccountGroupTypes.Depreciation.ToString(),
                     Nature=(int)AccountGroupNatures.Liability, // Dont know
                 },
                 new()
                 {
                     GroupTypeID=(int)AccountGroupTypes.NetProfitOrLoss,
                     GroupTypeName="Net Profit Or Loss",
                     Nature=(int)AccountGroupNatures.Liability, // Dont know
                 },
                 new()
                 {
                     GroupTypeID=(int)AccountGroupTypes.IntangibleAssests,
                     GroupTypeName="Intangible Assests",
                     Nature=(int)AccountGroupNatures.Liability, // Dont know
                 },
                 new()
                 {
                     GroupTypeID=(int)AccountGroupTypes.LongTermLiabilities,
                     GroupTypeName="Long Term Liabilities",
                     Nature=(int)AccountGroupNatures.Liability, // Dont know
                 },
                 new()
                 {
                     GroupTypeID=(int)AccountGroupTypes.Revenue,
                     GroupTypeName=AccountGroupTypes.Revenue.ToString(),
                     Nature=(int)AccountGroupNatures.Liability, // Dont know
                 },
             };

            await _dbContext.IdentityInsertRangeAsync(accountGroupTypes, tran);

            #endregion

            //The hsn and sac data is not present in the database initializer
        }

        private async Task RegisterFirstClient(IDbTransaction tran, string clientName = "Progbiz", string password = "Progbiz@2030", int countryId = (int)Countries.India, string phone = "8089123321", string email = "info@progbiz.io")
        {
            var clients = await _dbContext.GetByQueryAsync<int>($"Select Count(*) From Client", null, tran);
            if (clients == 0)
            {
                var entity = new EntityCustom
                {
                    Phone = phone,
                    EntityTypeID = (int)EntityType.Client,
                    EmailAddress = email,
                    CountryID = countryId,
                };
                entity.EntityID = await _dbContext.SaveAsync(entity, tran);

                var instituteInfo = new EntityInstituteInfo()
                {
                    Name = clientName,
                    EntityID = entity.EntityID,
                };
                await _dbContext.SaveAsync(instituteInfo, tran);

                var client = new ClientCustom
                {
                    EntityID = entity.EntityID,
                };
                client.ClientID = await _dbContext.SaveAsync(client, tran);

                Branch branch = new()
                {
                    ClientID = client.ClientID,
                    EntityID = entity.EntityID,
                };
                branch.BranchID = branch.BranchID = await _dbContext.SaveAsync(branch, tran);

                string passwordHash = Guid.NewGuid().ToString("n").Substring(0, 8);

                User user = new()
                {
                    EntityID = entity.EntityID,
                    LoginStatus = true,
                    UserName = clientName,
                    UserTypeID = (int)UserTypes.Client,
                    Password = UserRepository.GetHashPassword(password, passwordHash),
                    PasswordHash = passwordHash,
                    ClientID = client.ClientID,
                    EmailConfirmed = true,
                };
                user.UserID = await _dbContext.SaveAsync(user, tran);

                MembershipUserCapacity userCapacity = new()
                {
                    Capacity = 10
                };
                userCapacity.CapacityID = await _dbContext.SaveAsync(userCapacity, tran);

                MembershipPlan plan = new MembershipPlan()
                {
                    MonthCount = 1000,
                    PlanName = "Unlimited"
                };
                plan.PlanID = await _dbContext.SaveAsync(plan, tran);

                MembershipPackage package = new()
                {
                    CapacityID = userCapacity.CapacityID,
                    Fee = 0,
                    IsCustom = true,
                    PackageName = "Default Client Package",
                };
                package.PackageID = await _dbContext.SaveAsync(package, tran);

                client.PackageID = package.PackageID;
                await _dbContext.SaveAsync(client, tran);

                ClientInvoice invoice = new()
                {
                    ClientID = client.ClientID,
                    PackageID = package.PackageID,
                    Fee = 0,
                    InvoiceDate = DateTime.UtcNow,
                    StartDate = DateTime.UtcNow,
                    PaidStatus = (int)PaymentStatus.Paid,
                    GrossFee = 0,
                    Discount = 0,
                };
                await _dbContext.SaveAsync(invoice, tran);
            }
        }

    }
}
