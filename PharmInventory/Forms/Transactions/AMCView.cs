﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using PharmInventory.HelperClasses;
using PharmInventory.ViewModels;
using StockoutIndexBuilder.DAL;
using StockoutIndexBuilder.Models;

namespace PharmInventory.Forms.Transactions
{
    public partial class AMCView : DevExpress.XtraEditors.XtraForm
    {
        private readonly StockoutRepository _repository = new StockoutRepository();
        private readonly StoreRepository _storerepository = new StoreRepository();
        private readonly GeneralInfoRepository _generalInfoRepository = new GeneralInfoRepository();
        private readonly ReceiveDocRepository _receiveDocRepository =new ReceiveDocRepository();
        private readonly AmcReportRepository _amcReportRepository =new AmcReportRepository();
        private readonly UnitRepository _unitRepository = new UnitRepository();
        private List<AMCViewModel> _datasource; 
        public AMCView()
        {
            InitializeComponent();
            this.TopLevel = false;
            //loadamc();
        }

        private void loadamc()
        {
          
            var allstores = _storerepository.AllStores();
             storebindingSource.DataSource = allstores;
             lookUpEdit1.ItemIndex = 0;

             var allamcs = _amcReportRepository.AllAmcReport();
              amcbindingSource.DataSource = allamcs.Distinct().Where(m => m.StoreID == Convert.ToInt32(lookUpEdit1.EditValue)).OrderBy(m=>m.FullItemName);

              var allunits = _unitRepository.GetAll();
              unitsBindingSource.DataSource = allunits;
        }

        private void lookUpEdit1_EditValueChanged(object sender, EventArgs e)
        {
            //backgroundWorker1.RunWorkerAsync();
            var allamcs = _amcReportRepository.AllAmcReport();
            var allstores = _storerepository.AllStores();
            storebindingSource.DataSource = allstores;
            amcbindingSource.DataSource = allamcs.Where(m=>m.StoreID==Convert.ToInt32(lookUpEdit1.EditValue));
           // progressBar1.Visible = true;
            
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {            
            backgroundWorker1.ReportProgress(5);
            var generalinfo = _generalInfoRepository.AllGeneralInfos().First();
            backgroundWorker1.ReportProgress(10);

            var storeList = _storerepository.AllStores();
            foreach (var store in storeList)
            {
                var storeId = store.ID;
                var itemsrecieved = _receiveDocRepository.RecievedItems().Where(m => m.StoreID == storeId).Select(m => m.ItemID).Distinct();
                backgroundWorker1.ReportProgress(20);

                _datasource = new List<AMCViewModel>();
                var percentage = 20.0;
                var receiveDocs = _repository.AllItems().Where(m => itemsrecieved.Contains(m.ID)).ToList();
                double increment = 80.0 / Convert.ToDouble(receiveDocs.Count());

                IEnumerable<int?> unitid;
                //clear AMC table before another build
                _amcReportRepository.DeleteAll();
                if (VisibilitySetting.HandleUnits != 1)
                {
                    foreach (var item in receiveDocs)
                    {
                        unitid = _receiveDocRepository.RecievedItems().Where(m => m.ItemID == item.ID && m.StoreID == storeId).Select(m => m.UnitID).Distinct();
                        foreach (var i in unitid)
                        {
                            _datasource.Add(AMCViewModel.Create(item.ID, storeId, i.GetValueOrDefault(), generalinfo.AMCRange, DateTime.Today));
                        }
                        percentage += increment;
                        backgroundWorker1.ReportProgress((int)percentage);
                    }
                }
                else
                {
                    foreach (var item in receiveDocs)
                    {
                       _datasource.Add(AMCViewModel.Create(item.ID, storeId,generalinfo.AMCRange, DateTime.Today));
                        percentage += increment;
                        backgroundWorker1.ReportProgress((int)percentage);
                    }
                }

            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
           progressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar1.Value = 0;
            
            XtraMessageBox.Show("AMC report done successfully", "HCMIS FE");
            if(lookUpEdit1.EditValue == null)
                loadamc();
            else
            {
                lookUpEdit1_EditValueChanged(null,null);
            }
            progressBar1.Hide();
        }

        private void gridView1_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName == "")
                e.DisplayText = (e.RowHandle + 1).ToString();
        }

        private void btnBuild_Click(object sender, EventArgs e)
        {
            var result = XtraMessageBox.Show("This will build AMC for all stores and might take some time. Do you want to continue?", "HCMIS FE", MessageBoxButtons.YesNo);
            if(result == System.Windows.Forms.DialogResult.Yes)
                backgroundWorker1.RunWorkerAsync();
        }

        private void AMCView_Load(object sender, EventArgs e)
        {
            loadamc();
            var unitcolumn = ((GridView)gridControl1.MainView).Columns[9];

            if(VisibilitySetting.HandleUnits==1)
            {
                unitcolumn.Visible = false;

            }
            if (VisibilitySetting.HandleUnits == 2)
            {
                unitcolumn.Visible = true;

            }
            if (VisibilitySetting.HandleUnits == 3)
            {
                unitcolumn.Visible = true;

            }
        }

        private void btnExportToEx_Click(object sender, EventArgs e)
        {
            string fileName = MainWindow.GetNewFileName("xls");
            gridControl1.ExportToXls(fileName);
            MainWindow.OpenInExcel(fileName);
        }
      
    }
}