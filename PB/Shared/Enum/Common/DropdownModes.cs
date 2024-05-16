using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.CRM.Model.Enum
{
    public enum DropdownModes
    {
        None = 0,
        Zone,
        Country,
        State,
        City,
        StateForSignup,
        CityForSignup,
        Customer,
        Supplier,
        Feature,
        Plan,
        Capacity,
        LeadThrough,
        TaxPreference,
        TaxCategory,
        TaxCategoryLedger,
        Clients,
        IntraTaxCategory,
        InterTaxCategory,
        PaymentTerms,
        BusinessType,
        QuotationCreatedStaff,

        //CourtManagement
        GameMaster,
        HourMaster,   
        Package,
        CourtCustomer,
        CourtPackage,
        PriceGroup,
        
        //CRM
        PlaceOfSupply,
        Currency,
        FollowupStatus,

        //Accounts
        AccountGroups,
        VoucherType,
        Ledger,

        //Inventory
        InvoiceType,

        //AISCMembership
        AISCMembers,
        AISCTeam,

        //Common
        Branch,

        //Whatsapp
        WhatsappAccount,
        Language,

        //Reports
        GeneralLedgerReport,

        //Item
        Item,
        GoodsItem,  
        ItemPackingType,
        ItemColor,
        ItemGroup,
        ItemCategory,
        ItemSize,
        ItemBrand,

        // Customer
        CustomerCategory,
        CustomerSubscription,

        //Mode Groups
        CustomData = 101,
        CommonSearch,
        CommonSearchWithID,
        CommonSearchForSignup,
        AccountsSearch, 



    }
}
