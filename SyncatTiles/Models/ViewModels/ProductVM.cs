using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SyncatTiles.Models.ViewModels
{
    public class ProductVM
    {
        public Product Product { get; set; }

        public IEnumerable<SelectListItem> CategorySelectgList { get; set; }
        public IEnumerable<SelectListItem> ApplicationTypeList { get; set; }
    }
    
}
