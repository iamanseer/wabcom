using AutoMapper;
using PB.Model;
using PB.Shared.Models;
using PB.Shared.Tables;
using PB.Model.Tables;
using PB.Shared.Models.Court;
using PB.Shared.Tables.CourtClient;
using PB.Shared.Models.Accounts.AccountGroups;
using PB.Shared.Tables.Accounts.AccountGroups;
using PB.Shared.Models.Accounts.Ledgers;
using PB.Shared.Tables.Accounts.Ledgers;
using PB.Shared.Models.Accounts;
using PB.Shared.Tables.CRM;
using PB.Shared.Models.CRM;
using PB.Shared.Models.CRM.Enquiry;
using PB.Model.Models;
using PB.Shared.Models.Accounts.VoucherTypes;
using PB.Shared.Tables.Accounts.VoucherTypes;
using PB.Shared.Models.Accounts.VoucherEntry;
using PB.Shared.Tables.Accounts.JournalMaster;
using PB.Shared.Tables.Whatsapp;
using PB.Shared.Models.WhatsaApp;
using PB.Shared.Models.Inventory.Invoice;
using PB.Shared.Tables.Inventory.Invoices;
using PB.Shared.Models.Inventory.Invoices;
using PB.Shared.Models.Spin;
using PB.Shared.Tables.Inventory.Items;
using PB.Shared.Models.Inventory.Item;
using PB.Shared.Models.Inventory.Items;
using PB.Shared.Tables.Court;
using PB.Shared.Models.Common;
using PB.Shared.Models.Inventory.Supplier;
using PB.Shared.Tables.Inventory.Suppliers;
using PB.Shared.Models.SuperAdmin.Client;
using PB.Shared.Tables.Tax;
using PB.Shared.Tables.Common;
using PB.Shared.Models.eCommerce.Customers;
using PB.Shared.Tables.eCommerce.Entity;
using PB.Shared.Tables.eCommerce.Customer;
using PB.Shared.Models.eCommerce.WishList;
using PB.Shared.Models.eCommerce.Cart;
using PB.Shared.Tables.eCommerce.SEO;
using PB.Shared.Models.eCommerce.SEO;
using PB.Shared.Models.eCommerce.Product;
using PB.Shared.Tables.eCommerce.Products;
using PB.Shared.Models.CRM.Quotations;
using PB.Shared.Models.CRM.Customer;

