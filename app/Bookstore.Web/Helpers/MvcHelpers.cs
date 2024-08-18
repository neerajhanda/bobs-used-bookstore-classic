using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Bookstore.Web.Helpers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;

    public static class MvcHelpers
    {
        public static IEnumerable<SelectListItem> GetSelectListForEnum<T>(this HtmlHelper html, string emptyItem = null)
            where T : Enum
        {
            if (!string.IsNullOrEmpty(emptyItem))
            {
                yield return new SelectListItem()
                {
                    Text = emptyItem
                };
            }

            foreach (var val in Enum.GetValues(typeof(T)))
            {
                yield return new SelectListItem()
                {
                    Text = Enum.GetName(typeof(T), val)
                };
            }
        }
    }
}