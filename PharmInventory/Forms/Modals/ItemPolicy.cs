using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using BLL;
using DevExpress.XtraEditors;

namespace PharmInventory.Forms.Modals
{
    public partial class ItemPolicy : XtraForm
    {
        
        public ItemPolicy()
        {
            InitializeComponent();
        }

        readonly int _itemId = 0;
        readonly int _categoryId = 0;
        Boolean _isStorageTypeChanged = false;

        /// <summary>
        /// Initialize the form with an itemId or Category Id
        /// </summary>
        /// <param name="id">Item ID or Category ID</param>
        /// <param name="itemId">True - If the ID is an item ID, False - If the ID is a Category ID</param>
        public ItemPolicy(int id, bool itemId)
        {
            InitializeComponent();
            if (itemId)
                _itemId = id;
            else
                _categoryId = id;
        }

        /// <summary>
        /// Populates the fields based on the supplied itemID or categoryID
        /// </summary>
        private void PopulateFields()
        {
            if (_itemId == 0)
                return;

            Items itm = new Items();

            DataTable dtItem = itm.GetItemById(_itemId);
            txtItemName.Text = dtItem.Rows[0]["ItemName"].ToString() + " - " + dtItem.Rows[0]["DosageForm"].ToString() + " - " + dtItem.Rows[0]["Strength"].ToString();
            ckExculed.Checked = itm.IsInHospitalList;
            txtText.Text = itm.StockCodeDACA ?? string.Empty;
            txtQuantityPerPack.Text = itm.Cost ?? string.Empty;
           
            try
            {
                if (dtItem.Rows[0]["ABC"].ToString() == "A")
                    rdA.Checked = true;
                else if (dtItem.Rows[0]["ABC"].ToString() == "B")
                    rdB.Checked = true;
                else if (dtItem.Rows[0]["ABC"].ToString() == "C")
                    rdC.Checked = true;
            }
            catch { }

            try
            {
                if (dtItem.Rows[0]["VEN"].ToString() == "V")
                    rdV.Checked = true;
                else if (dtItem.Rows[0]["VEN"].ToString() == "E")
                    rdE.Checked = true;
                else if (dtItem.Rows[0]["VEN"].ToString() == "N")
                    rdN.Checked = true;
            }
            catch { }

            //ItemShelf itmShelf = new ItemShelf();
            //DataTable dtSlf = itmShelf.GetLocationByItem(itm.ID);
            //lstBinLocation.DataSource = dtSlf;


            Supplier sup = new Supplier();
            sup.GetActiveSuppliers();
            ItemSupplier itmSup = new ItemSupplier();
            itm.GetItemsBySupplier(_itemId);
            foreach (DataRowView dv in sup.DefaultView)
            {
                bool check = false;
                check = itmSup.CheckIfExist(_itemId, Convert.ToInt32(dv["ID"]));
                object obj = dv["CompanyName"];
                lstSuppliers.Items.Add(obj, check);
            }

            Programs prog = new Programs();
            prog.GetSubPrograms();
            ProgramProduct progItem = new ProgramProduct();
            lstPrograms.Items.Clear();
            foreach (DataRowView dv in prog.DefaultView)
            {
                bool check = false;
                check = progItem.CheckIfExists(_itemId, Convert.ToInt32(dv["ID"]));
                lstPrograms.Items.Add(dv["Name"], check);
            }

            ReceivingUnits dus = new ReceivingUnits();
            dus.GetActiveDispensaries();
            lstDUs.Items.Clear();

            DUsItemList duItem = new DUsItemList();

            foreach (DataRowView drDus in dus.DefaultView)
            {
                bool check = false;
                check = duItem.CheckIfExsits(_itemId, Convert.ToInt32(drDus["ID"]));
                lstDUs.Items.Add(drDus["Name"], check);
            }
        }

