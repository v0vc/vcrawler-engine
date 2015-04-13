using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces.POCO;

namespace DataBaseAPI.POCO
{
    public class TagPOCO :ITagPOCO
    {
        public string Title { get; set; }

        public TagPOCO(string title)
        {
            Title = title;
        }
    }
}
