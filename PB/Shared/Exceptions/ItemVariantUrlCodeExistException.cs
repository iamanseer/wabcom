
using System;

namespace PB.Model;

[Serializable]
public class ItemVariantUrlCodeExistException : Exception 
{
    public BaseErrorResponse Response { get; set; }

    public ItemVariantUrlCodeExistException(string message = "Error", string title = "", string description = "", string urlCode = "") 
    {
        Response = new BaseErrorResponse
        {
            ResponseMessage = message + ' ' + "UrlCode : " + urlCode,
            ResponseTitle = title,
            ResponseErrorDescription = description
        };
    }
}