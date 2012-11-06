﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace StockoutIndexBuilder.Models
{
    [Table("ReceiveDoc")]
    public class ReceiveDoc
    {
        [Key]
        [Column("ID")]
        public int ID { get; set; }
        [ForeignKey("Item")]
        public int ItemID { get; set; }
        [Column("EurDate")]
        public DateTime? Date { get; set; }
        public long Quantity { get; set; }
        public int StoreID { get; set; }

        public virtual Item Item{ get; set; }
        

    }
}
