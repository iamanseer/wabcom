using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Enum
{
    public enum Roles
    {
        //crm section
        Enquiry=1,
        Quotation,
        FollowUp,
        Customer,

        //whatsapp section
        BroadCast =100,
        Chat,
        Template,
        ChatbotSetup,
        WhatsappAccount,

        //Common section
        State = 200,
        City,
        Common,

        //Super Admin
        ClientManagement=400,
        MembershipManagment,
        PaymentsManagment,

        //CourtAdmin
        Hall =500,
        HallTiming,
        Court,
        CourtBooking,
        CourtPackage,

        //ClientRoles
        Branch = 600,
        Users,
        ClientSetting,
        ClientInvoice,

        //Spinwheel
        SpinWheel=700,

        //Accounts
        AccountsGroup=800,
        Accounts,
        VoucherType,
        VoucherEntry,

        //Inventory
        Item = 900,
        InvoiceType,
        Invoice,
        ItemStock,
        StockAdjustment,
        Supplier,

        //AISC Membership
        AISCMembership =1000,

        //Report
        BalanceSheet=1100,
        GeneralLedger,
        JournalReport,
        ProfitAndLoss,
        TrialBalance,
        RAndD,
        SalesByItemReport,
        SalesByCustomerReport,
        SalesByStaffReport
    }


    public enum RoleGroups
    {
        CRM=1,
        Whatsapp,
        Common,
        SupportRoles,
        CourtManagment,
        ClientRoles,
        SpinWheel,
        AccountsManagement,
        InventoryManagement,
        Report,
    }
}