        /// <summary>
        /// Prepares the Item Policy Form
        /// Loads combo boxes and populates fields
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemPolicy_Load(object sender, EventArgs e)
        {
            Shelf slf = new Shelf();
            DataTable dtSlf = slf.GetShelves();
            
            Programs prog = new Programs();
            prog.GetParentPrograms();
            cboPrograms.DataSource = prog.DefaultView;
            cboPrograms.SelectedIndex = -1;

            PopulateFields();
        }

        private string ValidateFields()
        {
            string valid = "true";
             if (txtQuantityPerPack.Text != "" && txtText.Text != "")
                    valid = "true";
             else if (txtQuantityPerPack.Text !="" && txtText.Text == "")
             {
                 valid = "Prefered pack text is required";
                 return valid;
             }
            return valid;
        }
        /// <summary>
        /// Closes the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        

        /// <summary>
        /// Saves the item policy
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, EventArgs e)
        {
            Items itm = new Items();
            ItemSupplier itmSup = new ItemSupplier();
            if (_itemId != 0)
                itm.LoadByPrimaryKey(_itemId);
            else
            {
                itm.AddNew();
                ProductsCategory prodCate = new ProductsCategory();
                prodCate.AddNew();
                prodCate.ItemId = itm.ID;
                prodCate.SubCategoryID = Convert.ToInt32(_categoryId);
                prodCate.Save();
            }
            if (rdA.Checked || rdB.Checked || rdC.Checked)
                itm.ABC = ((rdA.Checked) ? 1 : (rdB.Checked) ? 2 : 3);

            if (rdV.Checked || rdE.Checked || rdN.Checked)
                itm.VEN = ((rdV.Checked) ? 1 : (rdE.Checked) ? 2 : 3);
          
                itm.IsInHospitalList = ckExculed.Checked;
            string valid = ValidateFields();
            if (valid == "true")
            {
                itm.StockCodeDACA = txtText.Text;
                itm.Cost = txtQuantityPerPack.Text;
                itm.Save();
            }
            else
            {
                XtraMessageBox.Show(valid, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }


            //TODO: To add categories
            //Needs some modification on edit


            // this will only add the suppliers 
            //do some thing on edit

            itmSup.DeleteAllSupForItem(itm.ID);
            Supplier sup = new Supplier();
            foreach (object t in lstSuppliers.CheckedItems)
            {
                sup.GetSupplierByName(t.ToString());
                itmSup.AddNew();
                itmSup.ItemID = itm.ID;
                itmSup.SupplierID = sup.ID;
                itmSup.Save();
            }

            ProgramProduct progItm = new ProgramProduct();
            progItm.DeleteAllProgramsForItem(_itemId);
            Programs prog = new Programs();

            foreach (object t in lstPrograms.CheckedItems)
            {
                prog.GetProgramByName(t.ToString());
                progItm.AddNew();
                progItm.ItemID = itm.ID;
                progItm.ProgramID = prog.ID;
                progItm.Save();
            }

            DUsItemList duItem = new DUsItemList();
            ReceivingUnits dus = new ReceivingUnits();

            foreach (object t in lstDUs.CheckedItems)
            {
                dus.GetDUByName(t.ToString());
                duItem.AddNew();
                duItem.DUID = dus.ID;
                duItem.ItemID = _itemId;
                try
                {
                    duItem.Save();
                }
                catch
                {
                    
                }
            }

            XtraMessageBox.Show("Item Detail is Saved Successfully!", "Confirmation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
        
        /// <summary>
        /// When the cboPrograms changes, the program lists needs to be repopulated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboPrograms_SelectedIndexChanged(object sender, EventArgs e)
        {
            lstPrograms.Items.Clear();
            Programs prog = new Programs();
            ProgramProduct progItem = new ProgramProduct();
            if (cboPrograms.SelectedValue != null)
            {
                prog.GetSubProgramsByParentId(Convert.ToInt32(cboPrograms.SelectedValue));
            }
            else
            {
                prog.GetSubPrograms();
            }
            foreach (DataRowView dv in prog.DefaultView)
            {
                bool check = false;
                check = progItem.CheckIfExists(_itemId, Convert.ToInt32(dv["ID"]));
                lstPrograms.Items.Add(dv["Name"], check);
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

       
    }
}