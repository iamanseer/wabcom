namespace PB.Shared.Enum.WhatsApp
{
    public enum WhatsappTemplateStatus
    {
        NONE=0,
        APPROVED,
        REJECTED,
        PENDING,
        PAUSED,
        PENDING_DELETION
    }

    public enum WhatsappTemplateButtonType
    {
        URL = 1,
        PHONE_NUMBER,
        QUICK_REPLY
    }

    public enum WhatsappTemplateVariableSection
    {
        HEADER = 1,
        BODY,
        BUTTON
    }

    public enum WhatsappTemplateCategory
    {
        MARKETING = 1,
        UTILITY,
        AUTHENTICATION
    }

    public enum WhatsappTemplateHeaderType
    {
        NONE = 1,
        TEXT,
        MEDIA,
    }

    public enum WhatsappTemplateHeaderMediaType
    {
        IMAGE = 1,
        VIDEO,
        DOCUMENT,
        LOCATION
    }

    public enum WhatsappFooterButtonType
    {
        None = 1,
        CallToAction,
        QuickReply
    }

    public enum WhatsappTemplateButtonUrlType
    {
        STATIC = 1,
        DYNAMIC
    }

    public enum WhatsappTemplateVariableDataType
    {
        TEXT = 1,
        CURRENCY,
        DATETIME,
        IMAGE,
        DOCUMENT,
        VIDEO,
        LOCATION,
        URL
    }
}
