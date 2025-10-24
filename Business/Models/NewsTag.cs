using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class NewsTag
    {
        public short NewsArticleID { get; set; }
        public short TagID { get; set; }

        // Navigation
        [ForeignKey("NewsArticleID")]
        public NewsArticle NewsArticle { get; set; }

        [ForeignKey("TagID")]
        public Tag Tag { get; set; }
    }
}
