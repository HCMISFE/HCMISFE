﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using BLL;
using DAL;
using DevExpress.XtraEditors;
using PharmInventory.HelperClasses;

namespace PharmInventory.Forms.Modals
{
    public partial class TransferForm : DevExpress.XtraEditors.XtraForm
    {
        private readonly ReceiveDoc receive = new ReceiveDoc();

        public TransferForm()
        {
            InitializeComponent();
        }

        #region Members

        private int tabPage;
        private string selectedType = "Drug";
        private DataTable dtSelectedTable = null;
        private int itemID;
        private DataTable dtRecGrid = new DataTable();

        #endregion

        private void TransferForm_Load(object sender, EventArgs e)
        {
            var store = new Stores();
            store.GetActiveStores();
            lkFromStore.Properties.DataSource = store.DefaultView;
            lkFromStore.ItemIndex = 0;

            // lkToStore.Properties.DataSource = store.DefaultView;
            //lkToStore.Properties.DisplayMember = "Sto SupreName";
            //lkToStore.Properties.ValueMember = "ID";

            lkCategories.Properties.DataSource = BLL.Type.GetAllTypes().DefaultView;
            lkCategories.ItemIndex = 0;
            ImportItems();

            Stores stor = new Stores();
            stor.GetActiveStores();
            cboStores.Properties.DataSource = stor.DefaultView;

            Programs prog = new Programs();
            DataTable dtProg = prog.GetSubPrograms();
            object[] objProg = { 0, "All Programs", "", 0, "" };
            dtProg.Rows.Add(objProg);
            cboProgram.Properties.DataSource = dtProg;
            cboProgram.ItemIndex = -1;
            cboProgram.Text = "Select Program";
            cboProgram.Properties.DisplayMember = "Name";
            cboProgram.Properties.ValueMember = "ID";

            Supplier sup = new Supplier();
            DataTable dtSup = new DataTable();
            sup.GetActiveSuppliers();
            dtSup = sup.DefaultView.ToTable();
            cboSuppliers.DataSource = dtSup;
            cboSupplier.Properties.DataSource = sup.DefaultView;
            cboSuppliers.Text = "Select Supplier";
            cboSuppliers.ValueMember = "ID";
            cboSuppliers.DisplayMember = "CompanyName";

            // Bind the grid with only active items
            Items itm = new Items();
            //DataTable dtItem = itm.GetAllItems(1);
            DataTable dtItem = BLL.Items.GetActiveItemsByCommodityType(0);
            lkCategories.Properties.DataSource = BLL.Type.GetAllTypes();
            lkCategories.ItemIndex = 0;

            //PopulateItemList(dtItem);
            selectedType = radioGroup1.EditValue.ToString();
           // PopulateCatTree(selectedType);

            int userID = MainWindow.LoggedinId;
            User us = new User();
            us.LoadByPrimaryKey(userID);
            //txtReceivedBy.Text = us.FullName;

            // bind the current date as the datetime field
            dtRecDate.Value = DateTime.Now;
        }

        private void ImportItems()
        {
            PopulateItemList(receive.GetReceivedNotIssuedItems((int) lkFromStore.EditValue));
        }

        private void lkCategories_EditValueChanged(object sender, EventArgs e)
        {
            gridItemsView.ActiveFilterString = string.Format("TypeID={0}", (int) lkCategories.EditValue);
        }