namespace PB.Shared.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            //Whatsapp
            CreateMap<WhatsappTemplate, WhatsappTemplatePageModel>();
            CreateMap<WhatsappTemplateVariable, TemplateVariablePageModel>();
            CreateMap<WhatsappTemplateVariable, WhatsappTemplateVariableModel>();
            CreateMap<WhatsappTemplateButton, TemplateButtonPageModel>();
            CreateMap<WhatsappTemplatePageModel, WhatsappTemplateVariable>();
            CreateMap<BroadcastSearchModel, PagedListPostModelWithFilter>();
            CreateMap<RecreatBroadcastModel, PagedListPostModelWithFilter>();
            CreateMap<MessageReceipientSearchModel, PagedListPostModelWithFilter>();

            //User
            CreateMap<UserSingleModel, Entity>();
            CreateMap<UserSingleModel, EntityPersonalInfo>();
            CreateMap<UserSingleModel, EntityAddress>();
            CreateMap<UserSingleModel, User>();
            CreateMap<BranchUserModel, BranchUser>();
            CreateMap<UserTypeAccessModel, UserTypeRoleCustom>();
            CreateMap<UserSingleModel, UserCustom>();

            //Branch
            CreateMap<BranchModel, Entity>();
            CreateMap<BranchModel, EntityInstituteInfo>();
            CreateMap<BranchModel, EntityAddress>();
            CreateMap<BranchModel, Branch>();

            //Common
            CreateMap<CountryStateModel, CountryState>();

            //Qutation
            CreateMap<AddressModel, EntityAddressCustom>();
            CreateMap<QuotationModel, Quotation>();
            CreateMap<QuotationItemModel, QuotationItem>();
            CreateMap<QuotationAssigneeModel, QuotationAssignee>();
            CreateMap<QuotationMailRecipientModel, QuotationMailRecipient>();
            CreateMap<QuotationModelNew, Quotation>();
            CreateMap<QuotationItemModelNew, QuotationItem>();
            CreateMap<ItemVariantDetail, QuotationItemModelNew>();

            //Enquiry
            CreateMap<EnquiryAssigneeModel, EnquiryAssignee>();
            CreateMap<EnquiryModel, Enquiry>();
            CreateMap<EnquiryItemModel, EnquiryItem>();
            CreateMap<AddToCartModel, Enquiry>();
            CreateMap<PB.Shared.Models.Inventory.Items.AddToCartItemModel, EnquiryItem>(); 

            //Item
            CreateMap<ItemSingleModel, Item>();
            CreateMap<ItemSingleModel, ItemVariant>();
            CreateMap<ItemPackingTypeModel, ItemPackingType>();
            CreateMap<ItemVariantModel, ItemSize>();
            CreateMap<ItemVariantModel, ItemVariant>();
            CreateMap<ItemSizeModel, ItemSize>();
            CreateMap<ItemVariantImageModel, ItemVariantImage>();
            CreateMap<ItemImageModel, ItemImage>();
            CreateMap<ItemColor, ItemColorModel>();
            CreateMap<ItemColorModel, ItemColor>();
            CreateMap<ItemBrand, ItemBrandModel>();
            CreateMap<ItemBrandModel, ItemBrand>();
            CreateMap<ItemGroup, ItemGroupModel>();
            CreateMap<ItemGroupModel, ItemGroup>();
            CreateMap<ItemCategory, ItemCategoryModel>();
            CreateMap<ItemCategoryModel, ItemCategory>();
            CreateMap<ItemImageModel, ItemImage>();

            //MembershipFeature
            CreateMap<MembershipFeatureModel, MembershipFeature>();
            CreateMap<MembershipUserCapacityModel, MembershipUserCapacity>();
            CreateMap<MembershipPlanModel, MembershipPlan>();
            CreateMap<MembershipFeeModel, MembershipFee>();
            CreateMap<MembershipPackageModel, MembershipPackage>();
            CreateMap<ClientRegisterModel, MembershipPackage>();

            //Settings
            CreateMap<LeadThroughModel, LeadThrough>();
            CreateMap<ClientSettingModel, ClientSetting>();
            CreateMap<TaxCategoryModel, TaxCategory>();
            CreateMap<ClientSetting, ClientSettingModel>();
            CreateMap<PaymentTermModel, PaymentTerm>();
            CreateMap<PaymentTermSlabModel, PaymentTermSlab>();

            CreateMap<PromotionModel, Promotion>();
            CreateMap<PromotionItemListViewModel, PromotionItem>();
            CreateMap<BusinessTypeModel, BusinessType>();

            //CourtClient
            CreateMap<HallModel, Hall>();
            CreateMap<HallSectionModel, HallSection>();
            CreateMap<HallTimingModel, HallTiming>();
            CreateMap<CourtModel, Court>();
            CreateMap<CourtPriceGroupModel, CourtPriceGroup>();
            CreateMap<CourtPriceGroupItemModel, CourtPriceGroupItem>();

            //Spin
            CreateMap<ContestModel, Contest>();
            CreateMap<ContestGiftModel, ContestGift>();
            CreateMap<WhatsappContactModel, WhatsappContact>();

            //Accounts
            CreateMap<AccountGroupModel, AccAccountGroup>();

            CreateMap<AccLedgerModel, EntityCustom>();
            CreateMap<AccLedgerModel, EntityAddressCustom>();
            CreateMap<AccLedgerModel, EntityPersonalInfo>();
            CreateMap<AccLedgerModel, AccLedger>();
            CreateMap<BillToBillModel, AccBillToBill>();

            CreateMap<VoucherTypeModel, AccVoucherType>();
            CreateMap<VoucherTypeModel, AccVoucherTypeSetting>();

            CreateMap<VoucherEntryModel, AccJournalMaster>();
            CreateMap<AccJournalEntryModel, AccJournalEntry>();
            CreateMap<VoucherEntryMenuModel, ViewPageMenuModel>();


            //Inventory
            CreateMap<InvoiceTypeModel, VoucherTypeModel>();
            CreateMap<QuotationItemModel, InvoiceItem>();
            CreateMap<ItemVariantDetail, InvoiceItemModel>();

            //Invoice
            CreateMap<InvoiceModel, Invoice>();
            CreateMap<InvoiceItemModel, InvoiceItem>();
            CreateMap<QuotationModelNew, InvoiceModel>(); 
            CreateMap<QuotationAssignee,InvoiceAssignee>();
            CreateMap<InvoiceMailReceipient, QuotationMailRecipient>();

            CreateMap<QuotationItemModelNew, InvoiceItemModel>();
            CreateMap<InvoiceTypeChargeModel,InvoiceChargeModel>();
            CreateMap<QuotationMailRecipientModel, InvoiceMailReceipient>();
            CreateMap<InvoiceTypeChargeModel, InvoiceChargeModel>();

            CreateMap<InvoiceModel, InvoiceViewModel>();
            CreateMap<InvoiceItemModel, InvoiceViewItemModel>();

            //Supplier
            CreateMap<SupplierModel, EntityCustom>();
            CreateMap<SupplierModel, EntityInstituteInfo>();
            CreateMap<SupplierModel, Supplier>();
            CreateMap<SupplierContactPersonModel, EntityCustom>();
            CreateMap<SupplierContactPersonModel, SupplierContactPerson>();

            // Customer
            CreateMap<CustomerCategoryModel, CustomerCategory>();
            CreateMap<CustomerSubscriptionModel, CustomerSubscription>();



            //E-Commerce

            CreateMap<EC_CustomerAddressModel, EC_EntityAddress>();
            CreateMap<EC_WishListModel, EC_WishList>();
            CreateMap<EC_CartModel, EC_Cart>();
            CreateMap<EC_ItemCategoryModel, EC_ItemCategory>();


            //Blog
            CreateMap<BlogTgModel, EC_BlogTag>();




        }
    }
}
