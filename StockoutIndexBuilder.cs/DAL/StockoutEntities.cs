﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using StockoutIndexBuilder.Models;

namespace StockoutIndexBuilder.DAL
{
    internal class StockoutEntities : DbContext
    {
        public StockoutEntities() : base(Settings.ConnectionString)
        {
            Database.SetInitializer<StockoutEntities>(new CreateDatabaseIfNotExists<StockoutEntities>());
        }

        public DbSet<Stockout> Stockouts { get; set; }
        public DbSet<ReceiveDoc> ReceiveDocs { get; set; }
        public DbSet<IssueDoc> IssueDocs { get; set; }
        public DbSet<Disposal> Disposals { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<GeneralInfo> GenralInfos { get; set; }
        public DbSet<vwGetAllItems>VwGetAllItemses { get; set; }
        public DbSet<AmcReport> AmcReports { get; set; }
    }
}