        private void PopulateItemList(DataTable dtItem)
        {
           // gridItemsChoice.DataSource = dtItem;
            if (dtSelectedTable == null)
            {
                dtSelectedTable = dtItem.Clone();
                dtSelectedTable.PrimaryKey = new DataColumn[] { dtSelectedTable.Columns["ID"] };
            }
            gridItemsChoice.DataSource = dtItem;
            try
            {
                dtItem.Columns.Add("IsSelected", typeof(bool));
            }
            catch { }

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var transfer = new Transfer();
            Items itm = new Items();
            string valid = ValidateFields();
            if (valid == "true")
            {
                if (XtraMessageBox.Show("Are you sure you want to save this transaction?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    //MyGeneration.dOOdads.TransactionMgr mgr = MyGeneration.dOOdads.TransactionMgr.ThreadTransactionMgr();
                    try
                    {
                      //  mgr.BeginTransaction();
                        ReceiveDoc receiveValidation = new ReceiveDoc();
                        if (!receiveValidation.ValidateReceiveDate(Convert.ToDateTime(dtRecDate.Value)))
                        {
                            XtraMessageBox.Show("The receive date should be checked!", "Error", MessageBoxButtons.OK,
                                                MessageBoxIcon.Error);
                            return;
                        }

                        var dtRecGrid = (DataTable)receivingGrid.DataSource;
                      

                        for (int i = 0; i < dtRecGrid.Rows.Count; i++)
                        {
                            if (dtRecGrid.Rows[i]["Expiry Date"] != DBNull.Value)
                            {
                                if (Convert.ToDateTime(dtRecGrid.Rows[i]["Expiry Date"]) <= DateTime.Now)
                                {
                                    var dialog =
                                        XtraMessageBox.Show(
                                            "The item " + dtRecGrid.Rows[i]["Item Name"].ToString() +
                                            " has already expired.  Are you sure you want to receive it?", "Warning",
                                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                                    if (dialog == DialogResult.No)
                                    {
                                        return;
                                    }

                                }
                            }
                            transfer.AddNew();
                            transfer.FromStoreID = Convert.ToInt32(lkFromStore.EditValue);
                            transfer.ToStoreID = Convert.ToInt32(cboStores.EditValue);
                            transfer.RefNo = txtRefNo.Text.Trim();
                            DateTime xx = dtRecDate.Value;
                            dtRecDate.CustomFormat = "MM/dd/yyyy";
                            DateTime dtRec = new DateTime();
                            transfer.Date = ConvertDate.DateConverter(dtRecDate.Text);
                            dtRec = ConvertDate.DateConverter(dtRecDate.Text);
                            dtRecDate.IsGregorianCurrentCalendar = true;

                            transfer.EurDate = dtRecDate.Value;
                            dtRecDate.IsGregorianCurrentCalendar = false;
                            transfer.ItemID = itemID = Convert.ToInt32(dtRecGrid.Rows[i][0]);
                            transfer.NoOfPack = Convert.ToInt32(dtRecGrid.Rows[i]["Pack Qty"]);
                            transfer.QtyPerPack = Convert.ToInt32(dtRecGrid.Rows[i]["Qty/Pack"]);
                            transfer.Quantity = transfer.NoOfPack * transfer.QtyPerPack;
                            transfer.QuantityLeft = transfer.Quantity;
                            if (dtRecGrid.Rows[i]["Price/Pack"] != null &&
                                dtRecGrid.Rows[i]["Price/Pack"].ToString() != "")
                            {
                                double pre = Convert.ToDouble(dtRecGrid.Rows[i]["Price/Pack"]) / transfer.QtyPerPack;
                                transfer.Cost = Convert.ToDouble(pre);
                            }
                            else
                            {
                                transfer.Cost = 0;
                            }
                            itm.LoadByPrimaryKey(Convert.ToInt32(dtRecGrid.Rows[i]["ID"]));
                            transfer.BatchNo = dtRecGrid.Rows[i][8].ToString();
                            if (dtRecGrid.Rows[i]["Expiry Date"] != DBNull.Value)
                            {
                                transfer.ExpDate = Convert.ToDateTime(dtRecGrid.Rows[i]["Expiry Date"]);
                            }
                            transfer.SupplierID = Convert.ToInt32(cboSupplier.EditValue);
                            transfer.SubProgramID = Convert.ToInt32(cboProgram.EditValue);
                            string batch = DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() +
                                           DateTime.Now.Minute.ToString() + transfer.ItemID.ToString();
                            transfer.LocalBatchNo = batch;
                            transfer.Save();
                            dtRecDate.Value = xx;
                            
                           

                        }
                        
                        XtraMessageBox.Show("Transaction Successfully Saved!", "Success", MessageBoxButtons.OK,
                                            MessageBoxIcon.Information);
                       
                        var receiveDoc = new ReceiveDoc();
                        var trans = new Transfer();
                        trans.GetTransfered(itemID, (int) lkFromStore.EditValue);
                        receiveDoc.GetReceivedItems(itemID, (int)lkFromStore.EditValue);
                        receiveDoc.NoOfPack =receiveDoc.NoOfPack - trans.NoOfPack;
                        receiveDoc.QtyPerPack =receiveDoc.QtyPerPack - trans.QtyPerPack;
                        receiveDoc.Quantity = (receiveDoc.NoOfPack * receiveDoc.QtyPerPack) - (trans.NoOfPack * trans.QtyPerPack);
                        receiveDoc.QuantityLeft = receiveDoc.QuantityLeft -trans.QuantityLeft;
                        receiveDoc.Save();
                        ResetFields();
                        //mgr.CommitTransaction();


                    }
                    catch (Exception exp)
                    {
                        //mgr.RollbackTransaction();
                        BLL.User user = new User();
                        user.LoadByPrimaryKey(MainWindow.LoggedinId);
                        if (user.UserType == UserType.Constants.SYSTEM_ADMIN)
                            XtraMessageBox.Show(exp.Message);
                        else
                            XtraMessageBox.Show("Saving Error!", "Error", MessageBoxButtons.OK);
                    }
                }

            }
            else
            {
                XtraMessageBox.Show(valid, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }
        private string ValidateFields()
        {
            string valid = "true";

            if (Convert.ToDateTime(dtRecDate.Value) > DateTime.Now)
            {
                valid = "You cannot pick a Date in a future!";
                return valid;
            }
            if((int)lkFromStore.EditValue ==(int)cboStores.EditValue)
            {
                valid = "You cannot transfer an item to the same store.";
                return valid;
            }
            if (!dxValidationProvider1.Validate())
            {
                valid = "Please correct the highlighted filed!";
            }

            Items itm = new Items();
            for (int i = 0; i < gridRecieveView.DataRowCount; i++)
            {
                DataRow dr = gridRecieveView.GetDataRow(i);
                dr.ClearErrors();
                if (dr["Qty/Pack"] == null || dr["Qty/Pack"].ToString() == "" || Convert.ToInt64(dr["Qty/Pack"]) == 0)
                {
                    dr.SetColumnError("Qty/Pack", "This field cannot be left blank.");
                    valid = "Please fill the requested feild!";
                }

                if (dr["Pack Qty"] == null || dr["Pack Qty"].ToString() == "" || Convert.ToInt64(dr["Pack Qty"]) == 0)
                {
                    dr.SetColumnError("Pack Qty", "This field cannot be left blank.");
                    valid = "Please fill the requested feild!";
                }

                itm.LoadByPrimaryKey(Convert.ToInt32(dtRecGrid.Rows[i]["ID"]));

                if (!itm.IsColumnNull("NeedExpiryBatch"))
                {
                    if (itm.NeedExpiryBatch)
                    {

                        if (dr["Expiry Date"].ToString() == "")
                        {
                            dr.SetColumnError("Expiry Date", "This field cannot be left blank.");
                            valid = "Please fill the requested feild!";
                        }

                        if (dr["Batch No"].ToString() == "")
                        {
                            dr.SetColumnError("Batch No", "This field cannot be left blank.");
                            valid = "Please fill the requested feild!";
                        }
                    }
                }
            }

            return valid;

        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            ResetFields();
        }
        private void ResetFields()
        {
            tabControl1.SelectedTabPageIndex = 0;
            cboStores.ItemIndex = 0;
            //txtReceivedBy.Text = "";
            //txtRemark.Text = "";
            dtRecDate.Value = DateTime.Now;
            txtItemName.Text = "";
            dtSelectedTable.Rows.Clear();
            foreach (DataRowView dr in ((DataView)gridItemsView.DataSource))
            {
                dr["IsSelected"] = false;
            }
        }
        private void gridItemsView_RowClick(object sender, DevExpress.XtraGrid.Views.Grid.RowClickEventArgs e)
        {
            DataRow dr = gridItemsView.GetFocusedDataRow();
            bool b = (dr["IsSelected"] != DBNull.Value) && Convert.ToBoolean(dr["IsSelected"]);
            dr["IsSelected"] = !b;
            dr.EndEdit();
            OnItemCheckedChanged(new object(), new EventArgs());
        }

        private void OnItemCheckedChanged(object o, EventArgs eventArgs)
        {
            DataRow dr = gridItemsView.GetFocusedDataRow();

            bool b = (dr["IsSelected"] != DBNull.Value) ? Convert.ToBoolean(dr["IsSelected"]) : false;

            if (b)
            {
                try
                {
                    dtSelectedTable.ImportRow(dr);
                }
                catch
                {
                }
            }
            else
            {
                int id = Convert.ToInt32(dr["ID"]);
                DataRow rw = dtSelectedTable.Rows.Find(id);
                if (rw != null)
                {
                    dtSelectedTable.Rows.Remove(rw);
                    try
                    {
                        DataRow[] dataRows = dtRecGrid.Select(String.Format("ID = {0}", dr["ID"]));
                            // dtRecGrid.Rows.Remove(dtRecGrid.Rows.Find(dr["ID"]));
                        foreach (DataRow r in dataRows)
                        {
                            r.Delete();
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }

        private void PopulateListToGrid()
        {
            if (receivingGrid.DataSource != null)
            {
                DataTable dt = new DataTable();
                receivingGrid.DataSource = dt;
                dtRecGrid = new DataTable();
            }
            Items itm = new Items();
            tabPage = 1;
            tabControl1.SelectedTabPageIndex = 1;


            receivingGrid.DataSource = new DataTable();

            // No reason really to populate the grid columns like this.
            if (dtRecGrid.Columns.Count == 0)
            {
                string[] str = { "ID", "Stock Code", "Item Name", "Pack Qty", "Qty/pack", "BU Qty", "Unit", "Price/Pack", "Batch No", "Expiry Date", "Total Price" };
                foreach (string col in str)
                {
                    if (col == "Expiry Date")
                    {
                        dtRecGrid.Columns.Add(col, typeof(DateTime));
                    }
                    else
                    {
                        dtRecGrid.Columns.Add(col);
                    }
                }
            }
            int count = 1;

            // this could get better
            foreach (DataRow lst in dtSelectedTable.Rows)
            {
                string itemName = lst["FullItemName"].ToString();
                object[] obj = { lst["ID"].ToString(), lst["StockCode"].ToString(), itemName, 1, 1, 1, lst["Unit"].ToString(), "", "" };
                dtRecGrid.Rows.Add(obj);
                count++;
            }
            receivingGrid.DataSource = dtRecGrid;
            dtRecGrid.DefaultView.Sort = "Stock Code";
            dtRecDate.CustomFormat = "MMM dd,yyyy";
        }

        private void tabControl1_SelectedPageChanged(object sender, DevExpress.XtraTab.TabPageChangedEventArgs e)
        {
            if (tabPage == 0)
            {
                if (dtSelectedTable.Rows.Count > 0)
                {
                    PopulateListToGrid();
                }
                else
                {
                    if (tabControl1.SelectedTabPageIndex != 0)
                    {
                        tabControl1.SelectedTabPageIndex = 0;
                        XtraMessageBox.Show("You must select a drug to populate.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Stop);

                    }
                }
            }
            else if (tabPage == 1)
            {
                if (tabControl1.SelectedTabPageIndex != 1)
                    tabPage = 0;
            }
        }

        private void txtItemName_TextChanged(object sender, EventArgs e)
        {
            gridItemsView.ActiveFilterString = String.Format("[FullItemName] Like '{0}%' And [TypeID] = {1}", txtItemName.Text, (int)(lkCategories.EditValue ?? 0));
        }

        private void repositoryItemButtonEdit2_Click(object sender, EventArgs e)
        {
            DataRow dr = gridRecieveView.GetDataRow(gridRecieveView.GetSelectedRows()[0]);

            dtRecGrid.ImportRow(dr);
            dtRecGrid.DefaultView.Sort = "Stock Code";
        }

        private void lkFromStore_EditValueChanged(object sender, EventArgs e)
        {
            PopulateItemList(receive.GetReceivedNotIssuedItems((int)lkFromStore.EditValue));
        }
        

    }
}