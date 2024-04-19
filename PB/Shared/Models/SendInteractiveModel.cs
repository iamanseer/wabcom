using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace PB.Shared.Models
{
    public class SendInteractiveListMessageModel : SendModel
    {
        public InteractiveListMessageModel interactive { get; set; } = new();
    }

    public class SendInteractiveButtonMessageModel : SendModel
    {
        public InteractiveButtonMessageModel interactive { get; set; } = new();
    }

    public class InteractiveMessageModel
    {
        public string? type { get; set; }
        public InteractiveHeaderModel? header { get; set; }
        public InteractiveBodyModel? body { get; set; }
        public InteractiveBodyModel? footer { get; set; }
    }

    public  class InteractiveListMessageModel: InteractiveMessageModel
    {
        public InteractiveListActionModel? action { get; set; } = new();
    }

    public class InteractiveButtonMessageModel : InteractiveMessageModel
    {
        public InteractiveButtonActionModel? action { get; set; } = new();
    }

    public class InteractiveHeaderModel
    {
        public string? type { get; set; }
        public string? text { get; set; } = "";
        public SendInteractiveMediaModel? image { get; set; }
        //public SendMediaAudioModel? audio { get; set; }
        public SendInteractiveMediaModel? video { get; set; }
        public SendInteractiveDocumentModel? document { get; set; }
    }
    public class InteractiveBodyModel
    {
        public string? text { get; set; }
    }

    public class InteractiveListActionModel
    {
        public string? button { get; set; } = "View";
        public List<InteractiveActionSectionModel>? sections { get; set; } = new();
    }

    public class InteractiveActionSectionModel
    {
        public string? title { get; set; }
        public List<InteractiveActionSectionRowModel>? rows { get; set; } = new();
    }

    public class InteractiveActionSectionRowModel
    {
        public string? id { get; set; }
        public string? title { get; set; }
        public string? description { get; set; }
    }


    public class InteractiveButtonActionModel
    {
        public List<InteractiveButtonModel>? buttons { get; set; } = new();
    }

    public class InteractiveButtonModel
    {
        public string? type { get; set; } = "reply";
        public InteractiveButtonReplyModel? reply { get; set; } = new();
    }

    public class InteractiveButtonReplyModel
    {
        public string? id { get; set; }
        public string? title { get; set; }
    }
}
