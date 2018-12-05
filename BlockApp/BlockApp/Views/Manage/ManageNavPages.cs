using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace BlockApp.Views.Manage
{
    public static class ManageNavPages
    {
        public static string ActivePageKey => "ActivePage";
        public static string Index => "Index";
        public static string ChangePassword => "ChangePassword";
        public static string Merchant => "Merchant";
        //public static string Transaction => "Transaction";
        public static string Subscribe => "Subscribe";

        public static string IndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, Index);
        public static string ChangePasswordNavClass(ViewContext viewContext) => PageNavClass(viewContext, ChangePassword);
        public static string MerchantNavClass(ViewContext viewContext) => PageNavClass(viewContext, Merchant);
        //public static string TransactionNavClass(ViewContext viewContext) => PageNavClass(viewContext, Transaction);
        public static string SubscribeNavClass(ViewContext viewContext) => PageNavClass(viewContext, Subscribe);
    
        public static string PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["ActivePage"] as string;
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }

        public static void AddActivePage(this ViewDataDictionary viewData, string activePage) => viewData[ActivePageKey] = activePage;
    }
}
